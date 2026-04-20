using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public sealed class SoldierSystem : MonoBehaviour
{
    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private BoxCollider2D spawnArea;
    [SerializeField] private TextMeshProUGUI soldierDeathText;
    [SerializeField] private TextMeshProUGUI soldierRescueText;
    [SerializeField] private Transform rescueTarget;
    [SerializeField] private Collider2D rescueTargetCollider;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameManager gameManager;
    [SerializeField, Min(0f)] private float soldierLifeTime = 10f;
    [SerializeField, Min(0f)] private float soldierRescueDuration = 3f;
    [SerializeField, FormerlySerializedAs("spawnInterval"), Min(0.05f)] private float initialSpawnInterval = 3f;
    [SerializeField, Min(0.05f)] private float minSpawnInterval = 0.5f;
    [SerializeField, Min(0f)] private float spawnIntervalDecrement = 0.15f;
    [SerializeField, Min(0f)] private float minSpawnDistanceFromPlayer = 3f;
    [SerializeField, Min(1)] private int spawnPositionMaxAttempts = 20;

    private float spawnTimer;
    private float currentSpawnInterval;
    private int soldierDeaths;
    private int soldiersRescued;
    private bool warnedMissingPlayerTransform;
    private bool warnedNoValidSpawnPosition;

    public void SpawnSoldier()
    {
        TrySpawnSoldier();
    }

    private bool TrySpawnSoldier()
    {
        if (soldierPrefab == null || spawnArea == null)
        {
            return false;
        }

        if (!TryGetRandomSpawnPosition(out Vector3 spawnPosition))
        {
            return false;
        }

        GameObject soldierObject = Instantiate(soldierPrefab, spawnPosition, Quaternion.identity);

        if (soldierObject.TryGetComponent(out Soldier soldier))
        {
            soldier.Initialize(
                this,
                soldierLifeTime,
                rescueTarget,
                rescueTargetCollider,
                soldierRescueDuration);
        }

        return true;
    }

    public void RecordSoldierDeath()
    {
        if (gameManager != null && gameManager.IsGameEnded)
        {
            return;
        }

        soldierDeaths++;
        UpdateSoldierDeathText();

        if (gameManager != null)
        {
            gameManager.NotifySoldierDeath();
        }
    }

    public void RecordSoldierRescue()
    {
        soldiersRescued++;
        UpdateSoldierRescueText();
    }

    private void Awake()
    {
        ResolveReferences();

        currentSpawnInterval = Mathf.Max(minSpawnInterval, initialSpawnInterval);

        UpdateSoldierDeathText();
        UpdateSoldierRescueText();
    }

    private void Update()
    {
        if (gameManager != null && gameManager.IsGameEnded)
        {
            return;
        }

        if (currentSpawnInterval <= 0f)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer < currentSpawnInterval)
        {
            return; 
        }

        spawnTimer -= currentSpawnInterval;
        TrySpawnSoldier();

        currentSpawnInterval = Mathf.Max(
            minSpawnInterval,
            currentSpawnInterval - spawnIntervalDecrement);
    }

    private bool TryGetRandomSpawnPosition(out Vector3 spawnPosition)
    {
        spawnPosition = default;

        if (playerTransform == null && minSpawnDistanceFromPlayer > 0f)
        {
            if (!warnedMissingPlayerTransform)
            {
                Debug.LogWarning(
                    $"{nameof(SoldierSystem)} cannot spawn soldiers away from the player because no player transform is assigned.",
                    this);
                warnedMissingPlayerTransform = true;
            }

            return false;
        }

        for (int attempt = 0; attempt < spawnPositionMaxAttempts; attempt++)
        {
            Vector3 candidate = GetRandomPointInSpawnArea();

            if (IsValidSpawnPosition(candidate))
            {
                spawnPosition = candidate;
                warnedNoValidSpawnPosition = false;
                return true;
            }
        }

        if (TryGetFarthestValidSpawnPosition(out spawnPosition))
        {
            warnedNoValidSpawnPosition = false;
            return true;
        }

        if (!warnedNoValidSpawnPosition)
        {
            Debug.LogWarning(
                $"{nameof(SoldierSystem)} could not find a spawn point inside the map that is at least {minSpawnDistanceFromPlayer} units from the player.",
                this);
            warnedNoValidSpawnPosition = true;
        }

        return false;
    }

    private Vector3 GetRandomPointInSpawnArea()
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

    private bool TryGetFarthestValidSpawnPosition(out Vector3 spawnPosition)
    {
        spawnPosition = default;

        if (playerTransform == null || minSpawnDistanceFromPlayer <= 0f)
        {
            spawnPosition = GetRandomPointInSpawnArea();
            return true;
        }

        Vector2 halfSize = spawnArea.size * 0.5f;
        Vector2 offset = spawnArea.offset;
        Vector3[] corners =
        {
            GetWorldSpawnAreaPoint(offset.x - halfSize.x, offset.y - halfSize.y),
            GetWorldSpawnAreaPoint(offset.x - halfSize.x, offset.y + halfSize.y),
            GetWorldSpawnAreaPoint(offset.x + halfSize.x, offset.y - halfSize.y),
            GetWorldSpawnAreaPoint(offset.x + halfSize.x, offset.y + halfSize.y)
        };

        float bestDistanceSqr = -1f;

        foreach (Vector3 corner in corners)
        {
            if (!IsValidSpawnPosition(corner))
            {
                continue;
            }

            float distanceSqr = ((Vector2)(corner - playerTransform.position)).sqrMagnitude;

            if (distanceSqr <= bestDistanceSqr)
            {
                continue;
            }

            bestDistanceSqr = distanceSqr;
            spawnPosition = corner;
        }

        return bestDistanceSqr >= 0f;
    }

    private Vector3 GetWorldSpawnAreaPoint(float localX, float localY)
    {
        Vector3 worldPoint = spawnArea.transform.TransformPoint(new Vector3(localX, localY, 0f));
        worldPoint.z = transform.position.z;
        return worldPoint;
    }

    private bool IsValidSpawnPosition(Vector3 candidate)
    {
        if (!spawnArea.OverlapPoint(candidate))
        {
            return false;
        }

        if (playerTransform == null || minSpawnDistanceFromPlayer <= 0f)
        {
            return true;
        }

        float minDistanceSqr = minSpawnDistanceFromPlayer * minSpawnDistanceFromPlayer;
        return ((Vector2)(candidate - playerTransform.position)).sqrMagnitude >= minDistanceSqr;
    }

    private void ResolveReferences()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (playerTransform == null)
        {
            if (rescueTarget != null)
            {
                playerTransform = rescueTarget;
            }
            else
            {
                Player player = FindFirstObjectByType<Player>();

                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
        }

        if (rescueTarget == null)
        {
            rescueTarget = playerTransform;
        }

        if (rescueTargetCollider == null && rescueTarget != null)
        {
            rescueTarget.TryGetComponent(out rescueTargetCollider);
        }
    }

    private void UpdateSoldierDeathText()
    {
        if (soldierDeathText != null)
        {
            soldierDeathText.text = soldierDeaths.ToString();
        }
    }

    private void UpdateSoldierRescueText()
    {
        if (soldierRescueText != null)
        {
            soldierRescueText.text = soldiersRescued.ToString();
        }
    }
}
