using UnityEngine;

public class ColorChange : Interactive
{
    public Material greenMaterial;
    public Material redMaterial;
    private bool isRed = true;

    public new void Interact()
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
