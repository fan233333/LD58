using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class SectorAbsorb : MonoBehaviour
{
    [Header("���β���")]
    [Range(0.1f, 10f)]
    public float radius = 3f;
    [Range(1f, 360f)]
    public float angle = 90f;
    [Range(3, 64)]
    public int segments = 16;

    [Header("��������")]
    public string targetTag = "Collectible"; // Ҫ���յ���Ʒ��ǩ
    public LayerMask targetLayer = -1; // Ҫ���յ���Ʒ�㼶
    public float baseAbsorbSpeed = 2f; // ���������ٶ�
    public float minAbsorbSpeed = 0.5f; // ��С�����ٶȣ�������Զʱ��
    public float maxAbsorbSpeed = 5f; // ��������ٶȣ��������ʱ��
    public float checkInterval = 0.2f; // ��������룩

    [Header("�ٶ���������")]
    public AnimationCurve speedCurve = AnimationCurve.Linear(0, 0, 1, 1); // �ٶȾ�������

    [Header("��������")]
    public bool faceRight = true; // �Ƿ���

    [Header("����")]
    public tray Bag;

    private PolygonCollider2D sectorCollider;
    private List<Transform> absorbedItems = new List<Transform>(); // ���ڱ����յ���Ʒ
    private Dictionary<Transform, Collider2D> itemColliders = new Dictionary<Transform, Collider2D>(); // ��Ʒ������ײ���ӳ��
    private Dictionary<Transform, float> itemDistances = new Dictionary<Transform, float>(); // ��Ʒ�����ĵľ���
    private float checkTimer = 0f;

    void Start()
    {
        sectorCollider = GetComponent<PolygonCollider2D>();
        sectorCollider.isTrigger = true; // ����Ϊ�����������ڼ��
        UpdateSectorCollider();

        // ��ʼ��Ĭ���ٶ����ߣ����δ���ã�
        if (speedCurve == null || speedCurve.length == 0)
        {
            speedCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        }
    }

    void Update()
    {


        // ��ʱ������������ڵ���Ʒ
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckItemsInSector();
        }

        // ������Ʒ����
        UpdateItemDistances();

        // �ƶ������յ���Ʒ
        MoveAbsorbedItems();

        // �����������Ʒ�Ƿ�������������
        CheckAbsorbedItemsInSector();
    }

    void UpdateSectorCollider()
    {
        if (sectorCollider == null) return;

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

            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;

            points[i + 1] = new Vector2(x, y);
        }

        sectorCollider.points = points;
    }

    void CheckItemsInSector()
    {
        // ��ȡ���������ڵ�������ײ��
        Collider2D[] collidersInSector = Physics2D.OverlapAreaAll(
            sectorCollider.bounds.min,
            sectorCollider.bounds.max
        );

        foreach (Collider2D collider in collidersInSector)
        {
            // �����ײ���Ƿ�������������
            if (IsInSector(collider.transform.position) &&
                !absorbedItems.Contains(collider.transform))
            {
                // ����ǩ�Ͳ㼶
                bool tagMatch = string.IsNullOrEmpty(targetTag) || collider.CompareTag(targetTag);
                bool layerMatch = targetLayer.value == 0 || targetLayer == (targetLayer | (1 << collider.gameObject.layer));

                if (tagMatch && layerMatch)
                {
                    StartAbsorbingItem(collider.transform);
                }
            }
        }
    }

    // �������б�������Ʒ�ľ�����Ϣ
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

            // ���㲢���¾���
            float distance = Vector2.Distance(transform.position, item.position);
            itemDistances[item] = distance;
        }
    }

    // ���ݾ��������Ʒ�������ٶ�
    float CalculateAbsorbSpeed(Transform item)
    {
        if (!itemDistances.ContainsKey(item))
            return baseAbsorbSpeed;

        float distance = itemDistances[item];

        CollectibleItem collectible = item.GetComponent<CollectibleItem>();
        float mass = collectible.mass;

        // ������ת��Ϊ0-1�ķ�Χ��0=�����1=��Զ��
        float normalizedDistance = Mathf.Clamp01(distance / radius);

        // ʹ���ٶ����߼����ٶ�ϵ��
        //float speedFactor = speedCurve.Evaluate(1 - normalizedDistance); // ��ת����Ϊ����Խ���ٶ�Խ��

        // ���������ٶ�
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

    // �����������Ʒ�Ƿ���������������
    void CheckAbsorbedItemsInSector()
    {
        // �Ӻ���ǰ�������Ա����Ƴ�ʱ��Ӱ������
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

            // �����Ʒ�Ƿ���������������
            if (!IsInSector(item.position))
            {
                StopAbsorbingItem(item);
                absorbedItems.RemoveAt(i);
            }
        }
    }

    bool IsInSector(Vector2 position)
    {
        // ��λ��ת�������α�������ϵ
        Vector2 localPos = transform.InverseTransformPoint(position);

        // ������
        if (localPos.magnitude > radius) return false;

        // ���Ƕ�
        float itemAngle = Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
        float sectorStartAngle = faceRight ? -angle / 2 : -angle / 2 + 180;

        // ��׼���Ƕ�
        itemAngle = NormalizeAngle(itemAngle);
        float normalizedSectorStart = NormalizeAngle(sectorStartAngle);
        float normalizedSectorEnd = NormalizeAngle(sectorStartAngle + angle);

        // ����Ƿ������νǶȷ�Χ��
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
        // ��ȡ���ر���ײ��
        Collider2D itemCollider = item.GetComponent<Collider2D>();
        if (itemCollider != null)
        {
            itemCollider.enabled = false;
            itemColliders[item] = itemCollider; // ������ײ������
        }

        // �����ʼ����
        float initialDistance = Vector2.Distance(transform.position, item.position);
        itemDistances[item] = initialDistance;

        // ��ӵ������б�
        absorbedItems.Add(item);

        // ��ѡ������Ӿ�Ч��������
        //Debug.Log($"��ʼ������Ʒ: {item.name}, ��ʼ����: {initialDistance:F2}");
    }

    void StopAbsorbingItem(Transform item)
    {
        // ����������ײ��
        if (itemColliders.ContainsKey(item))
        {
            itemColliders[item].enabled = true;
            itemColliders.Remove(item);
        }

        // �Ƴ������¼
        if (itemDistances.ContainsKey(item))
        {
            itemDistances.Remove(item);
        }

        // ��ѡ���������Ч�����Ӿ�����������
        //Debug.Log($"��Ʒ������������: {item.name}");
    }

    void MoveAbsorbedItems()
    {
        // �Ӻ���ǰ�������Ա����Ƴ�ʱ��Ӱ������
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

            // ���ݾ�����㵱ǰ�ٶ�
            float currentSpeed = CalculateAbsorbSpeed(item);

            // �����ƶ�����
            Vector2 direction = (transform.position - item.position).normalized;

            // �ƶ���Ʒ
            item.position = Vector2.MoveTowards(
                item.position,
                transform.position,
                currentSpeed * Time.deltaTime
            );

            // ���¾���
            itemDistances[item] = Vector2.Distance(transform.position, item.position);

            // �����Ʒ�Ѿ��ǳ��ӽ����ģ��Ƴ���
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

    // ��SectorAbsorb�ű���OnItemAbsorbed���������ͳ�Ƽ�¼
    void OnItemAbsorbed(Transform item)
    {
        // ��ȡ��Ʒ���
        CollectibleItem collectible = item.GetComponent<CollectibleItem>();

        if (collectible != null)
        {
            // ��¼ͳ��
            ItemStatistics.Instance.RecordItem(collectible);
            ProcessBars.myHealth -= collectible.value;

            // �����ռ�Ч��
            if (collectible.collectEffect != null)
            {
                Instantiate(collectible.collectEffect, item.position, Quaternion.identity);
            }

            // �����ռ���Ч
            if (collectible.collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectible.collectSound, item.position);
            }

            //Debug.Log($"��Ʒ������: {collectible.GetTypeName()}");
        }
        else
        {
            //Debug.Log($"��Ʒ������: {item.name}");
        }

        Bag.SpawnPrefab(item.gameObject);
        //Debug.Log("Spawn");

        // ������Ʒ
        Destroy(item.gameObject);
    }

    // �������η���
    public void SetDirection(bool facingRight)
    {
        faceRight = facingRight;
        UpdateSectorCollider();
    }

    // ���ӻ�����
    void OnDrawGizmosSelected()
    {
        if (sectorCollider == null) return;

        // ������������
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Vector3 center = transform.position;
        Vector3[] worldPoints = new Vector3[sectorCollider.points.Length];

        for (int i = 0; i < sectorCollider.points.Length; i++)
        {
            worldPoints[i] = center + transform.TransformDirection(sectorCollider.points[i]);
        }

        // ���������������
        for (int i = 1; i < worldPoints.Length - 1; i++)
        {
            Gizmos.DrawLine(center, worldPoints[i]);
            Gizmos.DrawLine(worldPoints[i], worldPoints[i + 1]);
        }
        Gizmos.DrawLine(center, worldPoints[worldPoints.Length - 1]);

        // ���Ʊ�������Ʒ��·�����ٶ�ָʾ
        foreach (Transform item in absorbedItems)
        {
            if (item != null)
            {
                // �����ٶ�����������ɫ����ɫ=������ɫ=�죩
                float speed = CalculateAbsorbSpeed(item);
                float speedRatio = (speed - minAbsorbSpeed) / (maxAbsorbSpeed - minAbsorbSpeed);
                Gizmos.color = Color.Lerp(Color.red, Color.green, speedRatio);

                // �����ƶ�·��
                Gizmos.DrawLine(item.position, transform.position);

                // ����Ʒλ�û����ٶ�ָʾ��
                Vector3 direction = (transform.position - item.position).normalized;
                Gizmos.DrawRay(item.position, direction * 0.5f);

                // ���ƾ�����Ϣ
#if UNITY_EDITOR
                UnityEditor.Handles.Label(item.position + Vector3.up * 0.3f,
                    $"����: {itemDistances[item]:F1}\n�ٶ�: {speed:F1}");
#endif
            }
        }

        // �������������������Ʒ������У�
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

    // ��Inspector���޸Ĳ���ʱʵʱ����
    void OnValidate()
    {
        if (sectorCollider != null)
            UpdateSectorCollider();
    }

    
}