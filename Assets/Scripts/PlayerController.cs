using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int playerY;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject leftButton;
    [SerializeField] private GameObject rightButton;
    [SerializeField] private GameObject currentFuel;
    [SerializeField] private float maxFuelAmount = 100f;
    [SerializeField] private float fuelCellAmount = 40f;
    [SerializeField] private float wallImpactFuelLoss = 40f;
    [SerializeField] private float rockImpactFuelLoss = 30f;
    [SerializeField] private bool isWallTrueDamage = true;
    [SerializeField] private float moveDuration = 0.625f;
    [SerializeField] private float fuelConsumptionPerSecond = 5f;
    private float crystalFuelMultiplier = 0f;

    public int moveSpeedDurationLevel = 1;
    public int moveSpeedDurationMaxLevel = 11;
    public int shieldRegenIntervalLevel = 1;
    public int shieldRegenIntervalMaxLevel = 11;
    public int maxFuelAmountLevel = 1;
    public int maxFuelAmountMaxLevel = 11;
    public int fuelCellAmountLevel = 1;
    public int fuelCellAmountMaxLevel = 11;
    public int rockImpactFuelLossLevel = 1;
    public int rockImpactFuelLossMaxLevel = 6;
    public int wallImpactFuelLossLevel = 1;
    public int wallImpactFuelLossMaxLevel = 6;

    public int fuelConsumptionLevel = 1;
    public int fuelConsumptionMaxLevel = 3;
    public int isWallTrueDamageLevel = 1;
    public int isWallTrueDamageMaxLevel = 2;
    public int maxShieldCountLevel = 1;
    public int maxShieldCountMaxLevel = 3;
    public int isCrystalAFuelCellLevel = 1;
    public int isCrystalAFuelCellMaxLevel = 3;

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
    private Coroutine _flashCoroutine;
    private Coroutine _fuelFlashCoroutine;

    public event EventHandler<OnUpgradeMaxedOutArgs> OnUpgradeMaxedOut;
    public class OnUpgradeMaxedOutArgs : EventArgs
    {
        public bool IsLegendary;
        public int UpgradeIndex;
        public string UpgradeName;
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
        if (pauseButton != null)
            pauseButton.interactable = !paused;
        if (leftButton != null)
            leftButton.SetActive(!paused);
        if (rightButton != null)
            rightButton.SetActive(!paused);
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
            if (pauseButton != null)
                pauseButton.interactable = false;
            if (leftButton != null)
                leftButton.SetActive(false);
            if (rightButton != null)
                rightButton.SetActive(false);
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

        spriteRenderer.color = GetShieldRestingColor();
    }

    private Color32 GetShieldRestingColor()
    {
        return shieldCount > 0 ? ColorPalette.Blue : ColorPalette.White;
    }

    public void StartLevel()
    {
        isMoving = false;
        transform.position = new Vector2(0, playerY);
        lanePosition = new Vector2Int(0, playerY);
        fuelAmount = maxFuelAmount;
        OnFuelChanged?.Invoke(this, new OnFuelChangedArgs { FuelAmount = fuelAmount });
        shieldCount = maxShieldCount;
        OnCrystalCollected?.Invoke(this, new OnCrystalCollectedEventArgs { CrystalCount = crystalCount });
        OnShieldChanged?.Invoke(this, new OnShieldChangedEventArgs { ShieldCount = shieldCount });
        isLevelRunning = true;
        if (pauseButton != null) pauseButton.interactable = true;
        if (leftButton != null) leftButton.SetActive(true);
        if (rightButton != null) rightButton.SetActive(true);
        StartCoroutine(DrainFuelRoutine());
    }

    public void GameOver()
    {
        StopBlink();
        StopAllCoroutines();
        isLevelRunning = false;
        if (pauseButton != null) pauseButton.interactable = false;
        if (leftButton != null) leftButton.SetActive(false);
        if (rightButton != null) rightButton.SetActive(false);
    }

    public void EndLevel()
    {
        StopBlink();
        StopAllCoroutines();
        isLevelRunning = false;
        if (pauseButton != null) pauseButton.interactable = false;
        if (leftButton != null) leftButton.SetActive(false);
        if (rightButton != null) rightButton.SetActive(false);
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
        crystalFuelMultiplier = 0f;
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

        playerModel.transform.DOPunchScale(new Vector3(0.02f, -0.0125f, 0f), 0.15f, 2, 0.3f);
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

            if (fuelAmount <= 0)
            {
                fuelAmount = 0;
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

        if (fuelAmount > 40f)
            StopBlink();

        OnFuelChanged?.Invoke(this, new OnFuelChangedArgs { FuelAmount = fuelAmount });
    }

    private void LoseFuel(float fuelLoss)
    {
        fuelAmount -= fuelLoss;
        if (fuelAmount < 0f)
            fuelAmount = 0f;

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
        spriteRenderer.color = GetShieldRestingColor();
    }

    private IEnumerator FlashTwiceRoutine(Color32 color)
    {
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.DOKill();
            spriteRenderer.DOColor(color, 0.08f);
            yield return new WaitForSeconds(0.08f);
            Color32 restingColor = GetShieldRestingColor();
            spriteRenderer.DOKill();
            spriteRenderer.DOColor(restingColor, 0.08f);
            yield return new WaitForSeconds(0.08f);
        }

        spriteRenderer.DOKill();
        spriteRenderer.color = GetShieldRestingColor();
    }

    private void TriggerFlash(Color32 color)
    {
        if (_isBlinking) return;
        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashTwiceRoutine(color));
    }

    private void FlashFuelText(string text, Color32 color)
    {
        if (_fuelFlashCoroutine != null) StopCoroutine(_fuelFlashCoroutine);
        DOTween.Kill(fuelText.transform);
        DOTween.Kill(fuelText);
        fuelText.transform.localScale = Vector3.one;
        fuelText.color = new Color(fuelText.color.r, fuelText.color.g, fuelText.color.b, 1f);
        _fuelFlashCoroutine = StartCoroutine(FlashFuelTextRoutine(text, color));
    }

    private IEnumerator FlashFuelTextRoutine(string text, Color32 color)
    {
        fuelText.text = text;
        Color fullColor = new Color32(color.r, color.g, color.b, 255);
        Color lightColor = Color.Lerp(fullColor, Color.white, 0.85f);
        lightColor.a = 1f;
        fuelText.color = lightColor;
        fuelText.transform.DOPunchScale(new Vector3(0.4f, 0.4f, 0f), 0.2f, 1, 0.3f).SetUpdate(false);
        DOTween.Sequence()
            .Append(fuelText.DOColor(Color.white, 0.05f).SetUpdate(false))
            .AppendInterval(0.08f)
            .Append(fuelText.DOColor(fullColor, 0.05f).SetUpdate(false))
            .Append(fuelText.DOColor(Color.white, 0.05f).SetUpdate(false))
            .AppendInterval(0.08f)
            .Append(fuelText.DOColor(fullColor, 0.05f).SetUpdate(false))
            .SetUpdate(false);
        fuelText.DOFade(0f, 0.4f)
            .SetEase(Ease.OutQuad)
            .SetDelay(0.65f)
            .SetUpdate(false)
            .OnComplete(() =>
            {
                fuelText.color = Color.white;
                fuelText.text = "";
                _fuelFlashCoroutine = null;
            });
        yield break;
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
            oc?.Consume("", ColorPalette.Pink);
            if (!blocked) TriggerFlash(ColorPalette.Pink);
            if (!blocked) FlashFuelText("-" + Mathf.RoundToInt(rockImpactFuelLoss).ToString(), ColorPalette.Pink);
            if (blocked) FlashFuelText("BLOCKED!", ColorPalette.Blue);
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            bool blocked = UseShield(wallImpactFuelLoss, isWallTrueDamage, HitSource.Wall);
            oc?.Consume("", ColorPalette.Pink);
            if (!blocked) TriggerFlash(ColorPalette.Pink);
            if (!blocked) FlashFuelText("-" + Mathf.RoundToInt(wallImpactFuelLoss).ToString(), ColorPalette.Pink);
            if (blocked) FlashFuelText("BLOCKED!", ColorPalette.Blue);
        }
        else if (other.gameObject.CompareTag("Fuel"))
        {
            AddFuel();
            OnFuelCellCollected?.Invoke(this, EventArgs.Empty);
            oc?.Consume("", ColorPalette.Green);
            TriggerFlash(ColorPalette.Green);
            FlashFuelText("+" + Mathf.RoundToInt(fuelCellAmount).ToString(), ColorPalette.Green);
        }
        else if (other.gameObject.CompareTag("Crystal"))
        {
            if (crystalFuelMultiplier > 0f)
            {
                fuelAmount += GetCrystalFuelAmount();
                if (fuelAmount > maxFuelAmount) fuelAmount = maxFuelAmount;
                if (fuelAmount > 40f) StopBlink();
                OnFuelChanged?.Invoke(this, new OnFuelChangedArgs { FuelAmount = fuelAmount });
                OnFuelCellCollected?.Invoke(this, EventArgs.Empty);
            }
            crystalCount++;
            OnCrystalCollected?.Invoke(this, new OnCrystalCollectedEventArgs { CrystalCount = crystalCount });
            OnCrystalCollectedSfx?.Invoke(this, EventArgs.Empty);
            oc?.Consume("", ColorPalette.Cyan);
            TriggerFlash(ColorPalette.Cyan);
            FlashFuelText("+1", ColorPalette.Cyan);
        }
    }

    public void IncreaseMovementSpeed(float seconds)
    {
        moveSpeedDurationLevel++;
        if (moveSpeedDurationLevel >= moveSpeedDurationMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = false, UpgradeIndex = 0, UpgradeName = "Dodge Speed" });
        }
        moveDuration -= seconds;
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Dodge Speed", UpgradeLevel = moveSpeedDurationLevel });
    }

    public void IncreaseShieldRechargeRate(float seconds)
    {
        shieldRegenIntervalLevel++;
        if (shieldRegenIntervalLevel >= shieldRegenIntervalMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = false, UpgradeIndex = 1, UpgradeName = "Shield Recharge Rate" });
        }
        shieldRegenInterval -= seconds;
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Shield Recharge Rate", UpgradeLevel = shieldRegenIntervalLevel });
    }

    public void IncreaseFuelCapacity(float amt)
    {
        maxFuelAmountLevel++;
        if (maxFuelAmountLevel >= maxFuelAmountMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = false, UpgradeIndex = 2, UpgradeName = "Fuel Capacity" });
        }
        maxFuelAmount += amt;
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Fuel Capacity", UpgradeLevel = maxFuelAmountLevel });
    }

    public void IncreaseFuelCellAmount(float amt)
    {
        fuelCellAmountLevel++;
        if (fuelCellAmountLevel >= fuelCellAmountMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = false, UpgradeIndex = 3, UpgradeName = "Fuel Cell" });
        }
        fuelCellAmount += amt;
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Fuel Cell", UpgradeLevel = fuelCellAmountLevel });
    }

    public void DecreaseRockImpactFuelLoss(float amt)
    {
        rockImpactFuelLossLevel++;
        if (rockImpactFuelLossLevel >= rockImpactFuelLossMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = false, UpgradeIndex = 4, UpgradeName = "Rock Armor" });
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
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = false, UpgradeIndex = 5, UpgradeName = "Wall Armor" });
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
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = true, UpgradeIndex = 0, UpgradeName = "Fuel Efficiency" });
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
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = true, UpgradeIndex = 1, UpgradeName = "Asteroid Shield" });
        }

        isWallTrueDamage = false;
        OnUpgradePurchased?.Invoke(this, new OnUpgradePurchasedArgs { UpgradeName = "Asteroid Shield", UpgradeLevel = isWallTrueDamageLevel });
    }

    public void IncreaseMaxShieldCount()
    {
        maxShieldCountLevel++;
        if (maxShieldCountLevel >= maxShieldCountMaxLevel)
        {
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = true, UpgradeIndex = 2, UpgradeName = "Shield Charges" });
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
            OnUpgradeMaxedOut?.Invoke(this, new OnUpgradeMaxedOutArgs { IsLegendary = true, UpgradeIndex = 3, UpgradeName = "Crystal Fuel Cells" });
        }
        crystalFuelMultiplier = isCrystalAFuelCellLevel == 2 ? 0.5f : 1.0f;
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

    public float GetCrystalFuelMultiplier()
    {
        return crystalFuelMultiplier;
    }

    public float GetCrystalFuelAmount()
    {
        return Mathf.Round(fuelCellAmount * crystalFuelMultiplier);
    }

    public int GetUpgradeMaxDisplayLevel(string upgradeName)
    {
        switch (upgradeName)
        {
            case "Dodge Speed": return moveSpeedDurationMaxLevel - 1;
            case "Shield Recharge Rate": return shieldRegenIntervalMaxLevel - 1;
            case "Fuel Capacity": return maxFuelAmountMaxLevel - 1;
            case "Fuel Cell": return fuelCellAmountMaxLevel - 1;
            case "Rock Armor": return rockImpactFuelLossMaxLevel - 1;
            case "Wall Armor": return wallImpactFuelLossMaxLevel - 1;
            case "Fuel Efficiency": return fuelConsumptionMaxLevel - 1;
            case "Asteroid Shield": return isWallTrueDamageMaxLevel - 1;
            case "Shield Charges": return maxShieldCountMaxLevel - 1;
            case "Crystal Fuel Cells": return isCrystalAFuelCellMaxLevel - 1;
            default: return 0;
        }
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
