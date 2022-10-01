using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerDiveSwimState : PlayerBaseState
{

    private bool _isSwimming = false;
    private float _swimSpeed = 4f;
    private Vector3 _targetVelocity;

    public PlayerDiveSwimState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = false;
        InitSubState();
    }

    public override void EnterState()
    {
        _ctx.Motor.SetGroundSolvingActivation(false);
        Debug.Log("Enter Dive Swim State");
    }
    public override void ExitState()
    {
        _ctx.Motor.SetGroundSolvingActivation(true);
        Debug.Log("Exit Dive Swim State");
    }
    public override void UpdateState()
    {
        CheckState();
    }

    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {

        _ctx.Animator.SetFloat("speed", _ctx.Motor.Velocity.magnitude);

        Debug.Log(_ctx.InputActions.Player.ButtonWest.IsPressed());

        //if ( _ctx.InputActions.Player.ButtonWest.IsPressed())
       // {
            _targetVelocity = _ctx.Camera.transform.forward * _ctx.InputActions.Player.Move.ReadValue<Vector2>().y * _swimSpeed;

            velocity = Vector3.Lerp(velocity, _targetVelocity, 1f - Mathf.Exp(-_ctx.StableMovementSharpness * deltaTime));
      //  }
        //apply water drag
        velocity *= 1f - _ctx.UnderWaterDrag * _ctx.SubmergedAmount * Time.deltaTime;

        _ctx.CameraLookTarget = _ctx.Motor.Transform.position + _ctx.CameraLookTargetOffset;
    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
       rotation = _ctx.Camera.transform.rotation;
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
}