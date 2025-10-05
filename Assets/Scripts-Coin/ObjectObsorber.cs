using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAbsorber : MonoBehaviour
{
    [Header("��������")]
    public Transform container; // ��������
    public float absorbHeight = 5f; // ���յ�Ŀ��߶�
    public float layerSpacing = 1f; // ����
    public float absorbSpeed = 2f; // �����ٶ�
    public float delayBetweenLayers = 0.5f; // ����ӳ�

    private List<Transform> objectsInContainer = new List<Transform>();
    private bool isAbsorbing = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isAbsorbing)
        {
            StartAbsorption();
        }
    }

    // ��ʼ���չ���
    public void StartAbsorption()
    {
        if (container == null) return;

        // ��ȡ����������������
        GetAllObjectsInContainer();

        // ���ݸ߶�����
        SortObjectsByHeight();

        // ��ʼ�ֲ�����
        StartCoroutine(AbsorbObjectsByLayer());
    }

    // ��ȡ��������������
    void GetAllObjectsInContainer()
    {
        objectsInContainer.Clear();
        foreach (Transform child in container)
        {
            if (child.gameObject.activeInHierarchy)
            {
                objectsInContainer.Add(child);
            }
        }
    }

    // ���ݸ߶����򣨴ӵ͵��ߣ�
    void SortObjectsByHeight()
    {
        objectsInContainer.Sort((a, b) => b.position.y.CompareTo(a.position.y));
    }

    // �ֲ�����Э��
    IEnumerator AbsorbObjectsByLayer()
    {
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
    }

    // ��������鵽��ͬ��
    List<List<Transform>> GroupObjectsIntoLayers()
    {
        List<List<Transform>> layers = new List<List<Transform>>();

        if (objectsInContainer.Count == 0) return layers;

        // ʹ����ֵ���зֲ�
        float heightThreshold = 0.4f; // �߶���ֵ���ɸ�����Ҫ����

        List<Transform> currentLayer = new List<Transform>();
        float lastHeight = objectsInContainer[0].position.y;

        foreach (var obj in objectsInContainer)
        {
            if (Mathf.Abs(obj.position.y - lastHeight) > heightThreshold)
            {
                // ��ʼ�µ�һ��
                if (currentLayer.Count > 0)
                {
                    layers.Add(new List<Transform>(currentLayer));
                    currentLayer.Clear();
                }
            }

            currentLayer.Add(obj);
            lastHeight = obj.position.y;
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
        float targetY = absorbHeight + (layerIndex * layerSpacing);

        // Ϊ�ò�����������������Э��
        foreach (var obj in layerObjects)
        {
            Coroutine coroutine = StartCoroutine(MoveObjectToHeight(obj, targetY));
            coroutines.Add(coroutine);
            Destroy(obj);
        }

        // �ȴ��ò����������������
        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }
    }

    // �ƶ����嵽ָ���߶�
    IEnumerator MoveObjectToHeight(Transform obj, float targetHeight)
    {
        Vector3 startPos = obj.position;
        Vector3 targetPos = new Vector3(obj.position.x, targetHeight, obj.position.z);
        float journey = 0f;

        while (journey <= 1f)
        {
            journey += Time.deltaTime * absorbSpeed;
            obj.position = Vector3.Lerp(startPos, targetPos, journey);
            yield return null;
        }

        obj.position = targetPos;
    }
}