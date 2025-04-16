using UnityEngine;

public class SpringCompressor : MonoBehaviour
{
    [Header("Assign These")]
    public Transform spring;             // Spring that visually scales
    public Transform attachedSurface;    // Surface that moves back (in contact with pusher)
    public Transform pusherObject;       // Object that triggers the compression

    [Header("Compression Settings")]
    public float minScaleZ = 0.05f;
    public float maxScaleZ = 0.136411f;
    public float compressionSpeed = 5f;
    public float moveBackDistance = 0.1f;

    [Header("Detection")]
    public float activationDistance = 0.1f; // How close the pusher must be for compression to start

    private Vector3 initialSpringScale;
    private Vector3 initialSurfacePosition;

    void Start()
    {
        if (spring != null)
            initialSpringScale = spring.localScale;

        if (attachedSurface != null)
            initialSurfacePosition = attachedSurface.position;
    }

    void Update()
    {
        if (spring == null || attachedSurface == null || pusherObject == null)
            return;

        // Measure distance between the pusher and the attached surface
        float distance = Vector3.Distance(pusherObject.position, attachedSurface.position);
        bool isPushing = distance <= activationDistance;

        // Target compression value
        float targetZ = isPushing ? minScaleZ : maxScaleZ;

        // Lerp the spring scale Z based on compression speed and pusher distance
        Vector3 scale = spring.localScale;
        scale.z = Mathf.Lerp(scale.z, targetZ, Time.deltaTime * compressionSpeed);
        spring.localScale = scale;

        // Now calculate the compression percentage based on spring scale (min to max range)
        float compressionPercent = Mathf.InverseLerp(maxScaleZ, minScaleZ, scale.z);

        // Move the surface based on pusher distance, considering compression
        // We want the attached surface to move proportionally with the spring compression, but also follow the pusher's position
        Vector3 targetSurfacePosition = pusherObject.position - attachedSurface.forward * compressionPercent * moveBackDistance;

        // Smoothly move the surface towards the target position based on the compression percentage
        attachedSurface.position = Vector3.Lerp(attachedSurface.position, targetSurfacePosition, Time.deltaTime * compressionSpeed);
    }

    void OnDrawGizmosSelected()
    {
        if (attachedSurface != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attachedSurface.position, activationDistance);
        }
    }
}
