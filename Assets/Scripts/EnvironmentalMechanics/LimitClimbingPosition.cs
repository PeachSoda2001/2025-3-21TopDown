using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitClimbingPosition : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The volume to limit the player's climbing to.")]
    public Collider LimitingVolume;

    [SerializeField]
    [Tooltip("If true, the player can move out of the limiter and fall off the wallclimb. If not, they will be stuck until they jump out or drop off.")]
    public bool CanClimbOutOfLimiter;
}
