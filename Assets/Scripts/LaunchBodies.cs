using System;
using System.Collections.Generic;
using AnttiStarterKit.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

public class LaunchBodies : MonoBehaviour
{
    [SerializeField] private List<Rigidbody2D> bodies;
    [SerializeField] private float force = 10f;
    [SerializeField] private float torque = 1f;

    private void Start()
    {
        bodies.ForEach(Launch);
    }

    private void Launch(Rigidbody2D body)
    {
        var dir = Quaternion.Euler(0, 0, Random.Range(0, 360f)) * Vector2.one;
        body.AddForce(dir * Random.Range(0.5f * force, force), ForceMode2D.Impulse);
        body.AddTorque(Random.Range(-torque, torque), ForceMode2D.Impulse);
    }
}