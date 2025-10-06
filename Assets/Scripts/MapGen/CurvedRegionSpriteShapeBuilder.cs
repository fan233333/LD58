// CurvedRegionSpriteShapeBuilder.cs
// 将 0..3 四类地形的连通区域转成「曲线边界」的 SpriteShape 多边形（每个区域 = 一个子物体）。
// 依赖：Package Manager 安装 2D SpriteShape（UnityEngine.U2D）。
// 用法：
// 1) 将本脚本挂到空物体上；拖入 generator(TilemapProcGen) 与 tilemap（用于定位/尺度）。
// 2) 为四类地形分别指定 SpriteShapeProfile（Create → 2D → Sprite Shape Profile）。
// 3) 点右键组件标题 → Build All，或运行时 Start() 自动构建。可勾选 Hide Tilemap Renderer 隐藏原 Tile 可视化。

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D; // SpriteShape
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class CurvedRegionSpriteShapeBuilder : MonoBehaviour
{
    [Header("依赖")]
    public TilemapProcGen generator;   // 读取标签与尺寸
    public Tilemap tilemap;            // 用于对齐与单位尺寸

    [Header("每类地形的 SpriteShape Profile (0..3)")]
    public SpriteShape[] profiles = new SpriteShape[4];

    [Header("过滤与平滑")]
    [Tooltip("忽略像素格计数小于该值的区域（防止噪声小岛）")] public int minRegionSize = 12;
    [Tooltip("将边折线细分为长度不超过该值的线段（世界单位），越小越圆滑")] [Min(0.05f)] public float maxEdgeSegment = 0.5f;
    [Tooltip("切线强度（0~1+），越大曲率越强")] public float tangentScale = 0.4f;

    [Header("其它")]
    public bool autoGenerateIfEmpty = true;
    public bool hideTilemapRenderer = true;
    public Transform container; // 生成物父节点

    void Start()
    {
        if (Application.isPlaying) BuildAll();
    }

    [ContextMenu("Build All")]
    public void BuildAll()
    {
        if (generator == null || tilemap == null)
        { Debug.LogError("CurvedRegionSpriteShapeBuilder: 缺少 generator 或 tilemap"); return; }

        // 确保地图已生成
        int W = generator.Width, H = generator.Height;
        if ((W <= 0 || H <= 0) && autoGenerateIfEmpty)
        {
            generator.Generate(SeedStatic.tileSeed);
            W = generator.Width; H = generator.Height;
        }
        if (W <= 0 || H <= 0) { Debug.LogWarning("CurvedRegionSpriteShapeBuilder: 地图尺寸为 0"); return; }

        var parent = container ? container : transform;
        // 清空旧的
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var ch = parent.GetChild(i).gameObject;
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(ch); else Destroy(ch);
#else
            Destroy(ch);
#endif
        }

        // 构造每一类的连通区域并生成 SpriteShape
        for (int type = 0; type < 4; type++)
        {
            if (profiles == null || type >= profiles.Length || profiles[type] == null) continue;
            BuildType(type, parent);
        }

        if (hideTilemapRenderer)
        {
            var r = tilemap.GetComponent<TilemapRenderer>();
            if (r) r.enabled = false;
        }
    }

    void BuildType(int type, Transform parent)
    {
        int W = generator.Width, H = generator.Height;
        // 1) 收集该类型的所有格子
        var filled = new HashSet<Vector2Int>();
        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            if (TryGetTypeAt(x, y, out int lab) && lab == type) filled.Add(new Vector2Int(x, y));
        }
        if (filled.Count == 0) return;

        // 2) 连通分量（4邻接）
        var visited = new HashSet<Vector2Int>();
        var dirs = new Vector2Int[]{ new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1) };
        var q = new Queue<Vector2Int>();

        foreach (var start in new List<Vector2Int>(filled))
        {
            if (visited.Contains(start)) continue;
            // BFS 收集一个区域
            var comp = new List<Vector2Int>();
            visited.Add(start); q.Enqueue(start);
            while (q.Count > 0)
            {
                var p = q.Dequeue(); comp.Add(p);
                foreach (var d in dirs)
                {
                    var n = p + d;
                    if (!visited.Contains(n) && filled.Contains(n)) { visited.Add(n); q.Enqueue(n); }
                }
            }
            if (comp.Count < Mathf.Max(1, minRegionSize)) continue;

            // 3) 为该区域构建边界（轴对齐多边形）
            var loops = BuildBoundaryLoops(comp);
            if (loops == null || loops.Count == 0) continue;

            // 4) 为每个闭环生成一个 SpriteShapeController
            foreach (var loop in loops)
            {
                if (loop.Count < 3) continue;
                var go = new GameObject($"Type{type}_Region");
                go.transform.SetParent(parent, false);
                var ssc = go.AddComponent<SpriteShapeController>();
                ssc.spriteShape = profiles[type];
                ssc.worldSpaceUVs = false;
                ssc.fillPixelsPerUnit = 100; // 由 Profile 决定外观，数值影响不大
                var sr = go.GetComponent<SpriteShapeRenderer>();
                if (sr) sr.sortingOrder = type; // 简单分层

                // 写入点并曲线化
                var spline = ssc.spline;
                spline.Clear();

                // 细分长边，避免过于锯齿
                var refined = RefineBySegment(loop, maxEdgeSegment);

                for (int i = 0; i < refined.Count; i++)
                {
                    Vector3 wp = GridVertexToWorld(refined[i]);
                    Vector3 lp = go.transform.InverseTransformPoint(wp);
                    int idx = spline.GetPointCount();
                    spline.InsertPointAt(idx, lp);
                    spline.SetTangentMode(idx, ShapeTangentMode.Continuous);
                }
                // 设置切线（圆润）
                for (int i = 0; i < spline.GetPointCount(); i++)
                {
                    Vector3 p = spline.GetPosition(i);
                    Vector3 pPrev = spline.GetPosition((i - 1 + spline.GetPointCount()) % spline.GetPointCount());
                    Vector3 pNext = spline.GetPosition((i + 1) % spline.GetPointCount());
                    Vector3 v1 = (p - pPrev);
                    Vector3 v2 = (pNext - p);
                    float len = Mathf.Min(v1.magnitude, v2.magnitude) * tangentScale;
                    if (len < 1e-4f) len = 0f;
                    Vector3 dir1 = v1.normalized; Vector3 dir2 = v2.normalized;
                    Vector3 tdir = (dir1 + dir2).normalized; if (tdir.sqrMagnitude < 1e-6f) tdir = dir2;
                    spline.SetRightTangent(i, tdir * len);
                    spline.SetLeftTangent(i, -tdir * len);
                }
                ssc.spline.isOpenEnded = false;
                ssc.RefreshSpriteShape();
            }
        }
    }

    // —— 将格点(整数)转成世界坐标 ——
    Vector3 GridVertexToWorld(Vector2 v)
    {
        // 格点坐标：以 tile (0,0) 的左下角为 (0,0)，顶点范围 [0..W]x[0..H]
        var origin = generator.TilemapOrigin; // bottom-left cell center index
        var cellSize = tilemap.cellSize;
        Vector3 bottomLeftCenter = tilemap.GetCellCenterWorld(origin);
        // 顶点(0,0) = bottomLeftCenter + (-0.5,-0.5)*cellSize
        return bottomLeftCenter + new Vector3((v.x - 0.5f) * cellSize.x, (v.y - 0.5f) * cellSize.y, 0f);
    }

    // —— 根据一个区域（若干像素格）生成边界环 ——
    List<List<Vector2>> BuildBoundaryLoops(List<Vector2Int> comp)
    {
        var set = new HashSet<Vector2Int>(comp);
        // 1) 收集边界边（面向外部的格子边）
        var edges = new HashSet<(Vector2Int a, Vector2Int b)>();
        foreach (var c in comp)
        {
            int x = c.x, y = c.y;
            // 左边
            if (!set.Contains(new Vector2Int(x - 1, y))) AddEdge(edges, new Vector2Int(x, y), new Vector2Int(x, y + 1));
            // 右边
            if (!set.Contains(new Vector2Int(x + 1, y))) AddEdge(edges, new Vector2Int(x + 1, y), new Vector2Int(x + 1, y + 1));
            // 下边
            if (!set.Contains(new Vector2Int(x, y - 1))) AddEdge(edges, new Vector2Int(x, y), new Vector2Int(x + 1, y));
            // 上边
            if (!set.Contains(new Vector2Int(x, y + 1))) AddEdge(edges, new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1));
        }
        if (edges.Count == 0) return null;

        // 2) 将边拼接成闭环
        // 建立顶点邻接
        var adj = new Dictionary<Vector2Int, List<Vector2Int>>();
        foreach (var e in edges)
        {
            if (!adj.ContainsKey(e.a)) adj[e.a] = new List<Vector2Int>();
            if (!adj.ContainsKey(e.b)) adj[e.b] = new List<Vector2Int>();
            adj[e.a].Add(e.b);
            adj[e.b].Add(e.a);
        }

        var loops = new List<List<Vector2>>();
        var used = new HashSet<(Vector2Int a, Vector2Int b)>();

        foreach (var kv in adj)
        {
            var start = kv.Key;
            foreach (var nxt in kv.Value)
            {
                var e = NormEdge(start, nxt);
                if (used.Contains(e)) continue;

                // 沿边遍历闭环
                var loop = new List<Vector2>();
                var prev = start; var curr = nxt;
                loop.Add(ToFloat(start));
                loop.Add(ToFloat(curr));
                used.Add(e);
                int guard = 0;
                while (guard++ < edges.Count * 2)
                {
                    var neigh = adj[curr];
                    // 选择不是 prev 的那个（若有两个）
                    Vector2Int cand = neigh[0] == prev && neigh.Count > 1 ? neigh[1] : neigh[0];
                    var ne = NormEdge(curr, cand);
                    if (used.Contains(ne))
                    {
                        bool found = false;
                        for (int i = 0; i < neigh.Count; i++)
                        {
                            var c2 = neigh[i]; var ne2 = NormEdge(curr, c2);
                            if (!used.Contains(ne2) && c2 != prev) { cand = c2; ne = ne2; found = true; break; }
                        }
                        if (!found) break; // 死路
                    }
                    used.Add(ne);
                    prev = curr; curr = cand; loop.Add(ToFloat(curr));
                    if (curr == start) break;
                }
                // 去重首尾
                if (loop.Count >= 4)
                {
                    while (loop.Count > 1 && loop[loop.Count - 1] == loop[0]) loop.RemoveAt(loop.Count - 1);
                    loops.Add(loop);
                }
            }
        }

        return loops;
    }

    void AddEdge(HashSet<(Vector2Int a, Vector2Int b)> edges, Vector2Int a, Vector2Int b)
    {
        var e = NormEdge(a, b); edges.Add(e);
    }

    (Vector2Int a, Vector2Int b) NormEdge(Vector2Int a, Vector2Int b)
    {
        return (a.x < b.x || (a.x == b.x && a.y <= b.y)) ? (a, b) : (b, a);
    }

    Vector2 ToFloat(Vector2Int v) => new Vector2(v.x, v.y);

    // 将折线细分，限制每段最大长度，利于平滑
    List<Vector2> RefineBySegment(List<Vector2> poly, float maxSeg)
    {
        var res = new List<Vector2>();
        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 a = poly[i];
            Vector2 b = poly[(i + 1) % poly.Count];
            res.Add(a);
            float d = Vector2.Distance(a, b);
            if (d > maxSeg)
            {
                int n = Mathf.CeilToInt(d / maxSeg) - 1;
                for (int k = 1; k <= n; k++)
                {
                    float t = (float)k / (n + 1);
                    res.Add(Vector2.Lerp(a, b, t));
                }
            }
        }
        return res;
    }

    // —— 读取标签 ——
    bool TryGetTypeAt(int x, int y, out int type)
    {
        try { if (generator.TryGetLabelAt(x, y, out type)) return true; } catch { }
        var p = generator.TilemapOrigin + new Vector3Int(x, y, 0);
        var t = tilemap.GetTile(p);
        for (int k = 0; k < 4; k++)
        {
            if (k < generator.tiles.Length && generator.tiles[k] == t) { type = k; return true; }
        }
        type = -1; return false;
    }
}
