using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask boardLayer;
    [SerializeField] private float rayDistance = 100f;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        // Sol fare tıklaması veya mobil ekrana dokunma
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(Input.mousePosition);
        }
    }

    private void HandleClick(Vector3 screenPosition)
    {
        // Eğer kamera atanmamışsa çık
        if (mainCamera == null) return;

        // Ekrandaki tıklama noktasından 3D uzaya doğru bir ışın fırlatıyoruz
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        
        // Debug amaçlı sahnede ışını çizelim (Scene penceresinde kırmızı çizgi olarak görünür)
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 2f);

        // Işın, sadece boardLayer'da bulunan bir objeye çarparsa if bloğuna girer
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, boardLayer))
        {
            // Çarpılan noktanın 3D World koordinatını aldık
            Vector3 hitPoint = hit.point;

            // Çarpılan noktanın Board üzerindeki (x, y) indekslerini hesapla
            Vector2Int gridCoordinate = BoardManager.Instance.WorldToGridPosition(hitPoint);

            // Koordinat tahta sınırları içindeyse işlemi onayla
            if (BoardManager.Instance.IsValidCoordinate(gridCoordinate.x, gridCoordinate.y))
            {
                Debug.Log($"<color=yellow>[INPUT]</color> Tıklanan Hücre: [{gridCoordinate.x}, {gridCoordinate.y}]");
                
                // İleride buraya şunu ekleyeceğiz:
                // GameManager.Instance.OnCellClicked(gridCoordinate.x, gridCoordinate.y);
            }
            else
            {
                Debug.Log("<color=grey>[INPUT]</color> Tahta sınırları dışına tıklandı.");
            }
        }
    }

}