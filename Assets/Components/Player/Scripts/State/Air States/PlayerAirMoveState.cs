using UnityEngine;

public class PlayerAirMoveState : PlayerBaseState
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

    public PlayerAirMoveState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = false;
        InitSubState();
    }

    public override void EnterState()
    {
        Debug.Log("Entering AirMove State");
    }
    public override void ExitState()
    {
        Debug.Log("Exiting AirMove State");
    }
    public override void UpdateState()
    {
        CheckState();
    }

    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
        if (_ctx.MoveInputVector.sqrMagnitude > 0f)
        {
            Vector3 addedVelocity = _ctx.MoveInputVector * _ctx.AirAccelerationSpeed * deltaTime;

            Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(velocity, _ctx.Motor.CharacterUp);

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
                if (Vector3.Dot(velocity + addedVelocity, addedVelocity) > 0f)
                {
                    Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(_ctx.Motor.CharacterUp, _ctx.Motor.GroundingStatus.GroundNormal), _ctx.Motor.CharacterUp).normalized;
                    addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                }
            }

            // Apply added velocity
            velocity += addedVelocity;
        }

        _transform = _ctx.Motor.Transform;
        _ledgeGrabDetected = velocity.y < 0 && IsFacingWall(out _ledgeGrabWallInfo) && IsNearEdge(out _ledgeGrabSpaceInfo)
                    && HasClimbUpPoint(GetDistanceFromWall(_transform.position, _ledgeGrabWallInfo.point), _ledgeGrabWallInfo.normal, out _ledgeGrabLedgeInfo)
                    && HasEnoughClearance(_ctx.Motor.Transform.position, 1.6f, _ctx.PlayerLayer);

        _wallSlideDetected = !_ledgeGrabDetected && velocity.y < 0 && IsFacingWall(out _ledgeGrabWallInfo);

        _ctx.LedgeGrabWallInfo = _ledgeGrabWallInfo;
        _ctx.LedgeGrabSpaceInfo = _ledgeGrabSpaceInfo;
        _ctx.LedgeGrabLedgeInfo = _ledgeGrabLedgeInfo;


        _ctx.CameraLookTarget = _ctx.Motor.Transform.position + _ctx.CameraLookTargetOffset;
    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
        if (_ctx.LookInputVector.sqrMagnitude > 0f && _ctx.OrientationSharpness > 0f)
        {
            // Smoothly interpolate from current to target look direction
            Vector3 smoothedLookInputDirection = Vector3.Slerp(_ctx.Motor.CharacterForward, _ctx.LookInputVector, 1 - Mathf.Exp(-_ctx.OrientationSharpness * deltaTime)).normalized;

            // Set the current rotation (which will be used by the KinematicCharacterMotor)
            rotation = Quaternion.LookRotation(smoothedLookInputDirection, _ctx.Motor.CharacterUp);
        }
    }

    public override void InitSubState() { }
    public override void CheckState()
    {
        if (_ledgeGrabDetected)
        {
            SwitchState(_stateFactory.LedgeGrab());
        }
        else if (_wallSlideDetected)
        {
            // SwitchState(_stateFactory.WallSlide());
            SwitchState(_stateFactory.WallSlide());
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

    public override void OnTriggerEnter(Collider other)
    {
        
    }

    public override void OnTriggerExit(Collider other)
    {
        
    }
}