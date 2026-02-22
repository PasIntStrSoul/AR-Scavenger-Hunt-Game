using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTapToPlaceTreasure : MonoBehaviour
{
    [Header("References")]
    public TreasureManager manager; // drag _Game (TreasureManager) here

    [Header("AR")]
    [SerializeField] private ARRaycastManager raycastManager;
    private static readonly List<ARRaycastHit> hits = new();

    [Header("Prefabs")]
    public GameObject yellowTreasurePrefab;
    public GameObject redTreasurePrefab; // legacy, not used in mimic mode

    [Header("Legacy UI Toggle (Ignore for Mimic mode)")]
    public bool placeRed = false; // kept so nothing breaks

    [Header("Limits")]
    [Tooltip("Total number of treasures allowed to be placed (Yellow + Red combined).")]
    public int maxTotalPlacements = 10;

    [Header("Mimic Rules")]
    [Range(0, 100)]
    public int mimicChancePercent = 30;

    private int placedTotal = 0;

    public void SetPlaceRed(bool value)
    {
        placeRed = value;
        Debug.Log($"[Placer] placeRed now = {placeRed} (ignored in Mimic mode)");
    }

    private void Awake()
    {
        if (!raycastManager)
            raycastManager = FindObjectOfType<ARRaycastManager>();
    }

    private void Update()
    {
        if (placedTotal >= maxTotalPlacements)
            return;

        // Touch placement on device
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began) return;

            // Prevent placing when tapping UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            TryPlaceAtScreenPos(touch.position);
            return;
        }

#if UNITY_EDITOR
        // Mouse placement in Editor (optional, helps testing)
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            TryPlaceAtScreenPos(Input.mousePosition);
        }
#endif
    }

    void TryPlaceAtScreenPos(Vector2 screenPos)
    {
        if (raycastManager == null) return;

        if (!raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
            return;

        Pose pose = hits[0].pose;

        GameObject prefab = yellowTreasurePrefab;
        if (prefab == null)
        {
            Debug.LogWarning("[Placer] Yellow prefab is missing! Assign YellowTreasurePrefab in Inspector.");
            return;
        }

        // Spawn treasure
        GameObject go = Instantiate(prefab, pose.position, pose.rotation);

        // Find Treasure component robustly (root OR children)
        Treasure t = go.GetComponent<Treasure>();
        if (t == null)
            t = go.GetComponentInChildren<Treasure>(true);

        if (t != null)
        {
            // Set secret mimic state
            t.isMimic = (Random.Range(0, 100) < mimicChancePercent);

            // Force yellow appearance at spawn (Negrin rule)
            if (manager != null && manager.yellowMat != null)
                t.SetYellow(manager.yellowMat);
        }
        else
        {
            Debug.LogWarning("[Placer] Spawned treasure has no Treasure component (root or children). Add Treasure.cs to the prefab.");
        }

        placedTotal++;

        // Notify manager (counts placement)
        if (manager != null)
            manager.RecordPlaced(false);

        string secret = (t != null) ? (t.isMimic ? "MIMIC" : "GOOD") : "NO_TREASURE_COMPONENT";
        Debug.Log($"[Placer] Placed YELLOW (secret: {secret}) | Total = {placedTotal}/{maxTotalPlacements}");
    }

    public void ResetPlacementCount()
    {
        placedTotal = 0;
    }

    public void DisablePlacement()
    {
        placedTotal = maxTotalPlacements;
    }
}
