using System;
using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    
    public static CurrencyManager Instance { get; private set; }

    public static event Action<int> OnFameChanged;

    [Header("Currency Settings")]
    [SerializeField] private int currentFame = 1000;

    [Header("UI References")]
    [SerializeField] private TMP_Text fameText;

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

    private void Start()
    {
        UpdateUI();
    }

    public int GetCurrentFame()
    {
        return currentFame;
    }

    public bool HasEnoughFame(int amount)
    {
        return currentFame >= amount;
    }

    public void AddFame(int amount)
    {
        if (amount <= 0) return;

        currentFame += amount;
        UpdateUI();
        OnFameChanged?.Invoke(currentFame);
    }

    public bool TrySpendFame(int amount)
    {
        if (HasEnoughFame(amount))
        {
            currentFame -= amount;
            UpdateUI();
            OnFameChanged?.Invoke(currentFame);
            return true;
        }

        Debug.LogWarning("Not enough Fame to complete transaction!");
        return false;
    }

    private void UpdateUI()
    {
        if (fameText != null)
        {
            fameText.text = currentFame.ToString("N0");
        }
    }
}