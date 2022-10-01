using System;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Field field;

    private Rigidbody2D body;
    private bool canLand = true;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var pos = cam.ScreenToWorldPoint(Input.mousePosition).WhereZ(0);
            var dir = pos - transform.position;
            body.AddForce(dir * 5f, ForceMode2D.Impulse);
        }
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
        }
    }

    private void Land()
    {
        body.velocity = Vector2.zero;
        body.angularVelocity = 0;

        if (!canLand) return;
        
        canLand = false;

        this.StartCoroutine(() => canLand = true, 0.2f);
    }
}