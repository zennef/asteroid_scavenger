using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatDisplayController : MonoBehaviour
{
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gameManager;
    [SerializeField] private GameObject pipPrefab;
    [SerializeField] private Transform pipContainer;
    [SerializeField] private Color litColor = Color.white;
    [SerializeField] private Color unlitColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    private PlayerController playerController;
    private GameManager gameManagerScript;
    private int maxDisplayLevel;
    private List<Image> pips = new List<Image>();



    public void Initialize()
    {
        playerController = player.GetComponent<PlayerController>();
        maxDisplayLevel = playerController.GetUpgradeMaxDisplayLevel(upgradeType);

        for (int i = 0; i < maxDisplayLevel; i++)
        {
            GameObject pipInstance = Instantiate(pipPrefab, pipContainer);
            pips.Add(pipInstance.GetComponent<Image>());
        }
        SetLitCount(0);

        playerController.OnUpgradePurchased += PlayerController_OnUpgradePurchased;

        gameManagerScript = gameManager.GetComponent<GameManager>();
        gameManagerScript.OnGameStart += GameManagerScript_OnGameStart;
    }

    private void SetLitCount(int litCount)
    {
        for (int i = 0; i < pips.Count; i++)
        {
            pips[i].color = i < litCount ? litColor : unlitColor;
        }
    }

    private void GameManagerScript_OnGameStart(object sender, EventArgs e)
    {
        SetLitCount(0);
    }

    private void PlayerController_OnUpgradePurchased(object sender, PlayerController.OnUpgradePurchasedArgs e)
    {
        if (e.UpgradeType == upgradeType)
        {
            SetLitCount(e.UpgradeLevel - 1);
        }
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnUpgradePurchased -= PlayerController_OnUpgradePurchased;
        }
        if (gameManagerScript != null)
        {
            gameManagerScript.OnGameStart -= GameManagerScript_OnGameStart;
        }
    }
}
