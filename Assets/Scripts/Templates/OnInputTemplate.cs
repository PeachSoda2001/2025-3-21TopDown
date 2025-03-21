using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OnInputTemplate : MonoBehaviour
{
    [SerializeField]
    InputAction Input;

    readonly Maid maid = new();

    private void OnEnable()
    {
        Input.performed += DoSomething;
    }

    private void OnDisable()
    {
        Input.performed -= DoSomething;
    }

    // DoSomething is called every time the input is performed
    // context contains context about the input performed, like the phase of the interaction
    // check the documentation for more:
    // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputAction.CallbackContext.html
    private void DoSomething(InputAction.CallbackContext context)
    {
        Debug.Log($"Input performed!");
        // your code here
    }
}
