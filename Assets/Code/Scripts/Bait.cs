using System;
using UnityEngine;

public class Bait : MonoBehaviour
{
    public event Action<Bait> Destroyed;
    public void Consume()
    {
        Destroyed?.Invoke(this);
        Destroy(gameObject);
    }
}
