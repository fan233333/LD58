using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGenerator : MonoBehaviour
{
    public int width = 200;
    public int height = 200;

    [Header("Tilemap & Tiles")]
    public Tilemap tilemap;
    public TileBase waterTile;
    public TileBase sandTile;
    public TileBase grassTile;
    public TileBase mountainTile;

    [Header("Noise Settings")]
    public float continentScale = 0.008f;   // 低频：大形状
    public float detailScale    = 0.03f;    // 高频：细节
    public int fbmOctaves = 4;              // fBM 叠加
    public float fbmPersistence = 0.5f;
    public float fbmLacunarity = 2.0f;

    [Header("Domain Warping")]
    public float warpScale = 0.02f;         // 扭曲噪声频率
    public float warpAmp   = 15f;           // 扭曲幅度（像素偏移）

    [Header("Biomes Thresholds (after combine)")]
    // 按从低到高：water < sand < grass < mountain
    public float waterT = 0.38f;
    public float sandT  = 0.48f;
    public float grassT = 0.65f;

    [Header("Hysteresis seeding")]
    public float seedOffset = 0.08f;        // 高阈值 = 阈值 + seedOffset
    public int   growPasses = 2;            // 低阈值扩张次数

    [Header("Post Smoothing")]
    public int majorityPasses = 2;          // 多数化平滑次数
    public int minRegionSize = 60;          // 小连通块剔除阈值

    enum T { Water=0, Sand=1, Grass=2, Mountain=3 }
    private T[,] map;

    void Start()
    {
        Generate();
        Paint();
    }

    public void Generate()
    {
        map = new T[width, height];

        // 1) 生成基础值：fBM(continent) + 小幅 fBM(detail)，再做域扭曲
        float offX = Random.Range(0f, 10000f);
        float offY = Random.Range(0f, 10000f);

        float wOffX = Random.Range(0f, 10000f);
        float wOffY = Random.Range(0f, 10000f);

        float dOffX = Random.Range(0f, 10000f);
        float dOffY = Random.Range(0f, 10000f);

        float[,] value = new float[width, height];

        for (int x=0; x<width; x++)
        for (int y=0; y<height; y++)
        {
            // 域扭曲向量（低成本两张 Perlin）
            float dx = (Perlin01(x, y, warpScale, wOffX, wOffY) - 0.5f) * 2f * warpAmp;
            float dy = (Perlin01(x+9999, y+7777, warpScale, wOffX, wOffY) - 0.5f) * 2f * warpAmp;

            // 大形状：低频 fBM
            float big = fBM01(x + dx, y + dy, continentScale, offX, offY, fbmOctaves, fbmPersistence, fbmLacunarity);

            // 细节：高频 fBM（权重较小）
            float det = fBM01(x, y, detailScale, dOffX, dOffY, 2, 0.5f, 2.0f);

            // 组合：权重在 0.7/0.3 左右（可调）
            float v = Mathf.Clamp01(big * 0.7f + det * 0.3f);
            value[x, y] = v;
        }

        // 2) 滞回：高阈值播种 -> 低阈值扩张（减少碎块）
        T[,] seeded = SeedHighThenGrow(value);
        map = seeded;

        // 3) 多数化平滑
        for (int i=0; i<majorityPasses; i++)
            map = MajoritySmooth(map);

        // 4) 小连通块剔除
        RemoveSmallRegions(map, minRegionSize);
    }

    // === Noise helpers ===
    float Perlin01(float x, float y, float scale, float ox, float oy)
    {
        return Mathf.PerlinNoise((x+ox) * scale, (y+oy) * scale);
    }

    float fBM01(float x, float y, float baseScale, float ox, float oy, int octaves, float persistence, float lacunarity)
    {
        float amp = 1f;
        float freq = baseScale;
        float sum = 0f;
        float norm = 0f;
        for (int i=0; i<octaves; i++)
        {
            sum  += amp * Mathf.PerlinNoise((x+ox) * freq, (y+oy) * freq);
            norm += amp;
            amp  *= persistence;
            freq *= lacunarity;
        }
        return sum / Mathf.Max(0.0001f, norm); // 0..1
    }

    // === Hysteresis biomes ===
    T[,] SeedHighThenGrow(float[,] val)
    {
        int w = val.GetLength(0), h = val.GetLength(1);
        var res = new T[w,h];
        bool[,] isSeed = new bool[w,h];

        // 高阈值播种
        for (int x=0; x<w; x++)
        for (int y=0; y<h; y++)
        {
            float v = val[x,y];
            res[x,y] = Classify(v, high:true);
            // 标记：只有当落在“高阈值”的明确类别时才算种子
            isSeed[x,y] = IsSeed(v);
        }

        // 低阈值扩张（按邻接传播）
        for (int pass=0; pass<growPasses; pass++)
        {
            var next = (T[,])res.Clone();
            for (int x=0; x<w; x++)
            for (int y=0; y<h; y++)
            {
                if (isSeed[x,y]) continue; // 种子格保持不变

                // 查看邻居里“最强势”的类型（此处用简单多数）
                Dictionary<T, int> count = new Dictionary<T, int>();
                foreach (var (nx, ny) in Neigh8(x,y,w,h))
                {
                    T t = res[nx,ny];
                    count[t] = count.ContainsKey(t) ? count[t]+1 : 1;
                }
                T majority = res[x,y];
                int best = -1;
                foreach (var kv in count)
                {
                    if (kv.Value > best) { best = kv.Value; majority = kv.Key; }
                }

                // 如果当前像素在低阈值下也允许属于 majority 类型，则接受扩张
                if (CanBelong(val[x,y], majority))
                    next[x,y] = majority;
            }
            res = next;
        }

        return res;
    }

    T Classify(float v, bool high)
    {
        // 根据 high 与 seedOffset 做两套阈值：高阈值更“苛刻”，只抓核心
        float wT = waterT + (high ?  seedOffset : 0f);
        float sT = sandT  + (high ?  seedOffset : 0f);
        float gT = grassT + (high ?  seedOffset : 0f);

        if (v < wT) return T.Water;
        else if (v < sT) return T.Sand;
        else if (v < gT) return T.Grass;
        else return T.Mountain;
    }

    bool IsSeed(float v)
    {
        // 只有落入“高阈值”明确区间才当种子
        T hi = Classify(v, high:true);
        T lo = Classify(v, high:false);
        return hi == lo;
    }

    bool CanBelong(float v, T target)
    {
        // 低阈值判定：更宽容
        return Classify(v, high:false) == target;
    }

    // === Majority smoothing ===
    T[,] MajoritySmooth(T[,] src)
    {
        int w = src.GetLength(0), h = src.GetLength(1);
        var dst = (T[,])src.Clone();
        for (int x=0; x<w; x++)
        for (int y=0; y<h; y++)
        {
            Dictionary<T,int> count = new Dictionary<T, int>();
            foreach (var (nx, ny) in Neigh8(x,y,w,h))
            {
                var t = src[nx,ny];
                count[t] = count.ContainsKey(t) ? count[t]+1 : 1;
            }
            var cur = src[x,y];
            int best = -1;
            T maj = cur;
            foreach (var kv in count)
            {
                if (kv.Value > best) { best = kv.Value; maj = kv.Key; }
            }
            // 若邻居多数与当前不同且优势≥5（可调），则替换
            if (maj != cur && best >= 5) dst[x,y] = maj;
        }
        return dst;
    }

    // === Small region removal ===
    void RemoveSmallRegions(T[,] src, int minSize)
    {
        int w = src.GetLength(0), h = src.GetLength(1);
        bool[,] vis = new bool[w,h];

        var dirs = new (int, int)[]{(1,0),(-1,0),(0,1),(0,-1)};

        for (int x=0; x<w; x++)
        for (int y=0; y<h; y++)
        {
            if (vis[x,y]) continue;
            T type = src[x,y];

            // BFS 连通块
            List<(int,int)> cells = new List<(int,int)>();
            Queue<(int,int)> q = new Queue<(int,int)>();
            q.Enqueue((x,y));
            vis[x,y] = true;

            while (q.Count>0)
            {
                var (cx, cy) = q.Dequeue();
                cells.Add((cx,cy));
                foreach (var (dx,dy) in dirs)
                {
                    int nx = cx+dx, ny = cy+dy;
                    if (nx<0||ny<0||nx>=w||ny>=h) continue;
                    if (vis[nx,ny]) continue;
                    if (src[nx,ny] != type) continue;
                    vis[nx,ny] = true;
                    q.Enqueue((nx,ny));
                }
            }

            if (cells.Count < minSize)
            {
                // 找周边多数类型
                Dictionary<T,int> borderCount = new Dictionary<T, int>();
                foreach (var (cx, cy) in cells)
                {
                    foreach (var (dx,dy) in dirs)
                    {
                        int nx = cx+dx, ny = cy+dy;
                        if (nx<0||ny<0||nx>=w||ny>=h) continue;
                        if (src[nx,ny] == type) continue;
                        var t = src[nx,ny];
                        borderCount[t] = borderCount.ContainsKey(t) ? borderCount[t]+1 : 1;
                    }
                }
                if (borderCount.Count == 0) continue;
                // 选最多的邻接类型替换
                int best=-1; T rep=type;
                foreach (var kv in borderCount)
                    if (kv.Value>best) { best=kv.Value; rep=kv.Key; }

                foreach (var (cx, cy) in cells) src[cx,cy] = rep;
            }
        }
    }

    IEnumerable<(int,int)> Neigh8(int x,int y,int w,int h)
    {
        for (int dx=-1; dx<=1; dx++)
        for (int dy=-1; dy<=1; dy++)
        {
            if (dx==0 && dy==0) continue;
            int nx=x+dx, ny=y+dy;
            if (nx>=0 && ny>=0 && nx<w && ny<h)
                yield return (nx,ny);
        }
    }

    // === Rendering to Tilemap ===
    void Paint()
    {
        tilemap.ClearAllTiles();
        for (int x=0; x<width; x++)
        for (int y=0; y<height; y++)
        {
            var t = map[x,y];
            TileBase tile = waterTile;
            switch (t)
            {
                case T.Water:    tile = waterTile; break;
                case T.Sand:     tile = sandTile; break;
                case T.Grass:    tile = grassTile; break;
                case T.Mountain: tile = mountainTile; break;
            }
            tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        }
    }
}
