using System.Collections;
using System.Collections.Generic; // Listeler için eklendi
using UnityEngine;

public class BoardGravity : MonoBehaviour
{
    public static BoardGravity Instance { get; private set; }

    [Header("Gravity Settings")]
    [SerializeField] private float dropDuration = 0.35f;
    
    [Header("Goal Drop Settings")]
    [Tooltip("Yukarıdan düşen bir nesnenin GoalItem olma ihtimali (0-100)")]
    [Range(0, 100)] [SerializeField] private int goalDropChance = 15; 

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

                if (currentCell.IsEmpty)
                {
                    bool itemFell = false;

                    for (int upperY = y + 1; upperY < BoardManager.Instance.Height; upperY++)
                    {
                        Cell upperCell = BoardManager.Instance.GetCell(x, upperY);
                        if (!upperCell.IsPlayable) continue;

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

                    // =================================================================
                    // YENİ: YUKARIDAN YENİ NESNE ÜRETİMİ (REFILL) - GOAL VE ITEM AYRIMI
                    // =================================================================
                    if (!itemFell)
                    {
                        MonoBehaviour spawnedObject = null;
                        
                        // 1. Şansımızı deniyoruz: Acaba GoalItem düşürmeli miyiz?
                        if (Random.Range(0, 100) < goalDropChance && GoalManager.Instance != null)
                        {
                            // Hangi hedeflerden hala ihtiyacımız var bulalım
                            List<GoalType> neededGoals = new List<GoalType>();
                            foreach (GoalType type in System.Enum.GetValues(typeof(GoalType)))
                            {
                                if (type != GoalType.None && GoalManager.Instance.GetRemainingGoalAmount(type) > 0)
                                {
                                    neededGoals.Add(type);
                                }
                            }

                            // Eğer hala toplanması gereken hedef varsa, onlardan rastgele birini seç
                            if (neededGoals.Count > 0)
                            {
                                GoalType selectedGoalType = neededGoals[Random.Range(0, neededGoals.Count)];
                                GoalItem newGoal = GoalItemPool.Instance.GetItem(selectedGoalType);
                                
                                currentCell.SetGoalItem(newGoal);
                                newGoal.UpdateCoordinates(x, y);
                                spawnedObject = newGoal;
                            }
                        }

                        // 2. Eğer GoalItem düşmediyse (şans tutmadıysa veya hedefler bittiyse), Normal Item düşür
                        if (spawnedObject == null)
                        {
                            Item newItem = ItemPool.Instance.GetRandomItem();
                            currentCell.SetItem(newItem);
                            newItem.UpdateCoordinates(x, y);
                            spawnedObject = newItem;
                        }

                        // Üretilen objeyi (Goal veya Item) yukarıdan sahneye indir
                        Vector3 targetPos = BoardManager.Instance.GridToWorldPosition(x, y);
                        targetPos.y = 0.5f;

                        Vector3 spawnPos = BoardManager.Instance.GridToWorldPosition(x, BoardManager.Instance.Height + spawnCount);
                        spawnPos.y = 0.5f;
                        spawnedObject.transform.position = spawnPos;

                        if (spawnedObject is Item itemObj) itemObj.MoveTo(targetPos, dropDuration);
                        else if (spawnedObject is GoalItem goalObj) goalObj.MoveTo(targetPos, dropDuration);

                        gravityApplied = true;
                        spawnCount++;
                    }
                }
            }
        }

        if (gravityApplied) yield return new WaitForSeconds(dropDuration);
    }
}