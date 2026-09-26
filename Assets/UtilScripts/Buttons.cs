using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public static class Buttons
{
  public static Tasks.AsyncFn AwaitClick(this Button button) =>
    ct => button.OnClickAsync(ct);
}
