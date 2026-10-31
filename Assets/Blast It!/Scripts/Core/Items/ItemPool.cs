using System.Collections.Generic;
using UnityEngine;

public class ItemPool : MonoBehaviour
{
    public static ItemPool Instance { get; private set; }

    [Header("Pool Settings")]
    [Tooltip("Her renkten/tipten sahnede üretilecek prefabları buraya ekle")]
    [SerializeField] private Item[] itemPrefabs; 
    [SerializeField] private int initialSizePerType = 20;

    [Header("Hierarchy")]
    [Tooltip("Havuzdaki objelerin duracağı yer. Hierarchy'i temiz tutar.")]
    [SerializeField] private Transform poolRoot; 

    private Dictionary<ItemType, Queue<Item>> poolDictionary;
    
    // YENİ: Bu levelda düşmesine izin verilen renklerin listesi
    private List<ItemType> allowedItemTypes = new List<ItemType>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializePool();
    }

    private void InitializePool()
    {
        poolDictionary = new Dictionary<ItemType, Queue<Item>>();

        if (poolRoot == null)
        {
            poolRoot = new GameObject("PoolRoot").transform;
            poolRoot.SetParent(transform);
        }

        foreach (Item prefab in itemPrefabs)
        {
            ItemType type = prefab.Type;
            Queue<Item> objectPool = new Queue<Item>();

            for (int i = 0; i < initialSizePerType; i++)
            {
                Item newObj = Instantiate(prefab, poolRoot);
                newObj.gameObject.SetActive(false); 
                objectPool.Enqueue(newObj);
            }

            poolDictionary.Add(type, objectPool);
        }
        
        Debug.Log("<color=green>[ItemPool]</color> Tüm taşlar havuzda uykuya yatırıldı.");
    }

    // YENİ: BoardManager oyun başlarken bu metodu çağırıp izin verilen renkleri belirler
    public void SetAllowedItems(List<ItemType> newAllowedItems)
    {
        allowedItemTypes = newAllowedItems;
        Debug.Log($"<color=green>[ItemPool]</color> Havuz filtrelendi. Bu levelda {allowedItemTypes.Count} çeşit renk kullanılacak.");
    }

    /// <summary>
    /// Tahtayı doldururken SADECE İZİN VERİLEN (Allowed) renklerden rastgele bir taş verir.
    /// </summary>
    public Item GetRandomItem()
    {
        if (itemPrefabs.Length == 0) return null;

        // YENİ: Eğer liste boşsa (tasarımcı hata yaptıysa) güvenlik olarak eski sisteme dönüp tüm renklerden birini seç.
        if (allowedItemTypes == null || allowedItemTypes.Count == 0)
        {
            Debug.LogWarning("[ItemPool] Allowed Items listesi boş! Havuzdan kısıtlamasız rastgele taş çekiliyor.");
            int randomIndex = Random.Range(0, itemPrefabs.Length);
            return GetItem(itemPrefabs[randomIndex].Type);
        }

        // Tasarımcının belirlediği listeden rastgele bir renk seç
        ItemType randomType = allowedItemTypes[Random.Range(0, allowedItemTypes.Count)];
        return GetItem(randomType);
    }

    public Item GetItem(ItemType type)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogError($"[ItemPool] Havuzda {type} tipi bulunamadı! Prefab'ı ekledin mi?");
            return null;
        }

        Queue<Item> pool = poolDictionary[type];
        Item itemToReuse;

        if (pool.Count > 0)
        {
            itemToReuse = pool.Dequeue();
        }
        else
        {
            Debug.LogWarning($"[ItemPool] {type} havuzu yetersiz kaldı! Yeni obje üretiliyor.");
            Item prefabToSpawn = GetPrefabByType(type);
            itemToReuse = Instantiate(prefabToSpawn, poolRoot);
        }

        itemToReuse.gameObject.SetActive(true);
        itemToReuse.Initialize(type);
        
        return itemToReuse;
    }

    public void ReturnItem(Item item)
    {
        item.gameObject.SetActive(false); 
        item.transform.position = poolRoot.position; 

        if (poolDictionary.ContainsKey(item.Type))
        {
            poolDictionary[item.Type].Enqueue(item);
        }
        else
        {
            Destroy(item.gameObject);
        }
    }

    private Item GetPrefabByType(ItemType type)
    {
        foreach (Item prefab in itemPrefabs)
        {
            if (prefab.Type == type) return prefab;
        }
        return null;
    }
}