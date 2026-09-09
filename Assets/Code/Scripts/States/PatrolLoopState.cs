using UnityEngine;

public class PatrolLoopState : State
{
    private readonly FSMAgent _agent;
    private readonly PatrolData _patrolData;

    private int _currentWaypointIndex = 0;
    private float _patrolTimer = 0f;
    private float _patrolDuration = 5f; // Duration to stay in patrol state before switching

    public PatrolLoopState(FSMAgent fSM, PatrolData patrolData, StateMachine stateMachine) : base(stateMachine)
    {
        _agent = fSM;
        _patrolData = patrolData;
    }
    public override void Enter()
    {
        //Debug.Log("Entering PatrolLoop State");
        _patrolTimer = 0f;
    }

    public override void Update()
    {
        PatrolLoop();
        _patrolTimer += Time.deltaTime;
        if(_patrolTimer >= _patrolDuration)
        {
            _stateMachine.ChangeState(StateType.Idle);
        }
    }

    public override void Exit()
    {
        //Debug.Log("Exiting PatrolLoop State");
    }

    private void PatrolLoop()
    {
        var nextWaypoint = _patrolData.Waypoints[_currentWaypointIndex];

        if (Vector3.Distance(nextWaypoint.position, _patrolData.Transform.position) <= _patrolData.WaypointCheckDistance)
        {
            _currentWaypointIndex = _currentWaypointIndex + 1 < _patrolData.Waypoints.Count ? _currentWaypointIndex + 1 : 0;
        }

        var direction = (nextWaypoint.position - _patrolData.Transform.position).normalized;

        _patrolData.Transform.position += _agent._speed * Time.deltaTime * direction;
        _patrolData.Transform.forward = direction;
    }
}