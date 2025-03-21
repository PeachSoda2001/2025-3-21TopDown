using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOnQuery : MonoBehaviour
{
    [SerializeField]
    [Tooltip("How to get the point that should be grappled to")]
    CharacterQueryBehavior Querier;

    [SerializeField]
    [Tooltip("Amount of hitpoints to subtract from every Game Object with Health hit.")]
    float Damage = 1;

    private void OnEnable()
    {
        Querier.Hit += DoDamage;
    }

    private void OnDisable()
    {
        Querier.Hit -= DoDamage;
    }

    private void DoDamage(List<(Collider, Vector3)> damagedObjects)
    {
        foreach ((Collider collider, Vector3 _) in damagedObjects)
        {
            if (collider.TryGetComponent(out Health health))
            {
                health.DoDamage(Damage);
            }
        }
    }
}
