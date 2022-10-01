using System;
using System.Collections.Generic;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Extensions;
using AnttiStarterKit.Managers;
using AnttiStarterKit.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private SpriteRenderer front, back, lettuce, tomato1, tomato2;
    [SerializeField] private Transform visuals;
    [SerializeField] private GameObject charmEffect;
    [SerializeField] private List<SpriteRenderer> sprites;
    [SerializeField] private Color charmColor;

    private bool charmed;
    
    public Field Field { get; set; }

    public bool IsCharmed => charmed;

    private void Start()
    {
        EffectManager.AddEffects(new []{ 4 }, transform.position);
        
        var scale = Random.Range(0.7f, 1.3f);
        
        var angle = Random.Range(0, 360f);
        front.transform.Rotate(new Vector3(0, 0, angle));
        back.transform.Rotate(new Vector3(0, 0, angle + Random.Range(-20f, 20f)));
        
        tomato1.transform.position += Vector3.zero.RandomOffset(0.2f);
        tomato2.transform.position += Vector3.zero.RandomOffset(0.2f);

        front.flipX = Random.value > 0.5f;
        front.flipY = Random.value > 0.5f;
        
        lettuce.flipX = Random.value > 0.5f;
        lettuce.flipY = Random.value > 0.5f;
        
        visuals.localScale = Vector3.zero;
        
        body.AddForce(Vector3.zero.RandomOffset(1f) * Random.value, ForceMode2D.Impulse);
        
        Tweener.ScaleToBounceOut(visuals, Vector3.one * scale, 0.4f);
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        var player = col.GetComponent<Player>();
        if (!player || !player.IsAttacking) return;
        Field.Effect(0.2f);
        EffectManager.AddEffects(new []{ 0, 1, 2, 3}, transform.position);
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

    public void Charm()
    {
        if (charmed) return;
        charmed = true;
        charmEffect.SetActive(true);
        Colorize(charmColor);
    }

    public void Colorize(Color color)
    {
        sprites.ForEach(s => s.color *= color * color);
    }
}