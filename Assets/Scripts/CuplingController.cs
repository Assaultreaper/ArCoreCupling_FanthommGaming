using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

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

    [Header("Target Material")]
    public Material targetMaterial;
    public bool xrayOn = false;
    [Header("World Cutoff Values")]
    public float cutoffX = -999f;
    public float cutoffY = -1f;
    public float cutoffZ = -999f;

    void Start()
    {
        if (gasEffect != null)
            gasEffect.Stop();

        InitializeControls();
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
        if (targetMaterial != null)
        {
            if (targetMaterial.GetFloat("_CutoffX") == cutoffX )
            {
                targetMaterial.SetFloat("_CutoffX", 2.95f);
                targetMaterial.SetFloat("_CutoffY", cutoffY);
                targetMaterial.SetFloat("_CutoffZ", cutoffZ);
            }
            else
            {
                targetMaterial.SetFloat("_CutoffX", cutoffX);
                targetMaterial.SetFloat("_CutoffY", cutoffY);
                targetMaterial.SetFloat("_CutoffZ", cutoffZ);
            }
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
            RotateHandle(() => movementCoroutine = StartCoroutine(MoveToTarget(newTarget)));
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

        while (elapsed < angle / rotationSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / (angle / rotationSpeed));
            Handle.localEulerAngles = Vector3.Lerp(startEuler, endEuler, t);
            yield return null;
        }

        Handle.localEulerAngles = endEuler;
        handleCoroutine = null;
        OnComplete?.Invoke();
    }
}
