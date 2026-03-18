using System.Collections;
using UnityEngine;

public class MahoragaController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] frames;
    public float frameRate = 0.08f;
    public Vector3 worldPosition = Vector3.zero;
    public Vector3 displayScale = new Vector3(2f, 2f, 1f);

    [Header("Render")]
    public string sortingLayerName = "Default";
    public int sortingOrder = 200;

    private Coroutine playRoutine;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        ApplyRendererSettings();
        Hide();
    }

    void OnValidate()
    {
        ApplyRendererSettings();
    }

    void ApplyRendererSettings()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = sortingOrder;
    }

    public void PlaySequence(float duration)
    {
        if (spriteRenderer == null || frames == null || frames.Length == 0)
        {
            Debug.LogWarning("MahoragaController: faltan SpriteRenderer o frames.");
            return;
        }

        ApplyRendererSettings();

        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayRoutine(duration));
    }

    IEnumerator PlayRoutine(float duration)
    {
        transform.position = worldPosition;
        transform.localScale = displayScale;

        spriteRenderer.enabled = true;
        spriteRenderer.sprite = frames[0];

        float animationTimer = 0f;
        float frameTimer = 0f;
        int frameIndex = 0;

        while (animationTimer < duration)
        {
            animationTimer += Time.deltaTime;
            frameTimer += Time.deltaTime;

            if (frameTimer >= frameRate)
            {
                frameTimer -= frameRate;
                frameIndex = (frameIndex + 1) % frames.Length;
                spriteRenderer.sprite = frames[frameIndex];
            }

            yield return null;
        }

        Hide();
        playRoutine = null;
    }

    public void Hide()
    {
        if (spriteRenderer != null)
        {
            ApplyRendererSettings();
            spriteRenderer.enabled = false;
        }
    }
}