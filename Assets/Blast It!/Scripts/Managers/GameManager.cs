using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    
    private int currentMoves;

    // YENİ: Bekleme (Pending) bayrakları
    private bool isWinPending;
    private bool isLosePending;

    // YENİ: Observer Pattern (Sınıfları birbirinden bağımsızlaştıran yapı)
    private readonly List<IGameStateListener> stateListeners = new List<IGameStateListener>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Oyun ilk açıldığında Ana Menüdeyiz
        ChangeState(GameState.Menu);
    }

    // --- OBSERVER METOTLARI ---
    
    public void RegisterListener(IGameStateListener listener)
    {
        if (!stateListeners.Contains(listener))
        {
            stateListeners.Add(listener);
            // Kayıt olan script'e (Örn: UIManager) o anki durumu anında bildir ki geride kalmasın.
            listener.OnGameStateChanged(CurrentState); 
        }
    }

    public void UnregisterListener(IGameStateListener listener)
    {
        if (stateListeners.Contains(listener))
        {
            stateListeners.Remove(listener);
        }
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        Debug.Log($"<color=cyan>[GameManager]</color> State Changed to: {newState}");

        // Kayıtlı tüm sistemlere (UI, Ses, Efekt vb.) durumu bildir
        foreach (var listener in stateListeners)
        {
            listener.OnGameStateChanged(CurrentState);
        }
    }

    // --- OYUN AKIŞ METOTLARI ---

    public void SetMoves(int moves)
    {
        currentMoves = moves;
        
        // Yeni level başladığında bayrakları sıfırla
        isWinPending = false;
        isLosePending = false;
    }

    public void DecreaseMove()
    {
        if (CurrentState != GameState.Playing) return;

        currentMoves--;
        Debug.Log($"<color=orange>[GameManager]</color> Hamle: {currentMoves}");

        if (currentMoves <= 0)
        {
            // Hamle bitti, bayrağı kaldır ama şelale devam ediyorsa bekle
            isLosePending = true;
        }
    }

    public void WinGame()
    {
        if (CurrentState != GameState.Playing) return;
        
        // Anında state değiştirmek yerine bayrağı kaldır ve durumu değerlendir
        isWinPending = true;
        EvaluateGameEnd(); 
    }

    /// <summary>
    /// Şelale bittiğinde veya hedef vurulduğunda çağrılır.
    /// Kazanma/Kaybetme durumunu (animasyonlar bittiyse) uygular.
    /// </summary>
    public void EvaluateGameEnd()
    {
        // 1. KURAL: Eğer tahtada hala animasyon veya şelale devam ediyorsa BÖLME! (Bekle)
        if (BoardOrchestrator.Instance != null && BoardOrchestrator.Instance.IsAnimating) return;

        // 2. KURAL: Animasyonlar bitti, artık tabloya bakabiliriz.
        // Kazanma durumu HER ZAMAN kaybetmekten (hamle bitmesinden) önceliklidir!
        if (isWinPending)
        {
            ChangeState(GameState.LevelComplete);
            isWinPending = false;
        }
        else if (isLosePending)
        {
            ChangeState(GameState.GameOver);
            isLosePending = false;
        }
    }

    public bool CanPlay()
    {
        return CurrentState == GameState.Playing;
    }
}