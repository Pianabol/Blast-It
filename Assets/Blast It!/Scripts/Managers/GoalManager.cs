using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance { get; private set; }

    // YENİ: UI'ın dinleyeceği yayınlar (Events)
    public event Action<Dictionary<ItemType, int>> OnGoalsSet;
    public event Action<ItemType, int> OnGoalDecreased;

    private Dictionary<ItemType, int> activeGoals = new Dictionary<ItemType, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetGoals(List<LevelGoal> goals)
    {
        activeGoals.Clear();
        foreach (LevelGoal goal in goals)
        {
            if (activeGoals.ContainsKey(goal.TargetItemType))
                activeGoals[goal.TargetItemType] += goal.TargetAmount;
            else
                activeGoals.Add(goal.TargetItemType, goal.TargetAmount);
        }
        
        Debug.Log($"<color=yellow>[GoalManager]</color> Hedefler yüklendi! Toplam {activeGoals.Count} farklı görev var.");
        
        // YENİ: UI sistemine "Hedefler hazır, ekrana diz!" diye bağır
        OnGoalsSet?.Invoke(activeGoals);
    }

    public void DecreaseGoal(ItemType type)
    {
        if (activeGoals.ContainsKey(type) && activeGoals[type] > 0)
        {
            activeGoals[type]--;
            Debug.Log($"<color=green>[GoalManager]</color> Hedef vuruldu! Kalan {type}: {activeGoals[type]}");

            // YENİ: UI sistemine "Bu taştan 1 tane eksilt!" diye bağır
            OnGoalDecreased?.Invoke(type, activeGoals[type]);

            CheckWinCondition();
        }
    }

    private void CheckWinCondition()
    {
        bool allGoalsMet = activeGoals.Values.All(amount => amount <= 0);

        if (allGoalsMet)
        {
            Debug.Log($"<color=magenta>[GoalManager]</color> BÜTÜN HEDEFLER TAMAMLANDI!");
            if (GameManager.Instance != null) GameManager.Instance.WinGame();
        }
    }
}