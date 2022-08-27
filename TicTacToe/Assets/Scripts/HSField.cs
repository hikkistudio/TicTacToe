using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class HSField : MonoBehaviour
{
    //Создали ли мы панельку с сообщением пользователю?
    private static bool flInitUserMsg = false;

    [Header("Place where creating prefab")]
    public GameObject objGameField = null;

    [Header("Creating Cross and Zero")]
    public GameObject objCross;
    public GameObject objZero;

    [Header("Pop-up window")]
    public GameObject objWndEndGame;

    public Sprite imgWin;
    public Sprite imgDraw;
    public Sprite imgLose;

    [Header("Sound")]
    public Button btnToggleSound;
    public Sprite imgSoundOn;
    public Sprite imgSoundOff;

    [Header("Message for User")]
    //Сообщение пользователю над игровым полем
    public GameObject objMsgForUser;
    public Sprite imgCross, imgZero;
    private GameObject goMsgForUser;
    private Text TextUserMsg, TextCompMsg;
    private Image SpriteUserMsg, SpriteCompMsg;

    //Array with g.o. for drawing signs in cells 
    private static GameObject[] goCells = new GameObject[9];

    //Окно с результатом игры
    private static GameObject goWndEndGame;

    //Когда юзер или компьютер ходят
    //Рисуем символ в соответствующей ячейки игрового поля
    public static int nCurrentCreateUIObj = 0;

    /**************************************************************************************************/
    /*Сеттеры и геттеры*/
    /**************************************************************************************************/

    public bool flagInitUserMsg
    {
        get
        {
            return flInitUserMsg;
        }

        set
        {
            flInitUserMsg = value;
        }
    }

    /**************************************************************************************************/

    private void Start()
    {

        if (GameController.flagOnSound)
        {
            btnToggleSound.image.sprite = imgSoundOn;
        }
        else
        {
            btnToggleSound.image.sprite = imgSoundOff;
        }

        //Находим этот скрипт в главном скрипте программы и говорим, что мы появились и активировались
        GameController.InitUIGameField();

        //Debug.Log("Start HSField.cs");
    }

    //Функция централизованного распределения событий.
    //Создана, чтобы обеспечить одноообразность работы программы на самые разные действия пользователя
    public void HSFieldClick(string strNameBtn)
    {
        SoundManager.ClickPlay();

        //Debug.Log("Clicked: " + strNameBtn);

        switch (strNameBtn)
        {
            case "1":
            case "2":
            case "3":
            case "4":
            case "5":
            case "6":
            case "7":
            case "8":
            case "9": GameController.ClickCell(Convert.ToInt32(strNameBtn) - 1); break; // 1..9 -> 0..8
            case "ToggleSound": ToggleSound(); break;
            case "BackToMenu": BackToMenu(); break;
            case "ReloadGame": ReloadGame(); break;
            case "EndGame": EndGame(); break;
            default:
                Debug.Log("Error game field button click"); break;
        }

    }

    //Вызывая меню мы закрываем сцену с игрой, поэтому следует сохранить все переменные
    //После выхода в меню пользователь может вернуться к игре гораздо позже, к примеру, неделю спустя
    public void BackToMenu()
    {
        //Debug.Log("BackToMenu");

        GameController.Pause();

        //У нас есть корутины с эффектами и сообщением для пользователя, прекращаем их
        StopAllCoroutines();

        SceneManager.LoadScene("Menu");
    }

    //Не меняем сцену, просто приводим её в начальное состояние
    public void ReloadGame()
    {
        //Debug.Log("ReloadGame");

        //Убираем окно с результатом игры
        Destroy(goWndEndGame);

        //Очищаем визуальное поле от прошлых знаков
        ClearGameField();

        //Начинаем новую игру
        GameController.ReloadGame();
    }

    //Вызывается из окна с результатами прошлой игры
    //Поскольку игра была уже остановлена, от нас требуется просто загрузить сцену меню без сохранения
    //Следующим логическим шагом должно стать создание новой игры из под меню
    public void EndGame()
    {
        //Debug.Log("EndGame");

        //У нас есть корутины с эффектами и сообщением для пользователя, прекращаем их
        StopAllCoroutines();

        SceneManager.LoadScene("Menu");
    }



    /**************************************************************************************************/
    /*Работа с игровым полем программы*/
    /**************************************************************************************************/

    //Приводим поле в первоначальный вид
    public void ClearGameField()
    {
        //Debug.Log("ClearGameField");

        for (int i = 0; i < 9; i++)
        {
            //Debug.Log("ClearGameField: goCells[" + i + "] = " + goCells[i]);
            Destroy(goCells[i]);
            //Debug.Log("ClearGameField: goCells[" + i + "] = " + goCells[i]);
        }
    }

    //Создаём соответствующий знак на поле
    public void DrawSignInCell(int nCurrentCreateUIObj, bool flCrossOrZero)
    {
        //Debug.Log("DrawSignInCell(" + nCurrentCreateUIObj + ", " + flCrossOrZero + ")");

        Vector3 v3CellSign;

        switch (nCurrentCreateUIObj)
        {
            case 0: v3CellSign = new Vector3(-205, 165, 0); break;
            case 1: v3CellSign = new Vector3(0, 165, 0); break;
            case 2: v3CellSign = new Vector3(205, 165, 0); break;
            case 3: v3CellSign = new Vector3(-205, 0, 0); break;
            case 4: v3CellSign = new Vector3(0, 0, 0); break;
            case 5: v3CellSign = new Vector3(205, 0, 0); break;
            case 6: v3CellSign = new Vector3(-205, -165, 0); break;
            case 7: v3CellSign = new Vector3(0, -165, 0); break;
            case 8: v3CellSign = new Vector3(205, -165, 0); break;
            default:
                v3CellSign = new Vector3(-500, 0, 0); break;
        }

        //Debug.Log("1");

        if (flCrossOrZero)
        {
            //Cross
            goCells[nCurrentCreateUIObj] = Instantiate(objCross);
            //Debug.Log("DrawSignInCell: goCells[" + nCurrentCreateUIObj + "] = " + goCells[nCurrentCreateUIObj]);
        }
        else
        {
            //Zero
            goCells[nCurrentCreateUIObj] = Instantiate(objZero);
            //Debug.Log("DrawSignInCell: goCells[" + nCurrentCreateUIObj + "] = " + goCells[nCurrentCreateUIObj]);
        }

        //if (objGameField == null) //Debug.Log("objGameField Null!!!");
        //if (goCells == null) //Debug.Log("goCells Null!!!");


        goCells[nCurrentCreateUIObj].transform.SetParent(objGameField.transform, false);


        //Debug.Log("4");

        goCells[nCurrentCreateUIObj].transform.localPosition = v3CellSign;

        //Debug.Log("DrawSignInCell: end");
    }

    //Информационная надпись над игровым полем

    //Собираем все компоненты надписи и делаем их невидимыми
    public void InitMessageForUser()
    {
        //Debug.Log("ClearStartMessage");

        goMsgForUser = Instantiate(objMsgForUser);
        goMsgForUser.transform.SetParent(objGameField.transform, false);
        //Vector3 v3InitPos = new Vector3(0, 344, 0);
        //goMsgForUser.transform.localPosition = v3CellSign;

        /*
        goUserMsg = goMsgForUser.transform.Find("UserMsg").gameObject;
        TextUserMsg = goUserMsg.GetComponent<Text>().text;
        goImgUserMsg = goUserMsg.transform.Find("UserImg").gameObject;
        goImgUserMsg.GetComponent<Image>().sprite = (flagCrossOrZero) ? cross : zero;
        goCompMsg = goMsgForUser.transform.Find("CompMsg").gameObject;
        TextCompMsg = goCompMsg.GetComponent<Text>().text;
        goImgCompMsg = goCompMsg.transform.Find("CompImg").gameObject;
        goImgCompMsg.GetComponent<Image>().sprite = (flagCrossOrZero) ? zero : cross;
        */

        //Временная переменная для поиска компонентов
        GameObject go_tmp_obj;

        //Текст "User:"
        go_tmp_obj = goMsgForUser.transform.Find("UserMsg").gameObject;
        TextUserMsg = go_tmp_obj.GetComponent<Text>();
        TextUserMsg.text = "";
        //Картинка следующая за текстом "User:"
        SpriteUserMsg = go_tmp_obj.transform.Find("UserImg").gameObject.GetComponent<Image>();
        SpriteUserMsg.sprite = (GameController.flagCrossOrZero) ? imgCross : imgZero;

        //Текст "Comp:"
        go_tmp_obj = goMsgForUser.transform.Find("CompMsg").gameObject;
        TextCompMsg = go_tmp_obj.GetComponent<Text>();
        TextCompMsg.text = "";
        //Картинка следующая за текстом "Comp:"
        SpriteCompMsg = go_tmp_obj.transform.Find("CompImg").GetComponent<Image>();
        SpriteCompMsg.sprite = (GameController.flagCrossOrZero) ? imgZero : imgCross;

        //Создали панель с сооющениями, сохраняем состояние
        flInitUserMsg = true;
    }

    //Текст "User:" и текст "Comp:"
    public void WriteMsgTxt(string strMsgUser = "", string strMsgComp = "")
    {
        //Debug.Log("WriteMsgTxt");
        //Debug.Log("strMsgUser = " + strMsgUser);
        //Debug.Log("strMsComp = " + strMsgComp);

        //Обновляем весь текст в полях MsgUserType.Init
        TextUserMsg.text = strMsgUser;
        TextCompMsg.text = strMsgComp;
    }

    //Картинки к текстовому сообщению
    public void ShowMsgImg()
    {
        //Debug.Log("ShowMsgImg");

        //Показываем картинки
        SpriteUserMsg.enabled = true;
        SpriteCompMsg.enabled = true;
    }

    //Удалить панель с сообщением над игровым полем 
    public void DeleteStartMessage()
    {
        //Debug.Log("DeleteStartMessage");

        Destroy(goMsgForUser);

        flInitUserMsg = false;
    }

    /**************************************************************************************************/
    /*Sound On/Off*/
    /**************************************************************************************************/

    //Включаем или отключаем звук во всей программе
    //Данная функция дублируется в окне настроек
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

    /**************************************************************************************************/
    /*Всплывающее окошко с результатом игры*/
    /**************************************************************************************************/

    //Создаём окно с результатом и запоминаем его в глобальную переменную, чтобы потом при случае уничтожить
    public void WndEndGame(GameController.EndGameType ResEndGame)
    {
        //Debug.Log("WndEndGame");

        StartCoroutine(EndGameWndCoroutine(ResEndGame));

    }


    IEnumerator EndGameWndCoroutine(GameController.EndGameType ResEndGame)
    {

        //Немного придержим окно буквально на чуть-чуть,
        //чтобы программа успела закончить отрисовку последнего символа компа
        yield return new WaitForSeconds(0.5f);

        goWndEndGame = Instantiate(objWndEndGame);
        goWndEndGame.transform.SetParent(objGameField.transform, false);
        Vector3 v3InitPos = Vector3.zero;
        goWndEndGame.transform.localPosition = v3InitPos;

        GameObject go_obj = goWndEndGame.transform.Find("EndGameText").gameObject;

        if (ResEndGame == GameController.EndGameType.Win)
        {
            go_obj.GetComponent<Image>().sprite = imgWin;
        }
        else if (ResEndGame == GameController.EndGameType.Draw)
        {
            go_obj.GetComponent<Image>().sprite = imgDraw;
        }
        else
        {
            //Lose game
            go_obj.GetComponent<Image>().sprite = imgLose;
        }

        //Соответствующий звуковой эффект
        SoundManager.EndGamePlay(ResEndGame);

        //Эффект появления окна 
        StartCoroutine(ShowWndCoroutine(goWndEndGame, 0, -90));

    }

    //Визуальный эффект появления окна
    IEnumerator ShowWndCoroutine(GameObject goWnd, float fXTarget = 0f, float fYTarget = 0f, float fSpeed = 3.0f, string strEffect = "Random")
    {
        // Move our position a step closer to the target.
        float fStep = fSpeed * 1000 * Time.deltaTime; // calculate distance to move
        Vector3 v3Target = Vector3.zero;

        int variant_x_or_y = 0;
        int n_x, n_y;

        if (strEffect == "Random")
        {
            //float randomNumber = UnityEngine.Random.Range(1, 9);
            //Debug.Log(randomNumber);
            variant_x_or_y = Convert.ToInt32(UnityEngine.Random.Range(1, 9));
            //Debug.Log(variant_x_or_y);
            //variant_x_or_y = 5;
        }
        else if (strEffect == "FallAbove")
        {
            variant_x_or_y = 5;
        }

        //
        switch (variant_x_or_y)
        {
            case 1: n_x = 0; n_y = 0; break;
            case 2: n_x = -1000; n_y = 0; break;
            case 3: n_x = 0; n_y = -1000; break;
            case 4: n_x = 1000; n_y = 0; break;
            case 5: n_x = 0; n_y = 1000; break;
            case 6: n_x = -1000; n_y = -1000; break;
            case 7: n_x = -1000; n_y = 1000; break;
            case 8: n_x = 1000; n_y = -1000; break;
            case 9: n_x = 1000; n_y = 1000; break;
            default:
                n_x = 0; n_y = 0; break;
        }


        goWnd.transform.localPosition = new Vector3(n_x, n_y, 0);
        v3Target.x = fXTarget;
        v3Target.y = fYTarget;

        //
        while (Vector3.Distance(goWnd.transform.localPosition, v3Target) > 0.001f)
        {

            //Debug.Log("pos = " + goWnd.transform.localPosition + "; target = " + v3Target);
            goWnd.transform.localPosition = Vector3.MoveTowards(goWnd.transform.localPosition, v3Target, fStep);
            yield return null;
        }

    }

}

