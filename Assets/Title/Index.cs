using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class Title
{
  public static async UniTask<Tasks.AsyncFn> PlayAsync(
    Transform root,
    Settings settings,
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
    while (await view.AwaitChoice(ct) == TitleView.Choice.Option)
    {
      using var optionParent = parent.UseChild("Option");
      var option =
        await Addressables
          .InstantiateAsync("OptionView", optionParent)
          .WithCancellation(ct)
          .ContinueWith(it => it.GetComponent<OptionView>());
      view.SetMenuVisible(false);
      try
      {
        await option.EditAsync(settings, ct);
      }
      finally
      {
        view.SetMenuVisible(true);
      }
    }
    return _ => UniTask.CompletedTask;
  }
}
