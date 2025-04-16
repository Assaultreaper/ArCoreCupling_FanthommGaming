using System.Collections;
using UnityEngine;

public class CuplingController : MonoBehaviour
{
    public Transform StaticCupling;
    public Transform MovingCupling;
    private Vector3 PrevPos;
    public Transform Handle;
    public Vector3 TargetPosition;
    private bool Connectable = false;

    public float moveSpeed = 1f; // Units per second
    public float rotationSpeed = 90f; // Degrees per second
    public ParticleSystem gasEffect;

    private bool isConnected = false;
    private Coroutine movementCoroutine;
    private Coroutine handleCoroutine;

    [SerializeField] private Material targetMaterial;
    [SerializeField] private float originalZCutoff = -999f;  // Default for when X-ray is OFF
    [SerializeField] private float xrayZCutoff = 0.01f;      // Shallow slice for X-ray effect

    private bool isXrayOn = false;

    [Header("Spring Compression Settings")]
    public Transform SpringTransform;         // The spring to compress
    public Transform SpringSurface;           // Optional surface visually tied to spring
    public Transform Pusher;                  // The object that pushes the spring
    public Vector3 PusherCompressedPos;       // Target position when compressed
    private Vector3 PusherOriginalPos;        // Captured at Start
    public float SpringCompressedZ = 0.05f;
    public float SpringOriginalZ = 0.136411f;

    private float currentCompression = 0f;     // Tracks the current compression value
    private Vector3 SpringSurfaceOriginalPos; // To store the original position of the SpringSurface

    void Start()
    {
        if (gasEffect != null)
            gasEffect.Stop();

        InitializeControls();
        if (Pusher != null)
            PusherOriginalPos = Pusher.localPosition;

        // Store the original position of the SpringSurface
        if (SpringSurface != null)
            SpringSurfaceOriginalPos = SpringSurface.localPosition;
    }

    private void InitializeControls()
    {
        UIManager.Instance.ConnectCupling.AddListener(Connect);
        UIManager.Instance.DisconnectCupling.AddListener(Disconnect);
        UIManager.Instance.GasLeak.AddListener(TriggerGas);
        UIManager.Instance.Xray.AddListener(ToggleXray);
    }
    public void ToggleXray()
    {
        if (targetMaterial == null)
        {
            Debug.LogWarning("Target material is not assigned!");
            return;
        }

        isXrayOn = !isXrayOn;

        float newCullPercent = isXrayOn ? 0.5f : 0.0f; // 0.4 = 40% culling, 0 = fully visible
        targetMaterial.SetFloat("_CullPercent", newCullPercent);

        Debug.Log(isXrayOn ?
            $"X-ray On: Culling {newCullPercent * 100f}% based on camera view." :
            "X-ray Off: Full mesh visible.");
    }


    private void OnCollisionEnter(Collision collision)
    {

        if (collision.relativeVelocity.magnitude > 0.1f)
        {
            float compressionAmount = Mathf.Clamp(collision.relativeVelocity.magnitude * 0.1f, 0f, 1f); // You can tweak the multiplier here
            UpdateSpringCompression(compressionAmount);
        }
    }

    private void UpdateSpringCompression(float compressionAmount)
    {
       
        currentCompression = Mathf.Lerp(0f, 1f, compressionAmount); 

        Vector3 newScale = SpringTransform.localScale;
        newScale.z = Mathf.Lerp(SpringOriginalZ, SpringCompressedZ, currentCompression);
        SpringTransform.localScale = newScale;

        if (Pusher != null)
        { 
            Pusher.localPosition = Vector3.Lerp(PusherOriginalPos, PusherCompressedPos, currentCompression);
        }
        if (SpringSurface != null && Pusher != null)
        {
            SpringSurface.localPosition = SpringSurfaceOriginalPos + SpringTransform.forward * (newScale.z - SpringOriginalZ);
        }
    }

    public void MoveLeft()
    {
        if (!isConnected && movementCoroutine == null)
        {
            Vector3 newTarget = TargetPosition;
            movementCoroutine = StartCoroutine(MoveToTarget(newTarget));
        }
    }

    public void MoveRight()
    {
        if (!isConnected && movementCoroutine == null)
        {
            Vector3 newTarget = new Vector3(0.236585811f, -0.860000014f, 2.61000061f);
            movementCoroutine = StartCoroutine(MoveToTarget(newTarget));
        }
    }

