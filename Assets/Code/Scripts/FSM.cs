using UnityEngine;

public class FSM : MonoBehaviour
{
    [SerializeField] public float _speed = 10f;
    [SerializeField] private PatrolData _patrolData;

    private StateMachine _stateMachine;

    private void Awake()
    {
        _stateMachine = new StateMachine();

        IdleState idleState = new(_stateMachine);
        PatrolLoopState patrolState = new(this, _patrolData, _stateMachine);
        PlacingBaitState placingBaitState = new(transform, _stateMachine);

        _stateMachine.RegisterState(StateType.Idle, idleState);
        _stateMachine.RegisterState(StateType.Patrol, patrolState);
        _stateMachine.RegisterState(StateType.PlacingBait, placingBaitState);

        _stateMachine.ChangeState(StateType.Idle);
    }

    void Update()
    {
        _stateMachine.Update();
    }
}
