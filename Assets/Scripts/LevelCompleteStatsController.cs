using TMPro;
using UnityEngine;

public class LevelCompleteStatsController : MonoBehaviour
{
    [SerializeField] private GameObject levelStatsManager;
    [SerializeField] private GameObject player;

    [SerializeField] private TextMeshProUGUI fuelCellsText;
    [SerializeField] private TextMeshProUGUI crystalsText;
    [SerializeField] private TextMeshProUGUI shieldsUsedText;
    [SerializeField] private TextMeshProUGUI noShieldBonusText;
    [SerializeField] private TextMeshProUGUI rockImpactsText;
    [SerializeField] private TextMeshProUGUI asteroidImpactsText;
    [SerializeField] private TextMeshProUGUI totalImpactFuelLossText;
    [SerializeField] private TextMeshProUGUI impactFreeBonusText;
    [SerializeField] private TextMeshProUGUI totalCrystalBonusText;
    [SerializeField] private TextMeshProUGUI totalCrystalsText;

    private LevelStatsManager levelStatsManagerScript;
    private PlayerController playerController;

    public void Initialize()
    {
        levelStatsManagerScript = levelStatsManager.GetComponent<LevelStatsManager>();
        playerController = player.GetComponent<PlayerController>();
        levelStatsManagerScript.OnLevelStatsFinalized += LevelStatsManagerScript_OnLevelStatsFinalized;
    }

    private void OnDestroy()
    {
        if (levelStatsManagerScript != null)
        {
            levelStatsManagerScript.OnLevelStatsFinalized -= LevelStatsManagerScript_OnLevelStatsFinalized;
        }
    }

    private void LevelStatsManagerScript_OnLevelStatsFinalized(object sender, LevelStatsManager.OnLevelStatsFinalizedEventArgs e)
    {
        fuelCellsText.text = e.FuelCellsCollected + " (" + Mathf.RoundToInt(e.FuelCellsFuelGained) + " fuel)";

        bool crystalFuelUnlocked = playerController.GetCrystalFuelMultiplier() > 0f;
        crystalsText.text = crystalFuelUnlocked
            ? e.CrystalsCollected + " (" + Mathf.RoundToInt(e.CrystalsFuelGained) + " fuel)"
            : e.CrystalsCollected.ToString();

        shieldsUsedText.text = e.ShieldsUsed.ToString();
        noShieldBonusText.text = (e.NoShieldUsedBonusEarned ? 1 : 0).ToString();

        rockImpactsText.text = e.RockImpacts + " (-" + Mathf.RoundToInt(e.RockFuelLoss) + " fuel)";
        asteroidImpactsText.text = e.AsteroidImpacts + " (-" + Mathf.RoundToInt(e.AsteroidFuelLoss) + " fuel)";
        totalImpactFuelLossText.text = Mathf.RoundToInt(e.TotalImpactFuelLoss).ToString();
        impactFreeBonusText.text = (e.ImpactFreeBonusEarned ? 1 : 0).ToString();

        totalCrystalBonusText.text = e.TotalCrystalBonus.ToString();
        totalCrystalsText.text = e.TotalCrystalsEarnedThisLevel.ToString();
    }
}
