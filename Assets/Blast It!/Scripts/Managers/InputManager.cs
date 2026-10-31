using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask boardLayer;
    [SerializeField] private float rayDistance = 100f;

    private void Update()
    {
        // Fare tıklaması (veya dokunmatik ekran) kontrolü
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(Input.mousePosition);
        }

        // Manuel Shuffle Test Tetikleyicisi
        if (Input.GetKeyDown(KeyCode.S))
        {
            TriggerManualShuffle();
        }
    }

    private void HandleClick(Vector3 screenPosition)
    {
        if (CameraManager.Instance == null) return;

        // DİKKAT: Kamerayı ve Işını (Ray) artık CameraManager'dan istiyoruz! (Tam SoC)
        Ray ray = CameraManager.Instance.GetScreenRay(screenPosition);
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, boardLayer))
        {
            Vector2Int gridCoordinate = BoardManager.Instance.WorldToGridPosition(hit.point);

            if (BoardManager.Instance.IsValidCoordinate(gridCoordinate.x, gridCoordinate.y))
            {
                Debug.Log($"<color=yellow>[INPUT]</color> Tıklanan Hücre: [{gridCoordinate.x}, {gridCoordinate.y}]");
                
                if (BoardOrchestrator.Instance != null)
                {
                    BoardOrchestrator.Instance.OnCellClicked(gridCoordinate.x, gridCoordinate.y);
                }
                else
                {
                    Debug.LogError("<color=red>[InputManager]</color> Sahnede BoardOrchestrator bulunamadı!");
                }
            }
            else
            {
                Debug.Log("<color=grey>[INPUT]</color> Tahta sınırları dışına tıklandı.");
            }
        }
    }

    private void TriggerManualShuffle()
    {
        if (GameManager.Instance != null && !GameManager.Instance.CanPlay()) 
        {
            Debug.LogWarning("<color=yellow>[INPUT]</color> Oyun aktif değil, Shuffle yapılamaz.");
            return;
        }

        if (BoardOrchestrator.Instance != null && BoardOrchestrator.Instance.IsAnimating) 
        {
            Debug.LogWarning("<color=yellow>[INPUT]</color> Animasyon devam ederken Shuffle yapılamaz.");
            return;
        }

        Debug.Log("<color=magenta>[INPUT]</color> Manuel Shuffle Testi Tetiklendi!");
        
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.StartCoroutine(BoardManager.Instance.ShuffleBoardRoutine());
        }
    }
}