using System;
using UnityEngine;

public class EdgeDetector : MonoBehaviour
{
    [SerializeField] private float angle;
    [SerializeField] private Player player;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject == player.gameObject)
        {
            player.RotateTo(angle);
        }
    }
}