using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationButtonController : MonoBehaviour
{
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private GameObject player;
    [SerializeField] private int cost;
    [SerializeField] private GameObject upgradePrefab;
    private PlayerController playerController;
    private int playerCrystalCount;
    private Button button;
    private bool _isPurchased = false;
    [SerializeField] private GameObject selfPrefab;
    [SerializeField] private bool isLegendary;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        playerController = player.GetComponent<PlayerController>();
        playerController.OnUpgradePurchased += PlayerController_OnUpgradePurchased;
        playerController.OnCrystalCollected += PlayerController_OnCrystalCountChange;
        playerCrystalCount = playerController.GetCrystalCount();

        if (playerCrystalCount < cost)
        {
            button.interactable = false;
        }
    }

    private void PlayerController_OnCrystalCountChange(object sender, PlayerController.OnCrystalCollectedEventArgs e)
    {
        if (_isPurchased) return;
        playerCrystalCount = e.CrystalCount;
        if (playerCrystalCount < cost)
        {
            button.interactable = false;
        }
    }

    private void PlayerController_OnUpgradePurchased(object sender, PlayerController.OnUpgradePurchasedArgs e)
    {
        if (e.UpgradeType == upgradeType)
        {
            _isPurchased = true;
            button.interactable = false;
        }
    }

    public int GetCost()
    {
        return cost;
    }

    public GameObject GetUpgradePrefab()
    {
        return upgradePrefab;
    }

    void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnUpgradePurchased -= PlayerController_OnUpgradePurchased;
            playerController.OnCrystalCollected -= PlayerController_OnCrystalCountChange;
        }
    }

    public GameObject GetSelfPrefab() { return selfPrefab; }
    public bool GetIsLegendary() { return isLegendary; }
}
