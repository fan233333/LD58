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
        // 获取输入
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D 或 左右箭头
        movement.y = Input.GetAxisRaw("Vertical");   // W/S 或 上下箭头

        // 标准化向量，防止斜向移动更快
        movement = movement.normalized;

        if (movement != Vector2.zero)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // 使用固定旋转速度逐步转向目标方向
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotationSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        // 移动角色
        rb.MovePosition(rb.position + movement * maxSpeed * Time.fixedDeltaTime);
    }
}
