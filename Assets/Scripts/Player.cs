using System;
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

    private Rigidbody2D body;
    private bool canLand = true;
    private bool launching;
    private Animator anim;
    
    private static readonly int LandAnim = Animator.StringToHash("land");
    private static readonly int LaunchAnim = Animator.StringToHash("launch");

    public bool IsAttacking => launching;

    private void Start()
    {
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
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
    }

    private void Launch(Vector3 mousePos)
    {
        var dir = mousePos - transform.position;
        var amount = Mathf.Min(dir.magnitude * 5f, 10f);
        body.AddForce(dir.normalized * amount, ForceMode2D.Impulse);
        anim.SetTrigger(LaunchAnim);
        launching = true;
    }

    private void FixedUpdate()
    {
        Curve();
    }

    private void Curve()
    {
        if (!field.HasEnemies || !launching) return;
        var p = transform.position;
        var closest = field.GetClosestEnemyPosition(p);
        var diff = closest - p;
        body.AddForce(diff.normalized * 30f, ForceMode2D.Force);

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
        
        canLand = false;
        launching = false;
        field.AddEnemies();
        
        field.Effect(0.1f);
        anim.SetTrigger(LandAnim);
        field.ResetCombo();
        field.AddMulti();

        this.StartCoroutine(() => canLand = true, 0.2f);
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
    }

    public void Heal()
    {
        EffectManager.AddEffect(5, transform.position, body.rotation);
        field.Heal();
    }
}