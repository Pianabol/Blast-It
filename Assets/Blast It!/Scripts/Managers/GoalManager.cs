using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance { get; private set; }

    public event Action<Dictionary<GoalType, int>> OnGoalsSet;
    public event Action<GoalType, int> OnGoalDecreased;
    public event Action<GoalType, Vector3> OnGoalCollected;

    private Dictionary<GoalType, int> activeGoals = new Dictionary<GoalType, int>();
    
    // YENİ: Kota takibi için eklenen sözlükler
    private Dictionary<GoalType, int> totalGoals = new Dictionary<GoalType, int>();
    private Dictionary<GoalType, int> spawnedGoals = new Dictionary<GoalType, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetGoals(List<LevelGoal> goals)
    {
        activeGoals.Clear();
        totalGoals.Clear();
        spawnedGoals.Clear();

        foreach (LevelGoal goal in goals)
        {
            if (activeGoals.ContainsKey(goal.TargetGoalType))
            {
                activeGoals[goal.TargetGoalType] += goal.TargetAmount;
                totalGoals[goal.TargetGoalType] += goal.TargetAmount;
            }
            else
            {
                activeGoals.Add(goal.TargetGoalType, goal.TargetAmount);
                totalGoals.Add(goal.TargetGoalType, goal.TargetAmount);
                spawnedGoals.Add(goal.TargetGoalType, 0); // Başlangıçta 0 adet üretildi
            }
        }
        
        Debug.Log($"<color=yellow>[GoalManager]</color> Hedefler yüklendi! Toplam {activeGoals.Count} farklı görev var.");
        OnGoalsSet?.Invoke(activeGoals);
    }


    public void CollectGoal(GoalType type, Vector3 worldPosition)
    {
        if (type == GoalType.None ||
            !activeGoals.TryGetValue(type, out int remaining) ||
            remaining <= 0)
        {
            return;
        }

        // Visual systems receive the collection information.
        OnGoalCollected?.Invoke(type, worldPosition);

        // Gameplay data is updated independently of the animation.
        DecreaseGoal(type);
    }

    public void DecreaseGoal(GoalType type)
    {
        if (type == GoalType.None) return;

        if (activeGoals.ContainsKey(type) && activeGoals[type] > 0)
        {
            activeGoals[type]--;
            Debug.Log($"<color=green>[GoalManager]</color> Hedef vuruldu! Kalan {type}: {activeGoals[type]}");
            OnGoalDecreased?.Invoke(type, activeGoals[type]);
            CheckWinCondition();
        }
    }

    // YENİ: Editörden veya Havadan her Kupa/Saat üretildiğinde sayacı 1 artırır
    public void NotifyGoalSpawned(GoalType type)
    {
        if (spawnedGoals.ContainsKey(type))
        {
            spawnedGoals[type]++;
            Debug.Log($"<color=blue>[GoalManager]</color> {type} Spawnlandı. Kota: {spawnedGoals[type]}/{totalGoals[type]}");
        }
    }

    // Yerçekimi yeni bir hedef düşürmeden önce "Kota doldu mu veya Hedef bitti mi?" diye sorar
    public bool CanSpawnGoal(GoalType type)
    {
        // 1. ZORLUK KİLİDİ: Hedef zaten sıfırlanmışsa (UI'da tiklenmişse) ASLA üretme
        if (activeGoals.ContainsKey(type) && activeGoals[type] <= 0) return false;

        // 2. KOTA KİLİDİ: Üretim kotası dolmuşsa üretme
        if (totalGoals.ContainsKey(type) && spawnedGoals.ContainsKey(type))
        {
            return spawnedGoals[type] < totalGoals[type]; 
        }
        return false;
    }

    public int GetRemainingGoalAmount(GoalType type)
    {
        if (activeGoals.ContainsKey(type)) return activeGoals[type];
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