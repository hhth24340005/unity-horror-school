public sealed class Key : FieldItem
{
  protected override IInventoryItem CreateInventoryItem() =>
    new KeyItem();
}

public sealed class KeyItem : IInventoryItem
{
  public string Name => "Key";
}