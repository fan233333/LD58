using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("物品属性")]
    public string itemName = "未命名物品"; // 物品名称
    public ItemType itemType = ItemType.Circle; // 物品类型
    public int value = 1; // 物品价值

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
}