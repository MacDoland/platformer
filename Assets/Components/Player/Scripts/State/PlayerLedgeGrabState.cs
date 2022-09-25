using UnityEngine;
using System.Collections;

public class PlayerLedgeGrabState : PlayerBaseState
{
    public PlayerLedgeGrabState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = true;
        InitSubState();
    }

    public override void EnterState()
    {
         SwitchState(_stateFactory.LedgeHang());
    }
    public override void ExitState()
    {
    }
    public override void UpdateState()
    {
        CheckState();
    }
    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
    }

    public override void InitSubState()
    {
        SetSubState(_stateFactory.LedgeHang());
    }
    public override void CheckState()
    {
        if(_ctx.IsLedgeClimbComplete){
            SwitchState(_stateFactory.Grounded());
        }
    }
}