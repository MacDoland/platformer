using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWallSlideState : PlayerBaseState
{
    private bool _wallJumpRequested = false;
    private bool _wallJumpPerformed = false;
    private float _wallSlideclampY = -6f;
    private float _wallJumpIntent = 0f;
    private float _wallJumpVel = 0f;
    private Vector3 _jumpVelocity;
    private Vector3 _jumpDirection;
    private Vector3 _wallNormal;
    private Ray _wallRay;
    private RaycastHit _raycastHit;

    private Vector2 _moveInput;
    private Vector3 _moveInputFromCamera;

    public PlayerWallSlideState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = false;
        InitSubState();
    }

    public override void EnterState()
    {
        Debug.Log("Enter Wall Slide");
        _ctx.Animator.SetBool("wallSlide", true);
        _ctx.InputActions.Player.Jump.performed += WallJump;
    }
    public override void ExitState()
    {
        _wallJumpPerformed = false;
        _ctx.Animator.SetBool("wallSlide", false);
        _ctx.InputActions.Player.Jump.performed -= WallJump;
        Debug.Log("Exit Wall Slide");
    }
    public override void UpdateState()
    {
        CheckState();
    }

    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
        if (!_wallJumpPerformed)
        {
            _moveInput = _ctx.InputActions.Player.Move.ReadValue<Vector2>();
            _moveInputFromCamera = Vector3.ProjectOnPlane(_ctx.Camera.transform.TransformDirection(new Vector3(_moveInput.x, 0, _moveInput.y)), Vector3.up);
            _wallJumpIntent = Mathf.SmoothDamp(_wallJumpIntent, Vector3.Dot(_moveInputFromCamera, _raycastHit.normal), ref _wallJumpVel, 0.1f);

            velocity.y = Mathf.Clamp(velocity.y, _wallSlideclampY, Mathf.Infinity);
            velocity.x = 0f;
            velocity.z = 0f;
            _wallNormal = GetWallNormal();
        }

        if (_wallJumpRequested)
        {
            _jumpVelocity = _wallNormal;
            _jumpVelocity *= _ctx.WallJumpSpeed;
            _jumpVelocity.y = _ctx.JumpUpSpeed;
            _jumpDirection = _wallNormal;
            _jumpDirection.y = 0;

            _ctx.Motor.SetRotation(Quaternion.LookRotation(_jumpDirection.normalized, _ctx.Motor.Transform.up));
            velocity = _jumpVelocity;
            _wallJumpRequested = false;
            _wallJumpPerformed = true;

        }
    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime) { }
    public override void AfterUpdate(float deltaTime) { }
    public override void InitSubState() { }
    public override void CheckState()
    {
        if (_ctx.Motor.GroundingStatus.FoundAnyGround)
        {
            SwitchState(_stateFactory.AirMove());
        }
        else if (_wallJumpPerformed)
        {
            SwitchState(_stateFactory.AirMove());
        }

    }

    private Vector3 GetWallNormal()
    {
        _raycastHit = new RaycastHit();
        Physics.Raycast(_wallRay, out _raycastHit, 0.5f, _ctx.Layers);
        _wallRay = new Ray(_ctx.Motor.Transform.position + _ctx.Motor.Transform.up * 1.8f, _ctx.Motor.Transform.forward);
        return _raycastHit.normal;
    }

    private void WallJump(InputAction.CallbackContext context)
    {
        _wallJumpRequested = true;
    }

    public override void OnTriggerEnter(Collider other)
    {
        
    }

    public override void OnTriggerExit(Collider other)
    {
        
    }

    public override void OnTriggerStay(Collider other)
    {
       
    }
}