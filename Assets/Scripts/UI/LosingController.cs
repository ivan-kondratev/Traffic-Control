using UnityEngine;
using UnityEngine.UI;

public class LosingController : MonoBehaviour
{
    public GameObject losingPanel;
    public GameObject gamePanel;

    private Text losingText;

    public void ActivateLosingPanel()
    {
        Time.timeScale = 0f;
        losingPanel.SetActive(true);
        gamePanel.SetActive(false);
        losingText = losingPanel.GetComponentInChildren<Text>();
        losingText.text = "you lose!\nyour score: " + PlayerResources.score;
    }

    public bool PlayerLose()
    {
        return PlayerResources.health == 0;
    }
}
