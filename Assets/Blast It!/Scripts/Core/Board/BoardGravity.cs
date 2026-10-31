using System.Collections;
using UnityEngine;

public class BoardGravity : MonoBehaviour
{
    public static BoardGravity Instance { get; private set; }

    [Header("Gravity Settings")]
    [SerializeField] private float dropDuration = 0.35f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator ApplyGravityRoutine()
    {
        bool gravityApplied = false;

        for (int x = 0; x < BoardManager.Instance.Width; x++)
        {
            int spawnCount = 0;

            for (int y = 0; y < BoardManager.Instance.Height; y++)
            {
                Cell currentCell = BoardManager.Instance.GetCell(x, y);
                if (!currentCell.IsPlayable) continue;

                // Hücre tamamen boşsa (Normal taş da yok hedef de yok)
                if (currentCell.IsEmpty)
                {
                    bool itemFell = false;

                    for (int upperY = y + 1; upperY < BoardManager.Instance.Height; upperY++)
                    {
                        Cell upperCell = BoardManager.Instance.GetCell(x, upperY);
                        if (!upperCell.IsPlayable) continue;

                        // 1. DURUM: Yukarıda normal taş varsa düşür
                        if (upperCell.HasItem)
                        {
                            Item item = upperCell.ExtractItem(); 
                            currentCell.SetItem(item); 
                            item.UpdateCoordinates(x, y);

                            Vector3 targetPos = BoardManager.Instance.GridToWorldPosition(x, y);
                            targetPos.y = 0.5f;

                            item.MoveTo(targetPos, dropDuration);
                            gravityApplied = true;
                            itemFell = true;
                            break; 
                        }
                        // 2. DURUM (YENİ): Yukarıda Kupa/Saat varsa onu düşür
                        else if (upperCell.HasGoalItem)
                        {
                            GoalItem goal = upperCell.ExtractGoalItem();
                            currentCell.SetGoalItem(goal);
                            goal.UpdateCoordinates(x, y);

                            Vector3 targetPos = BoardManager.Instance.GridToWorldPosition(x, y);
                            targetPos.y = 0.5f;

                            goal.MoveTo(targetPos, dropDuration);
                            gravityApplied = true;
                            itemFell = true;
                            break;
                        }
                    }

                    // Yukarıda düşecek hiçbir şey kalmamış, ekranın tepesinden YENİ bir şey üretmeliyiz
                    if (!itemFell)
                    {
                        // TODO: Sıradaki aşamada buraya "GoalManager.GetRemainingGoalAmount()" sorusu 
                        // sorularak % ihtimalle yukarıdan GoalItem (Kupa) düşürme şansı eklenecek.
                        
                        Item newItem = ItemPool.Instance.GetRandomItem();
                        currentCell.SetItem(newItem);
                        newItem.UpdateCoordinates(x, y);

                        Vector3 targetPos = BoardManager.Instance.GridToWorldPosition(x, y);
                        targetPos.y = 0.5f;

                        Vector3 spawnPos = BoardManager.Instance.GridToWorldPosition(x, BoardManager.Instance.Height + spawnCount);
                        spawnPos.y = 0.5f;
                        newItem.transform.position = spawnPos;

                        newItem.MoveTo(targetPos, dropDuration);
                        gravityApplied = true;
                        spawnCount++;
                    }
                }
            }
        }

        if (gravityApplied) yield return new WaitForSeconds(dropDuration);
    }
}