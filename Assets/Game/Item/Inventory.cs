using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public sealed class Inventory
{
  private readonly List<IInventoryItem> _items = new();
  private readonly int _capacity;

  private Inventory(int capacity)
  {
    _capacity = capacity;
  }

  public static Inventory OfCapacity(int capacity) => new(capacity);

  public bool TryAddItem(IInventoryItem item)
  {
    if (_capacity <= _items.Count)
    {
      return false;
    }
    _items.Add(item);
    return true;
  }

  public void RemoveItem(IInventoryItem item)
  {
    if (!_items.Contains(item))
    {
      throw new ArgumentException($"{item} is not in the inventory");
    }
    _items.Remove(item);
  }
}

public interface IInventoryItem
{
  public string Name { get; }

  public UniTask UseOnHand(CancellationToken ct)
  {
    return UniTask.CompletedTask;
  }

  public UniTask OnPickup(CancellationToken ct)
  {
    return UniTask.CompletedTask;
  }
}
