using System;
using UnityEngine;

public class LevelStatsManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gameManager;

    private PlayerController playerController;
    private GameManager gameManagerScript;

    private int fuelCellsCollected;
    private float fuelCellsFuelGained;
    private int crystalsCollected;
    private float crystalsFuelGained;
    private int shieldsUsed;
    private int rockImpacts;
    private int asteroidImpacts;
    private float rockFuelLoss;
    private float asteroidFuelLoss;

    public event EventHandler<OnLevelStatsFinalizedEventArgs> OnLevelStatsFinalized;
    public class OnLevelStatsFinalizedEventArgs : EventArgs
    {
        public int FuelCellsCollected;
        public float FuelCellsFuelGained;
        public int CrystalsCollected;
        public float CrystalsFuelGained;
        public int ShieldsUsed;
        public bool NoShieldUsedBonusEarned;
        public int RockImpacts;
        public float RockFuelLoss;
        public int AsteroidImpacts;
        public float AsteroidFuelLoss;
        public float TotalImpactFuelLoss;
        public bool ImpactFreeBonusEarned;
        public int TotalCrystalBonus;
        public int TotalCrystalsEarnedThisLevel;
    }

    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        gameManagerScript = gameManager.GetComponent<GameManager>();

        playerController.OnDamageEvent += PlayerController_OnDamageEvent;
        playerController.OnResourceCollected += PlayerController_OnResourceCollected;
        gameManagerScript.OnLevelStarted += GameManagerScript_OnLevelStarted;
        gameManagerScript.OnLevelEnded += GameManagerScript_OnLevelEnded;
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnDamageEvent -= PlayerController_OnDamageEvent;
            playerController.OnResourceCollected -= PlayerController_OnResourceCollected;
        }
        if (gameManagerScript != null)
        {
            gameManagerScript.OnLevelStarted -= GameManagerScript_OnLevelStarted;
            gameManagerScript.OnLevelEnded -= GameManagerScript_OnLevelEnded;
        }
    }

    private void GameManagerScript_OnLevelStarted(object sender, EventArgs e)
    {
        fuelCellsCollected = 0;
        fuelCellsFuelGained = 0f;
        crystalsCollected = 0;
        crystalsFuelGained = 0f;
        shieldsUsed = 0;
        rockImpacts = 0;
        asteroidImpacts = 0;
        rockFuelLoss = 0f;
        asteroidFuelLoss = 0f;
    }

    private void PlayerController_OnResourceCollected(object sender, PlayerController.OnResourceCollectedEventArgs e)
    {
        if (e.Type == PlayerController.ResourceType.FuelCell)
        {
            fuelCellsCollected++;
            fuelCellsFuelGained += e.FuelGained;
        }
        else if (e.Type == PlayerController.ResourceType.Crystal)
        {
            crystalsCollected++;
            crystalsFuelGained += e.FuelGained;
        }
    }

    private void PlayerController_OnDamageEvent(object sender, PlayerController.OnDamageEventArgs e)
    {
        if (e.ShieldConsumed)
        {
            shieldsUsed++;
        }

        if (!e.WasBlocked)
        {
            if (e.Source == PlayerController.HitSource.Rock)
            {
                rockImpacts++;
                rockFuelLoss += e.Amount;
            }
            else if (e.Source == PlayerController.HitSource.Wall)
            {
                asteroidImpacts++;
                asteroidFuelLoss += e.Amount;
            }
        }
    }

    private void GameManagerScript_OnLevelEnded(object sender, EventArgs e)
    {
        bool noShieldUsedBonusEarned = shieldsUsed == 0;
        bool impactFreeBonusEarned = (rockImpacts + asteroidImpacts) == 0;

        int totalCrystalBonus = (noShieldUsedBonusEarned ? 1 : 0) + (impactFreeBonusEarned ? 1 : 0);

        if (totalCrystalBonus > 0)
        {
            playerController.AddCrystalCount(totalCrystalBonus);
        }

        OnLevelStatsFinalized?.Invoke(this, new OnLevelStatsFinalizedEventArgs
        {
            FuelCellsCollected = fuelCellsCollected,
            FuelCellsFuelGained = fuelCellsFuelGained,
            CrystalsCollected = crystalsCollected,
            CrystalsFuelGained = crystalsFuelGained,
            ShieldsUsed = shieldsUsed,
            NoShieldUsedBonusEarned = noShieldUsedBonusEarned,
            RockImpacts = rockImpacts,
            RockFuelLoss = rockFuelLoss,
            AsteroidImpacts = asteroidImpacts,
            AsteroidFuelLoss = asteroidFuelLoss,
            TotalImpactFuelLoss = rockFuelLoss + asteroidFuelLoss,
            ImpactFreeBonusEarned = impactFreeBonusEarned,
            TotalCrystalBonus = totalCrystalBonus,
            TotalCrystalsEarnedThisLevel = crystalsCollected + totalCrystalBonus
        });
    }
}
