using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAbsorber : MonoBehaviour
{
    [Header("吸收设置")]
    public Transform container; // 容器物体
    public float absorbHeight = 5f; // 吸收的目标高度
    public float layerSpacing = 1f; // 层间距
    public float absorbSpeed = 2f; // 吸收速度
    public float delayBetweenLayers = 0.5f; // 层间延迟

    private List<Transform> objectsInContainer = new List<Transform>();
    private bool isAbsorbing = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isAbsorbing)
        {
            StartAbsorption();
        }
    }

    // 开始吸收过程
    public void StartAbsorption()
    {
        if (container == null) return;

        // 获取容器内所有子物体
        GetAllObjectsInContainer();

        // 根据高度排序
        SortObjectsByHeight();

        // 开始分层吸收
        StartCoroutine(AbsorbObjectsByLayer());
    }

    // 获取容器内所有物体
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

    // 根据高度排序（从低到高）
    void SortObjectsByHeight()
    {
        objectsInContainer.Sort((a, b) => b.position.y.CompareTo(a.position.y));
    }

    // 分层吸收协程
    IEnumerator AbsorbObjectsByLayer()
    {
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
    }

    // 将物体分组到不同层
    List<List<Transform>> GroupObjectsIntoLayers()
    {
        List<List<Transform>> layers = new List<List<Transform>>();

        if (objectsInContainer.Count == 0) return layers;

        // 使用阈值进行分层
        float heightThreshold = 0.4f; // 高度阈值，可根据需要调整

        List<Transform> currentLayer = new List<Transform>();
        float lastHeight = objectsInContainer[0].position.y;

        foreach (var obj in objectsInContainer)
        {
            if (Mathf.Abs(obj.position.y - lastHeight) > heightThreshold)
            {
                // 开始新的一层
                if (currentLayer.Count > 0)
                {
                    layers.Add(new List<Transform>(currentLayer));
                    currentLayer.Clear();
                }
            }

            currentLayer.Add(obj);
            lastHeight = obj.position.y;
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
        float targetY = absorbHeight + (layerIndex * layerSpacing);

        // 为该层所有物体启动吸收协程
        foreach (var obj in layerObjects)
        {
            Coroutine coroutine = StartCoroutine(MoveObjectToHeight(obj, targetY));
            coroutines.Add(coroutine);
            Destroy(obj);
        }

        // 等待该层所有物体吸收完成
        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }
    }

    // 移动物体到指定高度
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