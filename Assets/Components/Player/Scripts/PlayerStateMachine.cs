using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UnityEngine.InputSystem;

public partial class PlayerStateMachine : MonoBehaviour, ICharacterController, ICameraTarget
{
    [Header("Motor")]
    [SerializeField]
    private KinematicCharacterMotor _motor;
    public KinematicCharacterMotor Motor { get { return _motor; } }

    [Header("Camera")]
    [SerializeField]
    private Transform _cameraTransform;
    [SerializeField] private Vector3 _cameraLookTargetOffset = new Vector3(0, 2f, 0);
    public Vector3 CameraLookTarget { get; set; }
    public Vector3 CameraLookTargetOffset { get { return _cameraLookTargetOffset; } }

    [Header("Collision Layers")]
    [SerializeField]
    private LayerMask _layers;
    public LayerMask Layers { get { return _layers; } }

    [SerializeField]
    private LayerMask _playerLayer;
    public LayerMask PlayerLayer { get { return _playerLayer; } }


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

    //state
    private PlayerBaseState _currentState;
    private PlayerStateFactory _states;
    [SerializeField]
    private string currentStateName;

    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }

    private Vector3 _internalVelocityAdd = Vector3.zero;

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
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(_cameraTransform.rotation * Vector3.forward, Motor.CharacterUp).normalized;

        if (cameraPlanarDirection.sqrMagnitude == 0f)
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(_cameraTransform.rotation * Vector3.up, Motor.CharacterUp).normalized;
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
}
