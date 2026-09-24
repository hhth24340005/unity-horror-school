using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

public static class InputActionsEx
{
  public static async UniTask AwaitPerformed(
    this InputAction input,
    CancellationToken ct
  )
  {
    await UniTask.Yield(PlayerLoopTiming.Update, ct);
    _ = await Tasks.SuspendCancellableCoroutine<int>(ct, complete =>
    {
      input.performed += OnPerform;
      return;

      void OnPerform(InputAction.CallbackContext ctx)
      {
        complete(0);
        input.performed -= OnPerform;
      }
    });
  }

  public static async UniTask<R> AwaitPerformed<R>(
    this InputAction input,
    CancellationToken ct
  ) where R : struct
  {
    await UniTask.Yield(PlayerLoopTiming.Update, ct);
    return await Tasks.SuspendCancellableCoroutine<R>(ct, complete =>
    {
      input.performed += OnPerform;
      return;

      void OnPerform(InputAction.CallbackContext ctx)
      {
        complete(ctx.ReadValue<R>());
        input.performed -= OnPerform;
      }
    });
  }

  public static async UniTask AwaitPressed(
    this InputAction input,
    CancellationToken ct
  )
  {
    await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
    if (!input.IsPressed())
    {
      await input.AwaitPerformed(ct);
    }
  }

  public static async UniTask<R> AwaitPressed<R>(
    this InputAction input,
    CancellationToken ct
  ) where R : struct
  {
    await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
    return input.IsPressed()
      ? input.ReadValue<R>()
      : await input.AwaitPerformed<R>(ct);
  }
}
