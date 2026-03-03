using UnityEngine;

public class CellSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject miniBossPrefab;

    [Header("Spawn Settings")]
    public float baseSpawnInterval = 1f;
    public float minSpawnInterval = 0.3f;

    public float spawnAreaX = 8f;
    public float spawnAreaY = 4f;

    [Header("Scaling")]
    public int baseMaxCells = 5;

    private float spawnTimer;
    private float currentSpawnInterval;

    private bool miniBossSpawnedThisRound = false;
    private int lastRoundChecked = -1;

    void Start()
    {
        spawnTimer = baseSpawnInterval;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsGameOver()) return;

        int round = GameManager.Instance.GetCurrentRound();
        float remainingTime = GameManager.Instance.GetRemainingTime();

        // Detectar nueva ronda
        if (round != lastRoundChecked)
        {
            miniBossSpawnedThisRound = false;
            lastRoundChecked = round;
        }

        // 👹 Spawn MiniBoss
        if (round > 0 && round % 5 == 0 && !miniBossSpawnedThisRound)
        {
            SpawnMiniBoss();
            miniBossSpawnedThisRound = true;
        }

        // Bloquear solo células normales al final
        if (remainingTime <= 2f)
            return;

        currentSpawnInterval = Mathf.Max(
            minSpawnInterval,
            baseSpawnInterval - (round * 0.1f)
        );

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            TrySpawnCells(round);
            spawnTimer = currentSpawnInterval;
        }
    }

    void TrySpawnCells(int round)
    {
        GameObject[] currentCells = GameObject.FindGameObjectsWithTag("Cell");

        int maxCellsThisRound = baseMaxCells + (round - 1) * 2;

        if (currentCells.Length >= maxCellsThisRound)
            return;

        float randomX = Random.Range(-spawnAreaX, spawnAreaX);
        float randomY = Random.Range(-spawnAreaY, spawnAreaY);

        Instantiate(cellPrefab, new Vector2(randomX, randomY), Quaternion.identity);
    }

    void SpawnMiniBoss()
    {
        if (miniBossPrefab == null)
        {
            Debug.LogWarning("MiniBoss Prefab no asignado.");
            return;
        }

        // Spawn visible
        Vector2 spawnPosition = new Vector2(0f, 2f);

        GameObject boss = Instantiate(miniBossPrefab, spawnPosition, Quaternion.identity);

        boss.transform.localScale = Vector3.one * 1.5f;

        Debug.Log("👹 MiniBoss Spawned en ronda " + lastRoundChecked);
    }
}