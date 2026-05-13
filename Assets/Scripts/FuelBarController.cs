using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FuelBarController : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI fuelText;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gameManager;
    private GameManager gameManagerScript;
    private PlayerController playerController;
    [SerializeField] private GameObject fuelBarFill;
    private Image fuelBarFillImage;

    void Start()
    {
        SetMaxFuel(100);
        SetFuel(100);

        playerController = player.GetComponent<PlayerController>();
        playerController.OnFuelChanged += PlayerController_OnFuelChanged;
        playerController.OnUpgradePurchased += PlayerController_OnUpgradePurchased;

        gameManagerScript = gameManager.GetComponent<GameManager>();
        gameManagerScript.OnGameOver += GameManagerScript_OnGameOver;

        fuelBarFillImage = fuelBarFill.GetComponent<Image>();
    }

    private void GameManagerScript_OnGameOver(object sender, EventArgs e)
    {
        SetMaxFuel(100);
    }

    private void PlayerController_OnUpgradePurchased(object sender, PlayerController.OnUpgradePurchasedArgs e)
    {
        if (e.UpgradeName == "Fuel Capacity")
        {
            int newMaxFuel = 100 + (e.UpgradeLevel * 5);
            SetMaxFuel(newMaxFuel);
            SetFuel(newMaxFuel); // Optionally refill fuel on upgrade
        }
    }

    private void PlayerController_OnFuelChanged(object sender, PlayerController.OnFuelChangedArgs e)
    {
        SetFuel((int)e.FuelAmount);
        if (e.FuelAmount <= 40)
        {
            fuelBarFillImage.color = ColorPalette.Pink;
        }
        else
        {
            fuelBarFillImage.color = ColorPalette.Green;
        }
    }

    public void SetMaxFuel(int maxFuel)
    {
        slider.maxValue = maxFuel;
        slider.value = maxFuel;
        fuelText.text = maxFuel.ToString();
    }

    public void SetFuel(int fuelAmount)
    {
        slider.value = fuelAmount;
        fuelText.text = fuelAmount.ToString();
    }
}
