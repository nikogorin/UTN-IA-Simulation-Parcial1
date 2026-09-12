using System;
using System.Collections.Generic;
using UnityEngine;

public class PreyAgent : SteeringAgent
{
    [Header("Life Stats")]
    [SerializeField] private float _maxHealth = 5f;

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
    private PreyStateUI _stateUI;
    private StateMachine _stateMachine;
    private bool _gathered;

    public float CurrentHealth { get; private set; }
    public PreyState CurrentState { get; private set; }
    public bool IsDead => CurrentHealth <= 0f;
    public bool IsBaitAssigned => _targetBait != null;
    public bool IsHunterDetected => _hunterAgent != null;
    public bool IsCloseToBait => _targetBait != null ? Vector3.Distance(transform.position, _targetBait.transform.position) <= eatingDistance : false;
    public float EatingTime => eatingTime;
    public bool IsGathered => _gathered;
    public bool CanTakeBait => CurrentState == PreyState.Flocking;

    protected override void Awake()
    {
        base.Awake();
        _stateUI = GetComponent<PreyStateUI>();
        _stateMachine = new StateMachine();
        _stateMachine.OnStateChanged += _stateUI.SetState;
        _stateMachine.OnStateChanged += SetStateChange;

        FlockingState flockingState = new(_stateMachine, this);
        EvadingState evadingState = new(_stateMachine, this);
        GoingToBaitState goingToBaitState = new(_stateMachine, this);
        EatingState eatingState = new(_stateMachine, this);
        DeadState deadState = new(_stateMachine, this);

        _stateMachine.RegisterState(PreyState.Flocking, flockingState);
        _stateMachine.RegisterState(PreyState.Evading, evadingState);
        _stateMachine.RegisterState(PreyState.GoingToBait, goingToBaitState);
        _stateMachine.RegisterState(PreyState.Eating, eatingState);
        _stateMachine.RegisterState(PreyState.Dead, deadState);

        _stateMachine.ChangeState(PreyState.Flocking);

        CurrentHealth = _maxHealth;
    }

    private void Update()
    {
        DetectFlock();
        DetectHunter();
        DetectBait();

        _stateMachine.Update();
    }

    private void OnDestroy()
    {
        if (_stateUI != null)
            _stateMachine.OnStateChanged -= _stateUI.SetState;

        _stateMachine.OnStateChanged -= SetStateChange;
    }

    public Vector3 GetFlockingSteering()
    {
        return Flocking(_flockAgents);
    }

    public Vector3 GetArriveSteering()
    {
        return Arrive(_targetBait.transform.position);
    }

    public Vector3 GetEvadeSteering()
    {
        return Evade(_hunterAgent);
    }

    public void ConsumeTargetBait()
    {
        if (_targetBait == null)
            return;

        _targetBait.Consume();
        _targetBait = null;
    }

    private void SetStateChange(Enum state)
    {
        CurrentState = (PreyState)state;
    }

    private void DetectFlock()
    {
        _flockAgents.Clear();
        Collider[] flockColliders = Physics.OverlapSphere(transform.position, flockDetectionRadius, flockLayer); // No need to use NonAlloc version since we are not concerned about performance here
        foreach (var collider in flockColliders)
        {
            PreyAgent prey = collider.GetComponent<PreyAgent>();
            if (prey != null && prey != this && prey.CurrentState == PreyState.Flocking)
            {
                _flockAgents.Add(prey);
            }
        }
        //Debug.Log($"Detected {_flockAgents.Count} flock agents.");
    }

    private void DetectHunter()
    {
        Collider[] hunterColliders = Physics.OverlapSphere(transform.position, hunterDetectionRadius, hunterLayer); // No need to use NonAlloc version since we are not concerned about performance here
        foreach (var collider in hunterColliders)
        {
            HunterAgent hunterAgent = collider.GetComponent<HunterAgent>();
            if (hunterAgent != null && hunterAgent != this)
            {
                _hunterAgent = hunterAgent;
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

        if (!CanTakeBait)
            return;

        Collider[] baitColliders = Physics.OverlapSphere(transform.position, baitDetectionRadius, baitLayer); // No need to use NonAlloc version since we are not concerned about performance here
        //Debug.Log($"Bait: {baitColliders.Length}");
        foreach (var collider in baitColliders)
        {
            Bait bait = collider.GetComponent<Bait>();
            if (bait != null && bait.TryAssignAgent(this))
            {
                _targetBait = bait;
                //Debug.Log($"Detected bait at position: {_targetBait.transform.position}");
                return; // Exit after finding the first bait
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
            return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, _maxHealth);
    }

    public void ReleaseBait()
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
