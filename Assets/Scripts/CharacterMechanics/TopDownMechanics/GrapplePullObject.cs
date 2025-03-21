using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class GrapplePullObject : MonoBehaviour
{

    [SerializeField]
    [Tooltip("How to get the point that should be grappled to")]
    CharacterQueryBehavior Querier;

    [SerializeField]
    [Tooltip("The time it will take to be pulled to the point grappled to.")]
    float PullTime = 0.1f;

    // TODO Maybe in a future semester
    //[SerializeField]
    //[Tooltip("The maxmimum distance the object will move")]
    //float PullDistance = 3;

    //[SerializeField]
    //[Tooltip("Changes Pull Distance to be a percentage of the overall distance from the character to the object")]
    //bool UsePullDistanceAsPercent = false;

    [SerializeField]
    [Tooltip("How fast to pull the object")]
    float PullingSpeed = 10;

    [SerializeField]
    [Tooltip("If true, the object will follow this transform until it reaches this. It is recommended to only use this if you are pulling triggers.")]
    bool FollowGrappler = false;

    // the bodies this puller is acting on, so we can cancel the coroutines if we pull them again
    Dictionary<Rigidbody, Coroutine> currentlyPulling = new();

    private void Awake()
    {
        // TODO give warnings if any necessary fields are missing
    }

    private void OnEnable()
    {
        Querier.Hit += DoPullObject;
    }

    private void OnDisable()
    {
        Querier.Hit -= DoPullObject;
    }

    void DoPullObject(List<(Collider, Vector3)> hitObjects)
    {
        foreach ((Collider collider, Vector3 pointHit) in hitObjects)
        {
            bool hasBody = collider.TryGetComponent(out Rigidbody body);

            Assert.IsTrue(hasBody, "Tried to grab object without rigidbody!!!");

            bool hasOverrides = collider.TryGetComponent(out PullableItemOverrides overrides);

            float pullTime = hasOverrides && overrides.OverridePullTime ? overrides.PullTimeOverride : PullTime;
            float pullingSpeed = hasOverrides && overrides.OverridePullingSpeed ? overrides.PullingSpeedOverride : PullingSpeed;
            bool followGrappler = hasOverrides && overrides.OverrideFollowGrappler ? overrides.FollowGrappler : FollowGrappler;

            if (currentlyPulling.ContainsKey(body))
            {
                StopCoroutine(currentlyPulling[body]);
            }

            currentlyPulling[body] = StartCoroutine(
                PullObjectTowards(
                    body,
                    pointHit,
                    transform.position,
                    pullTime,
                    pullingSpeed,
                    followGrappler
                )
            );
        }
    }

    Vector3 pullVector(Rigidbody body, Vector3 pointHit, Vector3 source, bool followGrappler)
    {
        if (followGrappler)
        {
            return transform.position - body.position;
        }

        return source - pointHit;
    }

    IEnumerator PullObjectTowards(Rigidbody body, Vector3 pointHit, Vector3 sourcePoint, float pullTime, float pullSpeed, bool followGrappler)
    {
        float overallPullTime = pullTime;
        float elapsedTime = 0;

        RigidbodyConstraints oldConstraints = body.constraints;
        body.constraints = RigidbodyConstraints.FreezeRotation;

        while (elapsedTime < overallPullTime)
        {
            body.velocity = pullVector(body, pointHit, sourcePoint, followGrappler).normalized * pullSpeed;
            if (!body.gameObject.activeSelf) break;
            if (!followGrappler && body.position == sourcePoint) break;

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        body.velocity = Vector3.zero;
        body.constraints = oldConstraints;

        currentlyPulling.Remove(body);
    }
        
}
