using System.Collections.Generic;
using UnityEngine;

public enum PreyState
{
    Flocking,
    GoingToBait,
    Eating,
    Evading,
    Dead
}

public class PreyAgent : SteeringAgent
{
    [Header("Flocking Settings")]
    [Tooltip("Layer mask to filter which objects are considered part of the flock.")]
    [SerializeField] private LayerMask flockLayer;
    [Tooltip("Radius within which the agent will detect other flock agents.")]
    [SerializeField] private float flockDetectionRadius = 10;

    [Header("Target Settings")]
    [Tooltip("Layer mask to filter which objects are considered hunters.")]
    [SerializeField] private LayerMask hunterLayer;
    [Tooltip("Radius within which the agent will detect hunter agents.")]
    [SerializeField] private float hunterDetectionRadius = 15f;
    [Tooltip("Layer mask to filter which objects are considered bait.")]
    [SerializeField] private LayerMask baitLayer;
    [Tooltip("Radius within which the agent will detect bait.")]
    [SerializeField] private float baitDetectionRadius = 15f;

    [Header("Eat Settings")]
    [SerializeField, Range(0, 1)] private float eatingDistance = 0.7f;
    [SerializeField, Min(0)] private float eatingTime = 2f;

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = false;

    private readonly List<Agent> _flockAgents = new();
    private Agent _hunterAgent;
    private Bait _targetBait;
    private PreyState _currentState;
    private HealthAgent _healthAgent;
    private float _currentEatingTime;
    private PreyStateUI _stateUI;

    protected override void Awake()
    {
        base.Awake();
        _stateUI = GetComponent<PreyStateUI>();
        _healthAgent = GetComponent<HealthAgent>();
        
        _currentEatingTime = 0;
    }

    private void Update()
    {
        DetectFlock();
        DetectHunter();
        DetectBait();

        UpdateState();
        UpdateEatingTime();

        Vector3 steering = CalculateSteeringBehavior();
        ApplySteering(steering);

        Move();
    }

    private void UpdateState()
    {
        if((_currentState == PreyState.GoingToBait && _targetBait != null && Vector3.Distance(transform.position, _targetBait.transform.position) <= eatingDistance) 
            || (_currentEatingTime != 0))
        {
            _currentState = PreyState.Eating;
            Debug.Log($"{PreyState.Eating}");
        }
        else if (_targetBait != null && _currentEatingTime == 0)
        {
            _currentState = PreyState.GoingToBait;
            Debug.Log($"{PreyState.GoingToBait}");
        }
        else if (_hunterAgent != null)
        {
            _currentState = PreyState.Evading;
        }
        else
            _currentState = PreyState.Flocking;

        _stateUI.SetState(_currentState);
    }

    private void UpdateEatingTime()
    {
        if (_currentState != PreyState.Eating)
            return;

        _currentEatingTime += Time.deltaTime;
        if(_currentEatingTime > eatingTime)
        {
            _targetBait.Consume();
            //_wasEating = true;
            _currentEatingTime = 0;
            _currentState = PreyState.Flocking;
        }
    }

    private void DetectFlock()
    {
        _flockAgents.Clear();
        Collider[] flockColliders = Physics.OverlapSphere(transform.position, flockDetectionRadius, flockLayer); // No need to use NonAlloc version since we are not concerned about performance here
        foreach (var collider in flockColliders)
        {
            Agent agent = collider.GetComponent<Agent>();
            if (agent != null && agent != this)
            {
                _flockAgents.Add(agent);
            }
        }
        //Debug.Log($"Detected {_flockAgents.Count} flock agents.");
    }

    private void DetectHunter()
    {
        Collider[] hunterColliders = Physics.OverlapSphere(transform.position, hunterDetectionRadius, hunterLayer); // No need to use NonAlloc version since we are not concerned about performance here
        foreach (var collider in hunterColliders)
        {
            Agent agent = collider.GetComponent<Agent>();
            if (agent != null && agent != this)
            {
                _hunterAgent = agent;
                //Debug.Log($"Detected hunter agent at position: {_hunterAgent.transform.position}");
                return; // Exit after finding the first hunter
            }
        }
        _hunterAgent = null; // No hunter detected
    }

    private void DetectBait()
    {
        if (_targetBait != null)
            return;

        Collider[] baitColliders = Physics.OverlapSphere(transform.position, baitDetectionRadius, baitLayer); // No need to use NonAlloc version since we are not concerned about performance here
        Debug.Log($"Bait: {baitColliders.Length}");
        foreach (var collider in baitColliders)
        {
            Bait bait = collider.GetComponent<Bait>();
            if (bait != null && bait.TryAssignAgent(this))
            {
                _targetBait = bait;
                Debug.Log($"Detected bait at position: {_targetBait.transform.position}");
                return; // Exit after finding the first bait
            }
        }
    }

    private Vector3 CalculateSteeringBehavior()
    {
        switch(_currentState)
        {
            case PreyState.Flocking:
                StartMoving();
                return Flocking(_flockAgents);
            case PreyState.GoingToBait:
                return Arrive(_targetBait.transform.position);
            case PreyState.Eating:
                StopMoving();
                return Vector3.zero;
            case PreyState.Evading:
                return Evade(_hunterAgent);
            default:
                return Vector3.zero;
        }
    }

    private void ReleaseBait()
    {
        if (_targetBait == null)
            return;

        _targetBait.ReleaseAgent(this);
        _targetBait = null;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.lightGreen;
        Gizmos.DrawWireSphere(transform.position, flockDetectionRadius);

        Gizmos.color = Color.lightPink;
        Gizmos.DrawWireSphere(transform.position, hunterDetectionRadius);

        Gizmos.color = Color.lightBlue;
        Gizmos.DrawWireSphere(transform.position, baitDetectionRadius);
    }
}
