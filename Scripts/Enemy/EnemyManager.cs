using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;
    public List<Transform> enemies = new();

    void Awake()
    {
        instance = this;
    }

    public void Register(Transform enemy)
    {
        if (!enemies.Contains(enemy))
            enemies.Add(enemy);
    }

    public void Unregister(Transform enemy)
    {
        enemies.Remove(enemy);
    }

    public List<Transform> GetEnemies() => enemies;
}
