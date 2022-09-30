using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    public PlayerMoveState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = false;
        InitSubState();
    }

    public override void EnterState()
    {
        //Debug.Log("Entering Move State");
    }
    public override void ExitState() { }
    public override void UpdateState()
    {

        CheckState();
    }
    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
        _ctx.Animator.SetFloat("speed", _ctx.Motor.Velocity.magnitude);
        float currentVelocityMagnitude = _ctx.Motor.Velocity.magnitude;

        Vector3 effectiveGroundNormal = _ctx.Motor.GroundingStatus.GroundNormal;

        // Reorient velocity on slope
        velocity = _ctx.Motor.GetDirectionTangentToSurface(velocity, effectiveGroundNormal) * currentVelocityMagnitude;

        // Calculate target velocity
        Vector3 inputRight = Vector3.Cross(_ctx.MoveInputVector, _ctx.Motor.CharacterUp);
        Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _ctx.MoveInputVector.magnitude;
        Vector3 targetMovementVelocity = reorientedInput * _ctx.MaxStableMoveSpeed;

        // Smooth movement Velocity
        velocity = Vector3.Lerp(velocity, targetMovementVelocity, 1f - Mathf.Exp(-_ctx.StableMovementSharpness * deltaTime));
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
    }
    public override void InitSubState() { }
    public override void CheckState()
    {

        if (_ctx.SprintButtonHeld)
        {
            SwitchState(_stateFactory.Sprint());
        }
    }

    public override void OnTriggerEnter(Collider other)
    {
        
    }

    public override void OnTriggerStay(Collider other)
    {
       
    }

    public override void OnTriggerExit(Collider other)
    {
        
    }
}