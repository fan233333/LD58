using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("ƽ����������")]
    public float smoothTime = 0.3f;
    public Vector3 offset = new Vector3(0, 0, -10);

    private Transform player;
    private Vector3 velocity = Vector3.zero;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (player != null)
        {
            transform.position = player.position + offset;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // ʹ��SmoothDampʵ�ָ�ƽ���ĸ���
        Vector3 targetPosition = player.position + offset;
        targetPosition.z = -10;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}