using System;
using System.Collections.Generic;
using UnityEngine;

namespace CNV.GirdCore
{
    public abstract class Grid2DBase : IGrid2D
    {
        public int Rows { get; }
        public int Cols { get; }
        public Vector2 Origin { get; }
        protected float Gap { get; }

        // ====== Storage nội dung/điều hướng ======
        private readonly GameObject[,] _content;
        private readonly bool[,] _walkable;
        private readonly float[,] _cost;

        protected Grid2DBase(int rows, int cols, Vector2 origin, float gap = 0f)
        {
            if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows));
            if (cols <= 0) throw new ArgumentOutOfRangeException(nameof(cols));
            if (gap < 0f)  throw new ArgumentOutOfRangeException(nameof(gap));

            Rows = rows;
            Cols = cols;
            Origin = origin;
            Gap = gap;

            _content  = new GameObject[rows, cols];
            _walkable = new bool[rows, cols];
            _cost     = new float[rows, cols];

            // Defaults: đi được & cost=1
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    _walkable[r, c] = true;
                    _cost[r, c] = 1f;
                }
        }

        // ===== Hình học (giữ nguyên) =====
        public Vector2 GetCenter(int row, int col)
        {
            ValidateIndex(row, col);
            return GetCenterCore(row, col);
        }

        public IEnumerable<Vector2> EnumerateCenters()
        {
            for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                yield return GetCenterCore(r, c);
        }

        public IReadOnlyList<Vector2> GetCellPolygon(int row, int col)
        {
            ValidateIndex(row, col);
            return GetCellPolygonCore(row, col);
        }

        public IEnumerable<IReadOnlyList<Vector2>> EnumerateCellPolygons()
        {
            for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                yield return GetCellPolygonCore(r, c);
        }

        protected void ValidateIndex(int row, int col)
        {
            if ((uint)row >= (uint)Rows) throw new ArgumentOutOfRangeException(nameof(row));
            if ((uint)col >= (uint)Cols) throw new ArgumentOutOfRangeException(nameof(col));
        }

        protected abstract Vector2 GetCenterCore(int row, int col);
        protected abstract IReadOnlyList<Vector2> GetCellPolygonCore(int row, int col);

        // ===== Nội dung/điều hướng — triển khai mặc định cho mọi grid =====
        public bool HasContent(int row, int col)
        {
            ValidateIndex(row, col);
            return _content[row, col] != null;
        }

        public GameObject GetContent(int row, int col)
        {
            ValidateIndex(row, col);
            return _content[row, col];
        }

        public void SetContent(int row, int col, GameObject content)
        {
            ValidateIndex(row, col);
            _content[row, col] = content;
        }

        public bool ClearContent(int row, int col)
        {
            ValidateIndex(row, col);
            bool had = _content[row, col] != null;
            _content[row, col] = null;
            return had;
        }

        public bool IsWalkable(int row, int col)
        {
            ValidateIndex(row, col);
            return _walkable[row, col];
        }

        public void SetWalkable(int row, int col, bool walkable)
        {
            ValidateIndex(row, col);
            _walkable[row, col] = walkable;
        }

        public float GetCost(int row, int col)
        {
            ValidateIndex(row, col);
            return _cost[row, col];
        }

        public void SetCost(int row, int col, float cost)
        {
            if (cost <= 0f) throw new ArgumentOutOfRangeException(nameof(cost), "cost must be > 0");
            ValidateIndex(row, col);
            _cost[row, col] = cost;
        }

        public CellInfo GetCellInfo(int row, int col)
        {
            ValidateIndex(row, col);
            var obj = _content[row, col];
            return new CellInfo(obj != null, obj, _walkable[row, col], _cost[row, col]);
        }
    }
}
