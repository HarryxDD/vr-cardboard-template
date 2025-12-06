using UnityEngine;
using System.Collections.Generic;

public class RayEmitter : MonoBehaviour
{
    public enum RayPattern
    {
        Single,           // One ray
        MultiRay,         // Multiple parallel rays in circular pattern
        ChromaticFan,     // RGB rays overlapping (for chromatic aberration)
        ParallelFan       // Parallel rays at different heights (for spherical aberration)
    }

    [Header("Ray Pattern")]
    public RayPattern pattern = RayPattern.Single;
    public int numberOfRays = 7;
    public float raySpread = 0.02f; // Vertical or radial spread

    [Header("Appearance")]
    public Color rayColor = Color.cyan;
    public float rayWidth = 0.002f;
    public float maxRayDistance = 10f;

    [Header("Chromatic Aberration Settings")]
    public bool enableChromaticAberration = false;
    public float chromaticSpread = 0.016f; // Difference between red and blue

    [Header("Debug")]
    public bool showDebugInfo = false;

    [Header("Performance")]
    [Tooltip("If true, rays only update once at start (best for static optical benches)")]
    public bool staticScene = true;
    public bool updateEveryFrame = false; // Set to false for static scenes
    public float updateInterval = 0.1f; // Update 10 times per second instead of 60
    public int maxRayBounces = 4; // Reduce for better performance (minimum 2 for one lens)
    [Tooltip("Process ray updates over multiple frames to avoid stalls")]
    public bool asyncTracing = true;
    [Tooltip("How many rays to process per frame when asyncTracing is enabled")]
    public int raysPerFrame = 3;

    private List<LineRenderer> rayRenderers = new List<LineRenderer>();
    private List<RayData> rayDataList = new List<RayData>();
    private List<Vector3> pointsBuffer = new List<Vector3>(50); // Reusable buffer - huge performance gain!
    private float lastUpdateTime = 0f;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private bool hasUpdatedOnce = false;
    private Coroutine tracingCoroutine;

    private struct RayData
    {
        public Vector3 offset;
        public Color color;
        public float refractiveIndexOffset; // Offset from lens base refractive index
    }

