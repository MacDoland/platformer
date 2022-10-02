using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerSwimState : PlayerBaseState
{
    private bool _orientationReached = false;
    private float _distanceFromSurface = 0f;
    private float _buoyancyY = 0f;
    private float _tempSpeed;
    private float _tempXRotationVelocity;
    private float _tempXRotation = 0f;
    public PlayerSwimState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = false;
        InitSubState();
    }

    public override void EnterState()
    {
        _orientationReached = false;
        _tempXRotation = _ctx.Motor.Transform.rotation.eulerAngles.x;
        _ctx.Motor.SetGroundSolvingActivation(false);
        _ctx.InputActions.Player.ButtonWest.performed += Dive;
        Debug.Log("Enter Swim State");
    }
    public override void ExitState()
    {
        _ctx.Motor.SetGroundSolvingActivation(true);
        _ctx.InputActions.Player.ButtonWest.performed -= Dive;
        Debug.Log("Exit Swim State");
    }
    public override void UpdateState()
    {
        CheckState();
    }

    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {

        _ctx.Animator.SetFloat("speed", _ctx.Motor.Velocity.magnitude);
        float currentVelocityMagnitude = _ctx.Motor.Velocity.magnitude;

        // Calculate target velocity
        // Vector3 inputRight = Vector3.Cross(_ctx.MoveInputVector, _ctx.Motor.CharacterUp);
        // Vector3 reorientedInput = Vector3.Cross(Vector3.up, inputRight).normalized * _ctx.MoveInputVector.magnitude;
        
        Vector3 targetMovementVelocity = _ctx.MoveInputVector * _ctx.SwimSpeed;
        targetMovementVelocity.y = velocity.y;

        // Smooth movement Velocity
        velocity = Vector3.Lerp(velocity, targetMovementVelocity, 1f - Mathf.Exp(-_ctx.StableMovementSharpness * deltaTime));

        _distanceFromSurface = _ctx.WaterLevel - (_ctx.Height * (_ctx.FloatingHeight) - 0.5f) - _ctx.Motor.Transform.position.y;


        // if (_ctx.InputActions.Player.ButtonWest.IsPressed() && velocity.magnitude < _ctx.MaxDiveSwimSpeed)
        // {
        //     velocity += _ctx.Motor.Transform.forward * _ctx.DiveSwimSpeed * deltaTime;
        // }


        //_distanceFromSurface =  (_ctx.WaterLevel - _ctx.Height +   _ctx.Height * _ctx.FloatingHeight) - _ctx.Motor.Transform.position.y;
        //_distanceFromSurface = _ctx.WaterLevel - (_ctx.Motor.Transform.position.y + (_ctx.Height * (1f-_ctx.FloatingHeight)));

        velocity += Vector3.up * _ctx.Gravity * (1f - _ctx.Buoyancy * _distanceFromSurface) * deltaTime;


        if (velocity.y > 0)
        {
            // applying extra drag to avoid breaking surface
            velocity *= 1f - _ctx.WaterDrag * Time.deltaTime;
        }


        //apply water drag
        velocity *= 1f - _ctx.WaterDrag * Time.deltaTime;
        /* */


        // _buoyancyY = Mathf.SmoothDamp(_buoyancyY, _distanceFromSurface, ref _tempSpeed, 0.125f);
        // velocity.y = _buoyancyY;

        _ctx.CameraLookTarget = _ctx.Motor.Transform.position + _ctx.CameraLookTargetOffset;
    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
        if (_ctx.LookInputVector.sqrMagnitude > 0f && _ctx.OrientationSharpness > 0f)
        {
            // Smoothly interpolate from current to target look direction
            Vector3 smoothedLookInputDirection = Vector3.Slerp(_ctx.Motor.CharacterForward, _ctx.LookInputVector, 1 - Mathf.Exp(-_ctx.OrientationSharpness * deltaTime)).normalized;

            // Set the current rotation (which will be used by the KinematicCharacterMotor)
            rotation = Quaternion.LookRotation(smoothedLookInputDirection, _ctx.Motor.CharacterUp);
        }


        if (!_orientationReached && rotation.eulerAngles.x != 0)
        {

            _tempXRotation = Mathf.SmoothDamp(_tempXRotation, 360f, ref _tempXRotationVelocity, 0.1f, 100f);
            if (_tempXRotation >= 359)
            {
                _tempXRotation = 0f;
                _orientationReached = true;
            }

            _ctx.Motor.SetRotation(Quaternion.Euler(new Vector3(_tempXRotation, _ctx.Motor.Transform.eulerAngles.y, 0)));
        }
        else if (!_orientationReached && rotation.eulerAngles.x == 0)
        {
            _orientationReached = true;
        }
    }

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

    public void Dive(InputAction.CallbackContext context)
    {
        Debug.Log("DIVE DIVE DIVE");
        SwitchState(_stateFactory.Dive());
    }
}