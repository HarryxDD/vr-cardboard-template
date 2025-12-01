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
        Vector3 entryNormal = GetSurfaceNormal(entryPoint, rayDirection, lensCollider);
        Vector3 refractedDirection = RefractRay(rayDirection, entryNormal, AIR_REFRACTIVE_INDEX, refractiveIndex);

        if (showDebug)
        {
            Debug.Log($"Entry refraction: n1={AIR_REFRACTIVE_INDEX} → n2={refractiveIndex}, direction={refractedDirection}");
        }

        // Move inside the lens
        Vector3 rayOrigin = entryPoint + refractedDirection * 0.001f;
        rayDirection = refractedDirection;
        points.Add(rayOrigin);

        // MARCH THROUGH LENS: Find exit point
        // Larger step size for mobile performance (less accurate but faster)
        float stepSize = 0.01f; // Changed from 0.005 to 0.01 (50% fewer iterations)
        float maxTravelDistance = 1.0f;
        float traveledDistance = stepSize;

        Vector3 exitPoint = rayOrigin;
        Vector3 exitNormal = -entryNormal;
        bool foundExit = false;

        while (traveledDistance < maxTravelDistance)
        {
            Vector3 testPoint = rayOrigin + rayDirection * traveledDistance;
            Vector3 closestPoint = lensCollider.ClosestPoint(testPoint);
            bool isInside = Vector3.Distance(testPoint, closestPoint) < 0.0001f;

            if (!isInside) // Exited!
            {
                exitPoint = closestPoint;
                
                // Calculate exit normal (points outward from lens surface)
                // For sphere collider, this is from center to exit point
                if (lensCollider is SphereCollider sphereCollider)
                {
                    Vector3 center = lensCollider.transform.position;
                    exitNormal = (exitPoint - center).normalized;
                }
                else
                {
                    // For other colliders, use difference method
                    exitNormal = (testPoint - closestPoint).normalized;
                }
                
                foundExit = true;
                break;
            }

            traveledDistance += stepSize;
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
        Vector3 exitNormalForRefraction = -exitNormal; // Flip for RefractRay
        Vector3 exitRefractedDirection = RefractRay(rayDirection, exitNormalForRefraction, refractiveIndex, AIR_REFRACTIVE_INDEX);

        if (showDebug)
        {
            Debug.Log($"Exit refraction: n1={refractiveIndex} → n2={AIR_REFRACTIVE_INDEX}, direction={exitRefractedDirection}");
        }

        // Return exit position and direction
        Vector3 exitOrigin = exitPoint + exitRefractedDirection * 0.05f;
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
