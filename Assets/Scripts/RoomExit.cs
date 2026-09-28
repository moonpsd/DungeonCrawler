using UnityEngine;
using UnityEngine.InputSystem;

public class RoomExit : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]
    [SerializeField] private GameObject interactionText;


    [Header("UI Position")]
    [SerializeField]
    private Vector3 textOffset =
        new Vector3(
            0f,
            1.5f,
            0f
        );


    // =========================================================
    // ESTADO
    // =========================================================

    private bool playerNearby = false;
    private bool completed = false;

    private Camera mainCamera;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        mainCamera =
            Camera.main;


        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Mantém o texto em cima da porta
        if (
            playerNearby
            &&
            interactionText != null
        )
        {
            UpdateTextPosition();
        }


        // Não está perto
        if (!playerNearby)
        {
            return;
        }


        // Já utilizou essa saída
        if (completed)
        {
            return;
        }


        // =====================================================
        // INTERAÇÃO
        // =====================================================

        if (
            Keyboard.current != null
            &&
            Keyboard.current.eKey.wasPressedThisFrame
        )
        {
            CompleteRoom();
        }
    }


    // =========================================================
    // POSICIONAR TEXTO
    // =========================================================

    private void UpdateTextPosition()
    {
        if (mainCamera == null)
        {
            return;
        }


        Vector3 worldPosition =
            transform.position
            + textOffset;


        Vector3 screenPosition =
            mainCamera.WorldToScreenPoint(
                worldPosition
            );


        interactionText.transform.position =
            screenPosition;
    }


    // =========================================================
    // PLAYER ENTROU
    // =========================================================

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;


            if (interactionText != null)
            {
                interactionText.SetActive(true);

                UpdateTextPosition();
            }
        }
    }


    // =========================================================
    // PLAYER SAIU
    // =========================================================

    private void OnTriggerExit2D(
        Collider2D other
    )
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;


            if (interactionText != null)
            {
                interactionText.SetActive(false);
            }
        }
    }


    // =========================================================
    // COMPLETAR SALA
    // =========================================================

    private void CompleteRoom()
    {
        completed = true;

        playerNearby = false;


        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }


        Debug.Log(
            "Sala concluída!"
        );


        // Avisa o DungeonManager
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.NextRoom();
        }
        else
        {
            Debug.LogError(
                "DungeonManager não encontrado!"
            );
        }
    }


    // =========================================================
    // RESETAR PORTA
    // =========================================================

    public void ResetExit()
    {
        completed = false;

        playerNearby = false;


        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }
}