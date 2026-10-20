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
    private bool isAnimating = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnCellClicked(int x, int y)
    {

        // YENİ EKLENEN KİLİT: Eğer oyun bitmişse veya başlamadıysa tıklamaları tamamen engelle
        if (GameManager.Instance != null && !GameManager.Instance.CanPlay()) return;

        if (isAnimating) return;

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

    private IEnumerator SwapRoutine(Cell cellA, Cell cellB)
    {
        isAnimating = true;

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
            
            isAnimating = false;
            firstSelectedCell = null;
        }
        else
        {
            // YENİ EKLENEN SATIR: Eşleşme başarılı oldu, tam bu anda 1 hamle (Move) düşüyoruz.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DecreaseMove();
            }
            
            // Eşleşme varsa "ProcessBoardRoutine" adındaki şelale döngüsüne giriyoruz!
            yield return StartCoroutine(ProcessBoardRoutine(matches));
            
            firstSelectedCell = null;
            isAnimating = false;
        }
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