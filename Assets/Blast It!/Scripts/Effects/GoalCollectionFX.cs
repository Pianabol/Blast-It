
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalCollectionFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform fxLayer;
    [SerializeField] private Image flyImagePrefab;

    [Header("Pool")]
    [SerializeField, Min(1)] private int initialPoolSize = 8;

    [Header("Animation")]
    [SerializeField] private float flightDuration = 0.6f;
    [SerializeField] private float arcHeight = 120f;

    private readonly Queue<Image> imagePool = new();
    private readonly List<Image> activeImages = new();

    private void Awake()
    {
        if (fxLayer == null || flyImagePrefab == null)
        {
            Debug.LogError("[GoalCollectionFX] Missing references!");
            enabled = false;
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            Image image = Instantiate(
                flyImagePrefab, fxLayer, false);

            image.raycastTarget = false;
            image.gameObject.SetActive(false);
            imagePool.Enqueue(image);
        }
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        Subscribe();
    }

    private void Subscribe()
    {
        if (GoalManager.Instance == null) return;

        GoalManager.Instance.OnGoalCollected -= PlayCollectionFX;
        GoalManager.Instance.OnGoalCollected += PlayCollectionFX;

        GoalManager.Instance.OnGoalsSet -= ResetEffects;
        GoalManager.Instance.OnGoalsSet += ResetEffects;
    }

    private void OnDisable()
    {
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.OnGoalCollected -= PlayCollectionFX;
            GoalManager.Instance.OnGoalsSet -= ResetEffects;
        }

        ResetEffects(null);
    }

    private void PlayCollectionFX(GoalType type, Vector3 worldPosition)
    {
        if (imagePool.Count == 0) return;

        if (GoalUIManager.Instance == null ||
            !GoalUIManager.Instance.TryGetGoalIcon(
                type, out RectTransform iconRect,
                out Sprite iconSprite))
            return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Convert 3D world position to screen coordinates.
        Vector3 startScreen = cam.WorldToScreenPoint(worldPosition);

        if (startScreen.z <= 0f) return;

        // UI icon center in screen coordinates.
        Vector3 iconCenter =
            iconRect.TransformPoint(iconRect.rect.center);

        Vector2 endScreen =
            RectTransformUtility.WorldToScreenPoint(null, iconCenter);

        // Convert both positions into FXLayer local space.
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                fxLayer, startScreen, null, out Vector2 start))
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                fxLayer, endScreen, null, out Vector2 end))
            return;

        Image image = imagePool.Dequeue();

        image.sprite = iconSprite;
        image.color = Color.white;
        image.rectTransform.anchoredPosition = start;
        image.rectTransform.localScale = Vector3.one;

        image.gameObject.SetActive(true);
        activeImages.Add(image);

        StartCoroutine(FlyRoutine(image, start, end));
    }

    private IEnumerator FlyRoutine(
        Image image, Vector2 start, Vector2 end)
    {
        float duration = Mathf.Max(0.01f, flightDuration);
        float elapsed = 0f;

        // Curved flight control point.
        Vector2 control =
            (start + end) * 0.5f + Vector2.up * arcHeight;

        RectTransform rect = image.rectTransform;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            // Quadratic Bezier.
            Vector2 a = Vector2.Lerp(start, control, eased);
            Vector2 b = Vector2.Lerp(control, end, eased);

            rect.anchoredPosition = Vector2.Lerp(a, b, eased);

            // Grow first, then shrink toward the icon.
            float scale = t < 0.3f
                ? Mathf.Lerp(1f, 1.2f, t / 0.3f)
                : Mathf.Lerp(1.2f, 0.2f, (t - 0.3f) / 0.7f);

            rect.localScale = Vector3.one * scale;

            elapsed += Time.deltaTime;
            yield return null;
        }

        image.gameObject.SetActive(false);
        activeImages.Remove(image);
        imagePool.Enqueue(image);
    }

    private void ResetEffects(Dictionary<GoalType, int> unused)
    {
        StopAllCoroutines();

        foreach (Image image in activeImages)
        {
            image.gameObject.SetActive(false);
            image.rectTransform.localScale = Vector3.one;
            imagePool.Enqueue(image);
        }

        activeImages.Clear();
    }
}
