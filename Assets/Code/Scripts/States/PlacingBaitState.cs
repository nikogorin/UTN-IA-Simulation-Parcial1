using UnityEngine;

public class PlacingBaitState : State
{
    float _placingBaitTime = 0f;
    float _timerToChange = 0.5f;
    Transform _transform;
    
    public PlacingBaitState(Transform transform, StateMachine stateMachine) : base(stateMachine)
    {
        _transform = transform;
    }

    public override void Enter()
    {
        //Debug.Log("Entering Idle State");
        _placingBaitTime = 0f;
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Idle State");
    }

    public override void Update()
    {
        //Debug.Log("Updating Idle State");
        _placingBaitTime += Time.deltaTime;

        if (_timerToChange <= _placingBaitTime)
        {
            if(!BaitManager.Instance.TrySpawnBait(_transform.position))
                Debug.Log("Failed to spawn bait.");

            _stateMachine.ChangeState(StateType.Patrol);
        }
    }
}
