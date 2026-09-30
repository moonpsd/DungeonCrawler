using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    // Variáveis de vida
    [SerializeField] private float maxHealth = 100f;

    private NetworkVariable<float> currentHealth =
        new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private Slider healthSlider;

    public override void OnNetworkSpawn()
    {
        // O servidor controla o valor real da vida
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        currentHealth.OnValueChanged += OnHealthChanged;

        if (IsOwner)
        {
            FindHealthSlider();
            UpdateHealthBar();
        }
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void FindHealthSlider()
    {
        GameObject sliderObject = GameObject.FindGameObjectWithTag("HealthSlider");

        if (sliderObject == null)
        {
            Debug.LogWarning(
                "Não encontrei um objeto com a Tag HealthSlider."
            );

            return;
        }

        healthSlider = sliderObject.GetComponent<Slider>();

        if (healthSlider == null)
        {
            Debug.LogWarning(
                "O objeto HealthSlider não possui componente Slider."
            );
        }
    }

    private void OnHealthChanged(
        float previousHealth,
        float newHealth
    )
    {
        if (IsOwner)
        {
            UpdateHealthBar();
        }
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

        Debug.Log("Player " + OwnerClientId + " Health: " + currentHealth.Value);

        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        // Somente servidor altera vida
        if (!IsServer)
            return;

        currentHealth.Value += amount;

        if (currentHealth.Value > maxHealth)
        {
            currentHealth.Value = maxHealth;
        }

        Debug.Log("Player " + OwnerClientId + " Health: " + currentHealth.Value);
    }

    private void UpdateHealthBar()
    {
        if (healthSlider == null)
            return;

        healthSlider.value = currentHealth.Value / maxHealth;
    }

    private void Die()
    {
        Debug.Log("Player " + OwnerClientId + " died!");
    }
}