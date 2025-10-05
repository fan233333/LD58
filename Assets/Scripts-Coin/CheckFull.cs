using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CheckFull: MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private string targetTag = "ReadyToFall"; // Ҫ���������ǩ
    [SerializeField] private string parentName = "BagParent"; // ����������
    [SerializeField] private Transform attract;
    [SerializeField] private float attractSpeed = 2f; // �����ٶ�
    [SerializeField] private float destroyDelay = 0.5f; // ɾ���ӳ�
    public float layerSpacing = 1f; // ����
    public float delayBetweenLayers = 0.5f; // ����ӳ�
    public static int attractCount = 0;

    private List<GameObject> childrenToAttract = new List<GameObject>();
    private bool isAttracting = false;

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

        bool full = false;

        // ��ȡ����������
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
        yield return new WaitForSeconds(1f);
        if (!StopAreaDetector.playerInArea)
        {
            childrenToAttract.Clear();
            yield break;
        }
        yield return null;
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
            yield return StartCoroutine(AbsorbLayer(layers[i], i));
            yield return new WaitForSeconds(delayBetweenLayers);
        }

        isAttracting = false;
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
        float targetY = attract.position.y;

        // Ϊ�ò�����������������Э��
        foreach (var obj in layerObjects)
        {
            if(obj == null) continue;
            Coroutine coroutine = StartCoroutine(MoveObjectToHeight(obj, targetY));
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
            if(obj != null)
            {
                Destroy(obj.gameObject);
            }
            

            // �����Ҫ���������������������Ч��������
            // PlayDestroyEffect(obj.position);
        }

        layerObjects.Clear();
    }

    // �ƶ����嵽ָ���߶�
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