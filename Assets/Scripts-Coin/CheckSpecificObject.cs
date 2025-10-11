using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CheckSpecificObject : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private string targetTag = "ReadyToFall"; // Ҫ���������ǩ
    [SerializeField] private string parentName = "BagParent"; // ����������
    [SerializeField] private string typeName = "Triangle"; // ����������
    [SerializeField] private Transform attract;
    [SerializeField] private float attractSpeed = 2f; // �����ٶ�
    public float layerSpacing = 1f; // ����
    public float delayBetweenLayers = 0.5f; // ����ӳ�
    //public ContainerManager containerManager;
    public TaskManager taskManager;

    public static bool gainEnough = false;

    private List<GameObject> childrenToAttract = new List<GameObject>();
    private bool isAbsorbing = false;


    // ���������
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
    //
    // ��⵽ָ����ǩ��������δ��ʼ��������
    //  if (collision.CompareTag(targetTag) && !isAttracting && rb.velocity.y >= 0)
    //   {
    //       Debug.Log($"��⵽���� {collision.gameObject.name} ���봥������");
    //       StartAttractionProcess();
    //  }
    //}

    // ��ʼ��������
    public void StartAttractionProcess()
    {
        //StartCoroutine(Wait());

        GameObject parentObj = GameObject.Find(parentName);
        if (parentObj == null)
        {
            Debug.LogError($"δ�ҵ���Ϊ {parentName} �ĸ�����!");
            return;
        }


        // ��ȡ����������
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
            Debug.LogWarning($"������ {parentName} ��û��������!");
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(2f);
    }

    // �����������������Э��
    private IEnumerator AttractAndDestroyChildren()
    {
        yield return new WaitForSeconds(0.5f);
        if (!StopAreaDetector.playerInArea)
        {
            childrenToAttract.Clear();
            yield break;
        }
        yield return null;
        Debug.Log($"��ʼ���� {childrenToAttract.Count} ��������");

        childrenToAttract = childrenToAttract.Where(obj => obj != null).ToList();
        childrenToAttract.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));

        isAbsorbing = true;

        // ����ֲ�
        var layers = GroupObjectsIntoLayers();

        // ������������
        for (int i = 0; i < layers.Count; i++)
        {
            yield return StartCoroutine(AbsorbLayer(layers[i], i));
            yield return new WaitForSeconds(delayBetweenLayers);
        }

        isAbsorbing = false;
        gainEnough = true;
        Debug.Log("�����������ѱ�������ɾ��");


        // ��ѡ������������ɾ��������Ĵ���
        // Destroy(GameObject.Find(parentName));
    }

    List<List<Transform>> GroupObjectsIntoLayers()
    {
        List<List<Transform>> layers = new List<List<Transform>>();

        if (childrenToAttract.Count == 0) return layers;

        // ʹ����ֵ���зֲ�
        float heightThreshold = 0.4f; // �߶���ֵ���ɸ�����Ҫ����

        List<Transform> currentLayer = new List<Transform>();
        float lastHeight = childrenToAttract[0].transform.position.y;

        foreach (var obj in childrenToAttract)
        {
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

        // ������һ��
        if (currentLayer.Count > 0)
        {
            layers.Add(currentLayer);
        }

        return layers;
    }

    // ���յ�������
    IEnumerator AbsorbLayer(List<Transform> layerObjects, int layerIndex)
    {
        List<Coroutine> coroutines = new List<Coroutine>();

        // ����ò��Ŀ��߶�
        float targetY = attract.position.y;// + (layerIndex * layerSpacing);

        // Ϊ�ò�����������������Э��
        foreach (var obj in layerObjects)
        {
            if (obj != null)
            {
                Coroutine coroutine = StartCoroutine(MoveObjectToHeight(obj, targetY));
                coroutines.Add(coroutine);
            }
            
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
            // ������Ϸ����
            Destroy(obj.gameObject);

            // �����Ҫ���������������������Ч��������
            // PlayDestroyEffect(obj.position);
        }

        layerObjects.Clear();
    }

    // �ƶ����嵽ָ���߶�
    IEnumerator MoveObjectToHeight(Transform obj, float targetHeight)
    {
        Vector2 startPos = obj.position;
        Vector2 targetPos = new Vector2(obj.position.x, targetHeight);

        // ��ȡ����� Rigidbody2D ���
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning($"���� {obj.name} û�� Rigidbody2D ������޷����������ƶ�");
            yield break;
        }

        // ȷ�� Rigidbody2D ����Ϊ��̬���ͣ��Ա���Ӧ����
        rb.bodyType = RigidbodyType2D.Dynamic;
        float journey = 0f;

        // �����ٶȻ������ƶ�����
        while (journey <= 3f)
        {
            if(obj != null)
            {
                journey += Time.deltaTime * attractSpeed;
                // ����1��ʹ���ٶ�ֱ���ƶ�
                Vector2 direction = (targetPos - (Vector2)obj.position).normalized;
                rb.velocity = direction * attractSpeed;

                // ������忨ס���������һ��С�����ƶ�
                if (rb.velocity.magnitude < 0.1f)
                {
                    rb.AddForce(direction * 10f);
                }

                yield return new WaitForFixedUpdate();
            }
            
        }

        // ����Ŀ��λ�ú�ֹͣ�ƶ�
        rb.velocity = Vector2.zero;

        // ��ѡ������������Ϊ��̬����ֹ�����ƶ�
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    // ��Scene��ͼ�п��ӻ�Ŀ��λ��
    private void OnDrawGizmosSelected()
    {
        Vector2 attractPosition = attract.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attractPosition, 0.3f);

        // ���ƴӴ�������Ŀ��λ�õ�����
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, attractPosition);
    }
}