using System;
using UnityEngine;
using Object = UnityEngine.Object;

public static class Transforms
{
  public static DisposableTransform UseChild(
    this Transform parent,
    string name
  )
  {
    var child = new GameObject(name);
    child.transform.SetParent(parent);
    return new DisposableTransform(child.transform);
  }
  
  public static DisposableTransform UseChild(
    this DisposableTransform parent,
    string name
  ) =>
    parent.Original.UseChild(name);
}

public sealed record DisposableTransform(
  Transform Original
) : IDisposable
{
  public Transform Original { get; } = Original;

  public static implicit operator Transform(DisposableTransform self)
    => self.Original;

  public void Dispose()
  {
    if (Original == null)
    {
      return;
    }
    Object.Destroy(Original.gameObject);
  }
}
