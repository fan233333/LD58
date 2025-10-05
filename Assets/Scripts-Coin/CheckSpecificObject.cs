using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CheckSpecificObject : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private string targetTag = "ReadyToFall"; // Ҫ���������ǩ
    [SerializeField] private string parentName = "BagParent"; // ����������
    [SerializeField] private string typeName = "Triangle"; // ����������
    [SerializeField] private Transform attract;
    [SerializeField] private float attractSpeed = 2f; // �����ٶ�
    [SerializeField] private float destroyDelay = 0.5f; // ɾ���ӳ�

    public static bool gainEnough = false;

    private List<GameObject> childrenToAttract = new List<GameObject>();
    private List<GameObject> otherChildren = new List<GameObject>();
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


        // ��ȡ����������
        foreach (Transform child in parentObj.transform)
        {
            CollectibleItem collectibleItem = child.GetComponent<CollectibleItem>();
            if(collectibleItem.GetTypeKey() == typeName)
            {
                childrenToAttract.Add(child.gameObject);
            }
            else
            {
                otherChildren.Add(child.gameObject);
            }
            
            
        }

        if (childrenToAttract.Count > 0)
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
        yield return new WaitForSeconds(2f);
        if (!StopAreaDetector.playerInArea)
        {
            childrenToAttract.Clear();
            yield break;
        }
        yield return null;
        Debug.Log($"��ʼ���� {childrenToAttract.Count} ��������");

        // �������������������Ч������ײ��
        foreach (GameObject child in childrenToAttract)
        {
            if (child != null)
            {
                
                Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                //if (rb != null) rb.simulated = false;

                //Collider2D collider = child.GetComponent<Collider2D>();
                //if (collider != null) collider.enabled = false;
            }
        }

        // �������̣��������������ƶ���Ŀ��λ��
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

                if (attractPosition.y- child.transform.position.y > 1f)
                {
                    Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 direction = (attractPosition - currentPos).normalized;
                        rb.velocity = direction * attractSpeed;
                        allReachedTarget = false;
                    }

                }

            }

            yield return null;
        }

        // �ȴ�һ��ʱ�����������������
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
        gainEnough = true;
        Debug.Log("�����������ѱ�������ɾ��");
        foreach (GameObject child in otherChildren)
        {
            if (child != null)
            {

                Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
                rb.gravityScale = 1;
                //if (rb != null) rb.simulated = false;

                //Collider2D collider = child.GetComponent<Collider2D>();
                //if (collider != null) collider.enabled = false;
            }
        }

        // ��ѡ������������ɾ��������Ĵ���
        // Destroy(GameObject.Find(parentName));
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