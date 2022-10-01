using System;
using System.Collections.Generic;
using AnttiStarterKit.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

public class Field : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;

    private List<Enemy> enemies = new();

    private void Start()
    {
        AddEnemy();
        Invoke(nameof(AddEnemy), 10f);
    }

    private void AddEnemy()
    {
        var pos = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
        var enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
        enemy.Field = this;
        enemies.Add(enemy);
        ShowEnemyCount();
    }

    private void ShowEnemyCount()
    {
        Debug.Log($"Now {enemies.Count} enemies");
    }

    private void Update()
    {
        if (DevKey.Down(KeyCode.A))
        {
            AddEnemy();
        }
    }

    public void RemoveEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
        ShowEnemyCount();
    }
}