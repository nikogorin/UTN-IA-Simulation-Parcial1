using UnityEngine;

public class PlacingBaitState : State
{
    private float _placingBaitTime = 0f;
    private readonly float _timerToChange = 0.5f;
    private readonly Transform _transform;
    
    public PlacingBaitState(Transform transform, StateMachine stateMachine) : base(stateMachine)
    {
        _transform = transform;
    }

    public override void Enter()
    {
        _placingBaitTime = 0f;
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        _placingBaitTime += Time.deltaTime;

        if (_timerToChange <= _placingBaitTime)
        {
            if(!BaitManager.Instance.TrySpawnBait(_transform.position))
                Debug.Log("Failed to spawn bait.");

            _stateMachine.ChangeState(HunterState.Patrol);
            return;
        }
    }
}
