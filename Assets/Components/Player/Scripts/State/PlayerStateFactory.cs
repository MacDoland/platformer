using System.Collections.Generic;

enum PlayerStates
{
    grounded, //super
    airborne, //super
    ledgeGrab, //super
    inWater, //super
    move, //sub
    airMove, //sub
    wallSlide, // sub
    idle, //sub
    sprint, //sub
    jump, //sub
    ledgeHang, //sub
    ledgeClimb, //sub
    swim, // sub,
    dive // sub
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
        _states[PlayerStates.wallSlide] = new PlayerWallSlideState(_context, this, "WallSlide");
        _states[PlayerStates.jump] = new PlayerJumpState(_context, this, "Jump");
        _states[PlayerStates.ledgeHang] = new PlayerLedgeHangState(_context, this, "LedgeHang");
        _states[PlayerStates.ledgeClimb] = new PlayerLedgeClimbState(_context, this, "LedgeClimb");
        _states[PlayerStates.swim] = new PlayerSwimState(_context, this, "Swim");
        _states[PlayerStates.dive] = new PlayerDiveState(_context, this, "Dive");

        // root states
        _states[PlayerStates.grounded] = new PlayerGroundedState(_context, this, "Grounded");
        _states[PlayerStates.airborne] = new PlayerAirborneState(_context, this, "Airborne");
        _states[PlayerStates.ledgeGrab] = new PlayerLedgeGrabState(_context, this, "LedgeGrab");
        _states[PlayerStates.inWater] = new PlayerInWaterState(_context, this, "InWater");
    }

    public PlayerBaseState Grounded()
    {
        return _states[PlayerStates.grounded];
    }

    public PlayerBaseState Airborne()
    {
        return _states[PlayerStates.airborne];
    }

    public PlayerBaseState LedgeGrab()
    {
        return _states[PlayerStates.ledgeGrab];
    }

        public PlayerBaseState InWater()
    {
        return _states[PlayerStates.inWater];
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

    public PlayerBaseState WallSlide()
    {
        return _states[PlayerStates.wallSlide];
    }

    public PlayerBaseState Jump()
    {
        return _states[PlayerStates.jump];
    }

    public PlayerBaseState LedgeHang()
    {
        return _states[PlayerStates.ledgeHang];
    }

    public PlayerBaseState LedgeClimb()
    {
        return _states[PlayerStates.ledgeClimb];
    }

    public PlayerBaseState Swim()
    {
        return _states[PlayerStates.swim];
    }

    public PlayerBaseState Dive()
    {
        return _states[PlayerStates.dive];
    }
}