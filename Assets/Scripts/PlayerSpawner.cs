using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform spawnPoint;

    private IEnumerator Start()
    {
        // Espera 1 frame para o RoomGenerator terminar
        // de gerar as plataformas e posicionar o PlayerSpawn
        yield return null;

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (player == null)
        {
            Debug.LogError("Player não foi configurado no PlayerSpawner.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point não foi configurado no PlayerSpawner.");
            return;
        }

        // Coloca o Player no ponto inicial da sala
        player.position = spawnPoint.position;
    }
}