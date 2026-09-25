using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using UnityEngine.AI;

public sealed class Enemy : MonoBehaviour
{
  private static readonly int Speed = Animator.StringToHash("Speed");

  [SerializeField]
  private Animator animator;

  [SerializeField]
  private NavMeshAgent agent;

  [SerializeField]
  private Collider catchCollision;

  public Tasks.AsyncFn UseAnimation() =>
    async ct => {
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

  public Tasks.AsyncFn UseFollower(Transform followee) =>
    async ct =>
    {
      try
      {
        while (true)
        {
          await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
          agent.SetDestination(followee.position);
        }
      }
      finally
      {
        agent.isStopped = true;
      }
    };

  public Tasks.AsyncFn AwaitCatch(Collider player) =>
    async ct => {
      var trigger = catchCollision.GetAsyncTriggerEnterTrigger();
      await trigger.FirstAsync(it => it == player, ct);
    };
}
