using System;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeController : MonoBehaviour
{
    [SerializeField] private int cost;
    [SerializeField] private GameObject player;
    private PlayerController playerController;
    private int playerCrystalCount;
    private Button upgradeButton;



    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        playerController.OnCrystalCollected += PlayerController_OnCrystalCountChange;
        playerCrystalCount = playerController.GetCrystalCount();
        upgradeButton = GetComponent<Button>();
        if (playerCrystalCount < cost) {
            upgradeButton.interactable = false;
        }
    }

    void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnCrystalCollected -= PlayerController_OnCrystalCountChange;
        }
    }

    private void PlayerController_OnCrystalCountChange(object sender, PlayerController.OnCrystalCollectedEventArgs e)
    {
        playerCrystalCount = e.CrystalCount;
        if (playerCrystalCount < cost) {
            upgradeButton.interactable = false;
        }
    }

    public void PurchaseUpgrade()
    {
        upgradeButton.interactable = false;
        playerController.DecreaseCrystalCount(cost);
    }
}
