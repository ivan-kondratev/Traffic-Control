using UnityEngine;

public class PauseController : MonoBehaviour
{
    public static bool gameIsPaused;

    public GameObject pausePanel;
    public GameObject pauseButton;

    void Update()
    {
        if (gameIsPaused)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    private void Pause()
    {
        pausePanel.SetActive(true);
        pauseButton.SetActive(false);
        Time.timeScale = 0f;
    }

    private void Resume()
    {
        pausePanel.SetActive(false);
        pauseButton.SetActive(true);
        Time.timeScale = 1f;
    }
}
