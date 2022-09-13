using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public GameObject pausePanel;
    public GameObject resetPanel;
    public GameObject pauseButton;
    private Board board;
    public bool isGamePause = false;

    void Start()
    {
        board = FindObjectOfType<Board>();
    }


    void Update()
    {
        scoreText.text = "Score: " + board.score;

    }
    public void PauseGame()
    {
        pausePanel.SetActive(true);
        pauseButton.SetActive(false);
        board.currentState = GameState.wait;
        isGamePause = true;

    }
    public void ContinueGame()
    {
        pausePanel.SetActive(false);
        pauseButton.SetActive(true);
        isGamePause = false;
        board.currentState = GameState.move;

    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
    }

    //When there is no block to blow up left, display restart button
    public void DeadLock()
    {
        resetPanel.SetActive(true);
        pauseButton.SetActive(false);
        board.currentState = GameState.wait;
        isGamePause = true;
    }
}
