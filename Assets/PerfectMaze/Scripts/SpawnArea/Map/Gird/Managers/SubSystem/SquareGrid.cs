using System;
using System.Collections.Generic;
using UnityEngine;

namespace CNV.GirdCore
{
    public sealed class SquareGrid : Grid2DBase
    {
        public float CellSize { get; }  // cạnh hình vuông

        public SquareGrid(int rows, int cols, float cellSize, Vector2 origin, float gap = 0f)
            : base(rows, cols, origin, gap)
        {
            if (cellSize <= 0f) throw new ArgumentOutOfRangeException(nameof(cellSize));
            CellSize = cellSize;
        }

        private float Pitch => CellSize + Gap;

        protected override Vector2 GetCenterCore(int row, int col)
        {
            float x = Origin.x + (col + 0.5f) * Pitch;
            float y = Origin.y + (row + 0.5f) * Pitch;
            return new Vector2(x, y);
        }

        protected override IReadOnlyList<Vector2> GetCellPolygonCore(int row, int col)
        {
            float x0 = Origin.x + col * Pitch;
            float y0 = Origin.y + row * Pitch;
            float x1 = x0 + CellSize;
            float y1 = y0 + CellSize;

            // Thứ tự: a(Trái-Trên) → b(Phải-Trên) → c(Phải-Dưới) → d(Trái-Dưới)
            // Có thể đảo nếu muốn.
            _poly4[0] = new Vector2(x0, y0);
            _poly4[1] = new Vector2(x1, y0);
            _poly4[2] = new Vector2(x1, y1);
            _poly4[3] = new Vector2(x0, y1);
            return _poly4;
        }

        // Reuse buffer để tránh GC khi gọi lặp lại nhiều lần.
        // Lưu ý: nếu bạn muốn giữ polygon sau khi method trả về lâu dài,
        // hãy sao chép ra mảng mới (đây là view tạm thời).
        private readonly Vector2[] _poly4 = new Vector2[4];
    }
}