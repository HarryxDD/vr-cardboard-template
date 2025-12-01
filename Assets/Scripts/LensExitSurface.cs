using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class LensExitSurface : MonoBehaviour
{
    [Header("Reference")]
    public Transform lensTransform;
    
    [Header("Settings")]
    public float distanceBehindLens = 0.05f;
    public float surfaceSize = 0.3f;

    void Start()
    {
        CreateExitSurface();
    }

    void CreateExitSurface()
    {
        // Create a simple quad mesh
        Mesh mesh = new Mesh();
        mesh.name = "LensExitSurface";

        float half = surfaceSize / 2f;
        
        // Vertices for a flat quad
        mesh.vertices = new Vector3[]
        {
            new Vector3(-half, -half, 0),
            new Vector3(half, -half, 0),
            new Vector3(-half, half, 0),
            new Vector3(half, half, 0)
        };

        // Normals pointing back toward the lens
        mesh.normals = new Vector3[]
        {
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward
        };

        // Triangles (two-sided)
        mesh.triangles = new int[]
        {
            0, 2, 1,  // Front face
            1, 2, 3,
            1, 2, 0,  // Back face (reversed winding)
            3, 2, 1
        };

        mesh.RecalculateBounds();

        // Apply mesh
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // Setup collider
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = false; // Flat surface doesn't need convex

        // Make invisible (remove renderer or disable it)
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = false;

        Debug.Log("Lens exit surface created - invisible collision plane");
    }

    void Update()
    {
        // Keep exit surface positioned behind the lens
        if (lensTransform != null)
        {
            transform.position = lensTransform.position + lensTransform.forward * distanceBehindLens;
            transform.rotation = lensTransform.rotation;
        }
    }
}
