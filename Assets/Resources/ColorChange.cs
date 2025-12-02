using Photon.Pun;
using UnityEngine;

public class ColorChange : Interactive
{
    public Material greenMaterial;
    public Material redMaterial;
    private bool isRed = true;
    PhotonView pv;

    void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    public new void Interact()
    {
        if (pv != null)
        {
            pv.RPC("ChangeColor", RpcTarget.AllBuffered);
        }
    }
    
    [PunRPC]
    void ChangeColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (isRed)
            {
                renderer.material = greenMaterial;
                isRed = false;
            }
            else
            {
                renderer.material = redMaterial;
                isRed = true;
            }
        }
    }
}
