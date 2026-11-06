
using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform bombIcon;

    [Header("Animation Settings")]
    [SerializeField] private float growDuration = 0.15f;
    [SerializeField] private float shrinkDuration = 0.2f;
    [SerializeField] private float growScale = 1.35f;

    private Vector3 originalScale = Vector3.one;

    private void Awake()
    {
        if (bombIcon != null)
            originalScale = bombIcon.localScale;
    }

    public void Activate()
    {
        if (PowerUpManager.Instance == null)
            return;

        PowerUpManager.Instance.SelectPowerUp(PowerUpType.Bomb);
    }

    public bool TryUseAt(int x, int y)
    {
        PowerUpManager manager = PowerUpManager.Instance;
        BoardOrchestrator board = BoardOrchestrator.Instance;

        if (manager == null || board == null)
            return false;

        if (manager.SelectedPowerUp != PowerUpType.Bomb)
            return false;

        if (!board.CanBombTarget(x, y))
            return false;

        if (!manager.TryBeginUse(PowerUpType.Bomb))
            return false;

        StartCoroutine(ExplodeRoutine(x, y));
        return true;
    }

    private IEnumerator ExplodeRoutine(int x, int y)
    {
        if (bombIcon != null)
        {
            LeanTween.cancel(bombIcon.gameObject);
            bombIcon.localScale = originalScale;

            // Grow.
            LeanTween.scale(
                bombIcon.gameObject,
                originalScale * growScale,
                growDuration
            ).setEaseOutBack();

            yield return new WaitForSeconds(growDuration);

            // Shrink.
            LeanTween.scale(
                bombIcon.gameObject,
                originalScale,
                shrinkDuration
            ).setEaseInOutQuad();

            yield return new WaitForSeconds(shrinkDuration);
        }

        // Destroy 3x3 area and process gravity/cascades.
        yield return StartCoroutine(
            BoardOrchestrator.Instance.ExecuteBomb(x, y)
        );

        // Consume one charge.
        PowerUpManager.Instance.FinishUse(true);
    }

    private void OnDisable()
    {
        if (bombIcon == null)
            return;

        LeanTween.cancel(bombIcon.gameObject);
        bombIcon.localScale = originalScale;
    }
}
