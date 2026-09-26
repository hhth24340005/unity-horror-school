using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class Title
{
  public static async UniTask<Tasks.AsyncFn> PlayAsync(
    Transform root,
    Tasks.AsyncFn transition,
    CancellationToken ct
  )
  {
    using var parent = root.UseChild("Title");
    var view =
      await Addressables
        .InstantiateAsync("TitleView", parent)
        .WithCancellation(ct)
        .ContinueWith(it => it.GetComponent<TitleView>());
    await transition(ct);
    await view.AwaitStart(ct);
    return _ => UniTask.CompletedTask;
  }
}
