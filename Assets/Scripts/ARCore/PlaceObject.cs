using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class PlaceObject : MonoBehaviour
{
    [SerializeField] private GameObject Prefab;

    private ARRaycastManager arRaycastManager;
    private ARPlaneManager arPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private GameObject placedObject;
    private bool hasPlaced = false;

    private void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arPlaneManager = GetComponent<ARPlaneManager>();
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Vertical;
    }

    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    private void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
    }

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (hasPlaced)
            return;

        if (finger.index != 0)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(finger.index))
            return;

        if (arRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            foreach (ARRaycastHit hit in hits)
            {
                Pose pose = hit.pose;
                ARPlane plane = arPlaneManager.GetPlane(hit.trackableId);

                placedObject = Instantiate(Prefab, pose.position, Prefab.transform.rotation);
                hasPlaced = true;

                if (plane.alignment == PlaneAlignment.Vertical)
                {
                    Quaternion rotationOffset = Prefab.transform.rotation;
                    Quaternion targetRotation = Quaternion.LookRotation(-plane.transform.forward) * rotationOffset;
                    placedObject.transform.rotation = targetRotation;

                    float offsetDistance = 0.01f;
                    placedObject.transform.position = pose.position + placedObject.transform.forward * offsetDistance;
                }
                else
                {
                    placedObject.transform.rotation = pose.rotation;
                }

                break; // Only use first valid hit
            }
        }
    }

    public void ResetPlacement()
    {
        if (placedObject != null)
        {
            Destroy(placedObject);
            placedObject = null;
        }

        hasPlaced = false;
    }
}
