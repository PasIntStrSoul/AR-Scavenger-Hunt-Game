using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TreasureManager : MonoBehaviour
{
    public enum GameMode
    {
        Placement,
        Picking,
        Finished
    }

    [Header("UI")]
    public Button collectButton;
    public TextMeshProUGUI scoreText;

    [Header("Start / Restart UI")]
    public Button startButton;
    public Button restartButton;

    [Header("Timer UI")]
    public TextMeshProUGUI timerCenterText;

    [Header("Final Score UI")]
    public TextMeshProUGUI finalScoreText;

    [Header("Rules")]
    public int maxTotalPlacements = 10;
    public int gameDurationSeconds = 60;

    [Header("References")]
    public ARTapToPlaceTreasure placer;

    private int placedTotal = 0;
    private int collectedGood = 0;
    private int collectedMimic = 0;
    private int score = 0;

    private Treasure selected;

    private GameMode currentMode = GameMode.Placement;

    private float timeLeft = 0f;
    private Coroutine timerRoutine;

    public GameMode CurrentMode
    {
        get { return currentMode; }
    }

    void Start()
    {
        if (collectButton != null)
            collectButton.onClick.AddListener(CollectSelected);

        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        EnterPlacementMode();
    }

    // ==========================================
    // MODE 1: PLACEMENT
    // ==========================================

    void EnterPlacementMode()
    {
        currentMode = GameMode.Placement;

        if (placer != null)
            placer.EnablePlacement();

        if (collectButton != null)
            collectButton.interactable = false;

        if (startButton != null)
            startButton.gameObject.SetActive(
                placedTotal >= maxTotalPlacements
            );

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        if (finalScoreText != null)
            finalScoreText.gameObject.SetActive(false);

        SetTimerVisible(false);

        UpdateUI();

        Debug.Log("GAME MODE: PLACEMENT");
    }

    public void RecordPlaced(bool isMimic)
    {
        if (currentMode != GameMode.Placement)
            return;

        placedTotal++;

        if (placedTotal >= maxTotalPlacements)
        {
            if (startButton != null)
                startButton.gameObject.SetActive(true);

            Debug.Log("Placement complete. Ready for Picking Mode.");
        }

        UpdateUI();
    }

    // ==========================================
    // MODE 2: PICKING
    // ==========================================

    public void StartGame()
    {
        if (currentMode != GameMode.Placement)
            return;

        if (placedTotal < maxTotalPlacements)
        {
            Debug.LogWarning(
                "Place all objects before starting Picking Mode."
            );

            return;
        }

        currentMode = GameMode.Picking;

        // Stop further object placement.
        if (placer != null)
            placer.DisablePlacement();

        if (startButton != null)
            startButton.gameObject.SetActive(false);

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        if (collectButton != null)
            collectButton.interactable = false;

        timeLeft = gameDurationSeconds;

        SetTimerVisible(true);
        UpdateTimerUI();

        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        timerRoutine = StartCoroutine(TimerCountdown());

        Debug.Log("GAME MODE: PICKING");
    }

    public void SetSelected(Treasure t)
    {
        if (currentMode != GameMode.Picking)
            return;

        if (t == null)
            return;

        if (selected != null)
            selected.Deselect();

        selected = t;
        selected.Select();

        if (selected.isMimic)
        {
            StartCoroutine(
                ShakeTreasure(selected.transform, 0.4f, 0.05f)
            );
        }

        if (collectButton != null)
            collectButton.interactable = true;
    }

    public void CollectSelected()
    {
        if (currentMode != GameMode.Picking)
            return;

        if (selected == null)
            return;

        Treasure treasureToCollect = selected;

        selected = null;

        if (collectButton != null)
            collectButton.interactable = false;

        StartCoroutine(
            HandleTreasureCollection(treasureToCollect)
        );
    }

    IEnumerator HandleTreasureCollection(Treasure t)
    {
        if (t == null)
            yield break;

        bool isMimic = t.isMimic;

        if (isMimic)
        {
            Debug.Log("MIMIC");

            t.SetRed();

            yield return new WaitForSeconds(0.5f);

            if (currentMode != GameMode.Picking)
                yield break;

            score -= 1;
        }
        else
        {
            Debug.Log("GOOD TREASURE");

            t.SetGreen();

            yield return new WaitForSeconds(0.3f);

            if (currentMode != GameMode.Picking)
                yield break;

            score += 1;
        }

        RecordCollected(isMimic);

        if (t != null)
            Destroy(t.gameObject);

        UpdateUI();

        if ((collectedGood + collectedMimic) >= maxTotalPlacements)
        {
            EndGame();
        }
    }

    public void RecordCollected(bool isMimic)
    {
        if (isMimic)
            collectedMimic++;
        else
            collectedGood++;

        UpdateUI();
    }

    // ==========================================
    // MIMIC SHAKE EFFECT
    // ==========================================

    IEnumerator ShakeTreasure(
        Transform target,
        float duration,
        float magnitude)
    {
        if (target == null)
            yield break;

        Vector3 originalPos = target.localPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (target == null)
                yield break;

            float x = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude;

            target.localPosition =
                originalPos + new Vector3(x, 0, z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        if (target != null)
            target.localPosition = originalPos;
    }

    // ==========================================
    // TIMER
    // ==========================================

    IEnumerator TimerCountdown()
    {
        while (
            currentMode == GameMode.Picking &&
            timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;

            UpdateTimerUI();

            yield return null;
        }

        if (currentMode == GameMode.Picking)
            EndGame();
    }

    // ==========================================
    // GAME END
    // ==========================================

    void EndGame()
    {
        if (currentMode == GameMode.Finished)
            return;

        currentMode = GameMode.Finished;

        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        if (placer != null)
            placer.DisablePlacement();

        if (selected != null)
        {
            selected.Deselect();
            selected = null;
        }

        if (collectButton != null)
            collectButton.interactable = false;

        if (restartButton != null)
            restartButton.gameObject.SetActive(true);

        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + score;
            finalScoreText.gameObject.SetActive(true);
        }

        Debug.Log("GAME MODE: FINISHED");
    }

    // ==========================================
    // RESTART
    // ==========================================

    void RestartGame()
    {
        currentMode = GameMode.Finished;

        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        StopAllCoroutines();

        if (selected != null)
        {
            selected.Deselect();
            selected = null;
        }

        foreach (Treasure t in FindObjectsOfType<Treasure>())
        {
            Destroy(t.gameObject);
        }

        score = 0;
        placedTotal = 0;
        collectedGood = 0;
        collectedMimic = 0;

        timeLeft = 0f;

        if (placer != null)
            placer.ResetPlacementCount();

        EnterPlacementMode();

        Debug.Log("Game restarted.");
    }

    // ==========================================
    // UI
    // ==========================================

    void UpdateUI()
    {
        if (scoreText == null)
            return;

        string modeText = "";

        switch (currentMode)
        {
            case GameMode.Placement:
                modeText = "PLACEMENT MODE";
                break;

            case GameMode.Picking:
                modeText = "PICKING MODE";
                break;

            case GameMode.Finished:
                modeText = "GAME FINISHED";
                break;
        }

        scoreText.text =
            modeText + "\n" +
            "Score: " + score + "\n" +
            "Placed: " + placedTotal + "/" +
            maxTotalPlacements + "\n" +
            "Collected Good: " + collectedGood + "\n" +
            "Mimic: " + collectedMimic;
    }

    void UpdateTimerUI()
    {
        if (timerCenterText == null)
            return;

        timerCenterText.text =
            Mathf.CeilToInt(Mathf.Max(0f, timeLeft)).ToString();
    }

    void SetTimerVisible(bool visible)
    {
        if (timerCenterText != null)
            timerCenterText.gameObject.SetActive(visible);
    }
}