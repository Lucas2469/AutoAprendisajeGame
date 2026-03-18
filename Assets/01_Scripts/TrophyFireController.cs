using UnityEngine;
using UnityEngine.UI;

public class TrophyFireController : MonoBehaviour
{
    public Image fireImage;

    [Header("Bronze Fire Frames")]
    public Sprite[] bronzeFrames;

    [Header("Silver Fire Frames")]
    public Sprite[] silverFrames;

    [Header("Gold Fire Frames")]
    public Sprite[] goldFrames;

    [Header("Final Fire Frames")]
    public Sprite[] finalFrames;

    public float frameRate = 0.12f;

    private Sprite[] currentFrames;
    private int currentFrame;
    private float timer;

    void Update()
    {
        if (fireImage == null || currentFrames == null || currentFrames.Length == 0)
            return;

        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer = 0f;
            currentFrame++;
            if (currentFrame >= currentFrames.Length)
                currentFrame = 0;

            fireImage.sprite = currentFrames[currentFrame];
        }
    }

    public void SetTier(int score)
    {
        if (score <= 30)
        {
            fireImage.enabled = false;
            currentFrames = null;
            return;
        }

        fireImage.enabled = true;

        if (score <= 60)
            currentFrames = bronzeFrames;
        else if (score <= 90)
            currentFrames = silverFrames;
        else if (score <= 120)
            currentFrames = goldFrames;
        else
            currentFrames = finalFrames;

        currentFrame = 0;
        timer = 0f;

        if (currentFrames != null && currentFrames.Length > 0)
            fireImage.sprite = currentFrames[0];
    }
}