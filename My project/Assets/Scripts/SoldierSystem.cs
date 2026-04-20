using TMPro;
using UnityEngine;

public sealed class SoldierSystem : MonoBehaviour
{
    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private BoxCollider2D spawnArea;
    [SerializeField] private TextMeshProUGUI soldierDeathText;
    [SerializeField] private TextMeshProUGUI soldierRescueText;
    [SerializeField] private Transform rescueTarget;
    [SerializeField] private Collider2D rescueTargetCollider;
    [SerializeField, Min(0f)] private float soldierLifeTime = 10f;
    [SerializeField, Min(0f)] private float soldierRescueDuration = 3f;
    [SerializeField, Min(0f)] private float spawnInterval = 2f;

    private float spawnTimer;
    private int soldierDeaths;
    private int soldiersRescued;

    public void SpawnSoldier()
    {
        if (soldierPrefab == null || spawnArea == null)
        {
            return;
        }

        GameObject soldierObject = Instantiate(soldierPrefab, GetRandomSpawnPosition(), Quaternion.identity);

        if (soldierObject.TryGetComponent(out Soldier soldier))
        {
            soldier.Initialize(
                this,
                soldierLifeTime,
                rescueTarget,
                rescueTargetCollider,
                soldierRescueDuration);
        }
    }

    public void RecordSoldierDeath()
    {
        soldierDeaths++;
        UpdateSoldierDeathText();
    }

    public void RecordSoldierRescue()
    {
        soldiersRescued++;
        UpdateSoldierRescueText();
    }

    private void Awake()
    {
        if (rescueTargetCollider == null && rescueTarget != null)
        {
            rescueTarget.TryGetComponent(out rescueTargetCollider);
        }

        UpdateSoldierDeathText();
        UpdateSoldierRescueText();
    }

    private void Update()
    {
        if (spawnInterval <= 0f)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval)
        {
            return;
        }

        spawnTimer -= spawnInterval;
        SpawnSoldier();
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Bounds bounds = spawnArea.bounds;

        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            transform.position.z);
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
