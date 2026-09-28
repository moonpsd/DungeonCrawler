using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    //attack damage, range and cooldown
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private Vector2 attackRange = new Vector2(0.8f, 0.4f);
    [SerializeField] private float attackCooldown = 0.5f;

    //references
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;

    private float nextAttackTime = 0f;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= nextAttackTime)
        {
            Attack();

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void Attack()
    {
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
