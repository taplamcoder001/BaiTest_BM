using System.Collections.Generic;
using UnityEngine;

namespace CNV.GirdCore
{
    public interface IGrid2D
    {
        int Rows { get; }
        int Cols { get; }
        Vector2 Origin { get; }

        // Hình học (giữ nguyên)
        Vector2 GetCenter(int row, int col);
        IEnumerable<Vector2> EnumerateCenters();
        IReadOnlyList<Vector2> GetCellPolygon(int row, int col);
        IEnumerable<IReadOnlyList<Vector2>> EnumerateCellPolygons();

        // ====== Nội dung/điều hướng ======
        // Có vật thể/đối tượng gì đặt trên ô?
        bool HasContent(int row, int col);
        GameObject GetContent(int row, int col);
        void SetContent(int row, int col, GameObject content);
        bool ClearContent(int row, int col);

        // Ô có thể đi qua không? (block/obstacle)
        bool IsWalkable(int row, int col);
        void SetWalkable(int row, int col, bool walkable);

        // “Cost” (trọng số) để pathfinding (A*, Dijkstra, v.v.)
        float GetCost(int row, int col);
        void SetCost(int row, int col, float cost);

        // Gói gọn thông tin một ô (thuận tiện debug/UI)
        CellInfo GetCellInfo(int row, int col);
    }

    /// <summary> Thông tin tóm tắt của một ô. </summary>
    public readonly struct CellInfo
    {
        public readonly bool HasContent;
        public readonly GameObject Content;
        public readonly bool Walkable;
        public readonly float Cost;

        public CellInfo(bool hasContent, GameObject content, bool walkable, float cost)
        {
            HasContent = hasContent;
            Content = content;
            Walkable = walkable;
            Cost = cost;
        }
    }
}