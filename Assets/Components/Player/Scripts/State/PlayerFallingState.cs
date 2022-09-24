using UnityEngine;

public class PlayerFallingState : PlayerBaseState
{
    public PlayerFallingState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name) { 
        _isRootState = true;
        InitSubState();
    }

    public override void EnterState() { }
    public override void ExitState() { }
    public override void UpdateState(ref Vector3 velocity, float deltaTime) {
        CheckState();
     }
    public override void InitSubState() { }
    public override void CheckState()
    {
        if (_ctx.Motor.GroundingStatus.IsStableOnGround)
        {
            SwitchState(_stateFactory.Grounded());
        }
        
    }
}