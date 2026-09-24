using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class Title
{
  public static async UniTask PlayAsync(
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
}
