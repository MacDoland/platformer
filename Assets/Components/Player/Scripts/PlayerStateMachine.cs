using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UnityEngine.InputSystem;
using Cinemachine;

public partial class PlayerStateMachine : MonoBehaviour, ICharacterController, ICameraTarget
{
    //State
    private PlayerStateFactory _states;
    [field: SerializeField, Header("State"), ReadOnly] private string CurrentStateName;
    [field: SerializeField] public PlayerBaseState CurrentState { get; set; }

    //Animator
    [field: SerializeField, Header("Animation")] public Animator Animator { get; private set; }

    //Input
    public PlatformerInputActions InputActions { get; private set; }
    private InputAction _playerMovement;
    public Vector3 MoveInputVector { get; private set; }
    public Vector3 LookInputVector { get; private set; }

    //Motor - handles velocity, collisions, ground detection etc
    [field: SerializeField, Header("Motor")] public KinematicCharacterMotor Motor { get; private set; }

    //Camera
    [field: SerializeField, Header("Camera")] public Camera Camera { get; private set; }
    [field: SerializeField, Tooltip("Placement of the virtual look target for the player camera.")]
    public Vector3 CameraLookTargetOffset { get; private set; } = new Vector3(0, 2f, 0);
    [field: SerializeField] public CinemachineVirtualCameraBase GeneralCamera { get; set; }
    [field: SerializeField] public CinemachineVirtualCameraBase WaterCamera { get; set; }

    public Vector3 PlayerCameraViewPosition { get; private set; }
    public Vector3 CameraLookTarget { get; set; }

    //Collision Layers
    [field: SerializeField, Header("Collision Layers")] public LayerMask Layers { get; private set; }
    [field: SerializeField] public LayerMask PlayerLayer { get; private set; }

    //Ground Movement
    [field: SerializeField, Header("Ground Movement"), Range(0f, 20f)] public float MaxStableMoveSpeed { get; private set; } = 6f;
    [field: SerializeField, Range(0f, 30f)] public float StableMovementSharpness { get; set; } = 15f;
    [field: SerializeField, Range(0f, 30f)] public float OrientationSharpness { get; private set; } = 10f;

    //Gravity
    [field: SerializeField, Header("Gravity"), Range(-60f, 0)] public float Gravity { get; private set; } = -40f;

    //Air Movement   
    [field: SerializeField, Header("Air Movement"), Range(0f, 20f)] public float MaxAirMoveSpeed { get; private set; } = 6f;
    [field: SerializeField, Range(0f, 50f)] public float AirAccelerationSpeed { get; private set; } = 30f;
    [field: SerializeField, Range(0f, 10f)] public float Drag { get; private set; } = 1f;

    //Jumping
    public bool JumpRequested { get; set; } = false;
    public bool JumpConsumed { get; set; } = false;
    public bool JumpedThisFrame { get; set; } = false;
    public float TimeSinceLastAbleToJump { get; set; } = Mathf.Infinity;
    public float TimeSinceJumpRequested { get; set; } = 0f;

    [field: SerializeField, Header("Jumping")] public bool AllowJumpingWhenSliding { get; private set; }
    [field: SerializeField, Range(0f, 40f)] public float JumpUpSpeed { get; private set; } = 16f;
    [field: SerializeField, Range(0f, 40f)] public float JumpScalableForwardSpeed { get; private set; } = 0f;
    [field: SerializeField, Range(0f, 40f)] public float JumpPreGroundingGraceTime { get; private set; } = 0.1f;
    [field: SerializeField, Range(0f, 40f)] public float JumpPostGroundingGraceTime { get; private set; } = 0.1f;
    [field: SerializeField, Range(0f, 40f)] public float WallJumpSpeed { get; private set; } = 4f;

    //Ledge Grabbing
    [field: SerializeField, Header("Ledge Grab"), ReadOnly] public float Height { get; private set; } = 1.6f;
    [field: SerializeField, Range(0f, 1f)] public float KeepDistanceFromWall { get; private set; } = 0.05f;
    [field: SerializeField, Range(0f, 2f)] public float ReachHeight { get; private set; } = 1f;
    [field: SerializeField, Range(0f, 1f)] public float ReachDistance { get; private set; } = 0.5f;
    [field: SerializeField, Range(-1, 1f)] public float LedgeGrabOffsetY { get; private set; } = -0.5f;

    private RaycastHit _ledgeGrabWallInfo;
    private RaycastHit _ledgeGrabSpaceInfo;
    private RaycastHit _ledgeGrabLedgeInfo;
    public RaycastHit LedgeGrabWallInfo { get { return _ledgeGrabWallInfo; } set { _ledgeGrabWallInfo = value; } }
    public RaycastHit LedgeGrabSpaceInfo { get { return _ledgeGrabSpaceInfo; } set { _ledgeGrabSpaceInfo = value; } }
    public RaycastHit LedgeGrabLedgeInfo { get { return _ledgeGrabLedgeInfo; } set { _ledgeGrabLedgeInfo = value; } }

