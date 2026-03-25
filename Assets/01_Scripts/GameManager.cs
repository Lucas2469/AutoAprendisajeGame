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

    [Header("Trofeo fijo")]
    public Image trophyImage;
    public Sprite bronzeTrophy;
    public Sprite silverTrophy;
    public Sprite goldTrophy;
    public Sprite finalTrophy;
    public TrophyFireController trophyFireController;

    [Header("Round Settings")]
    public float roundDuration = 10f;

    [Header("Animación Mahoraga")]
    public MahoragaController mahoragaAnimation;
    public float adaptationSequenceDuration = 4f;

    // Umbral de similitud para adaptación total (privado)
    private float colorSimilarityThreshold = 0.05f;

    // Sistema de aprendizaje
    private Color averageSurvivorColor = Color.white;
    private int totalSurvivorsProcessed = 0;
    private bool hasSurvivorsEver = false;

    // Adaptación total
    private int killsThisRound = 0;
    private bool isFullyAdapted = false;
    private Color fixedSpawnColor = Color.white;

    private int score = 0;
    private int currentRound = 1;
    private float currentTime;
    private bool isRoundTransitioning = false;
    private int lastTrophyTier = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (scoreText != null) scoreText.text = "SCORE: 0";
        if (roundText != null) roundText.text = "ROUND: 1";
        if (timerText != null) timerText.text = "TIME: " + Mathf.Ceil(roundDuration).ToString();

        UpdateTrophyVisual(false);
        if (mahoragaAnimation != null) mahoragaAnimation.Hide();

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
        killsThisRound = 0;
        if (timerText != null)
            timerText.text = "TIME: " + Mathf.Ceil(currentTime).ToString();
    }

    IEnumerator ResolveRound()
    {
        if (isRoundTransitioning) yield break;
        isRoundTransitioning = true;

        currentTime = 0f;
        if (timerText != null) timerText.text = "TIME: 0";

        // Obtener listas y filtrar objetos nulos/destruidos
        GameObject[] allCells = GameObject.FindGameObjectsWithTag("Cell");
        MiniBoss[] allBosses = FindObjectsByType<MiniBoss>(FindObjectsSortMode.None);

        List<GameObject> cells = new List<GameObject>();
        foreach (var cellObj in allCells)
        {
            if (cellObj != null) cells.Add(cellObj);
        }

        List<MiniBoss> livingBosses = new List<MiniBoss>();
        foreach (var boss in allBosses)
        {
            if (boss != null) livingBosses.Add(boss);
        }

        bool hasLivingCells = cells.Count > 0;
        bool hasLivingBosses = livingBosses.Count > 0;
        bool anySurvivor = hasLivingCells || hasLivingBosses;

        // Detección de adaptación total
        if (!isFullyAdapted && killsThisRound == 0 && hasLivingCells)
        {
            Color sum = Color.black;
            int count = 0;
            foreach (GameObject cellObj in cells)
            {
                if (cellObj == null) continue;
                Cell c = cellObj.GetComponent<Cell>();
                if (c != null && c.GetSpriteRenderer() != null)
                {
                    sum += c.GetSpriteRenderer().color;
                    count++;
                }
            }
            if (count > 0)
            {
                Color avg = sum / count;
                float maxDev = 0f;
                foreach (GameObject cellObj in cells)
                {
                    if (cellObj == null) continue;
                    Cell c = cellObj.GetComponent<Cell>();
                    if (c != null && c.GetSpriteRenderer() != null)
                    {
                        Color col = c.GetSpriteRenderer().color;
                        float dev = Mathf.Abs(col.r - avg.r) + Mathf.Abs(col.g - avg.g) + Mathf.Abs(col.b - avg.b);
                        if (dev > maxDev) maxDev = dev;
                    }
                }
                if (maxDev < colorSimilarityThreshold)
                {
                    isFullyAdapted = true;
                    fixedSpawnColor = avg;
                    Debug.Log("¡Adaptación total alcanzada! Color fijo: " + fixedSpawnColor);
                }
            }
        }

        // Aprendizaje a partir de células vivas
        if (hasLivingCells)
        {
            Color sum = Color.black;
            int count = 0;
            foreach (GameObject cellObj in cells)
            {
                if (cellObj == null) continue;
                Cell c = cellObj.GetComponent<Cell>();
                if (c != null && c.GetSpriteRenderer() != null)
                {
                    sum += c.GetSpriteRenderer().color;
                    count++;
                }
            }
            if (count > 0)
            {
                Color average = sum / count;
                if (!hasSurvivorsEver)
                {
                    averageSurvivorColor = average;
                    totalSurvivorsProcessed = count;
                    hasSurvivorsEver = true;
                }
                else
                {
                    int newTotal = totalSurvivorsProcessed + count;
                    averageSurvivorColor = (averageSurvivorColor * totalSurvivorsProcessed + sum) / newTotal;
                    totalSurvivorsProcessed = newTotal;
                }
            }
        }

        // Animación de Mahoraga si hubo supervivientes
        if (anySurvivor && mahoragaAnimation != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMahoraga();

            mahoragaAnimation.PlaySequence(adaptationSequenceDuration);
            yield return new WaitForSeconds(adaptationSequenceDuration);
            mahoragaAnimation.Hide();
        }
        else
        {
            yield return null;
        }

        // Volver a obtener las células que aún existen (algunas pudieron ser destruidas por explosión del miniboss)
        GameObject[] survivingCells = GameObject.FindGameObjectsWithTag("Cell");
        List<GameObject> finalCells = new List<GameObject>();
        foreach (var cellObj in survivingCells)
        {
            if (cellObj != null) finalCells.Add(cellObj);
        }

        // Aplicar animación de adaptación a las células que aún viven
        foreach (GameObject cellObj in finalCells)
        {
            if (cellObj == null) continue; // Evitar objetos destruidos
            Cell c = cellObj.GetComponent<Cell>();
            if (c != null)
                c.BeginAdaptationSequence(GetSpawnColor(), adaptationSequenceDuration);
        }

        // Miniboss: penalización y eliminación
        if (hasLivingBosses)
        {
            AddScore(-5);
            foreach (MiniBoss boss in livingBosses)
            {
                if (boss != null)
                    boss.BeginMahoragaElimination(adaptationSequenceDuration);
            }
        }

        // Avanzar ronda
        currentRound++;
        if (roundText != null) roundText.text = "ROUND: " + currentRound;

        StartRound();
        isRoundTransitioning = false;
    }

    public void EndRoundEarly()
    {
        if (isRoundTransitioning) return;
        StartCoroutine(ResolveRound());
    }

    public void RegisterCellKill()
    {
        killsThisRound++;
    }

    public int GetCurrentRound() => currentRound;
    public bool IsRoundTransitioning() => isRoundTransitioning;

    public void AddScore(int amount)
    {
        score += amount;
        if (score < 0) score = 0;
        if (scoreText != null) scoreText.text = "SCORE: " + score;
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
            trophyImage.enabled = currentTier > 0;
            switch (currentTier)
            {
                case 1: trophyImage.sprite = bronzeTrophy; break;
                case 2: trophyImage.sprite = silverTrophy; break;
                case 3: trophyImage.sprite = goldTrophy; break;
                case 4: trophyImage.sprite = finalTrophy; break;
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

    public Color GetSpawnColor()
    {
        if (isFullyAdapted)
            return fixedSpawnColor;

        if (hasSurvivorsEver)
        {
            float variation = 0.12f;
            return new Color(
                Mathf.Clamp01(averageSurvivorColor.r + Random.Range(-variation, variation)),
                Mathf.Clamp01(averageSurvivorColor.g + Random.Range(-variation, variation)),
                Mathf.Clamp01(averageSurvivorColor.b + Random.Range(-variation, variation)),
                1f
            );
        }

        return new Color(
            Random.Range(0.2f, 1f),
            Random.Range(0.2f, 1f),
            Random.Range(0.2f, 1f),
            1f
        );
    }

    public Rect GetTrophyWorldBlockRect()
    {
        if (trophyImage == null || !trophyImage.enabled)
            return new Rect(9999, 9999, 0, 0);

        RectTransform rt = trophyImage.rectTransform;
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Camera cam = Camera.main;
        if (cam == null) return new Rect(9999, 9999, 0, 0);

        Vector3 bl = cam.ScreenToWorldPoint(cam.WorldToScreenPoint(corners[0]));
        Vector3 tr = cam.ScreenToWorldPoint(cam.WorldToScreenPoint(corners[2]));
        return Rect.MinMaxRect(bl.x, bl.y, tr.x, tr.y);
    }
}