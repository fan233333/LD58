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
    
    private bool hasTaskCompleted = false;
    private bool hasTaskFailed = false;

    void Start()
    {
        // 初始化
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
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
}
