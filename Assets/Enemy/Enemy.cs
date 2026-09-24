using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public sealed class Enemy : MonoBehaviour
{
  private static readonly int Speed = Animator.StringToHash("Speed");

  [SerializeField]
  private Animator animator;

  [SerializeField]
  private Collider catchCollision;

  private async void Start()
  {
    var ct = Application.exitCancellationToken;
    while (true)
    {
      animator.SetFloat(Speed, 0);
      await UniTask.Delay(2000, cancellationToken: ct);
      animator.SetFloat(Speed, 2);
      await UniTask.Delay(2000, cancellationToken: ct);
      animator.SetFloat(Speed, 4);
      await UniTask.Delay(2000, cancellationToken: ct);
    }
  }

  public async UniTask AwaitCatch(
    Collider player,
    CancellationToken ct
  )
  {
    var trigger = catchCollision.GetAsyncTriggerEnterTrigger();
    await trigger.FirstAsync(it => it == player, ct);
  }
}
