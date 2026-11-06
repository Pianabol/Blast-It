using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardOrchestrator : MonoBehaviour
{
    public static BoardOrchestrator Instance { get; private set; }

    [Header("Swap Settings")]
    [SerializeField] private float swapDuration = 0.3f;
    [SerializeField] private float selectionLiftHeight = 1.0f;

    private Cell firstSelectedCell;
    
    // GameManager'ın durumu okuyabilmesi için public property yaptık
    public bool IsAnimating { get; private set; } 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnCellClicked(int x, int y)
    {
        // YENİ EKLENEN KİLİT: Eğer oyun bitmişse veya başlamadıysa tıklamaları tamamen engelle
        if (GameManager.Instance != null && !GameManager.Instance.CanPlay()) return;

        if (IsAnimating) return;

        Cell clickedCell = BoardManager.Instance.GetCell(x, y);
        if (clickedCell == null || !clickedCell.HasItem) return;

        if (firstSelectedCell == null)
        {
            firstSelectedCell = clickedCell;
            SelectVisuals(firstSelectedCell);
        }
        else
        {
            if (firstSelectedCell == clickedCell)
            {
                DeselectVisuals(firstSelectedCell);
                firstSelectedCell = null;
                return;
            }

            int dx = Mathf.Abs(firstSelectedCell.X - x);
            int dy = Mathf.Abs(firstSelectedCell.Y - y);

            if ((dx == 1 && dy == 0) || (dx == 0 && dy == 1))
            {
                StartCoroutine(SwapRoutine(firstSelectedCell, clickedCell));
            }
            else
            {
                DeselectVisuals(firstSelectedCell);
                firstSelectedCell = clickedCell;
                SelectVisuals(firstSelectedCell);
            }
        }
    }

    private void SelectVisuals(Cell cell)
    {
        GameObject itemObj = cell.CurrentItem.gameObject;
        LeanTween.cancel(itemObj);

        Vector3 targetPos = BoardManager.Instance.GridToWorldPosition(cell.X, cell.Y);
        targetPos.y = selectionLiftHeight;

        LeanTween.move(itemObj, targetPos, 0.15f).setEaseOutQuad();
        LeanTween.scale(itemObj, Vector3.one * 1.15f, 0.15f).setEaseOutQuad();
    }

    private void DeselectVisuals(Cell cell)
    {
        GameObject itemObj = cell.CurrentItem.gameObject;
        LeanTween.cancel(itemObj);

        Vector3 targetPos = BoardManager.Instance.GridToWorldPosition(cell.X, cell.Y);
        targetPos.y = 0.5f;

        LeanTween.move(itemObj, targetPos, 0.15f).setEaseInQuad();
        LeanTween.scale(itemObj, Vector3.one, 0.15f).setEaseInQuad();
    }

    /// <summary>
    /// Patlatma, Yerçekimi ve Zincirleme Reaksiyon (Cascade) döngüsünü yönetir.
    /// </summary>
    private IEnumerator ProcessBoardRoutine(List<Cell> matchesToResolve)
    {
        List<Cell> currentMatches = matchesToResolve;

        // Ekranda eşleşecek taş kalmayana kadar bu döngü dönecek (Zincirleme Kombo)
        while (currentMatches.Count > 0)
        {
            // 1. Taşları yok et
            yield return StartCoroutine(DestroyMatchesRoutine(currentMatches));

            // 2. Tahtadaki taşları düşür ve yeni taş üret
            yield return StartCoroutine(BoardGravity.Instance.ApplyGravityRoutine());

            // 3. Yerçekimi bittikten sonra tahtada yeni eşleşme oldu mu diye tara (Cascade)
            currentMatches = MatchDetector.Instance.FindAllMatches();
            
            if (currentMatches.Count > 0)
            {
                Debug.Log($"<color=magenta>[Cascade]</color> Zincirleme reaksiyon! {currentMatches.Count} taş daha patlıyor.");
            }
        }

        // ====================================================================
        // YENİ: DEADLOCK VE SHUFFLE KONTROLÜ
        // ====================================================================
        // Şelale duruldu. Peki oyuncunun yapabileceği bir hamle (Sahte Takas) var mı?
        while (!MatchDetector.Instance.HasAvailableMove())
        {
            // Hamle yoksa tahtayı karıştır
            yield return StartCoroutine(BoardManager.Instance.ShuffleBoardRoutine());
            
            // Karıştırdıktan sonra tesadüfen 3'lü eşleşme olmuş olabilir! Kontrol et.
            currentMatches = MatchDetector.Instance.FindAllMatches();
            
            if (currentMatches.Count > 0)
            {
                Debug.Log("<color=magenta>[Shuffle Cascade]</color> Karıştırma sonrası yeni eşleşmeler oluştu, patlatılıyor!");
                // Eğer eşleşme varsa tekrar aynı cascade döngüsüne gir
                while (currentMatches.Count > 0)
                {
                    yield return StartCoroutine(DestroyMatchesRoutine(currentMatches));
                    yield return StartCoroutine(BoardGravity.Instance.ApplyGravityRoutine());
                    currentMatches = MatchDetector.Instance.FindAllMatches();
                }
            }
            // Bu büyük While döngüsü, hamle kalana kadar veya oyun kazanılana kadar "Karıştır -> Varsa Patlat -> Hamle Kontrol Et" şeklinde döner.
        }
    }
#region PowerUps
    // shuffle sistemi

#region Shuffle PowerUp
    public IEnumerator ExecuteShuffle()
    {
        if (IsAnimating)
            yield break;

        // Lock the board during the entire operation.
        IsAnimating = true;

        // Clear any previously selected tile.
        ClearSelectionForBoardChange();

        // Shuffle existing items.
        yield return StartCoroutine(
            BoardManager.Instance.ShuffleBoardRoutine()
        );

        // Resolve matches, gravity, cascades and deadlocks.
        List<Cell> matches = MatchDetector.Instance.FindAllMatches();

        yield return StartCoroutine(
            ProcessBoardRoutine(matches)
        );

        // Unlock only after everything is finished.
        IsAnimating = false;

        if (GameManager.Instance != null)
            GameManager.Instance.EvaluateGameEnd();
    }

    private void ClearSelectionForBoardChange()
    {
        if (firstSelectedCell == null)
            return;

        if (firstSelectedCell.HasItem)
        {
            Item item = firstSelectedCell.CurrentItem;

            LeanTween.cancel(item.gameObject);

            item.transform.localScale = Vector3.one;

            Vector3 position = BoardManager.Instance
                .GridToWorldPosition(
                    firstSelectedCell.X,
                    firstSelectedCell.Y
                );

            position.y = 0.5f;
            item.transform.position = position;
        }

        firstSelectedCell = null;
    }

    //Hammer power up

    public void CancelCurrentSelection()
    {
        if (IsAnimating) return;

        ClearSelectionForBoardChange();
    }

#endregion

#region Hammer PowerUp
    public bool CanHammerTarget(int x, int y)
    {
        if (IsAnimating || BoardManager.Instance == null)
            return false;

        Cell cell = BoardManager.Instance.GetCell(x, y);

        return cell != null &&
            cell.IsPlayable &&
            (cell.HasItem || cell.HasGoalItem);
    }

    public IEnumerator ExecuteHammer(int x, int y)
    {
        if (!CanHammerTarget(x, y))
            yield break;

        // Lock the board immediately.
        IsAnimating = true;

        ClearSelectionForBoardChange();

        Cell targetCell = BoardManager.Instance.GetCell(x, y);

        if (targetCell.HasGoalItem)
        {
            // Collect through the existing Goal system.
            GoalItem goal = targetCell.ExtractGoalItem();

            LeanTween.cancel(goal.gameObject);
            goal.Collect();
        }
        else if (targetCell.HasItem)
        {
            Item item = targetCell.ExtractItem();

            LeanTween.cancel(item.gameObject);

            // Small destruction animation.
            LeanTween.scale(
            item.gameObject,
            Vector3.zero,
            0.15f
                            ).setEaseInBack();

            yield return new WaitForSeconds(0.15f);

            item.transform.localScale = Vector3.one;
            ItemPool.Instance.ReturnItem(item);
        }

        // Fill the empty cell.
        yield return StartCoroutine(
            BoardGravity.Instance.ApplyGravityRoutine()
        );

        // Resolve matches, cascades and deadlocks.
        List<Cell> matches = MatchDetector.Instance.FindAllMatches();

        yield return StartCoroutine(
            ProcessBoardRoutine(matches)
        );

        // Unlock after all board operations finish.
        IsAnimating = false;

        if (GameManager.Instance != null)
            GameManager.Instance.EvaluateGameEnd();
    }
#endregion
    //Bomb ile alakalı

#region Bomb PowerUp
    public bool CanBombTarget(int x, int y)
    {
        if (IsAnimating || BoardManager.Instance == null)
            return false;

        Cell cell = BoardManager.Instance.GetCell(x, y);

        return cell != null &&
            cell.IsPlayable &&
            (cell.HasItem || cell.HasGoalItem);
    }

    public IEnumerator ExecuteBomb(int centerX, int centerY)
    {
        if (!CanBombTarget(centerX, centerY))
            yield break;

        // Lock the board during the entire operation.
        IsAnimating = true;
        ClearSelectionForBoardChange();

        List<Item> itemsToDestroy = new List<Item>(9);
        List<GoalItem> goalsToCollect = new List<GoalItem>(9);

        // Scan the 3x3 area around the selected cell.
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Cell cell = BoardManager.Instance.GetCell(
                    centerX + dx,
                    centerY + dy
                );

                // Ignore out-of-bounds, Void and Obstacle cells.
                if (cell == null || !cell.IsPlayable)
                    continue;

                if (cell.HasItem)
                {
                    itemsToDestroy.Add(cell.ExtractItem());
                }
                else if (cell.HasGoalItem)
                {
                    goalsToCollect.Add(cell.ExtractGoalItem());
                }
            }
        }

        // Animate all affected objects simultaneously.
        const float destroyDuration = 0.2f;

        foreach (Item item in itemsToDestroy)
        {
            LeanTween.cancel(item.gameObject);

            LeanTween.scale(
                item.gameObject,
                Vector3.zero,
                destroyDuration
            ).setEaseInBack();
        }

        foreach (GoalItem goal in goalsToCollect)
        {
            LeanTween.cancel(goal.gameObject);

            LeanTween.scale(
                goal.gameObject,
                Vector3.zero,
                destroyDuration
            ).setEaseInBack();
        }

        yield return new WaitForSeconds(destroyDuration);

        // Return normal items to their pool.
        foreach (Item item in itemsToDestroy)
        {
            item.transform.localScale = Vector3.one;
            ItemPool.Instance.ReturnItem(item);
        }

        // Collect goals through the existing goal system.
        foreach (GoalItem goal in goalsToCollect)
        {
            goal.Collect();
        }

        // Fill empty cells using the existing gravity system.
        yield return StartCoroutine(
            BoardGravity.Instance.ApplyGravityRoutine()
        );

        // Resolve matches, cascades and deadlocks.
        List<Cell> matches =
            MatchDetector.Instance.FindAllMatches();

        yield return StartCoroutine(
            ProcessBoardRoutine(matches)
        );

        // Unlock only after every board operation finishes.
        IsAnimating = false;

        if (GameManager.Instance != null)
            GameManager.Instance.EvaluateGameEnd();
    }

