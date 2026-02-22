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

    // We will color ALL renderers under this treasure (Cube + lid etc.)
    Renderer[] rends;

    void Awake()
    {
        baseScale = transform.localScale;

        // Grab every renderer in children (including inactive)
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

    public void SetYellow(Material yellowMat)
    {
        if (yellowMat == null || rends == null) return;

        foreach (var r in rends)
        {
            if (r != null) r.material = yellowMat;
        }
    }

    public void SetRed(Material redMat)
    {
        if (redMat == null || rends == null) return;

        foreach (var r in rends)
        {
            if (r != null) r.material = redMat;
        }
    }
}
