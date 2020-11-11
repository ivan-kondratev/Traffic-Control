using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    private const string GameSceneName = "GameScene";
    private const string MainMenuSceneName = "MainMenu";
    private const string PausePanelTag = "PausePanel";
    private const string LosingControllerTag = "LosingController";
    private const string GamePanelTag = "GamePanel";

    private GameObject pausePanel;
    private GameObject losingPanel;
    private GameObject gamePanel;

    public void PlayPressed()
    {
        PlayerResources.ResetResources();
        SceneManager.LoadScene(GameSceneName);
    }
    
    public void ExitPressed()
    {
        Application.Quit();
    }

    public void GoToMenuPressed()
    {
        Time.timeScale = 1f;
        try
        {
            pausePanel = GameObject.FindWithTag(PausePanelTag);
            pausePanel.SetActive(false);
        }
        catch
        {

        }
        PauseController.gameIsPaused = false;
        SceneManager.LoadScene(MainMenuSceneName);
    }

    public void PausePressed()
    {
        PauseController.gameIsPaused = true;
    }

    public void ResumePressed()
    {
        PauseController.gameIsPaused = false;
    }

    public void RestartPressed()
    {
        Time.timeScale = 1f;
        losingPanel = GameObject.FindWithTag(LosingControllerTag);
        losingPanel.SetActive(false);
        PlayerResources.ResetResources();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
