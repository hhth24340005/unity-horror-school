public sealed class TestFieldItem : FieldItem
{
  protected override IInventoryItem CreateInventoryItem() =>
    new TestItem();
}

public sealed class TestItem : IInventoryItem
{
  public string Name => "Test";
}