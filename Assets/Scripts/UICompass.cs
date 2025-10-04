using UnityEngine;
using UnityEngine.UI;

public class UICompass : MonoBehaviour
{
    public Image Arrow;
    public Transform target;
    Camera mainCamera => Camera.main;
    RectTransform indicator => Arrow.rectTransform;
    static Rect rect=new Rect(0,0,1, 1);

    void Update()
    {
        if (target == null || mainCamera == null)
            return;
        
        Vector3 targetViewportPos = mainCamera.WorldToViewportPoint(target.position);
        //如果目标在摄像机的视野内
        if (targetViewportPos.z > 0 && rect.Contains(targetViewportPos))
        {
            
            indicator.anchoredPosition = new Vector2((targetViewportPos.x-0.5f)* Screen.width,(targetViewportPos.y-0.5f)* Screen.height);
            indicator.rotation = Quaternion.identity;
        }
        else
        {
            
            Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0)/ 2;
            Vector3 targetScreenPos = mainCamera.WorldToScreenPoint(target.position);//确保目标在摄像机前方
            if(targetScreenPos.z < 0)
                targetScreenPos *=-1;
            Vector3 directionFromCenter = (targetScreenPos -screenCenter).normalized;float aspect =Screen.width/(float)Screen.height /2;
// 根据屏幕的长宽比调整方向
            directionFromCenter.y /= aspect;
// 计算与屏幕边缘的交点
            float x= screenCenter.y / Mathf.Abs(directionFromCenter.y);
            float y=screenCenter.x/ Mathf.Abs(directionFromCenter.x);
            float d = Mathf.Min(x,y);Vector3 edgePosition = screenCenter + directionFromCenter * d * aspect;//将z坐标设置为0以保持在UI层
            edgePosition.z = 0;
            indicator.position =edgePosition;
//计算角度
            float angle = Mathf.Atan2(directionFromCenter.y, directionFromCenter.x)* Mathf.Rad2Deg;
//旋转箭头以指向目标
            indicator.rotation =Quaternion.Euler(0,0,angle + 90);
        }
    }
}