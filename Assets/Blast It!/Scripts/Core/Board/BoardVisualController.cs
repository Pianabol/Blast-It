using UnityEngine;

public class BoardVisualController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Tile tilePrefab;

    [Header("Hierarchy Containers")]
    [SerializeField] private Transform tilesRoot; // Sahnede Hierarchy'i temiz tutmak için

    [Header("Visual Settings")]
    [SerializeField] private Color colorA = new Color(0.8f, 0.8f, 0.8f); // Açık renk (Dama deseni için)
    [SerializeField] private Color colorB = new Color(0.6f, 0.6f, 0.6f); // Koyu renk (Dama deseni için)

    /// <summary>
    /// BoardManager'dan gelen grid verilerine göre arka planı (Tiles) sahnede oluşturur.
    /// </summary>
    public void GenerateBoardVisuals(int width, int height, float cellSize)
    {
        if (tilePrefab == null || tilesRoot == null)
        {
            Debug.LogError("[BoardVisualController] Tile Prefab veya TilesRoot atanmamış!");
            return;
        }

        // Varsa eski objeleri temizle (Level yeniden başlarken vs.)
        ClearTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Pozisyonu BoardManager'dan alıyoruz (O zaten offset'i falan ayarladı)
                Vector3 tilePos = BoardManager.Instance.GridToWorldPosition(x, y);

                // Tile objesini yarat
                Tile newTile = Instantiate(tilePrefab, tilePos, Quaternion.identity, tilesRoot);
                newTile.name = $"Tile_{x}_{y}";

                // Dama tahtası deseni oluştur
                bool isEven = (x + y) % 2 == 0;
                newTile.SetColor(isEven ? colorA : colorB);
                
                // DÜZELTME: 3D uzayda Y (kalınlık) eksenini ezerek ince bir zemin yapıyoruz.
                // X (genişlik) ve Z (derinlik) eksenlerini cellSize yaparak aradaki boşlukları kapatıyoruz.
                newTile.transform.localScale = new Vector3(cellSize, 0.15f, cellSize); 
            }
        }
        
        Debug.Log("<color=cyan>Board Visuals Generated!</color>");
    }

    private void ClearTiles()
    {
        // Editörde de çalışabilmesi için özel döngü
        for (int i = tilesRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(tilesRoot.GetChild(i).gameObject);
        }
    }
}