#endregion



#endregion

    private IEnumerator SwapRoutine(Cell cellA, Cell cellB)
    {
        IsAnimating = true;

        Item itemA = cellA.CurrentItem;
        Item itemB = cellB.CurrentItem;

        Vector3 posA = BoardManager.Instance.GridToWorldPosition(cellA.X, cellA.Y);
        posA.y = 0.5f;
        Vector3 posB = BoardManager.Instance.GridToWorldPosition(cellB.X, cellB.Y);
        posB.y = 0.5f;

        BoardManager.Instance.SwapItemsData(cellA.X, cellA.Y, cellB.X, cellB.Y);

        LeanTween.cancel(itemA.gameObject);
        LeanTween.scale(itemA.gameObject, Vector3.one, swapDuration).setEaseInOutQuad();
        LeanTween.move(itemA.gameObject, posB, swapDuration).setEaseInOutQuad();
        
        LeanTween.cancel(itemB.gameObject);
        LeanTween.scale(itemB.gameObject, Vector3.one, swapDuration).setEaseInOutQuad();
        LeanTween.move(itemB.gameObject, posA, swapDuration).setEaseInOutQuad();

        yield return new WaitForSeconds(swapDuration);

        List<Cell> matches = MatchDetector.Instance.FindMatches(cellA, cellB);

        if (matches.Count == 0)
        {
            BoardManager.Instance.SwapItemsData(cellA.X, cellA.Y, cellB.X, cellB.Y);
            
            LeanTween.move(itemA.gameObject, posA, swapDuration).setEaseInOutQuad();
            LeanTween.move(itemB.gameObject, posB, swapDuration).setEaseInOutQuad();

            yield return new WaitForSeconds(swapDuration);
            
            IsAnimating = false;
            firstSelectedCell = null;

            // YENİ: Taşlar geri döndü ve animasyon bitti. Eğer hamle bitmişse hakemi çağır.
            if (GameManager.Instance != null) GameManager.Instance.EvaluateGameEnd();
        }
        else
        {
            // Eşleşme başarılı oldu, tam bu anda 1 hamle (Move) düşüyoruz.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DecreaseMove();
            }
            
            // Eşleşme varsa "ProcessBoardRoutine" adındaki şelale döngüsüne giriyoruz!
            yield return StartCoroutine(ProcessBoardRoutine(matches));
            
            firstSelectedCell = null;
            IsAnimating = false;

            // YENİ: Dev şelale bitti, tahta tamamen duruldu. Hakemi çağır!
            if (GameManager.Instance != null) GameManager.Instance.EvaluateGameEnd();
        }
    }

    private IEnumerator DestroyMatchesRoutine(List<Cell> matches)
    {
        foreach (Cell cell in matches)
        {
            if (cell.HasItem)
            {
                Item item = cell.CurrentItem;
                LeanTween.cancel(item.gameObject);
                
                Vector3 upPos = item.transform.position;
                upPos.y += 0.5f;

                LeanTween.move(item.gameObject, upPos, 0.15f).setEaseOutQuad();
                LeanTween.scale(item.gameObject, Vector3.one * 1.15f, 0.15f).setEaseOutQuad();
            }
        }

        yield return new WaitForSeconds(0.15f);

        foreach (Cell cell in matches)
        {
            if (cell.HasItem)
            {
                Item item = cell.CurrentItem;
                LeanTween.scale(item.gameObject, Vector3.zero, 0.2f).setEaseInBack();
            }
        }

        yield return new WaitForSeconds(0.2f);

        // =========================================================
        // YENİ: KOMŞU (ADJACENT) RADARI
        // =========================================================
        HashSet<GoalItem> collectedGoals = new HashSet<GoalItem>();

        foreach (Cell cell in matches)
        {
            Vector2Int[] neighbors = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in neighbors)
            {
                Cell neighborCell = BoardManager.Instance.GetCell(cell.X + dir.x, cell.Y + dir.y);
                
                // Eğer komşuda bir Hedef (Kupa/Saat) varsa onu listeye ekle
                if (neighborCell != null && neighborCell.HasGoalItem)
                {
                    // HashSet kullandığımız için, 2 farklı patlayan taş aynı Kupaya değiyorsa bile sadece 1 kez listeye eklenir
                    collectedGoals.Add(neighborCell.CurrentGoalItem); 
                }
            }
        }

        // Toplanan hedefleri tahtadan sök ve Collect() çağır
        foreach (GoalItem goal in collectedGoals)
        {
            Cell cell = BoardManager.Instance.GetCell(goal.X, goal.Y);
            if (cell != null) cell.ExtractGoalItem(); // Tahtadan bağını kopar
            goal.Collect(); // Skoru düşür ve yok ol
        }
        // =========================================================

        foreach (Cell cell in matches)
        {
            Item itemToDestroy = cell.ExtractItem();
            if (itemToDestroy != null)
            {
                itemToDestroy.transform.localScale = Vector3.one;
                ItemPool.Instance.ReturnItem(itemToDestroy);
            }
        }
    }
}