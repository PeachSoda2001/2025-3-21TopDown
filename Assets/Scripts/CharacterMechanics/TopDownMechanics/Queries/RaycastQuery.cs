using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class RaycastQuery : CharacterQueryBehavior
{
    #region Inspector Properties
    [SerializeField]
    [Tooltip("The camera the player is viewing from. Needed if the mouse position is being used for targeting.")]
    public Camera Viewport;

    [Space(10)]

    // TODO maybe in a future semester
    //[SerializeField]
    //[Tooltip("The offset from the targeted point as an angle from the origin of the raycast around each axis. For a topdown game, only the Y should matter.")]
    //public Vector3 TargetAngularOffset = Vector3.zero;

    [SerializeField]
    [Tooltip("The maximum distance of the raycast.")]
    public float MaximumDistance = 20;

    [SerializeField]
    [Tooltip("How many objects the raycast should hit. If the raycast has not hit this many objects, it will continue until it reaches its maximum distance.")]
    public int MaximumObjectsToHit = 1;

    [Space(10)]

    [SerializeField]
    [Tooltip("Only objects on these layers will be queried when casting")]
    public LayerMask AllowedLayers = ControlConstants.RAYCAST_MASK;

    [SerializeField]
    [Tooltip("If true, all disallowed layers will be passed through when raycasting and not will not stop the query from finding objects beyond them.")]
    public bool PassThroughDisallowedLayers = false;

    [SerializeField]
    [Tooltip("If true, all triggers will be ignored when casting.")]
    public bool IgnoreTriggers = true;

    [Space(10)]

    [SerializeField]
    [Tooltip("The time to wait before another cast can begin.")]
    public float Cooldown = 1;

    [Space(20)]

    [SerializeField]
    [Tooltip("The input to shoot the raycast with.")]
    InputAction Raycast;

    [SerializeField]
    [Tooltip("The input to target for the raycast. If mouse position is used, the target will be where the mouse clicks. " +
        "If Up/Down/Left/Right composite is used, the target will be positioned relative to the origin in that direction from the camera. " +
        "Otherwise, the ray will fire in the direction the character is looking.")]
    InputAction Target;

    [SerializeField]
    [Tooltip("Snaps the targeted point to the y plane of this object's transform. " +
        "If desired, setting this to false will let the raycast target objects below and above the player with a mouse." +
        "Has no effect if targeting with a 2D vector.")]
    bool SnapTargetToYPlane = true;
    #endregion Inspector Properties

    #region Events
    public event Action<Ray, List<RaycastHit>> Raycasted;
    #endregion

    #region Computed Properties
    bool CooldownActive => Time.time - LastTimeRaycasted < Cooldown;
    #endregion

    float LastTimeRaycasted = 0;

    string targetingDevice;
    Vector3 rawTarget;

    void OnEnable()
    {
        Raycast.performed += DoRaycast;
        Raycast.Enable();
        Target.performed += SetTargetContext;
        Target.Enable();
    }

    void OnDisable()
    {
        Raycast.performed -= DoRaycast;
        Target.performed -= SetTargetContext;
    }

    void SetTargetContext(CallbackContext c)
    {
        targetingDevice = c.control.device.description.deviceClass;
        rawTarget = c.ReadValue<Vector2>();
    }

    void DoRaycast(CallbackContext c)
    {
        if (CooldownActive)
        {
            return;
        }

        LastTimeRaycasted = Time.time;

        Vector3 targetDirection = GetTargetDirection().normalized;

        Vector3 origin = transform.position;

        RaycastHit[] potentialHits = Physics.RaycastAll(
            origin, 
            targetDirection, 
            MaximumDistance, 
            PassThroughDisallowedLayers ? AllowedLayers : ControlConstants.RAYCAST_MASK,
            IgnoreTriggers ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide
        );

        // order based on distance
        List<RaycastHit> closestFirst = potentialHits.OrderBy(hit => (hit.point - origin).magnitude).ToList();

        // filter based on layer
        List<RaycastHit> onlyBeforeMiss = FilterToAllowedLayersBeforeFirstMiss(closestFirst);

        // filter based on number of objects (closest get priority)
        List<RaycastHit> constrainedToObjectNumber = onlyBeforeMiss.Take(MaximumObjectsToHit).ToList();

        List<RaycastHit> actuallyHit = constrainedToObjectNumber;

        OnHit(GetColliderPointPairs(actuallyHit));

        Raycasted?.Invoke(new Ray(origin, targetDirection), actuallyHit);
    }

    Vector3 GetTargetDirection()
    {
        if (targetingDevice == "Mouse")
        {
            Ray mouseRay = Viewport.ScreenPointToRay(rawTarget);

            float distanceTraveled;

            if (SnapTargetToYPlane)
            {
                new Plane(Vector3.up, transform.position).Raycast(mouseRay, out distanceTraveled);
            } 
            else
            {
                Physics.Raycast(mouseRay, out RaycastHit hit, 999, AllowedLayers, IgnoreTriggers ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
                distanceTraveled = hit.distance;
            }

            return mouseRay.GetPoint(distanceTraveled) - transform.position;
        }

        return rawTarget.magnitude != 0 ? Viewport.transform.TransformDirection(rawTarget) : transform.forward;
    }

    List<RaycastHit> FilterToAllowedLayersBeforeFirstMiss(List<RaycastHit> raycastHits)
    {
        List<RaycastHit> newHits = new();

        foreach (var hit in raycastHits)
        {
            bool isAllowed = LayerUtil.IsEnabledInMask(AllowedLayers, hit.collider.gameObject.layer);

            // if pass through isn't allowed, we have to break off the chain here
            if (!PassThroughDisallowedLayers && !isAllowed)
            {
                break;
            }

            if (isAllowed)
            {
                newHits.Add(hit);
            }
        }

        return newHits;
    }

    List<(Collider, Vector3)> GetColliderPointPairs(List<RaycastHit> raycastHits)
    {
        return raycastHits.Select((hit) => (hit.collider, hit.point)).ToList();
    }
}
