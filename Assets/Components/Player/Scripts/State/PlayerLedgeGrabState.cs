using UnityEngine;

public class PlayerLedgeGrabState : PlayerBaseState
{
    private Vector3 _edgePoint;
    private float distanceFromEdge;
    private float distanceFromWall;
    private Vector3 _currentPosition;

    public PlayerLedgeGrabState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = true;
        InitSubState();
    }

    public override void EnterState()
    {
          _ctx.Animator.SetBool("isLedgeGrabbing", true);
         _currentPosition = _ctx.Motor.Transform.position;
        // _edgePoint = _ctx.LedgeGrabWallInfo.point;
        // _edgePoint.y = _ctx.LedgeGrabLedgeInfo.point.y;
        // distanceFromEdge = Vector3.Distance(_ctx.LedgeGrabWallInfo.point, _edgePoint);
        // distanceFromWall = Vector3.Distance( _ctx.Motor.Transform.position + Vector3.up * _ctx.ReachHeight, _ctx.LedgeGrabWallInfo.point);
        // _ctx.Motor.SetPosition( _currentPosition + Vector3.up * (distanceFromEdge) + _ctx.LedgeGrabWallInfo.normal * _ctx.KeepDistanceFromWall);
    }
    public override void ExitState() { }
    public override void UpdateState()
    {
        CheckState();
    }
    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
         velocity = Vector3.zero;
        _edgePoint = _ctx.LedgeGrabWallInfo.point;
        _edgePoint.y = _ctx.LedgeGrabLedgeInfo.point.y;
        distanceFromEdge = Vector3.Distance(_ctx.LedgeGrabWallInfo.point, _edgePoint);
        distanceFromWall = Vector3.Distance( _ctx.Motor.Transform.position + Vector3.up * _ctx.ReachHeight, _ctx.LedgeGrabWallInfo.point);
        _ctx.Motor.SetPosition( _currentPosition + Vector3.up * (distanceFromEdge + _ctx.LedgeGrabOffsetY) + _ctx.LedgeGrabWallInfo.normal * _ctx.KeepDistanceFromWall);
    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
    }

    public override void InitSubState()
    {
    }
    public override void CheckState()
    {
    }
}