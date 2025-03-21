using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ItemPickup))]
public class PickupEvents : MonoBehaviour
{
    void Awake()
    {
        var itemPickup = GetComponent<ItemPickup>();

        itemPickup.Collected += OnCollect;
        itemPickup.Respawned += OnRespawn;
    }

    void OnCollect(GameObject character)
    {
        Debug.Log($"Item collected by {character.name}");
    }

    void OnRespawn()
    {
        Debug.Log("Item respawned");
    }
}
