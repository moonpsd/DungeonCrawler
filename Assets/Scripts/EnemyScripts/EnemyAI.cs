using UnityEngine;
using Unity.Netcode;

public class EnemyAI : NetworkBehaviour
{
    private EnemyEffects effects;

    // Movement speed of the enemy
    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private float patrolTime = 2f;

    private float patrolTimer;
    private float patrolDirection = 1f;

    // Detection and attack ranges
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private LayerMask playerLayer;

    // Attack properties
    [SerializeField] private float attackDamage = 30f;
    [SerializeField] private float attackCooldown = 2f;
    private Vector2 attackRangeHitbox = new Vector2(1.5f, 0.5f);

    [SerializeField] private Transform attackPoint;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private float nextAttackTime = 0f;

    private Rigidbody2D rb;
    private Transform player;
    private float attackPointOriginalX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        effects = GetComponent<EnemyEffects>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (attackPoint != null)
        {
            attackPointOriginalX = Mathf.Abs(attackPoint.localPosition.x);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        patrolTimer = patrolTime;
    }   

    void FixedUpdate()
    {
        if (!IsServer)
            return;

        DetectPlayer();

        if (player == null)
        {
            Patrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distanceToPlayer <= attackRange)
        {
            if (effects.IsKnockback)
            {
                return;
            }
            StopMoving();

            if (Time.time >= nextAttackTime)
            {
                Attack();

                nextAttackTime = Time.time + attackCooldown;
            }
        }

        else
        {
            if (effects.IsKnockback)
            {
                return;
            }
            ChasePlayer();
        }
    }

    private void Patrol()
    {
        rb.linearVelocity = new Vector2(
            patrolDirection * moveSpeed,
            rb.linearVelocity.y
        );

        patrolTimer -= Time.fixedDeltaTime;


        if (patrolTimer <= 0f)
        {
            patrolDirection *= -1f;

            patrolTimer = patrolTime;
        }

        if (patrolDirection > 0)
        {
            FlipRight();
        }
        else
        {
            FlipLeft();
        }
    }


    private void DetectPlayer()
    {
        Collider2D[] playersDetected = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRange,
            playerLayer
        );

        if (playersDetected.Length == 0)
        {
            player = null;
            return;
        }

        Collider2D closestPlayer = null;

        float closestDistance = Mathf.Infinity;


        foreach (Collider2D playerCollider in playersDetected)
        {
            float distance = Vector2.Distance(
                    transform.position,
                    playerCollider.transform.position
                );


            if (distance < closestDistance)
            {
                closestDistance = distance;

                closestPlayer = playerCollider;
            }
        }


        if (closestPlayer != null)
        {
            player = closestPlayer.transform;
        }
    }

    private void ChasePlayer()
    {
        float direction;

        if (player.position.x > transform.position.x)
        {
            direction = 1f;

            FlipRight();
        }
        else
        {
            direction = -1f;

            FlipLeft();
        }

        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void FlipLeft()
    {
        // Vira somente o sprite
        spriteRenderer.flipX = true;

        // Move o AttackPoint para esquerda
        Vector3 attackPosition = attackPoint.localPosition;

        attackPosition.x = -attackPointOriginalX;

        attackPoint.localPosition = attackPosition;
    }

    private void FlipRight()
    {
        // Sprite normal
        spriteRenderer.flipX = false;

        // Move o AttackPoint para direita
        Vector3 attackPosition = attackPoint.localPosition;

        attackPosition.x = attackPointOriginalX;

        attackPoint.localPosition = attackPosition;
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }

    private void Attack()
    {
        Collider2D[] playersHit = Physics2D.OverlapBoxAll(
            attackPoint.position,
            attackRangeHitbox,
            0f,
            playerLayer
        );

        foreach (Collider2D player in playersHit)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }

            //parte responsável por empurrar o player para trás quando ele é atingido
            PlayerEffects effects = player.GetComponent<PlayerEffects>();

            if (effects != null)
            {
                float direction = Mathf.Sign(
                    player.transform.position.x - transform.position.x
                );

                Vector2 knockbackForce = new Vector2(
                  direction * 5f,
                  2.5f
                );

                effects.ApplyKnockback(knockbackForce, 0.5f);
            }

            Debug.Log("Atingiu: " + player.name);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );

        if (attackPoint == null)
            return;

        Gizmos.DrawWireCube(
            attackPoint.position,
            attackRangeHitbox
        );
    }
}