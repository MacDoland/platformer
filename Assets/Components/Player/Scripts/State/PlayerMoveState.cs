using UnityEngine;

public partial class PlayerStateMachine {
    [Header("Stable Movement")]
    [SerializeField]
    private float _maxStableMoveSpeed = 10f;
    [SerializeField]
    private float _stableMovementSharpness = 15f;
    [SerializeField]
    private float _orientationSharpness = 10f;

    public float MaxStableMoveSpeed { get { return _maxStableMoveSpeed; } }
    public float StableMovementSharpness { get { return _stableMovementSharpness; } }
}

public class PlayerMoveState : PlayerBaseState
{
    public PlayerMoveState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = false;
        InitSubState();
    }

    public override void EnterState() { }
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
        Vector3 targetMovementVelocity = reorientedInput * _ctx.MaxStableMoveSpeed;

        // Smooth movement Velocity
        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-_ctx.StableMovementSharpness * deltaTime));
        CheckState();
    }
    public override void InitSubState() { }
    public override void CheckState()
    {
        

    }
}