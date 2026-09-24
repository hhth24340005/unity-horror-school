using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using AsyncFn = System.Func<System.Threading.CancellationToken, Cysharp.Threading.Tasks.UniTask>;

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
        player.UseControllerAsync(input.Player, inventory),
        enemy.UseAnimation(),
        enemy.AwaitCatch(player.Hitbox),
        enemy.UseFollower(player.transform)
      );
    }
    finally
    {
      input.Disable();
    }
  }

  private static AsyncFn UseControllerAsync(
    this Player player,
    InputActions.PlayerActions input,
    Inventory inventory
  ) =>
    async ct =>
    {
      while (true)
      {
        await Tasks.Race(
          ct,
          player.UseMovementAsync(input.Move, input.Sprint),
          player.UseRotationAsync(input.Look),
          player.UseInteractorAsync(input.Interact, inventory)
        );
      }
      // ReSharper disable once FunctionNeverReturns
    };

  private static AsyncFn UseMovementAsync(
    this Player player,
    InputAction move,
    InputAction sprint
  ) =>
    async ct =>
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
    };

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

  private static AsyncFn UseRotationAsync(
    this Player player,
    InputAction input
  ) =>
    async ct =>
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
    };

  private static AsyncFn UseInteractorAsync(
    this Player player,
    InputAction input,
    Inventory inventory
  ) =>
    async ct =>
    {
      while (true)
      {
        await input.AwaitPerformed(ct);
        await player.InteractItemOnSight(inventory, ct);
      }
      // ReSharper disable once FunctionNeverReturns
    };
}
