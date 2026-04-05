using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AutoSpawnTreasures : MonoBehaviour
{
    [Header("References")]
    public ARPlaneManager planeManager;
    public ARTapToPlaceTreasure placer;

    [Header("Settings")]
    public int spawnCount = 10;
    public float delayBetweenSpawns = 1.2f;

    [Header("Spacing Control")]
    public float minDistanceBetweenTreasures = 0.8f; // 🔥 increased

    private bool hasSpawned = false;
    private List<Vector3> placedPositions = new List<Vector3>();

    void Start()
    {
        StartAutoSpawn();
    }

    public void StartAutoSpawn()
    {
        if (!hasSpawned && planeManager != null && placer != null)
        {
            StartCoroutine(SpawnRoutine());
        }
        else
        {
            Debug.LogWarning("AutoSpawn: Missing references OR already spawned.");
        }
    }

    IEnumerator SpawnRoutine()
    {
        hasSpawned = true;

        int spawned = 0;

        // ✅ Wait for planes
        while (planeManager.trackables.count == 0)
        {
            yield return null;
        }

        // 🔥 IMPORTANT: wait for stable plane detection
        yield return new WaitForSeconds(2.5f);

        while (spawned < spawnCount)
        {
            List<ARPlane> validPlanes = new List<ARPlane>();

            foreach (var p in planeManager.trackables)
            {
                if (p.trackingState == TrackingState.Tracking &&
                    p.alignment == PlaneAlignment.HorizontalUp &&
                    p.size.x > 0.6f && p.size.y > 0.6f) // 🔥 bigger planes only
                {
                    validPlanes.Add(p);
                }
            }

            if (validPlanes.Count == 0)
            {
                yield return null;
                continue;
            }

            ARPlane plane = validPlanes[Random.Range(0, validPlanes.Count)];

            bool placed = false;

            // 🔥 MORE ATTEMPTS = better spread
            for (int i = 0; i < 20; i++)
            {
                Vector2 size = plane.size;

                Vector3 randomLocal = new Vector3(
                    Random.Range(-size.x / 2f, size.x / 2f),
                    0,
                    Random.Range(-size.y / 2f, size.y / 2f)
                );

                Vector3 worldPoint = plane.transform.TransformPoint(randomLocal);

                // 🔥 STRONG distance check
                bool tooClose = false;
                foreach (var pos in placedPositions)
                {
                    if (Vector3.Distance(pos, worldPoint) < minDistanceBetweenTreasures)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                    continue;

                Pose pose = new Pose(worldPoint, Quaternion.identity);

                // 🔥 PLACE TREASURE
                if (placer.TryAutoPlace(pose))
                {
                    placedPositions.Add(worldPoint);
                    spawned++;
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                // 🔥 WAIT MORE IF FAILED
                yield return new WaitForSeconds(0.6f);
            }
            else
            {
                yield return new WaitForSeconds(delayBetweenSpawns);
            }
        }

        Debug.Log("AutoSpawn Completed: " + spawned);
    }
}