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
    using var parent = root.UseChild("Title");
    var view =
      await Addressables
        .InstantiateAsync("TitleView", parent)
        .WithCancellation(ct)
        .ContinueWith(it => it.GetComponent<TitleView>());
    await view.AwaitStart(ct);
  }
}
