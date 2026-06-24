using System;
using TMPro;
using UnityEngine;

public class StatDisplayController : MonoBehaviour
{
    [SerializeField] private string upgradeName;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gameManager;
    private PlayerController playerController;
    private GameManager gameManagerScript;
    private int maxDisplayLevel;



    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        maxDisplayLevel = playerController.GetUpgradeMaxDisplayLevel(upgradeName);
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
        if (e.UpgradeName == upgradeName)
        {
            valueText.text = (e.UpgradeLevel - 1) + " / " + maxDisplayLevel;
        }
    }

    private void PlayerController_OnUpgradeMaxedOut(object sender, PlayerController.OnUpgradeMaxedOutArgs e)
    {
        if (e.UpgradeName == upgradeName)
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
