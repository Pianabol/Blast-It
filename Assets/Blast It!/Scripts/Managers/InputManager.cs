using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask boardLayer;
    [SerializeField] private float rayDistance = 100f;

    private void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            bool pointerOverUI = false;

            if (EventSystem.current != null)
            {
                pointerOverUI = Input.touchCount > 0
                    ? EventSystem.current.IsPointerOverGameObject(
                        Input.GetTouch(0).fingerId)
                    : EventSystem.current.IsPointerOverGameObject();
            }

            if (!pointerOverUI)
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

                if (PowerUpManager.Instance != null)
                {
                    if (PowerUpManager.Instance.IsUsingPowerUp)
                        return;

                    PowerUpType selected =
                        PowerUpManager.Instance.SelectedPowerUp;

                    if (selected != PowerUpType.None)
                    {
                        PowerUpManager.Instance.TryUseSelectedAt(
                            gridCoordinate.x,
                            gridCoordinate.y
                        );

                        // Never forward PowerUp clicks to normal swapping.
                        return;
                    }
                }

                
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
        if (PowerUpManager.Instance != null)
            PowerUpManager.Instance.UseShuffle();
    }
}