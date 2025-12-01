using UnityEngine;

public class LensProperties : MonoBehaviour
{
    public enum LensType
    {
        Convex,    // Converging lens (thicker in middle)
        Concave    // Diverging lens (thinner in middle)
    }
    
    [Header("Lens Configuration")]
    public LensType lensType = LensType.Convex;
    
    [Header("Optical Properties")]
    public float refractiveIndex = 1.5f;  // Glass refractive index
    public float focalLength = 0.5f;      // For reference/display only
    
    [Header("Visual")]
    public bool showLensInfo = true;
    
    void OnDrawGizmos()
    {
        if (showLensInfo)
        {
            Gizmos.color = lensType == LensType.Convex ? Color.blue : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.02f);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.2f,
                lensType.ToString() + " Lens\nf = " + focalLength + "m"
            );
            #endif
        }
    }
    
    public bool IsConvex()
    {
        return lensType == LensType.Convex;
    }
    
    public bool IsConcave()
    {
        return lensType == LensType.Concave;
    }
}
