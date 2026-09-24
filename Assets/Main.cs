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
    await Title.PlayAsync(parent, ct);
    await Game.PlayAsync(parent, ct);
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
}
