using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalUIItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    // ESKİ: private ItemType myType;
    private GoalType myType; // YENİ: Artık GoalType kullanıyor

    // ESKİ: public void Setup(ItemType type, int amount, Sprite icon)
    public void Setup(GoalType type, int amount, Sprite icon)
    {
        myType = type;
        
        amountText.text = amount.ToString();
        
        if (icon != null)
        {
            iconImage.sprite = icon;
        }
    }

    public void PlayUpdateAnimation(int remainingAmount)
    {
        amountText.text = remainingAmount.ToString();

        // Basit Pop (Büyüyüp küçülme) animasyonu
        LeanTween.cancel(gameObject);
        transform.localScale = Vector3.one;
        LeanTween.scale(gameObject, Vector3.one * 1.3f, 0.15f).setEaseOutQuad().setOnComplete(() =>
        {
            LeanTween.scale(gameObject, Vector3.one, 0.15f).setEaseInQuad();
        });
    }

    public void ResetAndSleep()
    {
        LeanTween.cancel(gameObject);
        gameObject.SetActive(false);
    }
}