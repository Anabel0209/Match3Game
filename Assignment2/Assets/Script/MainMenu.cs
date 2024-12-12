using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//class that controls the main menue behaviour
public class MainMenu : MonoBehaviour
{
    //play that sound when starting a level
    public AudioSource clickingSound;

    //starts the level 1
    public void playLevel1()
    {
        clickingSound.Play();
        StartCoroutine(DelaySceenLoad(0.2f, 1));
        Time.timeScale = 1.0f;
    }

    //starts the level 2
    public void playLevel2()
    {
        clickingSound.Play();
        StartCoroutine(DelaySceenLoad(0.2f, 2));
        Time.timeScale = 1.0f;
    }

    //method that quits the game
    public void Quit()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif  
    }

    //coroutine that waits to load the scene to give time for the sound to play
    private IEnumerator DelaySceenLoad(float delay, int scene)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadSceneAsync(scene);
    }

   
}
