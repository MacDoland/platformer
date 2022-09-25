using UnityEngine;

public class PlayerWallSlideState : PlayerBaseState
{
    private float wallSlideclampY = -6f;
    private float wallJumpIntent = 0f;
    private float wallJumpVel = 0f;

    public PlayerWallSlideState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = false;
        InitSubState();
    }

    public override void EnterState()
    {
       _ctx.Animator.SetBool("wallSlide", true);
    }
    public override void ExitState() { 
        _ctx.IsWallSliding = false;
        _ctx.Animator.SetBool("wallSlide", false);
    }
    public override void UpdateState()
    {
        CheckState();
    }

    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
        velocity.y = Mathf.Clamp(velocity.y, wallSlideclampY, Mathf.Infinity);
            velocity.x = 0f;
            velocity.z = 0f;
    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
    }

    public override void InitSubState() { }
    public override void CheckState()
    {
        if(_ctx.Motor.GroundingStatus.FoundAnyGround) {
            SwitchState(_stateFactory.AirMove());
        }
    }
}