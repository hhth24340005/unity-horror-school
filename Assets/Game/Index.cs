using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

public static class Game
{
  public static async UniTask PlayAsync(
    Transform root,
    CancellationToken ct
  )
  {
    var parent = new GameObject("Game").transform;
    parent.SetParent(root);

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
      Object.Destroy(parent.gameObject);
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
      Vector2 moveDelta;
      if (input.Move.IsPressed())
      {
        moveDelta = input.Move.ReadValue<Vector2>();
      }
      else
      {
        var tcs = new UniTaskCompletionSource<Vector2>();
        input.Move.performed +=
          ctx => { tcs.TrySetResult(ctx.ReadValue<Vector2>()); };
        await using var _ = ct.Register(() => tcs.TrySetCanceled());
        moveDelta = await tcs.Task;
      }
      player.Move(new Vector2(moveDelta.x, moveDelta.y));
      await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
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
      var tcs = new UniTaskCompletionSource<Vector2>();
      input.Look.performed +=
        ctx => { tcs.TrySetResult(ctx.ReadValue<Vector2>()); };
      await using var _ = ct.Register(() => tcs.TrySetCanceled());
      var mouseDelta = await tcs.Task;
      player.LookAround(new Vector2(mouseDelta.x, -mouseDelta.y));
    }
    // ReSharper disable once FunctionNeverReturns
  }
}
