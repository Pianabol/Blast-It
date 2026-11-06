
using System.Collections;
using UnityEngine;

public class Shuffle : MonoBehaviour
{
    [Header("Icon Animation")]
    [SerializeField] private RectTransform shuffleIcon;
    [SerializeField] private float spinDuration = 0.6f;

    private Quaternion originalRotation;
    private int spinTweenId = -1;

    private void Awake()
    {
        if (shuffleIcon != null)
            originalRotation = shuffleIcon.localRotation;
    }

    public void Execute()
    {
        PowerUpManager manager = PowerUpManager.Instance;
        BoardOrchestrator board = BoardOrchestrator.Instance;

        if (manager == null || board == null ||
            BoardManager.Instance == null ||
            MatchDetector.Instance == null)
        {
            Debug.LogWarning("[Shuffle] Required systems are missing.");
            return;
        }

        if (!manager.TryBeginUse(PowerUpType.Shuffle))
            return;

        PlayIconSpin();

        StartCoroutine(ExecuteRoutine());
    }

    private void PlayIconSpin()
    {
        if (shuffleIcon == null)
            return;

        if (spinTweenId != -1 && LeanTween.isTweening(spinTweenId))
            LeanTween.cancel(spinTweenId);

        shuffleIcon.localRotation = originalRotation;

        spinTweenId = LeanTween.value(
            shuffleIcon.gameObject,
            0f,
            360f,
            spinDuration
        )
        .setEase(LeanTweenType.easeInOutCubic)
        .setOnUpdate((float angle) =>
        {
            shuffleIcon.localRotation =
                originalRotation * Quaternion.Euler(0f, 0f, -angle);
        })
        .setOnComplete(() =>
        {
            shuffleIcon.localRotation = originalRotation;
            spinTweenId = -1;
        }).id;
    }

    private IEnumerator ExecuteRoutine()
    {
        yield return StartCoroutine(
            BoardOrchestrator.Instance.ExecuteShuffle()
        );

        PowerUpManager.Instance.FinishUse(true);
    }

    private void OnDisable()
    {
        if (spinTweenId != -1)
        {
            LeanTween.cancel(spinTweenId);
            spinTweenId = -1;
        }

        if (shuffleIcon != null)
            shuffleIcon.localRotation = originalRotation;
    }
}
