using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance { get; private set; }

    // UI'ın dinleyeceği yayınlar (Events) artık GoalType (Kupa, Saat vb.) kullanıyor
    public event Action<Dictionary<GoalType, int>> OnGoalsSet;
    public event Action<GoalType, int> OnGoalDecreased;

    private Dictionary<GoalType, int> activeGoals = new Dictionary<GoalType, int>();

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
            // Dictionary'de hedefi GoalType olarak kaydediyoruz
            if (activeGoals.ContainsKey(goal.TargetGoalType))
                activeGoals[goal.TargetGoalType] += goal.TargetAmount;
            else
                activeGoals.Add(goal.TargetGoalType, goal.TargetAmount);
        }
        
        Debug.Log($"<color=yellow>[GoalManager]</color> Hedefler yüklendi! Toplam {activeGoals.Count} farklı görev var.");
        
        // UI sistemine "Hedefler hazır, ekrana diz!" diye bağır
        OnGoalsSet?.Invoke(activeGoals);
    }

    public void DecreaseGoal(GoalType type)
    {
        // Eğer boş bir tip gönderildiyse (None) hiç işleme sokma
        if (type == GoalType.None) return;

        if (activeGoals.ContainsKey(type) && activeGoals[type] > 0)
        {
            activeGoals[type]--;
            Debug.Log($"<color=green>[GoalManager]</color> Hedef vuruldu! Kalan {type}: {activeGoals[type]}");

            // UI sistemine "Bu özel eşyadan 1 tane eksilt!" diye bağır
            OnGoalDecreased?.Invoke(type, activeGoals[type]);

            CheckWinCondition();
        }
    }

    // YENİ: Gravity Refill (Yerçekimi) sisteminin kalan hedefi sorgulaması için
    public int GetRemainingGoalAmount(GoalType type)
    {
        if (activeGoals.ContainsKey(type))
            return activeGoals[type];
            
        return 0;
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