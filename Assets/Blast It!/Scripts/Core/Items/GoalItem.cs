using UnityEngine;

public class GoalItem : MonoBehaviour, ICollectible
{
    [SerializeField] private GoalType goalType;
    public GoalType Type => goalType;

    public int X { get; private set; }
    public int Y { get; private set; }

    public void UpdateCoordinates(int x, int y)
    {
        X = x;
        Y = y;
    }

    // Yerçekimi ile düşme animasyonu için
    public void MoveTo(Vector3 targetPosition, float duration)
    {
        LeanTween.cancel(gameObject);
        LeanTween.move(gameObject, targetPosition, duration).setEaseInQuad();
    }

    public void Collect()
    {
        Debug.Log($"<color=orange>[GoalItem]</color> {goalType} toplandı!");
        
        // TODO: UI'a (Sol üstteki hedef tahtasına) uçma animasyonu eklenecek
        // TODO: GoalManager'a hedefin sayısını 1 düşürmesi söylenecek
        
        // Şimdilik toplandığı an direkt havuza geri gönderiyoruz
        GoalItemPool.Instance.ReturnItem(this);
    }
}