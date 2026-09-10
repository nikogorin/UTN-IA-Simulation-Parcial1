using UnityEngine;

public class IdleState : State
{
    private float _idleTime = 0f;
    private readonly float _timerToChange = 3f;

    public IdleState(StateMachine stateMachine) : base(stateMachine)
    {

    }

    public override void Enter()
    {
        _idleTime = 0f;
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        _idleTime += Time.deltaTime;

        if (_timerToChange <= _idleTime)
        {
            _stateMachine.ChangeState(HunterState.PlacingBait);
            return;
        }
    }
}
