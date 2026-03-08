using UnityEngine;

public class ClickMissDetector : MonoBehaviour
{
    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsGameOver()) return;

        if (!Input.GetMouseButtonDown(0)) return;

        if (Camera.main == null) return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        // Click al aire
        if (hit.collider == null)
        {
            ClickFeedback.MissClick();
            return;
        }

        // Si clickeó algo que NO es Cell ni MiniBoss, también lo contamos como miss
        string tag = hit.collider.tag;
        if (tag != "Cell" && tag != "MiniBoss")
        {
            ClickFeedback.MissClick();
        }
    }
}