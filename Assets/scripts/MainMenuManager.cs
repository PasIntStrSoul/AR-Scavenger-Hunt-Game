using UnityEngine;
using UnityEngine.SceneManagement; // ✅ REQUIRED for restart / main menu

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject gameplayUI;
    public GameObject optionsPanel;
    public GameObject scanPromptPanel;

    void Start()
    {
        if (PlayerPrefs.GetInt("SkipMainMenu", 0) == 1)
        {
            PlayerPrefs.SetInt("SkipMainMenu", 0);
            PlayerPrefs.Save();

            if (mainMenuUI != null)
                mainMenuUI.SetActive(false);

            if (gameplayUI != null)
                gameplayUI.SetActive(true);

            if (scanPromptPanel != null)
                scanPromptPanel.SetActive(true);

            if (optionsPanel != null)
                optionsPanel.SetActive(false);
        }
        else
        {
            if (mainMenuUI != null)
                mainMenuUI.SetActive(true);

            if (gameplayUI != null)
                gameplayUI.SetActive(false);

            if (optionsPanel != null)
                optionsPanel.SetActive(false);

            if (scanPromptPanel != null)
                scanPromptPanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        if (scanPromptPanel != null)
            scanPromptPanel.SetActive(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void OpenOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    // ✅ NEW FUNCTION (ONLY ADDITION)
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}