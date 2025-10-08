using UnityEngine;
using UnityEngine.UI;

public class UICompass : MonoBehaviour
{
    [Header("UI组件")]
    public Image Arrow;
    
    [Header("追踪目标")]
    public Transform target;
    
    [Header("绳子引用")]
    public RopeController ropeController;
    
    [Header("活动范围设置")]
    [Range(0.1f, 1f)]
    public float activeRangeX = 0.8f; // X轴活动范围 (0.8 = 屏幕边缘留20%空间)
    [Range(0.1f, 1f)]
    public float activeRangeY = 0.8f; // Y轴活动范围 (0.8 = 屏幕边缘留20%空间)
    
    [Header("边缘偏移")]
    public float edgeOffset = 50f; // 距离屏幕边缘的像素偏移
    
    Camera mainCamera => Camera.main;
    RectTransform indicator => Arrow.rectTransform;
    
    // 根据设置计算的活动区域
    Rect activeRect => new Rect(
        (1f - activeRangeX) * 0.5f, 
        (1f - activeRangeY) * 0.5f, 
        activeRangeX, 
        activeRangeY
    );

    void Update()
    {
        // 检查是否应该显示指南针
        if (!ShouldShowCompass())
        {
            Arrow.gameObject.SetActive(false);
            return;
        }
        
        Arrow.gameObject.SetActive(true);
        
        if (target == null || mainCamera == null)
            return;
        
        Vector3 targetViewportPos = mainCamera.WorldToViewportPoint(target.position);
        
        // 如果目标在摄像机的视野内且在活动范围内
        if (targetViewportPos.z > 0 && activeRect.Contains(targetViewportPos))
        {
            // 目标在视野内，隐藏指南针或显示在目标位置
            Arrow.gameObject.SetActive(false);
        }
        else
        {
            // 目标在视野外，显示边缘指示器
            Arrow.gameObject.SetActive(true);
            UpdateEdgeIndicator(targetViewportPos);
        }
    }
    
    bool ShouldShowCompass()
    {
        // 只有当绳子断裂时才显示指南针
        if (ropeController == null)
            return false;
            
        return ropeController.broken;
    }
    
    void UpdateEdgeIndicator(Vector3 targetViewportPos)
    {
        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
        Vector3 targetScreenPos = mainCamera.WorldToScreenPoint(target.position);
        
        // 确保目标在摄像机前方
        if (targetScreenPos.z < 0)
            targetScreenPos *= -1;
            
        Vector3 directionFromCenter = (targetScreenPos - screenCenter).normalized;
        
        // 计算活动区域的边界（像素坐标）
        float activeWidth = Screen.width * activeRangeX;
        float activeHeight = Screen.height * activeRangeY;
        float activeLeft = (Screen.width - activeWidth) * 0.5f + edgeOffset;
        float activeRight = Screen.width - activeLeft;
        float activeBottom = (Screen.height - activeHeight) * 0.5f + edgeOffset;
        float activeTop = Screen.height - activeBottom;
        
        // 计算与活动区域边缘的交点
        Vector3 edgePosition = screenCenter;
        
        if (Mathf.Abs(directionFromCenter.x) > 0.001f)
        {
            float t1 = (activeRight - screenCenter.x) / directionFromCenter.x;
            float t2 = (activeLeft - screenCenter.x) / directionFromCenter.x;
            float tx = directionFromCenter.x > 0 ? t1 : t2;
            
            if (Mathf.Abs(directionFromCenter.y) > 0.001f)
            {
                float t3 = (activeTop - screenCenter.y) / directionFromCenter.y;
                float t4 = (activeBottom - screenCenter.y) / directionFromCenter.y;
                float ty = directionFromCenter.y > 0 ? t3 : t4;
                
                float t = Mathf.Min(Mathf.Abs(tx), Mathf.Abs(ty));
                edgePosition = screenCenter + directionFromCenter * t;
            }
            else
            {
                edgePosition = screenCenter + directionFromCenter * Mathf.Abs(tx);
            }
        }
        else if (Mathf.Abs(directionFromCenter.y) > 0.001f)
        {
            float t3 = (activeTop - screenCenter.y) / directionFromCenter.y;
            float t4 = (activeBottom - screenCenter.y) / directionFromCenter.y;
            float ty = directionFromCenter.y > 0 ? t3 : t4;
            edgePosition = screenCenter + directionFromCenter * Mathf.Abs(ty);
        }
        
        // 确保在屏幕边界内
        edgePosition.x = Mathf.Clamp(edgePosition.x, activeLeft, activeRight);
        edgePosition.y = Mathf.Clamp(edgePosition.y, activeBottom, activeTop);
        edgePosition.z = 0;
        
        indicator.position = edgePosition;
        
        // 计算角度并旋转箭头
        float angle = Mathf.Atan2(directionFromCenter.y, directionFromCenter.x) * Mathf.Rad2Deg;
        indicator.rotation = Quaternion.Euler(0, 0, angle - 90); // 调整为箭头指向目标
    }
    
    // 编辑器中预览活动范围的辅助方法
    void OnDrawGizmosSelected()
    {
        if (mainCamera == null) return;
        
        // 绘制活动范围边界
        Vector3 screenSize = new Vector3(Screen.width, Screen.height, 0);
        Vector3 activeSize = new Vector3(screenSize.x * activeRangeX, screenSize.y * activeRangeY, 0);
        Vector3 offset = (screenSize - activeSize) * 0.5f;
        
        Gizmos.color = Color.yellow;
        Vector3 center = screenSize * 0.5f;
        Vector3 size = activeSize;
        
        // 转换为世界坐标进行绘制（这只是一个近似的可视化）
        Gizmos.DrawWireCube(center, size);
    }
}