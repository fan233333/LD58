using System;
using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("物品属性")]
    public string itemName = "未命名物品"; // 物品名称
    public ItemType itemType = ItemType.Circle; // 物品类型
    public int value = 1; // 物品价值
    public float mass = 1;

    [Header("视觉效果")]
    public Color itemColor = Color.white; // 物品颜色
    public ParticleSystem collectEffect; // 收集特效

    [Header("音频")]
    public AudioClip collectSound; // 收集音效

    // 物品类型枚举
    public enum ItemType
    {
        Circle,     // 圆形
        Triangle,   // 三角形
        Square,     // 正方形
        Diamond,    // 菱形
        Star,       // 星星
        Hexagon,    // 六边形
        Custom      // 自定义类型
    }

    // 获取物品类型名称
    public string GetTypeName()
    {
        switch (itemType)
        {
            case ItemType.Circle: return "圆形";
            case ItemType.Triangle: return "三角形";
            case ItemType.Square: return "正方形";
            case ItemType.Diamond: return "菱形";
            case ItemType.Star: return "星星";
            case ItemType.Hexagon: return "六边形";
            case ItemType.Custom: return itemName;
            default: return "未知";
        }
    }

    // 获取物品类型标识（用于统计）
    public string GetTypeKey()
    {
        return itemType.ToString();
    }
    
        [Header("必须：可选绑定一个粒子预制体（ParticleSystem或带ParticleSystem的Prefab）")]
    [SerializeField] private ParticleSystem particlePrefab;

    // 缓存引用，避免每次 GetComponent 开销
    private int selfInstanceId;

    private void Awake()
    {
        selfInstanceId = GetInstanceID();
    }

    // —— 如果是3D物理，把这个方法名改成 OnTriggerEnter（参数 Collider other）即可 ——
    private void OnCollisionEnter2D(Collision2D other)
    {
        // 只处理与 Collectible 标签的接触
        // if (!other.transform.CompareTag("Collectible")) return;

        // 要求对方也有 CollectibleItem
        if (!other.transform.TryGetComponent<CollectibleItem>(out var otherItem)) return;

        int otherKey = (int)otherItem.itemType;

        // 只关心 key 组成 {0,5} 这对
        bool pairMatches =
            ((int)itemType == 0 && otherKey == 5) ||
            ((int)itemType == 5 && otherKey == 0);

        if (!pairMatches) return;

        // 防止两个物体各自脚本都触发一次：只让“实例ID较小”的那一方来执行
        int otherGoId = other.gameObject.GetInstanceID();
        int selfGoId = gameObject.GetInstanceID();

        if (selfGoId > otherGoId) return; // 让ID更小的那一边负责一次性处理

        // 计算粒子播放位置：两者位置的中点
        Vector3 spawnPos = (transform.position + other.transform.position) * 0.5f;

        // 播放粒子
        if (particlePrefab != null)
        {
            // 简单做法：实例化一次性粒子
            ParticleSystem ps = Instantiate(particlePrefab, spawnPos, Quaternion.identity);
            ps.Play();
            // 粒子寿命结束后自动销毁（如果你的预制体里没设置StopAction=Destroy，手动销毁）
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetimeMultiplier + 0.5f);
        }

        // 同时销毁两件物体
        // 先禁用碰撞体，避免在销毁过程中再次触发
        DisableAllColliders(gameObject);
        DisableAllColliders(other.gameObject);

        Destroy(other.gameObject);
        Destroy(gameObject);
    }

    private void DisableAllColliders(GameObject go)
    {
        foreach (var c in go.GetComponentsInChildren<Collider2D>())
            c.enabled = false;
        // 如果用3D，把上面替换为：
        // foreach (var c in go.GetComponentsInChildren<Collider>()) c.enabled = false;
    }
}