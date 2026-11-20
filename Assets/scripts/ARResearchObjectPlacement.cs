using System;
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
        Selection,
        Collection
    }

    // Optional per-part material sets for objects with multiple meshes.
    [Serializable]
    public class RendererMaterialOverride
    {
        [Tooltip("Child GameObject name, e.g. Cap, Lead, or Tip")]
        public string rendererObjectName;

        public Material brightMaterial;
        public Material mediumMaterial;
        public Material darkMaterial;
    }

    [Serializable]
    public class ResearchObjectDefinition
    {
        public string objectName;
        public GameObject prefab;

        public Material brightMaterial;
        public Material mediumMaterial;
        public Material darkMaterial;

        [Tooltip("Optional overrides for multi-part objects. Unlisted parts use the main materials.")]
        public List<RendererMaterialOverride> rendererOverrides =
            new List<RendererMaterialOverride>();

        public LightingCondition initialLighting =
            LightingCondition.Bright;
    }

    private class PlacedResearchObject
    {
        public GameObject instance;
        public ResearchObjectDefinition definition;
        public LightingCondition lighting;
    }

    [Header("AR References")]
    public ARRaycastManager raycastManager;
    public Camera arCamera;

    [Header("Research Object Library")]
    public List<ResearchObjectDefinition> researchObjects =
        new List<ResearchObjectDefinition>();

    [Header("Interaction Mode")]
    public InteractionMode currentMode =
        InteractionMode.Placement;

    [Header("Placement Settings")]
    [Tooltip("0 means unlimited placements.")]
    public int maxPlacements = 0;

    [Range(0f, 1f)]
    public float minimumUpwardAlignment = 0.9f;

    public float verticalPlacementOffset = 0f;

    [Header("Collection Settings")]
    public int collectedCount = 0;

    private readonly List<PlacedResearchObject> placedObjects =
        new List<PlacedResearchObject>();

    private PlacedResearchObject selectedObject;

    private int nextObjectIndex = 0;
    private int totalPlacedCount = 0;

    private static readonly List<ARRaycastHit> hits =
        new List<ARRaycastHit>();

    public int ActiveObjectCount
    {
        get { return placedObjects.Count; }
    }

    public int TotalPlacedCount
    {
        get { return totalPlacedCount; }
    }

    public int CollectedCount
    {
        get { return collectedCount; }
    }

    private void Update()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase != TouchPhase.Began)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        switch (currentMode)
        {
            case InteractionMode.Placement:
                TryPlaceObject(touch.position);
                break;

            case InteractionMode.Selection:
                TrySelectObject(touch.position);
                break;

            case InteractionMode.Collection:
                TryCollectObject(touch.position);
                break;
        }
    }

    private void TryPlaceObject(Vector2 screenPosition)
    {
        if (researchObjects == null ||
            researchObjects.Count == 0)
        {
            Debug.LogWarning(
                "No research objects configured."
            );
            return;
        }

        if (maxPlacements > 0 &&
            placedObjects.Count >= maxPlacements)
        {
            Debug.Log("Maximum placements reached.");
            return;
        }

        if (raycastManager == null)
        {
            Debug.LogWarning(
                "AR Raycast Manager missing."
            );
            return;
        }

        if (!raycastManager.Raycast(
            screenPosition,
            hits,
            TrackableType.PlaneWithinPolygon))
        {
            return;
        }

        Pose hitPose = hits[0].pose;

        Vector3 surfaceNormal =
            hitPose.rotation * Vector3.up;

        float alignment =
            Vector3.Dot(
                surfaceNormal.normalized,
                Vector3.up
            );

        if (alignment < minimumUpwardAlignment)
            return;

        ResearchObjectDefinition definition = null;

        // Find the next configured prefab.
        for (int i = 0; i < researchObjects.Count; i++)
        {
            int index =
                (nextObjectIndex + i) %
                researchObjects.Count;

            if (researchObjects[index] != null &&
                researchObjects[index].prefab != null)
            {
                definition = researchObjects[index];

                nextObjectIndex =
                    (index + 1) %
                    researchObjects.Count;

                break;
            }
        }

        if (definition == null)
        {
            Debug.LogWarning(
                "No valid research prefabs available."
            );
            return;
        }

        Vector3 correctedPosition =
            hitPose.position +
            Vector3.up * verticalPlacementOffset;

        GameObject newObject = Instantiate(
            definition.prefab,
            correctedPosition,
            Quaternion.identity
        );

        newObject.SetActive(true);

        PlacedResearchObject record =
            new PlacedResearchObject
            {
                instance = newObject,
                definition = definition,
                lighting = definition.initialLighting
            };

        placedObjects.Add(record);
        totalPlacedCount++;

        ApplyLighting(
            record,
            definition.initialLighting
        );

        Debug.Log(
            "Placed: " + definition.objectName +
            " | Active: " + ActiveObjectCount
        );
    }

    private PlacedResearchObject FindObjectAtScreenPosition(
        Vector2 screenPosition)
    {
        if (arCamera == null)
            return null;

        Ray ray =
            arCamera.ScreenPointToRay(screenPosition);

        RaycastHit[] rayHits =
            Physics.RaycastAll(ray);

        Array.Sort(
            rayHits,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit hit in rayHits)
        {
            foreach (PlacedResearchObject record
                in placedObjects)
            {
                if (record.instance == null)
                    continue;

                Transform root =
                    record.instance.transform;

                if (hit.transform == root ||
                    hit.transform.IsChildOf(root))
                {
                    return record;
                }
            }
        }

        return null;
    }

    private void TrySelectObject(Vector2 screenPosition)
    {
        PlacedResearchObject record =
            FindObjectAtScreenPosition(screenPosition);

        if (record == null)
        {
            Debug.Log("No research object selected.");
            return;
        }

        selectedObject = record;

        Debug.Log(
            "Selected: " +
            record.definition.objectName
        );
    }

    private void TryCollectObject(Vector2 screenPosition)
    {
        PlacedResearchObject record =
            FindObjectAtScreenPosition(screenPosition);

        if (record == null)
        {
            Debug.Log("No research object to collect.");
            return;
        }

        CollectObject(record);
    }

    private void CollectObject(
        PlacedResearchObject record)
    {
        if (record == null ||
            record.instance == null)
            return;

        string collectedName =
            record.definition.objectName;

        if (selectedObject == record)
            selectedObject = null;

        placedObjects.Remove(record);

        Destroy(record.instance);

        collectedCount++;

        Debug.Log(
            "Collected: " + collectedName +
            " | Total Collected: " + collectedCount +
            " | Remaining: " + ActiveObjectCount
        );
    }

    // Select the material corresponding to the lighting condition.
    private static Material GetLightingMaterial(
        Material bright,
        Material medium,
        Material dark,
        LightingCondition condition)
    {
        switch (condition)
        {
            case LightingCondition.Bright:
                return bright;

            case LightingCondition.Medium:
                return medium;

            case LightingCondition.Dark:
                return dark;

            default:
                return bright;
        }
    }

    // Apply lighting while supporting individual mesh materials.
    private void ApplyLighting(
        PlacedResearchObject record,
        LightingCondition condition)
    {
        if (record == null ||
            record.instance == null ||
            record.definition == null)
        {
            return;
        }

        ResearchObjectDefinition definition =
            record.definition;

        Material mainMaterial = GetLightingMaterial(
            definition.brightMaterial,
            definition.mediumMaterial,
            definition.darkMaterial,
            condition
        );

        if (mainMaterial == null)
        {
            Debug.LogWarning(
                "HDRI material missing for " +
                definition.objectName
            );

            return;
        }

        Renderer[] renderers =
            record.instance.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            Material chosenMaterial = mainMaterial;

            // Check whether this mesh has its own materials.
            if (definition.rendererOverrides != null)
            {
                foreach (RendererMaterialOverride entry
                    in definition.rendererOverrides)
                {
                    if (entry == null ||
                        string.IsNullOrEmpty(entry.rendererObjectName) ||
                        entry.rendererObjectName != renderer.gameObject.name)
                    {
                        continue;
                    }

                    chosenMaterial = GetLightingMaterial(
                        entry.brightMaterial,
                        entry.mediumMaterial,
                        entry.darkMaterial,
                        condition
                    );

                    break;
                }
            }

            if (chosenMaterial == null)
            {
                Debug.LogWarning(
                    "Missing " + condition +
                    " material for " +
                    definition.objectName +
                    " / " + renderer.gameObject.name
                );

                continue;
            }

            Material[] materials =
                renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = chosenMaterial;
            }

            renderer.sharedMaterials = materials;
        }

        record.lighting = condition;
    }

    public void SetSelectedLighting(int lightingIndex)
    {
        if (selectedObject == null ||
            selectedObject.instance == null)
        {
            Debug.Log("Select an object first.");
            return;
        }

        if (lightingIndex < 0 ||
            lightingIndex > 2)
        {
            return;
        }

        LightingCondition condition =
            (LightingCondition)lightingIndex;

        ApplyLighting(
            selectedObject,
            condition
        );

        Debug.Log(
            "Lighting changed to: " + condition
        );
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

    public void EnableCollectionMode()
    {
        currentMode = InteractionMode.Collection;

        Debug.Log("Collection Mode Enabled");
    }

    public void CollectSelectedObject()
    {
        if (selectedObject == null)
        {
            Debug.Log("Select an object first.");
            return;
        }

        CollectObject(selectedObject);
    }

    public void ResetResearchPlacement()
    {
        foreach (PlacedResearchObject record
            in placedObjects)
        {
            if (record.instance != null)
            {
                Destroy(record.instance);
            }
        }

        placedObjects.Clear();

        selectedObject = null;
        nextObjectIndex = 0;
        totalPlacedCount = 0;
        collectedCount = 0;

        currentMode = InteractionMode.Placement;

        Debug.Log("Research Placement Reset");
    }
}