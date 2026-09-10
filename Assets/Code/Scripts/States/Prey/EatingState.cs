using UnityEngine;

public class EatingState : State
{
    private readonly PreyAgent _agent;
    private float _currentEatingTime;
    public EatingState(StateMachine stateMachine, PreyAgent agent) : base(stateMachine)
    {
        _agent = agent;
    }

    public override void Enter()
    {
        _currentEatingTime = 0;
        _agent.StopSteering();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        _currentEatingTime += Time.deltaTime;
        if (_currentEatingTime >= _agent.EatingTime)
        {
            _agent.ConsumeTargetBait();
            _currentEatingTime = 0;
            _stateMachine.ChangeState(PreyState.Flocking);
            return;
        }
    }
}
