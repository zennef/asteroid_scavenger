using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Scene References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gameManager;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip sfxCrystalCollect;
    [SerializeField] private AudioClip sfxFuelCellCollect;
    [SerializeField] private AudioClip sfxRockHit;
    [SerializeField] private AudioClip sfxWallHit;
    [SerializeField] private AudioClip sfxShieldAbsorb;
    [SerializeField] private AudioClip sfxShieldDestroyed;
    [SerializeField] private AudioClip sfxShieldRecharged;
    [SerializeField] private AudioClip sfxLowFuelWarning;
    [SerializeField] private AudioClip sfxLowTimeWarning;
    [SerializeField] private AudioClip sfxUpgradePurchased;
    [SerializeField] private AudioClip sfxLevelComplete;
    [SerializeField] private AudioClip sfxGameOver;
    [SerializeField] private AudioClip sfxYouWin;

    [Header("Music Clips")]
    [SerializeField] private AudioClip musicMenu;
    [SerializeField] private AudioClip[] musicGameplayEarly = new AudioClip[4];   // levels 1-4
    [SerializeField] private AudioClip[] musicGameplayMid = new AudioClip[4];     // levels 5-8
    [SerializeField] private AudioClip[] musicGameplayLate = new AudioClip[4];    // levels 9-12
    [SerializeField] private AudioClip musicShop;
    [SerializeField] private AudioClip musicGameOver;
    [SerializeField] private AudioClip musicYouWin;

    [Header("Volume (0-1)")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.6f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    [Header("SFX Source Pool Size")]
    [SerializeField] private int sfxSourceCount = 8;

    [Header("Low Fuel Threshold")]
    [SerializeField] private float lowFuelThreshold = 30f;

    private AudioSource musicSource;
    private List<AudioSource> sfxPool = new List<AudioSource>();

    private PlayerController playerController;
    private GameManager gameManagerScript;

    private AudioClip[] _shuffledEarly;
    private AudioClip[] _shuffledMid;
    private AudioClip[] _shuffledLate;

    private bool lowFuelWarningSent = false;
    private bool isMuted = false;
    private int previousShieldCount = -1;
    private int currentLevel = 1;
    private bool _isLowTimeWarningActive = false;
    private int _lastBeepSecond = -1;
    private bool _shieldUpgradeJustPurchased = false;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildAudioSources();
    }

    private void Start()
    {
        if (player == null || gameManager == null)
        {
            Debug.LogWarning("AudioManager: Player or GameManager reference not set in inspector.");
            return;
        }

        playerController = player.GetComponent<PlayerController>();
        gameManagerScript = gameManager.GetComponent<GameManager>();

        SubscribeToEvents();

        // Start on menu music immediately
        PlayMusic(musicMenu);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void Update()
    {
        if (!_isLowTimeWarningActive || gameManagerScript == null) return;

        float t = gameManagerScript.timeRemaining;
        if (t <= 0f || t > 10f) return;

        int wholeSecond = Mathf.FloorToInt(t);
        if (wholeSecond != _lastBeepSecond)
        {
            _lastBeepSecond = wholeSecond;
            PlaySFX(sfxLowTimeWarning);
        }
    }

    // =========================================================================
    // Setup
    // =========================================================================

    private void BuildAudioSources()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume * masterVolume;

        for (int i = 0; i < sfxSourceCount; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.volume = sfxVolume * masterVolume;
            sfxPool.Add(src);
        }
    }

    private void SubscribeToEvents()
    {
        playerController.OnShieldChanged += Handle_ShieldChanged;
        playerController.OnShieldDestroyed += Handle_ShieldDestroyed;
        playerController.OnUpgradePurchased += Handle_UpgradePurchased;
        playerController.OnFuelDepleted += Handle_FuelDepleted;
        playerController.OnPlayerHitByRock += Handle_RockCollision;
        playerController.OnPlayerHitByWall += Handle_AsteroidCollision;
        playerController.OnFuelCellCollected += Handle_FuelCellCollected;
        playerController.OnCrystalCollectedSfx += Handle_CrystalCollectedSfx;
        playerController.OnGamePaused += Handle_GamePaused;

        gameManagerScript.OnGameStart += Handle_GameStart;
        gameManagerScript.OnLevelStarted += Handle_LevelStarted;
        gameManagerScript.OnLevelEnded += Handle_LevelEnded;
        gameManagerScript.OnGameOver += Handle_GameOver;
        gameManagerScript.OnYouWin += Handle_YouWin;
        gameManagerScript.OnCurrentLevelIncrease += Handle_CurrentLevelIncrease;

    }

    private void UnsubscribeFromEvents()
    {
        if (playerController != null)
        {
            playerController.OnShieldChanged -= Handle_ShieldChanged;
            playerController.OnShieldDestroyed -= Handle_ShieldDestroyed;
            playerController.OnUpgradePurchased -= Handle_UpgradePurchased;
            playerController.OnFuelDepleted -= Handle_FuelDepleted;
            playerController.OnPlayerHitByRock -= Handle_RockCollision;
            playerController.OnPlayerHitByWall -= Handle_AsteroidCollision;
            playerController.OnFuelCellCollected -= Handle_FuelCellCollected;
            playerController.OnCrystalCollectedSfx -= Handle_CrystalCollectedSfx;
            playerController.OnGamePaused -= Handle_GamePaused;
        }

        if (gameManagerScript != null)
        {
            gameManagerScript.OnGameStart -= Handle_GameStart;
            gameManagerScript.OnLevelStarted -= Handle_LevelStarted;
            gameManagerScript.OnLevelEnded -= Handle_LevelEnded;
            gameManagerScript.OnGameOver -= Handle_GameOver;
            gameManagerScript.OnYouWin -= Handle_YouWin;
            gameManagerScript.OnCurrentLevelIncrease -= Handle_CurrentLevelIncrease;
        }
    }

    // =========================================================================
    // Event handlers
    // =========================================================================

    private void Handle_GamePaused(object sender, PlayerController.OnGamePausedArgs e)
    {
        if (e.IsGamePaused)
            musicSource.Pause();
        else
            musicSource.UnPause();
    }

    private void Handle_FuelCellCollected(object sender, EventArgs e)
    {
        PlaySFX(sfxFuelCellCollect);
    }

    private void Handle_RockCollision(object sender, EventArgs e)
    {
        PlaySFX(sfxRockHit);
    }

    private void Handle_AsteroidCollision(object sender, EventArgs e)
    {
        PlaySFX(sfxWallHit);
    }

    private void Handle_CrystalCollectedSfx(object sender, EventArgs e)
    {
        PlaySFX(sfxCrystalCollect);
    }

    private void Handle_ShieldChanged(object sender, PlayerController.OnShieldChangedEventArgs e)
    {
        if (previousShieldCount >= 0 && e.ShieldCount > previousShieldCount)
        {
            if (_shieldUpgradeJustPurchased)
                _shieldUpgradeJustPurchased = false;
            else
                PlaySFX(sfxShieldRecharged);
        }
        previousShieldCount = e.ShieldCount;
    }

    private void Handle_ShieldDestroyed(object sender, PlayerController.OnShieldDestroyedEventArgs e)
    {
        PlaySFX(sfxShieldDestroyed);
    }

    private void Handle_UpgradePurchased(object sender, PlayerController.OnUpgradePurchasedArgs e)
    {
        if (e.UpgradeType == UpgradeType.ShieldCharges)
            _shieldUpgradeJustPurchased = true;
        PlaySFX(sfxUpgradePurchased);
    }

    private void Handle_FuelDepleted(object sender, EventArgs e)
    {
        // Game over music handled in Handle_GameOver
    }

    private void Handle_GameStart(object sender, EventArgs e)
    {
        lowFuelWarningSent = false;
        previousShieldCount = -1;
        ShufflePlaylists();
        PlayMusic(GetGameplayMusicForLevel(currentLevel), forceRestart: true);
    }

    private void Handle_CurrentLevelIncrease(object sender, GameManager.OnCurrentLevelIncreaseEventArgs e)
    {
        currentLevel = e.CurrentLevel;
    }


    private void Handle_LevelStarted(object sender, EventArgs e)
    {
        lowFuelWarningSent = false;
        _isLowTimeWarningActive = true;
        _lastBeepSecond = -1;
        AudioClip correctClip = GetGameplayMusicForLevel(currentLevel);
        PlayMusic(correctClip, forceRestart: true);
    }

    private void Handle_LevelEnded(object sender, EventArgs e)
    {
        _isLowTimeWarningActive = false;
        PlaySFX(sfxLevelComplete);
    }

    private void Handle_GameOver(object sender, EventArgs e)
    {
        _isLowTimeWarningActive = false;
        PlaySFX(sfxGameOver);
        PlayMusic(musicGameOver);
    }

    private void Handle_YouWin(object sender, EventArgs e)
    {
        _isLowTimeWarningActive = false;
        PlaySFX(sfxYouWin);
        PlayMusic(musicYouWin);
    }

    // =========================================================================
    // Public API
    // =========================================================================

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || isMuted) return;

        AudioSource src = GetFreeSFXSource();
        if (src == null) return;

        src.volume = sfxVolume * masterVolume * volumeScale;
        src.PlayOneShot(clip);
    }

    public void PlaySFX(string clipName, float volumeScale = 1f)
    {
        PlaySFX(GetClipByName(clipName), volumeScale);
    }

    public void PlayMusic(AudioClip clip, bool forceRestart = false)
    {
        if (clip == null) return;
        if (!forceRestart && musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    public void SetMute(bool muted)
    {
        isMuted = muted;
        musicSource.mute = muted;
        foreach (AudioSource src in sfxPool) src.mute = muted;
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void PlayShopMusic()
    {
        PlayMusic(musicShop);
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    private AudioSource GetFreeSFXSource()
    {
        foreach (AudioSource src in sfxPool)
            if (!src.isPlaying) return src;

        return sfxPool[0];
    }

    private AudioClip GetClipByName(string clipName)
    {
        return clipName switch
        {
            "crystal_collect" => sfxCrystalCollect,
            "fuel_cell_collect" => sfxFuelCellCollect,
            "rock_hit" => sfxRockHit,
            "wall_hit" => sfxWallHit,
            "shield_absorb" => sfxShieldAbsorb,
            "shield_destroyed" => sfxShieldDestroyed,
            "shield_recharged" => sfxShieldRecharged,
            "low_fuel_warning" => sfxLowFuelWarning,
            "upgrade_purchased" => sfxUpgradePurchased,
            "level_complete" => sfxLevelComplete,
            "game_over" => sfxGameOver,
            "you_win" => sfxYouWin,
            _ => null
        };
    }

    private void ShufflePlaylists()
    {
        _shuffledEarly = ShuffleArray(musicGameplayEarly);
        _shuffledMid = ShuffleArray(musicGameplayMid);
        _shuffledLate = ShuffleArray(musicGameplayLate);
    }

    private AudioClip[] ShuffleArray(AudioClip[] source)
    {
        AudioClip[] result = (AudioClip[])source.Clone();
        for (int i = result.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            AudioClip temp = result[i];
            result[i] = result[j];
            result[j] = temp;
        }
        return result;
    }

    private AudioClip GetGameplayMusicForLevel(int level)
    {
        if (_shuffledEarly == null || _shuffledMid == null || _shuffledLate == null)
            ShufflePlaylists();

        if (level <= 4) return _shuffledEarly[level - 1];
        if (level <= 8) return _shuffledMid[level - 5];
        return _shuffledLate[level - 9];
    }
}