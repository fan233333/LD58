using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CheckSpecificObject : MonoBehaviour
{
    [Header("吸引设置")]
    [SerializeField] private string targetTag = "ReadyToFall"; // 要检测的物体标签
    [SerializeField] private string parentName = "BagParent"; // 父物体名称
    [SerializeField] private string typeName = "Triangle"; // 父物体名称
    [SerializeField] private Transform attract;
    [SerializeField] private float attractSpeed = 2f; // 吸引速度
    public float layerSpacing = 1f; // 层间距
    public float delayBetweenLayers = 0.5f; // 层间延迟
    //public ContainerManager containerManager;
    public TaskManager taskManager;

    public static bool gainEnough = false;

    private List<GameObject> childrenToAttract = new List<GameObject>();
    private bool isAbsorbing = false;


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


        // 获取所有子物体
        foreach (Transform child in parentObj.transform)
        {
            CollectibleItem collectibleItem = child.GetComponent<CollectibleItem>();
            if(collectibleItem.GetTypeKey() == typeName)
            {
                childrenToAttract.Add(child.gameObject);
                Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                if(typeName == "Square")
                {
                    IceOre iceOre = child.GetComponent<IceOre>();
                    if(iceOre != null)
                    {
                        iceOre.isShrinking = false;
                        child.tag = "Untagged";
                    }
                    
                }

            }
        }

        if (childrenToAttract.Count > 0 && !taskManager.CheckItemFull(typeName))
        {
            isAbsorbing = true;
            
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
        yield return new WaitForSeconds(0.5f);
        if (!StopAreaDetector.playerInArea)
        {
            childrenToAttract.Clear();
            yield break;
        }
        yield return null;
        Debug.Log($"开始吸引 {childrenToAttract.Count} 个子物体");

        childrenToAttract = childrenToAttract.Where(obj => obj != null).ToList();
        childrenToAttract.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));

        isAbsorbing = true;

        // 计算分层
        var layers = GroupObjectsIntoLayers();

        // 按层依次吸收
        for (int i = 0; i < layers.Count; i++)
        {
            yield return StartCoroutine(AbsorbLayer(layers[i], i));
            yield return new WaitForSeconds(delayBetweenLayers);
        }

        isAbsorbing = false;
        gainEnough = true;
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
        float targetY = attract.position.y;// + (layerIndex * layerSpacing);

        // 为该层所有物体启动吸收协程
        foreach (var obj in layerObjects)
        {
            if (obj != null)
            {
                Coroutine coroutine = StartCoroutine(MoveObjectToHeight(obj, targetY));
                coroutines.Add(coroutine);
            }
            
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
            CollectibleItem collectibleItem = obj.GetComponent<CollectibleItem>();
            if (collectibleItem != null)
            {
                if (collectibleItem.GetTypeKey() == typeName)
                {
                    Debug.Log($"ItemCollected{typeName}");
                    //containerManager.CreateObject(obj.gameObject, typeName);
                    taskManager.ItemCollected(obj.gameObject, typeName);
                }
                //if (collectibleItem.GetTypeKey() == "Triangle")
                //{
                //    containerManager.CreateTriangle(obj.gameObject);
                //    taskManager.ItemCollected("Circle");
                //}
                float mass = collectibleItem.mass;
                float totalMass = ItemStatistics.Instance.GetTotalMass();
                ItemStatistics.Instance.SetTotalMass(totalMass - mass);
            }
            // 销毁游戏对象
            Destroy(obj.gameObject);

            // 如果需要，可以在这里添加销毁特效、声音等
            // PlayDestroyEffect(obj.position);
        }

        layerObjects.Clear();
    }

    // 移动物体到指定高度
    IEnumerator MoveObjectToHeight(Transform obj, float targetHeight)
    {
        Vector2 startPos = obj.position;
        Vector2 targetPos = new Vector2(obj.position.x, targetHeight);

        // 获取物体的 Rigidbody2D 组件
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning($"物体 {obj.name} 没有 Rigidbody2D 组件，无法进行物理移动");
            yield break;
        }

        // 确保 Rigidbody2D 设置为动态类型，以便响应物理
        rb.bodyType = RigidbodyType2D.Dynamic;
        float journey = 0f;

        // 设置速度或力来移动物体
        while (journey <= 3f)
        {
            if(obj != null)
            {
                journey += Time.deltaTime * attractSpeed;
                // 方法1：使用速度直接移动
                Vector2 direction = (targetPos - (Vector2)obj.position).normalized;
                rb.velocity = direction * attractSpeed;

                // 如果物体卡住，可以添加一个小力来推动
                if (rb.velocity.magnitude < 0.1f)
                {
                    rb.AddForce(direction * 10f);
                }

                yield return new WaitForFixedUpdate();
            }
            
        }

        // 到达目标位置后停止移动
        rb.velocity = Vector2.zero;

        // 可选：将物体设置为静态，防止后续移动
        rb.bodyType = RigidbodyType2D.Kinematic;
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