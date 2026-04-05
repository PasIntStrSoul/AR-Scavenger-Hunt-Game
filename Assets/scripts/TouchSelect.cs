using UnityEngine;
using UnityEngine.EventSystems;

public class TouchSelect : MonoBehaviour
{
    public TreasureManager manager;

    [Header("Raycast")]
    public float rayDistance = 100f;

    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // Keep camera reference alive (important for AR)
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // ---------------------------
        // TOUCH (Phone)
        // ---------------------------
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase != TouchPhase.Began)
                return;

            // Ignore UI touches
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            TrySelectAtScreenPos(touch.position);
            return;
        }

#if UNITY_EDITOR
        // ---------------------------
        // MOUSE (Editor testing)
        // ---------------------------
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            TrySelectAtScreenPos(Input.mousePosition);
        }
#endif
    }

    void TrySelectAtScreenPos(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);

        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            Debug.Log("HIT: " + hit.collider.name);

            // Get Treasure from parent (IMPORTANT for child colliders)
            Treasure treasure = hit.collider.GetComponentInParent<Treasure>();

            if (treasure != null)
            {
                Debug.Log("Treasure Selected: " + treasure.name);

                if (manager != null)
                {
                    manager.SetSelected(treasure);
                }
                else
                {
                    Debug.LogWarning("TreasureManager is NULL!");
                }
            }
            else
            {
                Debug.Log("Hit object is NOT a Treasure");
            }
        }
        else
        {
            Debug.Log("Raycast did NOT hit anything");
        }
    }
}