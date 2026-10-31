using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTapToPlaceTreasure : MonoBehaviour
{
    [Header("References")]
    public ARRaycastManager raycastManager;
    public TreasureManager manager;

    [Header("Prefabs")]
    public GameObject yellowTreasurePrefab;

    [Header("Settings")]
    public int maxTotalPlacements = 10;

    [Header("Floor Placement")]
    [Tooltip("Allow upward-facing horizontal planes when floor classification is unavailable.")]
    public bool allowUnclassifiedHorizontalPlanes = true;

    [Range(0.5f, 1f)]
    public float minimumUpwardAlignment = 0.9f;

    private List<bool> spawnPattern = new List<bool>();

    private int spawnIndex = 0;
    private int placedCount = 0;

    private bool placementMode = true;

    private static List<ARRaycastHit> hits =
        new List<ARRaycastHit>();

    void Start()
    {
        GenerateSpawnPattern();
        placementMode = true;
    }

    void Update()
    {
        if (!placementMode)
            return;

        if (placedCount >= maxTotalPlacements)
            return;

        if (raycastManager == null)
            return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase != TouchPhase.Began)
                return;

            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(
                    touch.fingerId))
                return;

            TryPlaceAtScreenPosition(touch.position);
        }
    }

    void TryPlaceAtScreenPosition(Vector2 screenPosition)
    {
        if (!raycastManager.Raycast(
            screenPosition,
            hits,
            TrackableType.PlaneWithinPolygon))
        {
            return;
        }

        foreach (ARRaycastHit hit in hits)
        {
            ARPlane plane =
                hit.trackable as ARPlane;

            if (plane == null)
                continue;

            // Only accept upward-facing horizontal surfaces.
            if (plane.alignment != PlaneAlignment.HorizontalUp)
                continue;

            Vector3 surfaceNormal =
                hit.pose.rotation * Vector3.up;

            if (Vector3.Dot(
                surfaceNormal,
                Vector3.up) < minimumUpwardAlignment)
            {
                continue;
            }

            // Reject planes explicitly classified as non-floor.
            // ARCore may return None when classification is unavailable.
            PlaneClassification classification =
                plane.classification;

            if (classification == PlaneClassification.Floor)
            {
                TryAutoPlace(hit.pose);
                return;
            }

            if (classification == PlaneClassification.None &&
                allowUnclassifiedHorizontalPlanes)
            {
                TryAutoPlace(hit.pose);
                return;
            }
        }

        Debug.Log(
            "No acceptable floor plane detected at touch position.");
    }

    void GenerateSpawnPattern()
    {
        spawnPattern.Clear();

        for (int i = 0; i < 7; i++)
            spawnPattern.Add(false);

        for (int i = 0; i < 3; i++)
            spawnPattern.Add(true);

        for (int i = 0; i < spawnPattern.Count; i++)
        {
            int rand =
                Random.Range(i, spawnPattern.Count);

            bool temp = spawnPattern[i];

            spawnPattern[i] = spawnPattern[rand];
            spawnPattern[rand] = temp;
        }

        spawnIndex = 0;

        Debug.Log("Spawn Pattern Generated");
    }

    public bool TryAutoPlace(Pose pose)
    {
        if (!placementMode)
            return false;

        if (placedCount >= maxTotalPlacements)
            return false;

        if (spawnIndex >= spawnPattern.Count)
            return false;

        if (yellowTreasurePrefab == null)
        {
            Debug.LogError(
                "Treasure prefab is missing!");
            return false;
        }

        bool isMimic =
            spawnPattern[spawnIndex];

        // Preserve the original placement rotation.
        GameObject obj = Instantiate(
            yellowTreasurePrefab,
            pose.position,
            pose.rotation
        );

        Treasure treasure =
            obj.GetComponentInChildren<Treasure>();

        if (treasure != null)
        {
            treasure.isMimic = isMimic;
        }
        else
        {
            Debug.LogError(
                "Treasure script NOT FOUND!");

            Destroy(obj);
            return false;
        }

        spawnIndex++;
        placedCount++;

        if (manager != null)
            manager.RecordPlaced(isMimic);

        Debug.Log(
            "Object placed: " + placedCount);

        return true;
    }

    public void DisablePlacement()
    {
        placementMode = false;

        Debug.Log("Picking Mode Activated");
    }

    public void EnablePlacement()
    {
        placementMode = true;

        Debug.Log("Placement Mode Activated");
    }

    public void ResetPlacementCount()
    {
        placedCount = 0;

        GenerateSpawnPattern();

        placementMode = true;
    }
}