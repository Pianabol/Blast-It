using UnityEngine;

public enum ItemType
{
    None = 0,
    Red,
    Green,
    Blue,
    Yellow,
    Purple
}

public class Item : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private ItemType itemType;

    public int X { get; private set; }
    public int Y { get; private set; }

    public ItemType Type => itemType;

    /// <summary>
    /// Havuzdan çekildiğinde veya ilk yaratıldığında kendini sıfırlar.
    /// (3D objelerin renk/mesh atamalarını prefab seviyesinde farklılaştırabilirsin)
    /// </summary>
    public void Initialize(ItemType newType)
    {
        itemType = newType;
        
        // 3D Obje havuzdan çıkarken ölçeği ve rotasyonu sıfırlanmalı
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
    }

    public void UpdateCoordinates(int newX, int newY)
    {
        X = newX;
        Y = newY;
    }

    public void MoveTo(Vector3 targetPosition, float duration = 0.3f)
    {
        if (duration <= 0f)
        {
            transform.position = targetPosition;
        }
        else
        {
            // Varsa üzerindeki eski hareketi iptal et ve yeni pozisyona yumuşak (EaseInOut) kaydır
            LeanTween.cancel(gameObject);
            LeanTween.move(gameObject, targetPosition, duration).setEaseInOutQuad();
        }
    }
    
}