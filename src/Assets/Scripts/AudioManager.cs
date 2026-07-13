using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";

    public static AudioManager Instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource musicSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private bool playBackgroundMusicOnStart = true;
    [SerializeField, Range(0f, 1f)] private float backgroundMusicVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float bossMusicVolume = 0.7f;

    [Header("Sound Clips")]
    [SerializeField] private AudioClip gameStartSfx;
    [SerializeField] private AudioClip levelUpSfx;
    [SerializeField] private AudioClip upgradeChooseSfx;
    [SerializeField] private AudioClip victorySfx;
    [SerializeField] private AudioClip gameOverSfx;
    [SerializeField] private AudioClip goblinDeathSfx;
    [SerializeField] private AudioClip vampireBatDeathSfx;

    private bool goblinDeathSfxPlayedThisScene;
    private bool vampireBatDeathSfxPlayedThisScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.CopyMissingClipsFrom(this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeChannels();
    }

    private void Start()
    {
        CheckGlobalAudioSettings();

        if (playBackgroundMusicOnStart && !IsMainMenuScene(SceneManager.GetActiveScene()))
        {
            PlayBackgroundMusic();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetBossDeathSfxState();
        CheckGlobalAudioSettings();

        if (IsMainMenuScene(scene))
        {
            StopMusic();
            return;
        }

        if (playBackgroundMusicOnStart && !IsMusicPlaying())
        {
            PlayBackgroundMusic();
        }
    }

    private bool IsMainMenuScene(Scene scene)
    {
        return scene.name == MainMenuSceneName;
    }

    private void InitializeChannels()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = 1f;
        sfxSource.ignoreListenerPause = true;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = backgroundMusicVolume;
        musicSource.ignoreListenerPause = true;
    }

    private void CopyMissingClipsFrom(AudioManager other)
    {
        if (other == null) return;

        if (backgroundMusic == null) backgroundMusic = other.backgroundMusic;
        if (bossMusic == null) bossMusic = other.bossMusic;

        if (gameStartSfx == null) gameStartSfx = other.gameStartSfx;
        if (levelUpSfx == null) levelUpSfx = other.levelUpSfx;
        if (upgradeChooseSfx == null) upgradeChooseSfx = other.upgradeChooseSfx;
        if (victorySfx == null) victorySfx = other.victorySfx;
        if (gameOverSfx == null) gameOverSfx = other.gameOverSfx;
        if (goblinDeathSfx == null) goblinDeathSfx = other.goblinDeathSfx;
        if (vampireBatDeathSfx == null) vampireBatDeathSfx = other.vampireBatDeathSfx;
    }

    private void CheckGlobalAudioSettings()
    {
        if (AudioListener.volume <= 0f)
        {
            Debug.LogWarning("AudioManager: AudioListener.volume is 0. No sound will be heard.");
        }

        if (AudioListener.pause)
        {
            Debug.LogWarning("AudioManager: AudioListener.pause is true. Audio may be paused.");
        }
    }

    private bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    private void ResetBossDeathSfxState()
    {
        goblinDeathSfxPlayedThisScene = false;
        vampireBatDeathSfxPlayedThisScene = false;
    }

    public void PlayMusic(AudioClip musicClip, float volume = 0.5f)
    {
        if (musicSource == null)
        {
            Debug.LogError("AudioManager: musicSource is null.");
            return;
        }

        if (musicClip == null)
        {
            Debug.LogWarning("AudioManager: Tried to play music, but the music clip is not assigned.");
            return;
        }

        if (musicSource.clip == musicClip && musicSource.isPlaying)
        {
            Debug.Log("AudioManager: Music already playing: " + musicClip.name);
            return;
        }

        musicSource.Stop();
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.volume = volume;
        musicSource.Play();

        Debug.Log("AudioManager: Playing music: " + musicClip.name + " at volume " + volume);
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null)
        {
            Debug.LogWarning("AudioManager: Background Music is not assigned in the Inspector.");
            return;
        }

        PlayMusic(backgroundMusic, backgroundMusicVolume);
    }

    public void PlayBossMusic()
    {
        if (bossMusic == null)
        {
            Debug.LogWarning("AudioManager: Boss Music is not assigned in the Inspector.");
            return;
        }

        PlayMusic(bossMusic, bossMusicVolume);
    }

    public void StopMusic()
    {
        if (musicSource == null) return;

        musicSource.Stop();
        musicSource.clip = null;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource == null)
        {
            Debug.LogError("AudioManager: sfxSource is null.");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Tried to play SFX, but clip is not assigned.");
            return;
        }

        sfxSource.PlayOneShot(clip, volume);
        Debug.Log("AudioManager: Playing SFX: " + clip.name);
    }

    public void PlayVictorySFX()
    {
        PlaySFX(victorySfx, 1f);
    }

    public void PlayGameStartSFX()
    {
        PlaySFX(gameStartSfx, 1f);
    }

    public void PlayLevelUpSFX()
    {
        PlaySFX(levelUpSfx, 1f);
    }

    

    public void PlayUpgradeChooseSFX()
    {
        PlaySFX(upgradeChooseSfx, 1f);
    }

    public void PlayGameOverSFX()
    {
        PlaySFX(gameOverSfx, 1f);
    }

    public void PlayGoblinDeathSFX()
    {
        if (goblinDeathSfxPlayedThisScene)
        {
            return;
        }

        goblinDeathSfxPlayedThisScene = true;
        PlaySFX(goblinDeathSfx, 1f);
    }

    public void PlayVampireBatDeathSFX()
    {
        if (vampireBatDeathSfxPlayedThisScene)
        {
            return;
        }

        vampireBatDeathSfxPlayedThisScene = true;
        PlaySFX(vampireBatDeathSfx, 1f);
    }

    public void StopAllAudio()
    {
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }

        StopMusic();
    }
}