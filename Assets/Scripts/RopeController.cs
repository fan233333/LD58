using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteShapeController), typeof(SpriteShapeRenderer))]
public class RopeController : MonoBehaviour
{
    [Header("绳子节点（按顺序）")]
    public List<Transform> ropeNodes = new List<Transform>();

    [Header("平滑程度")]
    public float smoothness = 0.1f;

    private SpriteShapeController controller;
    private SpriteShapeRenderer renderer;

    public bool broken = false;

    void Awake()
    {
        controller = GetComponent<SpriteShapeController>();
        renderer = GetComponent<SpriteShapeRenderer>();
    }

    void LateUpdate()
    {
        if (ropeNodes.Count < 2) return;
        UpdateRopeShape();
        UpdateRopeColor();
    }

    void UpdateRopeShape()
    {
        var spline = controller.spline;
        int count = ropeNodes.Count;

        while (spline.GetPointCount() < count)
            spline.InsertPointAt(spline.GetPointCount(), Random.insideUnitCircle);
        while (spline.GetPointCount() > count)
            spline.RemovePointAt(spline.GetPointCount() - 1);

        // 更新每个点的位置
        for (int i = 0; i < count; i++)
        {
            spline.SetPosition(i, ropeNodes[i].localPosition);
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }

        // 使绳子平滑
        for (int i = 0; i < count; i++)
        {
            Vector3 tangent = Vector3.zero;
            if (i > 0 && i < count - 1)
            {
                Vector3 prev = ropeNodes[i - 1].localPosition;
                Vector3 next = ropeNodes[i + 1].localPosition;
                tangent = (next - prev).normalized * smoothness;
            }

            spline.SetLeftTangent(i, -tangent);
            spline.SetRightTangent(i, tangent);
        }
    }

    void UpdateRopeColor()
    {
        renderer.color = Color.red;
    }

    // ---------------------------
    // 💥 断裂逻辑
    // ---------------------------
    public bool SplitRopeAtNode(RopeNodeDurability brokenNode)
    {
        if (!broken)
        {
            broken = true;
            int index = ropeNodes.IndexOf(brokenNode.transform);
            if (index <= 0 || index >= ropeNodes.Count - 1) return true;

            // 左右分两段
            List<Transform> leftNodes = ropeNodes.GetRange(0, index);
            List<Transform> rightNodes = ropeNodes.GetRange(index, ropeNodes.Count - (index + 1));

            // 原绳子只保留左半段
            ropeNodes = leftNodes;

            // 创建右半段绳子
            RopeFactory.Instance.CreateNewRopeSegment(this, rightNodes);
            

            return true;
        }

        Debug.Log("Already broken");
        return false;
    }
    
}
