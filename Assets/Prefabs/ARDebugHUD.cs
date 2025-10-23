using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARDebugHUD : MonoBehaviour
{
    ARPlaneManager planes;
    void Awake() => planes = GetComponent<ARPlaneManager>();

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 600, 25), $"ARState: {ARSession.state}");
        GUI.Label(new Rect(10, 35, 600, 25), $"Planes: {(planes ? planes.trackables.count : 0)}");
    }
}

