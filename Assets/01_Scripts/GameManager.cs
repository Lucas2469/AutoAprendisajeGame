using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI roundText;
    public GameObject gameOverPanel;

    [Header("Round Settings")]
    public float roundDuration = 10f;
    public float minRoundDuration = 3f;

    private float currentTime;
    private int score = 0;
    private int currentRound = 1;
    private bool isGameOver = false;

    void Awake()
    {
        // Evita duplicados
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // ✅ Asegurar panel oculto (aunque lo tengas desactivado en Inspector, esto evita errores)
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        scoreText.text = "SCORE: 0";
        roundText.text = "ROUND: 1";
        StartRound();
    }

    void Update()
    {
        if (isGameOver) return;

        currentTime -= Time.deltaTime;
        timerText.text = "TIME: " + Mathf.Ceil(currentTime).ToString();

        if (currentTime <= 0f)
        {
            EndRound();
        }
    }

    void StartRound()
    {
        currentTime = roundDuration;
    }

    void EndRound()
    {
        if (isGameOver) return;

        GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");

        if (cells.Length > 0)
        {
            GameOver();
            return;
        }

        currentRound++;
        roundText.text = "ROUND: " + currentRound;

        IncreaseDifficulty();
        StartRound();
    }

    void IncreaseDifficulty()
    {
        roundDuration = Mathf.Max(minRoundDuration, roundDuration - 1f);
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;

        score += amount;
        scoreText.text = "SCORE: " + score;
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        // ✅ Resetear estado global acumulado (tamaño/velocidad/camuflaje/transparencia)
        ClickFeedback.ResetAll();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}