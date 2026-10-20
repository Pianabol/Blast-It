using UnityEngine;

public class BoardVisualController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Tile tilePrefab;

    [Header("Hierarchy Containers")]
    [SerializeField] private Transform tilesRoot;

    [Header("Visual Settings")]
    [SerializeField] private Color colorA = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color colorB = new Color(0.6f, 0.6f, 0.6f);

    // DİKKAT: Artık 4. parametre olarak Cell[,] grid alıyoruz
    public void GenerateBoardVisuals(int width, int height, float cellSize, Cell[,] grid)
    {
        if (tilePrefab == null || tilesRoot == null) return;

        ClearTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];

                // Eğer hücre VOID (Boşluk) ise oraya zemin çizme, es geç!
                if (cell.Type == CellType.Void)
                {
                    continue;
                }

                Vector3 tilePos = BoardManager.Instance.GridToWorldPosition(x, y);

                Tile newTile = Instantiate(tilePrefab, tilePos, Quaternion.identity, tilesRoot);
                newTile.name = $"Tile_{x}_{y}";

                // TODO: İleride Obstacle ise farklı bir renk/prefab basacağız. Şimdilik düz zemin çiziyoruz.

                bool isEven = (x + y) % 2 == 0;
                newTile.SetColor(isEven ? colorA : colorB);
                
                newTile.transform.localScale = new Vector3(cellSize, 0.15f, cellSize); 
            }
        }
        
        Debug.Log("<color=cyan>[BoardVisualController]</color> Board Visuals Generated!");
    }

    private void ClearTiles()
    {
        for (int i = tilesRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(tilesRoot.GetChild(i).gameObject);
        }
    }
}