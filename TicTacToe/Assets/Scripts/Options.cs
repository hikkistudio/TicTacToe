using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Options : MonoBehaviour
{

    [Header("ObjForSelect")]
    public GameObject imgFrameSelect;

    [Header("ObjForSounds")]
    public Button btnToggleSound;
    public Sprite imgSoundOn;
    public Sprite imgSoundOff;


    void Start()
    {
        //Чем играет пользователь. Нолики или крестики
        if (GameController.flagCrossOrZero)
        {
            //Cross
            imgFrameSelect.transform.localPosition = new Vector3(-175, -35, 0);
        }
        else
        {
            //Zero
            imgFrameSelect.transform.localPosition = new Vector3(175, -35, 0);
        }

        //Включён или выключен звук в игре
        if (GameController.flagOnSound)
        {
            btnToggleSound.image.sprite = imgSoundOn;
        }
        else
        {
            btnToggleSound.image.sprite = imgSoundOff;
        }
    }

    //Функция централизованного распределения событий.
    //Создана, чтобы обеспечить одноообразность работы программы на самые разные действия пользователя
    public void OptionsClick(string strNameBtn)
    {
        SoundManager.ClickPlay();

        switch (strNameBtn)
        {
            case "SelectCross": SelectSign(true); break;
            case "SelectZero": SelectSign(false); break;
            case "SoundOnOff": ToggleSound(); break;
            case "OptOk": BackToMenu(); break;
            default:
                Debug.Log("Error options button click"); break;
        }
    }

    //Пользователь выбрал каким знаком он будет играть.
    //Сохраняем выбор пользователя в настройках скрипта-хранилища.
    //Также стираем старую игру и создаём новую.
    public void SelectSign(bool flagCZ)
    {
        GameController.flagCrossOrZero = flagCZ;
        GameController.StopGame(); //Сбрасываем текущую игру

        if (flagCZ)
        {
            //Cross
            imgFrameSelect.transform.localPosition = new Vector3(-175, -35, 0);
        }
        else
        {
            //Zero
            imgFrameSelect.transform.localPosition = new Vector3(175, -35, 0);
        }

        
    }

    //Включаем или отключаем звук во всей программе
    //Данная функция дублируется в главном окне игры
    public void ToggleSound()
    {

        //Звук в игре был. Значит будем его отключать
        if (SoundManager.GetSoundState())
        {
            //Правда нам хотелось бы, чтобы и клацк на самой кнопке отключения успел програться
            StartCoroutine(ToggleSoundCoroutine());
        }
        else
        {
            //Звука в игре не было, включаем его
            SoundManager.SoundOnOff(!SoundManager.GetSoundState());

            btnToggleSound.image.sprite = imgSoundOn;
        }
    }

    //Подождём немного времени, чтобы клацк на кнопке отключения звука успел проиграться
    IEnumerator ToggleSoundCoroutine()
    {
        //Wait for button play audio "click"
        yield return new WaitForSeconds(0.2f);

        SoundManager.SoundOnOff(!SoundManager.GetSoundState());

        btnToggleSound.image.sprite = imgSoundOff;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
