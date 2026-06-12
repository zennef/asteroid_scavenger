using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hudCrystalText;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gameManager;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private GameObject shop;
    [SerializeField] private TextMeshProUGUI shopCrystalText;
    [SerializeField] private GameObject menu;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI youWinText;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject levelComplete;
    [SerializeField] private GameObject returnButton;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject pauseMenuPauseTitle;
    [SerializeField] private GameObject pauseMenuYouWinTitle;
    [SerializeField] private GameObject pauseMenuGameOverTitle;
    [SerializeField] private GameObject playerStats;
    [SerializeField] private GameObject menuBackground;
    [SerializeField] private TextMeshProUGUI shieldLabelText;
    [SerializeField] private TextMeshProUGUI maxFuelCapacityText;
    [SerializeField] private TextMeshProUGUI fuelEfficiencyText;
    [SerializeField] private GameObject asteroidShieldImage;

    [SerializeField] private List<GameObject> commonUpgrades;
    [SerializeField] private List<GameObject> legendaryUpgrades;
    [SerializeField] private Transform[] upgradeSpawnPositions;
    [SerializeField] private List<GameObject> availableCommonUpgrades;
    [SerializeField] private List<GameObject> availableLegendaryUpgrades;

    [SerializeField] private TextMeshProUGUI keyFuelValueText;
    [SerializeField] private TextMeshProUGUI keyCrystalValueText;
    [SerializeField] private TextMeshProUGUI keyWallValueText;
    [SerializeField] private TextMeshProUGUI keyRockValueText;
    [SerializeField] private GameObject hideCrystalFuelCellValue;

    // Animation settings � tweak these in one place
    private const float SLIDE_DISTANCE = 40f;
    private const float SLIDE_DURATION = 0.3f;
    private const float FADE_DURATION = 0.2f;
    private const float HIDE_DURATION = 0.15f;

    private PlayerController playerController;
    private GameManager gameManagerScript;


    void Start()
    {
        ResetAvailableUpgradeLists();

        gameManagerScript = gameManager.GetComponent<GameManager>();
        gameManagerScript.OnCurrentLevelIncrease += GameManagerScript_OnCurrentLevelIncrease;
        gameManagerScript.OnGameStart += GameManagerScript_OnGameStart;
        gameManagerScript.OnGameOver += GameManagerScript_OnGameOver;
        gameManagerScript.OnYouWin += GameManagerScript_OnYouWin;

        playerController = player.GetComponent<PlayerController>();
        playerController.OnCrystalCollected += PlayerController_OnCrystalCollected;
        playerController.OnUpgradeMaxedOut += PlayerController_OnUpgradeMaxedOut;
        playerController.OnUpgradePurchased += PlayerController_OnUpgradePurchased;
        playerController.OnGamePaused += PlayerController_OnGamePaused;

        // Initialize panel states without animation on start
        menuBackground.SetActive(true);
        EnsureCanvasGroup(menuBackground);

        menu.SetActive(true);
        EnsureCanvasGroup(menu);
        
        shop.SetActive(false);
        EnsureCanvasGroup(shop);

        playerStats.SetActive(false);
        EnsureCanvasGroup(playerStats);

        pauseMenu.SetActive(false);
        EnsureCanvasGroup(pauseMenu);

        EnsureCanvasGroup(levelComplete);
    }

    // ---------------------------------------------------------------------------
    // Animation helpers
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Ensures a CanvasGroup exists on the panel. Safe to call repeatedly.
    /// </summary>
    private CanvasGroup EnsureCanvasGroup(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        return cg;
    }

    /// <summary>
    /// Slides and fades a panel in. slideFromOffset is the starting offset
    /// relative to the panel's resting position (e.g. new Vector2(0, -SLIDE_DISTANCE)).
    /// </summary>
    private void ShowPanel(GameObject panel, Vector2 slideFromOffset, float delay = 0f)
    {
        panel.SetActive(true);

        RectTransform rt = panel.GetComponent<RectTransform>();
        CanvasGroup cg = EnsureCanvasGroup(panel);

        // Kill any in-progress tweens on this panel
        DOTween.Kill(rt);
        DOTween.Kill(cg);

        Vector2 restingPos = rt.anchoredPosition;
        rt.anchoredPosition = restingPos + slideFromOffset;
        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        if (delay > 0f) seq.AppendInterval(delay);
        seq.Append(rt.DOAnchorPos(restingPos, SLIDE_DURATION).SetEase(Ease.OutCubic));
        seq.Join(cg.DOFade(1f, FADE_DURATION));
        seq.SetUpdate(true); // Run during Time.timeScale = 0 (pause menu)
    }


    private void HidePanel(GameObject panel, Vector2 slideToOffset)
    {
        if (!panel.activeSelf) return;

        RectTransform rt = panel.GetComponent<RectTransform>();
        CanvasGroup cg = EnsureCanvasGroup(panel);

        DOTween.Kill(rt);
        DOTween.Kill(cg);

        Vector2 restingPos = rt.anchoredPosition;

        Sequence seq = DOTween.Sequence();
        seq.Append(cg.DOFade(0f, HIDE_DURATION));
        seq.Join(rt.DOAnchorPos(restingPos + slideToOffset, HIDE_DURATION).SetEase(Ease.InCubic));
        seq.OnComplete(() =>
        {
            panel.SetActive(false);
            rt.anchoredPosition = restingPos; // Reset so next ShowPanel starts clean
        });
        seq.SetUpdate(true);
    }

    private void ShowPanelInstant(GameObject panel)
    {
        CanvasGroup cg = EnsureCanvasGroup(panel);
        cg.alpha = 1f;
        panel.SetActive(true);
    }

    public void HideMenuBackgroundInstant()
    {
        menuBackground.SetActive(false);
    }

    // ---------------------------------------------------------------------------
    // Event handlers
    // ---------------------------------------------------------------------------

    private void GameManagerScript_OnGameOver(object sender, EventArgs e)
    {
        // console log for testing purposes to confirm event is firing at correct time
        Debug.Log("Game Over event fired");

        GameOver();
    }

    private void GameManagerScript_OnYouWin(object sender, EventArgs e)
    {
        YouWin();
    }

    private void PlayerController_OnGamePaused(object sender, PlayerController.OnGamePausedArgs e)
    {
        if (e.IsGamePaused)
        {
            ShowPanel(pauseMenu, new Vector2(0, -SLIDE_DISTANCE));
            ShowPanel(playerStats, new Vector2(0, -SLIDE_DISTANCE), delay: 0.05f);
        }
        else
        {
            HidePanel(pauseMenu, new Vector2(0, -SLIDE_DISTANCE));
            HidePanel(playerStats, new Vector2(0, -SLIDE_DISTANCE));
        }
    }

    private void GameManagerScript_OnGameStart(object sender, EventArgs e)
    {
        ResetHUDKeyValues();
        ResetAvailableUpgradeLists();
    }

    private void PlayerController_OnUpgradePurchased(object sender, PlayerController.OnUpgradePurchasedArgs e)
    {
        if (e.UpgradeName == "Fuel Cell")
        {
            keyFuelValueText.text = "+" + playerController.GetFuelCellAmount().ToString();
            keyCrystalValueText.text = "+" + playerController.GetFuelCellAmount().ToString();
        }
        else if (e.UpgradeName == "Wall Armor")
        {
            keyWallValueText.text = "-" + playerController.GetWallImpactFuelLoss().ToString();
        }
        else if (e.UpgradeName == "Rock Armor")
        {
            keyRockValueText.text = "-" + playerController.GetRockImpactFuelLoss().ToString();
        }
        else if (e.UpgradeName == "Crystal Fuel Cells")
        {
            hideCrystalFuelCellValue.SetActive(false);
            keyCrystalValueText.text = "+" + playerController.GetFuelCellAmount().ToString();
        }
        else if (e.UpgradeName == "Asteroid Shield")
        {
            shieldLabelText.text = "ASTEROID SHIELDS";
            asteroidShieldImage.SetActive(true);
        }
        else if (e.UpgradeName == "Fuel Efficiency")
        {
            fuelEfficiencyText.text = "-" + playerController.GetFuelEfficiency().ToString();
        }
        else if (e.UpgradeName == "Fuel Capacity")
        {
            maxFuelCapacityText.text = playerController.GetMaxFuelAmount().ToString();
        }
    }

    private void ResetHUDKeyValues()
    {
        keyFuelValueText.text = "+" + playerController.GetFuelCellAmount().ToString();
        keyCrystalValueText.text = "+" + playerController.GetFuelCellAmount().ToString();
        keyWallValueText.text = "-" + playerController.GetWallImpactFuelLoss().ToString();
        keyRockValueText.text = "-" + playerController.GetRockImpactFuelLoss().ToString();
        hideCrystalFuelCellValue.SetActive(true);
        shieldLabelText.text = "ROCK SHIELDS";
        asteroidShieldImage.SetActive(false);
        fuelEfficiencyText.text = "-" + playerController.GetFuelEfficiency().ToString();
        maxFuelCapacityText.text = playerController.GetMaxFuelAmount().ToString();
    }

    void Update()
    {
        float seconds = Mathf.FloorToInt(gameManagerScript.timeRemaining);
        timeText.text = string.Format("{0:00}", seconds);
        timeText.color = gameManagerScript.timeRemaining <= 10 ? ColorPalette.Pink : ColorPalette.Amber;
    }

    private void PlayerController_OnUpgradeMaxedOut(object sender, PlayerController.OnUpgradeMaxedOutArgs e)
    {
        RemoveUpgradeFromList(e.UpgradeIndex, e.IsLegendary);
    }

    private void PlayerController_OnCrystalCollected(object sender, PlayerController.OnCrystalCollectedEventArgs e)
    {
        UpdateCrystalText(e.CrystalCount);
    }

    private void GameManagerScript_OnCurrentLevelIncrease(object sender, GameManager.OnCurrentLevelIncreaseEventArgs e)
    {
        UpdateHUDCurrentLevel(e.CurrentLevel);
        if (e.CurrentLevel > 1) ShowLevelComplete();
    }

    // ---------------------------------------------------------------------------
    // Public panel methods
    // ---------------------------------------------------------------------------

    private void ShowLevelComplete()
    {
        ShowPanelInstant(menuBackground);
        ShowPanel(levelComplete, new Vector2(0, -SLIDE_DISTANCE));
    }

    public void ResetCrystals()
    {
        UpdateCrystalText(0);
    }

    public void ResetAvailableUpgradeLists()
    {
        availableCommonUpgrades = new List<GameObject>(commonUpgrades);
        availableLegendaryUpgrades = new List<GameObject>(legendaryUpgrades);
    }

    public void OpenShop()
    {
        AudioManager.Instance.PlayShopMusic();
        ShowPanel(shop, new Vector2(0, -SLIDE_DISTANCE));
        ShowPanel(playerStats, new Vector2(0, -SLIDE_DISTANCE), delay: 0.05f);
        SpawnRandomUpgrades();
    }

    public void OpenSpecialShop()
    {
        ShowPanelInstant(menuBackground);
        playerController.IncreaseCrystalCount(100);
        ShowPanel(shop, new Vector2(0, -SLIDE_DISTANCE));
        SpawnAllUpgrades();
    }

    public void DestroyShopUpgrades()
    {
        HidePanel(shop, new Vector2(0, -SLIDE_DISTANCE));
        HidePanel(playerStats, new Vector2(0, -SLIDE_DISTANCE));

        foreach (Transform spawnPosition in upgradeSpawnPositions)
        {
            foreach (Transform child in spawnPosition)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void GameOver()
    {
        youWinText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(true);

        pauseMenuGameOverTitle.SetActive(true);
        pauseMenuPauseTitle.SetActive(false);
        pauseMenuYouWinTitle.SetActive(false);
        returnButton.SetActive(false);
        restartButton.SetActive(true);

        ShowPanel(pauseMenu, new Vector2(0, -SLIDE_DISTANCE));
        ShowPanel(playerStats, new Vector2(0, -SLIDE_DISTANCE), delay: 0.05f);
    }

    public void YouWin()
    {
        pauseMenuYouWinTitle.SetActive(true);
        pauseMenuGameOverTitle.SetActive(false);
        pauseMenuPauseTitle.SetActive(false);
        returnButton.SetActive(false);
        restartButton.SetActive(true);

        ShowPanel(pauseMenu, new Vector2(0, -SLIDE_DISTANCE));
        ShowPanel(playerStats, new Vector2(0, -SLIDE_DISTANCE), delay: 0.05f);
    }

    // ---------------------------------------------------------------------------
    // Private helpers
    // ---------------------------------------------------------------------------

    private void UpdateCrystalText(int crystalCount)
    {
        hudCrystalText.text = "CRYSTALS: " + crystalCount.ToString();
        shopCrystalText.text = "CRYSTALS: " + crystalCount.ToString();
    }

    private void UpdateHUDCurrentLevel(int level)
    {
        levelText.text = level.ToString();
    }

    private void SpawnAllUpgrades()
    {
        foreach (Transform spawnPosition in upgradeSpawnPositions)
            foreach (Transform child in spawnPosition)
                Destroy(child.gameObject);

        int slotIndex = 0;
        foreach (GameObject upgrade in commonUpgrades)
        {
            if (slotIndex >= upgradeSpawnPositions.Length) break;
            Transform spawnPos = upgradeSpawnPositions[slotIndex++];
            Instantiate(upgrade, spawnPos.position, Quaternion.identity, spawnPos);
        }
        foreach (GameObject upgrade in legendaryUpgrades)
        {
            if (slotIndex >= upgradeSpawnPositions.Length) break;
            Transform spawnPos = upgradeSpawnPositions[slotIndex++];
            Instantiate(upgrade, spawnPos.position, Quaternion.identity, spawnPos);
        }
    }

    private void SpawnRandomUpgrades()
    {
        // Clear any lingering upgrades defensively
        foreach (Transform spawnPosition in upgradeSpawnPositions)
            foreach (Transform child in spawnPosition)
                Destroy(child.gameObject);

        GameObject[] shuffledCommonUpgrades = (GameObject[])ArrayHelpers.Shuffle(availableCommonUpgrades);
        GameObject[] shuffledLegendaryUpgrades = (GameObject[])ArrayHelpers.Shuffle(availableLegendaryUpgrades);

        for (int i = 0; i < 3; i++)
        {
            Transform spawnPos = upgradeSpawnPositions[i];
            GameObject upgradeToInstantiate;

            if (UnityEngine.Random.Range(0, 7) == 0 && shuffledLegendaryUpgrades.Length > i)
            {
                upgradeToInstantiate = shuffledLegendaryUpgrades[i];
            }
            else
            {
                upgradeToInstantiate = shuffledCommonUpgrades[i];
            }

            Instantiate(upgradeToInstantiate, spawnPos.position, Quaternion.identity, spawnPos.transform);
        }
    }

    public void RemoveUpgradeFromList(int upgradeIndex, bool isLegendary)
    {
        if (isLegendary)
            availableLegendaryUpgrades.Remove(legendaryUpgrades[upgradeIndex]);
        else
            availableCommonUpgrades.Remove(commonUpgrades[upgradeIndex]);
    }

    public void RerollUpgrade(int slotIndex)
    {
        if (playerController.GetCrystalCount() < 1) return;

        Transform slot = upgradeSpawnPositions[slotIndex];
        if (slot.childCount == 0) return;

        GameObject currentUpgrade = slot.GetChild(0).gameObject;
        ConfirmationButtonController cbc = currentUpgrade.GetComponent<ConfirmationButtonController>();
        if (cbc == null) return;

        if (availableCommonUpgrades.Count == 0 && availableLegendaryUpgrades.Count == 0) return;

        playerController.DecreaseCrystalCount(1);
        Destroy(currentUpgrade);

        GameObject[] shuffledCommon = (GameObject[])ArrayHelpers.Shuffle(availableCommonUpgrades);
        GameObject[] shuffledLegendary = (GameObject[])ArrayHelpers.Shuffle(availableLegendaryUpgrades);

        bool pickLegendary = UnityEngine.Random.Range(0, 7) == 0 && shuffledLegendary.Length > 0;
        GameObject upgradeToInstantiate;

        if (pickLegendary)
            upgradeToInstantiate = shuffledLegendary[0];
        else if (shuffledCommon.Length > 0)
            upgradeToInstantiate = shuffledCommon[0];
        else
            upgradeToInstantiate = shuffledLegendary[0];

        Instantiate(upgradeToInstantiate, slot.position, Quaternion.identity, slot);
    }
}