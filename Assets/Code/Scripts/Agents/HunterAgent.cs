using UnityEngine;

public class HunterAgent : Agent
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private PatrolData patrolData;

    private StateMachine _stateMachine;
    private HunterStateUI _stateUI;
    public float Speed => speed;

    private void Awake()
    {
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
}
