using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // referencia para o alvo que a câmera seguirá
    [SerializeField] private Transform target;

    // espaçamento entre a câmera e o alvo
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);
    [SerializeField] private float smoothSpeed = 5f;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}