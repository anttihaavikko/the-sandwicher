using System;
using AnttiStarterKit.Managers;
using AnttiStarterKit.ScriptableObjects;
using UnityEngine;

public class SoundOnCollision : MonoBehaviour
{
    [SerializeField] private SoundCollection sounds;
    [SerializeField] private float threshold = 1f;
    [SerializeField] private float volume = 1f;
    [SerializeField] private float maxAmount;

    private void OnCollisionEnter2D(Collision2D col)
    {
        var force = col.relativeVelocity.magnitude;
        if (col.relativeVelocity.magnitude > threshold)
        {
            AudioManager.Instance.PlayEffectFromCollection(sounds, col.contacts[0].point, volume * 0.1f * Mathf.Min(maxAmount, force));
        }
    }
}