    public void Connect()
    {
        if (!isConnected)
        {
            if (movementCoroutine == null)
                PrevPos = MovingCupling.transform.localPosition;

            movementCoroutine = StartCoroutine(MoveToTarget(TargetPosition, () =>
            {
                isConnected = true;
                RotateHandle(TriggerGas);
            }));
        }
        else
        {
            Disconnect();
        }
    }

    public void Disconnect()
    {
        if (isConnected && movementCoroutine == null)
        {
            TriggerGas();
            isConnected = false;
            Vector3 newTarget = PrevPos;
            RotateHandle(() =>
            {
                movementCoroutine = StartCoroutine(MoveToTarget(newTarget));
                ResetSpringSurfacePosition(); // Reset the SpringSurface position after disconnect
            });
        }
    }

    private void ResetSpringSurfacePosition()
    {
        // Reset the SpringSurface to its original position
        if (SpringSurface != null)
        {
            SpringSurface.localPosition = SpringSurfaceOriginalPos;
        }

        // Optionally reset other components (like Spring Transform or Pusher) to their original positions if needed
        if (Pusher != null)
        {
            Pusher.localPosition = PusherOriginalPos;
        }

        // Reset spring scale
        if (SpringTransform != null)
        {
            Vector3 scale = SpringTransform.localScale;
            scale.z = SpringOriginalZ;
            SpringTransform.localScale = scale;
        }
    }

    public void RotateHandle(System.Action OnComplete = null)
    {
        if (handleCoroutine == null)
        {
            float targetZ = isConnected ? 90f : 0f;
            handleCoroutine = StartCoroutine(RotateHandleSmooth(targetZ, OnComplete));
        }
    }

    public void TriggerGas()
    {
        if (gasEffect != null)
        {
            if (gasEffect.isPlaying)
                gasEffect.Stop();
            else
                gasEffect.Play();
        }
    }

    private IEnumerator MoveToTarget(Vector3 destination, System.Action onComplete = null)
    {
        Vector3 start = MovingCupling.localPosition;
        float distance = Vector3.Distance(start, destination);
        float elapsed = 0f;

        while (elapsed < distance / moveSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / (distance / moveSpeed));
            MovingCupling.localPosition = Vector3.Lerp(start, destination, t);
            yield return null;
        }

        MovingCupling.localPosition = destination;
        movementCoroutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator RotateHandleSmooth(float targetZ, System.Action OnComplete = null)
    {
        Vector3 startEuler = Handle.localEulerAngles;
        Vector3 endEuler = new Vector3(startEuler.x, startEuler.y, targetZ);
        float angle = Quaternion.Angle(Quaternion.Euler(startEuler), Quaternion.Euler(endEuler));
        float elapsed = 0f;

        float startSpringZ = SpringTransform != null ? SpringTransform.localScale.z : 0f;
        float targetSpringZ = isConnected ? SpringCompressedZ : SpringOriginalZ;

        Vector3 startPusherPos = Pusher != null ? Pusher.localPosition : Vector3.zero;
        Vector3 targetPusherPos = isConnected ? PusherCompressedPos : PusherOriginalPos;

        while (elapsed < angle / rotationSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / (angle / rotationSpeed));

            // Rotate Handle
            Handle.localEulerAngles = Vector3.Lerp(startEuler, endEuler, t);

            // Spring Compression
            if (SpringTransform != null)
            {
                Vector3 newScale = SpringTransform.localScale;
                newScale.z = Mathf.Lerp(startSpringZ, targetSpringZ, t);
                SpringTransform.localScale = newScale;

                // Adjust spring surface position based on Pusher's movement
                if (SpringSurface != null && Pusher != null)
                {
                    // Move SpringSurface relative to Pusher's movement
                    SpringSurface.localPosition = Pusher.localPosition + SpringTransform.forward * newScale.z;
                }
            }

            // Move Pusher
            if (Pusher != null)
                Pusher.localPosition = Vector3.Lerp(startPusherPos, targetPusherPos, t);

            yield return null;
        }

        Handle.localEulerAngles = endEuler;

        // Snap to final values
        if (SpringTransform != null)
        {
            Vector3 finalScale = SpringTransform.localScale;
            finalScale.z = targetSpringZ;
            SpringTransform.localScale = finalScale;
        }

        if (Pusher != null)
            Pusher.localPosition = targetPusherPos;

        handleCoroutine = null;
        OnComplete?.Invoke();
    }
}
