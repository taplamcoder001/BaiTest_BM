using UnityEngine;
using Random = System.Random;

namespace CNV.CreateMaze {
    public class CreateMazeDivision : IMazeCreatable {
        Random rng;

        public int[,] CreateMaze(int width, int height, int seed) {
            int w = Mathf.Max(width, 3);
            int h = Mathf.Max(height, 3);
            rng = new Random(seed);

            // 1 = wall, 0 = passage
            var m = new int[w, h];

            // B1) Viền ngoài là tường
            for (int x = 0; x < w; x++) {
                m[x, 0] = 1;
                m[x, h - 1] = 1;
            }

            for (int y = 0; y < h; y++) {
                m[0, y] = 1;
                m[w - 1, y] = 1;
            }

            // B2) Bên trong mặc định là đường đi (sau đó chia tường đệ quy)
            for (int y = 1; y < h - 1; y++)
            for (int x = 1; x < w - 1; x++)
                m[x, y] = 0;

            // Chia đệ quy vùng trong viền (1..w-2, 1..h-2)
            Divide(m, 1, 1, w - 2, h - 2);

            return m;
        }

        void Divide(int[,] m, int x0, int y0, int x1, int y1) {
            // Kích thước vùng hiện tại (theo toạ độ tile)
            int w = x1 - x0 + 1;
            int h = y1 - y0 + 1;

            // Nếu vùng quá mỏng, dừng
            if (w < 3 || h < 3) return;

            // Chọn hướng vẽ tường: ưu tiên hướng dài hơn (hoặc ngẫu nhiên)
            bool horizontal = w < h ? true : (w > h ? false : rng.Next(2) == 0);

            if (horizontal) {
                // Chọn hàng tường yWall là CHẴN bên trong [y0+1 .. y1-1]
                int yWall = PickEven(y0 + 1, y1 - 1);
                if (yWall == -1) return;

                // Vẽ tường ngang
                for (int x = x0; x <= x1; x++) m[x, yWall] = 1;

                // Đục 1 cửa tại cột lẻ
                int xHole = PickOdd(x0, x1);
                if (xHole != -1) m[xHole, yWall] = 0;

                // Chia 2 vùng: trên và dưới
                Divide(m, x0, y0, x1, yWall - 1);
                Divide(m, x0, yWall + 1, x1, y1);
            }
            else {
                // Chọn cột tường xWall là CHẴN bên trong [x0+1 .. x1-1]
                int xWall = PickEven(x0 + 1, x1 - 1);
                if (xWall == -1) return;

                // Vẽ tường dọc
                for (int y = y0; y <= y1; y++) m[xWall, y] = 1;

                // Đục 1 cửa tại hàng lẻ
                int yHole = PickOdd(y0, y1);
                if (yHole != -1) m[xWall, yHole] = 0;

                // Chia 2 vùng: trái và phải
                Divide(m, x0, y0, xWall - 1, y1);
                Divide(m, xWall + 1, y0, x1, y1);
            }
        }

        int PickEven(int a, int b) {
            // Lấy 1 số CHẴN trong [a..b], trả -1 nếu không có
            if (a > b) return -1;
            int first = (a % 2 == 0) ? a : a + 1;
            if (first > b) return -1;
            int count = (b - first) / 2 + 1;
            int k = rng.Next(count);
            return first + 2 * k;
        }

        int PickOdd(int a, int b) {
            // Lấy 1 số LẺ trong [a..b], trả -1 nếu không có
            if (a > b) return -1;
            int first = (a % 2 != 0) ? a : a + 1;
            if (first > b) return -1;
            int count = (b - first) / 2 + 1;
            int k = rng.Next(count);
            return first + 2 * k;
        }
    }
}