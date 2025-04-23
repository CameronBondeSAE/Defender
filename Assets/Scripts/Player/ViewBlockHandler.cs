using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class ViewBlockHandler : MonoBehaviour
{
    [SerializeField] private Material transparentMaterial;
    [SerializeField] private float transparencyAlpha = 0.3f;
    [SerializeField] private LayerMask blockLayers;
    [SerializeField] private float sphereCastRadius = 6f;
    private Transform mainCamera;

    // Dictionary to track currently transparent renderers and their original materials
    private Dictionary<Renderer, Material[]> originalMaterials = new();
    // Tracking original materials for EACH renderer (to avoid conflict with the above)
    private HashSet<Renderer> currentlyTransparent = new();

    void Start()
    {
        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        HandleTransparency();
    }

    void HandleTransparency()
    {
        Vector3 directionToCamera = mainCamera.position - transform.position;
        float distance = Vector3.Distance(transform.position, mainCamera.position);

        // Store all renderers that were transparent in this frame
        HashSet<Renderer> renderersToRestore = new HashSet<Renderer>(currentlyTransparent);

        // Cast big sphere rays from player to camera
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, sphereCastRadius, directionToCamera.normalized, distance, blockLayers);

        foreach (RaycastHit hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
            {
                if (!originalMaterials.ContainsKey(rend))
                {
                    // Store the original materials of the renderer if it hasn't been processed before
                    originalMaterials[rend] = rend.materials;
                }

                // Apply transparent material
                Material[] newMats = new Material[rend.materials.Length];
                for (int i = 0; i < newMats.Length; i++)
                {
                    Material mat = new Material(transparentMaterial);
                    Color color = mat.color;
                    color.a = transparencyAlpha;
                    mat.color = color;
                    newMats[i] = mat;
                }

                // Apply the new transparent materials
                rend.materials = newMats;
                currentlyTransparent.Add(rend);
                renderersToRestore.Remove(rend);
            }
        }

        // Restore materials for objects that are no longer transparent
        foreach (Renderer rend in renderersToRestore)
        {
            if (rend != null && originalMaterials.ContainsKey(rend))
            {
                // Restore the original materials
                rend.materials = originalMaterials[rend];
                currentlyTransparent.Remove(rend);
            }
        }
    }
}
