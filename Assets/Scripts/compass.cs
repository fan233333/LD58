using UnityEngine;
using UnityEngine.UI;

public class compass : MonoBehaviour
{
    [Header("角色 Transform（即被追踪者）")]
    public Transform player;

    [Header("目标点（通常为世界原点）")]
    public Vector2 targetPoint = Vector2.zero;

    [Header("是否反转方向（箭头指向目标 or 远离目标）")]
    public bool pointTowardTarget = true;

    private RectTransform compassRect;



    void Awake()
    {
        compassRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (player == null || compassRect == null) return;

        // 计算从角色到目标点的方向
        Vector2 dir = (targetPoint - (Vector2)player.position).normalized;
        if (!pointTowardTarget)
            dir = -dir;

        // 计算角度（以世界上方为0度）
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        // 更新UI箭头的旋转
        compassRect.localRotation = Quaternion.Euler(0, 0, angle);
    }
}