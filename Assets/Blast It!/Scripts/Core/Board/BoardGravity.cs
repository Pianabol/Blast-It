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

    /// <summary>
    /// Sütun sütun tarayarak havada kalan taşları aşağı indirir ve tepeden yeni taşlar düşürür.
    /// Orkestra şefi animasyonları bekleyebilsin diye IEnumerator döner.
    /// </summary>
    public IEnumerator ApplyGravityRoutine()
    {
        bool gravityApplied = false;

        // Her sütunu (X) ayrı ayrı aşağıdan yukarı (Y) doğru tarıyoruz
        for (int x = 0; x < BoardManager.Instance.Width; x++)
        {
            int emptyY = 0;     // Hedef boşluğun Y ekseni (Aşağıdan yukarı çıkar)
            int spawnCount = 0; // O sütunda tepeden kaç tane yeni taş düşecek?

            for (int y = 0; y < BoardManager.Instance.Height; y++)
            {
                Cell currentCell = BoardManager.Instance.GetCell(x, y);

                // Eğer engel veya oynanamaz hücre varsa (Şimdilik yok ama altyapı olsun)
                if (!currentCell.IsPlayable)
                {
                    emptyY = y + 1;
                    continue;
                }

                // Eğer hücrede taş varsa ve altında boşluk kalmışsa (y > emptyY)
                if (currentCell.HasItem)
                {
                    if (y > emptyY)
                    {
                        // Taşı yerinden sök ve aşağıdaki boşluğa oturt
                        Item item = currentCell.ExtractItem();
                        Cell targetCell = BoardManager.Instance.GetCell(x, emptyY);
                        
                        targetCell.SetItem(item);
                        item.UpdateCoordinates(x, emptyY);

                        // Fiziksel olarak animasyonla aşağı kaydır
                        Vector3 targetPos = BoardManager.Instance.GridToWorldPosition(x, emptyY);
                        targetPos.y = 0.5f;
                        
                        item.MoveTo(targetPos, dropDuration);
                        gravityApplied = true;
                    }
                    emptyY++;
                }
            }

            // Sütundaki var olan taşlar düştü, en tepede (emptyY'den Height'a kadar) boşluk kaldıysa doldur (Refill)
            for (int y = emptyY; y < BoardManager.Instance.Height; y++)
            {
                Cell targetCell = BoardManager.Instance.GetCell(x, y);
                if (!targetCell.IsPlayable) continue;

                // Havuzdan yeni taş çek ve hücreye yaz
                Item newItem = ItemPool.Instance.GetRandomItem();
                targetCell.SetItem(newItem);
                newItem.UpdateCoordinates(x, y);

                Vector3 targetPos = BoardManager.Instance.GridToWorldPosition(x, y);
                targetPos.y = 0.5f;

                // Taşı görsel olarak tahtanın tepesinden (ekran dışından) başlatıyoruz ki aşağı düşsün
                Vector3 spawnPos = BoardManager.Instance.GridToWorldPosition(x, BoardManager.Instance.Height + spawnCount);
                spawnPos.y = 0.5f;
                newItem.transform.position = spawnPos;

                newItem.MoveTo(targetPos, dropDuration);
                gravityApplied = true;
                spawnCount++;
            }
        }

        // Eğer en az 1 taş bile yer değiştirdiyse animasyon süresi kadar bekle
        if (gravityApplied)
        {
            yield return new WaitForSeconds(dropDuration);
        }
    }
}