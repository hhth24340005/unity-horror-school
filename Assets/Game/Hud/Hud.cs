using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public sealed class GameHud : MonoBehaviour
{
  [SerializeField] private CanvasGroup root;

  [SerializeField] private CanvasGroup controlGuide;

  public Tasks.AsyncFn UseHud() =>
    async ct =>
    {
      root.alpha = 1;
      root.blocksRaycasts = true;
      root.interactable = true;
      try
      {
        await UniTask.WhenAll(
          UniTask.Never(ct),
          UseControlGuide()(ct)
        );
      }
      finally
      {
        root.alpha = 0;
        root.blocksRaycasts = false;
        root.interactable = false;
      }
    };

  private Tasks.AsyncFn UseControlGuide() =>
    async ct =>
    {
      controlGuide.alpha = 1;
      controlGuide.blocksRaycasts = true;
      controlGuide.interactable = true;
      try
      {
        await UniTask.Delay(3000, cancellationToken: ct);
        await DOTween.To(
          () => controlGuide.alpha,
          it => controlGuide.alpha = it,
          0,
          1.5f
        ).WithCancellation(ct);
      }
      finally
      {
        controlGuide.alpha = 0;
        controlGuide.blocksRaycasts = false;
        controlGuide.interactable = false;
      }
    };
}
