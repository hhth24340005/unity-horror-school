using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
  public abstract UniTask InteractAsync(
    Inventory inventory,
    CancellationToken ct
  );
}
