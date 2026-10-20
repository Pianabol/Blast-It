using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance { get; private set; }

    // Hedefleri takip etmek için Sözlük kullanıyoruz. (Anahtar: Hangi Taş?, Değer: Kaç Tane Kaldı?)
    private Dictionary<ItemType, int> activeGoals = new Dictionary<ItemType, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// LevelLoader tarafından bölüm başlarken çağrılır.
    /// </summary>
    public void SetGoals(List<LevelGoal> goals)
    {
        activeGoals.Clear();
        foreach (LevelGoal goal in goals)
        {
            if (activeGoals.ContainsKey(goal.TargetItemType))
            {
                activeGoals[goal.TargetItemType] += goal.TargetAmount;
            }
            else
            {
                activeGoals.Add(goal.TargetItemType, goal.TargetAmount);
            }
        }
        Debug.Log($"<color=yellow>[GoalManager]</color> Hedefler yüklendi! Toplam {activeGoals.Count} farklı görev var.");
    }

    /// <summary>
    /// BoardOrchestrator bir taşı patlattığında bu metodu tetikler.
    /// </summary>
    public void DecreaseGoal(ItemType type)
    {
        // Eğer patlayan taş hedeflerimiz arasında varsa ve henüz 0'lanmadıysa
        if (activeGoals.ContainsKey(type) && activeGoals[type] > 0)
        {
            activeGoals[type]--;
            Debug.Log($"<color=green>[GoalManager]</color> Hedef vuruldu! Kalan {type}: {activeGoals[type]}");

            CheckWinCondition();
        }
    }

    private void CheckWinCondition()
    {
        // Sözlükteki (Dictionary) TÜM değerler 0 veya altına düştü mü?
        bool allGoalsMet = activeGoals.Values.All(amount => amount <= 0);

        if (allGoalsMet)
        {
            Debug.Log($"<color=magenta>[GoalManager]</color> BÜTÜN HEDEFLER TAMAMLANDI!");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinGame();
            }
        }
    }
}