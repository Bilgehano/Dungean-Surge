using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource musicSource;

    [Header("Sound Clips")]
    public AudioClip gameStartSound;
    public AudioClip levelUpSound;
    public AudioClip upgradeChooseSound;
    public AudioClip victorySound;
    public AudioClip endingSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeChannels();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeChannels()
    {
        // SFX Hoparlörü oluşturuluyor
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.ignoreListenerPause = true;
        sfxSource.playOnAwake = false;

        // Müzik Hoparlörü oluşturuluyor
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.ignoreListenerPause = true;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void PlayMusic(AudioClip musicClip, float volume = 0.5f)
    {
        if (musicSource == null || musicClip == null) return;

        // Eğer zaten aynı müzik çalıyorsa baştan başlatma
        if (musicSource.clip == musicClip && musicSource.isPlaying) return;

        musicSource.clip = musicClip;
        musicSource.volume = volume;
        musicSource.Play();
    }
}