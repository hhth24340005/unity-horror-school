using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.UI;

public sealed class OptionView : MonoBehaviour
{
  [SerializeField] private Slider seSlider;

  [SerializeField] private Slider sensitivitySlider;

  [SerializeField] private Button closeButton;

  public async UniTask EditAsync(Settings settings, CancellationToken ct)
  {
    seSlider.value = settings.SeVolume;
    sensitivitySlider.value = settings.MouseSensitivity;

    await Tasks.Race(
      c => seSlider
        .OnValueChangedAsAsyncEnumerable(c)
        .ForEachAsync(it =>
        {
          settings.SeVolume = it;
          AudioListener.volume = it;
        }, c),
      c => sensitivitySlider
        .OnValueChangedAsAsyncEnumerable(c)
        .ForEachAsync(it => settings.MouseSensitivity = it, c),
      closeButton.AwaitClick()
    )(ct);
  }
}
