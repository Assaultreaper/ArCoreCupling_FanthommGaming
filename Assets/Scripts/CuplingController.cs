using System.Collections;
using UnityEngine;

public class CuplingController : MonoBehaviour
{
    public Transform StaticCupling;
    public Transform MovingCupling;
    private Vector3 PrevPos;
    public Transform Handle;
    public Vector3 TargetPosition;

    public float moveSpeed = 1f;
    public float rotationSpeed = 90f;

    private bool isConnected = false;
    private Coroutine movementCoroutine;
    private Coroutine handleCoroutine;

    [SerializeField] private Material targetMaterial;

    private bool isXrayOn = false;
    public float xrayCutoff = 0.0f;
    public float xrayOffCutoff = -100.0f;

    [Header("Spring Compression Settings")]
    public Transform SpringTransform;
    public Transform SpringSurface;
    public Transform Pusher;
    public Vector3 PusherCompressedPos;
    private Vector3 PusherOriginalPos;
    public float SpringCompressedZ = 0.05f;
    public float SpringOriginalZ = 0.136411f;

    private float currentCompression = 0f;
    private Vector3 SpringSurfaceOriginalPos;

    public bool isFlowing = false;
    public Vector2 scrollSpeed = new Vector2(0f, 0.2f);
    private Vector2 currentOffset = Vector2.zero;

    [SerializeField] private Material flowMaterial;
    [SerializeField] private bool visualizeGas =  false; 
    [SerializeField] private GameObject GasOff;
    [SerializeField] private GameObject GasOff2;

    void Start()
    {
        InitializeControls();

        if (Pusher != null)
            PusherOriginalPos = Pusher.localPosition;

        if (SpringSurface != null)
            SpringSurfaceOriginalPos = SpringSurface.localPosition;
    }

    void Update()
    {
        if (visualizeGas == true)
        {
            if (isFlowing && flowMaterial != null)
            {
                GasOff.SetActive(true);
                GasOff2.SetActive(true);
                Vector4 tiling = flowMaterial.GetVector("_Tiling");
                tiling.w += Time.deltaTime * 0.1f;
                flowMaterial.SetVector("_Tiling", tiling);
            }
        }
        else
        {
            GasOff.SetActive(false);
            GasOff2.SetActive(false);
        }
    }

    private void InitializeControls()
    {
        UIManager.Instance.ConnectCupling.AddListener(OnConnectRequested);
        UIManager.Instance.DisconnectCupling.AddListener(OnDisconnectRequested);
        UIManager.Instance.GasLeak.AddListener(ToggleFlow);
        UIManager.Instance.Xray.AddListener(ToggleXray);
        UIManager.Instance.MoveTowards.AddListener(MoveLeft);
        UIManager.Instance.MoveAway.AddListener(MoveRight);
    }

    public void ToggleXray()
    {
        if (targetMaterial == null) return;

        isXrayOn = !isXrayOn;
        float cutoffValue = isXrayOn ? xrayCutoff : xrayOffCutoff;
        targetMaterial.SetFloat("_Cutoffx", cutoffValue);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 0.1f)
        {
            float compressionAmount = Mathf.Clamp(collision.relativeVelocity.magnitude * 0.1f, 0f, 1f);
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
            Pusher.localPosition = Vector3.Lerp(PusherOriginalPos, PusherCompressedPos, compressionAmount);

        if (SpringSurface != null && Pusher != null)
            SpringSurface.localPosition = SpringSurfaceOriginalPos + Vector3.forward * (newScale.z - SpringOriginalZ);
    }

    public void MoveLeft()
    {
        if (!isConnected && movementCoroutine == null)
        {
            PrevPos = MovingCupling.localPosition;

            Vector3 currentPos = MovingCupling.localPosition;
            Vector3 newTarget = new Vector3(currentPos.x, currentPos.y, TargetPosition.z);

            movementCoroutine = StartCoroutine(MoveToTarget(newTarget, () => GasOff.SetActive(true)));
        }
    }

    public void MoveRight()
    {
        if (!isConnected && movementCoroutine == null)
        {
            GasOff.SetActive(false);
            movementCoroutine = StartCoroutine(MoveToTarget(PrevPos));
        }
    }

    private void OnConnectRequested()
    {
        if (!isConnected)
        {
            Connect();
        }
    }

    private void OnDisconnectRequested()
    {
        if (isConnected)
        {
            Disconnect();
        }
    }

    public void Connect()
    {
        if (!isConnected && handleCoroutine == null)
        {
                isConnected = true;
                isFlowing = true;
                RotateHandle();
        }
    }

    public void Disconnect()
    {
        if (isConnected && handleCoroutine == null)
        {
            isConnected = false;
            isFlowing = false;
            RotateHandle();
            ResetSpringSurfacePosition();
        }
    }

    private void ResetSpringSurfacePosition()
    {
        if (SpringSurface != null)
            SpringSurface.localPosition = SpringSurfaceOriginalPos;

        if (Pusher != null)
            Pusher.localPosition = PusherOriginalPos;

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

    public void ToggleFlow()
    {
        if (visualizeGas)
        {
            visualizeGas = false;
        }
        else
        {
            visualizeGas = true;
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

            Handle.localEulerAngles = Vector3.Lerp(startEuler, endEuler, t);

            if (SpringTransform != null)
            {
                Vector3 newScale = SpringTransform.localScale;
                newScale.z = Mathf.Lerp(startSpringZ, targetSpringZ, t);
                SpringTransform.localScale = newScale;

                if (SpringSurface != null && Pusher != null)
                    SpringSurface.localPosition = SpringSurfaceOriginalPos + Vector3.forward * (newScale.z - SpringOriginalZ);
            }

            if (Pusher != null)
                Pusher.localPosition = Vector3.Lerp(startPusherPos, targetPusherPos, t);

            yield return null;
        }

        Handle.localEulerAngles = endEuler;

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
