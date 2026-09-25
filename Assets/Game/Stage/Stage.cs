using System.Collections.Generic;
using UnityEngine;

public sealed class Stage : MonoBehaviour
{
  [SerializeField]
  private List<Transform> playerSpawnPoints;

  [SerializeField]
  private List<Transform> enemySpawnPoints;

  [SerializeField]
  private List<Transform> keySpawnPoints;

  public IReadOnlyList<Transform> PlayerSpawnPoints => playerSpawnPoints;

  public IReadOnlyList<Transform> EnemySpawnPoints => enemySpawnPoints;

  public IReadOnlyList<Transform> KeySpawnPoints => keySpawnPoints;
}
