using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int playerY;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject currentFuel;
    [SerializeField] private float maxFuelAmount = 100f;
    [SerializeField] private float fuelCellAmount = 40f;
    [SerializeField] private float wallImpactFuelLoss = 40f;
    [SerializeField] private float rockImpactFuelLoss = 30f;
    [SerializeField] private bool isWallTrueDamage = true;
    [SerializeField] private float moveDuration = 0.625f;
    [SerializeField] private float fuelConsumptionPerSecond = 5f;
    [SerializeField] private bool isCrystalAFuelCell = false;

    public int moveSpeedDurationLevel = 1;
    public int moveSpeedDurationMaxLevel = 10;
    public int shieldRegenIntervalLevel = 1;
    public int shieldRegenIntervalMaxLevel = 10;
    public int maxFuelAmountLevel = 1;
    public int maxFuelAmountMaxLevel = 10;
    public int fuelCellAmountLevel = 1;
    public int fuelCellAmountMaxLevel = 10;
    public int rockImpactFuelLossLevel = 1;
    public int rockImpactFuelLossMaxLevel = 10;
    public int wallImpactFuelLossLevel = 1;
    public int wallImpactFuelLossMaxLevel = 10;

    public int fuelConsumptionLevel = 1;
    public int fuelConsumptionMaxLevel = 3;
    public int isWallTrueDamageLevel = 1;
    public int isWallTrueDamageMaxLevel = 2;
    public int maxShieldCountLevel = 1;
    public int maxShieldCountMaxLevel = 3;
    public int isCrystalAFuelCellLevel = 1;
    public int isCrystalAFuelCellMaxLevel = 2;

    private Vector2Int lanePosition;
    private bool isMoving = false;
    private TextMeshPro fuelText;
    private float fuelAmount = 100f;
    private bool isLevelRunning = false;

    public event EventHandler OnPlayerHitByRock;
    public event EventHandler OnPlayerHitByWall;
    public enum HitSource { Rock, Wall };

    public event EventHandler OnFuelCellCollected;
    public event EventHandler OnCrystalCollectedSfx;

    private int crystalCount = 0;
    public event EventHandler<OnCrystalCollectedEventArgs> OnCrystalCollected;
    public class OnCrystalCollectedEventArgs : EventArgs
    {
        public int CrystalCount;
    }

    private int shieldCount = 0;
    [SerializeField] private int maxShieldCount = 1;
    [SerializeField] private float shieldRegenInterval = 10f;
    public event EventHandler<OnShieldChangedEventArgs> OnShieldChanged;
    public class OnShieldChangedEventArgs : EventArgs
    {
        public int ShieldCount;
    }

    public event EventHandler<OnShieldDestroyedEventArgs> OnShieldDestroyed;
    public class OnShieldDestroyedEventArgs : EventArgs
    {
        public int ShieldCount;
    }

    public event EventHandler OnFuelDepleted;

    private SpriteRenderer spriteRenderer;
    private bool _isBlinking = false;
    private Coroutine _blinkCoroutine;

    public event EventHandler<OnUpgradeMaxedOutArgs> OnUpgradeMaxedOut;
    public class OnUpgradeMaxedOutArgs : EventArgs
    {
        public bool IsLegendary;
        public int UpgradeIndex;
    }

    public event EventHandler<OnUpgradePurchasedArgs> OnUpgradePurchased;
    public class OnUpgradePurchasedArgs : EventArgs
    {
        public string UpgradeName;
        public int UpgradeLevel;
    }

    public event EventHandler<OnFuelChangedArgs> OnFuelChanged;
    public class OnFuelChangedArgs : EventArgs
    {
        public float FuelAmount;
    }

    public event EventHandler<OnGamePausedArgs> OnGamePaused;
    public class OnGamePausedArgs : EventArgs
    {
        public bool IsGamePaused;
    }

    public InputActionAsset InputActions;
    private InputAction moveAction;
    private InputAction interaction;

    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        interaction = InputSystem.actions.FindAction("Interact");
    }

    private bool isGamePaused = false;
    public void SetIsGamePaused(bool paused)
    {
        isGamePaused = paused;
        OnGamePaused?.Invoke(this, new OnGamePausedArgs { IsGamePaused = isGamePaused});
    }

    void Start()
    {
        isGamePaused = false;
        fuelText = currentFuel.GetComponent<TextMeshPro>();
        spriteRenderer = playerModel.GetComponent<SpriteRenderer>();
        OnShieldChanged += PlayerController_OnShieldChanged;
    }



    void Update()
    {
        if (!isGamePaused && isLevelRunning && interaction.IsPressed())
        {
            isGamePaused = true;
            OnGamePaused?.Invoke(this, new OnGamePausedArgs { IsGamePaused = isGamePaused });
        }
        if (!isGamePaused && isLevelRunning)
        {
            Movement("None");
        }
    }

    public void Movement(string buttonDirection)
    {
        Vector2 movementInput = moveAction.ReadValue<Vector2>();

        if (isMoving)
        {
            return;
        }
        Vector2Int newLaneTarget = lanePosition;
        bool wantsToMove = false;
        if (movementInput.x > 0 || buttonDirection == "Right")
        {
            newLaneTarget.x += 2;
            wantsToMove = true;
        }
        else if (movementInput.x < 0 || buttonDirection == "Left")
        {
            newLaneTarget.x -= 2;
            wantsToMove = true;
        }
        if (wantsToMove)
        {
            if (newLaneTarget.x < -4 || newLaneTarget.x > 4)
                return;
            StartCoroutine(MoveRoutine(newLaneTarget));
        }
    }

    private void PlayerController_OnShieldChanged(object sender, OnShieldChangedEventArgs e)
    {
        if (fuelAmount <= 40f) return;

        if (e.ShieldCount > 0)
            spriteRenderer.color = new Color32(0x00, 0x8C, 0xFF, 0xFF);
        else
            spriteRenderer.color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    }

    public void StartLevel()
    {
        isMoving = false;
        transform.position = new Vector2(0, playerY);
        lanePosition = new Vector2Int(0, playerY);
        fuelAmount = maxFuelAmount;
        OnFuelChanged?.Invoke(this, new OnFuelChangedArgs { FuelAmount = fuelAmount });
        shieldCount = maxShieldCount;
        fuelText.text = Mathf.RoundToInt(fuelAmount).ToString();
        OnCrystalCollected?.Invoke(this, new OnCrystalCollectedEventArgs { CrystalCount = crystalCount });
        OnShieldChanged?.Invoke(this, new OnShieldChangedEventArgs { ShieldCount = shieldCount });
        isLevelRunning = true;
        StartCoroutine(DrainFuelRoutine());
    }

    public void GameOver()
    {
        StopBlink();
        StopAllCoroutines();
        isLevelRunning = false;
        fuelAmount = maxFuelAmount;
        OnFuelChanged?.Invoke(this, new OnFuelChangedArgs { FuelAmount = fuelAmount });
    }

    public void EndLevel()
    {
        StopBlink();
        StopAllCoroutines();
        isLevelRunning = false;
    }

    public void ResetAllPlayerStats()
    {
        maxFuelAmount = 100f;
        fuelCellAmount = 40f;
        wallImpactFuelLoss = 40f;
        rockImpactFuelLoss = 30f;
        isWallTrueDamage = true;
        moveDuration = 0.625f;
        fuelConsumptionPerSecond = 5f;
        maxShieldCount = 1;
        shieldRegenInterval = 10f;
        crystalCount = 0;
        isCrystalAFuelCell = false;
        OnCrystalCollected?.Invoke(this, new OnCrystalCollectedEventArgs { CrystalCount = crystalCount });
        OnShieldChanged?.Invoke(this, new OnShieldChangedEventArgs { ShieldCount = shieldCount });
        OnFuelChanged?.Invoke(this, new OnFuelChangedArgs { FuelAmount = fuelAmount });

        moveSpeedDurationLevel = 1;
        shieldRegenIntervalLevel = 1;
        maxFuelAmountLevel = 1;
        fuelCellAmountLevel = 1;
        rockImpactFuelLossLevel = 1;
        wallImpactFuelLossLevel = 1;
        fuelConsumptionLevel = 1;
        isWallTrueDamageLevel = 1;
        maxShieldCountLevel = 1;
        isCrystalAFuelCellLevel = 1;
    }

    IEnumerator MoveRoutine(Vector2Int newLaneTarget)
    {
        isMoving = true;

        float elapsedTime = 0f;
        Vector2 startPos = transform.position;
        Vector2 targetPos = new Vector2(newLaneTarget.x, newLaneTarget.y);

        while (elapsedTime < moveDuration)
        {
            while (isGamePaused)
            {
                yield return null;
            }

            elapsedTime += Time.deltaTime;

            float t = Mathf.SmoothStep(0, 1, elapsedTime / moveDuration);
            transform.position = Vector2.Lerp(startPos, targetPos, t);

            yield return null;
        }

        transform.position = targetPos;
        lanePosition = newLaneTarget;
        isMoving = false;
    }

    IEnumerator WaitForSecondsPaused(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!isGamePaused)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }
    }

    IEnumerator DrainFuelRoutine()
    {
        while (true)
        {
            yield return WaitForSecondsPaused(1f);
            LoseFuel(fuelConsumptionPerSecond);

            if (fuelText != null)
            {
                fuelText.text = Mathf.RoundToInt(fuelAmount).ToString();
            }

            if (fuelAmount <= 0)
            {
                fuelAmount = 0;
                fuelText.text = "0";
            }
        }
    }

    IEnumerator ShieldRegenRoutine()
    {
        yield return WaitForSecondsPaused(shieldRegenInterval);
        shieldCount++;
        OnShieldChanged?.Invoke(this, new OnShieldChangedEventArgs { ShieldCount = shieldCount });
    }

    private void AddFuel()
    {
        fuelAmount += fuelCellAmount;
        if (fuelAmount > maxFuelAmount)
            fuelAmount = maxFuelAmount;

        if (fuelText != null)
            fuelText.text = Mathf.RoundToInt(fuelAmount).ToString();

        if (fuelAmount > 40f)
            StopBlink();

        OnFuelChanged?.Invoke(this, new OnFuelChangedArgs { FuelAmount = fuelAmount });
    }

    private void LoseFuel(float fuelLoss)
    {
        fuelAmount -= fuelLoss;
        if (fuelAmount < 0f)
            fuelAmount = 0f;

        if (fuelText != null)
            fuelText.text = Mathf.RoundToInt(fuelAmount).ToString();

        if (fuelAmount <= 40f)
            StartBlink();

        if (fuelAmount == 0f && isLevelRunning)
            OnFuelDepleted?.Invoke(this, EventArgs.Empty);

        OnFuelChanged?.Invoke(this, new OnFuelChangedArgs { FuelAmount = fuelAmount });
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            spriteRenderer.DOKill();
            spriteRenderer.color = ColorPalette.Pink;
            Color32 targetColor = shieldCount > 0
                ? new Color32(0x00, 0x8C, 0xFF, 0xFF)
                : new Color32(0xFF, 0xFF, 0xFF, 0xFF);
            spriteRenderer.DOColor(targetColor, 0.25f);
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void StartBlink()
    {
        if (_isBlinking) return;
        _isBlinking = true;
        _blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    private void StopBlink()
    {
        if (!_isBlinking) return;
        _isBlinking = false;
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }
        spriteRenderer.DOKill();
        spriteRenderer.color = shieldCount > 0
            ? new Color32(0x00, 0x8C, 0xFF, 0xFF)
            : new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    }

    private bool UseShield(float dmgAmt, bool isTrueDamage, HitSource hitSource)
    {
        if (shieldCount > 0)
        {
            shieldCount--;
            OnShieldChanged?.Invoke(this, new OnShieldChangedEventArgs { ShieldCount = shieldCount });
            OnShieldDestroyed?.Invoke(this, new OnShieldDestroyedEventArgs { ShieldCount = shieldCount });
            if (shieldCount < maxShieldCount)
            {
                StartCoroutine(ShieldRegenRoutine());
            }
            if (isTrueDamage)
            {
                LoseFuel(dmgAmt);
                OnPlayerHitByWall?.Invoke(this, EventArgs.Empty);
                return false;
            }
            return true;
        }
        else
        {
            LoseFuel(dmgAmt);
            if (hitSource == HitSource.Rock)
            {
                OnPlayerHitByRock?.Invoke(this, EventArgs.Empty);
            }
            else if (hitSource == HitSource.Wall)
            {
                OnPlayerHitByWall?.Invoke(this, EventArgs.Empty);
            }
            return false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<ObjectController>(out var oc) && oc.IsConsumed) return;

        if (other.gameObject.CompareTag("Rock"))
        {
            bool blocked = UseShield(rockImpactFuelLoss, false, HitSource.Rock);
            oc?.Consume(blocked ? "BLOCKED!" : $"-{Mathf.RoundToInt(rockImpactFuelLoss)}", blocked ? ColorPalette.Blue : ColorPalette.Pink);
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            bool blocked = UseShield(wallImpactFuelLoss, isWallTrueDamage, HitSource.Wall);
            oc?.Consume(blocked ? "BLOCKED!" : $"-{Mathf.RoundToInt(wallImpactFuelLoss)}", blocked ? ColorPalette.Blue : ColorPalette.Pink);
        }
        else if (other.gameObject.CompareTag("Fuel"))
        {
            AddFuel();
            OnFuelCellCollected?.Invoke(this, EventArgs.Empty);
            oc?.Consume($"+{Mathf.RoundToInt(fuelCellAmount)}", ColorPalette.Green);
        }
        else if (other.gameObject.CompareTag("Crystal"))
        {
            if (isCrystalAFuelCell)
            {
                AddFuel();
                OnFuelCellCollected?.Invoke(this, EventArgs.Empty);
            }
            crystalCount++;
            OnCrystalCollected?.Invoke(this, new OnCrystalCollectedEventArgs { CrystalCount = crystalCount });
            OnCrystalCollectedSfx?.Invoke(this, EventArgs.Empty);
            oc?.Consume("+1", ColorPalette.Cyan);
        }
    }

    public void IncreaseMovementSpeed(float seconds)
    {
        moveSpeedDurationLevel++;
        if (moveSpeedDurationLevel >= moveSpeedDurationMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = false, UpgradeIndex = 0 });
        }
        moveDuration -= seconds;
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Dodge Speed", UpgradeLevel = moveSpeedDurationLevel });
    }

    public void IncreaseShieldRechargeRate(float seconds)
    {
        shieldRegenIntervalLevel++;
        if (shieldRegenIntervalLevel >= shieldRegenIntervalMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = false, UpgradeIndex = 1 });
        }
        shieldRegenInterval -= seconds;
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Shield Recharge Rate", UpgradeLevel = shieldRegenIntervalLevel });
    }

    public void IncreaseFuelCapacity(float amt)
    {
        maxFuelAmountLevel++;
        if (maxFuelAmountLevel >= maxFuelAmountMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = false, UpgradeIndex = 2 });
        }
        maxFuelAmount += amt;
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Fuel Capacity", UpgradeLevel = maxFuelAmountLevel });
    }

    public void IncreaseFuelCellAmount(float amt)
    {
        fuelCellAmountLevel++;
        if (fuelCellAmountLevel >= fuelCellAmountMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = false, UpgradeIndex = 3 });
        }
        fuelCellAmount += amt;
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Fuel Cell", UpgradeLevel = fuelCellAmountLevel });
    }

    public void DecreaseRockImpactFuelLoss(float amt)
    {
        rockImpactFuelLossLevel++;
        if (rockImpactFuelLossLevel >= rockImpactFuelLossMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = false, UpgradeIndex = 4 });
        }
        rockImpactFuelLoss -= amt;
        if (rockImpactFuelLoss < 0f)
        {
            rockImpactFuelLoss = 0f;
        }
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Rock Armor", UpgradeLevel = rockImpactFuelLossLevel });
    }

    public void DecreaseWallImpactFuelLoss(float amt)
    {
        wallImpactFuelLossLevel++;
        if (wallImpactFuelLossLevel >= wallImpactFuelLossMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = false, UpgradeIndex = 5 });
        }
        wallImpactFuelLoss -= amt;
        if (wallImpactFuelLoss < 0f)
        {
            wallImpactFuelLoss = 0f;
        }
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Wall Armor", UpgradeLevel = wallImpactFuelLossLevel });
    }

    public void DecreaseFuelConsumption(float amt)
    {
        fuelConsumptionLevel++;
        if (fuelConsumptionLevel >= fuelConsumptionMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = true, UpgradeIndex = 0 });
        }

        fuelConsumptionPerSecond -= amt;
        if (fuelConsumptionPerSecond < 0f)
        {
            fuelConsumptionPerSecond = 0f;
        }
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Fuel Efficiency", UpgradeLevel = fuelConsumptionLevel });
    }

    public void UpgradeShields()
    {
        isWallTrueDamageLevel++;
        if (isWallTrueDamageLevel >= isWallTrueDamageMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = true, UpgradeIndex = 1 });
        }

        isWallTrueDamage = false;
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Asteroid Shield", UpgradeLevel = isWallTrueDamageLevel });
    }

    public void IncreaseMaxShieldCount()
    {
        maxShieldCountLevel++;
        if (maxShieldCountLevel >= maxShieldCountMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = true, UpgradeIndex = 2 });
        }

        maxShieldCount++;
        shieldCount++;
        OnShieldChanged?.Invoke(this, new OnShieldChangedEventArgs { ShieldCount = shieldCount });
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Shield Charges", UpgradeLevel = maxShieldCountLevel });
    }

    public void SetCrystalAsFuelCell()
    {
        isCrystalAFuelCellLevel++;
        if (isCrystalAFuelCellLevel >= isCrystalAFuelCellMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = true, UpgradeIndex = 3 });
        }

        isCrystalAFuelCell = true;
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Crystal Fuel Cells", UpgradeLevel = isCrystalAFuelCellLevel });
    }

    public int GetCrystalCount()
    {
        return crystalCount;
    }

    public void DecreaseCrystalCount(int count)
    {
        if (crystalCount > 0)
        {
            crystalCount -= count;
            if (crystalCount < 0) { crystalCount = 0; }
            OnCrystalCollected?.Invoke(this, new OnCrystalCollectedEventArgs { CrystalCount = crystalCount });
        }
    }

    public void IncreaseCrystalCount(int count)
    {
        crystalCount = count;
        OnCrystalCollected?.Invoke(this, new OnCrystalCollectedEventArgs { CrystalCount = crystalCount });
    }

    public float GetFuelAmount()
    {
        return fuelAmount;
    }

    public float GetMaxFuelAmount()
    {
        return maxFuelAmount;
    }

    public int GetShieldCount()
    {
        return shieldCount;
    }

    public int GetMaxShieldCount()
    {
        return maxShieldCount;
    }

    public float GetFuelCellAmount()
    {
        return fuelCellAmount;
    }

    public float GetWallImpactFuelLoss()
    {
        return wallImpactFuelLoss;
    }
    public float GetRockImpactFuelLoss()
    {
        return rockImpactFuelLoss;
    }

    public bool GetIsWallTrueDamage()
    {
        return isWallTrueDamage;
    }

    public bool GetIsCrystalAFuelCell()
    {
        return isCrystalAFuelCell;
    }

    public float GetShieldRechargeInterval()
    {
        return shieldRegenInterval;
    }

    public float GetFuelEfficiency()
    {
        return fuelConsumptionPerSecond;
    }
}
