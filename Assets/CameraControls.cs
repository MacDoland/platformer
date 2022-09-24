using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CameraControls : MonoBehaviour
{
    public CinemachineVirtualCameraBase cameraBase;
    public Transform target;
    private PlatformerInputActions cameraInputActions;

    void Awake()
    {
        cameraInputActions = new PlatformerInputActions();
    }

    // Start is called before the first frame update
    void Start()
    {
        var currentDistance = Vector3.Distance(this.transform.position, target.position);
        cameraBase.ForceCameraPosition(target.position + target.forward * -currentDistance, Quaternion.LookRotation(target.forward, Vector3.up));
    }

    void ResetPosition (InputAction.CallbackContext context) {
        var currentDistance = Vector3.Distance(this.transform.position, target.position);
        cameraBase.ForceCameraPosition(target.position + target.forward * -currentDistance, Quaternion.LookRotation(target.forward, Vector3.up));
    }

    void OnEnable()
    {
        cameraInputActions.Camera.Reset.performed += ResetPosition;
        cameraInputActions.Camera.Reset.Enable();
    }

    void OnDisable()
    {
        cameraInputActions.Camera.Reset.Disable();
    }
}
