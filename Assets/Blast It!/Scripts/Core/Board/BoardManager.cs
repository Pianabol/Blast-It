using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("Board Settings")]
    [SerializeField, Min(3)] private int width = 8;
    [SerializeField, Min(3)] private int height = 8;
    [SerializeField] private float cellSize = 1.2f;

    [Header("Transform References")]
    [SerializeField] private Transform boardRoot; 

    [Header("Visual Controller")]
    [SerializeField] private BoardVisualController visualController;

    private Cell[,] grid;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitializeBoard();
    }

    public void InitializeBoard()
    {
        grid = new Cell[width, height];

        // 3D için Y yerine Z eksenine offset veriyoruz! Y ekseni 0'da kalıyor.
        Vector3 offset = new Vector3((width - 1) * cellSize / 2f, 0f, (height - 1) * cellSize / 2f);
        boardRoot.position = -offset;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Cell(x, y, CellType.Normal);
            }
        }

        Debug.Log($"<color=green>Board Initialized (3D Mode):</color> {width}x{height}");

        // Görselleri çiz
        if (visualController != null)
        {
            visualController.GenerateBoardVisuals(width, height, cellSize);
        }

        // Tahtayı güvenli (eşleşmesiz) taşlarla doldur
        FillBoardWithRandomItems();
    }

    /// <summary>
    /// Tahtadaki tüm oynanabilir boş hücreleri havuzdan çekilen rastgele ve güvenli taşlarla doldurur.
    /// </summary>
    private void FillBoardWithRandomItems()
    {
        if (ItemPool.Instance == null)
        {
            Debug.LogError("[BoardManager] ItemPool sahnede bulunamadı! Taş dizilemiyor.");
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];

                if (cell.IsPlayable && !cell.HasItem)
                {
                    Item newItem = null;
                    int maxAttempts = 100;
                    int attempts = 0;

                    // Güvenli (yan yana 3 tane olmayan) bir taş bulana kadar çek-iade et
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

                    // Aşırı ekstrem bir durumda sonsuz döngüden çıkılırsa rastgele bir taş ata
                    if (newItem == null)
                    {
                        newItem = ItemPool.Instance.GetRandomItem();
                    }

                    if (newItem != null)
                    {
                        cell.SetItem(newItem);
                        newItem.UpdateCoordinates(x, y);

                        // 3D konumlandırma (Taşı zeminin biraz üstüne koyuyoruz)
                        Vector3 targetPos = GridToWorldPosition(x, y);
                        targetPos.y = 0.5f; 
                        
                        newItem.MoveTo(targetPos, 0f); // Anında yerine otursun
                    }
                }
            }
        }
        
        Debug.Log("<color=cyan>Board Filled with Safe Items!</color>");
    }

    /// <summary>
    /// Verilen koordinata belirtilen tipte bir taş konursa 3'lü eşleşme olup olmayacağını denetler.
    /// (Sadece sol ve aşağı yönleri kontrol eder çünkü taşlar soldan sağa, aşağıdan yukarı diziliyor)
    /// </summary>
    private bool CausesMatch(int x, int y, ItemType type)
    {
        // Yatay Kontrol (Sola doğru 2 taş var mı?)
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

        // Dikey Kontrol (Aşağı doğru 2 taş var mı?)
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

        // Hücrelerden taşları sök al
        Item item1 = cell1.ExtractItem();
        Item item2 = cell2.ExtractItem();

        // Çaprazlama olarak geri yerleştir ve koordinatlarını güncelle
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