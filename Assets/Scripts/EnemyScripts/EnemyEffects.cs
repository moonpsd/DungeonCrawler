using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class EnemyEffects : NetworkBehaviour
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
        if (!IsServer)
            return;

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