using System;
using System.Collections.Generic;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Managers;
using UnityEngine;
using AnttiStarterKit.Extensions;
using AnttiStarterKit.ScriptableObjects;

public class LineDrawer : ObjectPool<LineRenderer>
{
    [SerializeField] private SoundCollection thunderSounds;
    
    public void AddLine(Vector3 from, Vector3 to, Color color, float width, float inDuration, float outDuration)
    {
        var line = Get(true);
        var t = line.transform;
        t.position = from;
        t.localScale = Vector3.zero;
        line.positionCount = 20;
        var step = 1f / line.positionCount;
        for (var i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i, Vector3.Lerp(from, to, i * step));
        }

        line.startColor = line.endColor = color;
        line.widthMultiplier = width;
        
        Tweener.ScaleToBounceOut(t, Vector3.one, inDuration);
        this.StartCoroutine(() => Tweener.ScaleToQuad(t, Vector3.zero, outDuration), inDuration);
        this.StartCoroutine(() => ReturnToPool(line), inDuration + outDuration);
    }

    public void AddThunderLine(Vector3 from, Vector3 to, Color color, float width)
    {
        AudioManager.Instance.PlayEffectFromCollection(thunderSounds, from, 1f);
        var line = Get(true);
        line.transform.localScale = Vector3.one;
        var mid = (from + to) * 0.5f;
        line.positionCount = 4;
        line.SetPositions(new []
        {
            from,
            mid.RandomOffset(2f),
            mid.RandomOffset(2f),
            to
        });

        line.startColor = line.endColor = color;
        line.widthMultiplier = width;

        this.StartCoroutine(() => ReturnToPool(line), 0.3f);
    }
}