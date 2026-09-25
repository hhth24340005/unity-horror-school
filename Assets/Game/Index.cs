using System.Collections.Immutable;
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
    var stage =
      await Addressables
        .InstantiateAsync("SchoolStage", root)
        .WithCancellation(ct)
        .ContinueWith(it => it.GetComponent<Stage>());

    var input = new InputActions();
    input.Enable();
    try
    {
      var player =
        await Addressables
          .InstantiateAsync("Player", stage.PlayerSpawnPoint)
          .WithCancellation(ct)
          .ContinueWith(it => it.GetComponent<Player>());

      var inventory = Inventory.OfCapacity(5);

      var enemy =
        await Addressables
          .InstantiateAsync("Enemy", stage.EnemySpawnPoint)
          .WithCancellation(ct)
          .ContinueWith(it => it.GetComponent<Enemy>());

      // Keys
      await stage
        .KeySpawnPoints
        .Select(point =>
          Addressables
            .InstantiateAsync("Key", point)
            .WithCancellation(ct)
        ).ToImmutableArray();

      await Tasks.Race(
        player.UseControllerAsync(input.Player, inventory),
        stage.AwaitExit(player.Hitbox, inventory),
        enemy.UseAnimation(),
        enemy.AwaitCatch(player.Hitbox),
        enemy.UseFollower(player.transform)
      )(ct);
    }
    finally
    {
      input.Disable();
    }
  }

  private static Tasks.AsyncFn UseControllerAsync(
    this Player player,
    InputActions.PlayerActions input,
    Inventory inventory
  ) =>
    async ct =>
    {
      while (true)
      {
        await Tasks.Race(
          player.UseMovementAsync(input.Move, input.Sprint),
          player.UseRotationAsync(input.Look),
          player.UseInteractorAsync(input.Interact, inventory)
        )(ct);
      }
      // ReSharper disable once FunctionNeverReturns
    };

  private static Tasks.AsyncFn UseMovementAsync(
    this Player player,
    InputAction move,
    InputAction sprint
  ) =>
    async ct =>
    {
      var walkSound =
        await Addressables
          .InstantiateAsync("WalkSound", player.transform)
          .WithCancellation(ct)
          .ContinueWith(it => it.GetComponent<AudioSource>());
      var sprintSound =
        await Addressables
          .InstantiateAsync("SprintSound", player.transform)
          .WithCancellation(ct)
          .ContinueWith(it => it.GetComponent<AudioSource>());
      try
      {
        while (true)
        {
          if (move.IsPressed())
          {
            var delta = move.ReadValue<Vector2>();
            if (delta.y > 0 && sprint.IsPressed())
            {
              walkSound?.Stop();
              sprintSound?.Play();
              try
              {
                await player.SprintAsync(move, ct);
              }
              finally
              {
                sprintSound?.Stop();
              }
              walkSound?.Play();

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
            walkSound?.Stop();
            await move.AwaitPerformed(ct);
            walkSound?.Play();
          }
        }
      }
      finally
      {
        walkSound?.Stop();
      }
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

  private static Tasks.AsyncFn UseRotationAsync(
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

  private static Tasks.AsyncFn UseInteractorAsync(
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
