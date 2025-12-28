using System;
using System.Collections.Generic;
using UnityEngine;

namespace CNV.GirdCore
{
    public sealed class HexGridPointy : Grid2DBase
    {
        public enum RowParity
        {
            EvenR,
            OddR
        }

        public float Radius { get; } // center → vertex
        public RowParity Parity { get; }

        // Precomputed góc đỉnh (6 đỉnh) cho pointy-top với offset 30°
        // Thứ tự: 30°, 90°, 150°, 210°, 270°, 330°
        private readonly Vector2[] _unitCorners = new Vector2[6];

        public HexGridPointy(
            int rows,
            int cols,
            float radius,
            Vector2 origin,
            float gap = 0f,
            RowParity parity = RowParity.EvenR)
            : base(rows, cols, origin, gap)
        {
            if (radius <= 0f) throw new ArgumentOutOfRangeException(nameof(radius));
            Radius = radius;
            Parity = parity;
            PrecomputeUnitCorners();
        }

        private void PrecomputeUnitCorners()
        {
            const float deg30 = (float)(Math.PI / 6.0); // 30°
            const float step = (float)(Math.PI / 3.0); // 60° mỗi đỉnh

            for (int i = 0; i < 6; i++)
            {
                float a = deg30 + step * i;
                // Với pointy-top, vector đỉnh tính trực tiếp từ bán kính
                _unitCorners[i] = new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
            }
        }

        private float CellWidth => (float)(Math.Sqrt(3.0) * Radius); // ~1.732 * R
        private float CellHeight => 2f * Radius;

        // PitchX: khoảng cách tâm theo cột
        // PitchY: khoảng cách tâm theo hàng (1.5 * R)
        private float PitchX => CellWidth + Gap;
        private float PitchY => 1.5f * Radius + Gap * 0.5f;

        protected override Vector2 GetCenterCore(int row, int col)
        {
            float x = Origin.x + col * PitchX;

            bool shifted =
                (Parity == RowParity.EvenR && (row % 2 == 0)) ||
                (Parity == RowParity.OddR && (row % 2 != 0));

            if (shifted) x += PitchX * 0.5f;

            float y = Origin.y + row * PitchY;
            return new Vector2(x, y);
        }

        protected override IReadOnlyList<Vector2> GetCellPolygonCore(int row, int col)
        {
            var center = GetCenterCore(row, col);
            for (int i = 0; i < 6; i++)
            {
                // scale unit corner bằng bán kính và tịnh tiến về center
                _poly6[i] = center + _unitCorners[i] * Radius;
            }

            return _poly6;
        }

        // Buffer dùng lại để tránh GC (xem lưu ý tương tự SquareGrid)
        private readonly Vector2[] _poly6 = new Vector2[6];
    }
}