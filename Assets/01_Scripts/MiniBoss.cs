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
    public float speedPerRound = 0.08f; // (0.05–0.12 suele ir bien)
    private float moveSpeed;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Dirección aleatoria
        moveDirection = Random.insideUnitCircle.normalized;

        // Ronda actual
        int round = (GameManager.Instance != null) ? GameManager.Instance.GetCurrentRound() : 1;

        // Boss number: ronda 5 => #1, ronda 10 => #2, etc.
        int bossNumber = Mathf.Max(1, round / 5);

        // Vida escalable por cada boss nuevo
        maxHealth = baseHealth + (bossNumber - 1) * healthIncreasePerBoss;
        currentHealth = maxHealth;

        // Velocidad escalable por ronda (aplica también al boss)
        moveSpeed = baseMoveSpeed + (round * speedPerRound);

        if (normalSprite != null)
            sr.sprite = normalSprite;

        UpdateHealthBar();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsGameOver()) return;

        Move();
        UpdateHealthBar();
        CheckPhase();
    }

    void Move()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x) > bounds.x)
            moveDirection.x *= -1;

        if (Mathf.Abs(transform.position.y) > bounds.y)
            moveDirection.y *= -1;
    }

    void OnMouseDown()
    {
        TakeDamage(1); // 1 click = 1 daño
    }

    void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    void CheckPhase()
    {
        // Enrage al bajar a la mitad
        if (!isEnraged && currentHealth <= maxHealth / 2)
        {
            isEnraged = true;

            if (enragedSprite != null)
                sr.sprite = enragedSprite;

            transform.localScale *= 1.15f;
            moveSpeed *= 1.15f; // más rápido al enfadarse
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

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(50);

        ExplodeAndClearCells();
        Destroy(gameObject);
    }

    void ExplodeAndClearCells()
    {
        // Explosión con auto-destrucción por seguridad
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 0.6f);
        }

        // Limpia células cercanas (si quieres que en ronda boss no haya cells, igual sirve por si quedaron)
        GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");
        foreach (GameObject cell in cells)
        {
            if (Vector2.Distance(transform.position, cell.transform.position) < 3f)
                Destroy(cell);
        }
    }
}