using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CharacterCamera : MonoBehaviour
{
    #region Positioning Controls

    [SerializeField]
    [Tooltip("The transform the camera should be positioned relative to. " +
        "Traditionally the character or a fixed point.")]
    public Transform FocusOn;
    [SerializeField]
    [Tooltip("Shifts the focus relative to the focus transform. " +
        "The camera will still move relative to the original focus, but will " +
        "be looking at a different point.")]
    public Vector3 FocusOffset = Vector3.zero;

    [SerializeField]
    [Tooltip("If set, the camera's position will be confined to points within this volume. Useful for 2D " +
    "games where the camera might not want to follow the player to the edge of the playable area.")]
    public Collider LimitingVolume;

    #endregion

    [Space(20)]

    #region Zoom Controls

    [SerializeField]
    [Tooltip("Whether the user can zoom in and out.")]
    bool CanZoom = false;

    [SerializeField]
    [Tooltip("The current distance from the focus with the offset applied. If the camera is in orthographic mode, this will also apply to the size of the viewport.")]
    public float ZoomLevel = 20f;

    [SerializeField]
    [Tooltip("The shortest distance the camera can be from the focus.")]
    public float MinZoom = 10f;

    [SerializeField]
    [Tooltip("The longest distance the camera can be from the focus.")]
    public float MaxZoom = 30f;

    [SerializeField]
    [Tooltip("The camera's zooming sensitivity from the scroll wheel on a mouse.")]
    float ZoomSensitivity = 1;

    [SerializeField]
    [Tooltip("The keybinds to make the camera zoom in and out.")]
    InputAction Zoom;

    [SerializeField]
    [Tooltip("Whether or not the camera should push inwards to avoid intersecting with level geometry.\n\n" +
"Note: Clipping will only be avoided on objects with colliders attached.")]
    bool MitigateClipping = true;

    #endregion

    //[Space(20)]

    // Disabling editing these right now because we only officially support 2D, maybe will enable next semester
    #region Orbiting Controls

    //[SerializeField]
    //[Tooltip("Whether the camera can move in a fixed radius around " +
    //"the character, controlled by the mouse or right stick on a gamepad.")]
    bool CanOrbit = false;

    //[SerializeField]
    //[Tooltip("The camera's orbiting sensitivity from moving the mouse.")]
    float MouseRotationSensitivity = 1;

    //[SerializeField]
    //[Tooltip("The camera's orbiting sensitivity from moving the right stick on a gamepad.")]
    float GamepadRotationSensitivity = 1;
    
    //[SerializeField]
    //[Tooltip("TODO")]
    InputAction Orbit;

    #endregion

    Vector3 DirectionFromFocus = -Vector3.forward;
    Vector3 CurrentOrbitRotation = Vector3.zero;
    const float Y_LIMIT = 80;

    GameObject cameraHelper;


    // Start is called before the first frame update
    void OnEnable()
    {
        CurrentOrbitRotation = transform.rotation.eulerAngles;

        cameraHelper = new();
        cameraHelper.name = "CameraHelper";

        if (Orbit is not null)
        {
            Orbit.performed += DoOrbit;
            Orbit.Enable();
        }

        if (Zoom is not null)
        {
            Zoom.performed += DoZoom;
            Zoom.Enable();
        }
    }

    private void OnDisable()
    {
        if (Orbit is not null)
        {
            Orbit.performed -= DoOrbit;
        }

        if (Zoom is not null)
        {
            Zoom.performed -= DoZoom;
        }

        Destroy(cameraHelper);
    }

    void DoZoom(InputAction.CallbackContext context)
    {
        if (!CanZoom)
        {
            return;
        }

        ZoomLevel = Mathf.Clamp(
            ZoomLevel - Vector3.Normalize(context.ReadValue<Vector2>()).y * ZoomSensitivity,
            MinZoom, MaxZoom
        );
    }

    void DoOrbit(InputAction.CallbackContext context)
    {
        if (!CanOrbit)
        {
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;

        Camera cam = GetComponent<Camera>();
        Vector2 delta = new(-context.ReadValue<Vector2>().y, context.ReadValue<Vector2>().x);

        if (context.control.device.description.deviceClass == "Mouse")
        {
            delta = MouseRotationSensitivity * Vector2.Scale(delta, new Vector2(1f / cam.pixelHeight, 1f / cam.pixelWidth));
        }
        else
        {
            delta *= GamepadRotationSensitivity;
        }

        AddRotationDelta(360 * delta);
    }


    void AddRotationDelta(Vector3 delta)
    {
        // limit rotation around the x axis between Y_LIMIT and -Y_LIMIT
        CurrentOrbitRotation = new Vector3(
            Mathf.Clamp(
                CurrentOrbitRotation.x + delta.x, -Y_LIMIT, Y_LIMIT
            ),
            CurrentOrbitRotation.y + delta.y,
            CurrentOrbitRotation.z + delta.z
        );
    }

    // avoids clipping by placing the camera infront of the wall it would clip into
    void SnapForwardToAvoidClipping(Transform t)
    {
        bool didHit = Physics.Raycast(
            Focus(), t.position - Focus(),
            out RaycastHit hit, ZoomLevel, ControlConstants.RAYCAST_MASK
        );

        if (didHit)
        {
            t.position = hit.point - (t.position - Focus()).normalized * GetComponent<Camera>().nearClipPlane;
        }

    }

    public Vector3 Focus()
    {
        return FocusOn.position + FocusOffset;
    }

    public Transform GetNextCameraTransform()
    {
        cameraHelper.transform.position = Focus() + Quaternion.Euler(CurrentOrbitRotation) * DirectionFromFocus * ZoomLevel;
            
        if (MitigateClipping)
        {
            SnapForwardToAvoidClipping(cameraHelper.transform);
        }

        cameraHelper.transform.LookAt(Focus(), Vector3.up);

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

        transform.SetPositionAndRotation(nextPosition, nextTransform.rotation);
        GetComponent<Camera>().orthographicSize = ZoomLevel / 2; // div 2 since orthoSize is the camera's half-size
    }
}
