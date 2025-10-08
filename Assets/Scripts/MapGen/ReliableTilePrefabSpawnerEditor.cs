#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(ReliableTilePrefabSpawner))]
public class ReliableTilePrefabSpawnerEditor : Editor
{
    private bool showDependencies = true;
    private bool showGeneralSettings = true;
    private bool showRules = true;
    private bool showAdvanced = false;
    private bool showDebug = false;

    public override void OnInspectorGUI()
    {
        var spawner = (ReliableTilePrefabSpawner)target;
        serializedObject.Update();

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("物品生成器 - 编辑器预览", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // ========== 快速参数区域 ==========
        EditorGUILayout.LabelField("快速参数（常用）", EditorStyles.boldLabel);
        
        // 种子参数 - 突出显示
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🎲 种子控制", EditorStyles.boldLabel);
        
        var seedProp = serializedObject.FindProperty("seed");
        EditorGUILayout.PropertyField(seedProp, new GUIContent("物品生成种子", "控制物品生成的随机性"));
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("随机种子", GUILayout.Width(80)))
        {
            seedProp.intValue = Random.Range(0, 999999);
        }
        if (GUILayout.Button("重置为0", GUILayout.Width(80)))
        {
            seedProp.intValue = 0;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // 生成按钮区域
        EditorGUILayout.BeginVertical("box");
        
        // 主要按钮
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🔄 生成物品", GUILayout.Height(30)))
        {
            spawner.Spawn();
            EditorUtility.SetDirty(spawner);
        }
        GUI.backgroundColor = Color.white;
        
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("🗑️ 清除物品", GUILayout.Height(30)))
        {
            ClearSpawnedItems(spawner);
            EditorUtility.SetDirty(spawner);
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        // 辅助按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🎲 随机种子+生成"))
        {
            spawner.seed = Random.Range(0, 999999);
            spawner.Spawn();
            EditorUtility.SetDirty(spawner);
        }
        
        if (GUILayout.Button("🔗 同步地图种子"))
        {
            if (spawner.generator != null && spawner.generator.settings != null)
            {
                spawner.seed = spawner.generator.settings.seed;
                EditorUtility.SetDirty(spawner);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // ========== 依赖设置 ==========
        showDependencies = EditorGUILayout.Foldout(showDependencies, "🔗 依赖设置", true);
        if (showDependencies)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("generator"), new GUIContent("地图生成器", "复用的TilemapProcGen生成器"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tilemap"), new GUIContent("瓦片地图", "通常与generator.tilemap相同"));
            
            // InitParent的特殊说明
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("🎯 特殊功能", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("InitParent"), new GUIContent("初始位置父节点", "用于名为'InitPos'的特殊规则"));
            EditorGUILayout.HelpBox("当规则名称为'InitPos'时，生成的物品会自动设置此Transform为父节点", MessageType.Info);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("container"), new GUIContent("容器节点", "生成物父节点，为空则挂在当前物体下"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);

        // ========== 通用设置 ==========
        showGeneralSettings = EditorGUILayout.Foldout(showGeneralSettings, "⚙️ 通用设置", true);
        if (showGeneralSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clearBeforeSpawn"), new GUIContent("生成前清除", "生成新物品前清除已有的生成物"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoGenerateIfEmpty"), new GUIContent("自动生成地图", "如果Tilemap为空，自动调用生成器先生成地图"));
            
            // 重叠控制 - 重要功能，需要突出显示
            EditorGUILayout.Space(3);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("🚫 重叠控制", EditorStyles.boldLabel);
            var preventOverlapProp = serializedObject.FindProperty("preventOverlapBetweenRules");
            EditorGUILayout.PropertyField(preventOverlapProp, new GUIContent("防止规则间重叠", "完全避免不同规则间的重叠生成"));
            
            if (preventOverlapProp.boolValue)
            {
                EditorGUILayout.HelpBox("✅ 已启用重叠控制：不同规则的物品不会在彼此的占用区域内生成", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠️ 重叠控制已关闭：物品可能会重叠生成", MessageType.Warning);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);

        // ========== 生成规则 ==========
        showRules = EditorGUILayout.Foldout(showRules, "📋 生成规则", true);
        if (showRules)
        {
            EditorGUI.indentLevel++;
            var rulesProp = serializedObject.FindProperty("rules");
            
            // 规则数量控制
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"规则数量: {rulesProp.arraySize}", EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                rulesProp.arraySize++;
            }
            if (GUILayout.Button("-", GUILayout.Width(30)) && rulesProp.arraySize > 0)
            {
                rulesProp.arraySize--;
            }
            EditorGUILayout.EndHorizontal();

            // 显示每个规则
            for (int i = 0; i < rulesProp.arraySize; i++)
            {
                var ruleProp = rulesProp.GetArrayElementAtIndex(i);
                var nameProp = ruleProp.FindPropertyRelative("name");
                var prefabProp = ruleProp.FindPropertyRelative("prefab");
                
                string ruleName = string.IsNullOrEmpty(nameProp.stringValue) ? $"规则 {i + 1}" : nameProp.stringValue;
                
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"🎯 {ruleName}", EditorStyles.boldLabel);
                
                if (GUILayout.Button("❌", GUILayout.Width(25)))
                {
                    rulesProp.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(nameProp, new GUIContent("名称"));
                EditorGUILayout.PropertyField(prefabProp, new GUIContent("预制体"));
                
                // 基础参数
                EditorGUILayout.PropertyField(ruleProp.FindPropertyRelative("densityPer100"), new GUIContent("密度(每100格)"));
                EditorGUILayout.PropertyField(ruleProp.FindPropertyRelative("countOverride"), new GUIContent("固定数量(-1使用密度)"));
                EditorGUILayout.PropertyField(ruleProp.FindPropertyRelative("cellJitter"), new GUIContent("位置抖动"));
                
                // 占用区域设置 - 重要功能，放在主要区域
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("📐 占用区域设置", EditorStyles.boldLabel);
                
                var occupyRadiusProp = ruleProp.FindPropertyRelative("occupyRadius");
                var customOccupyAreaProp = ruleProp.FindPropertyRelative("customOccupyArea");
                
                EditorGUILayout.PropertyField(occupyRadiusProp, new GUIContent("占用半径", "0=只占1格，1=3x3区域，2=5x5区域等"));
                
                // 显示占用半径的可视化说明
                if (occupyRadiusProp.intValue > 0)
                {
                    int radius = occupyRadiusProp.intValue;
                    int gridSize = radius * 2 + 1;
                    EditorGUILayout.HelpBox($"占用 {gridSize}×{gridSize} 区域 (共{gridSize * gridSize}格)", MessageType.Info);
                }
                
                EditorGUILayout.PropertyField(customOccupyAreaProp, new GUIContent("自定义占用区域", "如果设置，将忽略占用半径"), true);
                
                if (customOccupyAreaProp.arraySize > 0)
                {
                    EditorGUILayout.HelpBox($"自定义占用: {customOccupyAreaProp.arraySize} 个格子", MessageType.Info);
                }
                
                // 随机化参数
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("🎲 随机化设置", EditorStyles.boldLabel);
                
                // 大小随机化
                var minSizeProp = ruleProp.FindPropertyRelative("minSize");
                var maxSizeProp = ruleProp.FindPropertyRelative("maxSize");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("大小范围:", GUILayout.Width(60));
                minSizeProp.floatValue = EditorGUILayout.FloatField(minSizeProp.floatValue, GUILayout.Width(50));
                EditorGUILayout.LabelField("~", GUILayout.Width(15));
                maxSizeProp.floatValue = EditorGUILayout.FloatField(maxSizeProp.floatValue, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                // 角度随机化
                EditorGUILayout.PropertyField(ruleProp.FindPropertyRelative("angle"), new GUIContent("旋转角度(±度)"));
                
                // 精灵随机化
                EditorGUILayout.PropertyField(ruleProp.FindPropertyRelative("sprites"), new GUIContent("随机精灵列表"), true);
                
                // 允许类型
                var allowTypesProp = ruleProp.FindPropertyRelative("allowTypes");
                EditorGUILayout.LabelField("允许的地形类型:");
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < 4; j++)
                {
                    var typeProp = allowTypesProp.GetArrayElementAtIndex(j);
                    typeProp.boolValue = EditorGUILayout.Toggle($"T{j}", typeProp.boolValue, GUILayout.Width(40));
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(3);
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);

        // ========== 高级设置 ==========
        showAdvanced = EditorGUILayout.Foldout(showAdvanced, "🔧 高级设置", false);
        if (showAdvanced)
        {
            EditorGUI.indentLevel++;
            
            // 排除区域
            EditorGUILayout.LabelField("排除区域设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("excludeAreas"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("removeExcludeCollidersAfterGeneration"));
            
            // 排除区域管理按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🗑️ 清除排除区域Collider"))
            {
                spawner.RemoveExcludeAreaColliders();
                EditorUtility.SetDirty(spawner);
            }
            
            // 显示排除区域状态
            bool hasActiveAreas = spawner.HasActiveExcludeAreas();
            EditorGUILayout.LabelField(hasActiveAreas ? "✅ 有活跃排除区域" : "❌ 无活跃排除区域", 
                hasActiveAreas ? EditorStyles.boldLabel : EditorStyles.label);
            EditorGUILayout.EndHorizontal();
            

            
            EditorGUI.indentLevel--;
        }

        // ========== 调试设置 ==========
        showDebug = EditorGUILayout.Foldout(showDebug, "🐛 调试设置", false);
        if (showDebug)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("visualizeOccupiedCells"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("visualizeExcludeAreas"));
            EditorGUI.indentLevel--;
        }

        // ========== 实时预览检测 ==========
        if (GUI.changed)
        {
            // 检测到参数变化，自动重新生成（仅在编辑器模式下）
            if (!Application.isPlaying && spawner.generator != null)
            {
                // 延迟调用，避免在PropertyField期间调用Spawn
                EditorApplication.delayCall += () => {
                    if (spawner != null) // 确保对象还存在
                    {
                        spawner.Spawn();
                        EditorUtility.SetDirty(spawner);
                    }
                };
            }
        }

        serializedObject.ApplyModifiedProperties();

        // ========== 状态信息 ==========
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("📊 状态信息", EditorStyles.boldLabel);
        
        if (spawner.rules != null)
        {
            EditorGUILayout.LabelField($"配置规则数量: {spawner.rules.Length}");
            int validRules = spawner.rules.Count(r => r != null && r.prefab != null);
            int initPosRules = spawner.rules.Count(r => r != null && r.name == "InitPos");
            EditorGUILayout.LabelField($"有效规则数量: {validRules}");
            if (initPosRules > 0)
            {
                EditorGUILayout.LabelField($"🎯 InitPos特殊规则: {initPosRules}个", EditorStyles.boldLabel);
            }
        }
        
        var container = spawner.container ? spawner.container : spawner.transform;
        EditorGUILayout.LabelField($"当前生成物品数量: {container.childCount}");
        
        if (spawner.generator != null)
        {
            if (spawner.generator.settings != null)
            {
                EditorGUILayout.LabelField($"地图生成种子: {spawner.generator.settings.seed}");
            }
            EditorGUILayout.LabelField($"地图生成器状态: {(spawner.generator.tilemap != null ? "✅ 已连接" : "❌ 未连接")}");
        }
        
        // 排除区域状态
        if (spawner.excludeAreas != null && spawner.excludeAreas.Length > 0)
        {
            int activeAreas = spawner.excludeAreas.Count(area => area != null);
            EditorGUILayout.LabelField($"排除区域: {activeAreas}/{spawner.excludeAreas.Length} 活跃");
        }
        
        // 种子状态比较
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"物品生成种子: {spawner.seed}", EditorStyles.boldLabel);
        if (spawner.generator != null && spawner.generator.settings != null)
        {
            bool seedsMatch = spawner.seed == spawner.generator.settings.seed;
            EditorGUILayout.LabelField(seedsMatch ? "🔄 与地图同步" : "🎲 独立种子", 
                seedsMatch ? EditorStyles.label : EditorStyles.boldLabel);
        }
        EditorGUILayout.EndHorizontal();
        
        // 重叠控制状态
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("重叠控制:", GUILayout.Width(80));
        EditorGUILayout.LabelField(spawner.preventOverlapBetweenRules ? "🚫 已启用" : "⚠️ 已关闭",
            spawner.preventOverlapBetweenRules ? EditorStyles.boldLabel : EditorStyles.label);
        EditorGUILayout.EndHorizontal();
        
        // 占用区域统计
        if (spawner.rules != null)
        {
            int rulesWithOccupy = 0;
            int totalOccupyCells = 0;
            
            foreach (var rule in spawner.rules)
            {
                if (rule != null)
                {
                    if (rule.customOccupyArea != null && rule.customOccupyArea.Count > 0)
                    {
                        rulesWithOccupy++;
                        totalOccupyCells += rule.customOccupyArea.Count;
                    }
                    else if (rule.occupyRadius > 0)
                    {
                        rulesWithOccupy++;
                        int gridSize = rule.occupyRadius * 2 + 1;
                        totalOccupyCells += gridSize * gridSize;
                    }
                }
            }
            
            if (rulesWithOccupy > 0)
            {
                EditorGUILayout.LabelField($"📐 占用区域: {rulesWithOccupy}个规则设置了占用");
            }
        }
        
        EditorGUILayout.EndVertical();

        // ========== 帮助信息 ==========
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "💡 功能说明：\n" +
            "• 修改种子或规则参数时会自动重新生成预览\n" +
            "• 确保已设置 generator 和 tilemap 引用\n" +
            "• 密度设置为每100个格子的期望生成数量\n" +
            "• 固定数量设为-1时使用密度模式\n\n" +
            "🎲 随机化功能：\n" +
            "• 大小范围: minSize~maxSize 控制物品大小随机化\n" +
            "• 旋转角度: ±度数控制物品旋转随机化\n" +
            "• 随机精灵: 可设置多个精灵随机选择\n\n" +
            "🎯 特殊功能：\n" +
            "• InitPos规则: 名为'InitPos'的规则会使用InitParent作为父节点\n" +
            "• 排除区域: 可设置Collider2D来排除生成区域\n" +
            "• 占用控制: 防止不同规则间的重叠生成", 
            MessageType.Info);
    }

    private void ClearSpawnedItems(ReliableTilePrefabSpawner spawner)
    {
        var container = spawner.container ? spawner.container : spawner.transform;
        
        // 清除所有子物体
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            var child = container.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                DestroyImmediate(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
        
        Debug.Log($"已清除 {container.name} 下的所有生成物品");
    }
}
#endif