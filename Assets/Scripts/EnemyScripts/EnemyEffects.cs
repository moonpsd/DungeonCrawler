using System.Collections;
using UnityEngine;

public class EnemyEffects : MonoBehaviour
{
    private Rigidbody2D rb;

    // Estados
    public bool IsKnockback { get; private set; }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyKnockback(Vector2 force, float duration)
    {
        StopCoroutine(nameof(KnockbackCoroutine));
        StartCoroutine(KnockbackCoroutine(force, duration));
    }

    private IEnumerator KnockbackCoroutine(Vector2 force, float duration)
    {
        IsKnockback = true;

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = force;

        yield return new WaitForSeconds(duration);

        IsKnockback = false;
    }
}