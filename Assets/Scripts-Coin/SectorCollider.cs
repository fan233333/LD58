using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class SectorCollider : MonoBehaviour
{
    [Header("���β���")]
    [Range(0.1f, 10f)]
    public float radius = 2f;
    [Range(1f, 360f)]
    public float angle = 90f;
    [Range(3, 64)]
    public int segments = 16;

    [Header("��ײ������")]
    public bool isTrigger = false;

    [Header("��������")]
    public bool faceRight = true; // �Ƿ���

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

        // ����Ƕȣ����ȣ�
        float angleRad = angle * Mathf.Deg2Rad;

        // ������ʼ�Ƕȣ�ʹ���γ���
        float startAngle = faceRight ? -angleRad / 2 : (-angleRad / 2 + Mathf.PI);
        float angleStep = angleRad / segments;

        // ���ĵ�
        points[0] = Vector2.zero;

        // ��Ե��
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + i * angleStep;

            float x = Mathf.Cos(currentAngle) * radius; // ʹ��Cos����X
            float y = Mathf.Sin(currentAngle) * radius; // ʹ��Sin����Y

            points[i + 1] = new Vector2(x, y);
        }

        polygonCollider.points = points;
        polygonCollider.isTrigger = isTrigger;
    }

    // ��Inspector���޸Ĳ���ʱʵʱ����
    void OnValidate()
    {
        if (polygonCollider != null)
            UpdateSectorCollider();
        
    }

    // �������������ڶ�̬�޸Ĳ���
    public void SetSector(float newRadius, float newAngle, int newSegments = -1)
    {
        radius = newRadius;
        angle = newAngle;
        if (newSegments > 0) segments = newSegments;

        UpdateSectorCollider();
    }

    // �������η���
    public void SetDirection(bool facingRight)
    {
        faceRight = facingRight;
        UpdateSectorCollider();
    }

    // ���ӻ�����
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

        // ���������������
        for (int i = 1; i < worldPoints.Length - 1; i++)
        {
            Gizmos.DrawLine(center, worldPoints[i]);
            Gizmos.DrawLine(worldPoints[i], worldPoints[i + 1]);
        }
        Gizmos.DrawLine(center, worldPoints[worldPoints.Length - 1]);

        // ���Ʒ���ָʾ��
        Gizmos.color = Color.yellow;
        Vector3 direction = faceRight ? transform.right : -transform.right;
        Gizmos.DrawRay(center, direction * radius * 0.8f);
    }
}