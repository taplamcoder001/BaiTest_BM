using CNV.GirdCore;
using UnityEngine;
using System.Collections.Generic;

namespace CNV.Maze
{
    public sealed class MazeGenerator
    {
        private readonly GirdManager _girdManager;
        private readonly GridCell _cellPrefab;
        private readonly Agent _agentPrefab;

        private GridCell[,] _cells;
        private Agent _agent;

        public Agent Agent => _agent;

        // Lưu để DrawPath không đè
        private Vector2Int _startOutsideRC;
        private Vector2Int _goalOutsideRC;

        public MazeGenerator(GirdManager girdManager, MazeDataSO dataSO)
        {
            _girdManager = girdManager;
            _cellPrefab = dataSO.cellPrefab;
            _agentPrefab = dataSO.agentPrefab;
        }

        public void Render(
            int[,] mazeGrid,
            Vector2Int startOutsideRC,
            Vector2Int goalOutsideRC)
        {
            ValidateContract(mazeGrid);
            Clear();

            _startOutsideRC = startOutsideRC;
            _goalOutsideRC = goalOutsideRC;

            int cols = mazeGrid.GetLength(0);
            int rows = mazeGrid.GetLength(1);

            _cells = new GridCell[cols, rows];

            for (int col = 0; col < cols; col++)
            for (int row = 0; row < rows; row++)
            {
                var rc = new Vector2Int(col, row);
                var cell = Object.Instantiate(
                    _cellPrefab,
                    _girdManager.GetWorldCenterFromRC(rc),
                    Quaternion.identity
                );

                cell.GridPos = rc;
                cell.SetPaint(mazeGrid[col, row] == 1 ? CellPaint.Wall : CellPaint.Empty);
                _cells[col, row] = cell;
            }

            // ✅ Paint Start/Goal ở CẠNH NGOÀI (đúng ý bạn)
            _cells[startOutsideRC.x, startOutsideRC.y].SetPaint(CellPaint.Start);
            _cells[goalOutsideRC.x, goalOutsideRC.y].SetPaint(CellPaint.Goal);

            SpawnAgent(startOutsideRC);
        }

        public void DrawPath(IReadOnlyList<Vector2Int> path)
        {
            if (path == null || _cells == null) return;

            foreach (var rc in path)
            {
                // ✅ Không đè Start/Goal ở mép ngoài
                if (rc == _startOutsideRC || rc == _goalOutsideRC)
                    continue;

                var cell = _cells[rc.x, rc.y];
                if (cell == null) continue;

                // Nếu path đi qua ô start/goal inside mà bạn muốn giữ màu khác, bạn có thể thêm điều kiện tại đây.
                cell.SetPaint(CellPaint.Path);
            }
        }

        private void SpawnAgent(Vector2Int outsideRC)
        {
            _agent = Object.Instantiate(
                _agentPrefab,
                _girdManager.GetWorldCenterFromRC(outsideRC),
                Quaternion.identity
            );
            _agent.SnapToCell(outsideRC, _girdManager.GetWorldCenterFromRC(outsideRC));
        }

        private void ValidateContract(int[,] mazeGrid)
        {
            var grid = _girdManager.GetOrBuildGrid();
            if (mazeGrid.GetLength(0) != grid.Cols ||
                mazeGrid.GetLength(1) != grid.Rows)
            {
                throw new System.InvalidOperationException(
                    $"Maze/Grid size mismatch. Maze=[{mazeGrid.GetLength(0)},{mazeGrid.GetLength(1)}], " +
                    $"Grid=[{grid.Cols},{grid.Rows}]"
                );
            }
        }

        public void Clear()
        {
            if (_cells != null)
            {
                foreach (var c in _cells)
                    if (c != null)
                        Object.Destroy(c.gameObject);
            }

            if (_agent != null)
                Object.Destroy(_agent.gameObject);
        }
    }
}
