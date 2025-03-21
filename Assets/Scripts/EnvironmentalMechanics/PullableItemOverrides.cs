using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullableItemOverrides : MonoBehaviour
{
    public bool OverridePullTime = false;
    public float PullTimeOverride = 1f;

    [Space(10)]

    public bool OverridePullingSpeed = false;
    public float PullingSpeedOverride = 10;

    [Space(10)]

    public bool OverrideFollowGrappler = false;
    public bool FollowGrappler = false;
}
