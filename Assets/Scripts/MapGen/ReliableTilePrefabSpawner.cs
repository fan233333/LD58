using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ReliableTilePrefabSpawner : MonoBehaviour
{
    [Header("依赖")]
    public TilemapProcGen generator;    // 复用你的生成器
    public Tilemap tilemap;             // 通常与 generator.tilemap 相同

    [System.Serializable]
    public class Rule
    {
        public string name;
        public GameObject prefab;
        public List<Sprite> sprites;
        public float size;
        public float angle;
        [Tooltip("允许出现的地形类型（0..3）。至少勾选一个")] public bool[] allowTypes = new bool[4] { true, true, true, true };
        [Tooltip("密度：每 100 个合格 tile 期望生成多少个实例；若 Count Override>=0 则忽略密度")] [Min(0f)] public float densityPer100 = 5f;
        [Tooltip("固定生成数量；设为 -1 使用密度")] public int countOverride = -1;
        [Tooltip("在格子内部的随机抖动（0=居中）")] [Range(0f, 0.5f)] public float cellJitter = 0.25f;
    }

    [Header("规则列表")]
    public Rule[] rules = new Rule[0];

    [Header("通用控制")]
    public int seed = 2025;
    public bool clearBeforeSpawn = true;
    [Tooltip("若 Tilemap 没有内容，则自动调用 generator.Generate() 先生成地图")] public bool autoGenerateIfEmpty = true;
    public Transform container; // 生成物父节点；为空则挂在当前物体

    System.Random _rng;

    void Start()
    {
        generator.Generate(SeedStatic.tileSeed);
        seed = SeedStatic.objectSeed;
        if (Application.isPlaying) Spawn();
        
        
    }

    [ContextMenu("Spawn Now")] // 右键组件标题
    public void Spawn()
    {
        if (generator == null || tilemap == null)
        { Debug.LogError("ReliableTilePrefabSpawner: 缺少 generator 或 tilemap"); return; }
        if (rules == null || rules.Length == 0)
        { Debug.LogWarning("ReliableTilePrefabSpawner: 没有配置规则"); return; }

        // 1) 确保 Tilemap 已经有内容；必要时先生成
        if (autoGenerateIfEmpty && !TilemapHasTiles())
        {
            generator.Generate(SeedStatic.tileSeed);
        }
        if (!TilemapHasTiles())
        {
            Debug.LogWarning("ReliableTilePrefabSpawner: Tilemap 为空，未生成任何预制体。（确认已点击 Generate 或开启 autoGenerateIfEmpty）");
            return;
        }

        var parent = container ? container : transform;
        if (clearBeforeSpawn)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var ch = parent.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(ch); else DestroyImmediate(ch);
            }
        }

        _rng = new System.Random(seed);

        foreach (var rule in rules)
        {
            if (rule == null || rule.prefab == null) continue;

            // 2) 收集允许类型的所有格子（优先从 generator 获取；若不可用则回退扫描 Tilemap 的 tiles）
            var cells = CollectAllowedCells(rule);
            if (cells.Count == 0) continue;

            // 3) 计算数量（密度模式至少放 1 个，只要密度>0 且存在候选）
            int targetCount = rule.countOverride >= 0 ? rule.countOverride : Mathf.FloorToInt(rule.densityPer100 * cells.Count / 100f);
            if (targetCount <= 0 && rule.densityPer100 > 0f) targetCount = 1;
            if (targetCount <= 0) continue;

            Shuffle(cells);

            // 4) 实例化
            var cellSize = tilemap.cellSize;
            int spawned = 0;
            for (int i = 0; i < cells.Count && spawned < targetCount; i++)
            {
                var cell = cells[i];
                Vector3 world = tilemap.GetCellCenterWorld(cell);
                if (rule.cellJitter > 0f)
                {
                    float jx = ((float)_rng.NextDouble()*2 - 1) * rule.cellJitter * cellSize.x;
                    float jy = ((float)_rng.NextDouble()*2 - 1) * rule.cellJitter * cellSize.y;
                    world += new Vector3(jx, jy, 0f);
                }

                GameObject go;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    go = (GameObject)PrefabUtility.InstantiatePrefab(rule.prefab, parent);
                    go.transform.position = world;
                }
                else
                {
                    go = Instantiate(rule.prefab, world, Quaternion.identity, parent);
                    float size = Random.Range(0.5f, rule.size);
                    go.transform.localScale = new Vector3(size, size, size);
                    float rotateAngle = Random.Range(-rule.angle, rule.angle);
                    Quaternion targetRotation = Quaternion.AngleAxis(rotateAngle, Vector3.forward);
                    go.transform.rotation = targetRotation;
                    int spriteNo = Random.Range(0, rule.sprites.Count);
                    SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = rule.sprites[spriteNo];
                }
#else
                go = Instantiate(rule.prefab, world, Quaternion.identity, parent);
#endif
                spawned++;
            }
        }
    }

    // —— 帮助方法 ——
    bool TilemapHasTiles()
    {
        var b = tilemap.cellBounds;
        foreach (var pos in b.allPositionsWithin)
        {
            if (tilemap.HasTile((Vector3Int)pos)) return true;
        }
        return false;
    }

    List<Vector3Int> CollectAllowedCells(Rule rule)
    {
        var cells = new List<Vector3Int>();
        bool usedGenerator = false;

        // 尝试用 generator 提供的类型查询（最快）
        try
        {
            for (int t = 0; t < 4; t++)
            {
                if (!rule.allowTypes[t]) continue;
                foreach (var c in generator.GetCellsOfType(t)) { cells.Add(c); usedGenerator = true; }
            }
        }
        catch { /* ignore */ }

        // 若 generator 暂不可用或没有返回，回退到扫描 tilemap：通过 Tile 引用比对类型
        if (!usedGenerator || cells.Count == 0)
        {
            var b = tilemap.cellBounds;
            for (int x = b.xMin; x < b.xMax; x++)
            for (int y = b.yMin; y < b.yMax; y++)
            {
                var p = new Vector3Int(x, y, 0);
                var t = tilemap.GetTile(p);
                if (t == null) continue;
                // 与 generator.tiles[类型] 比对
                for (int type = 0; type < 4; type++)
                {
                    if (!rule.allowTypes[type]) continue;
                    if (type < generator.tiles.Length && generator.tiles[type] == t)
                    { cells.Add(p); break; }
                }
            }
        }
        return cells;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
