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
    var view =
      await Addressables
      .InstantiateAsync("TitleView", parent)
      .WithCancellation(ct)
      .ContinueWith(it => it.GetComponent<TitleView>());
    await view.AwaitStart(ct);
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static async void Boot()
  {
    var go = new GameObject("Root");
    try
    {
      await MainAsync(
        go.transform,
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
      Object.Destroy(go);
#if  UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
  }
}