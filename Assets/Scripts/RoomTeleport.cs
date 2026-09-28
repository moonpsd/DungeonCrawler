using System.Collections;
using UnityEngine;

public class RoomTeleport : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerSpawn;


    // =========================================================
    // SPAWN INICIAL
    // =========================================================

    private IEnumerator Start()
    {
        // Espera o RoomGenerator criar a primeira sala
        yield return null;

        TeleportNow();
    }


    // =========================================================
    // TELEPORTE ENTRE SALAS
    // =========================================================

    public void TeleportPlayer()
    {
        StartCoroutine(TeleportRoutine());
    }


    private IEnumerator TeleportRoutine()
    {
        // Espera a nova sala ser criada
        yield return null;

        TeleportNow();
    }


    // =========================================================
    // FAZER TELEPORTE
    // =========================================================

    private void TeleportNow()
    {
        if (player == null)
        {
            Debug.LogError(
                "Player não configurado no RoomTeleport!"
            );

            return;
        }


        if (playerSpawn == null)
        {
            Debug.LogError(
                "PlayerSpawn não configurado no RoomTeleport!"
            );

            return;
        }


        Rigidbody2D rb =
            player.GetComponent<Rigidbody2D>();


        if (rb != null)
        {
            // Para completamente o movimento
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // Para teleporte queremos definir diretamente
            // a posição física do Rigidbody.
            rb.position = playerSpawn.position;
        }


        player.position =
            playerSpawn.position;


        Physics2D.SyncTransforms();


        Debug.Log(
            "PLAYER TELEPORTADO PARA: "
            + playerSpawn.position
        );
    }
}