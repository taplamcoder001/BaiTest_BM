using System.Collections.Generic;
using UnityEngine;
using System;
using Random = System.Random;

namespace CNV.CreateMaze {
    public class CreateMazeEller : IMazeCreatable {
        public int w, h;

        public int[,] CreateMaze(int width, int height, int seed) {
            // Kỳ vọng w,h là số lẻ >= 3 (phòng ở tọa độ lẻ). Nếu chẵn, vẫn chạy nhưng biên có thể hẹp.
            w = Mathf.Max(width, 3);
            h = Mathf.Max(height, 3);

            int cellsW = (w - 1) / 2;
            int cellsH = (h - 1) / 2;

            var m = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                m[x, y] = 1; // toàn tường

            var rng = new Random(seed);

            // Set id cho từng ô của HÀNG hiện tại; -1 = chưa gán
            int[] setId = new int[cellsW];
            int[] nextSetId = new int[cellsW];
            for (int i = 0; i < cellsW; i++) setId[i] = -1;

            int nextId = 1;

            // Trợ giúp: chuyển "phòng (cx,cy)" sang toạ độ mảng m
            static Vector2Int RoomToGrid(int cx, int cy) => new Vector2Int(1 + 2 * cx, 1 + 2 * cy);

            for (int cy = 0; cy < cellsH; cy++) {
                // Bảo đảm mỗi cell trong hàng có 1 set id
                for (int cx = 0; cx < cellsW; cx++) {
                    if (setId[cx] == -1) setId[cx] = nextId++;
                    var rg = RoomToGrid(cx, cy);
                    m[rg.x, rg.y] = 0; // mở phòng
                }

                // --- Nối ngang (horizontal) ---
                // Ở hàng cuối, ta bắt buộc hợp nhất tất cả set bằng cách nối ngang
                bool lastRow = (cy == cellsH - 1);

                for (int cx = 0; cx < cellsW - 1; cx++) {
                    bool shouldJoin = rng.Next(2) == 0; // 50%
                    if (lastRow) shouldJoin = (setId[cx] != setId[cx + 1]); // ép nối để tất cả hợp nhất

                    if (shouldJoin && setId[cx] != setId[cx + 1]) {
                        // đục tường giữa (cx,cy) và (cx+1,cy)
                        var a = RoomToGrid(cx, cy);
                        m[a.x + 1, a.y] = 0;

                        int from = setId[cx];
                        int to = setId[cx + 1];
                        // hợp nhất set: chuyển tất cả "to" về "from"
                        for (int i = 0; i < cellsW; i++)
                            if (setId[i] == to)
                                setId[i] = from;
                    }
                }

                // Hàng cuối không cần nối dọc
                if (lastRow) break;

                // --- Nối dọc (vertical) ---
                // Với mỗi set trong hàng, chọn >=1 cell xuống hàng dưới
                for (int i = 0; i < cellsW; i++) nextSetId[i] = -1;

                // Gom cell theo set
                var setToCells = new Dictionary<int, List<int>>();
                for (int cx = 0; cx < cellsW; cx++) {
                    int id = setId[cx];
                    if (!setToCells.TryGetValue(id, out var list))
                        setToCells[id] = list = new List<int>();
                    list.Add(cx);
                }

                foreach (var kv in setToCells) {
                    var cells = kv.Value;

                    // Chọn ngẫu nhiên các cell sẽ mở đường xuống (mỗi set phải có ÍT NHẤT 1)
                    var chosen = new List<int>();
                    foreach (int cx in cells)
                        if (rng.Next(2) == 0)
                            chosen.Add(cx); // ~50%

                    if (chosen.Count == 0)
                        chosen.Add(cells[rng.Next(cells.Count)]); // đảm bảo >=1

                    // Đục tường xuống dưới và mang setId xuống hàng tiếp theo
                    foreach (int cx in chosen) {
                        var a = RoomToGrid(cx, cy);
                        m[a.x, a.y + 1] = 0; // đục tường dọc
                        var b = RoomToGrid(cx, cy + 1);
                        m[b.x, b.y] = 0; // mở phòng bên dưới
                        nextSetId[cx] = setId[cx]; // giữ nguyên set xuống hàng sau
                    }
                }

                // Các cell không được nối xuống sẽ được setId = -1 ở hàng kế → sẽ tạo set mới
                for (int cx = 0; cx < cellsW; cx++)
                    setId[cx] = nextSetId[cx];
            }

            return m;
        }
    }
}