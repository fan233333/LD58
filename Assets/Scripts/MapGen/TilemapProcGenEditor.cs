
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TilemapProcGen))]
public class TilemapProcGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var gen = (TilemapProcGen)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("tilemap"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("tiles"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"));
        
        // —— 新增：按类型分图层输出（用于不同 Collider） ——
        var pSplit = serializedObject.FindProperty("splitByType");
        var pTypeMaps = serializedObject.FindProperty("typeTilemaps");
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("分图层输出（每类独立 Tilemap）", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(pSplit, new GUIContent("Split By Type"));
        if (pSplit.boolValue)
        {
            // 确保数组长度为 4
            if (pTypeMaps.arraySize != 4) pTypeMaps.arraySize = 4;
            EditorGUI.indentLevel++;
            for (int i = 0; i < 4; i++)
            {
                var elem = pTypeMaps.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(elem, new GUIContent($"Type {i} Tilemap"));
            }
            EditorGUI.indentLevel--;


            bool missing = false;
            for (int i = 0; i < 4; i++)
            {
                var elem = pTypeMaps.GetArrayElementAtIndex(i);
                if (elem.objectReferenceValue == null) { missing = true; break; }
            }
            if (missing)
            {
                EditorGUILayout.HelpBox("开启 Split By Type 后，请为 typeTilemaps[0..3] 依次指定 4 个 Tilemap 对象。", MessageType.Warning);
            }
        }

        if (gen.settings != null)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("快速参数（常用）", EditorStyles.boldLabel);
            gen.settings.seed = EditorGUILayout.IntField("Seed", gen.settings.seed);
            gen.settings.algorithm = (TilemapProcGenSettings.GenerationAlgorithm)EditorGUILayout.EnumPopup("算法", gen.settings.algorithm);

            // 目标比例
            var w = gen.settings.targetWeights;
            EditorGUILayout.LabelField("四类目标比例 (将归一化)");
            w.x = EditorGUILayout.Slider("Type 0", w.x, 0f, 5f);
            w.y = EditorGUILayout.Slider("Type 1", w.y, 0f, 5f);
            w.z = EditorGUILayout.Slider("Type 2", w.z, 0f, 5f);
            w.w = EditorGUILayout.Slider("Type 3", w.w, 0f, 5f);
            gen.settings.targetWeights = w;

            EditorGUILayout.Space(6);
            if (gen.settings.algorithm != TilemapProcGenSettings.GenerationAlgorithm.VoronoiSeeds)
            {
                gen.settings.scale = EditorGUILayout.Slider("噪声 Scale", gen.settings.scale, 4f, 256f);
                gen.settings.octaves = EditorGUILayout.IntSlider("Octaves", gen.settings.octaves, 1, 8);
                gen.settings.lacunarity = EditorGUILayout.Slider("Lacunarity", gen.settings.lacunarity, 1.5f, 3.0f);
                gen.settings.gain = EditorGUILayout.Slider("Gain", gen.settings.gain, 0.2f, 0.9f);
            }

            gen.settings.warpStrength = EditorGUILayout.Slider("Warp 强度", gen.settings.warpStrength, 0f, 20f);
            gen.settings.warpScale = EditorGUILayout.Slider("Warp 缩放", gen.settings.warpScale, 8f, 256f);

            if (gen.settings.algorithm == TilemapProcGenSettings.GenerationAlgorithm.VoronoiSeeds)
            {
                gen.settings.baseSeeds = EditorGUILayout.IntSlider("基础种子数", gen.settings.baseSeeds, 4, 256);
                gen.settings.borderJitter = EditorGUILayout.Slider("边界抖动", gen.settings.borderJitter, 0f, 1f);
            }
            if (gen.settings.algorithm == TilemapProcGenSettings.GenerationAlgorithm.KMeansOnMultiNoise)
            {
                gen.settings.kmeansIters = EditorGUILayout.IntSlider("KMeans Iters", gen.settings.kmeansIters, 1, 32);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("后处理", EditorStyles.boldLabel);
            gen.settings.smoothIterations = EditorGUILayout.IntSlider("平滑迭代", gen.settings.smoothIterations, 0, 5);
            gen.settings.smoothBias = EditorGUILayout.Slider("平滑阈值", gen.settings.smoothBias, 0f, 1f);
            gen.settings.minRegionSize = EditorGUILayout.IntField("最小区域像素", gen.settings.minRegionSize);
            gen.settings.smallMapBias = EditorGUILayout.Slider("小地图偏置", gen.settings.smallMapBias, 0f, 1f);



            EditorGUILayout.Space(10);
            if (GUILayout.Button("Generate / 生成"))
            {
                gen.Generate(SeedStatic.tileSeed);
            }
            if (GUI.changed)
            {
                // 实时预览（编辑器下）

                if (!Application.isPlaying) gen.Generate(gen.settings.seed);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("请先创建并指定一个 TilemapProcGenSettings (右键 Project → Create → ProcGen → Tilemap Settings)", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();

    }
}
#endif
