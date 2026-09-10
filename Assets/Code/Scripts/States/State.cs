public enum HunterState
{
    Idle,
    Patrol,
    PlacingBait,
}

public abstract class State
{
    protected StateMachine _stateMachine;
    protected State(StateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public virtual void Enter() { }

    public virtual void Exit() { }

    public virtual void Update() { }
}