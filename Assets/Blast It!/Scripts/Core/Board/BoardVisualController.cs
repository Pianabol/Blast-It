using UnityEngine;

public class BoardVisualController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Tile tilePrefab;

    [Header("Hierarchy Containers")]
    [SerializeField] private Transform tilesRoot;

    // Renk değişkenlerini (colorA, colorB) sildik!

    public void GenerateBoardVisuals(int width, int height, float cellSize, Cell[,] grid)
    {
        if (tilePrefab == null || tilesRoot == null) return;

        ClearTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];

                if (cell.Type == CellType.Void)
                {
                    continue;
                }

                Vector3 tilePos = BoardManager.Instance.GridToWorldPosition(x, y);

                Tile newTile = Instantiate(tilePrefab, tilePos, Quaternion.identity, tilesRoot);
                newTile.name = $"Tile_{x}_{y}";

                // Y eksenindeki 0.15f ezme işlemini sildik! Artık modelin kendi orijinal kalınlığı korunacak.
                // Sadece X ve Z eksenlerinde hücre boyutuna göre scale ediyoruz, Y eksenini 1 bırakıyoruz.
                newTile.transform.localScale = new Vector3(cellSize, 1f, cellSize); 
            }
        }
        
        Debug.Log("<color=cyan>[BoardVisualController]</color> Board Visuals Generated with Glass Tiles!");
    }

    public void ClearTiles()
    {
        for (int i = tilesRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(tilesRoot.GetChild(i).gameObject);
        }
    }
}