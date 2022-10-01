using System;
using AnttiStarterKit.Extensions;
using AnttiStarterKit.Managers;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    
    public Field Field { get; set; }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        Field.Effect(0.2f);
        EffectManager.AddEffects(new []{ 0, 1}, transform.position);
        Field.RemoveEnemy(this);
        Destroy(gameObject);
    }

    public void Push(Vector3 from)
    {
        var diff = transform.position - from;
        if (diff.magnitude > 4f) return;
        var inversed = diff.normalized * 1 / diff.magnitude;
        body.AddForce(inversed * 2f, ForceMode2D.Impulse);
    }
}