using UnityEngine;
using UnityEngine.EventSystems;

public class TouchSelect : MonoBehaviour
{
    public TreasureManager manager;

    [Header("Raycast")]
    public float rayDistance = 100f;

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // Keep camera reference alive (AR setups sometimes change camera/main tag)
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // ---------------------------
        // TOUCH (Phone)
        // ---------------------------
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began) return;

            // Ignore touches on UI (Collect/Start/Restart buttons etc.)
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
            // Ignore clicks on UI in editor too
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            TrySelectAtScreenPos(Input.mousePosition);
        }
#endif
    }

    void TrySelectAtScreenPos(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            // Important: collider might be on lid/child, so go up to root Treasure
            Treasure treasure = hit.collider.GetComponentInParent<Treasure>();

            if (treasure != null && manager != null)
            {
                manager.SetSelected(treasure);
            }
        }
    }
}

