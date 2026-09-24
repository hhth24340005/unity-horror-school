using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class Player : MonoBehaviour
{
  [SerializeField]
  private Rigidbody rb;

  [SerializeField]
  private Camera camera;

  [SerializeField]
  private Collider hitbox;

  [SerializeField]
  private float accelerationMetersPerSecondSquared = 1f;

  [SerializeField]
  private float movementSpeedMetersPerSecond = 1f;

  [SerializeField]
  private Vector2 mouseSensitivity = Vector2.one;

  [SerializeField]
  private float interactDistance = Mathf.Infinity;

  [SerializeField]
  private LayerMask interactLayerMask;

  private const float Epsilon = 1e-5f;

  private void Awake()
  {
    var near = camera.nearClipPlane;
    var halfH = near * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
    var halfW = halfH * camera.aspect;
    var nearCornerDistance = new Vector3(halfW, halfH, near).magnitude;

    var bounds = hitbox.bounds;
    var offset = camera.transform.position - bounds.center;
    var offsetXZ = new Vector2(offset.x, offset.z).magnitude;
    var required = offsetXZ + nearCornerDistance;
    var hitboxRadius = Mathf.Min(bounds.extents.x, bounds.extents.z);
    if (hitboxRadius < required)
    {
      Debug.LogWarning(
        $"hitbox radius ({hitboxRadius}) is smaller than required ({required}).",
        this
      );
    }
  }

  public bool TryMove(Vector2 xzDelta)
  {
    var yaw = camera.transform.rotation.eulerAngles.y;
    var quaternion = Quaternion.Euler(0, yaw, 0);
    var target =
      quaternion * new Vector3(xzDelta.x, 0f, xzDelta.y)
      * movementSpeedMetersPerSecond;

    var v = rb.linearVelocity;
    var current = new Vector3(v.x, 0f, v.z);
    var next = Vector3.MoveTowards(
      current,
      target,
      accelerationMetersPerSecondSquared * Time.fixedDeltaTime
    );
    if (next == current)
    {
      return false;
    }
    rb.AddForce(next - current, ForceMode.VelocityChange);
    return true;
  }

  public void LookAround(Vector2 delta)
  {
    var eulerDelta =
      new Vector3(
        delta.y * mouseSensitivity.y,
        delta.x * mouseSensitivity.x,
        0
      );
    var original = camera.transform.eulerAngles;
    var pitch = Mathf.DeltaAngle(0f, original.x) + eulerDelta.x;
    pitch = Mathf.Clamp(pitch, -90 + Epsilon, 90 - Epsilon);
    var next = new Vector3(pitch, original.y + eulerDelta.y, original.z);
    camera.transform.eulerAngles = next;
  }

  public async UniTask InteractItemOnSight(
    Inventory inventory,
    CancellationToken ct
  )
  {
    if (
      !Physics.Raycast(
        camera.transform.position,
        camera.transform.forward,
        out var hit,
        interactDistance,
        interactLayerMask
      )
    )
    {
      return;
    }
    if (!hit.collider.TryGetComponent<Interactable>(out var item))
    {
      return;
    }
    await item.InteractAsync(inventory, ct);
  }
}
