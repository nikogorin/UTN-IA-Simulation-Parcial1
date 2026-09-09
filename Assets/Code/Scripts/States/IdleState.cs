using UnityEngine;

public class IdleState : State
{
    float _idleTime = 0f;
    float _timerToChange = 3f;

    public IdleState(StateMachine stateMachine) : base(stateMachine)
    {

    }

    public override void Enter()
    {
        //Debug.Log("Entering Idle State");
        _idleTime = 0f;
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Idle State");
    }

    public override void Update()
    {
        //Debug.Log("Updating Idle State");
        _idleTime += Time.deltaTime;

        if (_timerToChange <= _idleTime)
        {
            _stateMachine.ChangeState(StateType.PlacingBait);
        }
    }
}
