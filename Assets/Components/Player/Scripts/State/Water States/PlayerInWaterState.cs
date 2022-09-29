using UnityEngine;

public class PlayerInWaterState : PlayerBaseState
{
    public PlayerInWaterState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = true;
        InitSubState();
    }

    public override void EnterState(){}
    public override void ExitState() {}
    public override void UpdateState()
    {
        CheckState();
    }
    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
    }

    public override void InitSubState()
    {
        SetSubState(_stateFactory.Swim());
    }
    public override void CheckState()
    {
      
    }

    public override void OnTriggerEnter(Collider other)
    {
        
    }

    public override void OnTriggerExit(Collider other)
    {
        
    }
}