using UnityEngine;

public static class ClickFeedback
{
    public static float speedMult = 1f;
    public static float sizeMult = 1f;

    // 0 = normal, 1 = full camuflaje
    public static float camouflage = 0f;

    // 0 = opacas, 1 = máxima transparencia (hasta 60%)
    public static float transparency = 0f;

    public static int version = 0;

    // Castigo al fallar
    private const float SPEED_UP_ON_MISS = 0.08f;
    private const float SIZE_DOWN_ON_MISS = 0.05f;

    // Recompensa al matar (notoria)
    private const float SPEED_DOWN_ON_KILL = 0.16f;
    private const float SIZE_UP_ON_KILL = 0.10f;

    // Camuflaje (color)
    private const float CAMO_UP_ON_MISS = 0.18f;
    private const float CAMO_DOWN_ON_KILL = 0.12f;

    // Transparencia (alpha)
    private const float TRANS_UP_ON_MISS = 0.18f;   // más transparente al fallar
    private const float TRANS_DOWN_ON_KILL = 0.22f; // menos transparente al matar

    private const float MIN_SIZE_MULT = 0.5f;
    private const float MIN_SPEED_MULT = 1.0f;
    private const float MAX_SPEED_MULT = 1.65f;

    public static void MissClick()
    {
        speedMult = Mathf.Min(MAX_SPEED_MULT, speedMult + SPEED_UP_ON_MISS);
        sizeMult = Mathf.Max(MIN_SIZE_MULT, sizeMult - SIZE_DOWN_ON_MISS);

        camouflage = Mathf.Clamp01(camouflage + CAMO_UP_ON_MISS);
        transparency = Mathf.Clamp01(transparency + TRANS_UP_ON_MISS);

        version++;
    }

    public static void RewardKill()
    {
        speedMult = Mathf.Max(MIN_SPEED_MULT, speedMult - SPEED_DOWN_ON_KILL);
        sizeMult = Mathf.Min(1f, sizeMult + SIZE_UP_ON_KILL);

        camouflage = Mathf.Clamp01(camouflage - CAMO_DOWN_ON_KILL);
        transparency = Mathf.Clamp01(transparency - TRANS_DOWN_ON_KILL);

        version++;
    }

    // ✅ Llamar al reiniciar para que todo vuelva a normal
    public static void ResetAll()
    {
        speedMult = 1f;
        sizeMult = 1f;
        camouflage = 0f;
        transparency = 0f;
        version++;
    }
}