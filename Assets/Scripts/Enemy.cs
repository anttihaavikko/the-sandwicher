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
    [SerializeField] private Color charmColor, burnColor;
    [SerializeField] private Transform eyepatch;

    private bool charmed, burned;
    private bool champ;
    
    public Field Field { get; set; }

    public bool IsCharmed => charmed;
    
    public bool IsChamp => champ;

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
        
        Move();
        
        Tweener.ScaleToBounceOut(visuals, Vector3.one * scale, 0.4f);
    }

    private void Move()
    {
        body.AddForce(Vector3.zero.RandomOffset(1f) * Random.value, ForceMode2D.Impulse);
        body.AddTorque(Random.Range(-0.06f, 0.06f), ForceMode2D.Impulse);
        Invoke(nameof(Move), Random.Range(2f, 10f));
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        var player = col.GetComponent<Player>();
        if (!player || !player.IsAttacking) return;
        CancelInvoke(nameof(Crumble));
        CancelInvoke(nameof(Move));
        Field.Effect(0.2f);
        EffectManager.AddEffects(new []{ 0, 1, 2, 3}, transform.position);
        Field.RemoveEnemy(this);
        Destroy(gameObject);
    }

    public void Push(Vector3 from, float amount, ForceMode2D mode)
    {
        var diff = transform.position - from;
        if (diff.magnitude > 4f) return;
        var inversed = diff.normalized * 1 / diff.magnitude;
        body.AddForce(inversed * amount, mode);
    }

    public void Charm()
    {
        if (charmed || champ) return;
        charmed = true;
        charmEffect.SetActive(true);
        Colorize(charmColor);
    }

    public void Colorize(Color color)
    {
        sprites.ForEach(s => s.color *= color * color);
    }

    public void Burn()
    {
        if (burned) return;
        burned = true;
        Colorize(burnColor);
        Invoke(nameof(Crumble), Random.Range(0.5f, 2f));
    }

    private void Crumble()
    {
        EffectManager.AddEffect(6, transform.position);
        Field.RemoveEnemy(this, true);
        Destroy(gameObject);
    }

    public void Champify()
    {
        champ = true;
        eyepatch.gameObject.SetActive(true);
        eyepatch.localScale = Vector3.one.WhereY(Misc.PlusMinusOne());
    }
}