using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ContainerManager : MonoBehaviour
{
    [Header("��")]
    public GameObject ball;
    public GameObject triangle;
    public GameObject ice;
    public GameObject lava;
    public GameObject blue;

    [Header("����λ��")]
    public Transform ballContainer;
    public Transform triangleContainer;
    public Transform IceContainer;
    public Transform LavaContainer;
    public Transform BuleContainer;

    [Header("������")]
    public Transform ballParent;
    public Transform triangleParent;
    public Transform LavaParent;
    public Transform IceParent;
    public Transform BuleParent;

    [Header("��")]
    public float attractSpeed = 2f;
    public float minHorizontalForce = -2f;
    public float maxHorizontalForce = 2f;
    public float delayBetweenLayers = 0.5f; // ����ӳ�

    public TaskManager taskManager;
    public static bool isAllAttracted = false;

    public float size = 0.5f;

    private List<GameObject> childrenToAttract = new List<GameObject>();
    private bool isAttracting = false;

    public float globalSizeMultiplier = 0.3f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    // void Update()
    // {

    //     if (Input.GetKeyDown(KeyCode.Z)){
    //         CreateBall(ball);
    //     }
    //     if (Input.GetKeyDown(KeyCode.X))
    //     {
    //         CreateTriangle(triangle);
    //     }
    //     if (Input.GetKeyDown(KeyCode.C))
    //     {
    //         CreateIce(ice);
    //     }
    //     if (Input.GetKeyDown(KeyCode.V))
    //     {
    //         CreateLava(lava);
    //     }
    //     if (Input.GetKeyDown(KeyCode.B))
    //     {
    //         CreateBlue(blue);
    //     }
    //     //if (Input.GetKeyDown(KeyCode.O))
    //     //{
    //     //    StartAttractionProcess(ballParent, ballContainer.transform);
    //     //}

    //     //if (Input.GetKeyDown(KeyCode.L))
    //     //{
    //     //    StartAttractionProcess(triangleParent, triangleContainer.transform);
    //     //}
    // }

    public void CreateObject(GameObject obj, string typeName)
    {
        if(typeName == "Circle")
        {
            CreateBall(obj);
        }
        if(typeName == "Triangle")
        {
            CreateTriangle(obj);
        }
        if (typeName == "Square")
        {
            CreateIce(obj);
        }
        if(typeName == "Hexagon")
        {
            CreateLava(obj);
        }
        if(typeName == "Diamond")
        {
            CreateBlue(obj);
        }
    }

    public void CreateBall(GameObject obj)
    {
        GameObject newBall = Instantiate(obj, ballContainer.position, Quaternion.identity);
        newBall.layer = 0;
        newBall.transform.tag = "Untagged";
        // newBall.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        newBall.transform.localScale = obj.transform.localScale / size;
        newBall.transform.SetParent(ballParent);
        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();
        float dynamicScale = taskManager.GetItemScale("Circle");
        Vector3 currentScale = newBall.transform.localScale;
        newBall.transform.localScale = new Vector3(globalSizeMultiplier * dynamicScale * currentScale.x, globalSizeMultiplier * dynamicScale * currentScale.y, globalSizeMultiplier * dynamicScale * currentScale.z);
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            float randomForce = Random.Range(minHorizontalForce, maxHorizontalForce);
            rb.AddForce(new Vector2(randomForce, 0), ForceMode2D.Impulse);
        }
    }

    public void CreateTriangle(GameObject obj)
    {
        GameObject newBall = Instantiate(obj, triangleContainer.position, Quaternion.identity);
        newBall.layer = 0;
        newBall.transform.tag = "Untagged";
        // newBall.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        newBall.transform.localScale = obj.transform.localScale / size;
        newBall.transform.SetParent(triangleParent);
        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();
        float dynamicScale = taskManager.GetItemScale("Triangle");
        Vector3 currentScale = newBall.transform.localScale;
        newBall.transform.localScale = new Vector3(globalSizeMultiplier * dynamicScale * currentScale.x, globalSizeMultiplier * dynamicScale * currentScale.y, globalSizeMultiplier * dynamicScale * currentScale.z);
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            float randomForce = Random.Range(minHorizontalForce, maxHorizontalForce);
            rb.AddForce(new Vector2(randomForce, 0), ForceMode2D.Impulse);
        }
    }
    public void CreateIce(GameObject obj)
    {
        GameObject newBall = Instantiate(obj, IceContainer.position, Quaternion.identity);
        newBall.layer = 0;
        newBall.transform.tag = "Untagged";
        // newBall.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        newBall.transform.localScale = obj.transform.localScale / size;
        newBall.transform.SetParent(IceParent);
        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();
        float dynamicScale = taskManager.GetItemScale("Square");
        Vector3 currentScale = newBall.transform.localScale;
        newBall.transform.localScale = new Vector3(globalSizeMultiplier * dynamicScale * currentScale.x, globalSizeMultiplier * dynamicScale * currentScale.y, globalSizeMultiplier * dynamicScale * currentScale.z);
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            float randomForce = Random.Range(minHorizontalForce, maxHorizontalForce);
            rb.AddForce(new Vector2(randomForce, 0), ForceMode2D.Impulse);
        }
    }
    public void CreateLava(GameObject obj)
    {
        GameObject newBall = Instantiate(obj, LavaContainer.position, Quaternion.identity);
        newBall.layer = 0;
        newBall.transform.tag = "Untagged";
        // newBall.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        newBall.transform.localScale = obj.transform.localScale / size;
        newBall.transform.SetParent(LavaParent);
        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();
        float dynamicScale = taskManager.GetItemScale("Hexagon");
        Vector3 currentScale = newBall.transform.localScale;
        newBall.transform.localScale = new Vector3(globalSizeMultiplier * dynamicScale * currentScale.x, globalSizeMultiplier * dynamicScale * currentScale.y, globalSizeMultiplier * dynamicScale * currentScale.z);
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            float randomForce = Random.Range(minHorizontalForce, maxHorizontalForce);
            rb.AddForce(new Vector2(randomForce, 0), ForceMode2D.Impulse);
        }
    }

    public void CreateBlue(GameObject obj)
    {
        Debug.Log("CreateBlue");
        GameObject newBall = Instantiate(obj, BuleContainer.position, Quaternion.identity);
        newBall.layer = 0;
        newBall.transform.tag = "Untagged";
        // newBall.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        newBall.transform.localScale = obj.transform.localScale / size;
        newBall.transform.SetParent(BuleParent);
        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();
        float dynamicScale = taskManager.GetItemScale("Diamond");
        Vector3 currentScale = newBall.transform.localScale;
        newBall.transform.localScale = new Vector3(globalSizeMultiplier * dynamicScale * currentScale.x, globalSizeMultiplier * dynamicScale * currentScale.y, globalSizeMultiplier * dynamicScale * currentScale.z);
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            float randomForce = Random.Range(minHorizontalForce, maxHorizontalForce);
            rb.AddForce(new Vector2(randomForce, 0), ForceMode2D.Impulse);
        }
    }


    public void AttrackAll()
    {
        // 并行启动所有吸引过程
        StartCoroutine(StartAllAttractionProcesses());
    }

    private IEnumerator StartAllAttractionProcesses()
    {
        isAllAttracted = false;
        // 创建所有吸引过程的协程列表
        List<Coroutine> attractionCoroutines = new List<Coroutine>();

        // 并行启动所有吸引过程
        attractionCoroutines.Add(StartCoroutine(StartAttractionProcessAsync(ballParent, ballContainer.transform)));
        attractionCoroutines.Add(StartCoroutine(StartAttractionProcessAsync(triangleParent, triangleContainer.transform)));
        attractionCoroutines.Add(StartCoroutine(StartAttractionProcessAsync(IceParent, IceContainer.transform)));
        attractionCoroutines.Add(StartCoroutine(StartAttractionProcessAsync(LavaParent, LavaContainer.transform)));
        attractionCoroutines.Add(StartCoroutine(StartAttractionProcessAsync(BuleParent, BuleContainer.transform)));

        // 等待所有吸引过程完成
        foreach (var coroutine in attractionCoroutines)
        {
            yield return coroutine;
        }
        isAllAttracted = true;

        Debug.Log("所有吸引过程已完成");
    }

    // 异步版本的吸引过程
    public IEnumerator StartAttractionProcessAsync(Transform parent, Transform target)
    {
        // 每个过程有自己的对象列表，不共享全局列表
        List<GameObject> childrenToAttract = new List<GameObject>();

        // 获取父对象的所有子对象
        foreach (Transform child in parent)
        {
            childrenToAttract.Add(child.gameObject);
        }

        if (childrenToAttract.Count > 0)
        {
            yield return StartCoroutine(AttractAndDestroyChildrenAsync(childrenToAttract, target));
        }
    }

    // 异步版本的吸引和销毁协程
    private IEnumerator AttractAndDestroyChildrenAsync(List<GameObject> childrenToAttract, Transform target)
    {
        Debug.Log($"开始吸引 {childrenToAttract.Count} 个物体到 {target.name}");

        // 过滤空对象并按高度排序
        childrenToAttract = childrenToAttract.Where(obj => obj != null).ToList();
        childrenToAttract.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));

        // 分层处理
        var layers = GroupObjectsIntoLayers(childrenToAttract);
        Debug.Log($"生成 {layers.Count} 层");

        // 逐层处理
        for (int i = 0; i < layers.Count; i++)
        {
            yield return StartCoroutine(AbsorbLayerAsync(layers[i], target));
            yield return new WaitForSeconds(delayBetweenLayers);
        }

        Debug.Log($"吸引过程完成，目标: {target.name}");
    }

    // 修改分层方法，接收参数
    List<List<Transform>> GroupObjectsIntoLayers(List<GameObject> childrenToAttract)
    {
        List<List<Transform>> layers = new List<List<Transform>>();

        if (childrenToAttract.Count == 0) return layers;

        // 转换为Transform列表并按高度排序
        List<Transform> transforms = childrenToAttract
            .Where(obj => obj != null)
            .Select(obj => obj.transform)
            .OrderByDescending(t => t.position.y)
            .ToList();

        List<Transform> currentLayer = new List<Transform>();
        float lastHeight = transforms[0].position.y;

        foreach (var transform in transforms)
        {
            float heightThreshold = Random.Range(0.05f, 0.3f);
            if (Mathf.Abs(transform.position.y - lastHeight) > heightThreshold)
            {
                // 开始新的一层
                if (currentLayer.Count > 0)
                {
                    layers.Add(new List<Transform>(currentLayer));
                    currentLayer.Clear();
                }
            }

            currentLayer.Add(transform);
            lastHeight = transform.position.y;
        }

        // 添加最后一层
        if (currentLayer.Count > 0)
        {
            layers.Add(currentLayer);
        }

        return layers;
    }

    // 异步版本的吸收层
    IEnumerator AbsorbLayerAsync(List<Transform> layerObjects, Transform targetPos)
    {
        List<Coroutine> coroutines = new List<Coroutine>();

        float targetY = targetPos.position.y;
        float targetX = targetPos.position.x;

        Debug.Log($"吸引层物体到位置: X={targetX}, Y={targetY}, 目标: {targetPos.name}");

        // 为层中所有物体启动移动协程
        foreach (var obj in layerObjects)
        {
            if (obj == null) continue;

            Coroutine coroutine = StartCoroutine(MoveObjectToHeightAsync(obj, targetY, targetX));
            coroutines.Add(coroutine);
        }

        // 等待该层所有物体移动完成
        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }

        // 销毁该层物体
        DestroyLayerObjects(layerObjects);
    }

    // 异步版本的移动物体
    IEnumerator MoveObjectToHeightAsync(Transform obj, float targetHeight, float targetX)
    {
        if (obj == null) yield break;

        Vector3 startPos = obj.position;
        Vector3 targetPos = new Vector3(targetX, targetHeight, obj.position.z);
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

    // 销毁层物体方法保持不变
    void DestroyLayerObjects(List<Transform> layerObjects)
    {
        foreach (var obj in layerObjects)
        {
            if (obj != null)
            {
                Destroy(obj.gameObject);
            }
        }
        layerObjects.Clear();
    }

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Z))
    //     {
    //         CreateItemManually("Circle", ball);
    //     }
    //     if (Input.GetKeyDown(KeyCode.X))
    //     {
    //         CreateItemManually("Triangle", triangle);
    //     }
    //     if (Input.GetKeyDown(KeyCode.C))
    //     {
    //         CreateItemManually("Square", ice);
    //     }
    //     if (Input.GetKeyDown(KeyCode.V))
    //     {
    //         CreateItemManually("Hexagon", lava);
    //     }
    //     if (Input.GetKeyDown(KeyCode.B))
    //     {
    //         CreateItemManually("Diamond", blue);
    //     }
    // }

    /// <summary>
    /// 手动创建物品并更新任务进度
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <param name="prefab">预制体</param>
    public void CreateItemManually(string itemType, GameObject prefab)
    {
        // 检查任务是否已完成
        if (taskManager.CheckItemFull(itemType))
        {
            Debug.Log($"{itemType} 任务已完成，无法继续添加");
            return;
        }
        
        // 创建物体
        // CreateObject(prefab, itemType);
        
        // 更新任务进度
        taskManager.ItemCollected(prefab, itemType);
    }
}
