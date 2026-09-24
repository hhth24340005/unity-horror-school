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

      await Tasks.Race(
        ct,
        it => player.UseMovementAsync(input.Player.Move, it),
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
    InputAction input,
    CancellationToken ct
  )
  {
    while (true)
    {
      var moveDelta = await input.AwaitPressed<Vector2>(ct);
      player.Move(moveDelta);
    }
    // ReSharper disable once FunctionNeverReturns
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
