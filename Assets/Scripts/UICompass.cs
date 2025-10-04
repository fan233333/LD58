using UnityEngine;
using UnityEngine.UI;

public class UICompass : MonoBehaviour
{
    [Header("角色（通常是玩家）")]
    public Transform target;

    [Header("箭头 UI 对象")]
    public RectTransform compassArrow;

    [Header("主摄像机")]
    public Camera mainCamera;

    [Header("距离屏幕边缘的偏移")]
    public float edgeOffset = 50f;

    [Header("隐藏距离阈值（屏幕内多少范围算可见）")]
    [Range(0f, 1f)] public float screenVisibleThreshold = 0.1f;

    void Update()
    {
        if (target == null || compassArrow == null || mainCamera == null)
            return;

        // 世界中原点 (0,0)
        Vector3 origin = Vector3.zero;

        // 将原点转换到屏幕坐标
        Vector3 screenPos = mainCamera.WorldToViewportPoint(origin);

        bool isInFront = screenPos.z > 0;
        bool isInsideScreen =
            screenPos.x > 0 + screenVisibleThreshold &&
            screenPos.x < 1 - screenVisibleThreshold &&
            screenPos.y > 0 + screenVisibleThreshold &&
            screenPos.y < 1 - screenVisibleThreshold;

        // 如果原点在屏幕内，则隐藏箭头
        compassArrow.gameObject.SetActive(!(isInFront && isInsideScreen));

        if (!compassArrow.gameObject.activeSelf)
            return;

        // 计算角色到原点的方向
        Vector3 dir = (origin - target.position).normalized;

        // 箭头旋转
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        compassArrow.rotation = Quaternion.Euler(0, 0, angle - 90f); // UI箭头一般朝上，所以减90度

        // 将方向投影到屏幕边缘
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Vector3 targetScreenPos = mainCamera.WorldToScreenPoint(origin);
        Vector3 dirFromCenter = (targetScreenPos - screenCenter).normalized;

        // 计算箭头在屏幕边缘的位置
        float halfW = Screen.width / 2f - edgeOffset;
        float halfH = Screen.height / 2f - edgeOffset;
        float slope = dirFromCenter.y / dirFromCenter.x;
        Vector3 pos = screenCenter;

        if (Mathf.Abs(slope) < (halfH / halfW))
        {
            pos.x += (dirFromCenter.x > 0 ? halfW : -halfW);
            pos.y += slope * (dirFromCenter.x > 0 ? halfW : -halfW);
        }
        else
        {
            pos.y += (dirFromCenter.y > 0 ? halfH : -halfH);
            pos.x += (halfH / Mathf.Abs(slope)) * (dirFromCenter.x > 0 ? 1 : -1);
        }

        // 设置 UI 位置
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            compassArrow.parent as RectTransform, pos, mainCamera, out anchoredPos);
        compassArrow.anchoredPosition = anchoredPos;
    }
}