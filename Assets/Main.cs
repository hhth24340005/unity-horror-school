using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

internal static class Main
{
  private static async UniTask MainAsync()
  {

  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static async void Boot()
  {
    try
    {
      await MainAsync();
    }
    catch (Exception e)
    {
      Debug.LogError("Uncaught exception:");
      Debug.LogException(e);
    }
    finally
    {
      #if  UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
      #endif
    }
  }
}