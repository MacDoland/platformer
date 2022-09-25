using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name) { 
        _isRootState = true;
        InitSubState();
    }

    public override void EnterState() {
        //Debug.Log("Entering Grounded State");
     }
    public override void ExitState() { }
    public override void UpdateState() {
        CheckState();
    }
    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
    }
    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
    }
    public override void InitSubState() {
        SetSubState(_stateFactory.Move());
    }
    public override void CheckState()
    {
        if (_ctx.JumpRequested)
        {
            SwitchState(_stateFactory.Jump());
        }
        else if (!_ctx.Motor.GroundingStatus.IsStableOnGround)
        {
            SwitchState(_stateFactory.Airborne());
        }
    }
}