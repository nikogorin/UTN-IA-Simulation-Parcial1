using System.Collections.Generic;
using UnityEngine;

public class SteeringAgent : Agent
{
    public enum SteeringMode
    {
        Seek, Flee, Arrive, Pursuit, Evade, Flocking
    }

    //[Header("References")]
    //[Tooltip("The target agent that this agent will interact with.")]
    //[SerializeField] private Agent targetAgent;

    [Header("Stats")]
    [Tooltip("Maximum speed the agent can move.")]
    [SerializeField] private float speed = 5f;
    [Tooltip("Maximum acceleration the agent can apply to change its velocity.")]
    [SerializeField] private float maxAcceleration = 2f;

    [Header("Target Settings")]
    [SerializeField] private LayerMask hunterLayer;
    [SerializeField] private float hunterDetectionRadius = 25f;

    [Header("Flocking Settings")]
    [SerializeField] private LayerMask flockLayer;
    [SerializeField] private float flockDetectionRadius = 15f;

    [Header("Steering Settings")]
    //[Tooltip("Layer mask to filter which objects the agent can detect.")]
    //[SerializeField] private LayerMask layerMask;
    //[Tooltip("Radius within which the agent will detect other agents.")]
    //[SerializeField] private float detectionRadius = 5f;
    //[Tooltip("Distance at which the agent will start to flee from the target.")]
    //[SerializeField] private float fleeDistance = 3f;
    [Tooltip("Distance at which the agent will start to slow down when arriving at the target.")]
    [SerializeField] private float slowingDistance = 2f;
    //[Tooltip("Current steering behavior mode of the agent.")]
    //[SerializeField] private SteeringMode currentSteering;
    [Tooltip("Distance at which the agent will maintain separation from other agents.")]
    [SerializeField] private float separationDistance = 2f;
    [Tooltip("Distance at which the agent will align its velocity with other agents.")]
    [SerializeField] private float alignmentDistance = 3f;
    [Tooltip("Distance at which the agent will move towards the center of mass of nearby agents.")]
    [SerializeField] private float cohesionDistance = 3f;
    [Tooltip("Weight applied to the separation steering behavior.")]
    [SerializeField, Range(0f, 3f)] private float separationWeight = 3f;
    [Tooltip("Weight applied to the alignment steering behavior.")]
    [SerializeField, Range(0f, 3f)] private float alignmentWeight = 2f;
    [Tooltip("Weight applied to the cohesion steering behavior.")]
    [SerializeField, Range(0f, 3f)] private float cohesionWeight = 1f;

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;

    private Transform _currentTransform;
    private readonly List<Agent> _flockAgents = new();
    private readonly List<Agent> _hunterAgents = new();
    //private static List<Agent> _allAgents = new();

    private void Awake()
    {
        //_allAgents.Add(this);
        //Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        //_velocity = randomDirection.normalized * speed;
    }

    void Start()
    {
        _currentTransform = transform.parent != null ? transform.parent.transform : transform;
        //GetComponent<SphereCollider>().radius = detectionRadius;
        //DetectFlock();
        //DetectHunter();
    }


    void Update()
    {
        DetectFlock();
        DetectHunter();
        //DetectInterest();

        //FleeInRange();
        _velocity += SteeringVector();
        _velocity = Vector3.ClampMagnitude(_velocity, speed);
        //Debug.Log($"Velocity: {_velocity.magnitude}");

        _currentTransform.position += _velocity * Time.deltaTime;
        _currentTransform.forward = _velocity;

        _currentTransform.position = Bounds.Instance.OutOfBounds(_currentTransform.position);
    }

    //private void FleeInRange()
    //{
    //    var distance = (targetAgent.transform.position - _currentTransform.position).magnitude;

    //    if (distance <= fleeDistance)
    //    {
    //        Flee(targetAgent.transform.position);
    //        _currentTransform.position += Time.deltaTime * _velocity;
    //        _currentTransform.forward = _velocity;
    //    }
    //}

    private Vector3 SteeringVector()
    {
        if(_hunterAgents.Count > 0)
        {
            return Flee(_hunterAgents[0].transform.position);
        }

        return Flocking();
        //switch (currentSteering)
        //{
        //    case SteeringMode.Seek:
        //        return Seek(_hunterAgents[0].transform.position);
        //    case SteeringMode.Flee:
        //        return Flee(_hunterAgents[0].transform.position);
        //    case SteeringMode.Arrive:
        //        return Arrive(_hunterAgents[0].transform.position);
        //    case SteeringMode.Pursuit:
        //        return Persuit(_hunterAgents[0]);
        //    case SteeringMode.Evade:
        //        return Evade(_hunterAgents[0]);
        //    case SteeringMode.Flocking:
        //        return Flocking();
        //    default:
        //        return Vector3.zero;
        //}
    }

