using UnityEngine;

public partial class PlayerStateMachine
{
    [Header("Sprint")]
    [SerializeField]
    private float _sprintMultiplier = 1.5f;
    public float SprintMultiplier { get { return _sprintMultiplier; } }
}

public class PlayerSprintState : PlayerBaseState
{
    public PlayerSprintState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
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
        float velocityMagnitude = _ctx.Motor.Velocity.magnitude;

        Vector3 effectiveGroundNormal = _ctx.Motor.GroundingStatus.GroundNormal;

        // Reorient velocity on slope
        velocity = _ctx.Motor.GetDirectionTangentToSurface(velocity, effectiveGroundNormal) * velocityMagnitude;

        // Calculate target velocity
        Vector3 inputRight = Vector3.Cross(_ctx.MoveInputVector, _ctx.Motor.CharacterUp);
        Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _ctx.MoveInputVector.magnitude;
        Vector3 targetMovementVelocity = reorientedInput * _ctx.MaxStableMoveSpeed * _ctx.SprintMultiplier;

        // Smooth movement Velocity
        velocity = Vector3.Lerp(velocity, targetMovementVelocity, 1f - Mathf.Exp(-_ctx.StableMovementSharpness * deltaTime));
    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
    }

    public override void InitSubState() { }
    public override void CheckState()
    {
        if (!_ctx.SprintButtonHeld)
        {
            SwitchState(_stateFactory.Move());
        }
    }
}