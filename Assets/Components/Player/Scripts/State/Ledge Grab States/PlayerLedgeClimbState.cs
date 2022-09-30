using UnityEngine;
using System.Collections;

public partial class PlayerStateMachine
{
    private bool _ledgeClimbComplete;

    public bool IsLedgeClimbComplete { get { return _ledgeClimbComplete; } set { _ledgeClimbComplete = value; } }
    public void LedgeClimbComplete()
    {
        
        StartCoroutine(MovePlayerToClimbSpot());

    }
    private IEnumerator MovePlayerToClimbSpot()
    {
        _animator.SetBool("ledgeClimbUp", false);
        _animator.SetBool("isLedgeGrabbing", false);
        yield return null;

        _motor.SetPosition(_ledgeGrabLedgeInfo.point);
        _animator.SetBool("isGrounded", true);
        _ledgeClimbComplete = true;
    }
}

public class PlayerLedgeClimbState : PlayerBaseState
{
    private Vector3 _edgePoint;
    private float distanceFromEdge;
    private float distanceFromWall;
    private Vector3 _currentPosition;
    private Vector3 _tempVelocity = Vector3.zero;


    public PlayerLedgeClimbState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = false;
        InitSubState();
    }

    public override void EnterState()
    {
        _ctx.Animator.SetBool("ledgeClimbUp", true);
    }
    public override void ExitState()
    {
        _ctx.IsLedgeClimbComplete = false;
         _ctx.Animator.SetBool("isLedgeGrabbing", false);
    }
    public override void UpdateState()
    {
        CheckState();
    }
    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
        velocity = Vector3.zero;

        if (_ctx.JumpRequested) //TODO: Refactor name
        {
            _ctx.Animator.SetBool("ledgeClimbUp", true);
        }

        _ctx.CameraLookTarget = Vector3.SmoothDamp(
            _ctx.CameraLookTarget,
            _ctx.LedgeGrabLedgeInfo.point + _ctx.CameraLookTargetOffset,
            ref _tempVelocity,
            0.5f,
            Mathf.Infinity);
    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
    }

    public override void InitSubState()
    {
    }
    public override void CheckState()
    {
        if (_ctx.IsLedgeClimbComplete)
        {
            Debug.Log("Back to grounded");
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