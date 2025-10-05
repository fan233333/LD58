using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 2f;
    public float moveForce = 10f;
    public float maxRotationSpeed = 90f;
    public Sprite right;
    public Sprite left;
    public Sprite up;
    public Sprite down;
    public Sprite ur;
    public Sprite ul;
    public Sprite dr;
    public Sprite dl;
    public SpriteRenderer spriteRenderer;
    public Transform rotateObject;
    public float minMovementThreshold = 0.1f; // 最小移动阈值
    public static float playerangle = 0;

    private Rigidbody2D rb;
    private Vector2 movement;
    
    private Quaternion targetRotation;
    private Vector2 lastValidDirection = Vector2.right;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 设置线性阻尼 - 模拟空气/流体阻力
        rb.drag = 1f; // 值越大，停止越快

        // 设置角速度阻尼
        rb.angularDrag = 0.5f;
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

    }

    void FixedUpdate()
    {
        // 移动角色
        //rb.MovePosition(rb.position + movement * maxSpeed * Time.fixedDeltaTime);
        rb.AddForce(movement * moveForce);

        // 限制最大速度
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        if (movement != Vector2.zero)
        {
            // 直接计算看向方向的角度
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            playerangle = angle;

            // 创建目标旋转
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // 平滑旋转
            rotateObject.rotation = targetRotation;//Quaternion.RotateTowards(rotateObject.rotation, targetRotation, maxRotationSpeed * Time.deltaTime); ;

            float z = rotateObject.rotation.z;

            //Debug.Log(z);
            if(angle == 0)
            {
                spriteRenderer.sprite = right;
            }else if(angle == 180)
            {
                spriteRenderer.sprite = left;
            }else if(angle == 90)
            {
                spriteRenderer.sprite = up;
            }else if(angle == -90)
            {
                spriteRenderer.sprite = down;
            }else if(angle == 45)
            {
                spriteRenderer.sprite = ur;
            }
            else if(angle == -45)
            {
                spriteRenderer.sprite = dr;
            }
            else if(angle == 135)
            {
                spriteRenderer.sprite = ul;
            }
            else if(angle == -135)
            {
                spriteRenderer.sprite = dl;
            }
        }
    }
}
