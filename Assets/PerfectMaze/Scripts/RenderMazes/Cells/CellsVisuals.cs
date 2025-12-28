using System.Collections.Generic;
using UnityEngine;

public class CellsVisuals : MonoBehaviour
{
    [SerializeField]
    private CellVisualEntry[] visuals;

    private Dictionary<CellPaint, GameObject> _map;
    private GameObject _current;

    private void Awake()
    {
        _map = new Dictionary<CellPaint, GameObject>();

        foreach (var v in visuals)
        {
            if (v.visual == null)
                continue;

            _map[v.paint] = v.visual;
            v.visual.SetActive(false);
        }
    }

    public void SetVisual(CellPaint paint)
    {
        if (_current != null)
            _current.SetActive(false);

        if (_map.TryGetValue(paint, out var go))
        {
            go.SetActive(true);
            _current = go;
        }
        else
        {
            _current = null;
        }
    }
}

