using Kuchinashi.DataSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "New Plot Data", menuName = "Scriptable Objects/Plot Data")]
public class PlotData : ScriptableObject , IHaveId
{
    public string Id => _id;
    [SerializeField] private string _id;
    public string Name;
    [TextArea(3, 10)] public string Description;
    public TextAsset Script;
    public bool Temporary;  // Temporary plot will not be saved by GameProgress.
}