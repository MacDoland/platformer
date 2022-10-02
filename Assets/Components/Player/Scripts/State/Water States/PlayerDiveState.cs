using UnityEngine;

public class PlayerDiveState : PlayerBaseState
{
    private bool _isDiving = false;
    private float _exitDelay = 1f;
    private float _currentDelay = 0f;
    private float _diveProgress = 0f;
    private Vector3 _eulerRotation = Vector3.zero;
    private Vector3 _targetEulerRotation = Vector3.zero;

    public PlayerDiveState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string name)
    : base(currentContext, playerStateFactory, name)
    {
        _isRootState = false;
        InitSubState();
    }

    public override void EnterState()
    {
        _isDiving = false;
        _currentDelay = 0f;
        _ctx.WaterCamera.Priority = 100;
        _eulerRotation = _ctx.Motor.Transform.rotation.eulerAngles;
        _targetEulerRotation = new Vector3(45f, _ctx.Motor.Transform.rotation.eulerAngles.y, _ctx.Motor.Transform.rotation.eulerAngles.z);
    }
    public override void ExitState()
    {
        _currentDelay = 0f;
    }
    public override void UpdateState()
    {
        CheckState();
    }

    public override void UpdateStateVelocity(ref Vector3 velocity, float deltaTime)
    {
        _currentDelay += deltaTime;

        if (!_isDiving)
        {
            velocity -= Vector3.up * 4f;
        }

        _isDiving = true;

        //apply water drag
        velocity *= 1f - _ctx.WaterDrag * _ctx.SubmergedAmount * Time.deltaTime;

        _diveProgress = _currentDelay / _exitDelay;


        if (_currentDelay > _exitDelay)
        {
            SwitchState(_stateFactory.DiveSwim());
        }
    }

    public override void UpdateStateRotation(ref Quaternion rotation, float deltaTime)
    {
        _eulerRotation = Vector3.Lerp(_eulerRotation, _targetEulerRotation, _diveProgress);
        rotation = Quaternion.Euler(_eulerRotation);
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