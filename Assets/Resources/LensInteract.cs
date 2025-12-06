using UnityEngine;

public class LensInteract : Interactive
{
    [Header("Movement Settings")]
    public float moveStepSize = 0.05f;  // How far to move per twist
    
    [Header("Movement Constraints")]
    public float collisionCheckDistance = 0.08f;  // How far to check for obstacles
    
    [Header("Visual Feedback")]
    public bool showDebugRays = false;
    
    // Static variable to store twist direction from CameraInteract
    public static float lastTwistDirection = 0f;
    
    // Cached emitters to avoid expensive FindObjectsByType every move
    private static RayEmitter[] cachedEmitters;
    private static bool emittersCached = false;
    
    private LensProperties lensProperties;
    private Collider lensCollider;
    
    void Start()
    {
        // Get components from this lens object or its children
        lensProperties = GetComponentInChildren<LensProperties>();
        lensCollider = GetComponent<Collider>();
        
        if (lensProperties == null)
        {
            Debug.LogError("LensInteract requires LensProperties component on " + gameObject.name + " or its children");
        }
        
        // Cache all emitters once at start
        CacheEmitters();
    }
    
    /// <summary>
    /// Cache all RayEmitters in the scene to avoid expensive searches every move
    /// </summary>
    private static void CacheEmitters()
    {
        if (!emittersCached)
        {
            cachedEmitters = FindObjectsByType<RayEmitter>(FindObjectsSortMode.None);
            emittersCached = true;
            Debug.Log($"[LensInteract] Cached {cachedEmitters.Length} RayEmitters");
        }
    }
    
    /// <summary>
    /// Manually refresh emitter cache (call if emitters are added/removed at runtime)
    /// </summary>
    public static void RefreshEmitterCache()
    {
        emittersCached = false;
        CacheEmitters();
    }
    
    public new void Interact()
    {
        // Read direction from static variable set by CameraInteract
        // Invert it because CameraInteract's angdiff direction is opposite to what we want
        float direction = -lastTwistDirection;
        
        MoveLens(direction * moveStepSize);
    }
    

    
    void MoveLens(float moveAmount)
    {
        if (lensCollider == null)
        {
            Debug.LogWarning("[MoveLens] lensCollider is null!");
            return;
        }
        
        if (Mathf.Abs(moveAmount) < 0.001f)
        {
            Debug.LogWarning("[MoveLens] moveAmount too small, skipping");
            return;
        }
        
        Vector3 moveDirection = moveAmount > 0 ? Vector3.right : Vector3.left;
        float absMoveAmount = Mathf.Abs(moveAmount);
        
        Vector3 rayOrigin = transform.position;
        float checkDistance = absMoveAmount + collisionCheckDistance;
        
        // Raycast check
        RaycastHit hit;
        bool pathBlocked = false;
        
        if (Physics.Raycast(rayOrigin, moveDirection, out hit, checkDistance))
        {
            if (hit.collider != lensCollider && !hit.transform.IsChildOf(transform))
            {
                // Check if hit object is another lens (has LensInteract component)
                LensInteract otherLens = hit.collider.GetComponent<LensInteract>();
                bool isAnotherLens = otherLens != null && otherLens != this;
                
                // Block if: solid obstacle OR another lens (even if trigger)
                if (!hit.collider.isTrigger || isAnotherLens)
                {
                    pathBlocked = true;
                }
                else
                {
                    Debug.Log($"[MoveLens] Hit is trigger (not a lens), ignoring");
                }
            }
            else
            {
                Debug.Log($"[MoveLens] Hit is self or child, ignoring");
            }
        }
        else
        {
            Debug.Log($"[MoveLens] Raycast found NO obstacles - path is clear!");
        }
        
        // Only move if path is clear
        if (!pathBlocked)
        {
            Vector3 oldPosition = transform.position;
            Vector3 newPosition = transform.position + moveDirection * absMoveAmount;
            transform.position = newPosition;
            
            // TRIGGER RAY UPDATE after lens moves
            UpdateRays();
        }
        else
        {
            Debug.LogWarning($"[MoveLens] ❌ CANNOT MOVE {(moveAmount > 0 ? "RIGHT" : "LEFT")} - BLOCKED!");
        }
    }
    
    /// <summary>
    /// Triggers all ray emitters to update after lens movement
    /// </summary>
    void UpdateRays()
    {
        // Ensure physics world reflects latest lens transform before raycasts
        Physics.SyncTransforms();

        // Use cached emitters instead of expensive search
        if (cachedEmitters == null || cachedEmitters.Length == 0)
        {
            CacheEmitters();
        }
        
        foreach (var emitter in cachedEmitters)
        {
            if (emitter != null) // Safety check in case emitter was destroyed
            {
                emitter.ScheduleUpdate();
            }
        }
    }
    
    // Optional: Draw gizmo for this lens
    void OnDrawGizmos()
    {
        if (showDebugRays && lensProperties != null)
        {
            Gizmos.DrawWireSphere(transform.position, 0.15f);
        }
    }
}