    void Start()
    {
        CreateRays();
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void Update()
    {
        // STATIC MODE: Only update once, then stop completely (massive FPS gain!)
        if (staticScene)
        {
            if (!hasUpdatedOnce)
            {
                TraceAllRays();
                hasUpdatedOnce = true;
            }
            return; // Skip all updates after first frame
        }

        // DYNAMIC MODE: Only update if something changed or enough time passed
        bool positionChanged = Vector3.Distance(transform.position, lastPosition) > 0.001f;
        bool rotationChanged = Quaternion.Angle(transform.rotation, lastRotation) > 0.1f;
        bool timeElapsed = Time.time - lastUpdateTime >= updateInterval;

        if (updateEveryFrame || positionChanged || rotationChanged || timeElapsed)
        {
            ScheduleUpdate();
            lastUpdateTime = Time.time;
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
    }

    void CreateRays()
    {
        // Clear existing
        foreach (var ray in rayRenderers)
        {
            if (ray != null) Destroy(ray.gameObject);
        }
        rayRenderers.Clear();
        rayDataList.Clear();

        switch (pattern)
        {
            case RayPattern.Single:
                CreateSingleRay();
                break;
            case RayPattern.MultiRay:
                CreateMultiRay();
                break;
            case RayPattern.ChromaticFan:
                CreateChromaticFan();
                break;
            case RayPattern.ParallelFan:
                CreateParallelFan();
                break;
        }
    }

    void CreateSingleRay()
    {
        AddRay(Vector3.zero, rayColor, 0f);
    }

    void CreateMultiRay()
    {
        // Circular pattern of rays
        float angleStep = 360f / numberOfRays;
        for (int i = 0; i < numberOfRays; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * raySpread,
                Mathf.Sin(angle) * raySpread,
                0f
            );
            AddRay(offset, rayColor, 0f);
        }
    }

    void CreateChromaticFan()
    {
        // Create groups of RGB rays for chromatic aberration
        int whiteRayCount = numberOfRays / 3;
        Color[] colors = { Color.red, Color.green, Color.blue };
        float[] nOffsets = { -chromaticSpread / 2f, 0f, chromaticSpread / 2f }; // Red, Green, Blue

        for (int i = 0; i < whiteRayCount; i++)
        {
            float t = whiteRayCount > 1 ? (float)i / (whiteRayCount - 1) : 0.5f;
            float verticalOffset = (t - 0.5f) * raySpread;
            Vector3 baseOffset = new Vector3(0, verticalOffset, 0);

            // Create RGB triplet at same position
            for (int colorIdx = 0; colorIdx < 3; colorIdx++)
            {
                AddRay(baseOffset, colors[colorIdx], nOffsets[colorIdx]);
            }
        }
    }

    void CreateParallelFan()
    {
        // Parallel rays at different heights (spherical aberration)
        for (int i = 0; i < numberOfRays; i++)
        {
            float t = numberOfRays > 1 ? (float)i / (numberOfRays - 1) : 0.5f;
            float verticalOffset = (t - 0.5f) * raySpread;
            AddRay(new Vector3(0, verticalOffset, 0), rayColor, 0f);
        }
    }

    void AddRay(Vector3 offset, Color color, float nOffset)
    {
        GameObject rayObj = new GameObject($"Ray_{rayRenderers.Count}");
        rayObj.transform.parent = transform;

        LineRenderer lr = rayObj.AddComponent<LineRenderer>();
        lr.material = RayPhysics.CreateRayMaterial(color);
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = rayWidth;
        lr.endWidth = rayWidth;
        lr.sortingOrder = 1;

        rayRenderers.Add(lr);
        rayDataList.Add(new RayData { offset = offset, color = color, refractiveIndexOffset = nOffset });
    }

    void TraceAllRays()
    {
        for (int i = 0; i < rayRenderers.Count; i++)
        {
            TraceRay(rayRenderers[i], rayDataList[i], i);
        }
    }

    System.Collections.IEnumerator TraceAllRaysAsync()
    {
        int processed = 0;
        for (int i = 0; i < rayRenderers.Count; i++)
        {
            TraceRay(rayRenderers[i], rayDataList[i], i);
            processed++;
            if (asyncTracing && processed >= Mathf.Max(1, raysPerFrame))
            {
                processed = 0;
                yield return null; // spread work across frames
            }
        }
        tracingCoroutine = null;
    }

    void TraceRay(LineRenderer lineRenderer, RayData rayData, int rayIndex)
    {
        // Reuse buffer instead of creating new List every frame (avoids garbage collection!)
        pointsBuffer.Clear();

        // Starting position
        Vector3 rayOrigin = transform.position + transform.TransformDirection(rayData.offset);
        Vector3 rayDirection = transform.forward;
        pointsBuffer.Add(rayOrigin);

        RaycastHit hit;
        int bounces = 0;

        while (bounces < maxRayBounces)
        {
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxRayDistance))
            {
                // Skip trigger colliders
                if (hit.collider.isTrigger)
                {
                    rayOrigin = hit.point + rayDirection * 0.001f;
                    continue;
                }

                pointsBuffer.Add(hit.point);

                // Check if we hit a lens - LENS PROVIDES PROPERTIES!
                // Check both on the collider itself and its children
                LensProperties lens = hit.collider.GetComponent<LensProperties>();
                if (lens == null)
                {
                    lens = hit.collider.GetComponentInChildren<LensProperties>();
                }
                
                if (lens != null)
                {
                    // Get refractive index from the lens, apply chromatic offset if enabled
                    float refractiveIndex = lens.refractiveIndex + rayData.refractiveIndexOffset;

                    bool debug = showDebugInfo && rayIndex == 0;

                    // Use helper to trace through lens
                    var result = RayPhysics.TraceThroughLens(
                        hit.point,
                        rayDirection,
                        lens,
                        refractiveIndex,
                        pointsBuffer,
                        debug
                    );

                    rayOrigin = result.origin;
                    rayDirection = result.direction;
                    bounces += 2;
                    continue;
                }
                else
                {
                    // Hit target screen or other object, stop
                    break;
                }
            }
            else
            {
                // No hit, extend to max distance
                pointsBuffer.Add(rayOrigin + rayDirection * maxRayDistance);
                break;
            }
        }

        lineRenderer.positionCount = pointsBuffer.Count;
        lineRenderer.SetPositions(pointsBuffer.ToArray());
    }

    /// <summary>
    /// Manually trigger ray update (useful when lenses move in static scene mode)
    /// </summary>
    public void ForceUpdate()
    {
        ScheduleUpdate();
    }

    public void ScheduleUpdate()
    {
        if (!asyncTracing)
        {
            TraceAllRays();
            return;
        }
        if (tracingCoroutine != null)
        {
            return; // already scheduled
        }
        tracingCoroutine = StartCoroutine(TraceAllRaysAsync());
    }

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            CreateRays();
            hasUpdatedOnce = false; // Reset to allow update after changes
        }
    }
}
