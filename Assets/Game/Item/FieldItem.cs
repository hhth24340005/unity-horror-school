using System.Threading;
using Cysharp.Threading.Tasks;

public abstract class FieldItem : Interactable
{
  protected abstract IInventoryItem CreateInventoryItem();

  public override async UniTask InteractAsync(
    Inventory inventory,
    CancellationToken ct
  )
  {
    var item = CreateInventoryItem();
    if (inventory.TryAddItem(item))
    {
      Destroy(gameObject);
      await item.OnPickup(ct);
    }
    else
    {
      // インベントリいっぱいだった時の処理
    }
  }
}
