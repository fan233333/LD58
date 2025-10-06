using System;
using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("��Ʒ����")]
    public string itemName = "δ������Ʒ"; // ��Ʒ����
    public ItemType itemType = ItemType.Circle; // ��Ʒ����
    public int value = 1; // ��Ʒ��ֵ
    public float mass = 1;

    [Header("�Ӿ�Ч��")]
    public Color itemColor = Color.white; // ��Ʒ��ɫ
    public ParticleSystem collectEffect; // �ռ���Ч

    [Header("��Ƶ")]
    public AudioClip collectSound; // �ռ���Ч

    // ��Ʒ����ö��
    public enum ItemType
    {
        Circle,     // Բ��
        Triangle,   // ������
        Square,     // ������
        Diamond,    // ����
        Star,       // ����
        Hexagon,    // ������
        Custom      // �Զ�������
    }

    // ��ȡ��Ʒ��������
    public string GetTypeName()
    {
        switch (itemType)
        {
            case ItemType.Circle: return "Բ��";
            case ItemType.Triangle: return "������";
            case ItemType.Square: return "������";
            case ItemType.Diamond: return "����";
            case ItemType.Star: return "����";
            case ItemType.Hexagon: return "������";
            case ItemType.Custom: return itemName;
            default: return "δ֪";
        }
    }

    // ��ȡ��Ʒ���ͱ�ʶ������ͳ�ƣ�
    public string GetTypeKey()
    {
        return itemType.ToString();
    }
    
        [Header("���룺��ѡ��һ������Ԥ���壨ParticleSystem���ParticleSystem��Prefab��")]
    [SerializeField] private ParticleSystem particlePrefab;

    // �������ã�����ÿ�� GetComponent ����
    private int selfInstanceId;

    private void Awake()
    {
        selfInstanceId = GetInstanceID();
    }

    // ���� �����3D����������������ĳ� OnTriggerEnter������ Collider other������ ����
    private void OnCollisionEnter2D(Collision2D other)
    {
        // ֻ������ Collectible ��ǩ�ĽӴ�
        // if (!other.transform.CompareTag("Collectible")) return;

        // Ҫ��Է�Ҳ�� CollectibleItem
        if (!other.transform.TryGetComponent<CollectibleItem>(out var otherItem)) return;

        int otherKey = (int)otherItem.itemType;

        // ֻ���� key ��� {0,5} ���
        bool pairMatches =
            ((int)itemType == 0 && otherKey == 5) ||
            ((int)itemType == 5 && otherKey == 0);

        if (!pairMatches) return;

        // ��ֹ����������Խű�������һ�Σ�ֻ�á�ʵ��ID��С������һ����ִ��
        int otherGoId = other.gameObject.GetInstanceID();
        int selfGoId = gameObject.GetInstanceID();

        if (selfGoId > otherGoId) return; // ��ID��С����һ�߸���һ���Դ���

        // �������Ӳ���λ�ã�����λ�õ��е�
        Vector3 spawnPos = (transform.position + other.transform.position) * 0.5f;

        // ��������
        if (particlePrefab != null)
        {
            // ��������ʵ����һ��������
            ParticleSystem ps = Instantiate(particlePrefab, spawnPos, Quaternion.identity);
            ps.Play();
            // ���������������Զ����٣�������Ԥ������û����StopAction=Destroy���ֶ����٣�
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetimeMultiplier + 0.5f);
        }

        // ͬʱ������������
        // �Ƚ�����ײ�壬���������ٹ������ٴδ���
        DisableAllColliders(gameObject);
        DisableAllColliders(other.gameObject);

        Destroy(other.gameObject);
        Destroy(gameObject);
    }

    private void DisableAllColliders(GameObject go)
    {
        foreach (var c in go.GetComponentsInChildren<Collider2D>())
            c.enabled = false;
        // �����3D���������滻Ϊ��
        // foreach (var c in go.GetComponentsInChildren<Collider>()) c.enabled = false;
    }
}