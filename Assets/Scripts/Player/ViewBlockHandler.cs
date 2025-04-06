using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class ViewBlockHandler : MonoBehaviour
{
    [SerializeField] private Material transparentMaterial;
    [SerializeField] private float transparencyAlpha = 0.3f;
    [SerializeField] private LayerMask blockLayers;
    private Transform mainCamera;
    // couplets of material and renderers
    private Dictionary<Renderer, Material[]> originalMaterials = new();
    // list to keep track of transparent materials and restore them
    private List<Renderer> currentlyTransparent = new();

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

        // Restore materials from previous frame
        foreach (Renderer rend in currentlyTransparent)
        {
            if (rend != null && originalMaterials.ContainsKey(rend))
            {
                rend.materials = originalMaterials[rend];
            }
        }

        currentlyTransparent.Clear();

        // Cast rays from player to camera
        RaycastHit[] hits = Physics.RaycastAll(transform.position, directionToCamera.normalized, distance, blockLayers);

        foreach (RaycastHit hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
            {
                if (!originalMaterials.ContainsKey(rend))
                {
                    // stores original material if hit renderer hasn't been processed before
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

                rend.materials = newMats;
                currentlyTransparent.Add(rend);
            }
        }
    }
}
