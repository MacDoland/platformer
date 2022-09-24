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
    public abstract void UpdateState(ref Vector3 velocity, float deltaTime);
    public abstract void CheckState();
    public abstract void InitSubState();

    public void UpdateStates(ref Vector3 velocity, float deltaTime)
    {
        UpdateState(ref velocity, deltaTime);

        if (_currentSubState != null)
        {
            _currentSubState.UpdateStates(ref velocity, deltaTime);
        }
    }

    protected void SwitchState(PlayerBaseState newState)
    {
        //exit current state
        ExitState();

        //enter new state
        newState.EnterState();

        if (_isRootState)
        {
            //set current state
            _ctx.CurrentState = newState;
        }
        else if (_currentSuperState != null) {
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