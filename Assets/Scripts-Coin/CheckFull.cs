using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckFull: MonoBehaviour
{
    [Header("吸引设置")]
    [SerializeField] private string targetTag = "ReadyToFall"; // 要检测的物体标签
    [SerializeField] private string parentName = "BagParent"; // 父物体名称
    [SerializeField] private Transform attract;
    [SerializeField] private float attractSpeed = 2f; // 吸引速度
    [SerializeField] private float destroyDelay = 0.5f; // 删除延迟

    private List<GameObject> childrenToAttract = new List<GameObject>();
    private bool isAttracting = false;

    // 触发器检测
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
    //
        // 检测到指定标签的物体且未开始吸引过程
     //  if (collision.CompareTag(targetTag) && !isAttracting && rb.velocity.y >= 0)
     //   {
     //       Debug.Log($"检测到物体 {collision.gameObject.name} 进入触发区域");
     //       StartAttractionProcess();
      //  }
    //}

    // 开始吸引过程
    public void StartAttractionProcess()
    {
        //StartCoroutine(Wait());

        GameObject parentObj = GameObject.Find(parentName);
        if (parentObj == null)
        {
            Debug.LogError($"未找到名为 {parentName} 的父物体!");
            return;
        }

        bool full = false;

        // 获取所有子物体
        foreach (Transform child in parentObj.transform)
        {
            childrenToAttract.Add(child.gameObject);
            if(child.transform.position.y >= transform.position.y)
            {
                full = true;
            }
        }

        if (childrenToAttract.Count > 0 && full)
        {
            isAttracting = true;
            StartCoroutine(AttractAndDestroyChildren());
        }
        else
        {
            Debug.LogWarning($"父物体 {parentName} 下没有子物体!");
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(2f);
    }

    // 吸引并销毁子物体的协程
    private IEnumerator AttractAndDestroyChildren()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log($"开始吸引 {childrenToAttract.Count} 个子物体");

        // 禁用所有子物体的物理效果和碰撞体
        foreach (GameObject child in childrenToAttract)
        {
            if (child != null)
            {
                Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
                if (rb != null) rb.simulated = false;

                Collider2D collider = child.GetComponent<Collider2D>();
                if (collider != null) collider.enabled = false;
            }
        }

        // 吸引过程：将所有子物体移动到目标位置
        bool allReachedTarget = false;
        while (!allReachedTarget)
        {
            allReachedTarget = true;
            Vector2 attractPosition = attract.position;

            foreach (GameObject child in childrenToAttract)
            {
                if (child == null) continue;

                Vector2 currentPos = child.transform.position;
                float distance = Vector2.Distance(currentPos, attractPosition);

                if (distance > 0.1f)
                {
                    child.transform.position = Vector2.MoveTowards(
                        currentPos, attractPosition, attractSpeed * Time.deltaTime);
                    allReachedTarget = false;
                }
            }

            yield return null;
        }

        // 等待一段时间后销毁所有子物体
        yield return new WaitForSeconds(destroyDelay);

        foreach (GameObject child in childrenToAttract)
        {
            if (child != null)
            {
                Destroy(child);
            }
        }

        childrenToAttract.Clear();
        isAttracting = false;

        Debug.Log("所有子物体已被吸引并删除");

        // 可选：这里可以添加删除父物体的代码
        // Destroy(GameObject.Find(parentName));
    }

    // 在Scene视图中可视化目标位置
    private void OnDrawGizmosSelected()
    {
        Vector2 attractPosition = attract.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attractPosition, 0.3f);

        // 绘制从触发器到目标位置的连线
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, attractPosition);
    }
}