using System;
using System.Collections.Generic;
using System.Linq;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Extensions;
using AnttiStarterKit.Managers;
using AnttiStarterKit.Utils;
using AnttiStarterKit.Visuals;
using UnityEngine;
using Random = UnityEngine.Random;

public class Field : MonoBehaviour
{
    [SerializeField] private EffectCamera cam;
    [SerializeField] private Enemy enemyPrefab;

    private List<Enemy> enemies = new();
    
    private int round;
    private int multi = 1;

    public bool HasEnemies => enemies.Any();

    private void Start()
    {
        Increment();
    }

    private void Increment()
    {
        round++;
        AddEnemies();
        Invoke(nameof(Increment), 10f);
    }

    public void AddEnemies()
    {
        for (var i = 0; i < round; i++)
        {
            var pos = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
            var enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            enemy.Field = this;
            enemies.Add(enemy);
        }
    }

    private void Update()
    {
        if (DevKey.Down(KeyCode.A))
        {
            AddEnemies();
        }
    }

    public void RemoveEnemy(Enemy enemy)
    {
        var p = enemy.transform.position;

        var e = EffectManager.AddTextPopup((10 * multi).AsScore(), p + Vector3.up * 0.5f);
        Tweener.RotateToBounceOut(e.transform, Quaternion.Euler(0, 0, Random.Range(-10f, 10f)), 0.2f);
        
        enemies.Remove(enemy);
        PushEnemies(p);

        multi++;
    }

    public void ResetMulti()
    {
        multi = 1;
    }

    public Vector3 GetClosestEnemyPosition(Vector3 from)
    {
        return enemies.Select(e => e.transform.position).OrderBy(p => Vector3.Distance(p, from)).First();
    }

    private void PushEnemies(Vector3 from)
    {
        enemies.ForEach(e => e.Push(from));
    }

    public void Effect(float amount)
    {
        cam.BaseEffect(amount);
    }
}