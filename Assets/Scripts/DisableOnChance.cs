using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class DisableOnChance : MonoBehaviour
{
    [SerializeField] private float chance = 0.5f;

    private void Start()
    {
        if (Random.value < chance)
        {
            gameObject.SetActive(false);
        }
    }
}