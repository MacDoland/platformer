using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UnityEngine.InputSystem;

public partial class PlayerStateMachine : MonoBehaviour, ICharacterController, ICameraTarget
{
    [SerializeField] private Vector3 _currentVelocity;

    [Header("Motor")]
    [SerializeField]
    private KinematicCharacterMotor _motor;
    public KinematicCharacterMotor Motor { get { return _motor; } }

    [Header("Camera")]
    [SerializeField]
    private Camera _camera;
    [SerializeField] private Vector3 _cameraLookTargetOffset = new Vector3(0, 2f, 0);
    public Camera Camera { get { return _camera; } }
    public Vector3 CameraLookTarget { get; set; }
    public Vector3 CameraLookTargetOffset { get { return _cameraLookTargetOffset; } }
    public Vector3 PlayerCameraViewPosition { get; private set; }


    [Header("Collision Layers")]
    [SerializeField]
    private LayerMask _layers;
    public LayerMask Layers { get { return _layers; } }

    [SerializeField]
    private LayerMask _playerLayer;
    public LayerMask PlayerLayer { get { return _playerLayer; } }

    private Ray _groundHeightRay;
    private RaycastHit _groundHeightRayHitInfo;
    public float DistanceFromGround { get; private set; }
    public Vector3 GroundPointUnderneath { get; private set; }

    [Header("Stable Movement")]
    [SerializeField]
    private float _maxStableMoveSpeed = 10f;
    [SerializeField]
    private float _stableMovementSharpness = 15f;
    [SerializeField]
    private float _orientationSharpness = 10f;

    public float MaxStableMoveSpeed { get { return _maxStableMoveSpeed; } }
    public float StableMovementSharpness { get { return _stableMovementSharpness; } }
    public float OrientationSharpness { get { return _orientationSharpness; } }

    [Header("Air Movement")]
    [SerializeField] private float _maxAirMoveSpeed = 15f;
    [SerializeField] private float _airAccelerationSpeed = 15f;
    [SerializeField] private float _drag = 0.1f;

    public float MaxAirMoveSpeed { get { return _maxAirMoveSpeed; } }
    public float AirAccelerationSpeed { get { return _airAccelerationSpeed; } }
    public float Drag { get { return _drag; } }

    [Header("Jumping")]
    [SerializeField] private bool _allowJumpingWhenSliding = false;
    [SerializeField] private float _jumpUpSpeed = 10f;
    [SerializeField] private float _jumpScalableForwardSpeed = 10f;
    [SerializeField] private float _jumpPreGroundingGraceTime = 0f;
    [SerializeField] private float _jumpPostGroundingGraceTime = 0f;

    private bool _jumpRequested = false;
    private bool _jumpConsumed = false;
    private bool _jumpedThisFrame = false;
    private float _timeSinceJumpRequested = Mathf.Infinity;
    private float _timeSinceLastAbleToJump = 0f;

    public bool JumpRequested { get { return _jumpRequested; } set { _jumpRequested = value; } }
    public bool JumpConsumed { get { return _jumpConsumed; } set { _jumpConsumed = value; } }
    public bool JumpedThisFrame { get { return _jumpedThisFrame; } set { _jumpedThisFrame = value; } }
    public float TimeSinceLastAbleToJump { get { return _timeSinceLastAbleToJump; } }
    public bool AllowJumpingWhenSliding { get { return _allowJumpingWhenSliding; } }
    public float JumpUpSpeed { get { return _jumpUpSpeed; } }
    [field: SerializeField] public float WallJumpSpeed { get; private set; }
    public float JumpScalableForwardSpeed { get { return _jumpScalableForwardSpeed; } }
    public float JumpPreGroundingGraceTime { get { return _jumpPreGroundingGraceTime; } }
    public float JumpPostGroundingGraceTime { get { return _jumpPostGroundingGraceTime; } }

    [Header("Misc")]
    [SerializeField]
    private Animator _animator;
    public Animator Animator { get { return _animator; } }

    //input
    private PlatformerInputActions _platformerInputActions;
    private InputAction _playerMovement;
    private InputAction _jumpInput;
    private bool _sprintButtonHeld = false;
    public InputAction JumpInput { get { return _jumpInput; } }
    public bool SprintButtonHeld { get { return _sprintButtonHeld; } }

    private Vector3 _moveInputVector;
    private Vector3 _lookInputVector;

    public Vector3 MoveInputVector { get { return _moveInputVector; } }
    public Vector3 LookInputVector { get { return _lookInputVector; } }
    public PlatformerInputActions InputActions { get { return _platformerInputActions; } }

