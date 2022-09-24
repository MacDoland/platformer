using UnityEngine;

public partial class PlayerStateMachine
{
    [Header("Air Movement")]
    [SerializeField] private float _maxAirMoveSpeed = 15f;
    [SerializeField] private float _airAccelerationSpeed = 15f;
    [SerializeField] private float _drag = 0.1f;

    public float MaxAirMoveSpeed { get { return _maxAirMoveSpeed; } }
    public float AirAccelerationSpeed { get { return _airAccelerationSpeed; } }
    public float Drag { get { return _drag; } }
}

public class PlayerAirMoveState : PlayerBaseState
{
    public PlayerAirMoveState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = false;
        InitSubState();
    }

    public override void EnterState() {
        Debug.Log("Entering AirMove State");
     }
    public override void ExitState() { }
    public override void UpdateState(ref Vector3 currentVelocity, float deltaTime)
    {
        if (_ctx.MoveInputVector.sqrMagnitude > 0f)
        {
            Vector3 addedVelocity = _ctx.MoveInputVector * _ctx.AirAccelerationSpeed * deltaTime;

            Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, _ctx.Motor.CharacterUp);

            // Limit air velocity from inputs
            if (currentVelocityOnInputsPlane.magnitude < _ctx.MaxAirMoveSpeed)
            {
                // clamp addedVel to make total vel not exceed max vel on inputs plane
                Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, _ctx.MaxAirMoveSpeed);
                addedVelocity = newTotal - currentVelocityOnInputsPlane;
            }
            else
            {
                // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                {
                    addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                }
            }

            // Prevent air-climbing sloped walls
            if (_ctx.Motor.GroundingStatus.FoundAnyGround)
            {
                if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                {
                    Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(_ctx.Motor.CharacterUp, _ctx.Motor.GroundingStatus.GroundNormal), _ctx.Motor.CharacterUp).normalized;
                    addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                }
            }

            // Apply added velocity
            currentVelocity += addedVelocity;
        }

        CheckState();
    }
    public override void InitSubState() { }
    public override void CheckState()
    {

    }
}