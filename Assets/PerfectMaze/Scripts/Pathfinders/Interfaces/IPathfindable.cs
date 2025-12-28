using System;
using System.Collections.Generic;
using UnityEngine;

namespace CNV.Pathfinding {
    public interface IPathfindable {
        // Trả về danh sách tọa độ lưới từ start -> goal, hoặc null nếu không có đường.
        // onExpand được gọi mỗi khi 1 node được "mở rộng" (pop khỏi open) để bạn tô màu/debug.
        List<Vector2Int> FindPath(
            int[,] grid, Vector2Int start, Vector2Int goal,
            Action<Vector2Int> onExpand = null);
    }
}