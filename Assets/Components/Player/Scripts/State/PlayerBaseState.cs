using UnityEngine;

public abstract class PlayerBaseState
{
    protected bool _isRootState = false;
    protected PlayerStateMachine _ctx;
    protected PlayerStateFactory _stateFactory;
    protected PlayerBaseState _currentSubState;
    protected PlayerBaseState _currentSuperState;
    protected string _stateName;

    public string Name { get { return _stateName; } }

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, string stateName)
    {
        _ctx = currentContext;
        _stateFactory = playerStateFactory;
        _stateName = stateName;
    }

    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract void UpdateStateVelocity(ref Vector3 velocity, float deltaTime);
    public abstract void UpdateStateRotation(ref Quaternion rotation, float deltaTime);

    public abstract void CheckState();
    public abstract void InitSubState();

    public void UpdateStates()
    {
        UpdateState();

        if (_currentSubState != null)
        {
            _currentSubState.UpdateStates();
        }
    }

    public void UpdateStateRotations(ref Quaternion rotation, float deltaTime)
    {
        UpdateStateRotation(ref rotation, deltaTime);

        if (_currentSubState != null)
        {
            _currentSubState.UpdateStateRotations(ref rotation, deltaTime);
        }
    }

    public void UpdateStateVelocitys(ref Vector3 velocity, float deltaTime)
    {
        UpdateStateVelocity(ref velocity, deltaTime);

        if (_currentSubState != null)
        {
            _currentSubState.UpdateStateVelocitys(ref velocity, deltaTime);
        }
    }

    public string GetFullStateName()
    {
        var name = this.Name;

        if (_currentSubState != null)
        {
            name = name + ":" + _currentSubState.GetFullStateName();
        }

        return name;
    }

    protected void SwitchState(PlayerBaseState newState)
    {
        //Exit Children States
        if (_currentSubState != null)
        {
            _currentSubState.ExitState();
        }
        //exit current state
        ExitState();

        //enter new state
        newState.EnterState();

        if (_isRootState)
        {
            //set current state
            _ctx.CurrentState = newState;
        }
        else if (_currentSuperState != null)
        {
            _currentSuperState.SetSubState(newState);
        }
    }

    protected void SetSuperState(PlayerBaseState newSuperState)
    {
        _currentSuperState = newSuperState;
    }

    protected void SetSubState(PlayerBaseState newSubState)
    {
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }
}