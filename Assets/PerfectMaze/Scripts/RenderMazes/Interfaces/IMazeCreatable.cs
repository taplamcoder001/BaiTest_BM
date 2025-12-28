namespace CNV.CreateMaze {
    public interface IMazeCreatable {
        int[,] CreateMaze(int width, int height, int seed);
    }
}
