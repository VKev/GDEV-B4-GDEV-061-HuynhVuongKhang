using UnityEngine;

public sealed class DangerZoneSpawner : MonoBehaviour
{
    [SerializeField] private GameObject dangerZonePrefab;
    [SerializeField] private BoxCollider2D spawnArea;
    [SerializeField] private GameManager gameManager;
    [SerializeField, Min(0.1f)] private float dangerZoneLifetime = 3f;
    [SerializeField, Min(1)] private int maxActiveDangerZones = 5;
    [SerializeField, Min(0.05f)] private float minSpawnInterval = 0.5f;
    [SerializeField, Min(0.05f)] private float maxSpawnInterval = 1.25f;
    [SerializeField, Min(0f)] private float initialSpawnDelayMin = 0.25f;
    [SerializeField, Min(0f)] private float initialSpawnDelayMax = 1f;

    private float spawnTimer;

    private void Awake()
    {
        ResolveReferences();
        spawnTimer = Random.Range(initialSpawnDelayMin, Mathf.Max(initialSpawnDelayMin, initialSpawnDelayMax));
    }

    private void Update()
    {
        if (gameManager != null && gameManager.IsGameEnded)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f)
        {
            return;
        }

        if (DangerZone.ActiveCount < maxActiveDangerZones)
        {
            SpawnDangerZone();
        }

        ScheduleNextSpawn();
    }

    private void SpawnDangerZone()
    {
        if (dangerZonePrefab == null || spawnArea == null)
        {
            return;
        }

        GameObject dangerZoneObject = Instantiate(
            dangerZonePrefab,
            GetRandomSpawnPosition(),
            Quaternion.identity);

        if (!dangerZoneObject.TryGetComponent(out DangerZone dangerZone))
        {
            dangerZone = dangerZoneObject.AddComponent<DangerZone>();
        }

        dangerZone.Initialize(dangerZoneLifetime);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 halfSize = spawnArea.size * 0.5f;
        Vector3 localPoint = new Vector3(
            spawnArea.offset.x + Random.Range(-halfSize.x, halfSize.x),
            spawnArea.offset.y + Random.Range(-halfSize.y, halfSize.y),
            0f);
        Vector3 worldPoint = spawnArea.transform.TransformPoint(localPoint);
        worldPoint.z = transform.position.z;

        return worldPoint;
    }

    private void ScheduleNextSpawn()
    {
        spawnTimer = Random.Range(minSpawnInterval, Mathf.Max(minSpawnInterval, maxSpawnInterval));
    }

    private void ResolveReferences()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (spawnArea != null)
        {
            return;
        }

        BoxCollider2D[] colliders = FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None);

        foreach (BoxCollider2D candidate in colliders)
        {
            if (candidate.gameObject.name == "Boundary")
            {
                spawnArea = candidate;
                return;
            }
        }
    }

    private void OnValidate()
    {
        maxSpawnInterval = Mathf.Max(minSpawnInterval, maxSpawnInterval);
        initialSpawnDelayMax = Mathf.Max(initialSpawnDelayMin, initialSpawnDelayMax);
    }
}
