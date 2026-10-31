using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalUIItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private GameObject checkmarkObj;

    public RectTransform IconRect => iconImage.rectTransform;
    public Sprite IconSprite => iconImage.sprite;

    [Header("Animation Settings")]
    [SerializeField] private float flightArrivalDelay = 0.6f;
    [SerializeField] private float spinDuration = 0.45f;

    private GoalType myType;
    private bool isCompleting;

    public void Setup(GoalType type, int amount, Sprite icon)
    {
        LeanTween.cancel(gameObject);

        myType = type;
        isCompleting = false;

        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        if (icon != null)
        {
            iconImage.sprite = icon;
        }

        UpdateVisuals(amount);
    }

    public void PlayUpdateAnimation(int remainingAmount)
    {
        if (remainingAmount > 0)
        {
            PlayNormalUpdate(remainingAmount);
        }
        else if (!isCompleting)
        {
            PlayCompletionAnimation();
        }
    }

    private void PlayNormalUpdate(int amount)
    {
        LeanTween.cancel(gameObject);

        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        UpdateVisuals(amount);

        // Normal collection pop animation.
        LeanTween.scale(
            gameObject,
            Vector3.one * 1.3f,
            0.15f
        )
        .setEaseOutQuad()
        .setOnComplete(() =>
        {
            LeanTween.scale(
                gameObject,
                Vector3.one,
                0.15f
            ).setEaseInQuad();
        });
    }

    private void PlayCompletionAnimation()
    {
        isCompleting = true;

        LeanTween.cancel(gameObject);

        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        // Keep the previous amount visible while
        // the flying icon travels toward this card.
        LeanTween.delayedCall(
            gameObject,
            flightArrivalDelay,
            StartSpin
        );
    }

    private void StartSpin()
    {
        if (!gameObject.activeInHierarchy)
            return;

        amountText.gameObject.SetActive(false);

        if (checkmarkObj != null)
            checkmarkObj.SetActive(false);

        // Rotate a full 360 degrees around the Z axis.
        LeanTween.value(
            gameObject,
            0f,
            360f,
            spinDuration
        )
        .setEase(LeanTweenType.easeInOutCubic)
        .setOnUpdate((float angle) =>
        {
            transform.localRotation =
                Quaternion.Euler(0f, angle , 0f);
        })
        .setOnComplete(() =>
        {
            transform.localRotation = Quaternion.identity;
            ShowCheckmark();
        });
    }

    private void ShowCheckmark()
    {
        if (checkmarkObj != null)
            checkmarkObj.SetActive(true);

        // Small celebration pop after the spin.
        LeanTween.scale(
            gameObject,
            Vector3.one * 1.2f,
            0.15f
        )
        .setEaseOutBack()
        .setOnComplete(() =>
        {
            LeanTween.scale(
                gameObject,
                Vector3.one,
                0.15f
            ).setEaseInOutQuad();
        });
    }

    private void UpdateVisuals(int amount)
    {
        if (amount > 0)
        {
            amountText.gameObject.SetActive(true);
            amountText.text = amount.ToString();

            if (checkmarkObj != null)
                checkmarkObj.SetActive(false);
        }
        else
        {
            amountText.gameObject.SetActive(false);

            if (checkmarkObj != null)
                checkmarkObj.SetActive(true);
        }
    }

    public void ResetAndSleep()
    {
        LeanTween.cancel(gameObject);

        isCompleting = false;

        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);
    }
}