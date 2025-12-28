using System;
using UnityEngine;

[Serializable]
public struct CellVisualEntry
{
    public CellPaint paint;
    public GameObject visual;
}

public enum CellPaint
{
    Empty,      // Ô nền
    Wall,       // Tường
    Start,      // NPC (S)
    Goal,       // Đích (G)
    Visited,    // Ô đã duyệt
    Path        // Đường đi kết quả
}