using System.Collections.Generic;
using UnityEngine;

public class ItemPool : MonoBehaviour
{
    // Her yerden kolayca erişmek için Singleton yapıyoruz
    public static ItemPool Instance { get; private set; }

    [Header("Pool Settings")]
    [Tooltip("Her renkten/tipten sahnede üretilecek prefabları buraya ekle")]
    [SerializeField] private Item[] itemPrefabs; 
    [SerializeField] private int initialSizePerType = 20;

    [Header("Hierarchy")]
    [Tooltip("Havuzdaki objelerin duracağı yer. Hierarchy'i temiz tutar.")]
    [SerializeField] private Transform poolRoot; 

    // Objeleri tiplerine (renklerine) göre ayrı ayrı kuyruklarda tutan Sözlük (Dictionary)
    private Dictionary<ItemType, Queue<Item>> poolDictionary;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializePool();
    }

    private void InitializePool()
    {
        poolDictionary = new Dictionary<ItemType, Queue<Item>>();

        // Eğer Inspector'dan poolRoot atanmamışsa, kod ile otomatik oluştur.
        if (poolRoot == null)
        {
            poolRoot = new GameObject("PoolRoot").transform;
            poolRoot.SetParent(transform);
        }

        // Listeye verdiğimiz her bir prefab için ayrı bir havuz oluştur
        foreach (Item prefab in itemPrefabs)
        {
            ItemType type = prefab.Type;
            Queue<Item> objectPool = new Queue<Item>();

            for (int i = 0; i < initialSizePerType; i++)
            {
                Item newObj = Instantiate(prefab, poolRoot);
                newObj.gameObject.SetActive(false); // Başlangıçta hepsi uykuda
                objectPool.Enqueue(newObj);
            }

            poolDictionary.Add(type, objectPool);
        }
        
        Debug.Log("<color=green>[ItemPool]</color> Tüm taşlar havuzda uykuya yatırıldı.");
    }

    /// <summary>
    /// Tahtayı doldururken rastgele renkte bir taş verir.
    /// </summary>
    public Item GetRandomItem()
    {
        if (itemPrefabs.Length == 0) return null;

        int randomIndex = Random.Range(0, itemPrefabs.Length);
        ItemType randomType = itemPrefabs[randomIndex].Type;

        return GetItem(randomType);
    }

    /// <summary>
    /// İstenilen spesifik renkteki (tipteki) taşı uyandırır ve verir.
    /// </summary>
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
            // Eğer inanılmaz bir kombo oldu ve ekrandaki 20 sarı taş da kullanılıyorsa, 21.yi anında üretiriz (Expand).
            Debug.LogWarning($"[ItemPool] {type} havuzu yetersiz kaldı! Yeni obje üretiliyor.");
            Item prefabToSpawn = GetPrefabByType(type);
            itemToReuse = Instantiate(prefabToSpawn, poolRoot);
        }

        // Objeyi uyandır ve ayarlarını sıfırla
        itemToReuse.gameObject.SetActive(true);
        itemToReuse.Initialize(type);
        
        return itemToReuse;
    }

    /// <summary>
    /// Patlayan veya yok olması gereken taşı havuza geri gönderir.
    /// </summary>
    public void ReturnItem(Item item)
    {
        item.gameObject.SetActive(false); // Objeyi kapat
        item.transform.position = poolRoot.position; // Göz önünden çek
        
        // DİKKAT: Burada SetParent(poolRoot) YAPMIYORUZ! 
        // Hierarchy'de nerede duruyorsa orada kalsın, SetParent mobil cihazlarda FPS düşürür.

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