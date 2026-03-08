using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverPanelController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI titleText;   // GAME OVER
    public Image badgeImage;            // imagen del centro

    [Header("Badges (PNGs)")]
    public Sprite rondaBaja;            // Ronda 1-3
    public Sprite rondaMedia;           // Ronda 4-6
    public Sprite rondaAlta;            // Ronda 7-9
    public Sprite rondaMuyAlta;         // Ronda 10+

    [Header("Animación título")]
    public float moveAmplitude = 12f;   // píxeles
    public float moveSpeed = 1.5f;

    private RectTransform titleRect;
    private Vector2 titleStartPos;

    void Awake()
    {
        if (titleText != null)
        {
            titleRect = titleText.rectTransform;
            titleStartPos = titleRect.anchoredPosition;
        }
    }

    void OnEnable()
    {
        // Cuando se activa el panel, elige imagen por ronda
        if (GameManager.Instance != null)
            SetBadgeForRound(GameManager.Instance.GetCurrentRound());
    }

    void Update()
    {
        // animación suave izquierda-derecha
        if (titleRect != null)
        {
            float x = Mathf.Sin(Time.unscaledTime * moveSpeed) * moveAmplitude;
            titleRect.anchoredPosition = titleStartPos + new Vector2(x, 0f);
        }
    }

    public void SetBadgeForRound(int round)
    {
        if (badgeImage == null) return;

        Sprite chosen = null;

        if (round <= 3) chosen = rondaBaja;
        else if (round <= 6) chosen = rondaMedia;
        else if (round <= 9) chosen = rondaAlta;
        else chosen = rondaMuyAlta;

        badgeImage.sprite = chosen;
        badgeImage.enabled = (chosen != null);
    }
}