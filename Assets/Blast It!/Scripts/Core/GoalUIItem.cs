using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalUIItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    
    // YENİ: Yeşil tik (Checkmark) görseli için referans
    [SerializeField] private GameObject checkmarkObj;

    private GoalType myType;

    public void Setup(GoalType type, int amount, Sprite icon)
    {
        myType = type;
        
        if (icon != null)
        {
            iconImage.sprite = icon;
        }

        // Başlangıç durumu kontrolü
        UpdateVisuals(amount);
    }

    public void PlayUpdateAnimation(int remainingAmount)
    {
        UpdateVisuals(remainingAmount);

        // Hedef daha bitmediyse büyüyüp küçülme (Pop) animasyonu yap
        if (remainingAmount > 0)
        {
            LeanTween.cancel(gameObject);
            transform.localScale = Vector3.one;
            LeanTween.scale(gameObject, Vector3.one * 1.3f, 0.15f).setEaseOutQuad().setOnComplete(() =>
            {
                LeanTween.scale(gameObject, Vector3.one, 0.15f).setEaseInQuad();
            });
        }
        else
        {
            // YENİ: Hedef sıfırlandıysa daha havalı, kutlama tarzı bir pop animasyonu (Tik çıkarken)
            LeanTween.cancel(gameObject);
            transform.localScale = Vector3.one;
            LeanTween.scale(gameObject, Vector3.one * 1.5f, 0.2f).setEaseOutBack().setOnComplete(() =>
            {
                LeanTween.scale(gameObject, Vector3.one, 0.2f).setEaseInBack();
            });
        }
    }

    private void UpdateVisuals(int amount)
    {
        if (amount > 0)
        {
            // Sayı 0'dan büyükse: Sayıyı göster, tiki gizle
            amountText.gameObject.SetActive(true);
            amountText.text = amount.ToString();
            
            if (checkmarkObj != null) checkmarkObj.SetActive(false);
        }
        else
        {
            // Sayı bittiyse (0 veya altı): Sayıyı gizle, tiki göster
            amountText.gameObject.SetActive(false);
            
            if (checkmarkObj != null) checkmarkObj.SetActive(true);
        }
    }

    public void ResetAndSleep()
    {
        LeanTween.cancel(gameObject);
        gameObject.SetActive(false);
    }
}