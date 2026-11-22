using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// Put this on XR Origin (AR Rig)
public class ShopPlacementManager : MonoBehaviour
{
    public GameObject shopPrefab;
    public float shopScale = 1f;
    public float minPlaneSize = 0.5f;

    public GameObject placementIndicator;
    public GameObject scanningUI;
    public GameObject[] enableAfterPlacing;

    ARRaycastManager raycastManager;
    ARPlaneManager planeManager;
    Camera arCamera;

    GameObject spawnedShop;
    Pose placementPose;
    bool hasPlacementPose;

    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        arCamera = Camera.main;

        // Start in "scanning" mode: no shop yet, menu/spawner hidden
        SetPlaced(false);
    }

    void Update()
    {
        // Shop is already placed, nothing more to do
        if (spawnedShop != null)
            return;

        FindPlacementPose();

        // Move the indicator to where the shop would be placed
        if (placementIndicator != null)
        {
            placementIndicator.SetActive(hasPlacementPose);
            if (hasPlacementPose)
                placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }

        // Show "scan the surface" message until a floor is found
        if (scanningUI != null)
            scanningUI.SetActive(!hasPlacementPose);

        if (hasPlacementPose && IsTapped())
            PlaceShop();
    }

    // Checks if it hits a detected floor/surface
    void FindPlacementPose()
    {
        hasPlacementPose = false;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        if (!raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
            return;

        foreach (ARRaycastHit hit in hits)
        {
            ARPlane plane = planeManager.GetPlane(hit.trackableId);

            // Check whether the surface is flat + surface is big enough
            bool isFloor = plane != null && plane.alignment == PlaneAlignment.HorizontalUp;
            bool isBigEnough = plane != null && plane.size.x * plane.size.y >= minPlaneSize;

            if (isFloor && isBigEnough)
            {
                 // Turn the shop so its faces the player (only rotate left/right, not tilt)
                Vector3 lookDirection = arCamera.transform.position - hit.pose.position;
                lookDirection.y = 0;

                Quaternion rotation = lookDirection != Vector3.zero
                    ? Quaternion.LookRotation(lookDirection)
                    : hit.pose.rotation;

                placementPose = new Pose(hit.pose.position, rotation);
                hasPlacementPose = true;
                return;
            }
        }
    }

    bool IsTapped()
    {
        if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
            return false;

        bool tappedOnUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        return !tappedOnUI;
    }

    public void PlaceShop()
    {
        spawnedShop = Instantiate(shopPrefab, placementPose.position, placementPose.rotation);
        spawnedShop.transform.localScale = Vector3.one * shopScale;
        spawnedShop.AddComponent<ARAnchor>();

        SetPlaced(true);
    }

    // Removes the shop and goes back to "scanning"
    public void ResetShop()
    {
        if (spawnedShop != null)
            Destroy(spawnedShop);

        spawnedShop = null;
        SetPlaced(false);
    }

    // Switches everything between "scanning" and "placed" mode
    void SetPlaced(bool placed)
    {
        if (placementIndicator != null && placed)
            placementIndicator.SetActive(false);

        if (scanningUI != null)
            scanningUI.SetActive(!placed);

        foreach (GameObject obj in enableAfterPlacing)
            obj.SetActive(placed);

        ShowPlanes(!placed);
    }

    // Hides or shows the plane overlays. Planes keep tracking, so the Object Spawner still works
    void ShowPlanes(bool show)
    {
        foreach (ARPlane plane in planeManager.trackables)
        {
            foreach (Renderer r in plane.GetComponentsInChildren<Renderer>())
                r.enabled = show;
        }
    }
}