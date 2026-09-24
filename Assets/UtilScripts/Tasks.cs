using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public static class Tasks
{
  public static async UniTask Race(
    CancellationToken ct,
    params Func<CancellationToken, UniTask>[] tasks
  )
  {
    var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    try
    {
      await UniTask.WhenAny(
        tasks.Select(task => task(cts.Token))
      );
    }
    finally
    {
      cts.Cancel();
    }
  }

  public static async UniTask<R> SuspendCancellableCoroutine<R>(
    CancellationToken ct,
    Action<Action<R>> block
  )
  {
    var tcs = new UniTaskCompletionSource<R>();
    await using var _ = ct.Register(() => tcs.TrySetCanceled());
    block(result => tcs.TrySetResult(result));
    return await tcs.Task;
  }
}
