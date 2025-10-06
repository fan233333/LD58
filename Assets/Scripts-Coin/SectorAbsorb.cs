using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class SectorAbsorb : MonoBehaviour
{
    [Header("扇形参数")]
    [Range(0.1f, 10f)]
    public float radius = 3f;
    [Range(1f, 360f)]
    public float angle = 90f;
    [Range(3, 64)]
    public int segments = 16;

    [Header("吸收设置")]
    public string targetTag = "Collectible"; // 要吸收的物品标签
    public LayerMask targetLayer = -1; // 要吸收的物品层级
    public float baseAbsorbSpeed = 2f; // 基础吸收速度
    public float minAbsorbSpeed = 0.5f; // 最小吸收速度（距离最远时）
    public float maxAbsorbSpeed = 5f; // 最大吸收速度（距离最近时）
    public float checkInterval = 0.2f; // 检测间隔（秒）

    [Header("速度曲线设置")]
    public AnimationCurve speedCurve = AnimationCurve.Linear(0, 0, 1, 1); // 速度距离曲线

    [Header("方向设置")]
    public bool faceRight = true; // 是否朝右

    [Header("其他")]
    public tray Bag;

    private PolygonCollider2D sectorCollider;
    private List<Transform> absorbedItems = new List<Transform>(); // 正在被吸收的物品
    private Dictionary<Transform, Collider2D> itemColliders = new Dictionary<Transform, Collider2D>(); // 物品与其碰撞体的映射
    private Dictionary<Transform, float> itemDistances = new Dictionary<Transform, float>(); // 物品与中心的距离
    private float checkTimer = 0f;

    void Start()
    {
        sectorCollider = GetComponent<PolygonCollider2D>();
        sectorCollider.isTrigger = true; // 设置为触发器，用于检测
        UpdateSectorCollider();

        // 初始化默认速度曲线（如果未设置）
        if (speedCurve == null || speedCurve.length == 0)
        {
            speedCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        }
    }

    void Update()
    {


        // 定时检测扇形区域内的物品
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckItemsInSector();
        }

        // 更新物品距离
        UpdateItemDistances();

        // 移动被吸收的物品
        MoveAbsorbedItems();

        // 检查已吸收物品是否脱离扇形区域
        CheckAbsorbedItemsInSector();
    }

    void UpdateSectorCollider()
    {
        if (sectorCollider == null) return;

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

            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;

            points[i + 1] = new Vector2(x, y);
        }

        sectorCollider.points = points;
    }

    void CheckItemsInSector()
    {
        // 获取扇形区域内的所有碰撞体
        Collider2D[] collidersInSector = Physics2D.OverlapAreaAll(
            sectorCollider.bounds.min,
            sectorCollider.bounds.max
        );

        foreach (Collider2D collider in collidersInSector)
        {
            // 检查碰撞体是否在扇形区域内
            if (IsInSector(collider.transform.position) &&
                !absorbedItems.Contains(collider.transform))
            {
                // 检查标签和层级
                bool tagMatch = string.IsNullOrEmpty(targetTag) || collider.CompareTag(targetTag);
                bool layerMatch = targetLayer.value == 0 || targetLayer == (targetLayer | (1 << collider.gameObject.layer));

                if (tagMatch && layerMatch)
                {
                    StartAbsorbingItem(collider.transform);
                }
            }
        }
    }

    // 更新所有被吸收物品的距离信息
    void UpdateItemDistances()
    {
        for (int i = absorbedItems.Count - 1; i >= 0; i--)
        {
            Transform item = absorbedItems[i];

            if (item == null)
            {
                absorbedItems.RemoveAt(i);
                if (itemColliders.ContainsKey(item))
                    itemColliders.Remove(item);
                if (itemDistances.ContainsKey(item))
                    itemDistances.Remove(item);
                continue;
            }

            // 计算并更新距离
            float distance = Vector2.Distance(transform.position, item.position);
            itemDistances[item] = distance;
        }
    }

    // 根据距离计算物品的吸收速度
    float CalculateAbsorbSpeed(Transform item)
    {
        if (!itemDistances.ContainsKey(item))
            return baseAbsorbSpeed;

        float distance = itemDistances[item];

        CollectibleItem collectible = item.GetComponent<CollectibleItem>();
        float mass = collectible.mass;

        // 将距离转换为0-1的范围（0=最近，1=最远）
        float normalizedDistance = Mathf.Clamp01(distance / radius);

        // 使用速度曲线计算速度系数
        //float speedFactor = speedCurve.Evaluate(1 - normalizedDistance); // 反转，因为距离越近速度越快

        // 计算最终速度
        //float speed = Mathf.Lerp(minAbsorbSpeed, maxAbsorbSpeed, speedFactor);
        if(distance < radius/3*2)
        {
            return maxAbsorbSpeed * (1/mass);
        }
        else
        {
            return minAbsorbSpeed * (1/mass);
        }

            //return speed;
    }

    // 检查已吸收物品是否仍在扇形区域内
    void CheckAbsorbedItemsInSector()
    {
        // 从后往前遍历，以便在移除时不影响索引
        for (int i = absorbedItems.Count - 1; i >= 0; i--)
        {
            Transform item = absorbedItems[i];

            if (item == null)
            {
                absorbedItems.RemoveAt(i);
                if (itemColliders.ContainsKey(item))
                    itemColliders.Remove(item);
                if (itemDistances.ContainsKey(item))
                    itemDistances.Remove(item);
                continue;
            }

            // 检查物品是否仍在扇形区域内
            if (!IsInSector(item.position))
            {
                StopAbsorbingItem(item);
                absorbedItems.RemoveAt(i);
            }
        }
    }

    bool IsInSector(Vector2 position)
    {
        // 将位置转换到扇形本地坐标系
        Vector2 localPos = transform.InverseTransformPoint(position);

        // 检查距离
        if (localPos.magnitude > radius) return false;

        // 检查角度
        float itemAngle = Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
        float sectorStartAngle = faceRight ? -angle / 2 : -angle / 2 + 180;

        // 标准化角度
        itemAngle = NormalizeAngle(itemAngle);
        float normalizedSectorStart = NormalizeAngle(sectorStartAngle);
        float normalizedSectorEnd = NormalizeAngle(sectorStartAngle + angle);

        // 检查是否在扇形角度范围内
        if (normalizedSectorStart <= normalizedSectorEnd)
        {
            return itemAngle >= normalizedSectorStart && itemAngle <= normalizedSectorEnd;
        }
        else
        {
            return itemAngle >= normalizedSectorStart || itemAngle <= normalizedSectorEnd;
        }
    }

    float NormalizeAngle(float angle)
    {
        angle %= 360;
        if (angle < 0) angle += 360;
        return angle;
    }

    void StartAbsorbingItem(Transform item)
    {
        // 获取并关闭碰撞体
        Collider2D itemCollider = item.GetComponent<Collider2D>();
        if (itemCollider != null)
        {
            itemCollider.enabled = false;
            itemColliders[item] = itemCollider; // 保存碰撞体引用
        }

        // 计算初始距离
        float initialDistance = Vector2.Distance(transform.position, item.position);
        itemDistances[item] = initialDistance;

        // 添加到吸收列表
        absorbedItems.Add(item);

        // 可选：添加视觉效果或声音
        //Debug.Log($"开始吸收物品: {item.name}, 初始距离: {initialDistance:F2}");
    }

    void StopAbsorbingItem(Transform item)
    {
        // 重新启用碰撞体
        if (itemColliders.ContainsKey(item))
        {
            itemColliders[item].enabled = true;
            itemColliders.Remove(item);
        }

        // 移除距离记录
        if (itemDistances.ContainsKey(item))
        {
            itemDistances.Remove(item);
        }

        // 可选：添加脱离效果的视觉或声音反馈
        //Debug.Log($"物品脱离扇形区域: {item.name}");
    }

    void MoveAbsorbedItems()
    {
        // 从后往前遍历，以便在移除时不影响索引
        for (int i = absorbedItems.Count - 1; i >= 0; i--)
        {
            Transform item = absorbedItems[i];

            if (item == null)
            {
                absorbedItems.RemoveAt(i);
                if (itemColliders.ContainsKey(item))
                    itemColliders.Remove(item);
                if (itemDistances.ContainsKey(item))
                    itemDistances.Remove(item);
                continue;
            }

            // 根据距离计算当前速度
            float currentSpeed = CalculateAbsorbSpeed(item);

            // 计算移动方向
            Vector2 direction = (transform.position - item.position).normalized;

            // 移动物品
            item.position = Vector2.MoveTowards(
                item.position,
                transform.position,
                currentSpeed * Time.deltaTime
            );

            // 更新距离
            itemDistances[item] = Vector2.Distance(transform.position, item.position);

            // 如果物品已经非常接近中心，移除它
            if (Vector2.Distance(item.position, transform.position) < 1f)
            {
                OnItemAbsorbed(item);
                absorbedItems.RemoveAt(i);
                if (itemColliders.ContainsKey(item))
                    itemColliders.Remove(item);
                if (itemDistances.ContainsKey(item))
                    itemDistances.Remove(item);
            }
        }
    }

    // 在SectorAbsorb脚本的OnItemAbsorbed方法中添加统计记录
    void OnItemAbsorbed(Transform item)
    {
        // 获取物品组件
        CollectibleItem collectible = item.GetComponent<CollectibleItem>();

        if (collectible != null)
        {
            // 记录统计
            ItemStatistics.Instance.RecordItem(collectible);
            ProcessBars.myHealth -= collectible.value;

            // 播放收集效果
            if (collectible.collectEffect != null)
            {
                Instantiate(collectible.collectEffect, item.position, Quaternion.identity);
            }

            // 播放收集音效
            if (collectible.collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectible.collectSound, item.position);
            }

            //Debug.Log($"物品被吸收: {collectible.GetTypeName()}");
        }
        else
        {
            //Debug.Log($"物品被吸收: {item.name}");
        }

        Bag.SpawnPrefab(item.gameObject);
        //Debug.Log("Spawn");

        // 销毁物品
        Destroy(item.gameObject);
    }

    // 设置扇形方向
    public void SetDirection(bool facingRight)
    {
        faceRight = facingRight;
        UpdateSectorCollider();
    }

    // 可视化调试
    void OnDrawGizmosSelected()
    {
        if (sectorCollider == null) return;

        // 绘制扇形区域
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Vector3 center = transform.position;
        Vector3[] worldPoints = new Vector3[sectorCollider.points.Length];

        for (int i = 0; i < sectorCollider.points.Length; i++)
        {
            worldPoints[i] = center + transform.TransformDirection(sectorCollider.points[i]);
        }

        // 绘制扇形填充区域
        for (int i = 1; i < worldPoints.Length - 1; i++)
        {
            Gizmos.DrawLine(center, worldPoints[i]);
            Gizmos.DrawLine(worldPoints[i], worldPoints[i + 1]);
        }
        Gizmos.DrawLine(center, worldPoints[worldPoints.Length - 1]);

        // 绘制被吸收物品的路径和速度指示
        foreach (Transform item in absorbedItems)
        {
            if (item != null)
            {
                // 根据速度设置线条颜色（红色=慢，绿色=快）
                float speed = CalculateAbsorbSpeed(item);
                float speedRatio = (speed - minAbsorbSpeed) / (maxAbsorbSpeed - minAbsorbSpeed);
                Gizmos.color = Color.Lerp(Color.red, Color.green, speedRatio);

                // 绘制移动路径
                Gizmos.DrawLine(item.position, transform.position);

                // 在物品位置绘制速度指示器
                Vector3 direction = (transform.position - item.position).normalized;
                Gizmos.DrawRay(item.position, direction * 0.5f);

                // 绘制距离信息
#if UNITY_EDITOR
                UnityEditor.Handles.Label(item.position + Vector3.up * 0.3f,
                    $"距离: {itemDistances[item]:F1}\n速度: {speed:F1}");
#endif
            }
        }

        // 绘制脱离扇形区域的物品（如果有）
        Gizmos.color = Color.red;
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();
        foreach (Collider2D collider in allColliders)
        {
            if (collider.gameObject != gameObject &&
                !absorbedItems.Contains(collider.transform) &&
                IsValidTarget(collider.transform) &&
                IsInSector(collider.transform.position))
            {
                Gizmos.DrawWireCube(collider.transform.position, Vector3.one * 0.5f);
            }
        }
    }

    bool IsValidTarget(Transform target)
    {
        bool tagMatch = string.IsNullOrEmpty(targetTag) || target.CompareTag(targetTag);
        bool layerMatch = targetLayer.value == 0 || targetLayer == (targetLayer | (1 << target.gameObject.layer));
        return tagMatch && layerMatch;
    }

    // 在Inspector中修改参数时实时更新
    void OnValidate()
    {
        if (sectorCollider != null)
            UpdateSectorCollider();
    }

    
}