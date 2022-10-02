using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerDiveSwimState : PlayerBaseState
{

    private Vector2 _input;
    private bool _isSwimming = false;
    private Vector3 _targetVelocity;

    private float _yRotation = 0f;
    private float _xRotation = 0f;

    private float _rotateSpeed = 50f;
    private float _distanceToSurface = 0f;
    private float _surfaceSnapDistance = 1f;

    private bool _switchState = false;

    public PlayerDiveSwimState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = false;
        InitSubState();
    }

    public override void EnterState()
    {
        _ctx.Motor.SetGroundSolvingActivation(false);
        _ctx.WaterCamera.Priority = 100;
        Debug.Log("Enter Dive Swim State");
    }
    public override void ExitState()
    {

        _ctx.Motor.SetGroundSolvingActivation(true);
        Debug.Log("Exit Dive Swim State");
        _switchState = false;

        var forwardVector = Vector3.ProjectOnPlane(_ctx.Motor.Transform.forward * -1f, Vector3.up);
        _ctx.GeneralCamera.ForceCameraPosition(_ctx.Motor.Transform.position + (Vector3.up * 8f) + forwardVector * 12f, Quaternion.LookRotation(_ctx.Motor.Transform.forward, Vector3.up));
        _ctx.WaterCamera.Priority = 0;

    }
    public override void UpdateState()
    {
        CheckState();
    }

    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {

        _ctx.Animator.SetFloat("speed", _ctx.Motor.Velocity.magnitude);

        if (_ctx.InputActions.Player.ButtonWest.IsPressed() && velocity.magnitude < _ctx.MaxDiveSwimSpeed)
        {
            velocity += _ctx.Motor.Transform.forward * _ctx.DiveSwimSpeed * deltaTime;
        }


        velocity *= 1f - _ctx.UnderWaterDrag * Time.deltaTime;

        _ctx.CameraLookTarget = _ctx.Motor.Transform.position + _ctx.CameraLookTargetOffset;


        _distanceToSurface = _ctx.WaterLevel - _ctx.Motor.Transform.position.y;

        if (_distanceToSurface < _surfaceSnapDistance)
        {
            _switchState = true;
            _ctx.GeneralCamera.ForceCameraPosition(_ctx.Motor.Transform.position + (Vector3.up * 4f) + _ctx.Motor.Transform.forward * -12f, Quaternion.LookRotation(_ctx.Motor.Transform.forward, Vector3.up));

        }

        if (_switchState)
        {
            SwitchState(_stateFactory.Swim());
        }
    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
        //rotation = _ctx.Camera.transform.rotation;

        _input = _ctx.InputActions.Player.Move.ReadValue<Vector2>();

        //rotation *= Quaternion.Euler((Vector3.up * _input.x) * _rotateSpeed);

        _yRotation = _ctx.Motor.Transform.eulerAngles.y + _input.x * _rotateSpeed * deltaTime;
        _xRotation = _ctx.Motor.Transform.eulerAngles.x + _input.y * _rotateSpeed * deltaTime;
        //_xRotation = Mathf.Clamp(_xRotation, -89.5f, 89.5f);

        if (_xRotation > 90f && _xRotation < 180f)
        {
            _xRotation = 90f;
        }
        else if (_xRotation >= 180f && _xRotation < 270f)
        {
            _xRotation = 270f;
        }

        rotation = Quaternion.Euler(new Vector3(_xRotation, _yRotation, 0));
    }
    public override void AfterUpdate(float deltaTime) { }

    public override void InitSubState() { }
    public override void CheckState()
    {
    }

    public override void OnTriggerEnter(Collider other)
    {
        _currentSubState.OnTriggerEnter(other);
    }

    public override void OnTriggerStay(Collider other)
    {
        _currentSubState.OnTriggerStay(other);
    }

    public override void OnTriggerExit(Collider other)
    {
        _currentSubState.OnTriggerExit(other);
    }
}