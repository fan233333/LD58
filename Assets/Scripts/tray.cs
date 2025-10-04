using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tray : MonoBehaviour
{
    public GameObject prefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnPrefab();
        }
    }

    void SpawnPrefab()
    {

        // 获取生成位置与旋转
        Vector3 spawnPos = transform.position;
        Quaternion spawnRot = transform.rotation;

        // 生成 prefab
        GameObject newObject = Instantiate(prefab, spawnPos, spawnRot);

        // 如果设置了父物体，则设为其子物体
        newObject.transform.SetParent(transform);
    }
}
