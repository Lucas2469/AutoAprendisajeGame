using System.Collections;
using UnityEngine;

public class MiniBoss : MonoBehaviour
{
    [Header("Vida")]
    public int baseHealth = 2;
    private int currentHealth;
    private int maxHealth;

    [Header("Fase")]
    private bool isEnraged = false;
    private bool isBeingMahoragaKilled = false;

    [Header("Referencias")]
    public Sprite normalSprite;
    public Sprite enragedSprite;
    public GameObject explosionPrefab;

    [Header("Barra Vida")]
    public Transform healthFill;

    [Header("Movimiento")]
    public Vector2 bounds = new Vector2(8f, 4.5f);

    [Header("Velocidad")]
    public float baseMoveSpeed = 1.4f;
    public float speedPerRound = 0.04f;

    private float moveSpeed;
    private Vector2 moveDirection;

    private Vector3 baseScale;
    private float enragedScaleMult = 1f;

    private SpriteRenderer sr;
    private Collider2D col2D;
    private Collider col3D;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col2D = GetComponent<Collider2D>();
        col3D = GetComponent<Collider>();

        moveDirection = Random.insideUnitCircle.normalized;

        int round = (GameManager.Instance != null) ? GameManager.Instance.GetCurrentRound() : 1;

        maxHealth = baseHealth;
        currentHealth = maxHealth;

        moveSpeed = baseMoveSpeed + (round * speedPerRound);

        if (normalSprite != null)
            sr.sprite = normalSprite;

        baseScale = transform.localScale;

        UpdateHealthBar();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsRoundTransitioning()) return;
        if (isBeingMahoragaKilled) return;

        Move();
        UpdateHealthBar();
        CheckPhase();
    }

    void Move()
    {
        transform.localScale = baseScale * enragedScaleMult;

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x) > bounds.x)
            moveDirection.x *= -1;

        if (Mathf.Abs(transform.position.y) > bounds.y)
            moveDirection.y *= -1;
    }

    void OnMouseDown()
    {
        if (isBeingMahoragaKilled) return;
        TakeDamage(1);
    }

    void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
            DieByPlayer();
    }

    void CheckPhase()
    {
        if (!isEnraged && currentHealth <= maxHealth / 2)
        {
            isEnraged = true;

            if (enragedSprite != null)
                sr.sprite = enragedSprite;

            enragedScaleMult = 1.15f;
            moveSpeed *= 1.15f;
        }
    }

    void UpdateHealthBar()
    {
        if (healthFill == null) return;

        float ratio = Mathf.Clamp01((float)currentHealth / maxHealth);
        healthFill.localScale = new Vector3(ratio, 1f, 1f);
    }

    // Dentro del método DieByPlayer(), añadir la línea:
    void DieByPlayer()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBossKill();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(10);
            // Terminar la ronda inmediatamente
            GameManager.Instance.EndRoundEarly();
        }

        ExplodeAndClearCells();
        Destroy(gameObject);
    }

    public void BeginMahoragaElimination(float duration)
    {
        if (isBeingMahoragaKilled) return;
        StartCoroutine(MahoragaEliminationRoutine(duration));
    }

    IEnumerator MahoragaEliminationRoutine(float duration)
    {
        isBeingMahoragaKilled = true;

        if (col2D != null) col2D.enabled = false;
        if (col3D != null) col3D.enabled = false;

        Vector3 startScale = transform.localScale;
        Vector3 basePos = transform.position;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            float shakeX = Mathf.Sin(timer * 30f) * 0.04f;
            float shakeY = Mathf.Cos(timer * 24f) * 0.04f;
            transform.position = basePos + new Vector3(shakeX, shakeY, 0f);

            float pulse = 1f + Mathf.Sin(timer * 14f) * 0.04f;
            transform.localScale = Vector3.Lerp(startScale, startScale * 0.85f, t) * pulse;

            yield return null;
        }

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