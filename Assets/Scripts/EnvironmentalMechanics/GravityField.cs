using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GravDirection
{
    Up, Down
}

public class GravityField : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The direction the field will pull the character.")]
    GravDirection GravityDirection = GravDirection.Down;

    public event Action EnteredField;
    public event Action ExitedField;

    public Vector3 GravityUpVector
    {
        get
        {
            return GravityDirection switch
            {
                GravDirection.Up => -Vector3.up,
                GravDirection.Down => Vector3.up,
                _ => throw new ArgumentOutOfRangeException("Not a valid direction")
            };
        }
    }

    bool IsCharacter(Collider maybeCharacter, out ReverseGravity character)
    {
        return maybeCharacter.TryGetComponent(out character);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsCharacter(other, out ReverseGravity character))
        {
            return;
        }

        EnteredField?.Invoke();

        character.AddField(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsCharacter(other, out ReverseGravity character))
        {
            return;
        }

        ExitedField?.Invoke();

        character.RemoveField(this);
    }
}
