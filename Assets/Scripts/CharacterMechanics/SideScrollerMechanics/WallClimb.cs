using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;


[RequireComponent(typeof(CharacterMovement))]
public class WallClimb : MonoBehaviour
{

    #region Inspector Properties
    [SerializeField]
    [Tooltip("The keybinds for climbing up and down. Use Up/Down/Left/Right Composite")]
    InputAction Climb;

    [SerializeField]
    [Tooltip("The speed the character climbs at in units/second.")]
    float ClimbSpeed = 10;

    [SerializeField]
    [Tooltip("If true, the climb speed will be overwritten with the character's walk speed.")]
    bool ClimbSpeedMatchWalkSpeed = false;

    [SerializeField]
    [Tooltip("If true, player will grab the wall if they pass over it with Up or Down being held. If false, they will have to press the button while on the wall to grab it.")]
    bool GrabWhileHoldingInput = true;

    [SerializeField]
    [Tooltip("If true, the player won't grab a wall until the apex of their jump, this allows players to hold up and jump up walls. This will only work if Grab While Holding Input set to true.")]
    bool GrabAtApex = true;

    [Space(10)]

    [SerializeField]
    [Tooltip("The keybind for dropping off a wall.")]
    InputAction Drop;
    #endregion

    #region Events
    public event Action StartedClimbing;
    public event Action StoppedClimbing;
    public event Action JumpingFromWall;
    public event Action DroppingFromWall;
    #endregion

    [NonSerialized]
    public bool isClimbing = false;
    bool wantsToClimb = false;
    bool dropped = false;

    Vector2 ClimbDirection = Vector2.zero;

    CharacterMovement movement;
    CharacterController controller;

    GameObject LastWall;
    Vector3 LastWallPosition;

    Vector3 ClimbVelocity = Vector3.zero;

