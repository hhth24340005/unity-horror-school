using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public static class Enumerables
{
  public static IEnumerable<T> Shuffled<T>(this IEnumerable<T> source)
  {
    var buffer = source.ToArray();
    for (var i = buffer.Length - 1; i >= 0; i--)
    {
      var j = Random.Range(0, i + 1);
      yield return buffer[j];
      buffer[j] = buffer[i];
    }
  }
}
