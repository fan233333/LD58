using UnityEngine;

public class PlayerMouseController: MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private Camera mainCamera;
    private Rigidbody2D rb;

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();

        // 如果对象没有Rigidbody2D，添加一个
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; // 2D游戏中通常不需要重力
        }
    }

    void Update()
    {
        FollowMousePosition();
        RotateTowardsMouse();
    }

    void FollowMousePosition()
    {
        // 获取鼠标在世界空间的位置
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // 确保z坐标为0（2D空间）

        // 计算移动方向
        Vector2 moveDirection = (mousePosition - transform.position).normalized;

        // 移动玩家
        rb.velocity = moveDirection * moveSpeed;

        // 或者使用Transform移动（取消注释下面这行，并注释上面那行）
        // transform.position = Vector2.MoveTowards(transform.position, mousePosition, moveSpeed * Time.deltaTime);
    }

    void RotateTowardsMouse()
    {
        // 获取鼠标在世界空间的位置
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // 计算朝向鼠标的方向
        Vector2 direction = mousePosition - transform.position;

        // 计算角度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 平滑旋转朝向鼠标
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}