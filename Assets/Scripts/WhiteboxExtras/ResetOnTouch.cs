using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetOnTouch : MonoBehaviour
{
    [SerializeField]
    Vector3 RespawnPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out CharacterMovement c))
        {
            return;
        }

        c.Warp(RespawnPosition);
    }
}
