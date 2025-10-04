using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralTerrainGenerator : MonoBehaviour
{
    [Header("Tilemap 设置")]
    public Tilemap tilemap;
    public TileBase grassTile;
    public TileBase sandTile;
    public TileBase snowTile;

    [Header("生成范围")]
    public int width = 100;
    public int height = 50;
    public float noiseScale = 0.1f;

    [Header("柏林噪声偏移")]
    public Vector2 noiseOffset;

    [Header("地貌高度阈值 (0-1之间)")]
    [Range(0f, 1f)] public float grassThreshold = 0.4f;
    [Range(0f, 1f)] public float sandThreshold = 0.7f;
    // 大于 sandThreshold 的即为雪地

    [Header("资源 Prefabs")]
    public GameObject grassResourcePrefab;
    public GameObject sandResourcePrefab;
    public GameObject snowResourcePrefab;

    [Header("资源生成密度（0~1）")]
    [Range(0f, 1f)] public float grassDensity = 0.05f;
    [Range(0f, 1f)] public float sandDensity = 0.02f;
    [Range(0f, 1f)] public float snowDensity = 0.01f;

    [Header("父对象（用于整理生成的资源）")]
    public Transform resourceParent;

    void Start()
    {
        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap 未指定！");
            return;
        }

        tilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 获取柏林噪声值
                float sampleX = (x + noiseOffset.x) * noiseScale;
                float sampleY = (y + noiseOffset.y) * noiseScale;
                float noiseValue = Mathf.PerlinNoise(sampleX, sampleY);

                Vector3Int tilePos = new Vector3Int(x, y, 0);

                // 选择地貌类型
                if (noiseValue < grassThreshold)
                {
                    tilemap.SetTile(tilePos, grassTile);
                    TrySpawnResource(grassResourcePrefab, grassDensity, x, y);
                }
                else if (noiseValue < sandThreshold)
                {
                    tilemap.SetTile(tilePos, sandTile);
                    TrySpawnResource(sandResourcePrefab, sandDensity, x, y);
                }
                else
                {
                    tilemap.SetTile(tilePos, snowTile);
                    TrySpawnResource(snowResourcePrefab, snowDensity, x, y);
                }
            }
        }

        Debug.Log("✅ 地形生成完成！");
    }

    void TrySpawnResource(GameObject prefab, float density, int x, int y)
    {
        if (prefab == null) return;

        if (Random.value < density)
        {
            Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, 0.5f, 0);
            GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity, resourceParent);
        }
    }
}