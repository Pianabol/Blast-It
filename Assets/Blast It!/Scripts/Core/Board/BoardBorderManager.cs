using UnityEngine;

public class BoardBorderManager : MonoBehaviour
{
    public static BoardBorderManager Instance { get; private set; }

    [Header("Border Prefabs")]
    [SerializeField] private GameObject straightPrefab;
    [SerializeField] private GameObject cornerPrefab;

    [Header("Hierarchy Containers")]
    [SerializeField] private Transform borderRoot;

    [Header("Visual Tuning")]
    [SerializeField] private float borderThickness = 0.4f; // Çerçevenin kalınlığı (Y ve Z)
    [SerializeField] private float edgeShrinkAmount = 0.1f; // Köşeye denk gelen düz çubuğun ne kadar kısaltılacağı (Bulduğun 0.1 değeri)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GenerateBorders()
    {
        ClearBorders();

        int width = BoardManager.Instance.Width;
        int height = BoardManager.Instance.Height;
        float cellSize = BoardManager.Instance.CellSize;
        float halfCell = cellSize / 2f;

        // Tahtadaki sadece "Oynanabilir (Dolu)" hücreleri geziyoruz
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!IsPlayable(x, y)) continue;

                Vector3 center = BoardManager.Instance.GridToWorldPosition(x, y);

                // Komşuluk Durumları (True ise o yön BOŞLUK demektir)
                bool top = !IsPlayable(x, y + 1);
                bool bottom = !IsPlayable(x, y - 1);
                bool right = !IsPlayable(x + 1, y);
                bool left = !IsPlayable(x - 1, y);

                bool topRight = !IsPlayable(x + 1, y + 1);
                bool topLeft = !IsPlayable(x - 1, y + 1);
                bool bottomRight = !IsPlayable(x + 1, y - 1);
                bool bottomLeft = !IsPlayable(x - 1, y - 1);

                // 1. DÜZ KENARLAR (Kenar devamlılık kontrolü ile dinamik kısaltma)
                if (top) SpawnEdge(center + new Vector3(0, 0, halfCell), 0f, cellSize, 
                    IsPlayable(x - 1, y) && !IsPlayable(x - 1, y + 1), // Sola devam ediyor mu?
                    IsPlayable(x + 1, y) && !IsPlayable(x + 1, y + 1)); // Sağa devam ediyor mu?

                if (bottom) SpawnEdge(center + new Vector3(0, 0, -halfCell), 180f, cellSize, 
                    IsPlayable(x + 1, y) && !IsPlayable(x + 1, y - 1), // 180 derece lokal sol (dünya sağ)
                    IsPlayable(x - 1, y) && !IsPlayable(x - 1, y - 1)); // lokal sağ (dünya sol)

                if (right) SpawnEdge(center + new Vector3(halfCell, 0, 0), 90f, cellSize, 
                    IsPlayable(x, y + 1) && !IsPlayable(x + 1, y + 1), // 90 derece lokal sol (dünya yukarı)
                    IsPlayable(x, y - 1) && !IsPlayable(x + 1, y - 1)); // lokal sağ (dünya aşağı)

                if (left) SpawnEdge(center + new Vector3(-halfCell, 0, 0), 270f, cellSize, 
                    IsPlayable(x, y - 1) && !IsPlayable(x - 1, y - 1), // 270 derece lokal sol (dünya aşağı)
                    IsPlayable(x, y + 1) && !IsPlayable(x - 1, y + 1)); // lokal sağ (dünya yukarı)

                // 2. DIŞ KÖŞELER (İki dış kenarın birleştiği sivri uçlar)
                if (top && right) SpawnPiece(cornerPrefab, center + new Vector3(halfCell, 0, halfCell), 0f);
                if (bottom && right) SpawnPiece(cornerPrefab, center + new Vector3(halfCell, 0, -halfCell), 90f);
                if (bottom && left) SpawnPiece(cornerPrefab, center + new Vector3(-halfCell, 0, -halfCell), 180f);
                if (top && left) SpawnPiece(cornerPrefab, center + new Vector3(-halfCell, 0, halfCell), 270f);

                // 3. İÇ KÖŞELER (İçeriye doğru bükülen Void deliklerinin köşeleri)
                if (!top && !right && topRight) SpawnPiece(cornerPrefab, center + new Vector3(halfCell, 0, halfCell), 180f);
                if (!bottom && !right && bottomRight) SpawnPiece(cornerPrefab, center + new Vector3(halfCell, 0, -halfCell), 270f);
                if (!bottom && !left && bottomLeft) SpawnPiece(cornerPrefab, center + new Vector3(-halfCell, 0, -halfCell), 0f);
                if (!top && !left && topLeft) SpawnPiece(cornerPrefab, center + new Vector3(-halfCell, 0, halfCell), 90f);
            }
        }
        
        Debug.Log("<color=cyan>[BoardBorderManager]</color> Kusursuz Çerçeveler Çizildi!");
    }

    private bool IsPlayable(int cx, int cy)
    {
        if (BoardManager.Instance.IsValidCoordinate(cx, cy))
        {
            return BoardManager.Instance.GetCell(cx, cy).IsPlayable;
        }
        return false;
    }

    // YENİ: Düz çubukları uçlarındaki duruma göre dinamik tıraşlayan metot
    private void SpawnEdge(Vector3 pos, float rotationY, float cellSize, bool continuesLeft, bool continuesRight)
    {
        float length = cellSize;
        float shiftOffset = 0f;

        // Sol uçta köşe varsa (kenar devam etmiyorsa) soldan kırp, sağa kaydır
        if (!continuesLeft) 
        { 
            length -= edgeShrinkAmount; 
            shiftOffset += edgeShrinkAmount / 2f; 
        }
        
        // Sağ uçta köşe varsa sağdan kırp, sola kaydır
        if (!continuesRight) 
        { 
            length -= edgeShrinkAmount; 
            shiftOffset -= edgeShrinkAmount / 2f; 
        }

        // Rotasyona göre pozisyon kaydırmasını lokal eksende uygula
        Vector3 finalPos = pos + Quaternion.Euler(0, rotationY, 0) * new Vector3(shiftOffset, 0, 0);

        GameObject piece = Instantiate(straightPrefab, finalPos, Quaternion.Euler(0, rotationY, 0), borderRoot);
        piece.transform.localScale = new Vector3(length, borderThickness, borderThickness);
    }

    // Köşe parçaları (Kapaklar) için standart metot
    private void SpawnPiece(GameObject prefab, Vector3 pos, float rotationY)
    {
        if (prefab == null || borderRoot == null) return;

        GameObject piece = Instantiate(prefab, pos, Quaternion.Euler(0, rotationY, 0), borderRoot);
        piece.transform.localScale = new Vector3(borderThickness, borderThickness, borderThickness);
    }

    public void ClearBorders()
    {
        if (borderRoot == null) return;
        for (int i = borderRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(borderRoot.GetChild(i).gameObject);
        }
    }
}