using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

    public void MenuClick(string strNameBtn)
    {
        SoundManager.ClickPlay();

        //Debug.Log("Clicked: " + strNameBtn);

        switch (strNameBtn)
        {
            case "PlayGame": PlayGame(); break;
            case "Options": Options(); break;
            case "Exit": Exit(); break;
            default:
                Debug.Log("Error menu button click"); break;
        }
    }

    public void PlayGame()
    {
        //Debug.Log("PlayGame");

        //Если мы уже имеем запущенную игру,
        //то все данные уже должны быть загружены в памяти
        //Даже если мы выходили из игры данные подгрузятся благодаря инициализатору в GameController'е
        //Нам остаётся только запустить заново гейм-цикл
        if (GameController.GameState() == GameController.GameStateType.Pause)
        {
            GameController.Resume();
        }
        else
        {
            //В любом другом случае будем считать, что игру надо создавать заново
            GameController.NewGame();
        }

        SceneManager.LoadScene("Game");
    }

    public void Options()
    {
        SceneManager.LoadScene("Options");
    }

    public void Exit()
    {
        SoundManager.ClickPlay();

        StartCoroutine(ExitCoroutine());
    }

    IEnumerator ExitCoroutine()
    {
        //Wait for play audio "click"
        yield return new WaitForSeconds(0.2f);

        // save any game data here
        #if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}
