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

    public GameObject collectibleParent = null;
    public int stage1 = 5;
    public int stage2 = 10;
    public int stage3 = 15;
    public int stage4 = 20;

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
        float minDurability = 1f;
        foreach (var node in ropeNodes)
        {
            float durability = node.GetComponent<RopeNodeDurability>().GetDurabilityRatio();
            if (durability < minDurability)
                minDurability = durability;
        }

        int total = 0;
        if (collectibleParent == null)
        {
            renderer.color = Color.red;
            return;
        }
            
        foreach (Transform child in collectibleParent.transform)
        {
            CollectibleItem collectibleltem = child.GetComponent<CollectibleItem>();
            total += collectibleltem.value;
        }

        float f = getColor(total);

        if (f == 0f)
        {
            ropeNodes[ropeNodes.Count/2].GetComponent<RopeNodeDurability>().Break();
        }
        
        if (!broken)
            renderer.color = new Color(1, Mathf.Min(minDurability, f), Mathf.Min(minDurability, f));
        else
            renderer.color = Color.red;

        
    }

    float getColor(int total)
    {
        if (total < stage1)
            return 1;
        if (total < stage2)
            return 0.67f;
        if (total < stage3)
            return 0.33f;
        return 0f;
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
