
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Levels")]
    [SerializeField] private LevelData[] allLevels;

    private int currentLevelIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadCurrentLevel()
    {
        if (allLevels == null ||
            allLevels.Length == 0 ||
            currentLevelIndex >= allLevels.Length)
        {
            Debug.LogError("[LevelManager] Invalid level!");
            return;
        }

        LevelData level = allLevels[currentLevelIndex];

        // 1. Prepare goal data before spawning objects.
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.SetGoals(level.levelGoals);
        }

        // 2. Create the board and spawn its objects.
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.InitializeBoard(level);
        }

        // 3. Initialize gameplay move count.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMoves(level.totalMoves);
        }
    }

    public void LoadNextLevel()
    {
        currentLevelIndex++;

        if (allLevels != null &&
            currentLevelIndex < allLevels.Length)
        {
            LoadCurrentLevel();
        }
        else
        {
            Debug.Log("[LevelManager] All levels completed.");
        }
    }
}
