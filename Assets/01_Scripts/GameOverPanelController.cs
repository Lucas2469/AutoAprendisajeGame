using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverPanelController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI titleText;
    public Image badgeImage;

    [Header("Badges (PNGs)")]
    public Sprite rondaBaja;
    public Sprite rondaMedia;
    public Sprite rondaAlta;
    public Sprite rondaMuyAlta;

    [Header("Animación título")]
    public float moveAmplitude = 12f;
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
        if (GameManager.Instance != null)
            SetBadgeForRound(GameManager.Instance.GetCurrentRound());
    }

    void Update()
    {
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