using UnityEngine;

public class PatrolLoopState : State
{
    private readonly HunterAgent _agent;
    private readonly PatrolData _patrolData;

    private int _currentWaypointIndex = 0;
    private float _patrolTimer = 0f;
    private float _patrolDuration = 5f; // Duration to stay in patrol state before switching

    public PatrolLoopState(HunterAgent agent, PatrolData patrolData, StateMachine stateMachine) : base(stateMachine)
    {
        _agent = agent;
        _patrolData = patrolData;
    }
    public override void Enter()
    {
        _patrolTimer = 0f;
    }

    public override void Update()
    {
        PatrolLoop();
        _patrolTimer += Time.deltaTime;
        if(_patrolTimer >= _patrolDuration)
        {
            _stateMachine.ChangeState(HunterState.Idle);
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    private void PatrolLoop()
    {
        var currentWaypoint = _patrolData.Waypoints[_currentWaypointIndex];

        if (Vector3.Distance(currentWaypoint.position, _patrolData.Transform.position) <= _patrolData.WaypointCheckDistance)
        {
            _currentWaypointIndex = _currentWaypointIndex + 1 < _patrolData.Waypoints.Count ? _currentWaypointIndex + 1 : 0;
        }

        var nextWaypoint = _patrolData.Waypoints[_currentWaypointIndex];
        
        Vector3 steering = _agent.GetSeekSteering(nextWaypoint);
        _agent.ApplySteering(steering);
        _agent.Move();
    }
}