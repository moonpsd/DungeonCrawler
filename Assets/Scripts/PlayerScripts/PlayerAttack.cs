using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerAttack : NetworkBehaviour
{
    private PlayerEffects effects;

    //attack damage, range and cooldown
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private Vector2 attackRange = new Vector2(0.8f, 0.4f);
    [SerializeField] private float attackCooldown = 0.5f;

    //references
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;

    private float nextAttackTime = 0f;

    void Start()
    {
        effects = GetComponent<PlayerEffects>();
    }

    void Update()
    {
        if (!IsOwner)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= nextAttackTime)
        {
            if (effects.IsKnockback)
            {
                return;
            }

            RequestAttackRpc();

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestAttackRpc()
    {
        Attack();
    }

    private void Attack()
    {
        if (!IsServer)
            return;

        Collider2D[] enemiesHit = Physics2D.OverlapBoxAll(
            attackPoint.position,
            attackRange,
            0f,
            enemyLayer
        );

        foreach (Collider2D enemy in enemiesHit)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }

            //parte responsável por empurrar o player para trás quando ele é atingido
            EnemyEffects effects = enemy.GetComponent<EnemyEffects>();

            if (effects != null)
            {
                float direction = Mathf.Sign(
                    enemy.transform.position.x - transform.position.x
                );

                Vector2 knockbackForce = new Vector2(
                    direction * 5f,
                    2.5f
                );

                effects.ApplyKnockback(knockbackForce, 0.5f);
            }

            Debug.Log("Atingiu: " + enemy.name);
        }
    }

    private void OnDrawGizmos()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireCube(
            attackPoint.position,
            attackRange
        );
    }
}
