using System;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Extensions;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Field field;

    private Rigidbody2D body;
    private bool canLand = true;
    private bool launching;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CheckLaunch();
    }

    private void CheckLaunch()
    {
        if (Input.GetMouseButtonDown(0) && !launching)
        {
            Launch();
        }
    }

    private void Launch()
    {
        var pos = cam.ScreenToWorldPoint(Input.mousePosition).WhereZ(0);
        var dir = pos - transform.position;
        body.AddForce(dir * 5f, ForceMode2D.Impulse);
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

        this.StartCoroutine(() => canLand = true, 0.2f);
    }
}