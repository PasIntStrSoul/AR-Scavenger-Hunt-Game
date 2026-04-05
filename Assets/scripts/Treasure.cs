using UnityEngine;

public class Treasure : MonoBehaviour
{
    // ---------- Selection ----------
    Vector3 baseScale;
    [SerializeField] float selectedScaleMultiplier = 1.15f;
    public bool IsSelected { get; private set; }

    // ---------- Mimic ----------
    [Header("Mimic Settings")]
    public bool isMimic = false;

    // ---------- Materials ----------
    [Header("Materials")]
    public Material yellowMat;
    public Material redMat;

    Renderer[] rends;

    void Awake()
    {
        baseScale = transform.localScale;

        rends = GetComponentsInChildren<Renderer>(true);
    }

    public void Select()
    {
        IsSelected = true;
        transform.localScale = baseScale * selectedScaleMultiplier;
    }

    public void Deselect()
    {
        IsSelected = false;
        transform.localScale = baseScale;
    }

    // 🔥 Set Yellow
    public void SetYellow()
    {
        if (yellowMat == null || rends == null) return;

        foreach (var r in rends)
        {
            if (r != null) r.material = yellowMat;
        }
    }

    // 🔥 Set Red (USED BY MANAGER)
    public void SetRed()
    {
        if (redMat == null || rends == null) return;

        foreach (var r in rends)
        {
            if (r != null) r.material = redMat;
        }
    }
    public void SetGreen()
    {
        if (yellowMat == null || rends == null) return;

        foreach (var r in rends)
        {
            if (r != null) r.material.color = Color.green;
        }
    }
}