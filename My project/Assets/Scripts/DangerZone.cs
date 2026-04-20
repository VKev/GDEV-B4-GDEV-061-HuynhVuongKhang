using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public sealed class DangerZone : MonoBehaviour
{
    private static readonly List<DangerZone> activeZones = new();

    [SerializeField, Min(0.1f)] private float duration = 3f;
    [SerializeField, Min(0.01f)] private float flashFrequency = 6f;
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.15f;
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.75f;

    private SpriteRenderer spriteRenderer;
    private Collider2D zoneCollider;
    private Color baseColor;
    private float expireTime;
    private bool hasBaseColor;

    public static IReadOnlyList<DangerZone> ActiveZones => activeZones;
    public static int ActiveCount => activeZones.Count;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetActiveZones()
    {
        activeZones.Clear();
    }

    public void Initialize(float lifetime)
    {
        duration = Mathf.Max(0.1f, lifetime);
        expireTime = Time.time + duration;
    }

    public bool Contains(Transform target, Collider2D targetCollider)
    {
        if (target == null)
        {
            return false;
        }

        if (zoneCollider != null && targetCollider != null)
        {
            return zoneCollider.Distance(targetCollider).isOverlapped;
        }

        if (zoneCollider != null)
        {
            return zoneCollider.OverlapPoint(target.position);
        }

        return spriteRenderer != null && spriteRenderer.bounds.Contains(target.position);
    }

    private void Awake()
    {
        ResolveComponents();
    }

    private void OnEnable()
    {
        ResolveComponents();

        if (!activeZones.Contains(this))
        {
            activeZones.Add(this);
        }

        expireTime = Time.time + Mathf.Max(0.1f, duration);
    }

    private void OnDisable()
    {
        activeZones.Remove(this);
    }

    private void Update()
    {
        Flash();

        if (Time.time >= expireTime)
        {
            Destroy(gameObject);
        }
    }

    private void ResolveComponents()
    {
        if (spriteRenderer == null)
        {
            TryGetComponent(out spriteRenderer);
        }

        if (zoneCollider == null)
        {
            TryGetComponent(out zoneCollider);
        }

        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true;
        }

        if (spriteRenderer != null && !hasBaseColor)
        {
            baseColor = spriteRenderer.color;
            hasBaseColor = true;
        }

        if (maxAlpha < minAlpha)
        {
            maxAlpha = minAlpha;
        }
    }

    private void Flash()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        float flash = (Mathf.Sin(Time.time * flashFrequency * Mathf.PI * 2f) + 1f) * 0.5f;
        Color color = baseColor;
        color.a = Mathf.Lerp(minAlpha, maxAlpha, flash);
        spriteRenderer.color = color;
    }
}
