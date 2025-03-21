using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WallClimb))]
public class WallClimbEvents : MonoBehaviour
{
    void Awake()
    {
        var wallclimb = GetComponent<WallClimb>();

        wallclimb.StartedClimbing += OnClimbStart;
        wallclimb.StoppedClimbing += OnClimbStop;
        wallclimb.JumpingFromWall += OnJumpFromWall;
        wallclimb.DroppingFromWall += OnDropFromWall;
    }

    void OnClimbStart()
    {
        Debug.Log("Character started climbing");
    }

    void OnClimbStop()
    {
        Debug.Log("Character stopped climbing");
    }

    void OnJumpFromWall()
    {
        Debug.Log("Character jumped from wall");
    }

    void OnDropFromWall()
    {
        Debug.Log($"Character dropped from wall");
    }
}
