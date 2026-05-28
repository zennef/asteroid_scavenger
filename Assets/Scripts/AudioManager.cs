using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central audio manager for Asteroid Scavenger.
/// Attach to a persistent GameObject in your first scene.
/// 
/// SETUP IN INSPECTOR:
///   1. Create an empty GameObject named "AudioManager" in your scene.
///   2. Attach this script to it.
///   3. Set sfxSourceCount (8 is a good default).
///   4. Assign AudioClips to each field in the inspector.
///   5. Assign your Player and GameManager GameObjects.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Inspector: Scene references
    // -------------------------------------------------------------------------

    [Header("Scene References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gameManager;

    // -------------------------------------------------------------------------
    // Inspector: SFX clips — drag your AudioClips here
    // -------------------------------------------------------------------------

    [Header("SFX Clips")]
    [SerializeField] private AudioClip sfxCrystalCollect;      // Crystal picked up
    [SerializeField] private AudioClip sfxFuelCellCollect;     // Fuel cell picked up
    [SerializeField] private AudioClip sfxRockHit;             // Rock impact (shield absorbs or fuel lost)
    [SerializeField] private AudioClip sfxWallHit;             // Asteroid impact
    [SerializeField] private AudioClip sfxShieldAbsorb;        // Shield successfully blocks a hit
    [SerializeField] private AudioClip sfxShieldDestroyed;     // Shield charge depleted
    [SerializeField] private AudioClip sfxShieldRecharged;     // Shield comes back online
    [SerializeField] private AudioClip sfxLowFuelWarning;      // One-shot warning when fuel drops low
    [SerializeField] private AudioClip sfxUpgradePurchased;    // Upgrade bought in shop
    [SerializeField] private AudioClip sfxLevelComplete;       // Level cleared
    [SerializeField] private AudioClip sfxGameOver;            // Game over
    [SerializeField] private AudioClip sfxYouWin;              // All 12 levels cleared

    // -------------------------------------------------------------------------
    // Inspector: Music clips
    // -------------------------------------------------------------------------

    [Header("Music Clips")]
    [SerializeField] private AudioClip musicGameplay;          // Main gameplay loop
    [SerializeField] private AudioClip musicShop;              // Shop / between levels
    [SerializeField] private AudioClip musicGameOver;          // Game over screen
    [SerializeField] private AudioClip musicYouWin;            // Win screen

    // -------------------------------------------------------------------------
    // Inspector: Volume controls
    // -------------------------------------------------------------------------

    [Header("Volume (0–1)")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.6f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    [Header("SFX Source Pool Size")]
    [SerializeField] private int sfxSourceCount = 8; // Simultaneous SFX channels

    [Header("Low Fuel Threshold")]
    [SerializeField] private float lowFuelThreshold = 30f;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private AudioSource musicSource;
    private List<AudioSource> sfxPool = new List<AudioSource>();

    private PlayerController playerController;
    private GameManager gameManagerScript;

    private bool lowFuelWarningSent = false;  // So we only play it once per drop
    private bool isMuted = false;

    private int previousShieldCount = -1;     // Track shield count for recharge detection

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        // Singleton enforcement
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
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    // =========================================================================
    // Setup
    // =========================================================================

    private void BuildAudioSources()
    {
        // Dedicated music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume * masterVolume;

        // SFX pool — allows overlapping sounds
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
        // PlayerController events
        playerController.OnShieldChanged += Handle_ShieldChanged;
        playerController.OnShieldDestroyed += Handle_ShieldDestroyed;
        playerController.OnUpgradePurchased += Handle_UpgradePurchased;
        playerController.OnFuelDepleted += Handle_FuelDepleted;
        playerController.OnPlayerHitByRock += Handle_RockCollision;
        playerController.OnPlayerHitByWall += Handle_AsteroidCollision;
        playerController.OnFuelCellCollected += Handle_FuelCellCollected;
        playerController.OnCrystalCollectedSfx += Handle_CrystalCollectedSfx;

        // GameManager events
        gameManagerScript.OnGameStart += Handle_GameStart;
        gameManagerScript.OnLevelStarted += Handle_LevelStarted;
        gameManagerScript.OnLevelEnded += Handle_LevelEnded;
        gameManagerScript.OnGameOver += Handle_GameOver;
        gameManagerScript.OnYouWin += Handle_YouWin;
    }


    private void UnsubscribeFromEvents()
    {
        if (playerController != null)
        {
            playerController.OnShieldChanged -= Handle_ShieldChanged;
            playerController.OnShieldDestroyed -= Handle_ShieldDestroyed;
            playerController.OnUpgradePurchased -= Handle_UpgradePurchased;
            playerController.OnFuelDepleted -= Handle_FuelDepleted;
        }

        if (gameManagerScript != null)
        {
            gameManagerScript.OnGameStart -= Handle_GameStart;
            gameManagerScript.OnLevelStarted -= Handle_LevelStarted;
            gameManagerScript.OnLevelEnded -= Handle_LevelEnded;
            gameManagerScript.OnGameOver -= Handle_GameOver;
            gameManagerScript.OnYouWin -= Handle_YouWin;
        }
    }

    // =========================================================================
    // Event handlers
    // =========================================================================


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
        // Shield recharged — count went back up without a destroy event
        if (previousShieldCount >= 0 && e.ShieldCount > previousShieldCount)
        {
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
        PlaySFX(sfxUpgradePurchased);
    }

    private void Handle_FuelDepleted(object sender, EventArgs e)
    {
        // GameOver will fire shortly after — let Handle_GameOver handle the music swap.
        // Just play the SFX here if desired.
    }

    private void Handle_GameStart(object sender, EventArgs e)
    {
        lowFuelWarningSent = false;
        previousShieldCount = -1;
        PlayMusic(musicGameplay);
    }

    private void Handle_LevelStarted(object sender, EventArgs e)
    {
        lowFuelWarningSent = false;
        // Music continues from gameplay loop — no change needed here unless
        // you want a distinct track per level later.
        if (musicSource.clip != musicGameplay)
            PlayMusic(musicGameplay);
    }

    private void Handle_LevelEnded(object sender, EventArgs e)
    {
        PlaySFX(sfxLevelComplete);
        PlayMusic(musicShop);
    }

    private void Handle_GameOver(object sender, EventArgs e)
    {
        PlaySFX(sfxGameOver);
        PlayMusic(musicGameOver);
    }

    private void Handle_YouWin(object sender, EventArgs e)
    {
        PlaySFX(sfxYouWin);
        PlayMusic(musicYouWin);
    }

    // =========================================================================
    // Public API — call from anywhere, e.g. AudioManager.Instance.PlaySFX(...)
    // =========================================================================

    /// <summary>
    /// Play a one-shot SFX using the pool. Safe to call with a null clip (silently skips).
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || isMuted) return;

        AudioSource src = GetFreeSFXSource();
        if (src == null) return; // All channels busy — increase sfxSourceCount if this happens often

        src.volume = sfxVolume * masterVolume * volumeScale;
        src.PlayOneShot(clip);
    }

    /// <summary>
    /// Play a named SFX by string key. Useful for calling from UI buttons or
    /// places that don't have a direct AudioClip reference.
    /// </summary>
    public void PlaySFX(string clipName, float volumeScale = 1f)
    {
        PlaySFX(GetClipByName(clipName), volumeScale);
    }

    /// <summary>
    /// Swap the music track. Crossfades if you want to expand this later.
    /// </summary>
    public void PlayMusic(AudioClip clip, bool forceRestart = false)
    {
        if (clip == null) return;
        if (!forceRestart && musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    /// <summary>
    /// Master mute toggle — useful for a settings button.
    /// </summary>
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
        // SFX volume is applied per-play, so no need to update the pool here
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

    // =========================================================================
    // Private helpers
    // =========================================================================

    /// <summary>
    /// Returns the first AudioSource in the pool that isn't currently playing,
    /// or the one that has been playing longest if all are busy.
    /// </summary>
    private AudioSource GetFreeSFXSource()
    {
        foreach (AudioSource src in sfxPool)
            if (!src.isPlaying) return src;

        // All busy — steal the first one (oldest sound, likely done or nearly done)
        return sfxPool[0];
    }

    /// <summary>
    /// Maps string keys to AudioClips for PlaySFX(string) callers (e.g. UI buttons).
    /// Add any new clips here to keep them accessible by name.
    /// </summary>
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
}