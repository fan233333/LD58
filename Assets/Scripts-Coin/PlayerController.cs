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
    public float maxMass = 10f;
    public float baseBounceForce = 5f;
    public float velocityMultiplier = 1.5f;
    public float maxBounceForce = 20f;
    public Transform InitParent;

    public static float playerangle = 0;

    private Rigidbody2D rb;
    private Vector2 movement;
    private float totalMass = 0;
    private Vector2 lastPosition;
    private float currentSpeed;
    private bool isColliding = false;
    private bool isInit = false;


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
        totalMass = 0;
        lastPosition = transform.position;
        isColliding = false;
        isInit = false;

    }

    void Update()
    { 
        if(!isInit && InitParent.childCount > 0)
        {
            int index = Random.Range(0, InitParent.childCount);
            Vector3 InitPos = InitParent.GetChild(index).position;
            transform.position = InitPos;
            isInit = true;
        }
        // 计算当前速度
        currentSpeed = rb.velocity.magnitude;
        lastPosition = transform.position;

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
        totalMass = ItemStatistics.Instance.GetTotalMass();
        if(totalMass <= 0)
        {
            totalMass = 1;
        }else if(totalMass > maxMass)
        {
            totalMass = maxMass;
        }

        if (!isColliding)
        {
            // 移动角色
            //rb.MovePosition(rb.position + movement * maxSpeed * Time.fixedDeltaTime);
            rb.AddForce(movement * moveForce * (1 / totalMass));

            // 限制最大速度
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }


        if (movement != Vector2.zero)
        {
            // 直接计算看向方向的角度
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            playerangle = angle;
            //Debug.Log(playerangle);

            if(angle == 0)
            {
                Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.up);
                rotateObject.rotation = targetRotation;
            }else if(angle == 180)
            {
                Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.up);
                rotateObject.rotation = targetRotation;
            }
            else
            {
                // 创建目标旋转
                Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);

                // 平滑旋转
                rotateObject.rotation = targetRotation;//Quaternion.RotateTowards(rotateObject.rotation, targetRotation, maxRotationSpeed * Time.deltaTime); ;
            }

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            isColliding = true;

            // 获取碰撞法线
            Vector2 collisionNormal = GetCollisionNormal2D(other);

            // 计算基于速度的反弹力
            float dynamicBounceForce = CalculateDynamicBounceForce();

            // 计算反射方向
            Vector2 reflectDirection = Vector2.Reflect(rb.velocity.normalized, collisionNormal);

            // 应用基于速度的反弹力
            rb.velocity = reflectDirection * dynamicBounceForce;

            // 短暂延迟后重置碰撞状态
            Invoke("ResetCollision", 0.1f);

            Debug.Log($"碰撞速度: {currentSpeed}, 反弹力: {dynamicBounceForce}");
        }
    }

    float CalculateDynamicBounceForce()
    {
        // 基础反弹力 + 速度乘数 * 当前速度
        float calculatedForce = baseBounceForce + (velocityMultiplier * currentSpeed);

        // 限制最大反弹力
        return Mathf.Min(calculatedForce, maxBounceForce);
    }

    void ResetCollision()
    {
        isColliding = false;
    }

    Vector2 GetCollisionNormal2D(Collider2D wallCollider)
    {
        // 使用多个射线获取更精确的法线
        Vector2[] rayDirections = {
            Vector2.left, Vector2.right, Vector2.up, Vector2.down,
            new Vector2(1, 1).normalized, new Vector2(-1, 1).normalized,
            new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized
        };

        RaycastHit2D hit;
        Vector2 averageNormal = Vector2.zero;
        int hitCount = 0;

        foreach (Vector2 dir in rayDirections)
        {
            hit = Physics2D.Raycast(transform.position, dir, 1f);
            if (hit.collider != null && hit.collider == wallCollider)
            {
                averageNormal += hit.normal;
                hitCount++;
            }
        }

        if (hitCount > 0)
        {
            return (averageNormal / hitCount).normalized;
        }

        // 备用法线计算
        Vector2 closestPoint = wallCollider.ClosestPoint(transform.position);
        return ((Vector2)transform.position - closestPoint).normalized;
    }
}
