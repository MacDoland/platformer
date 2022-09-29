using UnityEngine;

public class PlayerSwimState : PlayerBaseState
{
    private Ray _surfaceRay;
    private bool _isOnSurface = false;
    private RaycastHit _hitInfo;
    private float distanceFromBodyToSurface = 0.5f;

    public PlayerSwimState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = false;
        InitSubState();
    }

    public override void EnterState() { }
    public override void ExitState() { }
    public override void UpdateState()
    {
        CheckState();
    }

    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
        _surfaceRay.origin = _ctx.Motor.Transform.position + Vector3.up * _ctx.Height;
        _surfaceRay.direction = -Vector3.up;
        _isOnSurface = !Physics.Raycast(_surfaceRay, out _hitInfo, distanceFromBodyToSurface, _ctx.WaterLayer, QueryTriggerInteraction.Collide);


        if(!_isOnSurface){
            Debug.Log("Under water");
        }
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
    }

    public override void OnTriggerEnter(Collider other)
    {
        _currentSubState.OnTriggerEnter(other);
    }

    public override void OnTriggerExit(Collider other)
    {
        _currentSubState.OnTriggerExit(other);
    }
}