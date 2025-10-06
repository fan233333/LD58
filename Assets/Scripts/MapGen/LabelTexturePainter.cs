// LabelTexturePainter.cs (standalone)
// 将四类地形标签直接渲染为一张彩色纹理（非 tile 可视化）。
// 依赖：TilemapProcGen（用于读取标签与尺寸）和 Tilemap（用于世界对齐）。
// 用法：
// 1) 在场景中新建空物体并挂载本脚本；
// 2) 将 Generator 指向你的 TilemapProcGen，Tilemap 指向对应的 Tilemap；
// 3) 设定 Palette（0..3 四种颜色）、Pixels Per Tile（建议 8~32）、Filter=Bilinear；
// 4) 右键组件标题 → Build Texture；或运行时在 Start() 自动生成；
// 5) 若勾选 Hide Tilemap Renderer，将隐藏原有 Tile 的渲染，仅显示颜色纹理。

using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class LabelTexturePainter : MonoBehaviour
{
    [Header("依赖")]
    public TilemapProcGen generator;   // 用于读取 Width/Height、标签、Tile 顺序与 Origin
    public Tilemap tilemap;            // 仅用于定位与单元大小，不再用它来显示

    [Header("渲染设置")]
    [Tooltip("每个地形格子对应多少像素（越大越平滑，非网格感更强）")]
    [Min(1)] public int pixelsPerTile = 16;
    public FilterMode filter = FilterMode.Bilinear;
    [Tooltip("生成后隐藏原 TilemapRenderer（只显示彩色纹理）")]
    public bool hideTilemapRenderer = true;
    [Tooltip("若 Tilemap 为空，则先自动调用 generator.Generate() 生成地图")]
    public bool autoGenerateIfEmpty = true;

    [Header("颜色（按 0..3 地形类型）")]
    public Color[] palette = new Color[4]
    {
        new Color(0.20f, 0.60f, 1.00f), // 0
        new Color(0.30f, 0.80f, 0.30f), // 1
        new Color(0.95f, 0.85f, 0.50f), // 2
        new Color(0.60f, 0.50f, 0.65f)  // 3
    };

    [Header("输出载体（可留空自动创建)")]
    public SpriteRenderer targetRenderer;

    private Texture2D _tex;

    void Start()
    {
        if (Application.isPlaying)
            Build();
    }

    [ContextMenu("Build Texture")]
    public void Build()
    {
        if (generator == null || tilemap == null)
        {
            Debug.LogError("LabelTexturePainter: 缺少 generator 或 tilemap 引用。");
            return;
        }

        // 若还没有地图，尝试生成一次
        if (autoGenerateIfEmpty && !TilemapHasTiles())
        {
            generator.Generate();
        }

        int W = generator.Width;
        int H = generator.Height;
        if (W <= 0 || H <= 0)
        {
            Debug.LogWarning("LabelTexturePainter: 无法从 generator 获取有效地图尺寸。");
            return;
        }

        // SpriteRenderer 载体
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
            if (targetRenderer == null) targetRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // 创建/重建纹理
        int texW = Mathf.Max(1, W * pixelsPerTile);
        int texH = Mathf.Max(1, H * pixelsPerTile);
        bool needRecreate = (_tex == null || _tex.width != texW || _tex.height != texH || _tex.format != TextureFormat.RGBA32);
        if (needRecreate)
        {
            if (_tex != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(_tex);
                else Destroy(_tex);
#else
                Destroy(_tex);
#endif
            }
            _tex = new Texture2D(texW, texH, TextureFormat.RGBA32, false, false);
            _tex.wrapMode = TextureWrapMode.Clamp;
        }
        _tex.filterMode = filter;

        // 填充像素缓冲（按 tile 方块批量写入）
        var buf = _tex.GetPixels32();
        for (int x = 0; x < W; x++)
        {
            int px0 = x * pixelsPerTile;
            for (int y = 0; y < H; y++)
            {
                int label;
                if (!generator.TryGetLabelAt(x, y, out label))
                {
                    Debug.Log("why");
                    label = 0; // 回退：若实现不可用则默认 0
                }

                if (label < 0 || label >= palette.Length)
                {
                    Debug.Log("how");
                    label = 0;
                }

                Color32 c = palette[label];
                Debug.Log("x:"+x+" y:"+y+" label:"+label);
                int py0 = y * pixelsPerTile;
                for (int dy = 0; dy < pixelsPerTile; dy++)
                {
                    int row = (py0 + dy) * texW;
                    for (int dx = 0; dx < pixelsPerTile; dx++)
                    {
                        int px = px0 + dx;
                        buf[row + px] = c;
                    }
                }
            }
        }
        _tex.SetPixels32(buf);
        _tex.Apply(false, false);

        // 生成精灵（pixelsPerUnit = pixelsPerTile，使 1 tile = 1 世界单位）
        var rect = new Rect(0, 0, texW, texH);
        var pivot = new Vector2(0.5f, 0.5f);
        var sprite = Sprite.Create(_tex, rect, pivot, pixelsPerTile);
        targetRenderer.sprite = sprite;

        // 对齐到 Tilemap：以生成器的 Origin 为左下角居中到世界
        var origin = generator.TilemapOrigin;        // (-W/2, -H/2, 0)
        var cellSize = tilemap.cellSize;             // 世界中每个 tile 的尺寸
        Vector3 bottomLeftCenter = tilemap.GetCellCenterWorld(origin);
        Vector3 centerOffset = new Vector3((W - 1) * 0.5f * cellSize.x, (H - 1) * 0.5f * cellSize.y, 0f);
        transform.position = bottomLeftCenter + centerOffset;
        transform.localScale = new Vector3(cellSize.x, cellSize.y, 1f);

        if (hideTilemapRenderer)
        {
            var r = tilemap.GetComponent<TilemapRenderer>();
            if (r != null) r.enabled = false;
        }
    }

    private bool TilemapHasTiles()
    {
        var b = tilemap.cellBounds;
        foreach (var pos in b.allPositionsWithin)
        {
            if (tilemap.HasTile((Vector3Int)pos)) return true;
        }
        return false;
    }
}