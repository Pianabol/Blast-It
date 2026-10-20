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
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(Input.mousePosition);
        }
    }

    private void HandleClick(Vector3 screenPosition)
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, boardLayer))
        {
            Vector2Int gridCoordinate = BoardManager.Instance.WorldToGridPosition(hit.point);

            if (BoardManager.Instance.IsValidCoordinate(gridCoordinate.x, gridCoordinate.y))
            {
                Debug.Log($"<color=yellow>[INPUT]</color> Tıklanan Hücre: [{gridCoordinate.x}, {gridCoordinate.y}]");
                
                // KRİTİK BAĞLANTI: Tıklama bilgisini Şef'e gönderiyoruz!
                if (BoardOrchestrator.Instance != null)
                {
                    BoardOrchestrator.Instance.OnCellClicked(gridCoordinate.x, gridCoordinate.y);
                }
                else
                {
                    Debug.LogError("<color=red>[InputManager] Sahnede BoardOrchestrator bulunamadı! Hierarchy'e ekledin mi?</color>");
                }
            }
            else
            {
                Debug.Log("<color=grey>[INPUT]</color> Tahta sınırları dışına tıklandı.");
            }
        }
    }
}