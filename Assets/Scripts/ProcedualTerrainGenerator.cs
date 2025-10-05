using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralTerrainGenerator : MonoBehaviour
{
    [Header("Tilemap 设置")]
    public Tilemap tilemap;
    public TileBase grassTile;
    public TileBase sandTile;
    public TileBase snowTile;
    public TileBase lavaTile;

    [Header("生成范围")]
    public int width = 100;
    public int height = 50;
    public float noiseScale = 0.1f;

    [Header("柏林噪声偏移")]
    public Vector2 noiseOffset;

    [Header("地貌高度阈值 (0-1之间)")]
    [Range(0f, 1f)] public float grassThreshold = 0.3f;
    [Range(0f, 1f)] public float sandThreshold = 0.6f;
    [Range(0f, 1f)] public float snowThreshold = 0.8f;

    [Header("资源 Prefabs")]
    public List<GameObject> grassResourcePrefab;
    public List<GameObject> sandResourcePrefab;
    public List<GameObject> snowResourcePrefab;
    public List<GameObject> lavaResourcePrefab;

    [Header("资源生成密度（0~1）")]
    [Range(0f, 1f)] public float grassResourceDensity = 0.05f;
    [Range(0f, 1f)] public float sandResourceDensity = 0.02f;
    [Range(0f, 1f)] public float snowResourceDensity = 0.01f;
    [Range(0f, 1f)] public float lavaResourceDensity = 0.01f;
    
    [Header("装饰 Prefabs")]
    public List<GameObject> grassDecoPrefab;
    public List<GameObject> sandDecoPrefab;
    public List<GameObject> snowDecoPrefab;
    public List<GameObject> lavaDecoPrefab;
    
    [Header("装饰生成密度（0~1）")]
    [Range(0f, 1f)] public float grassDecoDensity = 0.05f;
    [Range(0f, 1f)] public float sandDecoDensity = 0.02f;
    [Range(0f, 1f)] public float snowDecoDensity = 0.01f;
    [Range(0f, 1f)] public float lavaDecoDensity = 0.01f;


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
                    TrySpawnResource(grassResourcePrefab, grassResourceDensity, grassDecoPrefab, grassDecoDensity, x, y);
                }
                else if (noiseValue < sandThreshold)
                {
                    tilemap.SetTile(tilePos, sandTile);
                    TrySpawnResource(sandResourcePrefab, sandResourceDensity, sandDecoPrefab, sandDecoDensity, x, y);
                }
                else if (noiseValue < snowThreshold)
                {
                    tilemap.SetTile(tilePos, snowTile);
                    TrySpawnResource(snowResourcePrefab, snowResourceDensity, snowDecoPrefab, snowDecoDensity, x, y);
                }
                else
                {
                    tilemap.SetTile(tilePos, lavaTile);
                    TrySpawnResource(lavaResourcePrefab, lavaResourceDensity, lavaDecoPrefab, lavaDecoDensity, x, y);
                }
            }
        }

        Debug.Log("✅ 地形生成完成！");
    }

    void TrySpawnResource(List<GameObject> prefab, float density, List<GameObject> prefab2, float density2, int x, int y)
    {
        if (prefab.Count == 0) return;

        if (Random.value < density)
        {
            Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, 0.5f, 0);
            GameObject obj = Instantiate(prefab[(int)(Random.value*prefab.Count)], worldPos, Quaternion.identity, resourceParent);
        }
        else if (Random.value < density2)
        {
            Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, 0.5f, 0);
            GameObject obj = Instantiate(prefab2[(int)(Random.value*prefab2.Count)], worldPos, Quaternion.identity, resourceParent);
        }
    }
}