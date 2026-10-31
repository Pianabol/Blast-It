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
        // Hedefin mevcut dünya konumunu kaydet.
        Vector3 collectionPosition = transform.position;

        // Toplama işlemini GoalManager'a bildir.
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.CollectGoal(
                goalType,
                collectionPosition
            );
        }

        // Varsa devam eden hareket animasyonunu iptal et.
        LeanTween.cancel(gameObject);

        // Gerçek 3D objeyi havuza geri gönder.
        GoalItemPool.Instance.ReturnItem(this);
    }

}