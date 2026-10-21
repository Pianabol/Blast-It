using System.Collections.Generic;
using UnityEngine;

public class GoalUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform goalsContainer; 
    [SerializeField] private GoalUIItem goalItemPrefab; 

    [System.Serializable]
    public struct GoalSpriteMap
    {
        public ItemType type;
        public Sprite icon;
    }

    [Header("Visual Assets")]
    public List<GoalSpriteMap> spriteDatabaseList;

    // --- PROFESYONEL VERİ YAPILARI ---
    private Dictionary<ItemType, Sprite> spriteDatabase = new Dictionary<ItemType, Sprite>();
    
    // Aktif olarak ekranda olan hedefleri anında bulmak için (Arama O(1))
    private Dictionary<ItemType, GoalUIItem> activeUIItems = new Dictionary<ItemType, GoalUIItem>();
    
    // Object Pool (Uyuyanlar Listesi)
    private Queue<GoalUIItem> itemPool = new Queue<GoalUIItem>();

    private void Awake()
    {
        // 1. Liste formatındaki database'i oyunda hızlı erişim için Dictionary'e çevir
        foreach (var map in spriteDatabaseList)
        {
            if (!spriteDatabase.ContainsKey(map.type))
            {
                spriteDatabase.Add(map.type, map.icon);
            }
        }
    }

    private void OnEnable()
    {
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.OnGoalsSet += BuildGoalsUI;
            GoalManager.Instance.OnGoalDecreased += UpdateGoalAmount;
        }
    }

    private void Start()
    {
        // Execution Order (Çalışma sırası) güvenliği
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.OnGoalsSet -= BuildGoalsUI; // Çifte kaydı önle
            GoalManager.Instance.OnGoalsSet += BuildGoalsUI;
            
            GoalManager.Instance.OnGoalDecreased -= UpdateGoalAmount;
            GoalManager.Instance.OnGoalDecreased += UpdateGoalAmount;
        }
    }

    private void OnDisable()
    {
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.OnGoalsSet -= BuildGoalsUI;
            GoalManager.Instance.OnGoalDecreased -= UpdateGoalAmount;
        }
    }

    /// <summary>
    /// GoalManager yeni level verisini yolladığında çalışır.
    /// Eski UI'ları uyutup (Pool), gereken kadarını uyandırıp ekrana dizer.
    /// </summary>
    private void BuildGoalsUI(Dictionary<ItemType, int> newGoals)
    {
        // 1. Ekranda aktif olan her şeyi havuza geri at (Destroy YOK!)
        foreach (var activeItem in activeUIItems.Values)
        {
            activeItem.ResetAndSleep();
            itemPool.Enqueue(activeItem);
        }
        activeUIItems.Clear();

        // 2. Yeni hedefler için havuzdan eleman çek veya yetmezse yeni üret
        foreach (var kvp in newGoals)
        {
            ItemType type = kvp.Key;
            int amount = kvp.Value;

            GoalUIItem uiItem = GetItemFromPool();
            
            // Resmi bul
            Sprite icon = spriteDatabase.ContainsKey(type) ? spriteDatabase[type] : null;

            uiItem.Setup(type, amount, icon);
            activeUIItems.Add(type, uiItem);
        }
    }

    private GoalUIItem GetItemFromPool()
    {
        if (itemPool.Count > 0)
        {
            GoalUIItem item = itemPool.Dequeue();
            item.gameObject.SetActive(true);
            return item;
        }

        // Havuz boşsa (ilk level veya çok hedefler varsa) yeni üret
        return Instantiate(goalItemPrefab, goalsContainer);
    }

    private void UpdateGoalAmount(ItemType type, int remainingAmount)
    {
        // Hedef tahtada var mı diye Dictionary'den O(1) hızında sor
        if (activeUIItems.TryGetValue(type, out GoalUIItem uiItem))
        {
            uiItem.PlayUpdateAnimation(remainingAmount);
        }
    }
}