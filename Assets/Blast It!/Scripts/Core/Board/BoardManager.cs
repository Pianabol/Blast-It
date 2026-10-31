using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("Board Settings")]
    [SerializeField] private float cellSize = 1.2f;

    [Header("Transform References")]
    [SerializeField] private Transform boardRoot; 

    [Header("Visual Controller")]
    [SerializeField] private BoardVisualController visualController;

    private Cell[,] grid;
    private int width;
    private int height;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Start metodu içindeki InitializeBoard() çağrısını sildik. 
    /// Artık LevelLoader LevelData verisiyle burayı dışarıdan tetikleyecek.
    /// </summary>
    public void InitializeBoard(LevelData levelData)
    {
        ClearBoard();

        // =========================================================
        // YENİ: Havuza bu levelda hangi renklerin serbest olduğunu bildir
        if (ItemPool.Instance != null && levelData.allowedItemTypes != null)
        {
            ItemPool.Instance.SetAllowedItems(levelData.allowedItemTypes);
        }
        // =========================================================

        width = levelData.width;
        height = levelData.height;
        grid = new Cell[width, height];

        Vector3 offset = new Vector3((width - 1) * cellSize / 2f, 0f, (height - 1) * cellSize / 2f);
        boardRoot.position = -offset;

        // 1. AŞAMA: Hücreleri ve Zemin Tiplerini (Void/Obstacle) oluştur
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CellType type = levelData.GetCellType(x, y);
                grid[x, y] = new Cell(x, y, type);
                if (type == CellType.Void || type == CellType.Obstacle) grid[x, y].IsPlayable = false;
            }
        }

        if (visualController != null) visualController.GenerateBoardVisuals(width, height, cellSize, grid);
        if (BoardBorderManager.Instance != null) BoardBorderManager.Instance.GenerateBorders();

        // 2. AŞAMA: Grid Painter ile çizilen Hedefleri (Kupa, Saat) tahtaya dik!
        SpawnGoalItemsFromData(levelData);

        // 3. AŞAMA: Kalan boşlukları (IsEmpty) Normal Taşlarla Doldur
        FillBoardWithRandomItems();

        // 4. AŞAMA: Kamerayı Tahtaya Göre Otomatik Boyutlandır
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.FrameBoard(width, height, cellSize);
        }
    }

    private void SpawnGoalItemsFromData(LevelData levelData)
    {
        if (GoalItemPool.Instance == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];
                if (!cell.IsPlayable) continue;

                GoalType goalType = levelData.GetGoalType(x, y);
                if (goalType != GoalType.None) // O hücreye hedef çizilmişse
                {
                    GoalItem newGoal = GoalItemPool.Instance.GetItem(goalType);
                    if (newGoal != null)
                    {
                        cell.SetGoalItem(newGoal);
                        newGoal.UpdateCoordinates(x, y);

                        Vector3 targetPos = GridToWorldPosition(x, y);
                        targetPos.y = 0.5f;
                        newGoal.transform.position = targetPos;
                    }
                }
            }
        }
        Debug.Log("<color=cyan>[BoardManager]</color> Editor hedefleri sahneye yerleştirildi!");
    }

    private void FillBoardWithRandomItems()
    {
        if (ItemPool.Instance == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];

                // DİKKAT: Artık !cell.HasItem değil, cell.IsEmpty kullanıyoruz! (Goal varsa ezmesin)
                if (cell.IsPlayable && cell.IsEmpty) 
                {
                    Item newItem = null;
                    int maxAttempts = 100;
                    int attempts = 0;

                    do
                    {
                        newItem = ItemPool.Instance.GetRandomItem();
                        attempts++;

                        if (newItem != null && CausesMatch(x, y, newItem.Type))
                        {
                            ItemPool.Instance.ReturnItem(newItem);
                            newItem = null;
                        }
                    } while (newItem == null && attempts < maxAttempts);

                    if (newItem == null) newItem = ItemPool.Instance.GetRandomItem();

                    if (newItem != null)
                    {
                        cell.SetItem(newItem);
                        newItem.UpdateCoordinates(x, y);

                        Vector3 targetPos = GridToWorldPosition(x, y);
                        targetPos.y = 0.5f; 
                        newItem.MoveTo(targetPos, 0f);
                    }
                }
            }
        }
    }

    public void ClearBoard()
    {
        if (grid != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y] != null)
                    {
                        // 1. Normal taşları temizle
                        if (grid[x, y].HasItem)
                        {
                            Item itemToReturn = grid[x, y].ExtractItem();
                            itemToReturn.transform.localScale = Vector3.one; 
                            ItemPool.Instance.ReturnItem(itemToReturn);
                        }
                        
                        // 2. GoalItem'ları (Kupa, Saat) temizle
                        if (grid[x, y].HasGoalItem)
                        {
                            GoalItem goalToReturn = grid[x, y].ExtractGoalItem();
                            goalToReturn.transform.localScale = Vector3.one;
                            GoalItemPool.Instance.ReturnItem(goalToReturn);
                        }
                    }
                }
            }
            grid = null; 
        }

        if (visualController != null) visualController.ClearTiles();
        if (BoardBorderManager.Instance != null) BoardBorderManager.Instance.ClearBorders();
    }

    // ====================================================================
    // YENİ: TAHTA KARIŞTIRMA (SHUFFLE) SİSTEMİ
    // ====================================================================

    /// <summary>
    /// Tahtadaki tüm taşları rastgele karıştırır ve yeni yerlerine uçurur.
    /// </summary>
    public IEnumerator ShuffleBoardRoutine()
    {
        Debug.Log("<color=yellow>[BoardManager]</color> Tahta Karıştırılıyor (Shuffle)...");

        List<Item> allItems = new List<Item>();
        List<Cell> validCells = new List<Cell>();

        // 1. Oynanabilir tüm hücreleri ve içindeki taşları topla
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];
                if (cell != null && cell.IsPlayable && cell.HasItem)
                {
                    allItems.Add(cell.ExtractItem()); // Taşı hücreden kopar
                    validCells.Add(cell);
                }
            }
        }

        // 2. Fisher-Yates Algoritması ile taşları listende karıştır
        for (int i = 0; i < allItems.Count; i++)
        {
            Item temp = allItems[i];
            int randomIndex = Random.Range(i, allItems.Count);
            allItems[i] = allItems[randomIndex];
            allItems[randomIndex] = temp;
        }

        // 3. Karıştırılan taşları hücrelere geri dağıt ve hareket ettir (LeanTween)
        float shuffleDuration = 0.5f;
        for (int i = 0; i < validCells.Count; i++)
        {
            Cell cell = validCells[i];
            Item item = allItems[i];

            cell.SetItem(item);
            item.UpdateCoordinates(cell.X, cell.Y);

            Vector3 targetPos = GridToWorldPosition(cell.X, cell.Y);
            targetPos.y = 0.5f; // Tahtanın üzerindeki varsayılan yükseklik

            LeanTween.move(item.gameObject, targetPos, shuffleDuration).setEaseInOutBack();
        }

        // Animasyonların bitmesini bekle
        yield return new WaitForSeconds(shuffleDuration + 0.1f);
    }

    private bool CausesMatch(int x, int y, ItemType type)
    {
        if (x >= 2)
        {
            Cell left1 = GetCell(x - 1, y);
            Cell left2 = GetCell(x - 2, y);

            if (left1 != null && left2 != null && left1.HasItem && left2.HasItem)
            {
                if (left1.CurrentItem.Type == type && left2.CurrentItem.Type == type)
                    return true;
            }
        }

        if (y >= 2)
        {
            Cell down1 = GetCell(x, y - 1);
            Cell down2 = GetCell(x, y - 2);

            if (down1 != null && down2 != null && down1.HasItem && down2.HasItem)
            {
                if (down1.CurrentItem.Type == type && down2.CurrentItem.Type == type)
                    return true;
            }
        }

        return false;
    }

    public Vector3 GridToWorldPosition(int x, int y)
    {
        return new Vector3(x * cellSize, 0f, y * cellSize) + boardRoot.position;
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = boardRoot.InverseTransformPoint(worldPosition);
        int x = Mathf.RoundToInt(localPosition.x / cellSize);
        int y = Mathf.RoundToInt(localPosition.z / cellSize);
        return new Vector2Int(x, y);
    }

    public bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public Cell GetCell(int x, int y)
    {
        if (IsValidCoordinate(x, y)) return grid[x, y];
        return null;
    }

    public void SwapItemsData(int x1, int y1, int x2, int y2)
    {
        Cell cell1 = GetCell(x1, y1);
        Cell cell2 = GetCell(x2, y2);

        Item item1 = cell1.ExtractItem();
        Item item2 = cell2.ExtractItem();

        if (item2 != null)
        {
            cell1.SetItem(item2);
            item2.UpdateCoordinates(cell1.X, cell1.Y);
        }
        if (item1 != null)
        {
            cell2.SetItem(item1);
            item1.UpdateCoordinates(cell2.X, cell2.Y);
        }
    }
}