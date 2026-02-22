using UnityEngine;

public class PlaceToggleBinder : MonoBehaviour
{
    public ARTapToPlaceTreasure placer;

    public void OnToggleChanged(bool isOn)
    {
        Debug.Log($"[Toggle_Red] OnToggleChanged called. isOn={isOn}, placer={(placer ? placer.name : "NULL")}");
        if (placer) placer.placeRed = isOn;
        else Debug.LogWarning("[Toggle_Red] 'placer' is NOT assigned!");
    }
}