    private void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        controller = GetComponent<CharacterController>();
    }

    void OnEnable()
    {
        if (Climb is not null)
        {
            Climb.performed += DoClimb;
            Climb.canceled += CancelClimb;
            Climb.Enable();
        }

        if (Drop is not null)
        {
            Drop.performed += DoDrop;
            Drop.Enable();
        }

        movement.JumpRequested += JumpOff;
        movement.Landed += CancelDrop;
    }

    private void OnDisable()
    {
        if (Climb is not null)
        {
            Climb.performed -= DoClimb;
            Climb.canceled -= CancelClimb;
            Climb.Disable();
        }

        if (Drop is not null)
        {
            Drop.performed -= DoDrop;
            Drop.Disable();
        }

        movement.JumpRequested -= JumpOff;
        movement.Landed -= CancelDrop;

        if (isClimbing)
        {
            StopClimb();
        }
    }

    void JumpOff()
    {
        if (!isClimbing)
        {
            return;
        }

        JumpingFromWall?.Invoke();
        StopClimb();
    }

    void DoClimb(CallbackContext c)
    {
        if (c.ReadValue<Vector2>().y != 0)
        {
            wantsToClimb = true;
        }
        
        dropped = false;
        ClimbDirection = c.ReadValue<Vector2>();

        if (!GrabWhileHoldingInput)
        {
            TryStartClimb();
        }
    }
    
    void CancelClimb(CallbackContext c)
    {
        ClimbDirection = c.ReadValue<Vector2>();

        if (ClimbDirection.magnitude == 0)
        {
            wantsToClimb = false;
        }
    }

    void DoDrop(CallbackContext c)
    {
        if (!isClimbing)
        {
            return;
        }

        dropped = true;
        DroppingFromWall?.Invoke();
        StopClimb();
    }

    void CancelDrop()
    {
        dropped = false;
    }

    bool IsInLimiter(LimitClimbingPosition limiter, Vector3 addTo)
    {
        Vector3 checkPos = controller.transform.position + addTo;

        return (checkPos - limiter.LimitingVolume.ClosestPoint(checkPos)).magnitude < 0.001f;
    }

    void SnapToLimiter()
    {
        if (GetWall() != null && GetWall().TryGetComponent(out LimitClimbingPosition limiter))
        {
            controller.Move(limiter.LimitingVolume.ClosestPoint(controller.transform.position) - controller.transform.position);
        }
    }

    bool IsOnClimbableWall(Vector3? addToPosition = null, bool checkInsideLimiter = true)
    {
        Vector3 addTo = addToPosition ?? Vector3.zero;

        bool didHit = Physics.CapsuleCast(
            movement.TopSphereCenter + addTo, movement.BottomSphereCenter + addTo, controller.radius, 
            movement.Camera.transform.forward, out RaycastHit hit, 999, ControlConstants.CLIMBABLE_WALL_RAYCAST_MASK
        );

        if (didHit && checkInsideLimiter && hit.transform.gameObject.TryGetComponent(out LimitClimbingPosition limiter))
        {
            return IsInLimiter(limiter, addTo);
        }

        return didHit;
    }

    GameObject GetWall()
    {
        if (Physics.CapsuleCast(
            movement.TopSphereCenter, movement.BottomSphereCenter, controller.radius,
            movement.Camera.transform.forward, out RaycastHit wallHit, 999, ControlConstants.CLIMBABLE_WALL_RAYCAST_MASK
        )) {
            return wallHit.transform.gameObject;
        }

        return null;
    }

    bool TryStartClimb()
    {
        if (!IsOnClimbableWall(Vector3.zero, false))
        {
            return false;
        }

        if (!movement.IsOnStableGround() && GrabAtApex && GrabWhileHoldingInput && !movement.IsFalling)
        {
            return false;
        }

        StartClimb();

        return true;
    }

    void StartClimb()
    {
        if (isClimbing)
        {
            return;
        }

        isClimbing = true;
        movement.VerticalMovementEnabled = false;
        movement.LateralMovementEnabled = false;
        movement.DoPlatformTracking = false;
        SnapToLimiter();

        StartedClimbing?.Invoke();
    }

    void StopClimb()
    {
        if (!isClimbing)
        {
            return;
        }

        isClimbing = false;
        movement.VerticalMovementEnabled = true;
        movement.LateralMovementEnabled = true;
        movement.DoPlatformTracking = true;

        LastWall = null;

        StoppedClimbing?.Invoke();
    }

    Vector3 NextClimbDelta()
    {
        return ClimbDirection * ClimbSpeed * Time.deltaTime;
    }

    bool IsOnSameWall()
    {
        return IsOnClimbableWall() && GetWall() == LastWall && LastWall != null;
    }

    void MoveWithWall()
    {
        ClimbVelocity += LastWall.transform.position - LastWallPosition;
    }

    void UpdateLastWall()
    {
        LastWall = GetWall();

        if (GetWall() != null)
        {
            LastWallPosition = LastWall.transform.position;
        }
    }

    bool CurrentWallHasLimiter()
    {
        return isClimbing && GetWall() != null && GetWall().TryGetComponent<LimitClimbingPosition>(out _);
    }

    bool LimitingWallCanBeClimbedOutOf()
    {
        return GetWall().GetComponent<LimitClimbingPosition>().CanClimbOutOfLimiter;
    }

    bool CanBreakOutOfLimiter()
    {
        Vector2 horizontalComponent = Vector2.Scale(NextClimbDelta(), Vector2.right);
        Vector2 verticalComponent = Vector2.Scale(NextClimbDelta(), Vector2.up);

        bool horizontalBreaksOut = !IsOnClimbableWall(horizontalComponent);
        bool verticalBreaksOut = !IsOnClimbableWall(verticalComponent);

        bool horizontalMax = horizontalComponent.magnitude > verticalComponent.magnitude;
        bool verticalMax = horizontalComponent.magnitude > verticalComponent.magnitude;

        return horizontalBreaksOut && horizontalMax || verticalBreaksOut && verticalMax;
    }

    bool isExitingAtTopOfWall()
    {
        Vector2 verticalDelta = Vector2.Scale(NextClimbDelta(), Vector2.up);
        return !IsOnClimbableWall(verticalDelta) && Vector3.Dot(movement.GravityUpDirection, verticalDelta) > 0;
    }

    void Update()
    {
        if (ClimbSpeedMatchWalkSpeed)
        {
            ClimbSpeed = movement.WalkSpeed;
        }

        ClimbVelocity = NextClimbDelta();

        if (CurrentWallHasLimiter() && LimitingWallCanBeClimbedOutOf() && CanBreakOutOfLimiter())
        {
            wantsToClimb = false;
            StopClimb();
            return;
        }

        if (IsOnSameWall())
        {
            MoveWithWall();
        } 
        else if (isClimbing && wantsToClimb)
        {
            SnapToLimiter();
        }

        if (isClimbing && !IsOnClimbableWall())
        {
            StopClimb();
        }

        if (GrabWhileHoldingInput && wantsToClimb && !dropped && !isClimbing)
        {
            TryStartClimb();
        }

        if (isClimbing)
        {
            UpdateLastWall();

            // no limiter behavior
            if (!CurrentWallHasLimiter())
            {
                if (isExitingAtTopOfWall())
                {
                    ClimbVelocity = Vector2.Scale(ClimbVelocity, Vector2.right);
                }

                controller.Move(ClimbVelocity);
                return;
            }

            Vector2 horizontalComponent = Vector2.Scale(NextClimbDelta(), Vector2.right);
            Vector2 verticalComponent = Vector2.Scale(NextClimbDelta(), Vector2.up);

            bool onWallAfterHorizontal = IsOnClimbableWall(horizontalComponent);
            bool onWallAfterVertical = IsOnClimbableWall(verticalComponent);

            bool canMoveVertical = onWallAfterVertical || (!onWallAfterVertical && LimitingWallCanBeClimbedOutOf() && (!onWallAfterHorizontal || horizontalComponent.magnitude == 0));
            bool canMoveHorizontal = onWallAfterHorizontal || (!onWallAfterHorizontal && LimitingWallCanBeClimbedOutOf() && (!onWallAfterVertical || verticalComponent.magnitude == 0));

            if (canMoveHorizontal)
            {
                controller.Move(horizontalComponent);
            }

            if (canMoveVertical && !isExitingAtTopOfWall())
            {
                controller.Move(verticalComponent);
            }
        }


    }
}
