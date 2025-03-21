using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class HitboxQueryPart : MonoBehaviour
{
    Collider Collider => GetComponent<Collider>();

    public HashSet<Collider> Intersecting = new();

    public void Awake()
    {
        Assert.IsTrue(Collider.isTrigger, "Hitbox parts must be triggers!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Intersecting.Contains(other))
        {
            Intersecting.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Intersecting.Contains(other))
        {
            Intersecting.Remove(other);
        }
    }

    private void Update()
    {
        Intersecting.RemoveWhere(collider => collider.IsDestroyed() || !collider.gameObject.activeInHierarchy);
    }
}
