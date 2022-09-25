using System.Collections.Generic;

enum PlayerStates
{
    grounded, //super
    airborne, //super
    move, //sub
    airMove, //sub
    idle, //sub
    sprint, //sub
    jump //sub
}

public class PlayerStateFactory
{
    PlayerStateMachine _context;
    Dictionary<PlayerStates, PlayerBaseState> _states = new Dictionary<PlayerStates, PlayerBaseState>();

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
        // sub states
        _states[PlayerStates.move] = new PlayerMoveState(_context, this, "Move");
        _states[PlayerStates.sprint] = new PlayerSprintState(_context, this, "Sprint");
        _states[PlayerStates.airMove] = new PlayerAirMoveState(_context, this, "AirMove");
        _states[PlayerStates.jump] = new PlayerJumpState(_context, this, "Jump");

        // root states
        _states[PlayerStates.grounded] = new PlayerGroundedState(_context, this, "Grounded");
        _states[PlayerStates.airborne] = new PlayerAirborneState(_context, this, "Airborne");
    }

    public PlayerBaseState Grounded()
    {
        return _states[PlayerStates.grounded];
    }

    public PlayerBaseState Airborne()
    {
        return _states[PlayerStates.airborne];
    }

    public PlayerBaseState Move()
    {
        return _states[PlayerStates.move];
    }
    public PlayerBaseState Sprint()
    {
        return _states[PlayerStates.sprint];
    }

    public PlayerBaseState AirMove()
    {
        return _states[PlayerStates.airMove];
    }

    public PlayerBaseState Jump()
    {
        return _states[PlayerStates.jump];
    }
}