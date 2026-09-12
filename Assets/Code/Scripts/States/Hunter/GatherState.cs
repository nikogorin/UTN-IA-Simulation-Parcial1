using UnityEngine;

public class GatherState : State
{
    private readonly HunterAgent _agent;
    private float _currentTime = 0;

    public GatherState(HunterAgent agent, StateMachine stateMachine) : base(stateMachine)
    {
        _agent = agent;
    }

    public override void Enter()
    {
        _currentTime = 0;
        _agent.StopMoving();
    }

    public override void Update()
    {
        if (!_agent.HasTargetDead)
        {
            _stateMachine.ChangeState(HunterState.Patrol);
            return;
        }

        _currentTime += Time.deltaTime;

        if(_currentTime >= _agent.GatherDuration)
        {
            _agent.GatherPreyAgentDead();
            _stateMachine.ChangeState(HunterState.Patrol);
            return;
        }
    }
}