using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ARStudyManager : MonoBehaviour
{
    [Header("Study UI")]
    public GameObject studyWelcomePanel;
    public GameObject studyInstructionsText;
    public GameObject studyWelcomeTitle;
    public GameObject studyCountdownText;

    [Header("Buttons")]
    public Button beginStudyButton;

    [Header("Research Controls")]
    public GameObject researchControlsPanel;

    [Header("Countdown Settings")]
    public int countdownSeconds = 10;

    private TMP_Text countdownLabel;
    private bool studyStarted = false;
    private bool countdownRunning = false;

    private void Start()
    {
        countdownLabel = studyCountdownText.GetComponent<TMP_Text>();

        studyWelcomePanel.SetActive(true);
        studyWelcomeTitle.SetActive(true);
        studyInstructionsText.SetActive(true);
        studyCountdownText.SetActive(false);

        // Hide research controls until the study begins.
        researchControlsPanel.SetActive(false);

        beginStudyButton.onClick.AddListener(BeginStudy);
    }

    public void BeginStudy()
    {
        if (studyStarted || countdownRunning)
            return;

        StartCoroutine(StudyCountdown());
    }

    private IEnumerator StudyCountdown()
    {
        countdownRunning = true;

        studyWelcomeTitle.SetActive(false);
        studyInstructionsText.SetActive(false);
        beginStudyButton.gameObject.SetActive(false);

        studyCountdownText.SetActive(true);

        for (int seconds = countdownSeconds; seconds > 0; seconds--)
        {
            countdownLabel.text =
                "Your study begins in\n" + seconds + " seconds";

            yield return new WaitForSecondsRealtime(1f);
        }

        countdownLabel.text = "START!";

        yield return new WaitForSecondsRealtime(1f);

        studyWelcomePanel.SetActive(false);
        researchControlsPanel.SetActive(true);

        studyStarted = true;
        countdownRunning = false;

        Debug.Log("AR Visual Perception Study Started");
    }
}