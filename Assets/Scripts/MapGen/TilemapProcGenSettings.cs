using UnityEngine;


[CreateAssetMenu(fileName = "ProcGenSettings", menuName = "ProcGen/Tilemap Settings", order = 0)]
public class TilemapProcGenSettings : ScriptableObject
{
    [Header("地图尺寸")]
    [Min(8)] public int width = 96;
    [Min(8)] public int height = 96;


    [Header("随机种子 & 比例控制")]
    public int seed = 12345;
    // public int seed = SeedStatic.tileSeed;
    [Tooltip("四种地形出现的目标比例（和会自动归一化）。例如 [1,1,1,1] 表示期望平均。")]
    public Vector4 targetWeights = new Vector4(1,1,1,1);
    [Range(0f, 1f), Tooltip("小地图偏置：在小地图时额外提高区域合并力度，避免碎片。0=关闭，1=最强。")]
    public float smallMapBias = 0.4f;


    [Header("主算法 (选其一)")]
    public GenerationAlgorithm algorithm = GenerationAlgorithm.VoronoiSeeds;
    public enum GenerationAlgorithm { VoronoiSeeds, KMeansOnMultiNoise, FBmThresholds }


    [Header("FBm/Perlin 噪声参数（部分算法使用）")]
    [Min(1e-3f)] public float scale = 32f;
    [Range(1, 8)] public int octaves = 4;
    [Range(1.5f, 3.0f)] public float lacunarity = 2.0f;
    [Range(0.2f, 0.9f)] public float gain = 0.5f;


    [Header("Domain Warp（形变）")]
    [Tooltip("形变强度，制造更自然的边界。")]
    [Range(0f, 20f)] public float warpStrength = 8f;
    [Tooltip("形变噪声的缩放")]
    [Min(1e-3f)] public float warpScale = 64f;


    [Header("Voronoi 种子控制（VoronoiSeeds 算法使用）")]
    [Min(1)] public int baseSeeds = 24;
    [Range(0f, 1f), Tooltip("边界抖动幅度，避免直线边。")]
    public float borderJitter = 0.2f;


    [Header("KMeans 聚类（KMeansOnMultiNoise 算法使用）")]
    [Range(1, 32)] public int kmeansIters = 10;


    [Header("后处理（所有算法可用）")]
    [Range(0, 5), Tooltip("多数投票平滑迭代次数（8邻域）。")] public int smoothIterations = 2;
    [Range(0, 1), Tooltip("平滑强度，越高越偏向邻域多数。通常 0.5 即多数投票。")]
    public float smoothBias = 0.5f;
    [Min(0), Tooltip("最小连通区域尺寸，小于该尺寸的碎片会被并入周围主类。0=禁用")]
    public int minRegionSize = 20;
}