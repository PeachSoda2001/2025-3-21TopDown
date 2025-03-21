using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class MovementEvents : MonoBehaviour
{
    void OnEnable()
    {
        var movement = GetComponent<CharacterMovement>();

        movement.StartedWalking += OnWalkStart;
        movement.StoppedWalking += OnWalkStop;
        movement.JumpRequested += OnJumpRequest;
        movement.Jumped += OnJump;
        movement.Landed += OnLanding;
        movement.Falling += OnFalling;
    }

    void OnDisable()
    {
        var movement = GetComponent<CharacterMovement>();

        movement.StartedWalking -= OnWalkStart;
        movement.StoppedWalking -= OnWalkStop;
        movement.JumpRequested -= OnJumpRequest;
        movement.Jumped -= OnJump;
        movement.Landed -= OnLanding;
        movement.Falling -= OnFalling;
    }

    void OnWalkStart()
    {
        Debug.Log("Character started walking");
    }

    void OnWalkStop()
    {
        Debug.Log("Character stopped walking");
    }

    void OnJumpRequest()
    {
        Debug.Log("Character wanted to jump");
    }

    void OnJump(int n)
    {
        Debug.Log($"Character did jump number {n}");
    }

    void OnLanding()
    {
        Debug.Log("Character landed");
    }

    void OnFalling()
    {
        Debug.Log("Character falling");
    }
}
