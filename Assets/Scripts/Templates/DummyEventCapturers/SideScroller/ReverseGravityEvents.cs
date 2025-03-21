using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ReverseGravity))]
public class ReverseGravityEvents : MonoBehaviour
{
    void Awake()
    {
        var reverseGravity = GetComponent<ReverseGravity>();

        reverseGravity.FlipRequested += OnFlipRequest;
        reverseGravity.Flipped += OnFlip;
    }

    void OnFlipRequest()
    {
        Debug.Log("Character wants to flip");
    }

    void OnFlip(int direction)
    {
        Debug.Log($"Character flipped to direction {direction}");
    }
}