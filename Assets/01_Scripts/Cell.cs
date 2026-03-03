using UnityEngine;

public class Cell : MonoBehaviour
{
    private float moveSpeed;
    private Vector2 moveDirection;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        int round = GameManager.Instance.GetCurrentRound();

        // 🔹 Velocidad escala por ronda
        moveSpeed = 1.5f + (round * 0.3f);

        // 🔹 Dirección aleatoria
        moveDirection = Random.insideUnitCircle.normalized;

        // 🔹 Escala disminuye con la ronda
        float scale = Mathf.Max(0.5f, 1f - (round * 0.05f));
        transform.localScale = new Vector3(scale, scale, 1f);

        // 🔹 Color evolutivo (más rojo por ronda)
        float redIntensity = Mathf.Clamp01(0.5f + (round * 0.05f));
        spriteRenderer.color = new Color(redIntensity, 0.2f, 0.2f);
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver()) return;

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Rebote simple en bordes
        if (Mathf.Abs(transform.position.x) > 8f)
            moveDirection.x *= -1;

        if (Mathf.Abs(transform.position.y) > 4.5f)
            moveDirection.y *= -1;
    }

    void OnMouseDown()
    {
        GameManager.Instance.AddScore(1);
        Destroy(gameObject);
    }
}