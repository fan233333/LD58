using UnityEngine;

public class tray : MonoBehaviour
{
    public GameObject prefab;
    public float minHorizontalForce = -2f;
    public float maxHorizontalForce = 2f;
    public Transform bagParent;

    public void SpawnPrefab(GameObject pre)
    {

        // 获取生成位置与旋转
        Vector3 spawnPos = transform.position;
        Quaternion spawnRot = transform.rotation;

        // 生成 prefab
        GameObject newObject = Instantiate(pre, spawnPos, spawnRot);
        newObject.transform.localScale = pre.transform.localScale;
        newObject.layer = 7;

        // 如果设置了父物体，则设为其子物体
        newObject.transform.SetParent(bagParent);

        Rigidbody2D rb = newObject.GetComponent<Rigidbody2D>();
        rb.drag = 0f;
        rb.gravityScale = 1f;

        Collider2D col = rb.GetComponent<Collider2D>();
        col.enabled = true;

        newObject.tag = "ReadyToFall";

        SpriteRenderer spriteRenderer = newObject.GetComponent<SpriteRenderer>();

        // 方法1：通过名称设置Sorting Layer
        spriteRenderer.sortingLayerName = "UI";

        if (rb != null)
        {
            // 添加随机水平力
            float randomForce = Random.Range(minHorizontalForce, maxHorizontalForce);
            rb.AddForce(new Vector2(randomForce, 0), ForceMode2D.Impulse);
        }
    }
}
