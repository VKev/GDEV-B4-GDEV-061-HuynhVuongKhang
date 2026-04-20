using UnityEngine;

public sealed class Soldier : MonoBehaviour
{
    [SerializeField, Min(0f)] private float lifeTime = 10f;
    [SerializeField] private Collider2D rescueZone;
    [SerializeField, Min(0f)] private float rescueDuration = 3f;

    private SoldierSystem soldierSystem;
    private Transform rescueTarget;
    private Collider2D rescueTargetCollider;
    private float expireTime;
    private float rescueTimer;
    private bool isFinished;

    private void Awake()
    {
        if (rescueZone != null)
        {
            rescueZone.isTrigger = true;
        }
    }

    private void OnEnable()
    {
        expireTime = Time.time + lifeTime;
        rescueTimer = 0f;
        isFinished = false;
    }

    public void Initialize(
        SoldierSystem owner,
        float secondsToLive,
        Transform target,
        Collider2D targetCollider,
        float secondsToRescue)
    {
        soldierSystem = owner;
        lifeTime = secondsToLive;
        rescueTarget = target;
        rescueTargetCollider = targetCollider;
        rescueDuration = secondsToRescue;
        expireTime = Time.time + lifeTime;
        rescueTimer = 0f;
        isFinished = false;
    }

    private void Update()
    {
        if (isFinished)
        {
            return;
        }

        if (Time.time >= expireTime)
        {
            Expire();
            return;
        }

        if (!IsRescueTargetInsideZone())
        {
            rescueTimer = 0f;
            return;
        }

        rescueTimer += Time.deltaTime;

        if (rescueTimer >= rescueDuration)
        {
            Rescue();
        }
    }

    public void Rescue()
    {
        if (isFinished)
        {
            return;
        }

        isFinished = true;
        soldierSystem?.RecordSoldierRescue();
        Destroy(gameObject);
    }

    private void Expire()
    {
        if (isFinished)
        {
            return;
        }

        isFinished = true;
        soldierSystem?.RecordSoldierDeath();
        Destroy(gameObject);
    }

    private bool IsRescueTargetInsideZone()
    {
        if (rescueZone == null || rescueTarget == null)
        {
            return false;
        }

        if (rescueTargetCollider != null)
        {
            return rescueZone.Distance(rescueTargetCollider).isOverlapped;
        }

        return rescueZone.OverlapPoint(rescueTarget.position);
    }
}
