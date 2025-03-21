using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockAxis : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Locks the transform on the X axis. Note: setting at runtime is not supported.")]
    bool X = false;

    [SerializeField]
    [Tooltip("Locks the transform on the Y axis. Note: setting at runtime is not supported.")]
    bool Y = false;

    [SerializeField]
    [Tooltip("Locks the transform on the Z axis. Note: setting at runtime is not supported.")]
    bool Z = false;

    Vector3 LockedPosition = Vector3.zero;

    const float dx = 0.01f;

    CharacterMovement Movement => GetComponent<CharacterMovement>();
    CharacterController Controller => GetComponent<CharacterController>();

    void Awake()
    {
        LockedPosition = Vector3.Scale(transform.position, VectorFromAxis(X, Y, Z));
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 FreeComponents = Vector3.Scale(transform.position, Vector3.one - VectorFromAxis(X, Y, Z));
        Vector3 LockedComponents = Vector3.Scale(LockedPosition, VectorFromAxis(X, Y, Z));

        if ((Vector3.Scale(transform.position, VectorFromAxis(X, Y, Z)) - LockedComponents).magnitude > dx)
        {
            Movement.Warp(FreeComponents + LockedComponents);
        }
    }

    Vector3 VectorFromAxis(bool x, bool y, bool z)
    {
        return (x ? Vector3.right : Vector3.zero) + (y ? Vector3.up : Vector3.zero) + (z ? Vector3.forward : Vector3.zero);
    }
}
