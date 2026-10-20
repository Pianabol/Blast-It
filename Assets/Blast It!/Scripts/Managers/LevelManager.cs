using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Data")]
    [Tooltip("Test etmek istediğin LevelData dosyasını buraya sürükle")]
    [SerializeField] private LevelData currentLevel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Oyun başlar başlamaz atanan leveli yükle
        LoadLevel();
    }

    public void LoadLevel()
    {
        if (currentLevel != null)
        {
            Debug.Log($"<color=cyan>[LevelManager]</color> {currentLevel.name} tahtaya yükleniyor...");
            
            // Şimdilik GoalManager ve GameManager iletişimleri bilerek YOK.
            // Sadece tahtayı inşa etmesi için BoardManager'ı tetikliyoruz.
            if (BoardManager.Instance != null)
            {
                BoardManager.Instance.InitializeBoard(currentLevel);
            }
            else
            {
                Debug.LogError("<color=red>[LevelManager]</color> Sahnede BoardManager bulunamadı!");
            }

            // 2. YENİ AÇTIĞIMIZ KİLİT: GameManager'a hamleleri ver ve oyunu BAŞLAT (Playing state'ine geçir)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetMoves(currentLevel.totalMoves);
            }
        }
        else
        {
            Debug.LogWarning("<color=yellow>[LevelManager]</color> Inspector'dan bir LevelData dosyası atamadın!");
        }
    }
}