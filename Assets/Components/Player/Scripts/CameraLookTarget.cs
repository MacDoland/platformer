using UnityEngine;

public class CameraLookTarget : MonoBehaviour
{
    [SerializeField] private PlayerStateMachine _cameraTarget;
    [SerializeField] private float _dampTime = 0.1f;
    [SerializeField] private float _maxSpeed = Mathf.Infinity;
    private Vector3 _velocity;

    void Awake()
    {
        this.transform.rotation = _cameraTarget.transform.rotation;
    }
    void Update()
    {
        this.transform.position = Vector3.SmoothDamp(this.transform.position, _cameraTarget.GetCameraTargetPosition(), ref _velocity, _dampTime, _maxSpeed);
        this.transform.rotation = _cameraTarget.transform.rotation;
    }
}