using UnityEngine;
using UnityEngine.UI;

public class ToggleSync : MonoBehaviour
{
    public Toggle toggleRed;
    public ARTapToPlaceTreasure placer;

    void Start()
    {
        if (toggleRed != null)
        {
            toggleRed.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    void OnToggleChanged(bool isOn)
    {
        // 🔥 THIS SYSTEM IS NO LONGER USED
        // mimicChancePercent has been removed

        Debug.Log("Toggle changed (no longer controls mimic chance)");
    }
}