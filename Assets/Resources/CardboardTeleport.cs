using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class CardboardTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform playerRoot;
    public float maxTeleportDistance = 10f;
    
    [Header("Visual Marker")]
    public GameObject markerPrefab;
    public Color markerColor = Color.green;
    public float markerScale = 0.2f;
    
    private GameObject marker;
    private bool isValidTeleportLocation;
    private Vector3 targetTeleportPosition;
    private Camera vrCamera;
    private MeshRenderer markerRenderer;
    
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
            if (transform.parent != null)
            {
                playerRoot = transform.parent;
                Debug.Log("CardboardTeleport: Auto-found player root: " + playerRoot.name);
            }
            else
            {
                playerRoot = transform;
                Debug.Log("CardboardTeleport: Using current object as player root");
            }
        }
        
        EnhancedTouchSupport.Enable();
        CreateMarker();
    }
    
    void CreateMarker()
    {
        if (markerPrefab != null)
        {
            marker = Instantiate(markerPrefab);
        }
        else
        {
            marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "TeleportMarker";
            Destroy(marker.GetComponent<Collider>());
            marker.transform.localScale = new Vector3(markerScale, 0.01f, markerScale);
        }
        
        markerRenderer = marker.GetComponent<MeshRenderer>();
        Material markerMat = new Material(Shader.Find("Unlit/Color"));
        markerMat.color = markerColor;
        markerRenderer.material = markerMat;
        marker.SetActive(false);
    }
    
    void Update()
    {
        if (vrCamera == null) return;
        
        CheckTeleportLocation();
        
        bool inputDetected = false;
        
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            inputDetected = true;
        }
        
        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                if (touches[i].press.wasPressedThisFrame)
                {
                    inputDetected = true;
                    break;
                }
            }
        }
        
        if (isValidTeleportLocation && inputDetected)
        {
            Debug.Log("Teleporting to: " + targetTeleportPosition);
            TeleportToMarker();
        }
    }
    
    void CheckTeleportLocation()
    {
        Ray ray = new Ray(vrCamera.transform.position, vrCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxTeleportDistance))
        {
            if (hit.collider.CompareTag("Floor"))
            {
                isValidTeleportLocation = true;
                targetTeleportPosition = hit.point;
                
                marker.SetActive(true);
                marker.transform.position = hit.point + Vector3.up * 0.01f;
                marker.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            else
            {
                marker.SetActive(false);
                isValidTeleportLocation = false;
            }
        }
        else
        {
            marker.SetActive(false);
            isValidTeleportLocation = false;
        }
    }
    
    void TeleportToMarker()
    {
        if (!isValidTeleportLocation || playerRoot == null) return;
        
        // Only change X and Z, keep current Y height
        Vector3 teleportPos = new Vector3(
            targetTeleportPosition.x,
            playerRoot.position.y,
            targetTeleportPosition.z
        );
        
        playerRoot.position = teleportPos;
        
        marker.SetActive(false);
    }
    
    void OnDestroy()
    {
        if (marker != null)
        {
            Destroy(marker);
        }
        
        EnhancedTouchSupport.Disable();
    }
}
