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


    private List<GameObject> childrenToAttract = new List<GameObject>();
    private bool isAttracting = false;
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
        newBall.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        newBall.transform.SetParent(ballParent);
        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();
        float scale = taskManager.GetItemScale("Circle");
        newBall.transform.localScale = new Vector3(0.3f * scale, 0.3f * scale, 0.3f * scale);
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
        newBall.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        newBall.transform.SetParent(triangleParent);
        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();
        float scale = taskManager.GetItemScale("Triangle");
        newBall.transform.localScale = new Vector3(0.3f * scale, 0.3f * scale, 0.3f * scale);
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
        newBall.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        newBall.transform.SetParent(IceParent);
        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();
        float scale = taskManager.GetItemScale("Square");
        newBall.transform.localScale = new Vector3(0.3f * scale, 0.3f * scale, 0.3f * scale);
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
        newBall.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        newBall.transform.SetParent(LavaParent);
        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();
        float scale = taskManager.GetItemScale("Hexagon");
        newBall.transform.localScale = new Vector3(0.3f * scale, 0.3f * scale, 0.3f * scale);
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
        newBall.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        newBall.transform.SetParent(BuleParent);
        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();
        float scale = taskManager.GetItemScale("Diamond");
        newBall.transform.localScale = new Vector3(0.3f * scale, 0.3f * scale, 0.3f * scale);
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

        StartAttractionProcess(ballParent, ballContainer.transform);
        StartAttractionProcess(triangleParent, triangleContainer.transform);
        StartAttractionProcess(IceParent, IceContainer.transform);
        StartAttractionProcess(LavaParent, LavaContainer.transform);
        StartAttractionProcess(BuleParent, BuleContainer.transform);

    }


    public void StartAttractionProcess(Transform parent, Transform target)
    {

        // ��ȡ����������
        foreach (Transform child in parent)
        {
            childrenToAttract.Add(child.gameObject);

        }

        if (childrenToAttract.Count > 0)
        {
            isAttracting = true;
            StartCoroutine(AttractAndDestroyChildren(target));
        }

    }

    // �����������������Э��
    private IEnumerator AttractAndDestroyChildren(Transform target)
    {

        Debug.Log($"��ʼ���� {childrenToAttract.Count} ��������");

        childrenToAttract = childrenToAttract.Where(obj => obj != null).ToList();
        childrenToAttract.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));

        isAttracting = true;

        // ����ֲ�
        var layers = GroupObjectsIntoLayers();
        Debug.Log(layers.Count);

        // ������������
        for (int i = 0; i < layers.Count; i++)
        {
            yield return StartCoroutine(AbsorbLayer(layers[i], target));
            yield return new WaitForSeconds(delayBetweenLayers);
        }

        isAttracting = false;
        Debug.Log("�����������ѱ�������ɾ��");


        // ��ѡ�������������ɾ��������Ĵ���
        // Destroy(GameObject.Find(parentName));
    }

    List<List<Transform>> GroupObjectsIntoLayers()
    {
        List<List<Transform>> layers = new List<List<Transform>>();

        if (childrenToAttract.Count == 0) return layers;

        // ʹ����ֵ���зֲ�
        //float heightThreshold = 0.15f; // �߶���ֵ���ɸ�����Ҫ����

        List<Transform> currentLayer = new List<Transform>();
        float lastHeight = childrenToAttract[0].transform.position.y;

        foreach (var obj in childrenToAttract)
        {
            float heightThreshold = Random.Range(0.05f, 0.3f);
            if (Mathf.Abs(obj.transform.position.y - lastHeight) > heightThreshold)
            {
                // ��ʼ�µ�һ��
                if (currentLayer.Count > 0)
                {
                    layers.Add(new List<Transform>(currentLayer));
                    currentLayer.Clear();
                }
            }

            currentLayer.Add(obj.transform);
            lastHeight = obj.transform.position.y;
        }

        // �������һ��
        if (currentLayer.Count > 0)
        {
            layers.Add(currentLayer);
        }

        return layers;
    }

    IEnumerator AbsorbLayer(List<Transform> layerObjects, Transform targetPos)
    {
        List<Coroutine> coroutines = new List<Coroutine>();

        // ����ò��Ŀ��߶�
        float targetY = targetPos.position.y;
        float targetX = targetPos.position.x;

        // Ϊ�ò�����������������Э��
        foreach (var obj in layerObjects)
        {
            if (obj == null) continue;

            Coroutine coroutine = StartCoroutine(MoveObjectToHeight(obj, targetY, targetX));
            coroutines.Add(coroutine);

        }

        // �ȴ��ò����������������
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
            // ������Ϸ����
            if (obj != null)
            {
                Destroy(obj.gameObject);
            }


            // �����Ҫ����������������������Ч��������
            // PlayDestroyEffect(obj.position);
        }

        layerObjects.Clear();
    }

    // �ƶ����嵽ָ���߶�
    IEnumerator MoveObjectToHeight(Transform obj, float targetHeight, float targetX)
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            CreateItemManually("Circle", ball);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            CreateItemManually("Triangle", triangle);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreateItemManually("Square", ice);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            CreateItemManually("Hexagon", lava);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            CreateItemManually("Diamond", blue);
        }
    }

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
