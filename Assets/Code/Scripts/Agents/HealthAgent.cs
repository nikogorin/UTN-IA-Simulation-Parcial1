using UnityEngine;

public class HealthAgent : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 80f;

    public float CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0f;

    private void Awake()
    {
        CurrentHealth = _maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) 
            return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, _maxHealth);
        
    }
}