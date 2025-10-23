using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceOnPlane : MonoBehaviour
{
    [SerializeField] GameObject placedPrefab;

    ARRaycastManager raycaster;
    static readonly List<ARRaycastHit> hits = new();

    void Awake() => raycaster = GetComponent<ARRaycastManager>();

    void Update()
    {
        if (Input.touchCount == 0) return;

        var t = Input.GetTouch(0);
        if (t.phase != TouchPhase.Began) return;

        if (raycaster.Raycast(t.position, hits, TrackableType.PlaneWithinPolygon))
        {
            var pose = hits[0].pose;
            Instantiate(placedPrefab, pose.position, pose.rotation);
        }
    }
}
