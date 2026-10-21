using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour, IGameStateListener
{
    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Level Popup Elements")]
    [SerializeField] private GameObject levelPopupContainer;
    [SerializeField] private TextMeshProUGUI levelPopupText;

    private int popupTweenId = -1; // Race Condition'ı ezmek için Tween kimliği

    private void OnEnable()
    {
        // Güvenli kayıt: Eğer GameManager Awake olduysa anında kaydol
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterListener(this);
        }
    }

    private void Start()
    {
        // Garanti kayıt: OnEnable anında GameManager henüz yoksa (Execution Order) burada yakala
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterListener(this);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterListener(this);
        }
    }

    public void OnGameStateChanged(GameState gameState)
    {
        // Panel geçişleri
        if (menuPanel != null) menuPanel.SetActive(gameState == GameState.Menu);
        if (gamePanel != null) gamePanel.SetActive(gameState == GameState.Playing);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(gameState == GameState.LevelComplete);
        if (gameOverPanel != null) gameOverPanel.SetActive(gameState == GameState.GameOver);

        if (gameState == GameState.Playing)
        {
            ShowLevelPopup();
        }
    }

    private void ShowLevelPopup()
    {
        if (levelPopupContainer == null || levelPopupText == null) return;

        levelPopupText.text = "LEVEL STARTED"; 

        // ÖNCEKİ ANIMASYONU İPTAL ET (Eğer hala oynuyorsa)
        if (LeanTween.isTweening(popupTweenId))
        {
            LeanTween.cancel(popupTweenId);
        }
        LeanTween.cancel(levelPopupContainer);

        levelPopupContainer.SetActive(true);
        levelPopupContainer.transform.localScale = Vector3.zero;

        // Büyüme Animasyonu (Bittiğinde kimliği değişkene ata ki takip edebilelim)
        popupTweenId = LeanTween.scale(levelPopupContainer, Vector3.one, 0.4f)
            .setEaseOutBack()
            .setOnComplete(() =>
            {
                // Bekleme ve Küçülme Animasyonu
                popupTweenId = LeanTween.scale(levelPopupContainer, Vector3.zero, 0.3f)
                    .setDelay(1.5f) // 2 saniye yatay maç oyunu için biraz uzun hissettirebilir, 1.5 daha seri
                    .setEaseInBack()
                    .setOnComplete(() =>
                    {
                        levelPopupContainer.SetActive(false);
                    }).id;
            }).id;
    }

    public void Btn_Play()
    {
        // 1. Önce sahneyi ve verileri hazırla (Tahtayı diz, hedefleri ve hamleleri yükle)
        if (LevelManager.Instance != null) 
        {
            LevelManager.Instance.LoadCurrentLevel();
        }

        // 2. Her şey hazır olduğunda State'i Playing'e çek!
        if (GameManager.Instance != null) 
        {
            GameManager.Instance.ChangeState(GameState.Playing);
        }
    }

    public void Btn_NextLevel()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.LoadNextLevel();
    }

    public void Btn_Restart()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.LoadCurrentLevel();
    }

    public void Btn_Home()
    {
        if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.Menu);
        
        if (BoardManager.Instance != null) BoardManager.Instance.ClearBoard(); 
    }
}