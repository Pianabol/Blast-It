using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Camera References")]
    [SerializeField] private Camera mainCamera;

    [Header("Padding Settings")]
    [Tooltip("Tahtanın sol ve sağındaki UI (Hedefler vb.) için ekstra yatay boşluk")]
    [SerializeField] private float paddingX = 4.0f; 
    [Tooltip("Tahtanın alt ve üstündeki boşluk")]
    [SerializeField] private float paddingY = 2.0f; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (mainCamera == null) mainCamera = Camera.main;
    }

    public Ray GetScreenRay(Vector3 screenPosition)
    {
        return mainCamera.ScreenPointToRay(screenPosition);
    }

    /// <summary>
    /// Tahtanın genişlik, yükseklik ve hücre boyutuna göre kameranın Orthographic Size değerini ayarlar.
    /// Cihazın ekran oranına (Aspect Ratio) göre dinamik hesaplama yapar.
    /// </summary>
    public void FrameBoard(int width, int height, float cellSize)
    {
        if (mainCamera == null || !mainCamera.orthographic) return;

        // Tahtanın dünya koordinatlarındaki mutlak (world space) boyutları
        float boardWidth = width * cellSize;
        float boardHeight = height * cellSize;

        // Cihaz ekranının en-boy oranı (Örn: 1080x1920 = ~0.56)
        float screenRatio = (float)Screen.width / (float)Screen.height;
        
        // 1. Ekranın genişliğine göre kameranın alması gereken minimum boyut
        // orthographicSize = Yükseklik / 2'dir. Genişlik için aspect ratio'ya bölüyoruz.
        float requiredSizeWidth = (boardWidth + paddingX) / (2f * screenRatio);

        // 2. Ekranın yüksekliğine göre kameranın alması gereken minimum boyut
        float requiredSizeHeight = (boardHeight + paddingY) / 2f;

        // Tahtanın ve UI'ın asla ekrandan taşmaması için iki değerden BÜYÜK olanı seçiyoruz
        mainCamera.orthographicSize = Mathf.Max(requiredSizeWidth, requiredSizeHeight);
        
        Debug.Log($"<color=cyan>[CameraManager]</color> Kamera ayarlandı. Yeni Orthographic Size: {mainCamera.orthographicSize}");
    }
}