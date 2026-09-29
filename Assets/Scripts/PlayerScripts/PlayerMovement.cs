using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerEffects effects;

    // Movement parameters
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;

    // Ground check parameters
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // References

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform attackPoint;

    private float direction;
    private Rigidbody2D rb;
    private bool isGrounded;

    private float attackPointOriginalX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        effects = GetComponent<PlayerEffects>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        direction = 0f;

        if (Keyboard.current.aKey.isPressed)
        {
            direction = -1f;

            FlipLeft();
        }

        if (Keyboard.current.dKey.isPressed)
        {
            direction = 1f;

            FlipRight();
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
        }
    }

    void FixedUpdate()
    {
        if (effects.IsKnockback)
        {
            return;
        }
        rb.linearVelocity = new Vector2(
            direction * speed,
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

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}