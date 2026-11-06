
using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [Header("Starting Amounts")]
    [SerializeField, Min(0)] private int shuffleAmount = 2;
    [SerializeField, Min(0)] private int hammerAmount = 2;
    [SerializeField, Min(0)] private int bombAmount = 2;

    [Header("PowerUp References")]
    [SerializeField] private Shuffle shufflePowerUp;
    [SerializeField] private Hammer hammerPowerUp;
    [SerializeField] private Bomb bombPowerUp;

    public PowerUpType SelectedPowerUp { get; private set; }
        = PowerUpType.None;

    public bool IsUsingPowerUp { get; private set; }

    public event Action<PowerUpType, int> OnAmountChanged;
    public event Action<PowerUpType> OnSelectionChanged;

    private readonly Dictionary<PowerUpType, int> amounts = new();

    private PowerUpType executingPowerUp = PowerUpType.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        amounts[PowerUpType.Shuffle] = shuffleAmount;
        amounts[PowerUpType.Hammer] = hammerAmount;
        amounts[PowerUpType.Bomb] = bombAmount;
    }

    //terso
    
    public bool TryUseSelectedAt(int x, int y)
    {
        if (IsUsingPowerUp)
            return false;

        switch (SelectedPowerUp)
        {
            case PowerUpType.Hammer:
                return hammerPowerUp != null &&
                    hammerPowerUp.TryUseAt(x, y);

            case PowerUpType.Bomb:
                return bombPowerUp != null &&
                    bombPowerUp.TryUseAt(x, y);

            default:
                return false;
        }
    }



    public void UseShuffle()
    {
        if (shufflePowerUp != null)
            shufflePowerUp.Execute();
    }

    public void UseHammer()
    {
        if (hammerPowerUp != null)
            hammerPowerUp.Activate();
    }

    public void UseBomb()
    {
        if (bombPowerUp != null)
            bombPowerUp.Activate();
    }
    public int GetAmount(PowerUpType type)
    {
        return amounts.TryGetValue(type, out int amount)
            ? amount
            : 0;
    }

    public bool CanUse(PowerUpType type)
    {
        if (IsUsingPowerUp) return false;

        if (GameManager.Instance == null ||
            !GameManager.Instance.CanPlay())
            return false;

        if (BoardOrchestrator.Instance == null ||
            BoardOrchestrator.Instance.IsAnimating)
            return false;

        return GetAmount(type) > 0;
    }

    public void SelectPowerUp(PowerUpType type)
    {
        if (type != PowerUpType.Hammer &&
            type != PowerUpType.Bomb)
            return;

        if (SelectedPowerUp == type)
        {
            CancelSelection();
            return;
        }

        if (!CanUse(type)) return;

        // Clear previously selected board item.
        BoardOrchestrator.Instance.CancelCurrentSelection();

        SelectedPowerUp = type;
        OnSelectionChanged?.Invoke(type);
    }

    public void CancelSelection()
    {
        if (SelectedPowerUp == PowerUpType.None) return;

        SelectedPowerUp = PowerUpType.None;
        OnSelectionChanged?.Invoke(PowerUpType.None);
    }

    public bool TryBeginUse(PowerUpType type)
    {
        if (!CanUse(type)) return false;

        IsUsingPowerUp = true;
        executingPowerUp = type;

        CancelSelection();

        return true;
    }

    public void FinishUse(bool success)
    {
        if (!IsUsingPowerUp) return;

        if (success && amounts[executingPowerUp] > 0)
        {
            amounts[executingPowerUp]--;

            OnAmountChanged?.Invoke(
                executingPowerUp,
                amounts[executingPowerUp]
            );
        }

        executingPowerUp = PowerUpType.None;
        IsUsingPowerUp = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
