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
        //Debug.Log("Entering Airborne State");
    }
    public override void ExitState() { }
    public override void UpdateState()
    {
        CheckState();
    }

    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
        velocity += _ctx.Gravity * deltaTime;
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
    }

}