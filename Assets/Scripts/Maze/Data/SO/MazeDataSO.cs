using CNV.GirdCore;
using UnityEngine;

[CreateAssetMenu(fileName = "MazeDataSO", menuName = "Scriptable Objects/MazeDataSO")]
public class MazeDataSO : ScriptableObject
{
    public GirdData GirdData;
    [Header("Prefabs")] 
    public GridCell cellPrefab;
    public Agent agentPrefab;
}
