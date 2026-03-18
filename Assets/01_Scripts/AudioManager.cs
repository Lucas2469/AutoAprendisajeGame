using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip backgroundMusic;
    public AudioClip cellKillSfx;
    public AudioClip bossKillSfx;
    public AudioClip clickSfx;
    public AudioClip bonusSfx;
    public AudioClip mahoragaSfx;

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
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
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

    public void PlayBonus()
    {
        if (sfxSource != null && bonusSfx != null)
            sfxSource.PlayOneShot(bonusSfx);
    }

    public void PlayMahoraga()
    {
        if (sfxSource != null && mahoragaSfx != null)
            sfxSource.PlayOneShot(mahoragaSfx);
    }
}