using System.Collections;
using UnityEngine;

public class Cell : MonoBehaviour
{
    private float moveSpeed;
    private Vector2 moveDirection;
    private SpriteRenderer spriteRenderer;
    private Collider2D col2D;
    private Collider col3D;

    [Header("Movimiento lento")]
    public float minSlowSpeed = 0.12f;
    public float maxSlowSpeed = 0.22f;

    [Header("Escala base")]
    public float minScale = 0.75f;
    public float maxScale = 1.0f;

    private bool isAdapting = false;
    private Vector3 originalScale;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col2D = GetComponent<Collider2D>();
        col3D = GetComponent<Collider>();

        moveDirection = Random.insideUnitCircle.normalized;
        moveSpeed = Random.Range(minSlowSpeed, maxSlowSpeed);

        float scale = Random.Range(minScale, maxScale);
        transform.localScale = new Vector3(scale, scale, 1f);
        originalScale = transform.localScale;

        if (GameManager.Instance != null && spriteRenderer != null)
        {
            spriteRenderer.color = GameManager.Instance.GetSpawnColor();
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsRoundTransitioning()) return;
        if (isAdapting) return;

        Move();
    }

    void Move()
    {
        Vector3 nextPos = transform.position + (Vector3)(moveDirection * moveSpeed * Time.deltaTime);

        if (Mathf.Abs(nextPos.x) > 8f) moveDirection.x *= -1;
        if (Mathf.Abs(nextPos.y) > 4.5f) moveDirection.y *= -1;

        Rect blockedRect = GameManager.Instance.GetTrophyWorldBlockRect();
        if (blockedRect.Contains(nextPos))
        {
            moveDirection = -moveDirection;
            nextPos = transform.position + (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
        }

        transform.position = nextPos;
    }

    public void BeginAdaptationSequence(Color targetColor, float duration)
    {
        if (!gameObject.activeInHierarchy) return;
        if (isAdapting) return;
        StartCoroutine(AdaptationRoutine(targetColor, duration));
    }

    IEnumerator AdaptationRoutine(Color targetColor, float duration)
    {
        isAdapting = true;

        if (col2D != null) col2D.enabled = false;
        if (col3D != null) col3D.enabled = false;

        Color startColor = (spriteRenderer != null) ? spriteRenderer.color : Color.white;
        Vector3 startScale = transform.localScale;
        Vector3 basePos = transform.position;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(startColor, targetColor, t);

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            float shakeX = Mathf.Sin(timer * 35f) * 0.03f;
            float shakeY = Mathf.Cos(timer * 28f) * 0.03f;
            transform.position = basePos + new Vector3(shakeX, shakeY, 0f);

            yield return null;
        }

        Destroy(gameObject);
    }

    void OnMouseDown()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsRoundTransitioning()) return;
        if (isAdapting) return;
        if (spriteRenderer == null) return;

        // Registrar la muerte para el contador de la ronda
        GameManager.Instance.RegisterCellKill();

        GameManager.Instance.AddScore(1);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCellKill();

        Destroy(gameObject);
    }

    // Método auxiliar para que GameManager pueda obtener el color actual
    public SpriteRenderer GetSpriteRenderer() => spriteRenderer;
}