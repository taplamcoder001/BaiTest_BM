using CNV.CreateMaze;
using CNV.GirdCore;
using CNV.Maze;
using CNV.Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;
using EditorAttributes;
using Unity.VisualScripting;

public sealed class MazeController : MonoBehaviour
{
    [SerializeField] private CameraTopDownAutoFit cameraAutoFit;
    [SerializeField] private MazeDataSO mazeDataSO;
    [SerializeField] private GirdManager girdManager;

    private MazeGenerator _mazeGenerator;
    public Agent Agent => _mazeGenerator != null ? _mazeGenerator.Agent : null;
    private IMazeCreatable _mazeAlgorithm;
    private IPathfindable _pathfinder;

    private int[,] _mazeGrid;
    public int[,] MazeGrid => _mazeGrid;

    private Vector2Int _startRc;
    private Vector2Int _goalRC;

    private bool _initialized;

    private void Awake()
    {
        // Khởi tạo các service 1 lần
        _mazeAlgorithm = new CreateMazeDivision();
        _pathfinder = new PathfinderAStar4();
        _initialized = true;
    }

    private void Start()
    {
        GenerateMaze(mazeDataSO.GirdData.rows,  
            mazeDataSO.GirdData.cols);
    }

    /// <summary>
    /// API CÔNG KHAI – gọi hàm này để tạo maze mới
    /// </summary>
    [Button]
    public void GenerateMaze(int cols, int rows)
    {
        if (!_initialized)
            Awake();

        _mazeGenerator?.Clear();

        var runtimeGridData = mazeDataSO.GirdData;
        runtimeGridData.cols = cols;
        runtimeGridData.rows = rows;

        // 2️⃣ Set & reset grid
        girdManager.SetData(runtimeGridData);
        girdManager.ForceRebuild(); // 👈 bạn sẽ thêm hàm này

        // 3️⃣ Generate maze
        _mazeGrid = _mazeAlgorithm.CreateMaze(
            cols,
            rows,
            Random.Range(0, int.MaxValue)
        );

        CreateEntranceAndExit(_mazeGrid, cols, rows);

        // 4️⃣ Render
        _mazeGenerator = new MazeGenerator(girdManager, mazeDataSO);
        _mazeGenerator.Render(_mazeGrid, _startRc, _goalRC);

        cameraAutoFit.Fit(girdManager);
    }

    #region Entrance / Exit

    private void CreateEntranceAndExit(int[,] grid, int cols, int rows)
    {
        // ==== ENTRANCE: LEFT ====
        int startY = 0;
        grid[1, startY] = 0;
        _startRc = new Vector2Int(1, startY);

        // ==== EXIT: RIGHT ====
        int goalY = rows - 1;
        grid[cols - 2, goalY] = 0;
        _goalRC = new Vector2Int(cols - 2, goalY);
    }

    #endregion

    #region Pathfinding & Agent

    [Button]
    public void FindAndMoveAgent()
    {
        Agent agent = Agent;
        if (agent == null)
        {
            Debug.LogWarning("Agent not spawned yet");
            return;
        }

        Vector2Int fromRC = agent.CurrentRC;
        Vector2Int toRC = _goalRC;

        if (_mazeGrid[fromRC.x, fromRC.y] == 1)
        {
            Debug.LogError($"Agent is standing on WALL at {fromRC}");
            return;
        }

        if (_mazeGrid[toRC.x, toRC.y] == 1)
        {
            Debug.LogError($"Goal is on WALL at {toRC}");
            return;
        }

        var pathRC = _pathfinder.FindPath(
            _mazeGrid,
            fromRC,
            toRC
        );

        if (pathRC == null || pathRC.Count == 0)
        {
            Debug.LogWarning("No path found from agent to goal");
            return;
        }

        // Visual
        _mazeGenerator.DrawPath(pathRC);

        // ✅ QUAN TRỌNG: truyền RC, KHÔNG truyền world
        agent.MoveAlongPathRC(pathRC, girdManager);
    }

    public bool TryMoveAgentTo(Vector2Int targetRC)
    {
        Agent agent = Agent;
        if (agent == null)
            return false;

        Vector2Int fromRC = agent.CurrentRC;

        if (_mazeGrid[fromRC.x, fromRC.y] == 1 ||
            _mazeGrid[targetRC.x, targetRC.y] == 1)
            return false;

        var pathRC = _pathfinder.FindPath(
            _mazeGrid,
            fromRC,
            targetRC
        );

        if (pathRC == null || pathRC.Count == 0)
            return false;

        _mazeGenerator.DrawPath(pathRC);

        // ✅ move by RC so CurrentRC stays correct
        agent.MoveAlongPathRC(pathRC, girdManager);

        return true;
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (girdManager != null)
            girdManager.OnDrawGizmos();
    }
#endif
}