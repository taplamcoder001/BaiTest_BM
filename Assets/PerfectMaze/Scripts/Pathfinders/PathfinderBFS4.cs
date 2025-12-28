using System;
using System.Collections.Generic;
using UnityEngine;

namespace CNV.Pathfinding {
    public sealed class PathfinderBFS4 : IPathfindable
    {
        static readonly Vector2Int[] Dirs4 = new[]
        {
            new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(0,1), new Vector2Int(0,-1),
        };

        public List<Vector2Int> FindPath(
            int[,] grid, Vector2Int start, Vector2Int goal,
            Action<Vector2Int> onExpand = null)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);

            var q = new Queue<Vector2Int>();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var visited = new bool[w, h];

            q.Enqueue(start);
            visited[start.x, start.y] = true;

            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                onExpand?.Invoke(cur);

                if (cur == goal)
                    return ReconstructPath(cameFrom, cur);

                for (int i = 0; i < Dirs4.Length; i++)
                {
                    var nb = cur + Dirs4[i];
                    if (nb.x < 0 || nb.x >= w || nb.y < 0 || nb.y >= h) continue;
                    if (grid[nb.x, nb.y] == 1) continue;
                    if (visited[nb.x, nb.y]) continue;

                    visited[nb.x, nb.y] = true;
                    cameFrom[nb] = cur;
                    q.Enqueue(nb);
                }
            }
            return null;
        }

        static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int cur)
        {
            var path = new List<Vector2Int> { cur };
            while (cameFrom.TryGetValue(cur, out var prev))
            {
                cur = prev;
                path.Add(cur);
            }
            path.Reverse();
            return path;
        }
    }
}