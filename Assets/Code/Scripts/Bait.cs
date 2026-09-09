using System;
using UnityEngine;

public class Bait : MonoBehaviour
{
    public Agent AssignedAgent { get; private set; }
    public event Action<Bait> Destroyed;

    public void Consume()
    {
        Destroyed?.Invoke(this);
        Destroy(gameObject);
    }

    public bool TryAssignAgent(Agent agent)
    {
        if (AssignedAgent != null)
            return false;

        AssignedAgent = agent;
        return true;
    }

    public void ReleaseAgent(Agent agent)
    {
        if(AssignedAgent == agent)
            AssignedAgent = null;
    }
}
