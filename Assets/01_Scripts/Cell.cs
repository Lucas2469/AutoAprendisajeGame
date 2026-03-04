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
    public int blueUnlockRound = 8;     // desde aquí empiezan a aparecer azules
    public int blueFullRound = 20;      // desde aquí el azul puede salir mucho más
    // Rango aproximado de azules en HSV (0-1)
    public float blueHueMin = 0.55f;
    public float blueHueMax = 0.70f;

    [Header("Invisibilidad aleatoria")]
    public float minInvisibleInterval = 2f;
    public float maxInvisibleInterval = 5f;
    public float minInvisibleDuration = 0.5f;
    public float maxInvisibleDuration = 1.0f;

    private int lastRoundApplied = -1;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        moveDirection = Random.insideUnitCircle.normalized;

        ApplyRoundStats();                 // ✅ aplica stats iniciales
        StartCoroutine(RandomInvisibilityLoop());
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsGameOver()) return;

        // ✅ si cambia la ronda, re-aplicar color/tamaño/velocidad
        ApplyRoundStats();

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Rebote
        if (Mathf.Abs(transform.position.x) > 8f) moveDirection.x *= -1;
        if (Mathf.Abs(transform.position.y) > 4.5f) moveDirection.y *= -1;
    }

    void ApplyRoundStats()
    {
        int round = GameManager.Instance.GetCurrentRound();
        if (round == lastRoundApplied) return;
        lastRoundApplied = round;

        // Velocidad por ronda
        moveSpeed = baseSpeed + (round * speedPerRound);

        // Tamaño por ronda (si quieres que crezcan en vez de encoger, lo invertimos)
        float scale = Mathf.Max(minScale, baseScale - (round * scaleShrinkPerRound));
        transform.localScale = new Vector3(scale, scale, 1f);

        // Color nuevo por ronda
        spriteRenderer.color = GenerateRoundColor(round);
    }

    Color GenerateRoundColor(int round)
    {
        float blueChance = 0f;
        if (round >= blueUnlockRound)
            blueChance = Mathf.InverseLerp(blueUnlockRound, blueFullRound, round); // 0→1

        float hue;
        bool pickBlue = (Random.value < blueChance);

        if (pickBlue)
        {
            // azul permitido (cada vez más probable con la ronda)
            hue = Random.Range(blueHueMin, blueHueMax);
        }
        else
        {
            // evitar azules al inicio: elegimos hue fuera del rango azul
            // (hacemos “split range”)
            if (Random.value < 0.5f)
                hue = Random.Range(0f, blueHueMin);      // desde rojo hasta antes de azul
            else
                hue = Random.Range(blueHueMax, 1f);      // desde después de azul hasta rojo
        }

        float sat = Random.Range(0.65f, 1f);
        float val = Random.Range(0.75f, 1f);

        return Color.HSVToRGB(hue, sat, val);
    }

void OnMouseDown()
{
    if (spriteRenderer != null && !spriteRenderer.enabled) return;

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