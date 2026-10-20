public enum GameState
{
    Menu,
    Playing,
    LevelComplete,
    GameOver
}

public interface IGameStateListener
{
    // UIManager ve diğer sistemler bu fonksiyon üzerinden state değişimlerini dinleyecek
    void OnGameStateChanged(GameState newState);
}