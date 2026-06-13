using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject objectSpawner;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject objectDestroyer;
    [SerializeField] private GameObject shop;
    [SerializeField] private GameObject canvas;
    [SerializeField] private float wallLevelDefaultSpawnRate;
    [SerializeField] private float rockLevelDefaultSpawnRate;
    [SerializeField] private float wallLevelSpawnRateMult;
    [SerializeField] private float rockLevelSpawnRateMult;
    private PlayerController playerController;
    private ObjectSpawnerManager objectSpawnerManager;
    private ObjectDestroyerManager objectDestroyerManager;
    private CanvasManager canvasManager;

    public float timeRemaining;
    private bool isTimerRunning = false;
    public float defaultTimePerLevel = 70f;

    private int currentLevel = 1;

    public event EventHandler<OnCurrentLevelIncreaseEventArgs> OnCurrentLevelIncrease;
    public class OnCurrentLevelIncreaseEventArgs : EventArgs
    {
        public int CurrentLevel;
    }

    public event EventHandler OnGameStart;
    public event EventHandler OnGameOver;
    public event EventHandler OnYouWin;
    public event EventHandler OnLevelEnded;
    public event EventHandler OnLevelStarted;

    void Awake()
    {
        var playerInput = player.GetComponent<PlayerInput>();
    }

    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        objectSpawnerManager = objectSpawner.GetComponent<ObjectSpawnerManager>();
        objectDestroyerManager = objectDestroyer.GetComponent<ObjectDestroyerManager>();
        canvasManager = canvas.GetComponent<CanvasManager>();
        timeRemaining = defaultTimePerLevel;

        playerController.OnFuelDepleted += PlayerController_OnFuelDepleted;
        playerController.OnGamePaused += PlayerController_OnGamePaused;
    }

    void Update()
    {

        if (isTimerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                EndLevel(false);
                timeRemaining = 0;
                isTimerRunning = false;                
            }
        }
    }

    public void StartRun()
    {
        currentLevel = 1;
        OnCurrentLevelIncrease?.Invoke(this, new OnCurrentLevelIncreaseEventArgs { CurrentLevel = (int)currentLevel });
        playerController.ResetAllPlayerStats();
        canvasManager.ResetCrystals();
        playerController.StartLevel();
        IncreaseObstacleSpawnRate();
        objectSpawnerManager.StartLevel();
        timeRemaining = defaultTimePerLevel;
        isTimerRunning = true;
        OnGameStart?.Invoke(this, EventArgs.Empty);
        OnLevelStarted?.Invoke(this, EventArgs.Empty);
    }

    public void StartLevel()
    {
        playerController.StartLevel();
        IncreaseObstacleSpawnRate();
        objectSpawnerManager.StartLevel();
        timeRemaining = defaultTimePerLevel;
        isTimerRunning = true;
        OnLevelStarted?.Invoke(this, EventArgs.Empty);
    }

    private void IncreaseObstacleSpawnRate()
    {
        float t = (currentLevel - 1) / 11f; // 0 → 1 over 12 levels
        //float easedT = t * t; // ease-in

        // 0.15 = easier early
        // 0.35 = harder early
        float easedT = Mathf.Lerp(t * t, t, 0.4f);

        float wallMaxReduction = 1.7f - 0.95f;
        float rockMaxReduction = 1.45f - 0.25f;


        objectSpawnerManager.SetWallSpawnRate(wallLevelDefaultSpawnRate - easedT * wallMaxReduction);
        objectSpawnerManager.SetRockSpawnRate(rockLevelDefaultSpawnRate - easedT * rockMaxReduction);


        //rate = defaultRate - easedT * maxReduction;

        //if (currentLevel % 2 == 0) objectSpawnerManager.SetWallSpawnRate(wallLevelDefaultSpawnRate - (currentLevel - 1) * wallLevelSpawnRateMult); //1.5
        //if (currentLevel % 2 != 0) objectSpawnerManager.SetRockSpawnRate(rockLevelDefaultSpawnRate - (currentLevel - 1) * rockLevelSpawnRateMult); //1.25
    }

    public void EndLevel(bool isOutOfFuel)
    {
        OnLevelEnded?.Invoke(this, EventArgs.Empty);
        playerController.EndLevel();
        objectSpawnerManager.EndLevel();
        objectDestroyerManager.DestroyAllNonPlayerObjects();

        if (isOutOfFuel)
        {
            EndGame(false);
            return;
        }
        if (currentLevel == 12)
        {
            EndGame(true);
            return;
        }
        IncreaseLevel();
    }

    public void EndGame(bool isWin)
    {
        if (isWin)
            OnYouWin?.Invoke(this, EventArgs.Empty);
        else
            OnGameOver?.Invoke(this, EventArgs.Empty);

        playerController.GameOver();
        objectSpawnerManager.SetWallSpawnRate(wallLevelDefaultSpawnRate);
        objectSpawnerManager.SetRockSpawnRate(rockLevelDefaultSpawnRate);
        isTimerRunning = false;
    }

    public void IncreaseLevel()
    {
        currentLevel++;
        OnCurrentLevelIncrease?.Invoke(this, new OnCurrentLevelIncreaseEventArgs { CurrentLevel = (int)currentLevel });
    }

    private void PlayerController_OnFuelDepleted(object sender, System.EventArgs e)
    {
        Debug.Log("Fuel depleted, ending level.");
        EndLevel(true);
    }

    public void SetIsTimerRunning(bool isRunning)
    {
        isTimerRunning = isRunning;
    }

    private void PlayerController_OnGamePaused(object sender, PlayerController.OnGamePausedArgs e)
    {
        SetIsTimerRunning(!e.IsGamePaused);
    }
}