    //Swim
    [Header("Swim")]
    [field: SerializeField] public LayerMask WaterLayer;
    [field: SerializeField] public float WaterLevel;
    [field: SerializeField] public bool IsInWater;
    [field: SerializeField] public float SubmergedAmount { get; set; } = 0.5f;
    [field: SerializeField] public float FloatingHeight { get; set; } = 1f;
    [field: SerializeField] public float SwimSpeed { get; set; } = 4f;
    [field: SerializeField] public float DiveSwimSpeed { get; set; } = 6f;
    [field: SerializeField] public float MaxSwimSpeed { get; set; } = 8f;
    [field: SerializeField] public float MaxDiveSwimSpeed { get; set; } = 8f;
    [field: SerializeField, Range(0f, 10f)] public float WaterDrag { get; set; } = 1f;
    [field: SerializeField, Range(0f, 10f)] public float UnderWaterDrag { get; set; } = 1f;
    [field: SerializeField, Min(0)] public float Buoyancy { get; set; } = 1f;

    private Vector3 _internalVelocityAdd = Vector3.zero;

    /// Awake is called when the script instance is being loaded.
    void Awake()
    {
        _states = new PlayerStateFactory(this);
        this.CurrentState = _states.Grounded();
        this.CurrentState.EnterState();
        this.InputActions = new PlatformerInputActions();
    }

    // Start is called before the first frame update
    void Start()
    {
        Motor.CharacterController = this;
    }

    void OnEnable()
    {
        _playerMovement = this.InputActions.Player.Move;
        _playerMovement.Enable();

        this.InputActions.Player.Jump.performed += PerformJump;
        this.InputActions.Player.Jump.Enable();
        this.InputActions.Player.ButtonWest.Enable();
        this.InputActions.Player.Sprint.Enable();
    }

    void OnDisable()
    {
        _playerMovement.Disable();
        this.InputActions.Player.Jump.Disable();
        this.InputActions.Player.Sprint.Disable();
        this.InputActions.Player.ButtonWest.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate camera direction and rotation on the character plane
        #region Get Movement Vector
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(this.Camera.transform.rotation * Vector3.forward, Motor.CharacterUp).normalized;

        if (cameraPlanarDirection.sqrMagnitude == 0f)
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(this.Camera.transform.rotation * Vector3.up, Motor.CharacterUp).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

        Vector2 inputMovement = _playerMovement.ReadValue<Vector2>();
        #endregion
        this.MoveInputVector = cameraPlanarRotation * new Vector3(inputMovement.x, 0, inputMovement.y);
        this.LookInputVector = this.MoveInputVector.normalized;

        // this.CurrentState.UpdateStates();
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        // Handle jumping pre-ground grace period
        if (this.JumpRequested && this.TimeSinceJumpRequested > JumpPreGroundingGraceTime)
        {
            this.JumpRequested = false;
        }

        if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
        {
            // If we're on a ground surface, reset jumping values
            if (!this.JumpedThisFrame)
            {
                this.JumpConsumed = false;
            }
            this.TimeSinceLastAbleToJump = 0f;
        }
        else
        {
            // Keep track of time since we were last able to jump (for grace period)
            this.TimeSinceLastAbleToJump += deltaTime;
        }
this.CurrentState.UpdateStates();
        //this.CurrentState.AfterUpdateStates(deltaTime);
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
        this.CurrentState.UpdateStateRotations(ref currentRotation, Time.deltaTime);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        this.Animator.SetBool("isGrounded", Motor.GroundingStatus.IsStableOnGround);
        this.Animator.SetFloat("speedY", currentVelocity.y);

        this.JumpedThisFrame = false;
        this.TimeSinceJumpRequested += deltaTime;

        this.CurrentState.UpdateStateVelocitys(ref currentVelocity, Time.deltaTime);
        this.CurrentStateName = this.CurrentState.GetFullStateName();

        // Take into account additive velocity
        if (_internalVelocityAdd.sqrMagnitude > 0f)
        {
            currentVelocity += _internalVelocityAdd;
            _internalVelocityAdd = Vector3.zero;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (LayerIsInMask(WaterLayer, other.gameObject.layer))
        {
            this.WaterLevel = other.bounds.center.y;
        }


        this.CurrentState.OnTriggerEnter(other);
    }

    public void OnTriggerStay(Collider other)
    {

        if (LayerIsInMask(WaterLayer, other.gameObject.layer))
        {
            this.SubmergedAmount = Mathf.Clamp((this.WaterLevel - Motor.Transform.position.y) / this.Height, 0f, 1f);
            this.IsInWater = this.SubmergedAmount > this.FloatingHeight;
        }

        this.CurrentState.OnTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        this.CurrentState.OnTriggerExit(other);
    }

    private void PerformJump(InputAction.CallbackContext obj)
    {
        this.TimeSinceJumpRequested = 0f;
        this.JumpRequested = true;
    }

    public Vector3 GetCameraTargetPosition()
    {
        return this.CameraLookTarget;
    }

    private bool LayerIsInMask(LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }


    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    void OnDrawGizmos()
    {
        if (Application.IsPlaying(this.gameObject))
        {
            Gizmos.DrawSphere(Motor.transform.position + Vector3.up * 1.6f, 0.2f);
            Gizmos.DrawSphere(Motor.transform.position + -Vector3.up * 0.5f, 0.2f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(Motor.Transform.position.x, WaterLevel, Motor.Transform.position.z) - (Vector3.up * Height) + (Vector3.up * Height * FloatingHeight), 0.125f);
        }
        //- Motor.Transform.position.YOnly() + Vector3.up * (Height * FloatingHeight)    
    }
}
