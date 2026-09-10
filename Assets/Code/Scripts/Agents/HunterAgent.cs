using UnityEngine;

public class HunterAgent : SteeringAgent
{
    [SerializeField] private PatrolData patrolData;

    private StateMachine _stateMachine;
    private HunterStateUI _stateUI;

    protected override void Awake()
    {
        base.Awake();
        _stateUI = GetComponent<HunterStateUI>();
        _stateMachine = new StateMachine();
        _stateMachine.OnStateChanged += _stateUI.SetState;

        IdleState idleState = new(_stateMachine);
        PatrolLoopState patrolState = new(this, patrolData, _stateMachine);
        PlacingBaitState placingBaitState = new(transform, _stateMachine);

        _stateMachine.RegisterState(HunterState.Idle, idleState);
        _stateMachine.RegisterState(HunterState.Patrol, patrolState);
        _stateMachine.RegisterState(HunterState.PlacingBait, placingBaitState);

        _stateMachine.ChangeState(HunterState.Idle);
    }

    void Update()
    {
        _stateMachine.Update();
    }

    private void OnDestroy()
    {
        if(_stateUI != null)
            _stateMachine.OnStateChanged -= _stateUI.SetState;
    }

    public Vector3 GetSeekSteering(Transform waypoint)
    {
        return Seek(waypoint.position);
    }
}
