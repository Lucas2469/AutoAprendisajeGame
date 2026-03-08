using UnityEngine;

public class MiniBoss : MonoBehaviour
{
    [Header("Vida")]
    public int baseHealth = 2;                 // 2 clicks en el primer boss
    public int healthIncreasePerBoss = 5;      // +vida por cada boss nuevo (ronda 5,10,15...)
    private int currentHealth;
    private int maxHealth;

    [Header("Fase")]
    private bool isEnraged = false;

    [Header("Referencias")]
    public Sprite normalSprite;
    public Sprite enragedSprite;
    public GameObject explosionPrefab;

    [Header("Barra Vida")]
    public Transform healthFill;

    [Header("Movimiento")]
    public Vector2 bounds = new Vector2(8f, 4.5f);
    private Vector2 moveDirection;

    [Header("Escalado por ronda (velocidad)")]
    public float baseMoveSpeed = 2.2f;
    public float speedPerRound = 0.08f;
    private float moveSpeed;

    // Para aplicar tamaño global sin romper el enraged
    private Vector3 baseScale;
    private float enragedScaleMult = 1f;
    private int lastFeedbackVersion = -1;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        moveDirection = Random.insideUnitCircle.normalized;

        int round = (GameManager.Instance != null) ? GameManager.Instance.GetCurrentRound() : 1;
        int bossNumber = Mathf.Max(1, round / 5);

        maxHealth = baseHealth + (bossNumber - 1) * healthIncreasePerBoss;
        currentHealth = maxHealth;

        moveSpeed = baseMoveSpeed + (round * speedPerRound);

        if (normalSprite != null)
            sr.sprite = normalSprite;

        baseScale = transform.localScale;

        ApplyFeedbackScaleIfNeeded(force: true);
        UpdateHealthBar();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsGameOver()) return;

        ApplyFeedbackScaleIfNeeded(force: false);

        Move();
        UpdateHealthBar();
        CheckPhase();
    }

    void ApplyFeedbackScaleIfNeeded(bool force)
    {
        if (!force && lastFeedbackVersion == ClickFeedback.version) return;
        lastFeedbackVersion = ClickFeedback.version;

        float size = Mathf.Max(0.5f, ClickFeedback.sizeMult);
        transform.localScale = baseScale * enragedScaleMult * size;
    }

    void Move()
    {
        float effectiveSpeed = moveSpeed * ClickFeedback.speedMult;
        transform.Translate(moveDirection * effectiveSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x) > bounds.x)
            moveDirection.x *= -1;

        if (Mathf.Abs(transform.position.y) > bounds.y)
            moveDirection.y *= -1;
    }

    void OnMouseDown()
    {
        TakeDamage(1);
    }

    void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    void CheckPhase()
    {
        if (!isEnraged && currentHealth <= maxHealth / 2)
        {
            isEnraged = true;

            if (enragedSprite != null)
                sr.sprite = enragedSprite;

            enragedScaleMult = 1.15f; // ✅ no tocamos localScale directo
            moveSpeed *= 1.15f;       // enrage acelera aparte
            ApplyFeedbackScaleIfNeeded(force: true);
        }
    }

    void UpdateHealthBar()
    {
        if (healthFill == null) return;

        float ratio = Mathf.Clamp01((float)currentHealth / maxHealth);
        healthFill.localScale = new Vector3(ratio, 1f, 1f);
    }

    void Die()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBossKill();

        // ✅ recompensa también por matar boss (opcional, pero recomendable)
        ClickFeedback.RewardKill();

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(50);

        ExplodeAndClearCells();
        Destroy(gameObject);
    }

    void ExplodeAndClearCells()
    {
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 0.6f);
        }

        GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");
        foreach (GameObject cell in cells)
        {
            if (Vector2.Distance(transform.position, cell.transform.position) < 3f)
                Destroy(cell);
        }
    }
}