using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ARTapToPlaceTreasure))]
public class ToggleSync : MonoBehaviour
{
    public Toggle toggleRed;
    private ARTapToPlaceTreasure placer;

    void Awake()
    {
        placer = GetComponent<ARTapToPlaceTreasure>();
    }

    void Start()
    {
        if (!toggleRed || !placer) { Debug.LogWarning("[ToggleSync] Missing refs"); return; }

        // 1) Apply the initial toggle state at startup
        placer.SetPlaceRed(toggleRed.isOn);

        // 2) Keep them in sync on every change
        toggleRed.onValueChanged.AddListener(placer.SetPlaceRed);
    }
}

