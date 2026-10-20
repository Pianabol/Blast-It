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

        // DİKKAT: 3D için Y yerine Z eksenine offset veriyoruz! Y ekseni 0'da kalıyor.
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

        if (visualController != null)
        {
            visualController.GenerateBoardVisuals(width, height, cellSize);
        }
    }

    // DİKKAT: 3D için X ve Z eksenleri kullanılıyor, Y ekseni (yükseklik) şimdilik 0.
    public Vector3 GridToWorldPosition(int x, int y)
    {
        return new Vector3(x * cellSize, 0f, y * cellSize) + boardRoot.position;
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

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        // Dünya koordinatını, BoardRoot'un yerel (local) koordinatına çevir
        Vector3 localPosition = boardRoot.InverseTransformPoint(worldPosition);

        // Hücre boyutuna bölüp en yakın tam sayıya yuvarla
        int x = Mathf.RoundToInt(localPosition.x / cellSize);
        
        // 3D olduğu için yerleşim Z ekseninde. Bu yüzden z'yi okuyoruz.
        int y = Mathf.RoundToInt(localPosition.z / cellSize);

        return new Vector2Int(x, y);
    }
}