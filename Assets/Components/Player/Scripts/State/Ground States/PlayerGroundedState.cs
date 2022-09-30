using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    private Vector3 _tempPosition;
    private Vector3 _tempVelocity;

    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = true;
        InitSubState();
    }

    public override void EnterState()
    {
        //Debug.Log("Entering Grounded State");
    }
    public override void ExitState() { }
    public override void UpdateState()
    {
        CheckState();
    }
    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
        //Lets smooth the vertical camera motion but keep the horizontal movement instant
        _tempPosition = Vector3.SmoothDamp(
        _ctx.CameraLookTarget.YOnly(),
        _ctx.Motor.Transform.position.YOnly() + _ctx.CameraLookTargetOffset.YOnly(),
            ref _tempVelocity,
            0.1f,
            Mathf.Infinity);

        _ctx.CameraLookTarget = _ctx.Motor.Transform.position.XZPlane() + _tempPosition;


    }
    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
    }
    public override void InitSubState()
    {
        SetSubState(_stateFactory.Move());
    }
    public override void CheckState()
    {
        if (_ctx.JumpRequested)
        {
            SwitchState(_stateFactory.Jump());
            _ctx.JumpRequested = false;
        }
        else if (_ctx.IsInWater){
            SwitchState(_stateFactory.InWater());
        }
        else if (!_ctx.Motor.GroundingStatus.FoundAnyGround)
        {
            SwitchState(_stateFactory.Airborne());
        }
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