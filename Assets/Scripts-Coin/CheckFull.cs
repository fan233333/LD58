using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CheckFull: MonoBehaviour
{
    [Header("吸引设置")]
    [SerializeField] private string targetTag = "ReadyToFall"; // 要检测的物体标签
    [SerializeField] private string parentName = "BagParent"; // 父物体名称
    [SerializeField] private Transform attract;
    [SerializeField] private float attractSpeed = 2f; // 吸引速度
    [SerializeField] private float destroyDelay = 0.5f; // 删除延迟
    public float layerSpacing = 1f; // 层间距
    public float delayBetweenLayers = 0.5f; // 层间延迟
    public static int attractCount = 0;

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


            if (child.transform.position.y >= transform.position.y)
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
        yield return new WaitForSeconds(1f);
        if (!StopAreaDetector.playerInArea)
        {
            childrenToAttract.Clear();
            yield break;
        }
        yield return null;
        Debug.Log($"开始吸引 {childrenToAttract.Count} 个子物体");

        childrenToAttract = childrenToAttract.Where(obj => obj != null).ToList();
        childrenToAttract.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));

        isAttracting = true;

        // 计算分层
        var layers = GroupObjectsIntoLayers();
        Debug.Log(layers.Count);

        // 按层依次吸收
        for (int i = 0; i < layers.Count; i++)
        {
            yield return StartCoroutine(AbsorbLayer(layers[i], i));
            yield return new WaitForSeconds(delayBetweenLayers);
        }

        isAttracting = false;
        Debug.Log("所有子物体已被吸引并删除");


        // 可选：这里可以添加删除父物体的代码
        // Destroy(GameObject.Find(parentName));
    }

    List<List<Transform>> GroupObjectsIntoLayers()
    {
        List<List<Transform>> layers = new List<List<Transform>>();

        if (childrenToAttract.Count == 0) return layers;

        // 使用阈值进行分层
        float heightThreshold = 0.4f; // 高度阈值，可根据需要调整

        List<Transform> currentLayer = new List<Transform>();
        float lastHeight = childrenToAttract[0].transform.position.y;

        foreach (var obj in childrenToAttract)
        {

            if (Mathf.Abs(obj.transform.position.y - lastHeight) > heightThreshold)
            {
                // 开始新的一层
                if (currentLayer.Count > 0)
                {
                    layers.Add(new List<Transform>(currentLayer));
                    currentLayer.Clear();
                }
            }

            currentLayer.Add(obj.transform);
            lastHeight = obj.transform.position.y;
        }

        // 添加最后一层
        if (currentLayer.Count > 0)
        {
            layers.Add(currentLayer);
        }

        return layers;
    }

    // 吸收单层物体
    IEnumerator AbsorbLayer(List<Transform> layerObjects, int layerIndex)
    {
        List<Coroutine> coroutines = new List<Coroutine>();

        // 计算该层的目标高度
        float targetY = attract.position.y;

        // 为该层所有物体启动吸收协程
        foreach (var obj in layerObjects)
        {
            if(obj == null) continue;
            Coroutine coroutine = StartCoroutine(MoveObjectToHeight(obj, targetY));
            coroutines.Add(coroutine);

        }

        // 等待该层所有物体吸收完成
        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }
        DestroyLayerObjects(layerObjects);
    }

    void DestroyLayerObjects(List<Transform> layerObjects)
    {
        foreach (var obj in layerObjects)
        {
            // 销毁游戏对象
            if(obj != null)
            {
                Destroy(obj.gameObject);
            }
            

            // 如果需要，可以在这里添加销毁特效、声音等
            // PlayDestroyEffect(obj.position);
        }

        layerObjects.Clear();
    }

    // 移动物体到指定高度
    IEnumerator MoveObjectToHeight(Transform obj, float targetHeight)
    {
        if (obj == null) yield break;

        Vector3 startPos = obj.position;
        Vector3 targetPos = new Vector3(obj.position.x, targetHeight, obj.position.z);
        float journey = 0f;

        while (obj != null && journey <= 1f)
        {
            journey += Time.deltaTime * attractSpeed;
            obj.position = Vector3.Lerp(startPos, targetPos, journey);
            yield return null;
        }

        if (obj != null)
        {
            obj.position = targetPos;
        }
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