using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject enemyPrefab;
    [Tooltip("How many of this enemy to spawn in this wave")]
    public int count = 1;
}

[System.Serializable]
public class Wave
{
    public string waveName = "Wave";
    public List<EnemySpawnEntry> enemies = new List<EnemySpawnEntry>();
    [Tooltip("Delay in seconds between each individual enemy spawn")]
    public float spawnInterval = 0.5f;
    [Tooltip("Delay in seconds before this wave begins")]
    public float waveStartDelay = 1f;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Configuration")]
    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Spawner Settings")]
    [Tooltip("Time to wait after all enemies in a wave are dead before starting the next wave")]
    [SerializeField] private float timeBetweenWaves = 3f;
    [SerializeField] private bool autoStartOnAwake = false;

    [Header("Room Settings")]
    [Tooltip("Optional: door/gate objects to lock when combat starts and unlock when cleared")]
    [SerializeField] private List<GameObject> roomDoors = new List<GameObject>();

    [Header("Events")]
    public UnityEvent onRoomEntered;
    public UnityEvent onWaveStarted;
    public UnityEvent onWaveCleared;
    public UnityEvent onAllWavesCleared;

    private int currentWaveIndex = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawning = false;
    private bool roomCleared = false;
    private bool hasStarted = false;

    public int CurrentWaveIndex => currentWaveIndex;
    public int TotalWaves => waves.Count;
    public bool RoomCleared => roomCleared;
    public bool IsSpawning => isSpawning;

    private void Awake()
    {
        if (autoStartOnAwake)
            StartRoom();
    }


    public void StartRoom()
    {
        if (hasStarted || roomCleared) return;
        hasStarted = true;

        SetDoorsLocked(true);
        onRoomEntered?.Invoke();
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        currentWaveIndex = 0;

        while (currentWaveIndex < waves.Count)
        {
            Wave wave = waves[currentWaveIndex];

            yield return new WaitForSeconds(wave.waveStartDelay);

            Debug.Log($"[EnemySpawner] Starting {wave.waveName} ({currentWaveIndex + 1}/{waves.Count})");
            onWaveStarted?.Invoke();

            yield return StartCoroutine(SpawnWave(wave));

            yield return StartCoroutine(WaitForWaveClear());

            Debug.Log($"[EnemySpawner] {wave.waveName} cleared!");
            onWaveCleared?.Invoke();

            currentWaveIndex++;

            if (currentWaveIndex < waves.Count)
                yield return new WaitForSeconds(timeBetweenWaves);
        }

        roomCleared = true;
        SetDoorsLocked(false);
        Debug.Log("[EnemySpawner] All waves cleared! Room complete.");
        onAllWavesCleared?.Invoke();
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        isSpawning = true;

        List<GameObject> spawnList = new List<GameObject>();
        foreach (EnemySpawnEntry entry in wave.enemies)
        {
            if (entry.enemyPrefab == null) continue;
            for (int i = 0; i < entry.count; i++)
                spawnList.Add(entry.enemyPrefab);
        }

        ShuffleList(spawnList);

        foreach (GameObject prefab in spawnList)
        {
            SpawnEnemy(prefab);
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        isSpawning = false;
    }

    private void SpawnEnemy(GameObject prefab)
    {
        Transform spawnPoint = GetRandomSpawnPoint();
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;

        Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
        randomOffset.y = 0f;
        GameObject enemy = Instantiate(prefab, spawnPos + randomOffset, Quaternion.identity);
        activeEnemies.Add(enemy);

        EnemySpawnTracker tracker = enemy.AddComponent<EnemySpawnTracker>();
        tracker.Initialize(this);
    }

    private IEnumerator WaitForWaveClear()
    {
        yield return new WaitUntil(() =>
        {
            activeEnemies.RemoveAll(e => e == null);
            return activeEnemies.Count == 0;
        });
    }

    public void OnEnemyDied(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
    }

    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return transform;
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }

    private void SetDoorsLocked(bool locked)
    {
        foreach (GameObject door in roomDoors)
        {
            if (door != null)
                door.SetActive(locked);
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        foreach (Transform sp in spawnPoints)
        {
            if (sp != null)
            {
                Gizmos.DrawWireSphere(sp.position, 0.4f);
                Gizmos.DrawLine(transform.position, sp.position);
            }
        }
    }
}
public class EnemySpawnTracker : MonoBehaviour
{
    private EnemySpawner spawner;

    public void Initialize(EnemySpawner owner)
    {
        spawner = owner;
    }

    private void OnDestroy()
    {
        if (spawner != null)
            spawner.OnEnemyDied(gameObject);
    }
}