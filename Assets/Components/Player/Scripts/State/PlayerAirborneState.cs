using UnityEngine;

public partial class PlayerStateMachine
{
    [Header("Gravity")]
    [SerializeField] private Vector3 _gravity = new Vector3(0, -40f, 0);

    public Vector3 Gravity { get { return _gravity; } }
}

public class PlayerAirborneState : PlayerBaseState
{
    public PlayerAirborneState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = true;
        InitSubState();
    }

    public override void EnterState()
    {
        //Debug.Log("Entering Airborne State");
    }
    public override void ExitState() { }
    public override void UpdateState(ref Vector3 currentVelocity, float deltaTime)
    {
        currentVelocity += _ctx.Gravity * deltaTime;
        currentVelocity *= (1f / (1f + (_ctx.Drag * deltaTime)));
        CheckState();
    }
    public override void InitSubState()
    {
        SetSubState(_stateFactory.AirMove());
    }
    public override void CheckState()
    {
        if (_ctx.Motor.GroundingStatus.IsStableOnGround)
        {
            SwitchState(_stateFactory.Grounded());
        }
    }
}