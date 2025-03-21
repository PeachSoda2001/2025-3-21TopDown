using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrapplePullPlayer : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The movement controlling this character")]
    CharacterMovement Movement;

    [SerializeField]
    [Tooltip("How to get the point that should be grappled to")]
    CharacterQueryBehavior Querier;

    [SerializeField]
    [Tooltip("How fast the character will be pulled towards their destination.")]
    float PullSpeed = 50f;

    [SerializeField]
    [Tooltip("The maximum time the character will be pulled for. If the character arrives at their destination " +
        "they will stop grappling, so this is the time they will be pulled for given that they don't reach the destination.")]
    float MaximumPullTime = 5f;

    [SerializeField]
    [Tooltip("Whether or not gravity will apply while being pulled")]
    bool GravityWhilePulling = false;

    [SerializeField]
    [Tooltip("Whether or not the character can move laterally while being pulled")]
    bool MovementWhilePulling = false;

    Maid enableMaid = new();
    Maid grapplingMaid = new();

    float dx = 0.1f;

    #region Events
    public event Action GrappleStarting;
    public event Action GrappleCanceled;
    public event Action GrappleFinished;
    #endregion

    private void Awake()
    {
        Debug.Assert(Movement is not null, "Must provide a CharacterMovement for GrapplePullPlayer!");
        Debug.Assert(Querier is not null, "Must provide a Querier for GrapplePullPlayer!");
    }

    private void OnEnable()
    {
        enableMaid.GiveEvent(Querier, "Hit", (List<(Collider, Vector3)> x) => DoPullPlayer(x));
        enableMaid.GiveEvent(Movement, "Warped", (Vector3 _) => CancelPull());
        enableMaid.GiveTask(grapplingMaid);
    }

    private void OnDisable()
    {
        enableMaid.Cleanup();
    }

    void DoPullPlayer(List<(Collider c, Vector3 p)> points)
    {
        if (points.Count > 0)
        {
            GrappleStarting?.Invoke();
            grapplingMaid.Cleanup();

            grapplingMaid.GiveTask(() =>
            {
                Movement.GiveImpulse(Vector3.zero, 0);
                Movement.LateralMovementEnabled = true;
                Movement.GravityEnabled = true;
            });

            grapplingMaid.GiveCoroutine(this, StartCoroutine(PullPlayerTowards(points.First().p)));
        }
    }

    bool shouldStopGrapple(float grappleStartedTime, Plane pullingTowardsPlane, bool initialSide)
    {
        Transform characterTransform = Movement.transform;
        CharacterController controller = Movement.GetComponent<CharacterController>();

        bool isTimeUp = Time.time - grappleStartedTime > MaximumPullTime; // check for time
        bool passedGrapplePoint = pullingTowardsPlane.GetSide(characterTransform.position) != initialSide; // check if they've gone passed the plane

        Vector3 closestPointOnPlaneToCharacter = pullingTowardsPlane.ClosestPointOnPlane(characterTransform.position);
        Vector3 closestPointOnCharacterToPlane = controller.ClosestPoint(closestPointOnPlaneToCharacter);
        // check if the character arrived at the point (might not be able to go passed the plane)
        bool reachedPoint = (closestPointOnPlaneToCharacter - closestPointOnCharacterToPlane).magnitude < dx + controller.skinWidth;

        return isTimeUp || reachedPoint || passedGrapplePoint;
    }

    IEnumerator PullPlayerTowards(Vector3 towards)
    {
        Transform characterTransform = Movement.transform;
        CharacterController controller = Movement.GetComponent<CharacterController>();
        Plane pullingTowardsPlane = new((towards - characterTransform.position).normalized, towards);
        
        Movement.GravityEnabled = GravityWhilePulling;
        Movement.LateralMovementEnabled = MovementWhilePulling;
        Movement.GiveImpulse(PullSpeed * pullingTowardsPlane.normal, MaximumPullTime, VerticalMovementState.Jumping);

        float startTime = Time.time;
        bool initialSide = pullingTowardsPlane.GetSide(characterTransform.position);

        yield return new WaitUntil(
            () => shouldStopGrapple(startTime, pullingTowardsPlane, initialSide)
        );

        // TODO at some point it might be good to gradually ween off the impulse if the character went passed the grapple point
        // eg: if they moved and flew past it

        GrappleFinished?.Invoke();
        grapplingMaid.Cleanup();
    }

    void CancelPull()
    {
        GrappleCanceled?.Invoke();
        grapplingMaid.Cleanup();
    }
}
