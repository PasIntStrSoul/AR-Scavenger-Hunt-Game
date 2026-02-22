using UnityEngine;

public class TreasureIdleMotion : MonoBehaviour
{
    [Header("Motion Settings")]
    public float bobHeight = 0.03f;      // how much it moves up/down
    public float bobSpeed = 1.0f;        // how fast it moves

    [Header("Mimic Difference")]
    public float mimicSpeedMultiplier = 1.35f;  // mimic moves slightly faster
    public float mimicJitter = 0.005f;          // tiny sideways jitter

    Vector3 startPos;
    Treasure treasure;
    bool motionStopped = false;

    void Awake()
    {
        startPos = transform.localPosition;
        treasure = GetComponent<Treasure>();
    }

    void Update()
    {
        // ⛔ Do nothing once motion is stopped
        if (motionStopped) return;

        float speed = bobSpeed;

        bool isMimic = (treasure != null && treasure.isMimic);
        if (isMimic) speed *= mimicSpeedMultiplier;

        // Up/down bob
        float y = Mathf.Sin(Time.time * speed) * bobHeight;

        // Side jitter ONLY for mimics
        float xJitter = 0f;
        float zJitter = 0f;

        if (isMimic)
        {
            xJitter = (Mathf.PerlinNoise(Time.time * speed, 0f) - 0.5f) * mimicJitter;
            zJitter = (Mathf.PerlinNoise(0f, Time.time * speed) - 0.5f) * mimicJitter;
        }

        transform.localPosition = startPos + new Vector3(xJitter, y, zJitter);
    }

    // ✅ CALLED when treasure is collected
    public void StopMotion()
    {
        motionStopped = true;
        transform.localPosition = startPos;
    }

    void OnDisable()
    {
        // Safety reset
        transform.localPosition = startPos;
    }
}
