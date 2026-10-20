using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Levels")]
    [Tooltip("Oyundaki tüm levelleri sırasıyla buraya ekle (Level 1, Level 2...)")]
    [SerializeField] private LevelData[] allLevels;

    private int currentLevelIndex = 0; // Başlangıçta 0. indeksten (Level 1) başlar

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /*
    private void Start()
    {
        // Oyun başladığında sıradaki leveli yükle
        LoadCurrentLevel();
    }
    */

    public void LoadCurrentLevel()
    {
        if (allLevels != null && allLevels.Length > 0 && currentLevelIndex < allLevels.Length)
        {
            LevelData levelToLoad = allLevels[currentLevelIndex];
            Debug.Log($"<color=cyan>[LevelManager]</color> {levelToLoad.name} (Level {currentLevelIndex + 1}) tahtaya yükleniyor...");

            // 1. BoardManager'a tahtayı çizdir
            if (BoardManager.Instance != null)
            {
                BoardManager.Instance.InitializeBoard(levelToLoad);
            }

            // 2. GoalManager'a hedefleri ilet
            if (GoalManager.Instance != null)
            {
                GoalManager.Instance.SetGoals(levelToLoad.levelGoals);
            }

            // 3. GameManager'a hamleleri ver ve oyunu BAŞLAT (Kilidi aç)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetMoves(levelToLoad.totalMoves);
            }
        }
        else
        {
            Debug.LogError("<color=red>[LevelManager]</color> Yüklenecek level bulunamadı veya liste boş!");
        }
    }

    /// <summary>
    /// GameManager tarafından bölüm başarıyla bittiğinde çağrılabilir.
    /// </summary>
    public void LoadNextLevel()
    {
        currentLevelIndex++;
        
        if (currentLevelIndex < allLevels.Length)
        {
            LoadCurrentLevel();
        }
        else
        {
            Debug.Log("<color=magenta>[LevelManager]</color> OYUN BİTTİ! Tüm leveller tamamlandı.");
            // TODO: İleride başa dönme veya "Yakında Yeni Leveller" ekranı konabilir.
        }
    }
}