
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterQueryBehavior : MonoBehaviour
{
    public event Action<List<(Collider, Vector3)>> Hit;

    protected void OnHit(List<(Collider, Vector3)> c)
    {
        Hit?.Invoke(c);
    }
}