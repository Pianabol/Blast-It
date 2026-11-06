
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PowerUpButtonUI : MonoBehaviour
{
    [Header("PowerUp Settings")]
    [SerializeField] private PowerUpType powerUpType;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI amountText;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        Subscribe();
        RefreshUI();
    }

    private void Start()
    {
        // Fallback for initialization order.
        Subscribe();
        RefreshUI();
    }

    private void OnDisable()
    {
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.OnAmountChanged
                -= HandleAmountChanged;
        }
    }

    private void Subscribe()
    {
        if (PowerUpManager.Instance == null)
            return;

        // Prevent duplicate subscriptions.
        PowerUpManager.Instance.OnAmountChanged
            -= HandleAmountChanged;

        PowerUpManager.Instance.OnAmountChanged
            += HandleAmountChanged;
    }

    private void HandleAmountChanged(
        PowerUpType type,
        int remainingAmount)
    {
        if (type != powerUpType)
            return;

        UpdateVisuals(remainingAmount);
    }

    private void RefreshUI()
    {
        if (PowerUpManager.Instance == null)
            return;

        int amount = PowerUpManager.Instance
            .GetAmount(powerUpType);

        UpdateVisuals(amount);
    }

    private void UpdateVisuals(int amount)
    {
        if (amountText != null)
            amountText.text = $"x{amount}";

        button.interactable = amount > 0;
    }
}
