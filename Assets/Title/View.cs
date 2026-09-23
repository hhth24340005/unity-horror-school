using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class TitleView : MonoBehaviour
{
  [SerializeField]
  private Button startButton;

  public async UniTask AwaitStart(CancellationToken ct)
  {
    await startButton.OnClickAsync(ct);
  }
}
