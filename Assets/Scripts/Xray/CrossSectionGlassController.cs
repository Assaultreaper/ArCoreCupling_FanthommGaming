using UnityEngine;

public class CrossSectionGlassController : MonoBehaviour
{
    public Material crossSectionMaterial;
    public Transform cuttingPosition;
    public float cuttingRadius = 1.0f;
    public bool enable = false;

    void Start()
    {
        ToggleXRay(false);
        if (crossSectionMaterial != null)
        {
            // Set the cutting position and radius at runtime
            crossSectionMaterial.SetVector("_CuttingPosition", cuttingPosition.position);
            crossSectionMaterial.SetFloat("_CuttingRadius", cuttingRadius);
        }
    }

    public void ToggleXRay(bool _enable)
    {
        if (crossSectionMaterial != null)
        {
            // Enable or disable the X-ray effect
            crossSectionMaterial.SetFloat("_EnableXRay", _enable ? 1.0f : 0.0f);
        }
    }
}
