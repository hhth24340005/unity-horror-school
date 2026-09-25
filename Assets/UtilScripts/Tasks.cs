using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public static class Tasks
{
  public static AsyncFn Race(
    params AsyncFn[] tasks
  ) =>
    async ct =>
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
    };

  public static AsyncFn<R> Race<R>(
    params AsyncFn<R>[] tasks
  ) =>
    async ct =>
    {
      var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
      try
      {
        var (_, ret) = await UniTask.WhenAny(
          tasks.Select(task => task(cts.Token))
        );
        return ret;
      }
      finally
      {
        cts.Cancel();
      }
    };

  public static UniTask SuspendCancellableCoroutine(
    CancellationToken ct,
    Action<Action> block
  ) => SuspendCancellableCoroutine<int>(ct, c => block(() => c(0)));

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

  public delegate UniTask AsyncFn(CancellationToken ct);
  public delegate UniTask<T> AsyncFn<T>(CancellationToken ct);
}