    private Vector3 CalculateSeparation(IEnumerable<Agent> agents, float distance)
    {
        Vector3 desired = default;
        int count = 0;

        foreach (var agent in agents)
        {
            if (Vector3.Distance(agent.transform.position, _currentTransform.position) <= distance)
            {
                desired += _currentTransform.position - agent.transform.position;
                count++;
            }
        }
        if (count == 0)
            return Vector3.zero;

        desired /= count;

        return CalculateSteering(desired.normalized * speed);
    }

    private Vector3 CalculateAlignment(IEnumerable<Agent> agents, float distance)
    {
        Vector3 desired = default;
        int count = 0;
        foreach (var agent in agents)
        {
            if (Vector3.Distance(agent.transform.position, _currentTransform.position) <= distance)
            {
                desired += agent.Velocity;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        desired /= count;

        return CalculateSteering(desired.normalized * speed);
    }

    private Vector3 CalculateCohesion(IEnumerable<Agent> agents, float distance)
    {
        Vector3 desired = default;
        int count = 0;
        foreach (var agent in agents)
        {
            if (Vector3.Distance(agent.transform.position, _currentTransform.position) <= distance)
            {
                desired += agent.transform.position;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        desired /= count;

        return Seek(desired);
    }

    private Vector3 CalculateSteering(Vector3 desiredVelocity)
    {
        // Steering = Desired Velocity - Current Velocity
        Vector3 steering = desiredVelocity - _velocity;

        steering = Vector3.ClampMagnitude(steering, maxAcceleration * Time.deltaTime);

        return steering;
    }

    private Vector3 DesiredVelocity(Vector3 targetVector)
    {
        Vector3 direction = (targetVector - _currentTransform.position).normalized;

        Vector3 desiredVelocity = direction * speed;

        return desiredVelocity;
    }

    private Vector3 Seek(Vector3 targetVector)
    {
        Vector3 desiredVelocity = DesiredVelocity(targetVector);

        return CalculateSteering(desiredVelocity);
    }

    private Vector3 Flee(Vector3 targetVector)
    {
        Vector3 desiredVelocity = DesiredVelocity(targetVector);

        return CalculateSteering(-desiredVelocity);
    }

    private Vector3 Arrive(Vector3 targetVector)
    {
        Vector3 direction = (targetVector - _currentTransform.position).normalized;
        float distance = (targetVector - _currentTransform.position).magnitude;

        if (distance < 0.1f)
        {
            return CalculateSteering(Vector3.zero);
        }

        float targetSpeed = speed * (distance / slowingDistance);

        float desiredSpeed = Mathf.Min(targetSpeed, speed);

        Vector3 desiredVelocity = direction * desiredSpeed;
        Vector3 steering = CalculateSteering(desiredVelocity);

        return steering;
    }

    private Vector3 Persuit(Agent target)
    {
        Vector3 futurePosition = CalculateFuturePosition(target);

        return Seek(futurePosition);
    }

    private Vector3 Evade(Agent target)
    {
        Vector3 futurePosition = CalculateFuturePosition(target);

        return Flee(futurePosition);
    }

    private Vector3 Flocking()
    {
        return CalculateSeparation(_flockAgents, separationDistance) * separationWeight
            + CalculateAlignment(_flockAgents, alignmentDistance) * alignmentWeight
            + CalculateCohesion(_flockAgents, cohesionDistance) * cohesionWeight;
    }

    private Vector3 CalculateFuturePosition(Agent target)
    {
        float distance = (target.transform.position - _currentTransform.position).magnitude;

        var timePrediction = distance / (speed + target.Velocity.magnitude);

        Vector3 futurePosition = target.transform.position + (target.Velocity * timePrediction);

        return futurePosition;
    }

    private void DetectFlock()
    {
        _flockAgents.Clear();
        Collider[] flockColliders = Physics.OverlapSphere(_currentTransform.position, flockDetectionRadius, flockLayer); // No need to use NonAlloc version since we are not concerned about performance here
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
        _hunterAgents.Clear();
        Collider[] hunterColliders = Physics.OverlapSphere(_currentTransform.position, hunterDetectionRadius, hunterLayer); // No need to use NonAlloc version since we are not concerned about performance here
        foreach (var collider in hunterColliders)
        {
            Agent agent = collider.GetComponent<Agent>();
            if (agent != null && agent != this)
            {
                _hunterAgents.Add(agent);
            }
        }
        Debug.Log($"Detected {_hunterAgents.Count} hunter agents.");
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.lightGreen;
        Gizmos.DrawWireSphere(transform.position, flockDetectionRadius);

        Gizmos.color = Color.lightPink;
        Gizmos.DrawWireSphere(transform.position, hunterDetectionRadius);
    }
}
