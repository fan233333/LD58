using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class SectorCollider : MonoBehaviour
{
    [Header("扇形参数")]
    [Range(0.1f, 10f)]
    public float radius = 2f;
    [Range(1f, 360f)]
    public float angle = 90f;
    [Range(3, 64)]
    public int segments = 16;

    [Header("碰撞体设置")]
    public bool isTrigger = false;

    [Header("方向设置")]
    public bool faceRight = true; // 是否朝右

    private PolygonCollider2D polygonCollider;

    void Start()
    {
        polygonCollider = GetComponent<PolygonCollider2D>();
        UpdateSectorCollider();
    }

    void UpdateSectorCollider()
    {
        if (polygonCollider == null) return;

        int pointCount = segments + 2;
        Vector2[] points = new Vector2[pointCount];

        // 计算角度（弧度）
        float angleRad = angle * Mathf.Deg2Rad;

        // 调整起始角度，使扇形朝右
        float startAngle = faceRight ? -angleRad / 2 : (-angleRad / 2 + Mathf.PI);
        float angleStep = angleRad / segments;

        // 中心点
        points[0] = Vector2.zero;

        // 边缘点
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + i * angleStep;

            float x = Mathf.Cos(currentAngle) * radius; // 使用Cos计算X
            float y = Mathf.Sin(currentAngle) * radius; // 使用Sin计算Y

            points[i + 1] = new Vector2(x, y);
        }

        polygonCollider.points = points;
        polygonCollider.isTrigger = isTrigger;
    }

    // 在Inspector中修改参数时实时更新
    void OnValidate()
    {
        if (polygonCollider != null)
            UpdateSectorCollider();
        
    }

    // 公开方法，用于动态修改参数
    public void SetSector(float newRadius, float newAngle, int newSegments = -1)
    {
        radius = newRadius;
        angle = newAngle;
        if (newSegments > 0) segments = newSegments;

        UpdateSectorCollider();
    }

    // 设置扇形方向
    public void SetDirection(bool facingRight)
    {
        faceRight = facingRight;
        UpdateSectorCollider();
    }

    // 可视化调试
    void OnDrawGizmos()
    {
        if (polygonCollider == null) return;

        Gizmos.color = isTrigger ? new Color(0, 1, 1, 0.3f) : new Color(1, 0, 0, 0.3f);

        Vector3 center = transform.position;
        Vector3[] worldPoints = new Vector3[polygonCollider.points.Length];

        for (int i = 0; i < polygonCollider.points.Length; i++)
        {
            worldPoints[i] = center + transform.TransformDirection(polygonCollider.points[i]);
        }

        // 绘制扇形填充区域
        for (int i = 1; i < worldPoints.Length - 1; i++)
        {
            Gizmos.DrawLine(center, worldPoints[i]);
            Gizmos.DrawLine(worldPoints[i], worldPoints[i + 1]);
        }
        Gizmos.DrawLine(center, worldPoints[worldPoints.Length - 1]);

        // 绘制方向指示器
        Gizmos.color = Color.yellow;
        Vector3 direction = faceRight ? transform.right : -transform.right;
        Gizmos.DrawRay(center, direction * radius * 0.8f);
    }
}