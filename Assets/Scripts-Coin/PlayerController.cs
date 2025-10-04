using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 2f;
    public float maxRotationSpeed = 90f;
    public Sprite right;
    public Sprite left;
    public SpriteRenderer spriteRenderer;
    public Transform rotateObject;
    public float minMovementThreshold = 0.1f; // 最小移动阈值

    private Rigidbody2D rb;
    private Vector2 movement;
    
    private Quaternion targetRotation;
    private Vector2 lastValidDirection = Vector2.right;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetRotation = rotateObject.rotation;
        
    }

    void Update()
    {
        // 获取输入
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D 或 左右箭头
        movement.y = Input.GetAxisRaw("Vertical");   // W/S 或 上下箭头

        //if (movement != Vector2.zero)
        //{
        //   float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
        //    targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //}

        // 使用固定旋转速度逐步转向目标方向
        //rotateObject.localRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotationSpeed * Time.deltaTime);

        //rotateObject.localRotation = targetRotation;

        if(Input.GetKeyDown(KeyCode.A))
        {
            spriteRenderer.sprite = left;
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            spriteRenderer.sprite = right;
        }
    }

    void FixedUpdate()
    {
        // 移动角色
        rb.MovePosition(rb.position + movement * maxSpeed * Time.fixedDeltaTime);


        if (movement != Vector2.zero)
        {
            // 直接计算看向方向的角度
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;

            // 创建目标旋转
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // 平滑旋转
            rotateObject.rotation = targetRotation;//Quaternion.Slerp(rotateObject.rotation, targetRotation, maxRotationSpeed * Time.fixedDeltaTime);
        }
    }
}
