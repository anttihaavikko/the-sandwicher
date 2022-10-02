using System;
using System.Collections.Generic;
using System.Linq;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Extensions;
using AnttiStarterKit.Game;
using AnttiStarterKit.Managers;
using AnttiStarterKit.Utils;
using AnttiStarterKit.Visuals;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Field : MonoBehaviour
{
    [SerializeField] private EffectCamera cam;
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private ScoreDisplay scoreDisplay;
    [SerializeField] private Health health;
    [SerializeField] private NumberBar expBar, enemyBar;
    [SerializeField] private Player player;
    [SerializeField] private Shaker multiShaker;
    [SerializeField] private List<TMP_Text> levelTexts;
    [SerializeField] private List<Appearer> levelUpAppearers;
    [SerializeField] private List<TMP_Text> waveTexts;
    [SerializeField] private LineDrawer lineDrawer;
    [SerializeField] private Color fireColor, charmColor;
    [SerializeField] private Appearer shield;

    private List<Enemy> enemies = new();
    
    private int round;
    private int combo = 1;
    private int level = 1;
    private bool leveling;
    private bool locked;
    private int shields;
    
    private int[] stats = new int[6];

    private float levelUpCooldown = 10f, enemyCooldown = 10f;

    public bool HasEnemies => enemies.Any();
    public bool IsLocked => leveling || locked;

    public float HomingStrength => 20f;

    private void Start()
    {
        Increment();
        ShowLevel();
    }

    private void Increment()
    {
        round++;
        AddEnemies();
        ShowWave();
    }

    public void AddEnemies()
    {
        for (var i = 0; i < round; i++)
        {
            var pos = new Vector3(Random.Range(-5f, 5f), Random.Range(-3f, 3f), 0);
            var enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            enemy.Field = this;
            enemies.Add(enemy);

            if (round > 3 && (enemies.Count > 70 && Random.value < 0.5f || Random.value < 0.1f))
            {
                enemy.Champify();
            }
        }
    }

    private void Update()
    {
        if (DevKey.Down(KeyCode.A))
        {
            AddEnemies();
        }

        FillBars();
    }

    private void FillBars()
    {
        if (!player.gameObject.activeInHierarchy) return;
        
        UpdateBars();

        expBar.SetValue((10f - levelUpCooldown) * 0.1f);
        enemyBar.SetValue(enemyCooldown * 0.1f);
    }

    private void UpdateBars()
    {
        if (leveling) return;
        
        if (player.IsAttacking)
        {
            levelUpCooldown -= Time.deltaTime;
            if (levelUpCooldown <= 0) player.MarkForLevelUp();
            return;
        }
        
        enemyCooldown -= Time.deltaTime;
        if (enemyCooldown <= 0) TickEnemies();
    }

    private void TickEnemies()
    {
        enemyCooldown = 10f;
        Increment();
        scoreDisplay.ResetMulti();
        multiShaker.Shake();
    }

    public void LevelUp()
    {
        if (leveling) return;
        
        levelUpCooldown = 10f;
        level++;
        ShowLevel();
        
        health.Add(3);
        
        levelUpAppearers[0].Show();
        levelUpAppearers[1].ShowAfter(0.25f);
        levelUpAppearers[2].ShowAfter(0.5f);

        leveling = true;
    }

    private void ShowLevel()
    {
        var text = $"Level {level} Sandwicher";
        levelTexts.ForEach(t => t.text = text);
    }
    
    private void ShowWave()
    {
        var text = $"Wave {round}";
        waveTexts.ForEach(t => t.text = text);
    }

    public void RemoveEnemy(Enemy enemy, bool passive = false)
    {
        var p = enemy.transform.position;

        if (!enemy.IsCharmed)
        {
            Damage(enemy.IsChamp ? round : 1);
        }

        var mod = enemy.IsChamp ? round : 1;
        var amount = 10 * combo * mod;
        var shown = amount * scoreDisplay.Multi;
        var e = EffectManager.AddTextPopup(shown.AsScore(), p + Vector3.up * 0.5f);
        Tweener.RotateToBounceOut(e.transform, Quaternion.Euler(0, 0, Random.Range(-10f, 10f)), 0.2f);

        if (player.gameObject.activeInHierarchy)
        {
            scoreDisplay.Add(amount);
        }
        
        enemies.Remove(enemy);
        PushEnemies(p);

        combo++;

        if (passive) return;
        
        Charm();
    }

    private void Damage(int amount)
    {
        if (amount >= health.Current && shields > 0)
        {
            shields--;
            if (shields <= 0)
            {
                shield.Hide();
            }
            
            EffectManager.AddEffects(new []{ 0, 3 }, player.transform.position);

            return;
        }
        
        health.TakeDamage<GameObject>(amount);
    }

    public void Burn()
    {
        var amount = stats[2];
        if (amount < 1) return;

        var p = player.transform.position;
        var targets = enemies
            .OrderBy(e => Vector3.Distance(e.transform.position, p))
            .Take(amount)
            .ToList();
        
        targets.ForEach(BurnEnemy);

        if (targets.Any())
        {
            cam.BaseEffect(0.3f);
        }
    }

    private void BurnEnemy(Enemy e)
    {
        e.Burn();
        var pos = e.transform.position;
        lineDrawer.AddThunderLine(player.transform.position, pos, fireColor, 0.5f, 1f);
        EffectManager.AddEffects(new []{ 0, 1, 3 }, pos);
    }

    private void Charm()
    {
        var amount = stats[5];
        if (amount < 1) return;

        var p = player.transform.position;
        var targets = enemies
            .OrderBy(e => Vector3.Distance(e.transform.position, p))
            .Take(amount)
            .ToList();
        
        targets.ForEach(CharmEnemy);
    }

    private void CharmEnemy(Enemy e)
    {
        e.Charm();
        lineDrawer.AddThunderLine(player.transform.position, e.transform.position, charmColor, 0.2f, 1f);
    }

    public void AddMulti()
    {
        scoreDisplay.AddMulti();
    }

    public void ResetCombo()
    {
        combo = 1;
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
        health.Heal(SkillBox.BaseHeal + stats[0] * SkillBox.HealPerAlchemyLevel);

        if (stats[4] > 0)
        {
            shield.Show();
            shields = stats[4];
        }
    }

    public void Pick(int index)
    {
        if (!leveling) return;

        leveling = false;
        locked = true;

        stats[index]++;

        levelUpAppearers[2].Hide();
        levelUpAppearers[1].HideWithDelay(0.1f);
        levelUpAppearers[0].HideWithDelay(0.2f);

        this.StartCoroutine(() => locked = false, 0.1f);
    }
}