using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Jetpack))]
public class JetpackEvents : MonoBehaviour
{
    void Awake()
    {
        var jetpack = GetComponent<Jetpack>();

        jetpack.StartedFlying += OnFlyingStart;
        jetpack.StoppedFlying += OnFlyingStop;
        jetpack.DeniedFlight += OnFlightDenied;
        jetpack.RanOutOfFuel += OnNoFuel;
        jetpack.StartedRefuelling += OnRefuellingStart;
        jetpack.StoppedRefuelling += OnRefuellingStop;
    }

    void OnFlyingStart()
    {
        Debug.Log("Started flying");
    }

    void OnFlyingStop()
    {
        Debug.Log("Stopped flying");
    }

    void OnFlightDenied()
    {
        Debug.Log("Flight denied");
    }

    void OnRefuellingStart()
    {
        Debug.Log("Refuelling started");
    }

    void OnRefuellingStop()
    {
        Debug.Log("Refuelling stopped");
    }

    void OnNoFuel()
    {
        Debug.Log("Ran out of Fuel");
    }
}
