using Photon.Pun;
using UnityEngine;

public class GrabInteractive : Interactive
{
    private bool isGrabbed = false;
    private Rigidbody rb;
    private Transform originalParent;
    private bool useGravity;
    private bool wasKinematic;
    PhotonView pv;
    public float holdDistance = 2.0f;
    public float followSpeed = 10.0f;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
    }

    public new void Interact()
    {
        if (pv == null) return;

        if (!isGrabbed)
        {
            pv.RPC("Grab", RpcTarget.AllBuffered);
        }
        else
        {
            pv.RPC("Release", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void Grab()
    {
        isGrabbed = true;
        originalParent = transform.parent;

        if (rb != null)
        {
            useGravity = rb.useGravity;
            wasKinematic = rb.isKinematic;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        Debug.Log("Grabbed " + transform.name);
    }

    [PunRPC]
    void Release()
    {
        isGrabbed = false;

        if (rb != null)
        {
            rb.useGravity = useGravity;
            rb.isKinematic = wasKinematic;
        }

        transform.parent = originalParent;

        Debug.Log("Released " + transform.name);
    }

    void Update()
    {
        if (isGrabbed && Camera.main != null)
        {
            Vector3 targetPosition = Camera.main.transform.position + Camera.main.transform.forward * holdDistance;
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            
            transform.rotation = Quaternion.Lerp(transform.rotation, Camera.main.transform.rotation, followSpeed * Time.deltaTime);
        }
    }
}