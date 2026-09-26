using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class TitleView : MonoBehaviour
{
  public enum Choice
  {
    Start,
    Option,
  }

  [SerializeField]
  private Button startButton;

  [SerializeField]
  private Button optionButton;

  [SerializeField]
  private CanvasGroup menu;

  public UniTask<Choice> AwaitChoice(CancellationToken ct) =>
    Tasks.Race(
      startButton.AwaitClick().Returns(Choice.Start),
      optionButton.AwaitClick().Returns(Choice.Option)
    )(ct);

  public void SetMenuVisible(bool visible)
  {
    menu.alpha = visible ? 1 : 0;
    menu.interactable = visible;
    menu.blocksRaycasts = visible;
  }
}
