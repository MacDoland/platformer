using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = true;
        InitSubState();
    }

    public override void EnterState()
    {
        //Debug.Log("Entering Jump State");
    }
    public override void ExitState()
    {
        _ctx.JumpRequested = false;
        _ctx.JumpConsumed = true;
        _ctx.JumpedThisFrame = true;
    }
    public override void UpdateState()
    {
        CheckState();
    }
    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
        if (!_ctx.JumpConsumed && ((_ctx.AllowJumpingWhenSliding ? _ctx.Motor.GroundingStatus.FoundAnyGround : _ctx.Motor.GroundingStatus.IsStableOnGround) || _ctx.TimeSinceLastAbleToJump <= _ctx.JumpPostGroundingGraceTime))
        {

            Vector3 jumpDirection = _ctx.Motor.CharacterUp;
            if (_ctx.Motor.GroundingStatus.FoundAnyGround && !_ctx.Motor.GroundingStatus.IsStableOnGround)
            {
                jumpDirection = _ctx.Motor.GroundingStatus.GroundNormal;
            }

            // Makes the character skip ground probing/snapping on its next update. 
            // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
            _ctx.Motor.ForceUnground();

            // Add to the return velocity and reset jump state
            velocity += (jumpDirection * _ctx.JumpUpSpeed) - Vector3.Project(velocity, _ctx.Motor.CharacterUp);
            velocity += (_ctx.MoveInputVector * _ctx.JumpScalableForwardSpeed);

            _ctx.JumpRequested = false;
            _ctx.JumpConsumed = true;
            _ctx.JumpedThisFrame = true;
        }
    }
    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
    }
    public override void InitSubState() { }
    public override void CheckState()
    {
        SwitchState(_stateFactory.Airborne());
    }

    public override void OnTriggerEnter(Collider other)
    {
        
    }

    public override void OnTriggerExit(Collider other)
    {
        
    }
}