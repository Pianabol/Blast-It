
using System.Collections;
using UnityEngine;

public class Hammer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform hammerIcon;

    [Header("Strike Animation")]
    [SerializeField] private float strikeDuration = 0.13f;
    [SerializeField] private float returnDuration = 0.18f;
    [SerializeField] private float strikeAngle = -25f;
    [SerializeField] private Vector2 strikeOffset = new Vector2(12f, -10f);

    private Vector2 originalPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        if (hammerIcon == null) return;

        originalPosition = hammerIcon.anchoredPosition;
        originalRotation = hammerIcon.localRotation;
    }

    public void Activate()
    {
        PowerUpManager manager = PowerUpManager.Instance;
        if (manager == null) return;

        manager.SelectPowerUp(PowerUpType.Hammer);

        Debug.Log(
            $"[Hammer] Selected: {manager.SelectedPowerUp}"
        );
    }

    public bool TryUseAt(int x, int y)
    {
        PowerUpManager manager = PowerUpManager.Instance;
        BoardOrchestrator board = BoardOrchestrator.Instance;

        if (manager == null || board == null ||
            hammerIcon == null)
            return false;

        // Hammer must be selected.
        if (manager.SelectedPowerUp != PowerUpType.Hammer)
            return false;

        // Invalid cells must not consume a charge.
        if (!board.CanHammerTarget(x, y))
            return false;

        // Lock PowerUps before starting the coroutine.
        if (!manager.TryBeginUse(PowerUpType.Hammer))
            return false;

        StartCoroutine(StrikeRoutine(x, y));
        return true;
    }

    private IEnumerator StrikeRoutine(int x, int y)
    {
        GameObject iconObject = hammerIcon.gameObject;

        LeanTween.cancel(iconObject);

        hammerIcon.anchoredPosition = originalPosition;
        hammerIcon.localRotation = originalRotation;

        // Hammer tilts toward the bottom-right.
        LeanTween.value(iconObject, 0f, 1f, strikeDuration)
            .setEaseInQuad()
            .setOnUpdate((float t) =>
            {
                hammerIcon.anchoredPosition = Vector2.Lerp(
                    originalPosition,
                    originalPosition + strikeOffset,
                    t
                );

                hammerIcon.localRotation =
                    originalRotation *
                    Quaternion.Euler(0f, 0f, strikeAngle * t);
            });

        // Wait until impact.
        yield return new WaitForSeconds(strikeDuration);

        // Return the icon to its resting position.
        LeanTween.value(iconObject, 0f, 1f, returnDuration)
            .setEaseOutBack()
            .setOnUpdate((float t) =>
            {
                hammerIcon.anchoredPosition = Vector2.Lerp(
                    originalPosition + strikeOffset,
                    originalPosition,
                    t
                );

                hammerIcon.localRotation =
                    originalRotation *
                    Quaternion.Euler(
                        0f, 0f,
                        Mathf.Lerp(strikeAngle, 0f, t)
                    );
            });

        // Execute destruction, gravity and cascades.
        yield return StartCoroutine(
            BoardOrchestrator.Instance.ExecuteHammer(x, y)
        );

        // Consume one charge after successful execution.
        PowerUpManager.Instance.FinishUse(true);
    }

    private void OnDisable()
    {
        if (hammerIcon == null) return;

        LeanTween.cancel(hammerIcon.gameObject);

        hammerIcon.anchoredPosition = originalPosition;
        hammerIcon.localRotation = originalRotation;
    }
}
