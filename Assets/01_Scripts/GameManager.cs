using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI adaptText;
    public TextMeshProUGUI mahoragaTurnsText;

    [Header("Trofeo fijo")]
    public Image trophyImage;
    public Sprite bronzeTrophy;
    public Sprite silverTrophy;
    public Sprite goldTrophy;
    public Sprite finalTrophy;
    public TrophyFireController trophyFireController;

    [Header("Round Settings")]
    public float roundDuration = 10f;

    [Header("Adaptación progresiva normal")]
    public Color backgroundTargetColor = new Color32(20, 90, 130, 255);
    public Color fullyAdaptedFinalColor = new Color32(20, 90, 130, 255);
    [Range(0f, 1f)] public float adaptationLevel = 0f;
    public float adaptationIncreasePerFailedRound = 0.05f;
    public int maxStoredKilledColors = 12;
    public bool resetAdaptationOnStart = true;

    [Header("Primeras rondas cálidas")]
    public int warmOnlyRounds = 10;

    [Header("Mahoraga")]
    [Min(1)] public int mahoragaMaxTurns = 12;
    public float adaptationSequenceDuration = 4f;
    public MahoragaController mahoragaController;
    public Color mahoragaMidPhaseColor = new Color32(55, 120, 170, 255);
    [Range(0f, 1f)] public float mahoragaMaxBlend = 0.85f;

    private int mahoragaRemainingTurns;
    private float currentTime;
    private int score = 0;
    private int currentRound = 1;
    private bool isRoundTransitioning = false;
    private int lastTrophyTier = 0;

    private readonly List<Color> killedColorHistory = new List<Color>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResetRuntimeState();
    }

    void OnEnable()
    {
        if (Application.isPlaying)
            ResetRuntimeState();
    }

    void OnValidate()
    {
        if (mahoragaMaxTurns < 1)
            mahoragaMaxTurns = 1;

        if (adaptationSequenceDuration < 0.1f)
            adaptationSequenceDuration = 0.1f;

        if (maxStoredKilledColors < 1)
            maxStoredKilledColors = 1;

        if (adaptationIncreasePerFailedRound < 0f)
            adaptationIncreasePerFailedRound = 0f;

        if (warmOnlyRounds < 1)
            warmOnlyRounds = 1;
    }

    void ResetRuntimeState()
    {
        score = 0;
        currentRound = 1;
        currentTime = roundDuration;
        isRoundTransitioning = false;
        mahoragaRemainingTurns = mahoragaMaxTurns;
        lastTrophyTier = 0;
        killedColorHistory.Clear();

        if (resetAdaptationOnStart)
            adaptationLevel = 0f;
    }

    void Start()
    {
        if (adaptText != null)
        {
            adaptText.text = "ADAPTANDOSE...";
            adaptText.gameObject.SetActive(false);
        }

        if (mahoragaTurnsText != null)
        {
            mahoragaTurnsText.text = "";
            mahoragaTurnsText.gameObject.SetActive(false);
        }

        if (scoreText != null)
            scoreText.text = "SCORE: 0";

        if (roundText != null)
            roundText.text = "ROUND: 1";

        if (timerText != null)
            timerText.text = "TIME: " + Mathf.Ceil(roundDuration).ToString();

        UpdateTrophyVisual(false);

        if (mahoragaController != null)
            mahoragaController.Hide();

        StartRound();
    }

    void Update()
    {
        if (isRoundTransitioning) return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(0f, currentTime);

        if (timerText != null)
            timerText.text = "TIME: " + Mathf.Ceil(currentTime).ToString();

        if (currentTime <= 0f)
            StartCoroutine(ResolveRound());
    }

    void StartRound()
    {
        currentTime = roundDuration;

        if (timerText != null)
            timerText.text = "TIME: " + Mathf.Ceil(currentTime).ToString();
    }

    IEnumerator ResolveRound()
    {
        if (isRoundTransitioning) yield break;

        isRoundTransitioning = true;
        currentTime = 0f;

        if (timerText != null)
            timerText.text = "TIME: 0";

        GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");
        MiniBoss[] livingBosses = FindObjectsByType<MiniBoss>(FindObjectsSortMode.None);

        bool hasLivingCells = cells.Length > 0;
        bool hasLivingBosses = livingBosses.Length > 0;

        if (hasLivingCells || hasLivingBosses)
        {
            adaptationLevel = Mathf.Clamp01(adaptationLevel + adaptationIncreasePerFailedRound);

            ConsumeMahoragaTurn();
            ShowMahoragaUI();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMahoraga();

            if (mahoragaController != null)
                mahoragaController.PlaySequence(adaptationSequenceDuration);

            if (hasLivingCells)
            {
                foreach (GameObject cellObj in cells)
                {
                    Cell c = cellObj.GetComponent<Cell>();
                    if (c != null)
                        c.BeginAdaptationSequence(GetFinalAdaptedColor(), adaptationSequenceDuration);
                }
            }

            if (hasLivingBosses)
            {
                // Penalización: -5 por jefe no eliminado a tiempo, sin bajar de 0
                AddScore(-5);

                foreach (MiniBoss boss in livingBosses)
                {
                    if (boss != null)
                        boss.BeginMahoragaElimination(adaptationSequenceDuration);
                }
            }

            yield return new WaitForSeconds(adaptationSequenceDuration);

            HideMahoragaUI();
        }

        currentRound++;

        if (roundText != null)
            roundText.text = "ROUND: " + currentRound;

        StartRound();
        isRoundTransitioning = false;
    }

    void ConsumeMahoragaTurn()
    {
        if (mahoragaRemainingTurns > 0)
            mahoragaRemainingTurns--;

        if (mahoragaRemainingTurns <= 0)
        {
            mahoragaRemainingTurns = 0;
            adaptationLevel = 1f;
        }
    }

    void ShowMahoragaUI()
    {
        if (adaptText != null)
        {
            adaptText.text = "ADAPTANDOSE...";
            adaptText.gameObject.SetActive(true);
        }

        if (mahoragaTurnsText != null)
        {
            if (mahoragaRemainingTurns <= 0)
                mahoragaTurnsText.text = "SE HA ADAPTADO POR COMPLETO";
            else
                mahoragaTurnsText.text = "QUEDAN " + mahoragaRemainingTurns + " GIROS";

            mahoragaTurnsText.gameObject.SetActive(true);
        }
    }

    void HideMahoragaUI()
    {
        if (adaptText != null)
            adaptText.gameObject.SetActive(false);

        if (mahoragaTurnsText != null)
            mahoragaTurnsText.gameObject.SetActive(false);
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }

    public bool IsRoundTransitioning()
    {
        return isRoundTransitioning;
    }

    public int GetMahoragaRemainingTurns()
    {
        return mahoragaRemainingTurns;
    }

    public bool IsFullyAdapted()
    {
        return mahoragaRemainingTurns <= 0;
    }

    public Color GetFinalAdaptedColor()
    {
        return fullyAdaptedFinalColor;
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (score < 0)
            score = 0;

        if (scoreText != null)
            scoreText.text = "SCORE: " + score;

        UpdateTrophyVisual(true);
    }

    int GetTrophyTier(int currentScore)
    {
        if (currentScore <= 30) return 0;
        if (currentScore <= 60) return 1;
        if (currentScore <= 90) return 2;
        if (currentScore <= 120) return 3;
        return 4;
    }

    void UpdateTrophyVisual(bool canPlayTierSfx)
    {
        int currentTier = GetTrophyTier(score);

        if (trophyImage != null)
        {
            switch (currentTier)
            {
                case 0:
                    trophyImage.enabled = false;
                    break;
                case 1:
                    trophyImage.enabled = true;
                    trophyImage.sprite = bronzeTrophy;
                    break;
                case 2:
                    trophyImage.enabled = true;
                    trophyImage.sprite = silverTrophy;
                    break;
                case 3:
                    trophyImage.enabled = true;
                    trophyImage.sprite = goldTrophy;
                    break;
                case 4:
                    trophyImage.enabled = true;
                    trophyImage.sprite = finalTrophy;
                    break;
            }
        }

        if (trophyFireController != null)
            trophyFireController.SetTier(score);

        if (canPlayTierSfx && currentTier > lastTrophyTier && currentTier > 0)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayBonus();
        }

        lastTrophyTier = currentTier;
    }

    public void RegisterKilledCellColor(Color color)
    {
        killedColorHistory.Add(color);

        if (killedColorHistory.Count > maxStoredKilledColors)
            killedColorHistory.RemoveAt(0);
    }

    Color GetWarmOnlyBaseColor()
    {
        Color[] warmPalette = new Color[]
        {
            new Color32(220, 60, 60, 255),
            new Color32(220, 60, 60, 255),
            new Color32(235, 70, 50, 255),
            new Color32(255, 210, 70, 255),
            new Color32(255, 210, 70, 255),
            new Color32(255, 180, 70, 255),
            new Color32(255, 145, 60, 255),
            new Color32(255, 130, 50, 255),
            new Color32(190, 70, 120, 255),
            new Color32(120, 80, 50, 255)
        };

        return warmPalette[Random.Range(0, warmPalette.Length)];
    }

    Color GetNormalBaseColor()
    {
        Color[] normalPalette = new Color[]
        {
            new Color32(220, 60, 60, 255),
            new Color32(255, 210, 70, 255),
            new Color32(160, 90, 220, 255),
            new Color32(50, 50, 50, 255),
            new Color32(235, 235, 235, 255),
            new Color32(120, 80, 50, 255),
            new Color32(255, 145, 60, 255),
            new Color32(190, 70, 120, 255)
        };

        return normalPalette[Random.Range(0, normalPalette.Length)];
    }

    int GetMahoragaHalfTrigger()
    {
        return Mathf.CeilToInt(mahoragaMaxTurns / 2f);
    }

    bool IsMahoragaBluePhase()
    {
        int usedTurns = mahoragaMaxTurns - mahoragaRemainingTurns;
        int halfTrigger = GetMahoragaHalfTrigger();
        return usedTurns >= halfTrigger;
    }

    public Color GetSpawnColor()
    {
        if (mahoragaRemainingTurns <= 0)
            return GetFinalAdaptedColor();

        bool warmOnlyPhase = currentRound <= warmOnlyRounds && !IsMahoragaBluePhase();

        if (warmOnlyPhase)
            return GetWarmOnlyBaseColor();

        Color randomBase = GetNormalBaseColor();
        Color normalColor = randomBase;

        if (killedColorHistory.Count > 0)
        {
            Color avgKilled = Color.black;

            for (int i = 0; i < killedColorHistory.Count; i++)
                avgKilled += killedColorHistory[i];

            avgKilled /= killedColorHistory.Count;

            normalColor = Color.Lerp(randomBase, avgKilled, 0.12f);
        }

        normalColor = Color.Lerp(normalColor, backgroundTargetColor, adaptationLevel * 0.18f);

        if (!IsMahoragaBluePhase())
            return normalColor;

        int usedTurns = mahoragaMaxTurns - mahoragaRemainingTurns;
        int halfTrigger = GetMahoragaHalfTrigger();
        int bluePhaseSteps = Mathf.Max(1, mahoragaMaxTurns - halfTrigger);
        int progressInBluePhase = usedTurns - halfTrigger + 1;
        float mahoragaProgress = Mathf.Clamp01((float)progressInBluePhase / bluePhaseSteps);

        Color blueishTarget = Color.Lerp(mahoragaMidPhaseColor, backgroundTargetColor, 0.55f);
        float extraBlend = Mathf.Lerp(0.25f, mahoragaMaxBlend, mahoragaProgress);

        return Color.Lerp(normalColor, blueishTarget, extraBlend);
    }

    public Rect GetTrophyWorldBlockRect()
    {
        if (trophyImage == null || !trophyImage.enabled)
            return new Rect(9999, 9999, 0, 0);

        RectTransform rt = trophyImage.rectTransform;
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Camera cam = Camera.main;
        if (cam == null)
            return new Rect(9999, 9999, 0, 0);

        Vector3 bl = cam.ScreenToWorldPoint(cam.WorldToScreenPoint(corners[0]));
        Vector3 tr = cam.ScreenToWorldPoint(cam.WorldToScreenPoint(corners[2]));

        return Rect.MinMaxRect(bl.x, bl.y, tr.x, tr.y);
    }
}