using UnityEngine;
using CNV.GirdCore;

[RequireComponent(typeof(Camera))]
public class CameraTopDownAutoFit : MonoBehaviour
{
    [Header("View Angle")]
    [Range(30f, 80f)]
    [SerializeField] private float pitch = 55f;
    [Range(0f, 360f)]
    [SerializeField] private float yaw = 45f;

    [Header("Fit Settings")]
    [SerializeField] private float padding = 1.2f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 200f;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = false;
    }

    public void Fit(GirdManager girdManager)
    {
        Vector3 center = girdManager.GetWorldCenter();

        Vector3 size = girdManager.GetWorldSize();

        float maxExtent = Mathf.Max(size.x, size.z) * 0.5f * padding;

        float halfFovRad = _cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float distance = maxExtent / Mathf.Tan(halfFovRad);

        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = center - transform.forward * distance;
    }
}