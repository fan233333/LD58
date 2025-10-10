using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CarmeraManager : MonoBehaviour
{
    [Header("摄像机设置")]
    public CinemachineVirtualCamera virtualCamera; // 要控制的虚拟摄像机
    
    [Header("跟随目标")]
    public Transform defaultTarget; // 默认跟随目标
    public List<Transform> availableTargets = new List<Transform>(); // 可选的跟随目标列表
    
    [Header("切换设置")]
    public float transitionTime = 1f; // 切换过渡时间
    public bool useSmoothing = true; // 是否使用平滑过渡
    
    private Transform currentTarget; // 当前跟随的目标
    private int currentTargetIndex = 0; // 当前目标在列表中的索引

    [Header("任务相关")]
    public TaskManager taskManager; // 任务管理器引用
    public Transform taskCompletionTarget; // 任务完成时的摄像机目标
    public Transform taskFailedTarget; // 任务失败时的摄像机目标
    
    [Header("相机缩放设置")]
    public float taskCompletionCameraSize = 8f; // 任务完成时的相机范围大小
    public float normalCameraSize = 5f; // 正常相机范围大小
    public float zoomTransitionSpeed = 2f; // 缩放过渡速度
    
    private bool hasTaskCompleted = false;
    private bool hasTaskFailed = false;
    private float originalCameraSize; // 原始相机大小

    void Start()
    {
        // 初始化
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }
        
        // 记录原始相机大小
        if (virtualCamera != null)
        {
            originalCameraSize = virtualCamera.m_Lens.OrthographicSize;
            normalCameraSize = originalCameraSize; // 设置正常大小为当前大小
        }
        
        // 设置默认目标
        if (defaultTarget != null)
        {
            SetFollowTarget(defaultTarget);
        }
    }

    void Update()
    {
        if (taskManager != null)
        {
            // 任务完成时切换摄像机
            if (!hasTaskCompleted && taskManager.IsTaskCompleted())
            {
                hasTaskCompleted = true;
                if (taskCompletionTarget != null)
                {
                    SetFollowTarget(taskCompletionTarget);
                    Debug.Log("任务完成，切换到完成视角");
                }
                
                // 调整相机范围大小
                SetCameraSize(taskCompletionCameraSize);
                Debug.Log($"任务完成，相机范围调整到: {taskCompletionCameraSize}");
            }
            
            // 任务失败时切换摄像机
            if (!hasTaskFailed && taskManager.IsTaskFailed())
            {
                hasTaskFailed = true;
                if (taskFailedTarget != null)
                {
                    SetFollowTarget(taskFailedTarget);
                    Debug.Log("任务失败，切换到失败视角");
                }
                
                // 可选：任务失败时也可以调整相机大小
                // SetCameraSize(normalCameraSize);
            }
        }
        

    }

    /// <summary>
    /// 设置摄像机跟随目标
    /// </summary>
    /// <param name="target">要跟随的目标</param>
    public void SetFollowTarget(Transform target)
    {
        if (virtualCamera != null && target != null)
        {
            virtualCamera.Follow = target;
            currentTarget = target;
            
            Debug.Log($"摄像机现在跟随: {target.name}");
        }
    }

    /// <summary>
    /// 设置摄像机跟随目标（通过名称查找）
    /// </summary>
    /// <param name="targetName">目标对象的名称</param>
    public void SetFollowTargetByName(string targetName)
    {
        Transform target = GameObject.Find(targetName)?.transform;
        if (target != null)
        {
            SetFollowTarget(target);
        }
        else
        {
            Debug.LogWarning($"未找到名为 '{targetName}' 的目标对象");
        }
    }

    /// <summary>
    /// 设置摄像机跟随目标（通过标签查找）
    /// </summary>
    /// <param name="tag">目标对象的标签</param>
    public void SetFollowTargetByTag(string tag)
    {
        GameObject targetObj = GameObject.FindWithTag(tag);
        if (targetObj != null)
        {
            SetFollowTarget(targetObj.transform);
        }
        else
        {
            Debug.LogWarning($"未找到标签为 '{tag}' 的目标对象");
        }
    }

    /// <summary>
    /// 切换到列表中的下一个目标
    /// </summary>
    public void SwitchToNextTarget()
    {
        if (availableTargets.Count > 0)
        {
            currentTargetIndex = (currentTargetIndex + 1) % availableTargets.Count;
            SetFollowTarget(availableTargets[currentTargetIndex]);
        }
    }

    /// <summary>
    /// 切换到列表中的上一个目标
    /// </summary>
    public void SwitchToPreviousTarget()
    {
        if (availableTargets.Count > 0)
        {
            currentTargetIndex = (currentTargetIndex - 1 + availableTargets.Count) % availableTargets.Count;
            SetFollowTarget(availableTargets[currentTargetIndex]);
        }
    }

    /// <summary>
    /// 切换到指定索引的目标
    /// </summary>
    /// <param name="index">目标索引</param>
    public void SwitchToTargetByIndex(int index)
    {
        if (index >= 0 && index < availableTargets.Count)
        {
            currentTargetIndex = index;
            SetFollowTarget(availableTargets[index]);
        }
        else
        {
            Debug.LogWarning($"目标索引 {index} 超出范围 (0-{availableTargets.Count - 1})");
        }
    }

    /// <summary>
    /// 重置到默认目标
    /// </summary>
    public void ResetToDefaultTarget()
    {
        if (defaultTarget != null)
        {
            SetFollowTarget(defaultTarget);
        }
    }

    /// <summary>
    /// 添加新的可跟随目标到列表
    /// </summary>
    /// <param name="target">要添加的目标</param>
    public void AddAvailableTarget(Transform target)
    {
        if (target != null && !availableTargets.Contains(target))
        {
            availableTargets.Add(target);
        }
    }

    /// <summary>
    /// 从列表中移除目标
    /// </summary>
    /// <param name="target">要移除的目标</param>
    public void RemoveAvailableTarget(Transform target)
    {
        if (availableTargets.Contains(target))
        {
            availableTargets.Remove(target);
        }
    }

    /// <summary>
    /// 获取当前跟随的目标
    /// </summary>
    /// <returns>当前目标</returns>
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    /// <summary>
    /// 检查是否正在跟随指定目标
    /// </summary>
    /// <param name="target">要检查的目标</param>
    /// <returns>是否正在跟随</returns>
    public bool IsFollowing(Transform target)
    {
        return currentTarget == target;
    }

    /// <summary>
    /// 临时跟随目标（带自动恢复）
    /// </summary>
    /// <param name="target">临时目标</param>
    /// <param name="duration">跟随时长</param>
    public void TemporaryFollow(Transform target, float duration)
    {
        if (target != null)
        {
            Transform previousTarget = currentTarget;
            SetFollowTarget(target);
            StartCoroutine(RestoreTargetAfterDelay(previousTarget, duration));
        }
    }

    /// <summary>
    /// 延迟恢复目标的协程
    /// </summary>
    private IEnumerator RestoreTargetAfterDelay(Transform previousTarget, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (previousTarget != null)
        {
            SetFollowTarget(previousTarget);
        }
        else
        {
            ResetToDefaultTarget();
        }
    }

    /// <summary>
    /// 设置摄像机的Look At目标（如果需要）
    /// </summary>
    /// <param name="target">要看向的目标</param>
    public void SetLookAtTarget(Transform target)
    {
        if (virtualCamera != null)
        {
            virtualCamera.LookAt = target;
        }
    }

    /// <summary>
    /// 清除Look At目标
    /// </summary>
    public void ClearLookAtTarget()
    {
        if (virtualCamera != null)
        {
            virtualCamera.LookAt = null;
        }
    }

    /// <summary>
    /// 设置相机范围大小
    /// </summary>
    /// <param name="size">目标大小</param>
    public void SetCameraSize(float size)
    {
        if (virtualCamera != null)
        {
            StartCoroutine(SmoothZoomTo(size));
        }
    }

    /// <summary>
    /// 立即设置相机范围大小（无过渡）
    /// </summary>
    /// <param name="size">目标大小</param>
    public void SetCameraSizeImmediate(float size)
    {
        if (virtualCamera != null)
        {
            virtualCamera.m_Lens.OrthographicSize = size;
        }
    }

    /// <summary>
    /// 重置相机到正常大小
    /// </summary>
    public void ResetCameraSize()
    {
        SetCameraSize(normalCameraSize);
    }

    /// <summary>
    /// 平滑缩放到目标大小的协程
    /// </summary>
    private IEnumerator SmoothZoomTo(float targetSize)
    {
        if (virtualCamera == null) yield break;

        float startSize = virtualCamera.m_Lens.OrthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionTime;
            
            // 使用平滑曲线
            t = Mathf.SmoothStep(0f, 1f, t);
            
            float currentSize = Mathf.Lerp(startSize, targetSize, t);
            virtualCamera.m_Lens.OrthographicSize = currentSize;
            
            yield return null;
        }

        // 确保最终设置为目标大小
        virtualCamera.m_Lens.OrthographicSize = targetSize;
    }

    /// <summary>
    /// 调试方法：测试任务完成相机效果
    /// </summary>
    [ContextMenu("Test Task Completion Camera")]
    public void TestTaskCompletionCamera()
    {
        if (taskCompletionTarget != null)
        {
            SetFollowTarget(taskCompletionTarget);
        }
        SetCameraSize(taskCompletionCameraSize);
        Debug.Log("测试任务完成相机效果");
    }

    /// <summary>
    /// 调试方法：重置相机到默认状态
    /// </summary>
    [ContextMenu("Reset Camera to Default")]
    public void ResetCameraToDefault()
    {
        ResetToDefaultTarget();
        ResetCameraSize();
        Debug.Log("相机重置到默认状态");
    }

    /// <summary>
    /// 获取当前相机大小
    /// </summary>
    public float GetCurrentCameraSize()
    {
        return virtualCamera != null ? virtualCamera.m_Lens.OrthographicSize : 0f;
    }
}
