using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

internal static class Main
{
  private static async UniTask MainAsync(
    Transform parent,
    CancellationToken ct
  )
  {
    await PlayTitleAsync(parent, ct);
    await PlayGameAsync(parent, ct);
  }

  private static async UniTask PlayTitleAsync(
    Transform root,
    CancellationToken ct
  )
  {
    var parent = new GameObject("Title").transform;
    parent.SetParent(root);
    try
    {
      var view =
        await Addressables
          .InstantiateAsync("TitleView", parent)
          .WithCancellation(ct)
          .ContinueWith(it => it.GetComponent<TitleView>());
      await view.AwaitStart(ct);
    }
    finally
    {
      Object.Destroy(parent.gameObject);
    }
  }

  private static async UniTask PlayGameAsync(
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

      await Race(
        ct,
        it => player.UseMovementAsync(input.Player, it),
        it => player.UseRotationAsync(input.Player, it),
        it => UniTask.Delay(5000, cancellationToken: it)
      );
      await UniTask.Never(ct);
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

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static async void Boot()
  {
    var root = new GameObject("Root");
    try
    {
      await MainAsync(
        root.transform,
        Application.exitCancellationToken
      );
    }
    catch (Exception e)
    {
      Debug.LogError("Uncaught exception:");
      Debug.LogException(e);
    }
    finally
    {
      Object.Destroy(root.gameObject);
#if  UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
  }


  private static async UniTask Race(
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
}
