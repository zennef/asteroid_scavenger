using System;
using TMPro;
using UnityEngine;

public class StatDisplayController : MonoBehaviour
{
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gameManager;
    private PlayerController playerController;
    private GameManager gameManagerScript;
    private int maxDisplayLevel;



    public void Initialize()
    {
        playerController = player.GetComponent<PlayerController>();
        maxDisplayLevel = playerController.GetUpgradeMaxDisplayLevel(upgradeType);
        valueText.text = "0 / " + maxDisplayLevel;
        playerController.OnUpgradePurchased += PlayerController_OnUpgradePurchased;
        playerController.OnUpgradeMaxedOut += PlayerController_OnUpgradeMaxedOut;

        gameManagerScript = gameManager.GetComponent<GameManager>();
        gameManagerScript.OnGameStart += GameManagerScript_OnGameStart;
    }

    private void GameManagerScript_OnGameStart(object sender, EventArgs e)
    {
        valueText.text = "0 / " + maxDisplayLevel;
    }

    private void PlayerController_OnUpgradePurchased(object sender, PlayerController.OnUpgradePurchasedArgs e)
    {
        if (e.UpgradeType == upgradeType)
        {
            valueText.text = (e.UpgradeLevel - 1) + " / " + maxDisplayLevel;
        }
    }

    private void PlayerController_OnUpgradeMaxedOut(object sender, PlayerController.OnUpgradeMaxedOutArgs e)
    {
        if (e.UpgradeType == upgradeType)
        {
            valueText.text = "MAXED";
        }
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnUpgradePurchased -= PlayerController_OnUpgradePurchased;
            playerController.OnUpgradeMaxedOut -= PlayerController_OnUpgradeMaxedOut;
        }
        if (gameManagerScript != null)
        {
            gameManagerScript.OnGameStart -= GameManagerScript_OnGameStart;
        }
    }
}
