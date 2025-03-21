using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(GravityField))]
public class GravityFieldEvents : MonoBehaviour
{
    void Awake()
    {
        var reverseGravity = GetComponent<GravityField>();

        reverseGravity.EnteredField += OnEnter;
        reverseGravity.ExitedField += OnExit;
    }

    void OnEnter()
    {
        Debug.Log("Character entered field");
    }

    void OnExit()
    {
        Debug.Log("Character exited field");
    }
}
