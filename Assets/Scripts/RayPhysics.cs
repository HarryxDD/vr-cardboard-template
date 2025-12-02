using UnityEngine;

/// <summary>
/// Static helper class for optical ray tracing calculations
/// </summary>
public static class RayPhysics
{
    public const float AIR_REFRACTIVE_INDEX = 1.0f;

    /// <summary>
    /// Apply Snell's Law to calculate refracted ray direction
    /// </summary>
    /// <param name="direction">Incoming ray direction (normalized)</param>
    /// <param name="normal">Surface normal pointing toward the incoming ray</param>
    /// <param name="n1">Refractive index of material ray is leaving</param>
    /// <param name="n2">Refractive index of material ray is entering</param>
    /// <returns>Refracted ray direction (normalized)</returns>
    public static Vector3 RefractRay(Vector3 direction, Vector3 normal, float n1, float n2)
    {
        direction = direction.normalized;
        normal = normal.normalized;

        float eta = n1 / n2;
        float cosi = -Vector3.Dot(normal, direction);
        float cost2 = 1.0f - eta * eta * (1.0f - cosi * cosi);

        // Total internal reflection - for demo purposes, force refraction by clamping
        if (cost2 < 0.0f)
        {
            // Instead of reflecting, approximate refraction by continuing mostly forward
            // This prevents rays from bouncing back in the demo
            return (direction + normal * 0.1f).normalized;
        }

        return (eta * direction + (eta * cosi - Mathf.Sqrt(cost2)) * normal).normalized;
    }

