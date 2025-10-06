using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AmountRange
{
    [Min(0)] public int min;
    [Min(0)] public int max;
}

[System.Serializable]
public class ItemConfig
{
    public string itemName;
    [Min(1)] public int maxNumber = 1;

    [Header("当本次任务包含 1/2/3 种物品时，对应的 requiredAmount 区间")]
    public AmountRange when1Item = new AmountRange { min = 1, max = 3 };
    public AmountRange when2Items = new AmountRange { min = 1, max = 2 };
    public AmountRange when3Items = new AmountRange { min = 1, max = 1 };

    public AmountRange GetRangeByTotalCount(int totalKinds)
    {
        switch (Mathf.Clamp(totalKinds, 1, 3))
        {
            case 1: return when1Item;
            case 2: return when2Items;
            default: return when3Items;
        }
    }
}

public class TaskGenerator : MonoBehaviour
{
    [Header("目标：把生成结果写入 TaskManager.TaskItems")]
    public TaskManager taskManager;

    [Header("物品配置（每种一个配置）")]
    public List<ItemConfig> itemConfigs = new List<ItemConfig>();

    [Header("生成控制")]
    public bool useFixedItemKindCount = false;
    [Range(1, 3)] public int fixedItemKinds = 3;     // 勾选 useFixedItemKindCount 时生效
    [Tooltip("随机种子（勾选 useSeed 时生效）")] public int seed = 0;
    public bool useSeed = false;

    [Tooltip("是否允许 requiredAmount 为 0；一般不需要，默认 false 会强制>=1")]
    public bool allowZeroRequired = false;

    /// <summary> 生成并直接赋值给 TaskManager.TaskItems </summary>
    public void GenerateAndAssign()
    {
        if (taskManager == null)
        {
            Debug.LogError("[TaskGenerator] 请先指定 TaskManager 引用。");
            return;
        }
        if (itemConfigs == null || itemConfigs.Count == 0)
        {
            Debug.LogError("[TaskGenerator] itemConfigs 为空。");
            return;
        }

        // 随机源
        System.Random rng = useSeed ? new System.Random(seed) : new System.Random();

        // 1) 决定这次任务一共有几种物品
        int totalKinds = useFixedItemKindCount ? Mathf.Clamp(fixedItemKinds, 1, 3) : (rng.Next(1, 4)); // [1,3]

        // 2) 从配置里随机挑选 totalKinds 个不重复的物品
        var pool = new List<ItemConfig>(itemConfigs);
        if (pool.Count < totalKinds)
        {
            Debug.LogWarning($"[TaskGenerator] 可用物品配置不足（需要 {totalKinds}，只有 {pool.Count}），将按上限生成。");
            totalKinds = pool.Count;
        }

        // 打乱
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        var picked = pool.GetRange(0, totalKinds);

        // 3) 为每个选中的物品生成 requiredAmount（根据 1/2/3 种的区间），并 clamp 到 maxNumber
        var result = new List<TaskItem>(totalKinds);
        foreach (var cfg in picked)
        {
            var range = cfg.GetRangeByTotalCount(totalKinds);
            int lo = Mathf.Max(0, Mathf.Min(range.min, range.max));
            int hi = Mathf.Max(0, Mathf.Max(range.min, range.max));
            int val = (lo == hi) ? lo : rng.Next(lo, hi + 1);

            // clamp 到该物品的 maxNumber
            val = Mathf.Min(val, Mathf.Max(1, cfg.maxNumber));
            if (!allowZeroRequired) val = Mathf.Max(1, val);

            var ti = new TaskItem
            {
                itemName = cfg.itemName,
                requiredAmount = val,
                maxNumber = cfg.maxNumber,
                Container = null,           // 按你的要求：不生成
                currentAmount = 0           // 按你的要求：不生成（HideInInspector）
            };
            result.Add(ti);
        }

        // 4) 写回 TaskManager
        taskManager.taskItems = result;

#if UNITY_EDITOR
        // 标记脏，方便编辑器下保存
        UnityEditor.EditorUtility.SetDirty(taskManager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(taskManager.gameObject.scene);
#endif

        Debug.Log($"[TaskGenerator] 已生成 {result.Count} 个任务物品，并写入 TaskManager.TaskItems。");
    }

    // 右键组件标题 → Generate & Assign
    [ContextMenu("Generate & Assign")]
    private void ContextGenerate() => GenerateAndAssign();
}
