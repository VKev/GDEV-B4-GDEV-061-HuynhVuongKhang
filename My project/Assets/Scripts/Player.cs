using UnityEngine;
using UnityEngine.InputSystem;

public sealed class Player : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0f)] private float stoppingDistance = 0.05f;

    [SerializeField] private BoxCollider2D movementBoundary;

    private BoxCollider playerCollider;
    private Vector3 targetPosition;
    private bool isMoving;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        TryGetComponent(out playerCollider);
        transform.position = ClampToMovementBoundary(transform.position);
        targetPosition = transform.position;
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TrySetTargetPosition(Mouse.current.position.ReadValue());
        }

        if (!isMoving)
        {
            return;
        }

        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime);

        transform.position = ClampToMovementBoundary(nextPosition);

        if (Vector3.Distance(transform.position, targetPosition) <= stoppingDistance)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }

    private void TrySetTargetPosition(Vector2 screenPosition)
    {
        if (playerCamera == null)
        {
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(screenPosition);
        Plane movementPlane = new Plane(playerCamera.transform.forward, transform.position);

        if (!movementPlane.Raycast(ray, out float distance))
        {
            return;
        }

        targetPosition = ClampToMovementBoundary(ray.GetPoint(distance));
        isMoving = true;
    }

    private Vector3 ClampToMovementBoundary(Vector3 position)
    {
        if (movementBoundary == null)
        {
            return position;
        }

        Bounds boundaryBounds = movementBoundary.bounds;
        Vector2 colliderExtents = Vector2.zero;
        Vector2 colliderCenterOffset = Vector2.zero;

        if (playerCollider != null)
        {
            Bounds playerBounds = playerCollider.bounds;
            colliderExtents = playerBounds.extents;
            colliderCenterOffset = playerBounds.center - transform.position;
        }

        float minX = boundaryBounds.min.x + colliderExtents.x - colliderCenterOffset.x;
        float maxX = boundaryBounds.max.x - colliderExtents.x - colliderCenterOffset.x;
        float minY = boundaryBounds.min.y + colliderExtents.y - colliderCenterOffset.y;
        float maxY = boundaryBounds.max.y - colliderExtents.y - colliderCenterOffset.y;

        position.x = ClampAxis(position.x, minX, maxX, boundaryBounds.center.x - colliderCenterOffset.x);
        position.y = ClampAxis(position.y, minY, maxY, boundaryBounds.center.y - colliderCenterOffset.y);

        return position;
    }

    private static float ClampAxis(float value, float min, float max, float fallback)
    {
        return min <= max ? Mathf.Clamp(value, min, max) : fallback;
    }
}
