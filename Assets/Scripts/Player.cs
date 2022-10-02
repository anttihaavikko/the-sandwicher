using System;
using System.Collections.Generic;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Extensions;
using AnttiStarterKit.Game;
using AnttiStarterKit.Managers;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Field field;
    [SerializeField] private LineRenderer arrow;
    [SerializeField] private GameObject extras, bits;
    [SerializeField] private Flasher flasher;
    [SerializeField] private Rigidbody2D potionPrefab;
    [SerializeField] private Transform potionSpawnPos;
    [SerializeField] private Transform feetPos;
    [SerializeField] private bool demo = false;
    
    private Rigidbody2D body;
    private bool canLand = true;
    private bool launching;
    private Animator anim;
    private readonly Queue<Rigidbody2D> potions = new();

    private bool willLevel;
    
    private static readonly int LandAnim = Animator.StringToHash("land");
    private static readonly int LaunchAnim = Animator.StringToHash("launch");

    public bool IsAttacking => launching;

    private void Start()
    {
        AudioManager.Instance.TargetPitch = 1;
        
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (demo) return;
        CheckLaunch();
    }

    private void CheckLaunch()
    {
        var mousePos = cam.ScreenToWorldPoint(Input.mousePosition).WhereZ(0);
        
        if (Input.GetMouseButtonDown(0) && !launching)
        {
            Launch(mousePos);
        }

        var p = transform.position;
        arrow.SetPosition(0, p);
        arrow.SetPosition(1, p * 0.25f + mousePos * 0.75f);
        arrow.SetPosition(2, mousePos);

        if (launching)
        {
            field.Push();
        }
    }

    private void Launch(Vector3 mousePos)
    {
        if (field.IsLocked) return;

        var position = transform.position;
        var dir = mousePos - position;
        var amount = Mathf.Min(dir.magnitude * 5f, 10f);
        body.AddForce(dir.normalized * amount, ForceMode2D.Impulse);
        anim.SetTrigger(LaunchAnim);
        launching = true;
        JumpLines();
        
        AudioManager.Instance.PlayEffectFromCollection(1, position);
        AudioManager.Instance.PlayEffectFromCollection(11, position);
    }

    private void JumpLines()
    {
        EffectManager.AddEffect(7, feetPos.position, body.rotation);
    }

    private void FixedUpdate()
    {
        Curve();
    }

    private void Curve()
    {
        if (!field || !field.HasEnemies || !launching) return;
        var p = transform.position;
        var closest = field.GetClosestEnemyPosition(p);
        var diff = closest - p;
        body.AddForce(diff.normalized * field.HomingStrength, ForceMode2D.Force);

        var angle = body.rotation < Vector2.Angle(body.velocity, Vector2.zero) ? 1f : -1f;
        body.AddTorque(angle * 2f, ForceMode2D.Force);
    }

    public void RotateTo(float angle)
    {
        Tweener.RotateToBounceOut(transform, Quaternion.Euler(0, 0, angle), 0.2f);
        body.angularVelocity = 0;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Wall"))
        {
            Land();
            RotateTo(col.collider.GetComponent<Wall>().Angle);
        }
    }

    private void Land()
    {
        body.velocity = Vector2.zero;
        body.angularVelocity = 0;

        if (!canLand) return;
        
        field.Effect(0.1f);
        
        field.Burn();
        
        canLand = false;
        launching = false;
        field.AddEnemies();
        
        anim.SetTrigger(LandAnim);
        field.ResetCombo();
        field.AddMulti();
        
        JumpLines();

        var p = transform.position;
        AudioManager.Instance.PlayEffectFromCollection(9, p);
        AudioManager.Instance.PlayEffectFromCollection(8, p);

        this.StartCoroutine(() => canLand = true, 0.2f);

        if (willLevel)
        {
            willLevel = false;
            field.LevelUp();
        }
    }

    public void SwingSound()
    {
        AudioManager.Instance.PlayEffectFromCollection(7, transform.position);
    }
    
    public void HitSound()
    {
        AudioManager.Instance.PlayEffectFromCollection(2, transform.position);
    }

    public void Die()
    {
        field.Effect(0.5f);
        var pos = transform.position;
        EffectManager.AddEffects(new []{ 0, 1, 2, 3}, pos);
        gameObject.SetActive(false);
        extras.SetActive(false);
        bits.transform.position = pos;
        bits.SetActive(true);
        field.GameOver();

        AudioManager.Instance.PlayEffectFromCollection(14, pos, 3f);
        AudioManager.Instance.TargetPitch = 0;
    }

    public void Heal()
    {
        var p = transform.position;
        EffectManager.AddEffect(5, p, body.rotation);
        AudioManager.Instance.PlayEffectAt(2, p, 0.3f);
        field.Heal();
    }

    public void MarkForLevelUp()
    {
        if (!willLevel)
        {
            // AudioManager.Instance.FilterFor(2f);
            var am = AudioManager.Instance;
            am.TargetPitch = 3f;
            am.PlayEffectAt(5, Vector3.zero, 2.5f);
            this.StartCoroutine(() => am.TargetPitch = 1f, 1f);
            am.LowpassFor(2f);
        }
        
        willLevel = true;
    }

    private void DropPotion()
    {
        var pot = CreatePotion();
        var t = pot.transform;
        var pos = potionSpawnPos.position;
        t.position = pos;
        t.rotation = potionSpawnPos.rotation;
        pot.AddForce(Vector3.zero.RandomOffset(0.5f), ForceMode2D.Impulse);
        potions.Enqueue(pot);
        AudioManager.Instance.PlayEffectFromCollection(12, pos, 0.6f);
    }

    private Rigidbody2D CreatePotion()
    {
        return potions.Count > 20 ? 
            potions.Dequeue() : 
            Instantiate(potionPrefab, null);
    }

    public void Hurt()
    {
        AudioManager.Instance.PlayEffectFromCollection(0, transform.position);
    }

    public void ShieldSound()
    {
        AudioManager.Instance.PlayEffectAt(4, transform.position, 0.8f);
    }
}