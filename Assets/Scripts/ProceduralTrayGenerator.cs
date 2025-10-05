using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(RopeController))]
public class ProceduralTrayGenerator : MonoBehaviour
{
    public enum RopeJointType
    {
        Spring,
        Hinge
    }

    [Header("基础参数")]
    public int segmentCount = 32;           // 节点数量
    public float ropeLength = 5f;          // 绳子总长度
    public float ropeSag = 2f;              // 双曲余弦函数弧度影响
    public GameObject nodePrefab;           // 圆形节点预制体
    public Transform startPoint;            // 起点
    public Transform endPoint;              // 终点

    [Header("物理属性")]
    public RopeJointType jointType = RopeJointType.Spring;
    public float springFrequency = 15f;      // 弹簧刚度
    public float springDamping = 0.9f;      // 弹簧阻尼
    public float hingeDistance = 0.5f;      // 铰链连接的节点距离（可调）

    private RopeController ropeController;
    private List<Transform> ropeNodes = new List<Transform>();

    void Start()
    {
        ropeController = GetComponent<RopeController>();

        if (nodePrefab == null || startPoint == null || endPoint == null)
        {
            Debug.LogError("请在 Inspector 中指定 nodePrefab、startPoint、endPoint！");
            return;
        }

        GenerateRope();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            segmentCount = 47;
            ropeSag = 1f;
            ropeLength = 5f;
            GenerateRope();
        }
    }

    public void GenerateRope()
    {
        // 清理旧节点
        foreach (Transform child in transform)
            if (child.CompareTag("Rope"))
            {
                Destroy(child.gameObject);
            }

        ropeNodes.Clear();

        float step = 1f / (segmentCount - 1);

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i * step;
            Vector3 pos = GetCoshPosition(t);
            GameObject node = Instantiate(nodePrefab, pos, Quaternion.identity, transform);
            node.name = $"RopeNode_{i}";

            Rigidbody2D rb = node.GetComponent<Rigidbody2D>();
            if (rb == null)
                rb = node.AddComponent<Rigidbody2D>();

            // 固定两端节点
            if (i == 0)
            {
                rb.bodyType = RigidbodyType2D.Static;
                node.transform.position = startPoint.position;
            }
            else if (i == segmentCount - 1)
            {
                rb.bodyType = RigidbodyType2D.Static;
                node.transform.position = endPoint.position;
            }

            ropeNodes.Add(node.transform);

            // 连接前一个节点
            if (i > 0)
            {
                Rigidbody2D prevRb = ropeNodes[i - 1].GetComponent<Rigidbody2D>();

                switch (jointType)
                {
                    case RopeJointType.Spring:
                        AddSpringJoint(node, prevRb);
                        break;

                    case RopeJointType.Hinge:
                        AddHingeJoint(node, prevRb);
                        break;
                }
            }
        }

        // 更新 RopeSpriteShape 的节点
        ropeController.ropeNodes = ropeNodes;
    }

    void AddSpringJoint(GameObject node, Rigidbody2D connectedBody)
    {
        SpringJoint2D joint = node.AddComponent<SpringJoint2D>();
        joint.connectedBody = connectedBody;
        joint.autoConfigureDistance = false;
        joint.distance = ropeLength / segmentCount;
        joint.frequency = springFrequency;
        joint.dampingRatio = springDamping;
    }

    void AddHingeJoint(GameObject node, Rigidbody2D connectedBody)
    {
        HingeJoint2D joint = node.AddComponent<HingeJoint2D>();
        joint.connectedBody = connectedBody;
    }

    Vector3 GetCoshPosition(float t)
    {
        Vector3 start = startPoint.position;
        Vector3 end = endPoint.position;
        Vector3 right = (end - start);
        float length = right.magnitude;
        Vector3 dir = right.normalized;

        float x = t * length;
        float y = (float)(-ropeSag * Math.Cosh((x - length / 2f) / (length / 4f)) + ropeSag * Math.Cosh(length / (2f * (length / 4f))));
        Vector3 localPos = start + dir * x + Vector3.down * y;
        return localPos;
    }
}