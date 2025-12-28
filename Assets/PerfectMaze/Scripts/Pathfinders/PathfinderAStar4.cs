using System;
using System.Collections.Generic;
using UnityEngine;

namespace CNV.Pathfinding
{
    public class PathfinderAStar4 : IPathfindable
    {
        static readonly Vector2Int[] Dirs4 =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1),
        };

        public List<Vector2Int> FindPath(
            int[,] grid, Vector2Int start, Vector2Int goal,
            Action<Vector2Int> onExpand = null)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);

            var gScore = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                gScore[x, y] = float.PositiveInfinity;

            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var closed = new HashSet<Vector2Int>();

            var open = new MinHeap();
            gScore[start.x, start.y] = 0f;
            open.Push(start, Heuristic(start, goal));

            while (open.Count > 0)
            {
                var current = open.Pop();
                if (closed.Contains(current))
                    continue;

                closed.Add(current);
                onExpand?.Invoke(current);

                if (current == goal)
                    return ReconstructPath(cameFrom, current);

                float gCur = gScore[current.x, current.y];

                foreach (var d in Dirs4)
                {
                    var nb = current + d;
                    if (!InBounds(nb, w, h)) continue;
                    if (grid[nb.x, nb.y] == 1) continue;
                    if (closed.Contains(nb)) continue;

                    float tentative = gCur + 1f;
                    if (tentative < gScore[nb.x, nb.y])
                    {
                        cameFrom[nb] = current;
                        gScore[nb.x, nb.y] = tentative;
                        float f = tentative + Heuristic(nb, goal);
                        open.Push(nb, f);
                    }
                }
            }

            return null;
        }

        static float Heuristic(Vector2Int a, Vector2Int b)
            => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        static bool InBounds(Vector2Int p, int w, int h)
            => p.x >= 0 && p.x < w && p.y >= 0 && p.y < h;

        static List<Vector2Int> ReconstructPath(
            Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int cur)
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

        class MinHeap
        {
            private readonly List<(Vector2Int pos, float f)> _data = new();
            public int Count => _data.Count;

            public void Push(Vector2Int pos, float f)
            {
                _data.Add((pos, f));
                SiftUp(_data.Count - 1);
            }

            public Vector2Int Pop()
            {
                var root = _data[0].pos;
                int last = _data.Count - 1;
                _data[0] = _data[last];
                _data.RemoveAt(last);
                if (_data.Count > 0) SiftDown(0);
                return root;
            }

            void SiftUp(int i)
            {
                while (i > 0)
                {
                    int p = (i - 1) / 2;
                    if (_data[p].f <= _data[i].f) break;
                    (_data[p], _data[i]) = (_data[i], _data[p]);
                    i = p;
                }
            }

            void SiftDown(int i)
            {
                int n = _data.Count;
                while (true)
                {
                    int l = 2 * i + 1, r = 2 * i + 2, s = i;
                    if (l < n && _data[l].f < _data[s].f) s = l;
                    if (r < n && _data[r].f < _data[s].f) s = r;
                    if (s == i) break;
                    (_data[s], _data[i]) = (_data[i], _data[s]);
                    i = s;
                }
            }
        }
    }
}
