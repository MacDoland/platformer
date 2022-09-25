using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    private bool _ledgeGrabDetected = false;
    private bool _wallSlideDetected = false;
    private Ray _ray;
    private RaycastHit _ledgeGrabWallInfo;
    private RaycastHit _ledgeGrabSpaceInfo;
    private RaycastHit _ledgeGrabLedgeInfo;
    private Transform _transform;
    private bool _leftClear;
    private bool _rightClear;
    private bool _ledgeClimbUp = false;

    private float _distanceFromEdge;
    private float _distanceFromWall;
    private Vector3 _edgePoint;

    private float _desiredDistanceFromWall = 0.5f;

    private Vector3 _tempPosition;
    private Vector3 _tempVelocity;
    private float _heightFromGround = 0f;

    private float cameraLookTargetScreenPosition;


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
    public override void UpdateState()
    {
        CheckState();
    }

    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
        velocity += _ctx.Gravity * deltaTime;
        velocity *= (1f / (1f + (_ctx.Drag * deltaTime)));

        _transform = _ctx.Motor.Transform;
        _ledgeGrabDetected = velocity.y < 0 && IsFacingWall(out _ledgeGrabWallInfo) && IsNearEdge(out _ledgeGrabSpaceInfo)
                    && HasClimbUpPoint(GetDistanceFromWall(_transform.position, _ledgeGrabWallInfo.point), _ledgeGrabWallInfo.normal, out _ledgeGrabLedgeInfo)
                    && HasEnoughClearance(_ctx.Motor.Transform.position, 1.6f, _ctx.PlayerLayer);

        _wallSlideDetected =  !_ledgeGrabDetected && velocity.y < 0 && IsFacingWall(out _ledgeGrabWallInfo);      

        _ctx.LedgeGrabWallInfo = _ledgeGrabWallInfo;
        _ctx.LedgeGrabSpaceInfo = _ledgeGrabSpaceInfo;
        _ctx.LedgeGrabLedgeInfo = _ledgeGrabLedgeInfo;


        _ctx.CameraLookTarget = _ctx.Motor.Transform.position + _ctx.CameraLookTargetOffset;

        //cameraLookTargetScreenPosition = _ctx.Camera.WorldToViewportPoint(_ctx.CameraLookTarget).y;

        // if (cameraLookTargetScreenPosition > 0.95f || cameraLookTargetScreenPosition < 0.05f)
        // {
        //     _ctx.CameraLookTarget = Vector3.SmoothDamp(
        //     _ctx.CameraLookTarget,
        //     _ctx.Motor.Transform.position + _ctx.CameraLookTargetOffset,
        //     ref _tempVelocity,
        //     0.1f,
        //     Mathf.Infinity);
        // }
        // else
        // {
        //     _ctx.CameraLookTarget = _ctx.Motor.Transform.position.XZPlane() + new Vector3(0, _ctx.CameraLookTarget.y, 0);
        // }

    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
       
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
        else if (_ledgeGrabDetected)
        {
            SwitchState(_stateFactory.LedgeGrab());
        }
        else if (_wallSlideDetected)
        {
           // SwitchState(_stateFactory.WallSlide());
           _ctx.IsWallSliding = true;
        }
    }

    private bool IsFacingWall(out RaycastHit hitInfo)
    {
        _ray.origin = _transform.position + Vector3.up * _ctx.ReachHeight;
        _ray.direction = _transform.forward;

        Debug.DrawRay(_ray.origin, _ray.direction, new Color(0.89f, 0.6f, 0.13f));

        return Physics.Raycast(_ray, out hitInfo, _ctx.ReachDistance, _ctx.Layers);
    }

    private bool IsNearEdge(out RaycastHit hitInfo)
    {
        _ray.origin = _transform.position + Vector3.up * _ctx.Height;
        _ray.direction = _transform.forward;

        Debug.DrawRay(_ray.origin, _ray.direction, new Color(0.89f, 0.6f, 0.13f));

        return !Physics.Raycast(_ray, out hitInfo, _ctx.ReachDistance * 2f, _ctx.Layers);
    }

    private bool HasClimbUpPoint(float distanceToWall, Vector3 wallNormal, out RaycastHit hitInfo)
    {

        _ray.origin = _transform.position + Vector3.up * _ctx.Height - wallNormal * (distanceToWall + 0.2f);
        _ray.direction = -Vector3.up;

        Debug.DrawRay(_ray.origin, _ray.direction, new Color(0.89f, 0.6f, 0.13f));

        return Physics.Raycast(_ray, out hitInfo, _ctx.ReachDistance * 2f, _ctx.Layers);
    }

    private bool HasEnoughClearance(Vector3 origin, float requiredClearance, LayerMask ignoreLayers)
    {
        //doesnt have enough clearance if the ray hits something
        Debug.DrawLine(origin, new Vector3(origin.x, origin.y - requiredClearance, origin.z), Color.red);
        return !Physics.Raycast(new Ray(origin, -Vector3.up), requiredClearance, ignoreLayers);
    }

    private float GetDistanceFromWall(Vector3 position, Vector3 wallPoint)
    {
        position.y = 0f;
        wallPoint.y = 0f;

        return Vector3.Distance(position, wallPoint);
    }
}