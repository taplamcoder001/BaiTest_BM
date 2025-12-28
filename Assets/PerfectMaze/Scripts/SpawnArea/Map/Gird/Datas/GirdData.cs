using System;
using UnityEngine;

namespace CNV.GirdCore
{
    [Serializable]
    public class GirdData
    {
        [Header("Grid Type")] public GridType gridType = GridType.Square;

        [Header("Shared Layout")] 
        [Min(4)] public int rows = 5;
        [Min(4)] public int cols = 8;
        public Vector3 origin = Vector3.zero;
        [Min(0f)] public float gap = 0f;

        [Header("Square Settings")] [Min(0.01f)]
        public float squareCellSize = 1f;

        [Header("Hex (Pointy-Top) Settings")] [Min(0.01f)]
        public float hexRadius = 0.6f;

        public RowParity hexRowParity = RowParity.EvenR;

        [Header("Axis Mapping")] [Tooltip("Bật để đảo ánh xạ: row -> X, col -> Z (ngược với mặc định col->X, row->Z).")]
        public bool invertRCtoXZ = true;
    }
}