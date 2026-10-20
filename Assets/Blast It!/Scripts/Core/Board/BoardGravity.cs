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

    public IEnumerator ApplyGravityRoutine()
    {
        bool gravityApplied = false;

        for (int x = 0; x < BoardManager.Instance.Width; x++)
        {
            int spawnCount = 0; // Tepeden (ekran dışından) kaç tane yeni taş doğacak?

            // Sütunu en aşağıdan (y=0) en tepeye kadar tarıyoruz
            for (int y = 0; y < BoardManager.Instance.Height; y++)
            {
                Cell currentCell = BoardManager.Instance.GetCell(x, y);

                // Eğer hücre Void veya Obstacle ise burayla işimiz yok, es geç
                if (!currentCell.IsPlayable) continue;

                // Oynanabilir bir hücre BOŞ kalmışsa, doldurmamız lazım!
                if (!currentCell.HasItem)
                {
                    bool itemFell = false;

                    // Bu boşluğun üstündeki (upperY) tüm hücreleri tarayıp düşecek taş arıyoruz
                    for (int upperY = y + 1; upperY < BoardManager.Instance.Height; upperY++)
                    {
                        Cell upperCell = BoardManager.Instance.GetCell(x, upperY);

                        // Eğer yukarıdaki hücre Void/Obstacle ise taşı engellemesin, içinden geçip gitsin
                        if (!upperCell.IsPlayable) continue;

                        // Düşecek bir taş bulduk!
                        if (upperCell.HasItem)
                        {
                            Item item = upperCell.ExtractItem(); // Yukarıdan sök
                            currentCell.SetItem(item);           // Boşluğa yerleştir
                            item.UpdateCoordinates(x, y);

                            Vector3 targetPos = BoardManager.Instance.GridToWorldPosition(x, y);
                            targetPos.y = 0.5f;

                            item.MoveTo(targetPos, dropDuration);
                            gravityApplied = true;
                            itemFell = true;
                            break; // Bu boşluk dolduğuna göre, dış döngüdeki bir sonraki boşluğu aramaya geç
                        }
                    }

                    // Yukarıdaki her yere baktık ama düşecek taş bulamadık. 
                    // Demek ki o sütundaki tüm var olan taşlar düştü. Havuzdan YENİ TAŞ çekmeliyiz!
                    if (!itemFell)
                    {
                        Item newItem = ItemPool.Instance.GetRandomItem();
                        currentCell.SetItem(newItem);
                        newItem.UpdateCoordinates(x, y);

                        Vector3 targetPos = BoardManager.Instance.GridToWorldPosition(x, y);
                        targetPos.y = 0.5f;

                        // Taşı ekran dışından (en tepeden) başlatarak düşme efekti veriyoruz
                        Vector3 spawnPos = BoardManager.Instance.GridToWorldPosition(x, BoardManager.Instance.Height + spawnCount);
                        spawnPos.y = 0.5f;
                        newItem.transform.position = spawnPos;

                        newItem.MoveTo(targetPos, dropDuration);
                        gravityApplied = true;
                        spawnCount++;
                    }
                }
            }
        }

        // Eğer en az bir taş hareket ettiyse, animasyon bitene kadar şefi beklet
        if (gravityApplied)
        {
            yield return new WaitForSeconds(dropDuration);
        }
    }
}