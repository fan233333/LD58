using UnityEngine;
using UnityEngine.U2D; // SpriteShape 相关命名空间
using System.Collections.Generic;

[RequireComponent(typeof(SpriteShapeController))]
public class RopeSpriteShape : MonoBehaviour
{
    [Header("绳子节点（按顺序）")]
    public List<Transform> ropeNodes = new List<Transform>();

    [Header("采样精度（越大越平滑）")]
    public float smoothness = 10f;

    private SpriteShapeController spriteShapeController;

    void Awake()
    {
        spriteShapeController = GetComponent<SpriteShapeController>();
    }

    void LateUpdate()
    {
        if (ropeNodes == null || ropeNodes.Count < 2)
            return;

        UpdateRopeShape();
    }

    void UpdateRopeShape()
    {
        // 获取或创建 spline
        var spline = spriteShapeController.spline;
        int nodeCount = ropeNodes.Count;

        // 确保 spline 节点数一致
        while (spline.GetPointCount() < nodeCount)
            spline.InsertPointAt(spline.GetPointCount(), Vector3.zero);

        while (spline.GetPointCount() > nodeCount)
            spline.RemovePointAt(spline.GetPointCount() - 1);

        // 更新每个点的位置
        for (int i = 0; i < nodeCount; i++)
        {
            spline.SetPosition(i, ropeNodes[i].localPosition);
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }

        // 使绳子平滑
        for (int i = 0; i < nodeCount; i++)
        {
            Vector3 tangent = Vector3.zero;
            if (i > 0 && i < nodeCount - 1)
            {
                Vector3 prev = ropeNodes[i - 1].localPosition;
                Vector3 next = ropeNodes[i + 1].localPosition;
                tangent = (next - prev).normalized * smoothness;
            }

            spline.SetLeftTangent(i, -tangent);
            spline.SetRightTangent(i, tangent);
        }
    }
}