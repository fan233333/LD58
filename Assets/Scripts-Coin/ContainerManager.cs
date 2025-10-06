using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ContainerManager : MonoBehaviour
{
    [Header("容器位置")]
    public Transform ballContainer;
    public Transform triangleContainer;
    public Transform IceContainer;
    public Transform LavaContainer;
    public Transform BuleContainer;

    [Header("父物体")]
    public Transform ballParent;
    public Transform triangleParent;
    public Transform LavaParent;
    public Transform IceParent;
    public Transform BuleParent;

    [Header("力")]
    public float attractSpeed = 2f;
    public float minHorizontalForce = -2f;
    public float maxHorizontalForce = 2f;
    public float delayBetweenLayers = 0.5f; // 层间延迟

    public TaskManager taskManager;

    private List<GameObject> childrenToAttract = new List<GameObject>();
    private bool isAttracting = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            StartAttractionProcess(ballParent, ballContainer.transform);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            StartAttractionProcess(triangleParent, triangleContainer.transform);
        }
    }

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
        GameObject newBall = Instantiate(obj, ballContainer.position, Quaternion.identity);
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
        GameObject newBall = Instantiate(obj, ballContainer.position, Quaternion.identity);
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
        GameObject newBall = Instantiate(obj, ballContainer.position, Quaternion.identity);
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

    public void StartAttractionProcess(Transform parent, Transform target)
    {

        // 获取所有子物体
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

    // 吸引并销毁子物体的协程
    private IEnumerator AttractAndDestroyChildren(Transform target)
    {

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
            yield return StartCoroutine(AbsorbLayer(layers[i], target));
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
        //float heightThreshold = 0.15f; // 高度阈值，可根据需要调整

        List<Transform> currentLayer = new List<Transform>();
        float lastHeight = childrenToAttract[0].transform.position.y;

        foreach (var obj in childrenToAttract)
        {
            float heightThreshold = Random.Range(0.05f, 0.3f);
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

    IEnumerator AbsorbLayer(List<Transform> layerObjects, Transform targetPos)
    {
        List<Coroutine> coroutines = new List<Coroutine>();

        // 计算该层的目标高度
        float targetY = targetPos.position.y;
        float targetX = targetPos.position.x;

        // 为该层所有物体启动吸收协程
        foreach (var obj in layerObjects)
        {
            if (obj == null) continue;

            Coroutine coroutine = StartCoroutine(MoveObjectToHeight(obj, targetY, targetX));
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
            if (obj != null)
            {
                Destroy(obj.gameObject);
            }


            // 如果需要，可以在这里添加销毁特效、声音等
            // PlayDestroyEffect(obj.position);
        }

        layerObjects.Clear();
    }

    // 移动物体到指定高度
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
}
