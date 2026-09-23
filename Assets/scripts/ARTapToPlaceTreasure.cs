using System.Collections.Generic;
using UnityEngine;
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

    // Fixed pattern (7 good, 3 mimic)
    private List<bool> spawnPattern = new List<bool>();
    private int spawnIndex = 0;

    private int placedCount = 0;

    void Start()
    {
        GenerateSpawnPattern();
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
            int rand = Random.Range(i, spawnPattern.Count);
            bool temp = spawnPattern[i];
            spawnPattern[i] = spawnPattern[rand];
            spawnPattern[rand] = temp;
        }

        spawnIndex = 0;

        Debug.Log("Spawn Pattern Generated: " + string.Join(",", spawnPattern));
    }

    public bool TryAutoPlace(Pose pose)
    {
        if (placedCount >= maxTotalPlacements)
            return false;

        if (spawnIndex >= spawnPattern.Count)
            return false;

        bool isMimic = spawnPattern[spawnIndex];
        spawnIndex++;

        Debug.Log("Spawning Mimic: " + isMimic);

        GameObject obj = Instantiate(yellowTreasurePrefab, pose.position, pose.rotation);

        Treasure treasure = obj.GetComponentInChildren<Treasure>();

        if (treasure != null)
        {
            treasure.isMimic = isMimic;
        }
        else
        {
            Debug.LogError("Treasure script NOT FOUND!");
        }

        placedCount++;

        Debug.Log("Placed Count: " + placedCount);

        // 🔥 CRITICAL FIX — always notify manager
        if (manager != null)
        {
            manager.RecordPlaced(isMimic);
        }
        else
        {
            Debug.LogError("TreasureManager NOT assigned!");
        }

        return true;
    }

    public void ResetPlacementCount()
    {
        placedCount = 0;
        GenerateSpawnPattern();
    }
}