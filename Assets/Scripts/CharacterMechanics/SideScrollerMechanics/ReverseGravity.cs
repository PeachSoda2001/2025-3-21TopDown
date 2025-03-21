using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterMovement))]
public class ReverseGravity : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The input to trigger gravity flipping.")]
    InputAction Flip;

    [SerializeField]
    [Tooltip("Whether the player can switch gravity in midair.")]
    bool SwitchableMidair = true;

    [SerializeField]
    [Range(0,1)]
    [Tooltip("What fraction of the vertical velocity is retained after flipping. A higher value will feel floatier.")]
    float VelocityRetention = 0.1f;

    /** **/
    public event Action FlipRequested;
    public event Action<int> Flipped;
    /** **/

    CharacterMovement movement;
    List<GravityField> InFields = new();

    void Awake()
    {
        movement = GetComponent<CharacterMovement>();
    }

    void OnEnable()
    {
        if (Flip is not null)
        {
            Flip.performed += DoSwitchGravity;
            Flip.Enable();
        }
    }

    private void OnDisable()
    {
        if (Flip is not null)
        {
            Flip.performed -= DoSwitchGravity;
            Flip.Disable();
        }
    }

    void DoSwitchGravity(InputAction.CallbackContext _)
    {
        FlipRequested?.Invoke();

        if ((SwitchableMidair || GetComponent<CharacterMovement>().IsOnGround()) && InFields.Count == 0)
        {
            SetGravityUp(-movement.GravityUpDirection);
        }
    }

    public void SetGravityUp(Vector3 gravityUp)
    {
        if (movement.GravityUpDirection == gravityUp)
        {
            return;
        }

        movement.GravityUpDirection = gravityUp;
        movement.VerticalSpeed *= -VelocityRetention;
        Flipped?.Invoke((int)movement.GravityUpDirection.y);
    }

    public void AddField(GravityField field)
    {
        if (InFields.Contains(field))
        {
            return;
        }

        InFields.Insert(0, field);
        SetGravityUp(field.GravityUpVector);
    }

    public void RemoveField(GravityField field)
    {
        if (!InFields.Contains(field))
        {
            return;
        }

        InFields.Remove(field);

        if (InFields.Count == 0)
        {
            return;
        }

        SetGravityUp(InFields[0].GravityUpVector);
    }

}
