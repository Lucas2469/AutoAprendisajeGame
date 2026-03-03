using UnityEngine;

public class MiniBoss : MonoBehaviour
{
    [Header("Vida")]
    public int baseHealth = 20;
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

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager no encontrado.");
            return;
        }

        int round = GameManager.Instance.GetCurrentRound();

        maxHealth = baseHealth + round * 5;
        currentHealth = maxHealth;

        if (normalSprite != null)
            sr.sprite = normalSprite;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsGameOver()) return;

        UpdateHealthBar();
        CheckPhase();
    }

    void OnMouseDown()
    {
        TakeDamage(1);
    }

    void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
            Die();
    }

    void CheckPhase()
    {
        if (!isEnraged && currentHealth <= maxHealth / 2)
        {
            isEnraged = true;

            if (enragedSprite != null)
                sr.sprite = enragedSprite;

            transform.localScale *= 1.2f;
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
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(50);

        ExplodeAndClearCells();
        Destroy(gameObject);
    }

    void ExplodeAndClearCells()
    {
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");

        foreach (GameObject cell in cells)
        {
            if (Vector2.Distance(transform.position, cell.transform.position) < 3f)
            {
                Destroy(cell);
            }
        }
    }
}