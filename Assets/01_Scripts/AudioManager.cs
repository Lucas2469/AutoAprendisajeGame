using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip backgroundMusic;   // asignas tu música aquí
    public AudioClip cellKillSfx;
    public AudioClip bossKillSfx;
    public AudioClip clickSfx;          // opcional

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Música en loop
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;          // ✅ se repite sola al terminar
            musicSource.Play();
        }
    }

    public void PlayCellKill()
    {
        if (sfxSource != null && cellKillSfx != null)
            sfxSource.PlayOneShot(cellKillSfx);
    }

    public void PlayBossKill()
    {
        if (sfxSource != null && bossKillSfx != null)
            sfxSource.PlayOneShot(bossKillSfx);
    }

    public void PlayClick()
    {
        if (sfxSource != null && clickSfx != null)
            sfxSource.PlayOneShot(clickSfx);
    }
}