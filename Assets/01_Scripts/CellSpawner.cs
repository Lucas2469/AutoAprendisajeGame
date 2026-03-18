using UnityEngine;

public class CellSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject miniBossPrefab;

    [Header("Spawn Area")]
    public float spawnAreaX = 8f;
    public float spawnAreaY = 4f;

    [Header("Límites")]
    public int maxCellsOnScreen = 20;
    public int cellsToSpawnAtRoundStart = 8;

    private bool roundSpawnDone = false;
    private bool miniBossSpawnedThisRound = false;
    private int lastRoundChecked = -1;

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsRoundTransitioning()) return;

        int round = GameManager.Instance.GetCurrentRound();

        if (round != lastRoundChecked)
        {
            roundSpawnDone = false;
            miniBossSpawnedThisRound = false;
            lastRoundChecked = round;
        }

        bool isBossRound = (round % 5 == 0);

        if (isBossRound)
        {
            if (!miniBossSpawnedThisRound)
            {
                SpawnMiniBoss();
                miniBossSpawnedThisRound = true;
            }
            return;
        }

        if (!roundSpawnDone)
        {
            SpawnRoundCells();
            roundSpawnDone = true;
        }
    }

    void SpawnRoundCells()
    {
        GameObject[] currentCells = GameObject.FindGameObjectsWithTag("Cell");
        int currentCount = currentCells.Length;

        int availableToSpawn = Mathf.Max(0, maxCellsOnScreen - currentCount);
        int spawnCount = Mathf.Min(cellsToSpawnAtRoundStart, availableToSpawn);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 pos;
            if (TryGetValidSpawnPosition(out pos))
            {
                Instantiate(cellPrefab, pos, Quaternion.identity);
            }
        }
    }

    bool TryGetValidSpawnPosition(out Vector2 spawnPos)
    {
        Rect blockedRect = GameManager.Instance.GetTrophyWorldBlockRect();

        for (int attempt = 0; attempt < 30; attempt++)
        {
            float randomX = Random.Range(-spawnAreaX, spawnAreaX);
            float randomY = Random.Range(-spawnAreaY, spawnAreaY);
            Vector2 testPos = new Vector2(randomX, randomY);

            if (!blockedRect.Contains(testPos))
            {
                spawnPos = testPos;
                return true;
            }
        }

        spawnPos = Vector2.zero;
        return false;
    }

    void SpawnMiniBoss()
    {
        if (miniBossPrefab == null)
        {
            Debug.LogWarning("MiniBoss Prefab no asignado.");
            return;
        }

        Vector2 spawnPosition = new Vector2(0f, 2f);
        Instantiate(miniBossPrefab, spawnPosition, Quaternion.identity);
    }
}