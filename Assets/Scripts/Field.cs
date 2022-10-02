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
    [SerializeField] private GameObject gameOverStuff;
    [SerializeField] private List<TMP_Text> deathReasonTexts;

    private List<Enemy> enemies = new();
    
    private int round;
    private int combo = 1;
    private int level = 1;
    private bool leveling;
    private bool locked;
    private int shields;
    private bool hasHealed, killedByChamp;
    
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

            if (round > 3 && ((enemies.Count > 70 || round > 10 || scoreDisplay.Total > 1000000) && Random.value < 0.5f || Random.value < 0.1f))
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
        
        enemyCooldown -= Time.deltaTime * Mathf.Pow(0.8f, stats[3]);
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
        var dmg = DamageFor(enemy);

        if (!enemy.IsCharmed && !passive)
        {
            Damage(dmg);
        }

        if (!passive)
        {
            AudioManager.Instance.PlayEffectFromCollection(14, p, 1.2f);
            AudioManager.Instance.PlayEffectFromCollection(5, p);   
        }

        var amount = 10 * combo * dmg;
        var shown = amount * scoreDisplay.Multi;
        var e = EffectManager.AddTextPopup(shown.AsScore(), p + Vector3.up * 0.5f);
        Tweener.RotateToBounceOut(e.transform, Quaternion.Euler(0, 0, Random.Range(-10f, 10f)), 0.2f);

        if (player.gameObject.activeInHierarchy)
        {
            player.HitSound();
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
            player.ShieldSound();

            return;
        }

        if (health.Current <= amount && amount > 1)
        {
            killedByChamp = true;
        }
        
        health.TakeDamage<GameObject>(amount);
        player.Hurt();
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
            var vol = Mathf.Min(4f, 2f + targets.Count * 0.4f);
            AudioManager.Instance.PlayEffectFromCollection(9, p, vol);
            AudioManager.Instance.PlayEffectFromCollection(5, p, vol * 0.6f);
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
        enemies.ForEach(e => e.Push(from, 2f, ForceMode2D.Impulse));
    }

    public void Effect(float amount)
    {
        cam.BaseEffect(amount);
    }

    public void Heal()
    {
        hasHealed = true;
        
        health.Heal(SkillBox.BaseHeal + stats[0] * SkillBox.HealPerAlchemyLevel);

        if (stats[4] > 0)
        {
            if (shields <= 0)
            {
                player.ShieldSound();
            }
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

    public void Push()
    {
        if (stats[1] < 1) return;
        var targets = enemies.Where(e => DamageFor(e) >= health.Current).ToList();
        targets.ForEach(Push);
    }
    
    private void Push(Enemy e)
    {
        var p = player.transform.position;
        e.Push(p, 7f * stats[1], ForceMode2D.Force);
    }

    private int DamageFor(Enemy e)
    {
        return e.IsChamp ? round : 1;
    }

    public void GameOver()
    {
        this.StartCoroutine(() => AudioManager.Instance.PlayEffectAt(3, Vector3.zero, 0.5f), 0.2f);
        var reason = GetDeathReason();
        deathReasonTexts.ForEach(t => t.text = reason);
        gameOverStuff.SetActive(true);
    }

    private string GetDeathReason()
    {
        if (scoreDisplay.Total >= 100000)
        {
            return new []
            {
                "Wasemir would be proud of you...",
                "Superb job!"
            }.Random();
        }
        
        if (!hasHealed) return new []
        {
            "Try taking it more slowly to use potions...",
            "Breadalt will quaff a potion after landing...",
            "Maybe try a less aggressive approach..."
        }.Random();
        
        if (killedByChamp) return new []
        {
            "The champion enemies can be dangerous!",
            "Take it easy around the champion enemies!",
            "Be cautious of the champion enemies!"
        }.Random();

        if (scoreDisplay.Total >= 10000)
        {
            return new []
            {
                "Decent try...",
                "Nicely done...",
                "Getting better..."
            }.Random();
        }

        return new []
        {
            "You can do better!",
            "I had high expectations of you...",
            "You've got much to learn..."
        }.Random();
    }
    
    
}