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

    [Header("Timer UI")]
    public TextMeshProUGUI timerCenterText;

    [Header("Final Score UI")]
    public TextMeshProUGUI finalScoreText;

    [Header("Rules")]
    public int maxTotalPlacements = 10;
    public int gameDurationSeconds = 60;

    [Header("References")]
    public ARTapToPlaceTreasure placer;

    int placedTotal;
    int collectedGood, collectedMimic;
    int score;

    Treasure selected;

    bool gameRunning = false;
    float timeLeft = 0f;
    Coroutine timerRoutine;

    void Start()
    {
        if (collectButton) collectButton.onClick.AddListener(CollectSelected);
        if (startButton) startButton.onClick.AddListener(StartGame);
        if (restartButton) restartButton.onClick.AddListener(RestartGame);

        if (collectButton) collectButton.interactable = false;
        if (startButton) startButton.gameObject.SetActive(false);
        if (restartButton) restartButton.gameObject.SetActive(false);
        if (finalScoreText) finalScoreText.gameObject.SetActive(false);

        SetTimerVisible(false);
        UpdateUI();
    }

    public void RecordPlaced(bool isMimic)
    {
        placedTotal++;

        if (placedTotal >= maxTotalPlacements)
        {
            if (startButton != null)
                startButton.gameObject.SetActive(true);
        }

        UpdateUI();
    }

    public void RecordCollected(bool isMimic)
    {
        if (isMimic) collectedMimic++;
        else collectedGood++;

        UpdateUI();
    }

    public void SetSelected(Treasure t)
    {
        if (!gameRunning) return;
        if (t == null) return;

        if (selected != null)
            selected.Deselect();

        selected = t;
        selected.Select();

        // 🔥 NEW: If mimic → shake on select
        if (selected.isMimic)
        {
            StartCoroutine(ShakeTreasure(selected.transform, 0.4f, 0.05f));
        }

        if (collectButton)
            collectButton.interactable = true;
    }

    // 🔥 UPDATED: Use coroutine instead of instant destroy
    public void CollectSelected()
    {
        if (!gameRunning) return;
        if (!selected) return;

        StartCoroutine(HandleTreasureCollection(selected));

        selected = null;

        if (collectButton)
            collectButton.interactable = false;
    }

    // 🔥 NEW: Handles mimic behavior
    IEnumerator HandleTreasureCollection(Treasure t)
    {
        bool isMimic = t.isMimic;

        if (isMimic)
        {
            Debug.Log("MIMIC ❌");

            // 🔥 ONLY TURN RED ON COLLECT
            t.SetRed();

            yield return new WaitForSeconds(0.5f);

            score -= 1;
        }
        else
        {
            Debug.Log("GOOD ✅");

            // 🔥 TURN GREEN
            t.SetGreen();

            yield return new WaitForSeconds(0.3f);

            score += 1;
        }

        RecordCollected(isMimic);

        Destroy(t.gameObject);

        UpdateUI();
        // 🔥 NEW: End game early if all treasures collected
        if ((collectedGood + collectedMimic) >= maxTotalPlacements)
        {
            EndGame();
        }
    }

    // 🔥 NEW: Shake effect
    IEnumerator ShakeTreasure(Transform target, float duration, float magnitude)
    {
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude;

            target.localPosition = originalPos + new Vector3(x, 0, z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPos;
    }

    public void StartGame()
    {
        gameRunning = true;

        if (startButton) startButton.gameObject.SetActive(false);
        if (restartButton) restartButton.gameObject.SetActive(false);

        timeLeft = gameDurationSeconds;

        SetTimerVisible(true);
        UpdateTimerUI();

        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        timerRoutine = StartCoroutine(TimerCountdown());
    }

    IEnumerator TimerCountdown()
    {
        while (gameRunning && timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimerUI();
            yield return null;
        }

        EndGame();
    }

    void EndGame()
    {
        gameRunning = false;

        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        if (restartButton)
            restartButton.gameObject.SetActive(true);

        if (finalScoreText)
        {
            finalScoreText.text = "Final Score: " + score;
            finalScoreText.gameObject.SetActive(true);
        }
    }

    void RestartGame()
    {
        gameRunning = false;

        score = 0;
        placedTotal = 0;
        collectedGood = 0;
        collectedMimic = 0;

        foreach (var t in FindObjectsOfType<Treasure>())
            Destroy(t.gameObject);

        if (placer != null)
            placer.ResetPlacementCount();

        if (startButton) startButton.gameObject.SetActive(false);
        if (restartButton) restartButton.gameObject.SetActive(false);
        if (finalScoreText) finalScoreText.gameObject.SetActive(false);

        SetTimerVisible(false);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText)
        {
            scoreText.text =
                "Score: " + score + "\n" +
                "Placed: " + placedTotal + "\n" +
                "Collected  Good:" + collectedGood + "  Mimic:" + collectedMimic;
        }
    }

    void UpdateTimerUI()
    {
        if (!timerCenterText) return;
        timerCenterText.text = Mathf.CeilToInt(timeLeft).ToString();
    }

    void SetTimerVisible(bool on)
    {
        if (timerCenterText) timerCenterText.gameObject.SetActive(on);
    }
}