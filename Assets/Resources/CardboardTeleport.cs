using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class CardboardTeleport : Interactive
{
    [Header("Teleport Settings")]
    public Transform playerRoot;
    public float maxTeleportDistance = 100f;
    public float twistCooldownSeconds = 0.4f;

    private Camera vrCamera;
    private float _lastTeleportTime = -999f;

    void Start()
    {
        vrCamera = Camera.main;
        if (vrCamera == null)
        {
            Debug.LogError("CardboardTeleport: No main camera found!");
            return;
        }

        if (playerRoot == null)
        {
            playerRoot = transform.parent != null ? transform.parent : transform;
        }

        EnhancedTouchSupport.Enable();

        if (interactModeOverride == InteractMode.None)
        {
            interactModeOverride = InteractMode.Dwell;
        }

        // override dwell time for this teleport object
        dwellTimerOverride = 2f;
    }

    public new void Interact()
    {
        if (vrCamera == null || playerRoot == null) return;

        // Cooldown to prevent double-teleport from strong twist
        if (Time.time - _lastTeleportTime < twistCooldownSeconds)
        {
            return;
        }

        Ray ray = new Ray(vrCamera.transform.position, vrCamera.transform.forward);
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, maxTeleportDistance);
        if (hitSomething && hit.collider != null && hit.collider.transform == transform)
        {
            Vector3 target = hit.point;
            Vector3 teleportPos = new Vector3(target.x, playerRoot.position.y, target.z);
            playerRoot.position = teleportPos;
            _lastTeleportTime = Time.time;
        }
    }
}
