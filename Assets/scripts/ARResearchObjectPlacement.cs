using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARResearchObjectPlacement : MonoBehaviour
{
    public enum LightingCondition
    {
        Bright,
        Medium,
        Dark
    }

    public enum InteractionMode
    {
        Placement,
        Selection
    }

    private enum ResearchObjectType
    {
        Chair,
        Stool,
        Vase
    }

    [Header("AR References")]
    public ARRaycastManager raycastManager;
    public Camera arCamera;

    [Header("Research Object Prefabs")]
    public GameObject chairPrefab;
    public GameObject stoolPrefab;
    public GameObject vasePrefab;

    [Header("Chair HDRI Materials")]
    public Material chairBright;
    public Material chairMedium;
    public Material chairDark;

    [Header("Stool HDRI Materials")]
    public Material stoolBright;
    public Material stoolMedium;
    public Material stoolDark;

    [Header("Vase HDRI Materials")]
    public Material vaseBright;
    public Material vaseMedium;
    public Material vaseDark;

    [Header("Initial Lighting Configuration")]
    public LightingCondition chairLighting = LightingCondition.Bright;
    public LightingCondition stoolLighting = LightingCondition.Medium;
    public LightingCondition vaseLighting = LightingCondition.Dark;

    [Header("Interaction Mode")]
    public InteractionMode currentMode = InteractionMode.Placement;

    [Header("Placement Settings")]
    public int maxPlacements = 3;
    public float minimumUpwardAlignment = 0.8f;
    public float verticalPlacementOffset = 0.155f;

    private readonly List<GameObject> placedObjects =
        new List<GameObject>();

    private GameObject selectedObject;

    private static readonly List<ARRaycastHit> hits =
        new List<ARRaycastHit>();

    void Update()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase != TouchPhase.Began)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        if (currentMode == InteractionMode.Placement)
            TryPlaceObject(touch.position);
        else
            TrySelectObject(touch.position);
    }

    void TryPlaceObject(Vector2 screenPosition)
    {
        int placementIndex = placedObjects.Count;

        if (placementIndex >= maxPlacements || placementIndex >= 3)
        {
            Debug.Log("Maximum research objects placed.");
            return;
        }

        if (raycastManager == null)
        {
            Debug.LogWarning("AR Raycast Manager missing.");
            return;
        }

        ResearchObjectType objectType =
            (ResearchObjectType)placementIndex;

        GameObject prefabToPlace = null;
        LightingCondition initialLighting = LightingCondition.Bright;

        switch (objectType)
        {
            case ResearchObjectType.Chair:
                prefabToPlace = chairPrefab;
                initialLighting = chairLighting;
                break;

            case ResearchObjectType.Stool:
                prefabToPlace = stoolPrefab;
                initialLighting = stoolLighting;
                break;

            case ResearchObjectType.Vase:
                prefabToPlace = vasePrefab;
                initialLighting = vaseLighting;
                break;
        }

        if (prefabToPlace == null)
        {
            Debug.LogWarning("Research prefab missing.");
            return;
        }

        if (!raycastManager.Raycast(
            screenPosition,
            hits,
            TrackableType.PlaneWithinPolygon))
            return;

        Pose hitPose = hits[0].pose;

        Vector3 surfaceNormal =
            hitPose.rotation * Vector3.up;

        float alignment =
            Vector3.Dot(surfaceNormal, Vector3.up);

        if (alignment < minimumUpwardAlignment)
            return;

        Vector3 correctedPosition =
            hitPose.position +
            Vector3.up * verticalPlacementOffset;

        GameObject newObject = Instantiate(
            prefabToPlace,
            correctedPosition,
            Quaternion.identity
        );

        newObject.SetActive(true);

        placedObjects.Add(newObject);

        ApplyLighting(
            newObject,
            objectType,
            initialLighting
        );

        Debug.Log("Placed: " + newObject.name);
    }

    void TrySelectObject(Vector2 screenPosition)
    {
        if (arCamera == null)
            return;

        Ray ray = arCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            foreach (GameObject obj in placedObjects)
            {
                if (obj != null &&
                    (hit.transform == obj.transform ||
                     hit.transform.IsChildOf(obj.transform)))
                {
                    selectedObject = obj;

                    Debug.Log("Selected: " + obj.name);
                    return;
                }
            }
        }
    }

    void ApplyLighting(
        GameObject obj,
        ResearchObjectType objectType,
        LightingCondition condition)
    {
        Material selectedMaterial = null;

        switch (objectType)
        {
            case ResearchObjectType.Chair:

                switch (condition)
                {
                    case LightingCondition.Bright:
                        selectedMaterial = chairBright;
                        break;

                    case LightingCondition.Medium:
                        selectedMaterial = chairMedium;
                        break;

                    case LightingCondition.Dark:
                        selectedMaterial = chairDark;
                        break;
                }
                break;

            case ResearchObjectType.Stool:

                switch (condition)
                {
                    case LightingCondition.Bright:
                        selectedMaterial = stoolBright;
                        break;

                    case LightingCondition.Medium:
                        selectedMaterial = stoolMedium;
                        break;

                    case LightingCondition.Dark:
                        selectedMaterial = stoolDark;
                        break;
                }
                break;

            case ResearchObjectType.Vase:

                switch (condition)
                {
                    case LightingCondition.Bright:
                        selectedMaterial = vaseBright;
                        break;

                    case LightingCondition.Medium:
                        selectedMaterial = vaseMedium;
                        break;

                    case LightingCondition.Dark:
                        selectedMaterial = vaseDark;
                        break;
                }
                break;
        }

        if (selectedMaterial == null)
        {
            Debug.LogWarning("HDRI material missing.");
            return;
        }

        Renderer renderer = obj.GetComponentInChildren<Renderer>();

        if (renderer != null)
            renderer.sharedMaterial = selectedMaterial;
    }

    public void SetSelectedLighting(int lightingIndex)
    {
        if (selectedObject == null)
        {
            Debug.Log("Select an object first.");
            return;
        }

        if (lightingIndex < 0 || lightingIndex > 2)
            return;

        LightingCondition condition =
            (LightingCondition)lightingIndex;

        int objectIndex =
            placedObjects.IndexOf(selectedObject);

        if (objectIndex < 0 || objectIndex >= 3)
            return;

        ResearchObjectType objectType =
            (ResearchObjectType)objectIndex;

        ApplyLighting(
            selectedObject,
            objectType,
            condition
        );

        Debug.Log("Lighting changed to: " + condition);
    }

    public void EnablePlacementMode()
    {
        currentMode = InteractionMode.Placement;
        Debug.Log("Placement Mode Enabled");
    }

    public void EnableSelectionMode()
    {
        currentMode = InteractionMode.Selection;
        Debug.Log("Selection Mode Enabled");
    }

    public void ResetResearchPlacement()
    {
        foreach (GameObject obj in placedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        placedObjects.Clear();
        selectedObject = null;

        currentMode = InteractionMode.Placement;

        Debug.Log("Research Placement Reset");
    }
}