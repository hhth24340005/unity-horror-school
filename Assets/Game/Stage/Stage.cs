using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public sealed class Stage : MonoBehaviour
{
  [SerializeField]
  private List<Transform> playerSpawnPoints;

  [SerializeField]
  private List<Transform> enemySpawnPoints;

  [SerializeField]
  private List<Transform> keySpawnPoints;

  [SerializeField]
  private Collider exitCollider;

  [SerializeField]
  private int requiredKeys;

  public Transform PlayerSpawnPoint =>
    playerSpawnPoints
      .Shuffled()
      .First();

  public Transform EnemySpawnPoint =>
    enemySpawnPoints
      .Shuffled()
      .First();

  public IEnumerable<Transform> KeySpawnPoints =>
    keySpawnPoints
      .Shuffled()
      .Take(requiredKeys);

  public Tasks.AsyncFn AwaitExit(Collider player, Inventory inventory) =>
    async ct =>
    {
      foreach (var i in Enumerable.Range(0, requiredKeys))
      {
        while (true)
        {
          await exitCollider
            .GetAsyncTriggerStayTrigger()
            .FirstAsync(it => it == player, ct);
          if (inventory.TryRemoveItem(typeof(KeyItem)))
          {
            break;
          }
        }
        Debug.Log($"{i + 1}/{requiredKeys}");
      }
    };
}
