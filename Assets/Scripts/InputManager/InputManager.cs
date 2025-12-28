using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private MazeController mazeController;

    [Header("Raycast")]
    [SerializeField] private LayerMask cellLayer;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    private void HandleClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, cellLayer))
            return;

        GridCell cell = hit.collider.GetComponent<GridCell>();
        if (cell == null)
            return;

        if (!cell.Walkable)
            return;

        if (mazeController == null)
            return;

        // ✅ Chỉ REQUEST di chuyển
        bool success = mazeController.TryMoveAgentTo(cell.GridPos);

        if (!success)
        {
            Debug.Log("Cannot move agent to selected cell");
        }
    }
}