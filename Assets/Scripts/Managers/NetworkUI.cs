using UnityEngine;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
    private void OnGUI()
    {
        if (NetworkManager.Singleton == null)
        {
            GUI.Label(
                new Rect(10, 10, 400, 30),
                "NetworkManager nao encontrado!"
            );

            return;
        }

        GUILayout.BeginArea(
            new Rect(10, 10, 300, 300)
        );
        if (!NetworkManager.Singleton.IsClient &&
            !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("HOST"))
            {
                NetworkManager.Singleton.StartHost();
            }

            if (GUILayout.Button("CLIENT"))
            {
                NetworkManager.Singleton.StartClient();
            }
        }
        else
        {
            if (NetworkManager.Singleton.IsHost)
            {
                GUILayout.Label("HOST conectado");
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                GUILayout.Label("CLIENT conectado");
            }
        }

        GUILayout.EndArea();
    }
}