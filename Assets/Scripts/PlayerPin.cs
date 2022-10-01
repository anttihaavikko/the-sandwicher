using System;
using UnityEngine;

public class PlayerPin : MonoBehaviour
{
    [SerializeField] private Transform target;

    private Rigidbody2D body;


    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        body.MovePosition(target.position);
        body.SetRotation(target.rotation.eulerAngles.z - 90);
    }
}