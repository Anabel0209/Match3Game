using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//class that controls the pause overlay behaviour
public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject board;
    Board myBoard;

    //keep track if the game is paused
    private bool isPaused = false;

    //the main music of the game
    public AudioSource mainMusic;
   
    void Awake()
    {
        myBoard = board.GetComponent<Board>();
    }

   
    void Update()
    {
        //trigger the display of the menu uppon pressing escape
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    //pause the game and show the overlay and stop the time
    public void Pause()
    {
        pauseMenu.SetActive(true);
        myBoard.HideCandy(true);
        mainMusic.Pause();
        Time.timeScale = 0;
        isPaused = true;
    }

    //resume the game and hide the overlay
    public void Resume()
    {
        pauseMenu.SetActive(false);
        board.GetComponent<Board>().HideCandy(false);
        mainMusic.Play();
        Time.timeScale = 1;
        isPaused = false;
    }

    //method that brings back the player in the main menu
    public void ReturnMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
