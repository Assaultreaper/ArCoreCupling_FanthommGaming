using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [Serializable]
    public enum PanelTypes
    {
        SplashScreen,
        MainMenu,
        UIControls,
        ARTips
    }
    [Serializable]
    public class PanelManagement
    {
        public GameObject Panel;
        public UIManager.PanelTypes PanelTypes;
        [HideInInspector] public UnityEvent RequestPanel = new UnityEvent();
    }


    public PanelManagement CurrentPanel;
    public List<PanelManagement> panels;

    [HideInInspector]public UnityEvent ConnectCupling;
    [HideInInspector]public UnityEvent DisconnectCupling;
    [HideInInspector] public UnityEvent MoveTowards;
    [HideInInspector] public UnityEvent MoveAway;
    [HideInInspector]public UnityEvent GasLeak;
    [HideInInspector] public UnityEvent Xray;

    public RawImage targetImage;          // Assign in Inspector
    public Texture2D[] animationFrames;   // Assign your textures in Inspector
    public float frameDelay = 0.1f;       // Delay between frames (in seconds)
    public bool loop = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // <- Destroy the duplicate
        }
        StartCoroutine(InitializePanels(PanelTypes.SplashScreen));
        StartCoroutine(PlayTextureAnimation());
        StartCoroutine(DelayAction(3f, () => MainMenu()));
        
    }
    private IEnumerator InitializePanels(PanelTypes _requirePanel)
    {
        CurrentPanel.PanelTypes = _requirePanel;

        foreach (var item in panels)
        {
            item.Panel.SetActive(item.PanelTypes == CurrentPanel.PanelTypes);
        }
        CurrentPanel = GetPanel(CurrentPanel.PanelTypes);
        yield return new WaitUntil(() => CurrentPanel.Panel != null);
        CurrentPanel.Panel.SetActive(true);
    }

    private IEnumerator DelayAction(float duration, Action OnComplete)
    {
        yield return new WaitForSeconds(duration);
        OnComplete?.Invoke();
    }

    public void MainMenu()
    {
        targetImage.gameObject.SetActive(true);
        StartCoroutine(PlayTextureAnimation());
        FindAnyObjectByType<PlaceObject>().ResetPlacement();
        StartCoroutine(InitializePanels(PanelTypes.MainMenu));
    }
    public void startSimulation()
    {
        StartCoroutine(InitializePanels(PanelTypes.UIControls));
        targetImage.gameObject.SetActive(false);
    }

    private PanelManagement GetPanel(PanelTypes _requiredPanel)
    {
        if (CurrentPanel.PanelTypes != _requiredPanel)
        { 
            return panels.Find(x => x.PanelTypes == _requiredPanel);
        }
        else
        {
            return CurrentPanel;
        }
    }

    public void Connect()
    {
        ConnectCupling?.Invoke();
    }

    public void Disconnect()
    {
        DisconnectCupling?.Invoke();
    }

    public void GasTrigger()
    {
        GasLeak?.Invoke();
    }

    public void EnableXray()
    {
        Xray?.Invoke();
    }

    public void MoveTowardsCupling()
    {
        MoveTowards?.Invoke();
    }

    public void MoveAwayCupling()
    {
        MoveAway?.Invoke();
    }

    IEnumerator PlayTextureAnimation()
    {
        int index = 0;
        while (loop || index < animationFrames.Length)
        {
            targetImage.texture = animationFrames[index];
            index = (index + 1) % animationFrames.Length;
            yield return new WaitForSeconds(frameDelay);
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }
}
