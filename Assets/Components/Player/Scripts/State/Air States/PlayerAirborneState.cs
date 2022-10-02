using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    public PlayerAirborneState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = true;
        InitSubState();
    }

    public override void EnterState()
    {
        SwitchState(_stateFactory.AirMove());
    }
    public override void ExitState() { }
    public override void UpdateState()
    {
        CheckState();
    }

    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
        velocity += Vector3.up * _ctx.Gravity * deltaTime;
        velocity *= (1f / (1f + (_ctx.Drag * deltaTime)));
    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
       
    }

    public override void InitSubState()
    {
        SetSubState(_stateFactory.AirMove());
    }
    public override void CheckState()
    {
        if (_ctx.Motor.GroundingStatus.IsStableOnGround)
        {
            SwitchState(_stateFactory.Grounded());
        }
        else if (_ctx.IsInWater) {
             SwitchState(_stateFactory.InWater());
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