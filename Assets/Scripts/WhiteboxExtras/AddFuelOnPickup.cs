using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ItemPickup))]
public class AddFuelOnPickup : MonoBehaviour
{
    ItemPickup Pickup;

    private void Awake()
    {
        Pickup = GetComponent<ItemPickup>();
    }

    void OnEnable()
    {
        Pickup.Collected += GiveFuel;
    }

    void OnDisable()
    {
        Pickup.Collected -= GiveFuel;
    }

    void GiveFuel(GameObject character)
    {
        character.GetComponent<Jetpack>().FuelLeft = character.GetComponent<Jetpack>().Duration;
    }
}
