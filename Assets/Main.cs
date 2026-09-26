using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

internal static class Main
{
  private static async UniTask MainAsync(
    Transform parent,
    CancellationToken ct
  )
  {
    Tasks.AsyncFn transition = _ => UniTask.CompletedTask;
    while (true)
    {
      var gameTransition =
        await Title.PlayAsync(parent, transition, ct);
      transition =
        await Game.PlayAsync(parent, gameTransition, ct);
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
      await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
#if  UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
  }
}
