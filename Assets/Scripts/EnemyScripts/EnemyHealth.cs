using UnityEngine;
using Unity.Netcode;

public class EnemyHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        currentHealth.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float previousHealth, float newHealth)
    {
        Debug.Log("Enemy Health mudou: " + previousHealth + " -> " + newHealth);
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer)
            return;

        currentHealth.Value -= damage;

        if (currentHealth.Value < 0)
        {
            currentHealth.Value = 0;
        }

        Debug.Log("Health: " + currentHealth.Value);

        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (!IsServer)
            return;

        currentHealth.Value += amount;

        if (currentHealth.Value > maxHealth)
        {
            currentHealth.Value = maxHealth;
        }

        Debug.Log("Health: " + currentHealth.Value);
    }

    private void Die()
    {
        if (!IsServer)
            return;

        Debug.Log("Enemy died!");

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth.Value;
    }


    public float GetMaxHealth()
    {
        return maxHealth;
    }
}