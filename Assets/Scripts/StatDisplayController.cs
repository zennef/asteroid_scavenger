using System;
using TMPro;
using UnityEngine;

public class StatDisplayController : MonoBehaviour
{
    [SerializeField] private string upgradeName;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gameManager;
    [SerializeField] private bool isLegendary;
    private PlayerController playerController;
    private GameManager gameManagerScript;



    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        playerController.OnUpgradePurchased += PlayerController_OnUpgradePurchased;

        gameManagerScript = gameManager.GetComponent<GameManager>();
        gameManagerScript.OnGameStart += GameManagerScript_OnGameStart;
    }

    private void GameManagerScript_OnGameStart(object sender, EventArgs e)
    {
        valueText.text = isLegendary ? "0" : "1";
    }

    private void PlayerController_OnUpgradePurchased(object sender, PlayerController.OnUpgradePurchasedArgs e)
    {
if (e.UpgradeName == upgradeName)
        {
            valueText.text = isLegendary ? (e.UpgradeLevel - 1).ToString() : e.UpgradeLevel.ToString();
        }
    }

}
