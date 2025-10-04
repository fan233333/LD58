using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 2f;
    public float maxRotationSpeed = 90f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Quaternion targetRotation;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetRotation = transform.rotation;
    }

    void Update()
    {
        // ��ȡ����
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D �� ���Ҽ�ͷ
        movement.y = Input.GetAxisRaw("Vertical");   // W/S �� ���¼�ͷ

        // ��׼����������ֹб���ƶ�����
        movement = movement.normalized;

        if (movement != Vector2.zero)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // ʹ�ù̶���ת�ٶ���ת��Ŀ�귽��
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotationSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        // �ƶ���ɫ
        rb.MovePosition(rb.position + movement * maxSpeed * Time.fixedDeltaTime);
    }
}
