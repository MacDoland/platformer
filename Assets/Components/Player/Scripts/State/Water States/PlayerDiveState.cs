using UnityEngine;

public class PlayerDiveState : PlayerBaseState
{
    private bool _isDiving = false;

    public PlayerDiveState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = false;
        InitSubState();
    }

    public override void EnterState()
    {

    }
    public override void ExitState() { }
    public override void UpdateState()
    {
        CheckState();
    }

    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
        if (!_isDiving)
        {
            velocity -= Vector3.up * 4f;
        }

        _isDiving = true;

        //apply water drag
        velocity *= 1f - _ctx.WaterDrag * _ctx.SubmergedAmount * Time.deltaTime;
    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {

    }

    public override void InitSubState() { }
    public override void CheckState()
    {
    }

    public override void OnTriggerEnter(Collider other)
    {
        _currentSubState.OnTriggerEnter(other);
    }

    public override void OnTriggerStay(Collider other)
    {
        _currentSubState.OnTriggerStay(other);
    }

    public override void OnTriggerExit(Collider other)
    {
        _currentSubState.OnTriggerExit(other);
    }
}