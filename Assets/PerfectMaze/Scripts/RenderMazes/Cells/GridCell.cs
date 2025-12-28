using UnityEngine;

public class GridCell : MonoBehaviour
{
    [HideInInspector] public Vector2Int GridPos;
    [HideInInspector] public bool Walkable = true;
    public CellPaint Paint;

    [SerializeField] private CellsVisuals visuals;

    public void SetPaint(CellPaint paint)
    {
        Paint = paint;
        switch (paint)
        {
            case CellPaint.Wall:
                Walkable = false;
                break;

            case CellPaint.Empty:
            case CellPaint.Start:
            case CellPaint.Goal:
            case CellPaint.Path:
            case CellPaint.Visited:
                Walkable = true;
                break;
        }

        if (visuals != null)
            visuals.SetVisual(paint);
    }
}
