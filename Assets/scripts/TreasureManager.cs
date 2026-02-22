using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TreasureManager : MonoBehaviour
{
    [Header("UI")]
    public Button collectButton;
    public TextMeshProUGUI scoreText;

    [Header("Start / Restart UI")]
    public Button startButton;
    public Button restartButton;

    [Header("Timer UI (Optional but recommended)")]
    public TextMeshProUGUI timerCenterText;

    [Header("Final Score UI (Optional)")]
    public TextMeshProUGUI finalScoreText;

    [Header("Rules")]
    public int maxTotalPlacements = 10;
    public int gameDurationSeconds = 60;

    [Header("Mimic Visuals")]
    public Material yellowMat;              // assign in Inspector
    public Material redMat;                 // assign in Inspector
    public float mimicRevealSeconds = 1.0f; // how long mimic stays red before disappearing

    [Header("Mimic Tap Shake (VERY VISIBLE)")]
    [Tooltip("How long the mimic shakes after you TAP/select it (before collect).")]
    public float mimicTapShakeSeconds = 0.6f;

    [Tooltip("How far it shakes side-to-side (try 0.02 to 0.06).")]
    public float mimicTapShakeAmount = 0.035f;

    [Tooltip("How fast it shakes (try 40 to 80).")]
    public float mimicTapShakeSpeed = 55f;

    [Header("References")]
    public ARTapToPlaceTreasure placer; // drag XR Origin (with ARTapToPlaceTreasure) here

    // Counters
    int placedYellow;
    int collectedYellow, collectedRed;
    int score;

    // Selection
    Treasure selected;

    // Game state
    bool gameRunning = false;
    float timeLeft = 0f;
    Coroutine timerRoutine;

    // Tap-shake control
    Coroutine currentShakeRoutine;
    Transform shakeTarget;
    Vector3 shakeStartLocalPos;

    void Start()
    {
        if (collectButton) collectButton.onClick.AddListener(CollectSelected);
        if (startButton) startButton.onClick.AddListener(StartGame);
        if (restartButton) restartButton.onClick.AddListener(RestartGame);

        // Initial UI state
        if (restartButton) restartButton.gameObject.SetActive(false);
        if (startButton) startButton.gameObject.SetActive(false);

        SetTimerVisible(false);

        if (finalScoreText) finalScoreText.gameObject.SetActive(false);

        // Gameplay UI visible in placement phase
        SetGameplayUIVisible(true);

        UpdateUI();
    }

    // Called by ARTapToPlaceTreasure when a treasure is spawned
    // Negrin rule: everything looks yellow at placement time
    public void RecordPlaced(bool isRed)
    {
        placedYellow++;

        // show Start once we placed all
        if (TotalPlaced() >= maxTotalPlacements)
        {
            if (startButton) startButton.gameObject.SetActive(true);
        }

        UpdateUI();
    }

    // Called when a treasure is collected
    // Here, isRed means "isMimic" (bad)
    public void RecordCollected(bool isRed)
    {
        if (isRed) collectedRed++;
        else collectedYellow++;

        UpdateUI();
    }

    // Called by TouchSelect when you tap a treasure
    public void SetSelected(Treasure t)
    {
        if (!gameRunning)
        {
            StopAnyShake();
            if (selected) selected.Deselect();
            selected = null;
            if (collectButton) collectButton.interactable = false;
            return;
        }

        // Deselect old
        if (selected && selected != t)
        {
            StopAnyShake();
            selected.Deselect();
        }

        selected = t;

        if (selected)
        {
            selected.Select();

            // ✅ VERY visible shake ONLY for mimics (on tap/select)
            if (selected.isMimic)
                StartMimicTapShake(selected);
            else
                StopAnyShake();
        }

        UpdateUI();
    }

    void CollectSelected()
    {
        if (!gameRunning) return;
        if (!selected) return;

        // Stop tap shake cleanly
        StopAnyShake();

        bool isMimic = selected.isMimic;

        // ✅ Freeze idle motion BEFORE reveal/destroy (prevents weird “blink/disappear”)
        var idle = selected.GetComponent<TreasureIdleMotion>();
        if (idle != null) idle.StopMotion();

        // Update score immediately
        score += isMimic ? -1 : 1;
        RecordCollected(isMimic);

        // Reveal mimic (turn red briefly) then destroy
        StartCoroutine(RevealThenDestroy(selected, isMimic));

        // Clear current selection immediately
        selected = null;
        UpdateUI();
    }

    IEnumerator RevealThenDestroy(Treasure t, bool isMimic)
    {
        if (t == null) yield break;

        if (isMimic)
        {
            // ✅ TURN RED IMMEDIATELY (you had this working, keep it)
            if (redMat != null) t.SetRed(redMat);

            yield return new WaitForSeconds(mimicRevealSeconds);
        }

        if (t != null) Destroy(t.gameObject);

        // End if all collected
        if (TotalPlaced() >= maxTotalPlacements && TotalCollected() >= TotalPlaced())
        {
            EndGame("All treasures collected!");
        }
    }

    void StartGame()
    {
        if (TotalPlaced() < maxTotalPlacements) return;

        gameRunning = true;

        if (startButton) startButton.gameObject.SetActive(false);
        if (restartButton) restartButton.gameObject.SetActive(false);
        if (finalScoreText) finalScoreText.gameObject.SetActive(false);

        SetGameplayUIVisible(true);

        timeLeft = gameDurationSeconds;
        SetTimerVisible(true);
        UpdateTimerUI();

        if (timerRoutine != null) StopCoroutine(timerRoutine);
        timerRoutine = StartCoroutine(TimerCountdown());

        UpdateUI();
    }

    IEnumerator TimerCountdown()
    {
        while (gameRunning && timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0f) timeLeft = 0f;

            UpdateTimerUI();
            yield return null;
        }

        if (gameRunning)
        {
            EndGame("Time up!");
        }
    }

    void EndGame(string reason)
    {
        gameRunning = false;

        StopAnyShake();

        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        // Hide gameplay UI on restart screen
        SetGameplayUIVisible(false);

        if (restartButton) restartButton.gameObject.SetActive(true);

        if (finalScoreText)
        {
            finalScoreText.text = "Final Score: " + score;
            finalScoreText.gameObject.SetActive(true);
        }
    }

    void RestartGame()
    {
        gameRunning = false;

        StopAnyShake();

        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        // Reset counters
        placedYellow = 0;
        collectedYellow = 0;
        collectedRed = 0;
        score = 0;

        if (selected) selected.Deselect();
        selected = null;

        foreach (var t in FindObjectsOfType<Treasure>())
        {
            Destroy(t.gameObject);
        }

        if (placer != null)
        {
            placer.ResetPlacementCount();
        }

        if (restartButton) restartButton.gameObject.SetActive(false);
        if (startButton) startButton.gameObject.SetActive(false);

        if (finalScoreText) finalScoreText.gameObject.SetActive(false);

        SetGameplayUIVisible(true);
        SetTimerVisible(false);

        UpdateUI();
    }

    void UpdateUI()
    {
        if (collectButton)
            collectButton.interactable = gameRunning && selected != null;

        int remaining = maxTotalPlacements - TotalPlaced();
        string placeMsg = remaining > 0 ? ("\nPlace " + remaining + " more") : "";

        if (scoreText)
        {
            scoreText.text =
                "Score: " + score + "\n" +
                "Placed: " + TotalPlaced() + "\n" +
                "Collected  Good:" + collectedYellow + "  Mimic:" + collectedRed +
                placeMsg;
        }
    }

    void UpdateTimerUI()
    {
        if (!timerCenterText) return;

        int seconds = Mathf.CeilToInt(timeLeft);
        timerCenterText.text = seconds.ToString();
    }

    void SetTimerVisible(bool on)
    {
        if (timerCenterText) timerCenterText.gameObject.SetActive(on);
    }

    void SetGameplayUIVisible(bool on)
    {
        if (scoreText) scoreText.gameObject.SetActive(on);
        if (collectButton) collectButton.gameObject.SetActive(on);
        SetTimerVisible(on);
    }

    int TotalPlaced() { return placedYellow; }
    int TotalCollected() { return collectedYellow + collectedRed; }

    // ---------------------------
    // Mimic Tap Shake (always visible, angle-independent)
    // ---------------------------
    void StartMimicTapShake(Treasure t)
    {
        if (t == null) return;

        StopAnyShake();

        // IMPORTANT: IdleMotion also writes localPosition each frame.
        // Instead of disabling it, we call StopMotion() so it stops fighting the shake.
        var idle = t.GetComponent<TreasureIdleMotion>();
        if (idle != null) idle.StopMotion();

        shakeTarget = t.transform;
        shakeStartLocalPos = shakeTarget.localPosition;

        currentShakeRoutine = StartCoroutine(MimicTapShakeRoutine());
    }

    IEnumerator MimicTapShakeRoutine()
    {
        float endTime = Time.time + mimicTapShakeSeconds;

        while (shakeTarget != null && Time.time < endTime)
        {
            float s = Mathf.Sin(Time.time * mimicTapShakeSpeed);
            float c = Mathf.Cos(Time.time * mimicTapShakeSpeed * 1.1f);

            float x = s * mimicTapShakeAmount;
            float z = c * mimicTapShakeAmount;

            shakeTarget.localPosition = shakeStartLocalPos + new Vector3(x, 0f, z);
            yield return null;
        }

        if (shakeTarget != null)
            shakeTarget.localPosition = shakeStartLocalPos;

        currentShakeRoutine = null;
        shakeTarget = null;
    }

    void StopAnyShake()
    {
        if (currentShakeRoutine != null)
        {
            StopCoroutine(currentShakeRoutine);
            currentShakeRoutine = null;
        }

        if (shakeTarget != null)
        {
            shakeTarget.localPosition = shakeStartLocalPos;
            shakeTarget = null;
        }
    }
}
