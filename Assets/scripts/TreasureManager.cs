using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

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

    [Header("Scan Prompt UI")]
    public GameObject scanPromptPanel;

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

        if (collectButton) collectButton.gameObject.SetActive(false);
        if (startButton) startButton.gameObject.SetActive(false);
        if (restartButton) restartButton.gameObject.SetActive(false);
        if (finalScoreText) finalScoreText.gameObject.SetActive(false);

        // Changed: prompt should NOT appear before main menu start
        if (scanPromptPanel) scanPromptPanel.SetActive(false);

        SetTimerVisible(false);
        UpdateUI();
    }

    public void RecordPlaced(bool isMimic)
    {
        placedTotal++;

        if (placedTotal >= maxTotalPlacements)
        {
            if (startButton)
                startButton.gameObject.SetActive(true);

            if (scanPromptPanel)
                scanPromptPanel.SetActive(false);
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

        if (selected.isMimic)
        {
            StartCoroutine(ShakeTreasure(selected.transform, 0.4f, 0.05f));
        }

        if (collectButton)
            collectButton.interactable = true;
    }

    public void CollectSelected()
    {
        if (!gameRunning) return;
        if (!selected) return;

        StartCoroutine(HandleTreasureCollection(selected));

        selected = null;

        if (collectButton)
            collectButton.interactable = false;
    }

    IEnumerator HandleTreasureCollection(Treasure t)
    {
        bool isMimic = t.isMimic;

        if (isMimic)
        {
            t.SetRed();
            yield return new WaitForSeconds(0.5f);
            score -= 1;
        }
        else
        {
            t.SetGreen();
            yield return new WaitForSeconds(0.3f);
            score += 1;
        }

        RecordCollected(isMimic);

        Destroy(t.gameObject);

        UpdateUI();

        if ((collectedGood + collectedMimic) >= maxTotalPlacements)
        {
            EndGame();
        }
    }

    IEnumerator ShakeTreasure(Transform target, float duration, float magnitude)
    {
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude;

            target.localPosition = originalPos + new Vector3(x, 0f, z);

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

        if (scoreText) scoreText.gameObject.SetActive(true);
        if (collectButton)
        {
            collectButton.gameObject.SetActive(true);
            collectButton.interactable = false;
        }

        if (scanPromptPanel)
            scanPromptPanel.SetActive(false);

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

        if (collectButton)
            collectButton.gameObject.SetActive(false);

        if (finalScoreText)
        {
            finalScoreText.text = "Final Score: " + score;
            finalScoreText.gameObject.SetActive(true);
        }

        if (scanPromptPanel)
            scanPromptPanel.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        if (timerCenterText)
            timerCenterText.gameObject.SetActive(on);
    }
}