using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class EndingView : MonoBehaviour
{
  [SerializeField] private Image image;

  [SerializeField] private CanvasGroup overlay;

  [SerializeField] private Sprite gameClear;

  [SerializeField] private Sprite gameOver;

  public Tasks.AsyncFn<Tasks.AsyncFn> ShowGameClear() =>
    Show(gameClear, fadeIn: 1.5f);

  public Tasks.AsyncFn<Tasks.AsyncFn> ShowGameOver() =>
    Show(gameOver, fadeIn: 0);

  private Tasks.AsyncFn<Tasks.AsyncFn> Show(
    Sprite sprite,
    float fadeIn
  ) =>
    async ct =>
    {
      image.sprite = sprite;
      image.color = new Color(1, 1, 1, 0);
      overlay.alpha = 0;

      // Game -> Image
      await DOTween.To(
        () => image.color.a,
        it => image.color = new Color(1, 1, 1, it),
        1,
        fadeIn
      ).WithCancellation(ct);
      await UniTask.Delay(3000, cancellationToken: ct);

      // Image -> Black
      await DOTween.To(
        () => overlay.alpha,
        it => overlay.alpha = it,
        1,
        1.5f
      ).WithCancellation(ct);
      image.color = new Color(1, 1, 1, 0);
      return Hide();
    };

  private Tasks.AsyncFn Hide() =>
    async ct =>
    {
      try
      {
        // Black -> Next scene
        await DOTween.To(
          () => overlay.alpha,
          it => overlay.alpha = it,
          0,
          1.5f
        ).WithCancellation(ct);
      }
      finally
      {
        Destroy(gameObject);
      }
    };
}
