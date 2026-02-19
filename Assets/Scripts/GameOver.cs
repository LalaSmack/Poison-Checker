using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEditor;

public class GameOver : MonoBehaviour
{
    [SerializeField] private TMP_Text reasonText;
    [SerializeField] private GameObject gameOverPanel;

    private bool isGameOver;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameOverPanel.SetActive(false);
        isGameOver = false;
    }

    public void TriggerGameOver(string reason="")
    {
        if (isGameOver) return; // Prevent multiple triggers
        
        isGameOver = true;
        if (reasonText != null)
            reasonText.text = reason;
        gameOverPanel.SetActive(true);

        Time.timeScale = 0f; // Pause the game
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f; // Resume the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; // Resume the game

        #if UNITY_EDITOR
            EditorApplication.isPlaying = false; // Stop play mode in the editor
        #else
            Application.Quit(); // Quit the application
        #endif
    }
}
