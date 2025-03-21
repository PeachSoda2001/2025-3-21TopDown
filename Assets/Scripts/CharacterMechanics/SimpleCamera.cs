using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class SimpleCamera : MonoBehaviour
{
    #region Positioning Controls

    [SerializeField]
    [Tooltip(
        "The transform the camera should be positioned relative to. " +
        "Traditionally the character or a fixed point."
    )]
    public Transform Follow;

    [SerializeField]
    [Tooltip(
        "If set, the camera's position will be confined to points within this volume. Useful for 2D " +
        "games where the camera might not want to follow the player to the edge of the playable area."
    )]
    public Collider LimitingVolume;

    [SerializeField]
    [Tooltip(
        "Whether or not the camera should push inwards to avoid intersecting with level geometry.\n\n" +
        "Note: Clipping will only be avoided on objects with colliders attached."
    )]
    bool MitigateClipping = true;

    #endregion

    Vector3 FollowToFocus;
    Vector3 FocusToCamera;

    GameObject cameraHelper;


    // Start is called before the first frame update
    void OnEnable()
    {
        Plane focusPlane = new(transform.forward, Follow.position);
        Vector3 focus = focusPlane.ClosestPointOnPlane(transform.position);

        FollowToFocus = focus - Follow.position;
        FocusToCamera = transform.position - focus;

        cameraHelper = new()
        {
            name = "CameraHelper"
        };
    }

    private void OnDisable()
    {
        Destroy(cameraHelper);
    }

    // avoids clipping by placing the camera infront of the wall it would clip into
    void SnapForwardToAvoidClipping(Transform t)
    {
        bool didHit = Physics.Raycast(
            Focus(), t.position - Focus(),
            out RaycastHit hit, FocusToCamera.magnitude, ControlConstants.RAYCAST_MASK
        );

        if (didHit)
        {
            t.position = hit.point - (t.position - Focus()).normalized * GetComponent<Camera>().nearClipPlane;
        }
    }

    public Vector3 Focus()
    {
        return Follow.position + FollowToFocus;
    }

    public Transform GetNextCameraTransform()
    {
        cameraHelper.transform.position = Focus() + FocusToCamera;
        
        if (MitigateClipping)
        {
            SnapForwardToAvoidClipping(cameraHelper.transform);
        }

        return cameraHelper.transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Transform nextTransform = GetNextCameraTransform();

        Vector3 nextPosition = nextTransform.position;

        if (LimitingVolume != null)
        {
            nextPosition = LimitingVolume.ClosestPoint(nextPosition);
        }

        transform.position = nextPosition;
    }
}
