using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CNV.GirdCore;

public class Agent : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f; // world units / second
    private Coroutine _moveRoutine;

    public Vector2Int CurrentRC { get; private set; }

    public void SnapToCell(Vector2Int rc, Vector3 worldPos)
    {
        CurrentRC = rc;
        transform.position = worldPos;
    }

    /// <summary>
    /// Move by grid path (RC). This keeps CurrentRC correct.
    /// </summary>
    public void MoveAlongPathRC(IReadOnlyList<Vector2Int> pathRC, GirdManager girdManager)
    {
        if (pathRC == null || pathRC.Count == 0 || girdManager == null)
            return;

        if (_moveRoutine != null)
            StopCoroutine(_moveRoutine);

        _moveRoutine = StartCoroutine(CoMoveRC(pathRC, girdManager));
    }

    private IEnumerator CoMoveRC(IReadOnlyList<Vector2Int> pathRC, GirdManager girdManager)
    {
        // snap to first node
        CurrentRC = pathRC[0];
        transform.position = girdManager.GetWorldCenterFromRC(CurrentRC);

        for (int i = 1; i < pathRC.Count; i++)
        {
            Vector2Int nextRC = pathRC[i];
            Vector3 from = transform.position;
            Vector3 to = girdManager.GetWorldCenterFromRC(nextRC);

            float dist = Vector3.Distance(from, to);
            float duration = dist / Mathf.Max(0.01f, moveSpeed);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(from, to, t);
                yield return null;
            }

            // ✅ update logical position when reach the cell
            CurrentRC = nextRC;
        }

        _moveRoutine = null;
    }
}