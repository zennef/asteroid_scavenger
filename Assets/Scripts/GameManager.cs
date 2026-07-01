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
    [SerializeField] private float wallLevelFinalSpawnRate = 0.864f;
    [SerializeField] private float rockLevelFinalSpawnRate = 0.205f;
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
        int levelIndex = currentLevel - 1;

        float wallRateStart = 1f / wallLevelDefaultSpawnRate;
        float wallRateEnd = 1f / wallLevelFinalSpawnRate;
        float wallRatio = Mathf.Pow(wallRateEnd / wallRateStart, 1f / 11f);
        float wallRate = wallRateStart * Mathf.Pow(wallRatio, levelIndex);
        objectSpawnerManager.SetWallSpawnRate(1f / wallRate);

        float rockRateStart = 1f / rockLevelDefaultSpawnRate;
        float rockRateEnd = 1f / rockLevelFinalSpawnRate;
        float rockRatio = Mathf.Pow(rockRateEnd / rockRateStart, 1f / 11f);
        float rockRate = rockRateStart * Mathf.Pow(rockRatio, levelIndex);
        objectSpawnerManager.SetRockSpawnRate(1f / rockRate);
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
