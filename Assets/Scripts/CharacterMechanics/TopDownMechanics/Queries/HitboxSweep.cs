using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class HitboxSweep : CharacterQueryBehavior
{
    #region Inspector Properties
    [SerializeField]
    [Tooltip("All the hitboxes to query the objects within.")]
    public List<HitboxQueryPart> Hitboxes;

    [Space(10)]

    [SerializeField]
    [Tooltip("How many objects this hitbox can read at once. Items closer to this transform (that this script is on) will be prioritized.")]
    public int MaximumObjectsToHit = 1;

    [Space(10)]

    [SerializeField]
    [Tooltip("If changed to a positive layer, only objects on that layer will be queried. Otherwise all objects are queried.")]
    public LayerMask AllowedLayers = ControlConstants.RAYCAST_MASK;

    [SerializeField]
    [Tooltip("If true, there must be an unobstructed path from this transform (that the script is on) to the item in the hitbox in order to be queried. " +
        "If it seems like too much is being blocked, make sure your hitbox is on an ignored layer, like Layer 2 (Ignore Raycast) or Layer 3 (Character).")]
    public bool RequiresLineOfSight = false;

    [Space(10)]

    [SerializeField]
    [Tooltip("The time to wait before the hitbox can be checked again.")]
    public float Cooldown = 1;

    [Space(10)]

    [SerializeField]
    [Tooltip("The input to check the hitbox with.")]
    InputAction SweepHitboxes;
    #endregion Inspector Properties

    #region Events
    public event Action<int> HitboxSwept;
    #endregion

    #region Computed Properties
    bool CooldownActive => Time.time - LastTimeSwept < Cooldown;
    #endregion

    float LastTimeSwept;

    void OnEnable()
    {
        SweepHitboxes.performed += DoSweep;
        SweepHitboxes.Enable();
    }

    void OnDisable()
    {
        SweepHitboxes.performed -= DoSweep;
    }

    void DoSweep(CallbackContext c)
    {
        if (CooldownActive)
        {
            return;
        }

        LastTimeSwept = Time.time;

        List<Collider> collidersToHit = Hitboxes
            .SelectMany(hitbox => hitbox.Intersecting) // all intersecting colliders

            .GroupBy(x => x) // remove duplicates
            .Select(duplicateGroup => duplicateGroup.First())

            .Where(collider => LayerUtil.IsEnabledInMask(AllowedLayers, collider.gameObject.layer)) // filter by layer

            .OrderBy(collider => (collider.transform.position - transform.position).magnitude) // take the closest N colliders
            .Take(MaximumObjectsToHit)

            .Where(collider => !RequiresLineOfSight || Physics.Raycast( // if line of sight is required, check that it is visible from this object's transform
                    new Ray(transform.position, transform.position - collider.transform.position),
                    (transform.position - collider.transform.position).magnitude,
                    ControlConstants.RAYCAST_MASK,
                    QueryTriggerInteraction.Ignore
                )
            )

            .ToList();

        OnHit(collidersToHit.Select(x => (x, x.transform.position)).ToList());
        HitboxSwept?.Invoke(collidersToHit.Count());
    }
}
