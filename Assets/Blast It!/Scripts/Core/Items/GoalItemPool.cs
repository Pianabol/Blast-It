using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GoalPoolSetup
{
    public GoalType type;
    public GoalItem prefab;
    public int initialSize = 10;
}

public class GoalItemPool : MonoBehaviour
{
    public static GoalItemPool Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private List<GoalPoolSetup> poolSetups;
    [SerializeField] private Transform poolRoot;

    private Dictionary<GoalType, Queue<GoalItem>> poolDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else Destroy(gameObject);
    }

    private void InitializePool()
    {
        poolDictionary = new Dictionary<GoalType, Queue<GoalItem>>();

        foreach (var setup in poolSetups)
        {
            Queue<GoalItem> objectPool = new Queue<GoalItem>();

            for (int i = 0; i < setup.initialSize; i++)
            {
                GoalItem newObj = Instantiate(setup.prefab, poolRoot);
                newObj.gameObject.SetActive(false);
                objectPool.Enqueue(newObj);
            }

            poolDictionary.Add(setup.type, objectPool);
        }
    }

    public GoalItem GetItem(GoalType type)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogError($"<color=red>[GoalItemPool]</color> Havuzda {type} bulunamadı!");
            return null;
        }

        // Eğer havuz bittiyse anlık olarak genişlet (Expand)
        if (poolDictionary[type].Count == 0)
        {
            GoalPoolSetup setup = poolSetups.Find(s => s.type == type);
            GoalItem newObj = Instantiate(setup.prefab, poolRoot);
            newObj.gameObject.SetActive(false);
            poolDictionary[type].Enqueue(newObj);
        }

        GoalItem itemToSpawn = poolDictionary[type].Dequeue();
        itemToSpawn.gameObject.SetActive(true);
        itemToSpawn.transform.localScale = Vector3.one; // Önceki animasyonlardan kalan scale'i sıfırla
        return itemToSpawn;
    }

    public void ReturnItem(GoalItem item)
    {
        item.gameObject.SetActive(false);
        poolDictionary[item.Type].Enqueue(item);
    }
}