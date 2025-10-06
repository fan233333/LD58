using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TileLabelMap", menuName = "ProcGen/Tile Label Map", order = 50)]
public class TileLabelMapAsset : ScriptableObject
{
    public int width;
    public int height;
    [SerializeField] private int[] labels; // x + y*width

    public void Set(int[,] src)
    {
        width = src.GetLength(0); height = src.GetLength(1);
        int n = width * height;
        if (labels == null || labels.Length != n) labels = new int[n];
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            labels[x + y * width] = src[x, y];
    }

    public bool TryGet(int x, int y, out int label)
    {
        label = -1;
        if (labels == null || x < 0 || y < 0 || x >= width || y >= height) return false;
        label = labels[x + y * width];
        return true;
    }

    public IEnumerable<Vector2Int> AllOfType(int type)
    {
        if (labels == null) yield break;
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            if (labels[x + y * width] == type) yield return new Vector2Int(x, y);
    }
}