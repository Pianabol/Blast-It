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
        // Hedefi skordan düşür
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.DecreaseGoal(goalType);
        }

        // Şimdilik anında havuza dönüyor (Sonraki adımda UI'a uçuracağız)
        GoalItemPool.Instance.ReturnItem(this);
    }
}