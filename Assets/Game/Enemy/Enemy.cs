using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using UnityEngine.AI;

public sealed class Enemy : MonoBehaviour
{
  private static readonly int Speed = Animator.StringToHash("Speed");

  [SerializeField] private Animator animator;

  [SerializeField] private NavMeshAgent agent;

  [SerializeField] private Collider catchCollision;

  [SerializeField] private float walkSpeed = 1.5f;

  [SerializeField] private float runSpeed = 3.5f;

  [SerializeField] private float wanderRadius = 10f;

  [SerializeField] private float wanderViewDistance = 15f;

  [SerializeField] private float wanderViewAngle = 120f;

  [SerializeField] private float chaseViewDistance = 30f;

  [SerializeField] private float chaseViewAngle = 360f;

  [SerializeField] private float eyeHeight = 1.6f;

  [SerializeField] private float targetHeight = 1.0f;

  [SerializeField] private float loseTime = 5f;

  public Tasks.AsyncFn UseAnimation() =>
    async ct =>
    {
      try
      {
        animator.speed = 1;
        while (true)
        {
          await UniTask.Yield(PlayerLoopTiming.Update, ct);
          animator.SetFloat(Speed, agent.speed);
        }
      }
      finally
      {
        animator.speed = 0;
      }
    };

  public Tasks.AsyncFn UseNavigatorAsync(Transform player) =>
    async ct =>
    {
      agent.isStopped = false;
      try
      {
        while (true)
        {
          await Tasks.Race(
            UseWanderer(player),
            AwaitFound(player)
          )(ct);

          await Tasks.Race(
            UseFollower(player),
            AwaitLost(player)
          )(ct);
        }
      }
      finally
      {
        animator.SetFloat(Speed, 0);
        if (agent != null)
        {
          agent.isStopped = true;
        }
      }
    };

  private Tasks.AsyncFn UseWanderer(Transform player) =>
    async ct =>
    {
      agent.speed = walkSpeed;
      while (true)
      {
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);

        var offset = Random.insideUnitSphere * wanderRadius;
        offset.y = 0;
        if (!NavMesh.SamplePosition(player.position + offset, out var hit,
              wanderRadius, NavMesh.AllAreas))
        {
          continue;
        }

        agent.SetDestination(hit.position);
        await UniTask.WaitUntil(
          () => !agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance,
          PlayerLoopTiming.FixedUpdate,
          ct
        );
      }
      // ReSharper disable once FunctionNeverReturns
    };

  private Tasks.AsyncFn UseFollower(Transform followee) =>
    async ct =>
    {
      agent.speed = runSpeed;
      while (true)
      {
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
        agent.SetDestination(followee.position);
      }
      // ReSharper disable once FunctionNeverReturns
    };

  private Tasks.AsyncFn AwaitFound(Transform player) =>
    async ct =>
    {
      await UniTask.WaitUntil(
        () => CanSee(player, wanderViewDistance, wanderViewAngle),
        PlayerLoopTiming.FixedUpdate,
        ct
      );
    };

  private Tasks.AsyncFn AwaitLost(Transform player) =>
    async ct =>
    {
      var lostTimer = 0f;
      while (lostTimer < loseTime)
      {
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
        lostTimer = CanSee(player, chaseViewDistance, chaseViewAngle) ? 0 : lostTimer + Time.fixedDeltaTime;
      }
    };

  public Tasks.AsyncFn AwaitCatch(Collider player) =>
    async ct => {
      var trigger = catchCollision.GetAsyncTriggerEnterTrigger();
      await trigger.FirstAsync(it => it == player, ct);
    };

  private bool CanSee(Transform player, float distance, float angle)
  {
    var eye = transform.position + Vector3.up * eyeHeight;
    var target = player.position + Vector3.up * targetHeight;
    var toPlayer = target - eye;

    if (toPlayer.magnitude > distance) return false;
    if (Vector3.Angle(transform.forward, toPlayer) > angle / 2) return false;
    return Physics.Raycast(
      eye,
      toPlayer.normalized,
      out var hit,
      distance,
      ~0,
      QueryTriggerInteraction.Ignore
    ) && hit.transform.IsChildOf(player);
  }
}
