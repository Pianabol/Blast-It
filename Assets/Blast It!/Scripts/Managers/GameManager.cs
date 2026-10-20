using UnityEngine;

public enum GameState
{
    Initializing,
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    
    private int currentMoves;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CurrentState = GameState.Initializing;
    }

    /// <summary>
    /// LevelLoader tarafından bölüm başlarken çağrılır.
    /// </summary>
    public void SetMoves(int moves)
    {
        currentMoves = moves;
        StartGame();
    }

    private void StartGame()
    {
        CurrentState = GameState.Playing;
        Debug.Log($"<color=cyan>[GameManager]</color> Oyun Başladı! Toplam Hamle: {currentMoves}");
    }

    public void DecreaseMove()
    {
        if (CurrentState != GameState.Playing) return;

        currentMoves--;
        Debug.Log($"<color=orange>[GameManager]</color> Hamle yapıldı! Kalan Hamle: {currentMoves}");

        if (currentMoves <= 0)
        {
            CurrentState = GameState.GameOver;
            Debug.Log($"<color=red>[GameManager]</color> Hamleler bitti! Tahtanın durulması bekleniyor...");
        }
    }

    public bool CanPlay()
    {
        return CurrentState == GameState.Playing;
    }
}