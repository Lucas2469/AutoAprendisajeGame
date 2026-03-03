using UnityEngine;

public class MiniBossExplosion : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 0.5f);
    }
}