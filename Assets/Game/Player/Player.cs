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
  private float force = 1f;

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
    rb.maxLinearVelocity = movementSpeedMetersPerSecond;
  }

  public void Move(Vector2 xzDelta)
  {
    var yaw = camera.transform.rotation.eulerAngles.y;
    var quaternion = Quaternion.Euler(0, yaw, 0);
    var moveDelta = quaternion * new Vector3(xzDelta.x, 0f, xzDelta.y);
    rb.AddForce(moveDelta * force);
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
