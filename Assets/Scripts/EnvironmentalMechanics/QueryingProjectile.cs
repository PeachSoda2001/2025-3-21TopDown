using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class QueryingProjectile : MonoBehaviour
{
    public event Action<Collision> Collided;

    public Maid ProjectileCleaner;

    private void OnCollisionEnter(Collision collision)
    {
        Collided?.Invoke(collision);
    }

    private void OnDestroy()
    {
        foreach (Action<Collision> bound in Collided.GetInvocationList())
        {
            Collided -= bound;
        }
    }
}
