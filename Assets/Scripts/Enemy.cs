using System;
using AnttiStarterKit.Extensions;
using AnttiStarterKit.Managers;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Field Field { get; set; }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        EffectManager.AddEffects(new []{ 0, 1}, transform.position);
        Field.RemoveEnemy(this);
        Destroy(gameObject);
    }
}