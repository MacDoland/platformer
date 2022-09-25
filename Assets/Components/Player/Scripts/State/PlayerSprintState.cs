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
    public override void UpdateState(ref Vector3 currentVelocity, float deltaTime)
    {
        _ctx.Animator.SetFloat("speed", _ctx.Motor.Velocity.magnitude);
        float currentVelocityMagnitude = _ctx.Motor.Velocity.magnitude;

        Vector3 effectiveGroundNormal = _ctx.Motor.GroundingStatus.GroundNormal;

        // Reorient velocity on slope
        currentVelocity = _ctx.Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

        // Calculate target velocity
        Vector3 inputRight = Vector3.Cross(_ctx.MoveInputVector, _ctx.Motor.CharacterUp);
        Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _ctx.MoveInputVector.magnitude;
        Vector3 targetMovementVelocity = reorientedInput * _ctx.MaxStableMoveSpeed * _ctx.SprintMultiplier;

        // Smooth movement Velocity
        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-_ctx.StableMovementSharpness * deltaTime));
        CheckState();
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