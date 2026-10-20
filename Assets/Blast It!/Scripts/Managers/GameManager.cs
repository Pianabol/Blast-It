using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    
    private int currentMoves;

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
        //ChangeState(GameState.Playing); // Level yüklenince otomatik Playing'e geç 
    }

    public void DecreaseMove()
    {
        if (CurrentState != GameState.Playing) return;

        currentMoves--;
        Debug.Log($"<color=orange>[GameManager]</color> Hamle: {currentMoves}");

        if (currentMoves <= 0)
        {
            ChangeState(GameState.GameOver);
        }
    }

    public void WinGame()
    {
        if (CurrentState != GameState.Playing) return;
        ChangeState(GameState.LevelComplete);
    }

    public bool CanPlay()
    {
        return CurrentState == GameState.Playing;
    }
}