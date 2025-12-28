using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CNV.GirdCore
{
    [Serializable]
    public class GirdManager
    {
        #region Setting
        public GirdData GirdData;
        [Header("Shared Layout")] 
        public Transform pointTransform;
        
        [Header("Gizmos")] public bool drawOutlines = true;
        public bool drawCenters = true;
        public bool drawIndices = false;
        public bool drawWalkableOverlay = true;
        public bool drawContentMarker = true;

        [Min(0f)] public float centerSphereRadius = 0.05f;
        [Range(0.05f, 1.0f)] public float contentMarkerSize = 0.25f;

        public Color outlineColor = new Color(0f, 0f, 0f, 0.8f);
        public Color centerColor = new Color(0.15f, 0.7f, 1f, 0.95f);
        public Color indexColor = new Color(1f, 0.6f, 0.1f, 1f);
        public Color blockedFill = new Color(1f, 0f, 0f, 0.18f);
        public Color blockedOutline = new Color(1f, 0f, 0f, 0.9f);
        public Color contentColor = new Color(0.2f, 0.85f, 0.2f, 0.95f);

        #endregion

        private readonly List<Vector3> _tmp3 = new List<Vector3>(8);

        // ========= RUNTIME CACHE =========
        private IGrid2D _runtimeGrid;

        public IGrid2D GetOrBuildGrid()
        {
            if (_runtimeGrid == null)
            {
                _runtimeGrid = BuildGrid();
            }
            return _runtimeGrid;
        }

        // (row,col) -> world center (X,Z) với mapping có thể đảo
        public Vector3 GetWorldCenterFromRC(Vector2Int rc, float addY = 0f)
        {
            var g = GetOrBuildGrid();
            Vector2 c2;

            // Mặc định (gốc grid): col -> X, row -> Z. Khi invert, hoán vị chỉ số khi hỏi grid.
            if (GirdData.invertRCtoXZ)
            {
                // muốn X phụ thuộc row, Z phụ thuộc col -> gọi GetCenter(col=row, row=col)
                c2 = g.GetCenter(rc.y, rc.x);
            }
            else
            {
                c2 = g.GetCenter(rc.x, rc.y);
            }

            float y = pointTransform.position.y + GirdData.origin.y + addY;
            return new Vector3(c2.x, y, c2.y);
        }

        public bool IsWalkableRC(Vector2Int rc)
        {
            var g = GetOrBuildGrid();
            int qr, qc;
            if (GirdData.invertRCtoXZ)
            {
                qr = rc.y;
                qc = rc.x;
            }
            else
            {
                qr = rc.x;
                qc = rc.y;
            }

            if ((uint)qr >= (uint)g.Rows || (uint)qc >= (uint)g.Cols) return false;
            return g.IsWalkable(qr, qc);
        }
        
        public Vector3 GetWorldCenter(float addY = 0f)
        {
            var grid = GetOrBuildGrid();

            // Lấy tâm 2 cell đối diện
            Vector2 min = grid.GetCenter(0, 0);
            Vector2 max = grid.GetCenter(grid.Rows - 1, grid.Cols - 1);

            Vector2 center2 = (min + max) * 0.5f;

            float y = pointTransform.position.y + GirdData.origin.y + addY;
            return new Vector3(center2.x, y, center2.y);
        }

        public Vector3 GetWorldSize()
        {
            var grid = GetOrBuildGrid();

            Vector2 min = grid.GetCenter(0, 0);
            Vector2 max = grid.GetCenter(grid.Rows - 1, grid.Cols - 1);

            float width = Mathf.Abs(max.x - min.x);
            float depth = Mathf.Abs(max.y - min.y);

            return new Vector3(width, 0f, depth);
        }

        public void SetData(GirdData data)
        {
            GirdData = data;
        }

        private void OnValidate()
        {
            _runtimeGrid = null;
        }

        // ========= BUILD GRID (chuẩn col->X, row->Z) =========
        private IGrid2D BuildGrid()
        {
            var worldOrigin2D = new Vector2(
                pointTransform.position.x + GirdData.origin.x,
                pointTransform.position.z + GirdData.origin.z
            );

            switch (GirdData.gridType)
            {
                case GridType.Square:
                    return new SquareGrid(GirdData.rows, GirdData.cols, GirdData.squareCellSize, worldOrigin2D,
                        GirdData.gap); // col->X, row->Z  :contentReference[oaicite:0]{index=0}

                case GridType.HexPointy:
                    return new HexGridPointy(
                        GirdData.rows, GirdData.cols, GirdData.hexRadius, worldOrigin2D, GirdData.gap,
                        parity: (GirdData.hexRowParity == RowParity.EvenR)
                            ? HexGridPointy.RowParity.EvenR
                            : HexGridPointy.RowParity.OddR
                    ); // col->X, row->Z  :contentReference[oaicite:1]{index=1}
            }

            return null;
        }
        
        public void ForceRebuild()
        {
            _runtimeGrid = null;
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            var grid = BuildGrid();
            if (grid == null) return;

            float y = pointTransform.position.y + GirdData.origin.y;

            // 1) Overlay walkable/blocked + content
            for (int r = 0; r < grid.Rows; r++)
            for (int c = 0; c < grid.Cols; c++)
            {
                var poly2 = grid.GetCellPolygon(r,
                    c); // hình học gốc col->X,row->Z  :contentReference[oaicite:2]{index=2}
                ToWorld3Loop(poly2, y, _tmp3);

                if (drawWalkableOverlay && !grid.IsWalkable(r, c))
                {
                    Gizmos.color = blockedFill;
                    for (int i = 1; i < _tmp3.Count - 1; i++)
                        Gizmos.DrawLine(_tmp3[0], _tmp3[i]);

                    Gizmos.color = blockedOutline;
                    DrawPolylineLoop(_tmp3);
                }

                if (drawContentMarker && grid.HasContent(r, c))
                {
                    var center2 = grid.GetCenter(r, c);
                    var center3 = new Vector3(center2.x, y, center2.y);
                    Gizmos.color = contentColor;
                    float s = contentMarkerSize * GuessCellScale(grid);
                    Gizmos.DrawCube(center3, new Vector3(s, s, s));
                }
            }

            // 2) Viền & tâm
            if (drawOutlines)
            {
                Gizmos.color = outlineColor;
                foreach (var poly2 in grid.EnumerateCellPolygons())
                {
                    ToWorld3Loop(poly2, y, _tmp3);
                    DrawPolylineLoop(_tmp3);
                }
            }

            if (drawCenters)
            {
                Gizmos.color = centerColor;
                foreach (var c2 in grid.EnumerateCenters())
                    Gizmos.DrawSphere(new Vector3(c2.x, y, c2.y), centerSphereRadius);
            }

            // 3) Nhãn chỉ số (theo hình học gốc)
            if (drawIndices)
            {
                Handles.color = indexColor;
                for (int r = 0; r < grid.Rows; r++)
                for (int c = 0; c < grid.Cols; c++)
                {
                    var center2 = grid.GetCenter(r, c);
                    Handles.Label(new Vector3(center2.x, y, center2.y), $"({r},{c})");
                }
            }
        }

        private static void ToWorld3Loop(IReadOnlyList<Vector2> poly2, float y, List<Vector3> buffer)
        {
            buffer.Clear();
            int n = poly2.Count;
            if (buffer.Capacity < n) buffer.Capacity = n;
            for (int i = 0; i < n; i++)
            {
                var p = poly2[i];
                buffer.Add(new Vector3(p.x, y, p.y));
            }
        }

        private static void DrawPolylineLoop(List<Vector3> pts)
        {
            int n = pts.Count;
            if (n < 2) return;
            for (int i = 0; i < n; i++)
            {
                var a = pts[i];
                var b = pts[(i + 1) % n];
                Gizmos.DrawLine(a, b);
            }
        }

        private float GuessCellScale(IGrid2D grid)
        {
            return (grid is SquareGrid sg) ? sg.CellSize
                : (grid is HexGridPointy hg) ? hg.Radius * 0.8f
                : 1f;
        }
#endif
    }
}