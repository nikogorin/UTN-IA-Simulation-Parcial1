using UnityEngine;

public class PatrolPingPongState : State
{
    private readonly HunterAgent _agent;
    private readonly PatrolData _patrolData;

    private int _currentWaypointIndex = 0;
    private int _patrolDirection = 1;

    public PatrolPingPongState(HunterAgent fSM, PatrolData patrolData, StateMachine stateMachine) : base(stateMachine)
    {
        _agent = fSM;
        _patrolData = patrolData;
    }
    public override void Enter()
    {
        //Debug.Log("Entering PatrolPingPong State");
    }

    public override void Update()
    {
        PatrolPingPong();
    }

    public override void Exit()
    {
        //Debug.Log("Exiting PatrolPingPong State");
    }

    private void PatrolPingPong()
    {
        var nextWaypoint = _patrolData.Waypoints[_currentWaypointIndex];

        if (Vector3.Distance(nextWaypoint.position, _patrolData.Transform.position) <= _patrolData.WaypointCheckDistance)
        {
            _currentWaypointIndex += _patrolDirection;

            if (_currentWaypointIndex >= _patrolData.Waypoints.Count)
            {
                _currentWaypointIndex = _patrolData.Waypoints.Count - 1;
                _patrolDirection = -1;
            }
            else if (_currentWaypointIndex < 0)
            {
                _currentWaypointIndex = 0;
                _patrolDirection = 1;
            }
        }

        var direction = (nextWaypoint.position - _patrolData.Transform.position).normalized;

        _patrolData.Transform.position += _agent.Speed * Time.deltaTime * direction;
    }
}