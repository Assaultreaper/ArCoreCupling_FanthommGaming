using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class PlaceObject : MonoBehaviour
{
    [SerializeField] private GameObject Prefab;

    private ARRaycastManager arRaycastManager;
    private ARPlaneManager arPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private GameObject placedObject; // 👈 reference to current object

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
        if (finger.index != 0)
            return;

        if (arRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            foreach (ARRaycastHit hit in hits)
            {
                Pose pose = hit.pose;
                ARPlane plane = arPlaneManager.GetPlane(hit.trackableId);

                // Destroy previously placed object
                if (placedObject != null)
                    Destroy(placedObject);

                // Spawn with identity rotation first
                placedObject = Instantiate(Prefab, pose.position, Quaternion.identity);

                if (plane.alignment == PlaneAlignment.Vertical)
                {
                    Vector3 wallNormal = pose.rotation * Vector3.back;
                    Quaternion targetRotation = Quaternion.LookRotation(-wallNormal);
                    placedObject.transform.rotation = targetRotation;

                    float offsetDistance = 0.01f;
                    placedObject.transform.position += wallNormal * offsetDistance;
                }
                else
                {
                    placedObject.transform.rotation = pose.rotation;
                }

                break; // only handle the first hit
            }
        }
    }
}
