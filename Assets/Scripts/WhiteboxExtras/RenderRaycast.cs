using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(RaycastQuery))]
public class RenderRaycast : MonoBehaviour
{
    [SerializeField]
    [Tooltip("How long it'll be on screen")]
    float Duration = 0.2f;

    RaycastQuery RaycastQuery => GetComponent<RaycastQuery>();
    LineRenderer Line => GetComponent<LineRenderer>();

    float LastStartedRendering = -999;

    void OnEnable()
    {
        RaycastQuery.Raycasted += SetLineHitSomething;
        RaycastQuery.Raycasted += SetLineHitNothing;
    }

    void OnDisable()
    {
        RaycastQuery.Raycasted -= SetLineHitSomething;
        RaycastQuery.Raycasted -= SetLineHitNothing;
    }

    void SetLineHitSomething(Ray ray, List<RaycastHit> hits)
    {
        if (hits.Count() != RaycastQuery.MaximumObjectsToHit)
        {
            return;
        }

        Line.SetPositions(new Vector3[] { ray.origin, hits.Last().point });
        LastStartedRendering = Time.time;
    }

    void SetLineHitNothing(Ray ray, List<RaycastHit> hits)
    {
        if (hits.Count >= RaycastQuery.MaximumObjectsToHit)
        {
            return;
        }

        Line.SetPositions(new Vector3[] { ray.origin, ray.origin + ray.direction * RaycastQuery.MaximumDistance });
        LastStartedRendering = Time.time;
    }

    void Update()
    {
        Line.enabled = Time.time - LastStartedRendering <= Duration;
    }
}
