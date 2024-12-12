using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

//What controls the behaviours of our win overlay
public class Win : MonoBehaviour
{

    public GameObject winMenu;

    //score that is displayed
    public TMP_Text myScore;

    //stars judging the score of the player
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;
    public GameObject emptyStar2;
    public GameObject emptyStar3;

    public AudioSource winSound;
   

    //method called when the player wins the game
    public void WinDisplay(int score)
    {
        winSound.Play();
        winMenu.SetActive(true);

        //uodate the TMP of the score in the UI
        myScore.text = "Score: " + score.ToString();

        //if the score is lower than 220 (double match 15 times)
        if(score < 220)
        {
            displayOneStar();

        }
        //the score is between 220 and 400 (triple match 8 times)
        else if(score < 400)
        {
            displayTwoStar();
        }
        //more than 400
        else
        {
            displayThreeStar();
        }
    }

    //method called to return to the main menu
    public void ReturnMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }

    //method that displays one star
    public void displayOneStar()
    {
        star1.SetActive(true);
        emptyStar2.SetActive(true);
        emptyStar3.SetActive(true);
    }

    //method that displays two stars
    public void displayTwoStar()
    {
        star1.SetActive(true);
        star2.SetActive(true);
        emptyStar3.SetActive(true);
    }

    //method that displays three stars
    public void displayThreeStar()
    {
        star1.SetActive(true);
        star2.SetActive(true);
        star3.SetActive(true);
    }
}
