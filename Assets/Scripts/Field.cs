using System;
using System.Collections.Generic;
using System.Linq;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Extensions;
using AnttiStarterKit.Game;
using AnttiStarterKit.Managers;
using AnttiStarterKit.Utils;
using AnttiStarterKit.Visuals;
using UnityEngine;
using Random = UnityEngine.Random;

public class Field : MonoBehaviour
{
    [SerializeField] private EffectCamera cam;
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private ScoreDisplay scoreDisplay;
    [SerializeField] private Health health;

    private List<Enemy> enemies = new();
    
    private int round;
    private int multi = 1;

    private int healAmount = 3;

    private float levelUpCooldown, enemyCooldown;

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

        var amount = Mathf.RoundToInt(10 * Mathf.Pow(multi, 2));
        var e = EffectManager.AddTextPopup(amount.AsScore(), p + Vector3.up * 0.5f);
        Tweener.RotateToBounceOut(e.transform, Quaternion.Euler(0, 0, Random.Range(-10f, 10f)), 0.2f);
        
        scoreDisplay.Add(amount);
        
        enemies.Remove(enemy);
        PushEnemies(p);

        multi++;
        
        health.TakeDamage<GameObject>(1);
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

    public void Heal()
    {
        health.Heal(healAmount);
    }
}