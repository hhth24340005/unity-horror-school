using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;

public static class Game
{
  public static async UniTask PlayAsync(
    Transform root,
    CancellationToken ct
  )
  {
    using var parent = root.UseChild("Game");

    var input = new InputActions();
    input.Enable();
    try
    {
      var player =
        await Addressables
          .InstantiateAsync("Player", parent)
          .WithCancellation(ct)
          .ContinueWith(it => it.GetComponent<Player>());

      await Tasks.Race(
        ct,
        it => player.UseMovementAsync(input.Player, it),
        it => player.UseRotationAsync(input.Player, it)
      );
    }
    finally
    {
      input.Disable();
    }
  }

  private static async UniTask UseMovementAsync(
    this Player player,
    InputActions.PlayerActions input,
    CancellationToken ct
  )
  {
    while (true)
    {
      var moveDelta = await input.Move.Await<Vector2>(ct);
      player.Move(moveDelta);
    }
    // ReSharper disable once FunctionNeverReturns
  }

  private static async UniTask UseRotationAsync(
    this Player player,
    InputActions.PlayerActions input,
    CancellationToken ct
  )
  {
    while (true)
    {
      var mouseDelta = await input.Look.Await<Vector2>(ct);
      player.LookAround(new Vector2(mouseDelta.x, -mouseDelta.y));
    }
    // ReSharper disable once FunctionNeverReturns
  }

  private static async UniTask<R> Await<R>(
    this InputAction input,
    CancellationToken ct
  ) where R : struct
  {
    R ret;
    if (input.IsPressed())
    {
      ret = input.ReadValue<R>();
      await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
    }
    else
    {
      ret = await Tasks.SuspendCancellableCoroutine<R>(ct, complete =>
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

    return ret;
  }
}
