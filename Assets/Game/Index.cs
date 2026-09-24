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
      var inventory = Inventory.OfCapacity(5);
      var enemy =
        await Addressables
          .InstantiateAsync("Enemy", parent)
          .WithCancellation(ct)
          .ContinueWith(it => it.GetComponent<Enemy>());
      enemy.transform.position = new Vector3
      {
        x = 3.5f,
        z = -4
      };

      await Tasks.Race(
        ct,
        it => player.UseMovementAsync(
          input.Player.Move,
          input.Player.Sprint,
          it
        ),
        it => player.UseRotationAsync(input.Player.Look, it),
        it => player.UseInteractorAsync(input.Player.Interact, inventory, it)
      );
    }
    finally
    {
      input.Disable();
    }
  }

  private static async UniTask UseMovementAsync(
    this Player player,
    InputAction move,
    InputAction sprint,
    CancellationToken ct
  )
  {
    while (true)
    {
      if (move.IsPressed())
      {
        var delta = move.ReadValue<Vector2>();
        if (delta.y > 0 && sprint.IsPressed())
        {
          await player.SprintAsync(move, ct);
          continue;
        }
        player.TryMove(delta);
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
      }
      else if (player.TryMove(Vector2.zero))
      {
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
      }
      else
      {
        await move.AwaitPerformed(ct);
      }
    }
    // ReSharper disable once FunctionNeverReturns
  }

  private static async UniTask SprintAsync(
    this Player player,
    InputAction move,
    CancellationToken ct
  )
  {
    player.SetSprintFov(true);
    try
    {
      while (move.ReadValue<Vector2>() is { y: > 0 } delta)
      {
        player.TryMove(delta, sprinting: true);
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
      }
    }
    finally
    {
      player.SetSprintFov(false);
    }
  }

  private static async UniTask UseRotationAsync(
    this Player player,
    InputAction input,
    CancellationToken ct
  )
  {
    try
    {
      Cursor.visible = false;
      Cursor.lockState = CursorLockMode.Locked;
      while (true)
      {
        var mouseDelta = await input.AwaitPressed<Vector2>(ct);
        player.LookAround(new Vector2(mouseDelta.x, -mouseDelta.y));
      }
    }
    finally
    {
      Cursor.visible = true;
      Cursor.lockState = CursorLockMode.None;
    }
  }

  private static async UniTask UseInteractorAsync(
    this Player player,
    InputAction input,
    Inventory inventory,
    CancellationToken ct
  )
  {
    while (true)
    {
      await input.AwaitPerformed(ct);
      await player.InteractItemOnSight(inventory, ct);
    }
    // ReSharper disable once FunctionNeverReturns
  }
}
