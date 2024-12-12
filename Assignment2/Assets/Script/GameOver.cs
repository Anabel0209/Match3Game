using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

//class that manages the GameOver overlay
public class GameOver : MonoBehaviour
{
    public GameObject gameOverMenu;

    //reference to the TMP that displays the score in the gameover menu
    public TMP_Text myScore;

    //display the gameover overlay and stop the time
    public void GameOverDisplay(int score)
    {
        gameOverMenu.SetActive(true);
        myScore.text = "Score: " + score.ToString();
        Time.timeScale = 0f;
    }

    //method that brings the player to the main menu
    public void ReturnMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }

    //method that reload the level for the player to try again
    public void Retry()
    {
        SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1f;

    }
}
