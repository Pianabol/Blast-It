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
        //Önceki level'ı temizle

        ClearBoard();
        
        width = levelData.width;
        height = levelData.height;

        grid = new Cell[width, height];

        Vector3 offset = new Vector3((width - 1) * cellSize / 2f, 0f, (height - 1) * cellSize / 2f);
        boardRoot.position = -offset;

        // 1. AŞAMA: LevelData'dan Grid (Void/Obstacle) haritasını oku ve hücreleri oluştur
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CellType type = levelData.GetCellType(x, y);
                grid[x, y] = new Cell(x, y, type);

                // DİKKAT: Void veya Obstacle ise buraya taş gelemez!
                if (type == CellType.Void || type == CellType.Obstacle)
                {
                    grid[x, y].IsPlayable = false;
                }
            }
        }

        Debug.Log($"<color=green>[BoardManager]</color> Board Initialized (Data-Driven): {width}x{height}");

        // 2. AŞAMA: Görselleri çiz (Void/Obstacle görselleri de eklenecek)
        if (visualController != null)
        {
            visualController.GenerateBoardVisuals(width, height, cellSize, grid);
        }

        // 3. AŞAMA: Tahtayı Taşlarla Doldur
        FillBoardWithRandomItems();
    }

    private void FillBoardWithRandomItems()
    {
        if (ItemPool.Instance == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];

                // DİKKAT: Sadece IsPlayable olan (Normal) hücrelere taş koy
                if (cell.IsPlayable && !cell.HasItem)
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

                    if (newItem == null)
                    {
                        newItem = ItemPool.Instance.GetRandomItem();
                    }

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
        
        Debug.Log("<color=cyan>[BoardManager]</color> Board Filled with Safe Items!");
    }

    /// <summary>
    /// Sahnede var olan eski tahtayı, taşları ve grid verilerini tamamen temizler.
    /// </summary>
    public void ClearBoard()
    {
        if (grid != null)
        {
            // 1. Grid üzerindeki tüm taşları (Item) bul ve Object Pool'a (Havuza) iade et
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y] != null && grid[x, y].HasItem)
                    {
                        Item itemToReturn = grid[x, y].ExtractItem();
                        
                        // Taşın ölçeğini sıfırlayıp havuza gönderiyoruz ki bir sonraki levele temiz gitsin
                        itemToReturn.transform.localScale = Vector3.one; 
                        ItemPool.Instance.ReturnItem(itemToReturn);
                    }
                }
            }
            // 2. Eski grid verisini hafızadan sil
            grid = null; 
        }

        // 3. Altımızdaki kare zemin (Tile) görsellerini yok et
        if (visualController != null)
        {
            visualController.ClearTiles();
        }

        Debug.Log("<color=red>[BoardManager]</color> Eski tahta tamamen yıkıldı ve temizlendi!");
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