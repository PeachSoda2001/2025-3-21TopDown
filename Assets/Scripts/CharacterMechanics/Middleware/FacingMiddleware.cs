using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public static class FacingMiddleware
{

    public static Func<Vector3, float, Vector3> UpdateOnlyWhenMoving(CharacterMovement movement)
    {
        return (v, dt) =>
        {
            if (movement.MovementDirection.magnitude == 0)
            {
                return movement.FacingDirection;
            }

            return Quaternion.LookRotation(-movement.ForwardMovementDirectionFromCamera(), Vector3.up) * movement.RawFacingDirection;
        };
    }

    public static Func<Vector3, float, Vector3> FaceMovementDirection(CharacterMovement movement)
    {
        return (v, dt) =>
        {
            return Quaternion.LookRotation(-movement.ForwardMovementDirectionFromCamera(), Vector3.up) * movement.MovementDirection.normalized;
        };
    }

    public static Func<Vector3, float, Vector3> LookAtMouse(CharacterMovement movement)
    {
        return (v, dt) =>
        {
            Ray mouseRay = movement.Camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            // get the distance the ray travels to intersect the Y plane of the character
            Plane characterYPlane = new(Vector3.up, movement.transform.position);
            characterYPlane.Raycast(mouseRay, out float distanceAlongRay);

            // make a direction out of that by zeroing it to the character
            return (mouseRay.GetPoint(distanceAlongRay) - movement.transform.position).normalized;
        };
    }
}