    //state
    private PlayerBaseState _currentState;
    private PlayerStateFactory _states;
    [SerializeField]
    private string currentStateName;

    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }

    private Vector3 _internalVelocityAdd = Vector3.zero;

    [Header("Gravity")]
    [SerializeField] private Vector3 _gravity = new Vector3(0, -40f, 0);

    [Header("Ledge Grab")]
    [SerializeField] private float _height = 1.6f;
    [SerializeField] private float _keepDistanceFromWall = 0.15f;
    [SerializeField] private float _ledgeGrabOffsetY = -0.5f;
    private float _reachHeight = 1f;
    private float _reachDistance = 0.5f;

    private RaycastHit _ledgeGrabWallInfo;
    private RaycastHit _ledgeGrabSpaceInfo;
    private RaycastHit _ledgeGrabLedgeInfo;

    public Vector3 Gravity { get { return _gravity; } }
    public float Height { get { return _height; } }
    public float KeepDistanceFromWall { get { return _keepDistanceFromWall; } }
    public float ReachHeight { get { return _reachHeight; } }
    public float ReachDistance { get { return _reachDistance; } }
    public float LedgeGrabOffsetY { get { return _ledgeGrabOffsetY; } }

    public RaycastHit LedgeGrabWallInfo { get { return _ledgeGrabWallInfo; } set { _ledgeGrabWallInfo = value; } }
    public RaycastHit LedgeGrabSpaceInfo { get { return _ledgeGrabSpaceInfo; } set { _ledgeGrabSpaceInfo = value; } }
    public RaycastHit LedgeGrabLedgeInfo { get { return _ledgeGrabLedgeInfo; } set { _ledgeGrabLedgeInfo = value; } }

    
    [Header("Swim")]
    [field: SerializeField] public LayerMask WaterLayer;
    [field: SerializeField] public bool IsInWater;

    //[Header("Wall Slide")]
    public bool IsWallSliding { get; set; }


    /// Awake is called when the script instance is being loaded.
    void Awake()
    {
        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();
        _platformerInputActions = new PlatformerInputActions();
    }

    // Start is called before the first frame update
    void Start()
    {
        Motor.CharacterController = this;
    }

    void OnEnable()
    {
        _playerMovement = _platformerInputActions.Player.Move;
        _playerMovement.Enable();

        _platformerInputActions.Player.Jump.performed += PerformJump;
        _platformerInputActions.Player.Jump.Enable();
        _platformerInputActions.Player.Sprint.Enable();

    }

    void OnDisable()
    {
        _playerMovement.Disable();
        _platformerInputActions.Player.Jump.Disable();
        _platformerInputActions.Player.Sprint.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate camera direction and rotation on the character plane
        #region Get Movement Vector
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(_camera.transform.rotation * Vector3.forward, Motor.CharacterUp).normalized;

        if (cameraPlanarDirection.sqrMagnitude == 0f)
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(_camera.transform.rotation * Vector3.up, Motor.CharacterUp).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

        Vector2 inputMovement = _playerMovement.ReadValue<Vector2>();
        #endregion
        _moveInputVector = cameraPlanarRotation * new Vector3(inputMovement.x, 0, inputMovement.y);
        _lookInputVector = _moveInputVector.normalized;
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        // Handle jumping pre-ground grace period
        if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
        {
            _jumpRequested = false;
        }

        if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
        {
            // If we're on a ground surface, reset jumping values
            if (!_jumpedThisFrame)
            {
                _jumpConsumed = false;
            }
            _timeSinceLastAbleToJump = 0f;
        }
        else
        {
            // Keep track of time since we were last able to jump (for grace period)
            _timeSinceLastAbleToJump += deltaTime;
        }

        _currentState.UpdateStates();
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        _currentState.UpdateStateRotations(ref currentRotation, Time.deltaTime);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        _animator.SetBool("isGrounded", Motor.GroundingStatus.IsStableOnGround);
        _animator.SetFloat("speedY", currentVelocity.y);

        _jumpedThisFrame = false;
        _timeSinceJumpRequested += deltaTime;
        _sprintButtonHeld = _platformerInputActions.Player.Sprint.IsPressed();


        _currentState.UpdateStateVelocitys(ref currentVelocity, Time.deltaTime);
        currentStateName = _currentState.GetFullStateName();

        // Take into account additive velocity
        if (_internalVelocityAdd.sqrMagnitude > 0f)
        {
            currentVelocity += _internalVelocityAdd;
            _internalVelocityAdd = Vector3.zero;
        }

        _groundHeightRay = new Ray(this.transform.position + (Vector3.up * 2f), -Vector3.up);

        if (Physics.Raycast(_groundHeightRay, out _groundHeightRayHitInfo, Mathf.Infinity, _layers))
        {
            this.DistanceFromGround = Vector3.Distance(this.transform.position, _groundHeightRayHitInfo.point);
            this.GroundPointUnderneath = _groundHeightRayHitInfo.point;
        }

        _currentVelocity = currentVelocity;
    }

    public void OnTriggerEnter(Collider other)
    {
        IsInWater = LayerIsInMask(WaterLayer, other.gameObject.layer);

        _currentState.OnTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        if(LayerIsInMask(WaterLayer, other.gameObject.layer)) {
            IsInWater = false;
        }

        _currentState.OnTriggerExit(other);
    }

    private void PerformJump(InputAction.CallbackContext obj)
    {
        _timeSinceJumpRequested = 0f;
        _jumpRequested = true;
    }

    public Vector3 GetCameraTargetPosition()
    {
        return this.CameraLookTarget;
    }

    private bool LayerIsInMask(LayerMask mask, int layer) {
        return mask == (mask | (1 << layer));
    }


    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(Motor.transform.position + Vector3.up * 1.6f, 0.2f);
        Gizmos.DrawSphere(Motor.transform.position + -Vector3.up * 0.5f, 0.2f);
    }
}
