using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(CharacterMovement))]
public class Jetpack : MonoBehaviour
{
    /**         START USER PROPERTIES           **/

    [Space(10)]

    [SerializeField]
    [Tooltip("The input to start the jetpack. Holding to boost.")]
    InputAction Boost;

    [SerializeField]
    [Min(0.01f)]
    [Tooltip("The acceleration that opposes gravity. Character will only fly upward when this number is higher than the character gravity.")]
    public float FlightAcceleration = 96;

    [SerializeField]
    [Min(0.01f)]
    [Tooltip("The maximum upward speed while the jetpack is active.")]
    public float MaxFlightSpeed = 24;

    [Space(10)]

    [SerializeField]
    [Min(0)]
    [Tooltip("The initial instant velocity applied when the jetpack is started.")]
    public float TakeOffSpeed = 0;

    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("How much of the downward velocity will be kept when the jetpack starts. 0 will feel snappy and cancel everything, 1 will feel a lot more gradual and realistic.")]
    public float FallingVelocityRetention = 0.1f; // 0-1 how much of fall velocity to retain when starting the jetpack

    [Space(10)]

    [SerializeField]
    [Min(0.01f)]
    [Tooltip("The maximum duration of the jetpack before refueling (in seconds).")]
    public float Duration = 5;

    [SerializeField]
    [Min(0)]
    [Tooltip("How fast to refuel the jetpack in seconds of fuel per second.")]
    public float RefuelSpeed = 2;

    [SerializeField]
    [Tooltip("Whether the jetpack can automatically refuel while in the air. Only refuels while the jetpack isn't being used.")]
    public bool RefuelInMidair = false;

    [SerializeField]
    [Tooltip("Whether the jetpack can automatically refuel while on the ground.")]
    public bool RefuelOnGround = true;

    /**         END USER PROPERTIES                 **/

    /**         START EVENTS                        **/

    public event Action StartedFlying; // jetpack starts
    public event Action StoppedFlying; // jetpack stops
    public event Action DeniedFlight; // player tried to use jetpack but no fuel
    public event Action RanOutOfFuel; // player ran out of fuel
    public event Action StartedRefuelling; // started refuelling jetpack
    public event Action StoppedRefuelling; // stopped refuelling jetpack

    /**         END EVENTS                          **/

    public bool IsFlying
    {
        get; private set;
    } = false;

    [SerializeField]
    [Tooltip("Not for use in editor, just good for testing to see how much fuel you currently have.")]
    public float FuelLeft;

    public bool HasFuel => FuelLeft > 0;

    public bool IsRefueling { get; private set; } = false;


    CharacterMovement movement;
    const float dx = 0.01f;

    void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        FuelLeft = Duration;
    }

    void OnEnable()
    {
        if (Boost is not null)
        {
            Boost.performed += DoBoost;
            Boost.canceled += DoBoost;
            Boost.Enable();
        }
    }

    void OnDisable()
    {
        if (Boost is not null)
        {
            Boost.performed -= DoBoost;
            Boost.canceled -= DoBoost;
            Boost.Disable();
        }
    }

    void DoBoost(CallbackContext c)
    {
        if (!HasFuel)
        {
            DeniedFlight?.Invoke();
            return;
        }

        bool startedFlying = c.phase == InputActionPhase.Performed;

        IsFlying = startedFlying;

        if (!startedFlying)
        {
            if (HasFuel && !movement.IsOnStableGround())
            {
                StoppedFlying?.Invoke();
            }

            return;
        }

        StartedFlying?.Invoke();

        // only retain downward velocity
        if (movement.VerticalSpeed < 0)
        {
            movement.VerticalSpeed *= FallingVelocityRetention;
        }

        // check if we're moving upward so we don't apply the take off speed while already boosting
        if (movement.VerticalSpeed < dx)
        {
            movement.VerticalSpeed += TakeOffSpeed;
        }
    }

    void TryRefuel(float fuelToGive)
    {
        bool canRefuelGround = RefuelOnGround && movement.IsOnStableGround();
        bool canRefuelMidair = RefuelInMidair && !movement.IsOnStableGround() && !IsFlying;

        if (FuelLeft >= Duration || (!canRefuelGround && !canRefuelMidair))
        {
            IsRefueling = false;
            return;
        }

        IsRefueling = true;
        FuelLeft = Mathf.Min(Duration, FuelLeft + fuelToGive);
    }

    void UseFuel()
    {
        bool hadFuel = HasFuel;

        FuelLeft = Mathf.Max(0, FuelLeft - Time.deltaTime);

        if (hadFuel && !HasFuel)
        {
            RanOutOfFuel?.Invoke();
            StoppedFlying?.Invoke();
        }
    }

    void Update()
    {
        if (!HasFuel)
        {
            IsFlying = false;
        }

        bool wasRefueling = IsRefueling;

        TryRefuel(Time.deltaTime * RefuelSpeed);


        if (!wasRefueling && IsRefueling)
        {
            StartedRefuelling?.Invoke();
        } 
        else if (wasRefueling && !IsRefueling)
        {
            StoppedRefuelling?.Invoke();
        }

        if (IsFlying)
        {
            UseFuel();

            movement.VerticalSpeed += Time.deltaTime * FlightAcceleration;
            movement.VerticalSpeed = Mathf.Min(movement.VerticalSpeed, MaxFlightSpeed);
        }
    }
}
