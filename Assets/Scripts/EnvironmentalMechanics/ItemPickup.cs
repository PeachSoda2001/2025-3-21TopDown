using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Whether or not the pickup respawns after being collected.")]
    bool DoesRespawn = true;

    [SerializeField]
    [Tooltip("The time it takes for the object to become active again.")]
    float RespawnTime = 1;

    [SerializeField]
    [Tooltip("Whether or not the Visual should be invisible while the pickup's cooldown is counting down.")]
    bool HideWhenDespawned = true;

    [SerializeField]
    [Tooltip("The game object to hide if Hide When Despawned is true.")]
    GameObject Visual;


    public event Action<GameObject> Collected;
    public event Action Respawned;

    float RespawnTimeLeft = 0;
    bool Despawned = false;

    private void Awake()
    {
        // TODO warn if collider is not a trigger
    }

    private void Update()
    {
        if (DoesRespawn && Despawned)
        {
            RespawnTimeLeft -= Time.deltaTime;
        }

        if (RespawnTimeLeft <= 0 && Despawned)
        {
            Respawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMovement>(out _) && !Despawned)
        {
            Collect(other.gameObject);
        }
    }

    void Collect(GameObject character)
    {
        Despawn();
        Collected?.Invoke(character);
    }

    void Despawn()
    {
        Despawned = true;
        RespawnTimeLeft = RespawnTime;

        if (HideWhenDespawned && Visual != null)
        {
            Visual.SetActive(false);
        }

        // remove collider for when students want to collect these one, so that queries don't keep intercepting them
        if (!DoesRespawn)
        {
            gameObject.SetActive(false);
        }
    }

    void Respawn()
    { 
        Despawned = false;

        if (HideWhenDespawned && Visual != null)
        {
            Visual.SetActive(true);
        }

        Respawned?.Invoke();
    }
}
