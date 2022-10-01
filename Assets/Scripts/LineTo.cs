using System;
using UnityEngine;

public class LineTo : MonoBehaviour
{
    [SerializeField] private Transform target;

    private LineRenderer line;

    private void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, target.position);
    }
}