using UnityEngine;

public class PlayerMouseController: MonoBehaviour
{
    [Header("�ƶ�����")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private Camera mainCamera;
    private Rigidbody2D rb;

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();

        // �������û��Rigidbody2D�����һ��
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; // 2D��Ϸ��ͨ������Ҫ����
        }
    }

    void Update()
    {
        FollowMousePosition();
        RotateTowardsMouse();
    }

    void FollowMousePosition()
    {
        // ��ȡ���������ռ��λ��
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // ȷ��z����Ϊ0��2D�ռ䣩

        // �����ƶ�����
        Vector2 moveDirection = (mousePosition - transform.position).normalized;

        // �ƶ����
        rb.velocity = moveDirection * moveSpeed;

        // ����ʹ��Transform�ƶ���ȡ��ע���������У���ע���������У�
        // transform.position = Vector2.MoveTowards(transform.position, mousePosition, moveSpeed * Time.deltaTime);
    }

    void RotateTowardsMouse()
    {
        // ��ȡ���������ռ��λ��
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // ���㳯�����ķ���
        Vector2 direction = mousePosition - transform.position;

        // ����Ƕ�
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // ƽ����ת�������
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}