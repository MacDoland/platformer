using UnityEngine;
using Cinemachine;

public class CameraControls : MonoBehaviour
{
    public CinemachineVirtualCameraBase cameraBase;
    public Transform target;
    // Start is called before the first frame update
    void Start()
    {
        cameraBase.ForceCameraPosition(target.forward * -12f, Quaternion.LookRotation(target.forward, Vector3.up));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
