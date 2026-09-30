using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerEffects : NetworkBehaviour
{
    private Rigidbody2D rb;

    // Estados
    public bool IsKnockback { get; private set; }

    private Coroutine knockbackCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyKnockback(Vector2 force, float duration)
    {
        if (!IsServer)
            return;

        ApplyKnockbackRpc(force, duration);
    }

    [Rpc(SendTo.Owner)]
    private void ApplyKnockbackRpc(Vector2 force, float duration)
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(KnockbackCoroutine(force, duration));
    }

    private IEnumerator KnockbackCoroutine(Vector2 force, float duration)
    {
        IsKnockback = true;

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = force;

        yield return new WaitForSeconds(duration);

        IsKnockback = false;

        knockbackCoroutine = null;
    }
}