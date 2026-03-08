using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour
{
    private float moveSpeed;
    private Vector2 moveDirection;
    private SpriteRenderer spriteRenderer;

    [Header("Escalado por ronda")]
    public float baseSpeed = 1.5f;
    public float speedPerRound = 0.3f;

    public float baseScale = 1f;
    public float scaleShrinkPerRound = 0.05f;
    public float minScale = 0.5f;

    [Header("Color por ronda (sin azul al inicio)")]
    public int blueUnlockRound = 8;
    public int blueFullRound = 20;
    public float blueHueMin = 0.55f;
    public float blueHueMax = 0.70f;

    [Header("Camuflaje por fallos (CYAN -> CELESTE -> AZUL MARINO)")]
    public Color camoCyan = new Color(0.0f, 0.95f, 1.0f);   // cyan
    public Color camoSky = new Color(0.45f, 0.85f, 1.0f);  // celeste claro
    public Color camoNavy = new Color(0.05f, 0.10f, 0.35f); // azul marino

    [Header("Transparencia por fallos")]
    [Range(0.05f, 1f)]
    public float minAlpha = 0.40f; // ✅ 0.40 = 60% transparente (límite)

    [Header("Invisibilidad aleatoria")]
    public float minInvisibleInterval = 2f;
    public float maxInvisibleInterval = 5f;
    public float minInvisibleDuration = 0.5f;
    public float maxInvisibleDuration = 1.0f;

    private int lastRoundApplied = -1;
    private int lastFeedbackVersion = -1;

    private Color baseColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        moveDirection = Random.insideUnitCircle.normalized;

        ApplyStatsIfNeeded(force: true);
        StartCoroutine(RandomInvisibilityLoop());
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsGameOver()) return;

        ApplyStatsIfNeeded(force: false);

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x) > 8f) moveDirection.x *= -1;
        if (Mathf.Abs(transform.position.y) > 4.5f) moveDirection.y *= -1;
    }

    void ApplyStatsIfNeeded(bool force)
    {
        int round = GameManager.Instance.GetCurrentRound();

        bool roundChanged = (round != lastRoundApplied);
        bool feedbackChanged = (ClickFeedback.version != lastFeedbackVersion);

        if (!force && !roundChanged && !feedbackChanged) return;

        lastRoundApplied = round;
        lastFeedbackVersion = ClickFeedback.version;

        // Velocidad con multiplicador global (cappeado en ClickFeedback)
        moveSpeed = (baseSpeed + (round * speedPerRound)) * ClickFeedback.speedMult;

        // Tamaño con multiplicador global (nunca baja de minScale)
        float baseRoundScale = Mathf.Max(minScale, baseScale - (round * scaleShrinkPerRound));
        float finalScale = Mathf.Max(minScale, baseRoundScale * ClickFeedback.sizeMult);
        transform.localScale = new Vector3(finalScale, finalScale, 1f);

        // Color base solo cuando cambia la ronda
        if (roundChanged || force)
            baseColor = GenerateRoundColor(round);

        // 1) camuflaje por color
        Color c = ApplyCamouflage(baseColor);

        // 2) transparencia por fallos (alpha baja hasta minAlpha)
        float t = Mathf.Clamp01(ClickFeedback.transparency);
        float a = Mathf.Lerp(1f, minAlpha, t);
        c.a = a;

        spriteRenderer.color = c;
    }

    Color ApplyCamouflage(Color original)
    {
        float t = Mathf.Clamp01(ClickFeedback.camouflage);

        // Gradiente en 2 tramos:
        // 0 -> 0.5 : cyan -> celeste
        // 0.5 -> 1 : celeste -> navy
        Color camo;
        if (t < 0.5f)
            camo = Color.Lerp(camoCyan, camoSky, t / 0.5f);
        else
            camo = Color.Lerp(camoSky, camoNavy, (t - 0.5f) / 0.5f);

        return Color.Lerp(original, camo, t);
    }

    Color GenerateRoundColor(int round)
    {
        float blueChance = 0f;
        if (round >= blueUnlockRound)
            blueChance = Mathf.InverseLerp(blueUnlockRound, blueFullRound, round);

        float hue;
        bool pickBlue = (Random.value < blueChance);

        if (pickBlue)
        {
            hue = Random.Range(blueHueMin, blueHueMax);
        }
        else
        {
            // Split range para evitar azules al inicio
            if (Random.value < 0.5f)
                hue = Random.Range(0f, blueHueMin);
            else
                hue = Random.Range(blueHueMax, 1f);
        }

        float sat = Random.Range(0.65f, 1f);
        float val = Random.Range(0.75f, 1f);
        return Color.HSVToRGB(hue, sat, val);
    }

    void OnMouseDown()
    {
        if (spriteRenderer != null && !spriteRenderer.enabled) return;

        // Recompensa global al matar: menos transparencia + menos camuflaje + menos speed + más size
        ClickFeedback.RewardKill();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCellKill();

        GameManager.Instance.AddScore(1);
        Destroy(gameObject);
    }

    IEnumerator RandomInvisibilityLoop()
    {
        while (true)
        {
            float wait = Random.Range(minInvisibleInterval, maxInvisibleInterval);
            yield return new WaitForSeconds(wait);

            if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
                yield break;

            spriteRenderer.enabled = false;

            float duration = Random.Range(minInvisibleDuration, maxInvisibleDuration);
            yield return new WaitForSeconds(duration);

            if (spriteRenderer != null)
                spriteRenderer.enabled = true;
        }
    }
}