    /// <summary>
    /// Trace a ray through a lens, handling entry and exit refraction
    /// </summary>
    /// <param name="entryPoint">Point where ray hit the lens surface</param>
    /// <param name="rayDirection">Direction of ray</param>
    /// <param name="lens">Lens component to trace through</param>
    /// <param name="refractiveIndex">Refractive index for this ray (can differ for chromatic aberration)</param>
    /// <param name="points">List to add ray path points to</param>
    /// <param name="showDebug">Whether to show debug logs</param>
    /// <returns>Exit ray origin and direction</returns>
    public static (Vector3 origin, Vector3 direction) TraceThroughLens(
        Vector3 entryPoint,
        Vector3 rayDirection, 
        LensProperties lens,
        float refractiveIndex,
        System.Collections.Generic.List<Vector3> points,
        bool showDebug = false)
    {
        Collider lensCollider = lens.GetComponent<Collider>();
        if (lensCollider == null)
        {
            Debug.LogError("Lens has no collider!");
            return (entryPoint, rayDirection);
        }

        // FIRST REFRACTION: Ray enters lens (air → glass)
        Vector3 entryNormal;
        
        // For sphere colliders, calculate normal geometrically for consistency
        if (lensCollider is SphereCollider sphereCollider)
        {
            Vector3 center = lensCollider.transform.position;
            entryNormal = (entryPoint - center).normalized;
        }
        else
        {
            // For other colliders, use raycast
            entryNormal = GetSurfaceNormal(entryPoint, rayDirection, lensCollider);
        }
        
        Vector3 refractedDirection = RefractRay(rayDirection, entryNormal, AIR_REFRACTIVE_INDEX, refractiveIndex);

        if (showDebug)
        {
            Debug.Log($"Entry refraction: n1={AIR_REFRACTIVE_INDEX} → n2={refractiveIndex}, direction={refractedDirection}");
        }

        // Move inside the lens
        Vector3 rayOrigin = entryPoint + refractedDirection * 0.001f;
        rayDirection = refractedDirection;
        points.Add(rayOrigin);

        // FIND LENS EXIT: Prefer analytic solution for sphere lenses
        Vector3 exitPoint = rayOrigin;
        Vector3 exitNormal = -entryNormal;
        bool foundExit = false;

        if (lensCollider is SphereCollider sphere)
        {
            // Analytic ray-sphere intersection from inside the sphere
            // Sphere center and radius
            Vector3 center = sphere.transform.position;
            float radius = sphere.radius * Mathf.Max(
                sphere.transform.lossyScale.x,
                Mathf.Max(sphere.transform.lossyScale.y, sphere.transform.lossyScale.z)
            );

            // Shift to sphere space
            Vector3 p = rayOrigin - center;
            Vector3 d = rayDirection.normalized;

            // Solve |p + t d|^2 = r^2 -> t^2 + 2(p·d)t + (|p|^2 - r^2) = 0
            float b = 2f * Vector3.Dot(p, d);
             // c should be negative when inside sphere (|p| < r)
            float c = p.sqrMagnitude - radius * radius;
            float disc = b * b - 4f * c; // since a=1

            if (disc >= 0f)
            {
                float tExit = (-b + Mathf.Sqrt(disc)) * 0.5f; // larger root: exiting forward
                if (tExit > 0f)
                {
                    exitPoint = rayOrigin + d * tExit;
                    exitNormal = (exitPoint - center).normalized; // outward normal
                    foundExit = true;
                }
            }
        }
        
        if (!foundExit)
        {
            // Fallback for non-sphere (or degenerate cases): minimal marching with robust check
            float stepSize = 0.01f;
            float maxTravelDistance = 1.0f;
            float traveledDistance = stepSize;

            while (traveledDistance < maxTravelDistance)
            {
                Vector3 testPoint = rayOrigin + rayDirection * traveledDistance;
                Vector3 closestPoint = lensCollider.ClosestPoint(testPoint);
                bool isInside = (testPoint - closestPoint).sqrMagnitude < 1e-6f; // relaxed tolerance

                if (!isInside)
                {
                    exitPoint = closestPoint;

                    // Compute outward normal
                    if (lensCollider is SphereCollider)
                    {
                        Vector3 center = lensCollider.transform.position;
                        exitNormal = (exitPoint - center).normalized;
                    }
                    else
                    {
                        // Try raycast from just outside back towards surface for accurate normal
                        RaycastHit exitHit;
                        Vector3 outsideStart = testPoint + (-rayDirection) * 0.02f;
                        if (Physics.Raycast(outsideStart, rayDirection, out exitHit, 0.05f))
                        {
                            if (exitHit.collider == lensCollider)
                            {
                                exitPoint = exitHit.point;
                                exitNormal = exitHit.normal;
                            }
                            else
                            {
                                exitNormal = (testPoint - closestPoint).normalized;
                            }
                        }
                        else
                        {
                            exitNormal = (testPoint - closestPoint).normalized;
                        }
                    }

                    foundExit = true;
                    break;
                }

                traveledDistance += stepSize;
            }
        }

        if (!foundExit)
        {
            exitPoint = rayOrigin + rayDirection * 0.2f;
            exitNormal = rayDirection;
            if (showDebug)
            {
                Debug.LogWarning("Could not find lens exit, forcing exit");
            }
        }

        points.Add(exitPoint);

        // SECOND REFRACTION: Ray exits lens (glass → air)
        // exitNormal points outward; for Snell's law use inward normal (-exitNormal)
        Vector3 exitRefractedDirection = RefractRay(rayDirection, -exitNormal, refractiveIndex, AIR_REFRACTIVE_INDEX);

        if (showDebug)
        {
            Debug.Log($"Exit refraction: n1={refractiveIndex} → n2={AIR_REFRACTIVE_INDEX}, direction={exitRefractedDirection}");
        }

        // Return exit position and direction
        Vector3 exitOrigin = exitPoint + exitRefractedDirection * 0.001f;
        return (exitOrigin, exitRefractedDirection);
    }

    /// <summary>
    /// Get surface normal at raycast hit point
    /// </summary>
    private static Vector3 GetSurfaceNormal(Vector3 origin, Vector3 direction, Collider collider)
    {
        RaycastHit hit;
        if (Physics.Raycast(origin - direction * 0.01f, direction, out hit, 0.02f))
        {
            if (hit.collider == collider)
            {
                return hit.normal;
            }
        }
        return -direction; // Fallback
    }

    /// <summary>
    /// Create a line renderer material that always renders on top
    /// </summary>
    public static Material CreateRayMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = color;
        mat.renderQueue = 3000; // Render after transparent objects
        return mat;
    }
}
