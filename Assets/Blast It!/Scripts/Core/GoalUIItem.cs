using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalUIItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private GameObject checkmarkObject; 

    private bool isCompleted = false;

    public void Setup(ItemType type, int amount, Sprite icon)
    {
        iconImage.sprite = icon;
        amountText.text = amount.ToString();
        
        checkmarkObject.SetActive(false);
        amountText.gameObject.SetActive(true);
        isCompleted = false;
        
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Object Pool'a (Havuza) geri dönerken üzerindeki tüm animasyonları durdurur ve temizler.
    /// </summary>
    public void ResetAndSleep()
    {
        LeanTween.cancel(gameObject);
        LeanTween.cancel(checkmarkObject);
        gameObject.SetActive(false);
    }

    public void PlayUpdateAnimation(int remainingAmount)
    {
        if (isCompleted) return; 

        if (remainingAmount <= 0)
        {
            isCompleted = true;
            amountText.text = "0";

            LeanTween.cancel(gameObject);
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;

            // Kendi etrafında 360 derece dönme animasyonu
            LeanTween.rotateAroundLocal(gameObject, Vector3.forward, -360f, 0.5f)
                .setEaseOutBack()
                .setOnComplete(() => 
                {
                    amountText.gameObject.SetActive(false); 
                    checkmarkObject.SetActive(true);        
                    
                    checkmarkObject.transform.localScale = Vector3.zero;
                    LeanTween.scale(checkmarkObject, Vector3.one, 0.3f).setEaseOutBounce();
                });
        }
        else
        {
            amountText.text = remainingAmount.ToString();
            
            LeanTween.cancel(gameObject);
            transform.localScale = Vector3.one;
            LeanTween.scale(gameObject, Vector3.one * 1.15f, 0.15f).setEasePunch();
        }
    }
}