using System;
using UnityEngine;

public class ShieldManager : MonoBehaviour
{
    [SerializeField] private GameObject shield1;
    [SerializeField] private GameObject shield2;
    [SerializeField] private GameObject shield3;
    private ShieldBarController shieldBarController1;
    private ShieldBarController shieldBarController2;
    private ShieldBarController shieldBarController3;
    private ShieldBarController[] shields;
    [SerializeField] private GameObject player;
    private PlayerController playerController;
    private int numberOfShields;
    [SerializeField] private GameObject gameManager;
    private GameManager gameManagerScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shieldBarController1 = shield1.GetComponent<ShieldBarController>();
        shieldBarController2 = shield2.GetComponent<ShieldBarController>();
        shieldBarController3 = shield3.GetComponent<ShieldBarController>();

        shields = new ShieldBarController[]
        {
            shieldBarController1,
            shieldBarController2,
            shieldBarController3
        };

        shield1.SetActive(true);

        playerController = player.GetComponent<PlayerController>();
        numberOfShields = playerController.GetMaxShieldCount();
        playerController.OnShieldDestroyed += PlayerController_OnShieldDestroyed;
        playerController.OnUpgradePurchased += PlayerController_OnUpgradePurchased;
        playerController.OnGamePaused += PlayerController_OnGamePaused;

        shieldBarController1.SetRechargeDuration(playerController.GetShieldRechargeInterval());

        gameManagerScript = gameManager.GetComponent<GameManager>();
        gameManagerScript.OnGameOver += GameManagerScript_OnGameOver;
        gameManagerScript.OnGameStart += GameManagerScript_OnGameStart;
        gameManagerScript.OnLevelEnded += GameManagerScript_OnLevelEnded;
    }

    private void GameManagerScript_OnGameStart(object sender, EventArgs e)
    {
        shield2.SetActive(false);
        shield3.SetActive(false);
    }

    private void GameManagerScript_OnLevelEnded(object sender, EventArgs e)
    {
        ResetShields();
    }

    private void GameManagerScript_OnGameOver(object sender, EventArgs e)
    {
        ResetShields();
    }

    private void ResetShields()
    {
        for (int i = 0; i < numberOfShields; i++)
        {
            shields[i].SetShield(100);
        }
    }

    private void PlayerController_OnGamePaused(object sender, PlayerController.OnGamePausedArgs e)
    {
        for (int i = 0; i < numberOfShields; i++)
        {
            shields[i].togglePause();
        }
    }

    private void PlayerController_OnUpgradePurchased(object sender, PlayerController.OnUpgradePurchasedArgs e)
    {
        if (e.UpgradeName == "Shield Charges")
        {
            numberOfShields++;
            if (numberOfShields == 2)
            {
                shield2.SetActive(true);
                shieldBarController2.SetRechargeDuration(playerController.GetShieldRechargeInterval());
            }
            else if (numberOfShields == 3)
            {
                shield3.SetActive(true);
                shieldBarController3.SetRechargeDuration(playerController.GetShieldRechargeInterval());
            }
        }
        else if(e.UpgradeName == "Shield Recharge Rate")
        {
            float newRate = playerController.GetShieldRechargeInterval();
            for (int i = 0; i < numberOfShields; i++)
            {
                shields[i].SetRechargeDuration(newRate);
            }
        }
    }

    private void PlayerController_OnShieldDestroyed(object sender, PlayerController.OnShieldDestroyedEventArgs e)
    {

        for (int i = 0; i < Mathf.Min(numberOfShields, shields.Length); i++)
        {
            if (shields[i].isOnline)
            {
                shields[i].SetShield(0);
                return;
            }
        }

    }


}
