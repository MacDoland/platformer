using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UnityEngine.InputSystem;

public partial class PlayerStateMachine : MonoBehaviour, ICharacterController
{
    [Header("Motor")]
    [SerializeField]
    private KinematicCharacterMotor _motor;
    public KinematicCharacterMotor Motor { get { return _motor; } }

    [Header("Camera")]
    [SerializeField]
    private Transform _cameraTransform;

    [SerializeField]
    private Animator _animator;
    public Animator Animator { get { return _animator; } }

    //input
    private PlatformerInputActions _platformerInputActions;
    private InputAction _playerMovement;
    private InputAction _jumpInput;
    public InputAction JumpInput { get { return _jumpInput; } }

    private Vector3 _moveInputVector;
    private Vector3 _lookInputVector;

    public Vector3 MoveInputVector { get { return _moveInputVector; } }


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
    }

    void OnDisable()
    {
        _playerMovement.Disable();
        _platformerInputActions.Player.Jump.Disable();
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
        if (_lookInputVector.sqrMagnitude > 0f && _orientationSharpness > 0f)
        {
            // Smoothly interpolate from current to target look direction
            Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-_orientationSharpness * deltaTime)).normalized;

            // Set the current rotation (which will be used by the KinematicCharacterMotor)
            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        _animator.SetBool("isGrounded", Motor.GroundingStatus.IsStableOnGround);
        _animator.SetFloat("speedY", currentVelocity.y);

        _jumpedThisFrame = false;
        _timeSinceJumpRequested += deltaTime;

        _currentState.UpdateStates(ref currentVelocity, Time.deltaTime);
        currentStateName = _currentState.Name;

        // Take into account additive velocity
        if (_internalVelocityAdd.sqrMagnitude > 0f)
        {
            currentVelocity += _internalVelocityAdd;
            _internalVelocityAdd = Vector3.zero;
        }
    }

    private void PerformJump(InputAction.CallbackContext obj)
    {
        Debug.Log("JUMPED");
        _timeSinceJumpRequested = 0f;
        _jumpRequested = true;
    }
}
