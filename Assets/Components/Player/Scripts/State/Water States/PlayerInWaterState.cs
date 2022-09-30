using UnityEngine;

public class PlayerInWaterState : PlayerBaseState
{
    public PlayerInWaterState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = true;
        InitSubState();
    }

    public override void EnterState(){
        _ctx.Animator.SetBool("inWater", true);
    }
    public override void ExitState() {
        _ctx.Animator.SetBool("inWater", false);
    }
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
      if(!_ctx.IsInWater && !_ctx.Motor.GroundingStatus.FoundAnyGround){
        SwitchState(_stateFactory.Airborne());
      }
      else if(!_ctx.IsInWater){
        SwitchState(_stateFactory.Grounded());
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