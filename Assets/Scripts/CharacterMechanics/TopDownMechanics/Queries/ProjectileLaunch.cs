using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class ProjectileLaunch : CharacterQueryBehavior
{
    #region Inspector Properties
    [SerializeField]
    [Tooltip("The camera the player is viewing from. Needed if the mouse position is being used for targetting.")]
    public Camera Viewport;

    [SerializeField]
    [Tooltip("This projectile will be launched. Object must have a rigidbody attached.")]
    public QueryingProjectile ProjectilePrefab;

    [Space(10)]
    
    [SerializeField]
    [Tooltip("The speed of this projectile when it's launched.")]
    public float LaunchSpeed = 5;

    [SerializeField]
    [Tooltip("Additional velocity loss when the projectile collides with something. " +
        "The engine may already reduce velocity based on elasticity and friction values. " +
        "To more finely control this, use a physics material on your projectile prefab.")]
    public float SpeedLossOnCollision = 0;

    [Space(10)]

    [SerializeField]
    [Tooltip("The maximum distance the projectile can travel before being destroyed.")]
    public float MaximumDistance = 20;

    [SerializeField]
    [Tooltip("The maximum time the projectile has before being destroyed.")]
    public float MaximumTimeActive = 5;

    [SerializeField]
    [Tooltip("How many objects the projectile can hit. On the final object, the projectile will disappear.")]
    public int MaximumObjectsToHit = 1;

    [Space(10)]

    [SerializeField]
    [Tooltip("Only objects on these layers will be queried when hit. Other objects will still have collisions.")]
    public LayerMask AllowedLayers = ControlConstants.RAYCAST_MASK;

    [SerializeField]
    [Tooltip("If true, collisions with disallowed layers will be queried, but not count towards the MaximumObjects count.")]
    public bool QueryCollisionsWithDisallowedLayers = false;

    [Space(10)]

    [SerializeField]
    [Tooltip("The time to wait between launches.")]
    public float Cooldown = 1;

    [Space(20)]

    [SerializeField]
    [Tooltip("The input to launch the projectile with.")]
    InputAction Launch;

    [SerializeField]
    [Tooltip("The input to target the direction the projectile is launched in. If mouse position is used, the target will be where the mouse clicks. " +
        "If Up/Down/Left/Right composite is used, the target will be positioned relative to the origin in that direction from the camera. " +
        "Otherwise, the projectile will fire in the direction the character is looking.")]
    InputAction Target;

    [SerializeField]
    [Tooltip("Snaps the targeted point to the y plane of this object's transform. " +
    "If desired, setting this to false will let the raycast target objects below and above the player with a mouse." +
    "Has no effect if targeting with a 2D vector.")]
    bool SnapTargetToYPlane = true;
    #endregion Inspector Properties

    #region Events
    public event Action<QueryingProjectile, Collision> ProjectileCollided;
    public event Action<QueryingProjectile> ProjectileLaunched;
    public event Action<QueryingProjectile> ProjectileDestroying;
    #endregion

    #region Computed Properties
    bool CooldownActive => Time.time - LastTimeLaunched < Cooldown;
    #endregion

    float LastTimeLaunched = 0;

    string targetingDevice;
    Vector3 rawTarget;

    Maid maid = new();

    void OnEnable()
    {
        maid.GiveEvent(Launch, "performed", (CallbackContext c) => DoLaunch(c));
        Launch.Enable();
        Target.performed += SetTargetContext;
        Target.Enable();
    }

    void OnDisable()
    {
        Launch.performed -= DoLaunch;
        Target.performed -= SetTargetContext;
    }

    void SetTargetContext(CallbackContext c)
    {
        targetingDevice = c.control.device.description.deviceClass;
        rawTarget = c.ReadValue<Vector2>();
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
                Physics.Raycast(mouseRay, out RaycastHit hit, 999, AllowedLayers, QueryTriggerInteraction.Ignore);
                distanceTraveled = hit.distance;
            }

            return mouseRay.GetPoint(distanceTraveled) - transform.position;
        }

        return rawTarget.magnitude != 0 ? Viewport.transform.TransformDirection(rawTarget) : transform.forward;
    }

    void DoLaunch(CallbackContext c)
    {
        if (CooldownActive)
        {
            return;
        }

        LastTimeLaunched = Time.time;

        QueryingProjectile projectile = Instantiate(ProjectilePrefab);

        ProjectileLaunched?.Invoke(projectile);

        Rigidbody body = projectile.GetComponent<Rigidbody>();

        projectile.transform.position = transform.position;
        body.velocity = GetTargetDirection().normalized * LaunchSpeed;

        // projectile cleans up all bound functions, so we don't need to worry about disconnecting this
        projectile.Collided += (Collision collision) =>
        {
            if (!QueryCollisionsWithDisallowedLayers && !LayerUtil.IsEnabledInMask(AllowedLayers, collision.collider.gameObject.layer))
            {
                return;
            }

            OnHit(new() { (collision.collider, collision.GetContact(0).point) });
            ProjectileCollided?.Invoke(projectile, collision);
            body.velocity -= SpeedLossOnCollision * body.velocity / body.velocity.magnitude;
        };

        StartCoroutine(RemoveWhenSatisfied(projectile));
    }

    IEnumerator RemoveWhenSatisfied(QueryingProjectile projectile)
    {
        float initialized = Time.time;
        float distanceTraveled = 0;
        int objectsHit = 0;

        projectile.Collided += (Collision collision) =>
        {
            objectsHit++;
        };

        while (true)
        {
            if (Time.time - initialized >= MaximumTimeActive)
            {
                break;
            }

            if (objectsHit >= MaximumObjectsToHit)
            {
                break;
            }

            if (distanceTraveled >= MaximumDistance)
            {
                break;
            }

            distanceTraveled += Time.deltaTime * projectile.GetComponent<Rigidbody>().velocity.magnitude;

            yield return new WaitForEndOfFrame();
        }

        ProjectileDestroying?.Invoke(projectile);

        Destroy(projectile.gameObject);
    }
}
