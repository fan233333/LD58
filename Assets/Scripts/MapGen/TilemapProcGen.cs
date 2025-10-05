

// ===============================
// File: TilemapProcGen.cs
// MonoBehaviour：将结果画到 Unity Tilemap
// ===============================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class TilemapProcGen : MonoBehaviour
{
    [Header("引用")]
    public Tilemap tilemap;
    [Tooltip("四种 Tile 的顺序需与生成索引一致 (0..3)")]
    public TileBase[] tiles = new TileBase[4];

    [Header("参数")]
    public TilemapProcGenSettings settings;

    // 运行态缓存
    private int[,] _map; // 值域 0..3

    // ============ 对外入口 ============
    public void Generate()
    {
        if (settings == null || tilemap == null || tiles == null || tiles.Length < 4)
        {
            Debug.LogError("TilemapProcGen: 缺少必要引用/参数。");
            return;
        }

        Random.InitState(settings.seed);
        _map = new int[settings.width, settings.height];

        switch (settings.algorithm)
        {
            case TilemapProcGenSettings.GenerationAlgorithm.VoronoiSeeds:
                GenerateVoronoi();
                break;
            case TilemapProcGenSettings.GenerationAlgorithm.KMeansOnMultiNoise:
                GenerateKMeans();
                break;
            case TilemapProcGenSettings.GenerationAlgorithm.FBmThresholds:
                GenerateFBmThresholds();
                break;
        }

        PostProcess();
        PaintToTilemap();
    }

    // ============ 算法 1：Voronoi 种子 + 边界抖动（强集中，概率易控） ============
    private struct Seed { public Vector2 p; public int label; }

    void GenerateVoronoi()
    {
        var seeds = new List<Seed>();
        // 按 targetWeights 确定每类种子数
        Vector4 w = settings.targetWeights;
        float sum = Mathf.Max(1e-6f, w.x + w.y + w.z + w.w);
        int total = Mathf.Max(1, settings.baseSeeds);
        int[] count = new int[4];
        // 先按比例取整
        count[0] = Mathf.FloorToInt(total * (w.x / sum));
        count[1] = Mathf.FloorToInt(total * (w.y / sum));
        count[2] = Mathf.FloorToInt(total * (w.z / sum));
        count[3] = Mathf.FloorToInt(total * (w.w / sum));
        // 补足到 total
        int remain = total - (count[0] + count[1] + count[2] + count[3]);
        for (int i = 0; i < remain; i++) count[i % 4]++;

        // 简单“蓝噪声”采样：拒绝采样（dart throwing）
        float minDist = Mathf.Max(4f, Mathf.Min(settings.width, settings.height) / Mathf.Sqrt(total) * 0.6f);
        for (int label = 0; label < 4; label++)
        {
            int placed = 0; int guard = 0;
            while (placed < count[label] && guard++ < 10000)
            {
                var cand = new Vector2(Random.Range(0f, settings.width), Random.Range(0f, settings.height));
                bool ok = true;
                for (int s = 0; s < seeds.Count; s++)
                {
                    if (Vector2.SqrMagnitude(seeds[s].p - cand) < minDist * minDist) { ok = false; break; }
                }
                if (ok) { seeds.Add(new Seed { p = cand, label = label }); placed++; }
            }
        }

        // 标号：最近种子（Voronoi），带噪声抖动 + 可选形变
        for (int x = 0; x < settings.width; x++)
        for (int y = 0; y < settings.height; y++)
        {
            Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
            // domain warp
            if (settings.warpStrength > 0f)
            {
                var wv = WarpOffset(x, y);
                pos += wv * settings.warpStrength;
            }
            // border jitter：在距离相近处用噪声微调
            int best = -1; float bestD = float.MaxValue;
            for (int i = 0; i < seeds.Count; i++)
            {
                float d = (pos - seeds[i].p).sqrMagnitude;
                if (d < bestD) { bestD = d; best = i; }
            }
            // 抖动：用一个轻微噪声对最近距离加偏置（只在近邻情况下显著）
            float jitter = (Noise01(x * 0.9f, y * 0.9f, 7.77f) - 0.5f) * settings.borderJitter * 2f * Mathf.Sqrt(bestD + 1e-3f);
            int alt = best; float altD = bestD;
            for (int i = 0; i < seeds.Count; i++)
            {
                if (i == best) continue;
                float d = (pos - seeds[i].p).sqrMagnitude + jitter;
                if (d < altD) { altD = d; alt = i; }
            }
            _map[x, y] = seeds[alt].label;
        }
    }

    // ============ 算法 2：多通道噪声向量 + KMeans（自然块状、受噪声控制） ============
    void GenerateKMeans()
    {
        int W = settings.width, H = settings.height;
        var feats = new Vector3[W * H];
        // 三通道特征：基础 Perlin，偏移 Perlin，Warp 后 Perlin
        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            float n1 = FBm(x, y, settings.scale);
            float n2 = FBm(x + 123.4f, y + 77.7f, settings.scale * 0.8f);
            var wv = WarpOffset(x, y);
            float n3 = FBm(x + wv.x * 5f, y + wv.y * 5f, settings.scale * 1.2f);
            feats[x + y * W] = new Vector3(n1, n2, n3);
        }
        // KMeans 初始化：根据 targetWeights 从特征空间的四个象限取初始心
        Vector3[] cent = InitCentroidsWeighted(feats, 4, settings.seed);
        int[] label = new int[feats.Length];
        for (int it = 0; it < settings.kmeansIters; it++)
        {
            // 分配
            for (int i = 0; i < feats.Length; i++)
            {
                int bi = 0; float bd = float.MaxValue;
                for (int k = 0; k < 4; k++)
                {
                    float d = (feats[i] - cent[k]).sqrMagnitude;
                    if (d < bd) { bd = d; bi = k; }
                }
                label[i] = bi;
            }
            // 更新
            Vector3[] sum = new Vector3[4];
            int[] cnt = new int[4];
            for (int i = 0; i < feats.Length; i++) { sum[label[i]] += feats[i]; cnt[label[i]]++; }
            for (int k = 0; k < 4; k++) if (cnt[k] > 0) cent[k] = sum[k] / cnt[k];
        }
        // 写回
        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
            _map[x, y] = label[x + y * W];

        // 轻微类别重映射以贴合 targetWeights（按像素数排序 -> 目标比例排序）
        RemapToWeightsApprox();
    }

    // ============ 算法 3：FBm 阈值四分（带比例自适应 + 形变） ============
    void GenerateFBmThresholds()
    {
        int W = settings.width, H = settings.height;
        var values = new float[W * H];
        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            var wv = WarpOffset(x, y);
            values[x + y * W] = FBm(x + wv.x * 5f, y + wv.y * 5f, settings.scale);
        }
        // 根据 targetWeights 求四个分位点阈值
        float[] cuts = ComputeAdaptiveQuartiles(values, settings.targetWeights);
        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            float v = values[x + y * W];
            int c = 0;
            if (v >= cuts[0]) c = 1;
            if (v >= cuts[1]) c = 2;
            if (v >= cuts[2]) c = 3;
            _map[x, y] = c;
        }
    }

    // ============ 后处理 ============
    void PostProcess()
    {
        // 小地图附加偏置：增大多数投票与合并力度
        float mapFactor = Mathf.Clamp01( (64f / Mathf.Max(settings.width, settings.height)) );
        int extraSmooth = Mathf.RoundToInt(settings.smallMapBias * mapFactor * 2f);
        int smoothIters = settings.smoothIterations + extraSmooth;

        for (int i = 0; i < smoothIters; i++) MajoritySmooth();
        if (settings.minRegionSize > 0) MergeSmallRegions(settings.minRegionSize + Mathf.RoundToInt(extraSmooth * 10));
    }

    void MajoritySmooth()
    {
        int W = settings.width, H = settings.height;
        var copy = (int[,])_map.Clone();
        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            int[] cnt = new int[4];
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (nx>=0 && nx<W && ny>=0 && ny<H) cnt[copy[nx, ny]]++;
            }
            int cur = copy[x, y];
            int best = cur; int bestCnt = cnt[cur];
            for (int c = 0; c < 4; c++) if (cnt[c] > bestCnt) { bestCnt = cnt[c]; best = c; }
            float majority = bestCnt / 8f;
            _map[x, y] = (majority >= settings.smoothBias) ? best : cur;
        }
    }

    void MergeSmallRegions(int minSize)
    {
        int W = settings.width, H = settings.height;
        var visited = new bool[W, H];
        var dirs = new (int x,int y)[]{(1,0),(-1,0),(0,1),(0,-1)};
        var q = new Queue<(int,int)>();

        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            if (visited[x,y]) continue;
            int label = _map[x,y];
            q.Clear();
            var comp = new List<(int,int)>();
            visited[x,y] = true; q.Enqueue((x,y));
            while (q.Count>0)
            {
                var p = q.Dequeue(); comp.Add(p);
                foreach (var d in dirs)
                {
                    int nx=p.Item1+d.x, ny=p.Item2+d.y;
                    if (nx>=0 && nx<W && ny>=0 && ny<H && !visited[nx,ny] && _map[nx,ny]==label)
                    { visited[nx,ny]=true; q.Enqueue((nx,ny)); }
                }
            }
            if (comp.Count < minSize)
            {
                // 并入周围最多的类别
                int[] border = new int[4];
                foreach (var p in comp)
                {
                    foreach (var d in dirs)
                    {
                        int nx=p.Item1+d.x, ny=p.Item2+d.y;
                        if (nx>=0 && nx<W && ny>=0 && ny<H && _map[nx,ny]!=label)
                            border[_map[nx,ny]]++;
                    }
                }
                int tgt = 0, bc=-1; for (int c=0;c<4;c++) if (border[c]>bc){bc=border[c];tgt=c;}
                foreach (var p in comp) _map[p.Item1,p.Item2]=tgt;
            }
        }
    }

    void RemapToWeightsApprox()
    {
        int W = settings.width, H = settings.height;
        int[] cnt = new int[4];
        for (int x=0;x<W;x++) for(int y=0;y<H;y++) cnt[_map[x,y]]++;
        // 当前数量排序 -> 目标比例排序，建立重映射
        int[] idx = new int[]{0,1,2,3};
        System.Array.Sort(idx, (a,b)=>cnt[a].CompareTo(cnt[b])); // 从少到多
        int[] tgt = new int[]{0,1,2,3};
        Vector4 w = settings.targetWeights; float s = Mathf.Max(1e-6f, w.x+w.y+w.z+w.w);
        float[] wn = new float[]{w.x/s,w.y/s,w.z/s,w.w/s};
        int[] idw = new int[]{0,1,2,3};
        System.Array.Sort(idw, (a,b)=>wn[a].CompareTo(wn[b])); // 从小到大
        int[] map = new int[4];
        for (int i=0;i<4;i++) map[idx[i]] = idw[i];
        for (int x=0;x<W;x++) for (int y=0;y<H;y++) _map[x,y]=map[_map[x,y]];
    }

    // ============ 噪声与工具 ============
    Vector2 WarpOffset(int x, int y)
    {
        if (settings.warpStrength <= 0f) return Vector2.zero;
        float u = Noise01(x / settings.warpScale, y / settings.warpScale, 19.37f);
        float v = Noise01(x / settings.warpScale, y / settings.warpScale, 41.13f);
        return new Vector2(u - 0.5f, v - 0.5f);
    }

    float FBm(float x, float y, float scale)
    {
        float amp = 1f, freq = 1f/scale, sum = 0f, norm = 0f;
        for (int o=0;o<settings.octaves;o++)
        {
            sum += amp * Mathf.PerlinNoise(x*freq, y*freq);
            norm += amp;
            freq *= settings.lacunarity;
            amp *= settings.gain;
        }
        return sum / Mathf.Max(1e-6f, norm);
    }

    float Noise01(float x, float y, float salt)
    {
        return Mathf.PerlinNoise(x + settings.seed * 0.017f + salt, y + settings.seed * 0.031f + salt*0.37f);
    }

    float[] ComputeAdaptiveQuartiles(float[] vals, Vector4 weights)
    {
        // 根据权重计算三个分割点：p1,p2,p3，使四段比例接近目标
        float s = Mathf.Max(1e-6f, weights.x+weights.y+weights.z+weights.w);
        float p1 = weights.x/s;
        float p2 = p1 + weights.y/s;
        float p3 = p2 + weights.z/s;
        var tmp = (float[])vals.Clone();
        System.Array.Sort(tmp);
        float Q(float p){ int i=Mathf.Clamp(Mathf.RoundToInt(p*(tmp.Length-1)),0,tmp.Length-1); return tmp[i]; }
        return new float[]{ Q(p1), Q(p2), Q(p3) };
    }

    Vector3[] InitCentroidsWeighted(Vector3[] feats, int k, int seed)
    {
        // 取特征的 min/max 框，按四象限加随机扰动，保证初始分散
        Vector3 mn = new Vector3(float.MaxValue,float.MaxValue,float.MaxValue);
        Vector3 mx = new Vector3(float.MinValue,float.MinValue,float.MinValue);
        foreach (var f in feats)
        { mn = Vector3.Min(mn, f); mx = Vector3.Max(mx, f); }
        Random.InitState(seed);
        var cents = new Vector3[k];
        for (int i=0;i<k;i++)
        {
            float ax = (i&1)==0?0.25f:0.75f;
            float ay = (i&2)==0?0.25f:0.75f;
            float az = (i%3==0)?0.3f:0.7f;
            var t = new Vector3(ax, ay, az) + new Vector3(Random.value, Random.value, Random.value) * 0.1f;

// 分别对三个维度进行插值
            cents[i] = new Vector3(
                Mathf.LerpUnclamped(mn.x, mx.x, t.x),
                Mathf.LerpUnclamped(mn.y, mx.y, t.y),
                Mathf.LerpUnclamped(mn.z, mx.z, t.z)
            );
        }
        return cents;
    }

    // ============ 绘制 ============
    void PaintToTilemap()
    {
        tilemap.ClearAllTiles();
        int W = settings.width, H = settings.height;
        var origin = new Vector3Int(-W/2, -H/2, 0);
        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            int idx = Mathf.Clamp(_map[x,y], 0, tiles.Length-1);
            tilemap.SetTile(origin + new Vector3Int(x, y, 0), tiles[idx]);
        }
    }
    // ======= Public API for other systems (e.g., prefab spawner) =======
    public int Width => settings != null ? settings.width : 0;
    public int Height => settings != null ? settings.height : 0;
    public Vector3Int TilemapOrigin => new Vector3Int(-(settings!=null?settings.width:0)/2, -(settings!=null?settings.height:0)/2, 0);

    public bool TryGetLabelAt(int x, int y, out int label)
    {
        label = -1;
        if (_map == null) return false;
        if (x < 0 || y < 0 || x >= Width || y >= Height) return false;
        label = _map[x, y];
        return true;
    }

    public System.Collections.Generic.IEnumerable<Vector3Int> GetCellsOfType(int type)
    {
        if (_map == null) yield break;
        int W = Width, H = Height;
        var origin = TilemapOrigin;
        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            if (_map[x, y] == type) yield return origin + new Vector3Int(x, y, 0);
        }
    }

}

