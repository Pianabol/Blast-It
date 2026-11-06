
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PowerUpButtonPressFX : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("Press Animation")]
    [SerializeField] private float pressedScale = 0.90f;
    [SerializeField] private float pressDuration = 0.08f;
    [SerializeField] private float releaseDuration = 0.18f;

    private Button button;
    private Vector3 originalScale;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.IsActive() || !button.IsInteractable())
            return;

        AnimateScale(
            originalScale * pressedScale,
            pressDuration,
            LeanTweenType.easeOutQuad
        );
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetScale();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetScale();
    }

    private void ResetScale()
    {
        AnimateScale(
            originalScale,
            releaseDuration,
            LeanTweenType.easeOutBack
        );
    }

    private void AnimateScale(
        Vector3 target,
        float duration,
        LeanTweenType ease)
    {
        LeanTween.cancel(gameObject);

        LeanTween.scale(gameObject, target, duration)
            .setEase(ease);
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);
        transform.localScale = originalScale;
    }
}
