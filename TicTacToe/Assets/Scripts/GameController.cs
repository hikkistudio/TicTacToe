using System.Collections;
//using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class GameController : MonoBehaviour
{
    //Скрипт управляющий игровым полем
    private static HSField HSFieldScript;

    //Те значения которые могут принимать ячейки на поле
    public enum CellType
    {
        Empty = 0,
        User = 1,
        Comp = 2
    }

    //Разные варианты окончания игры
    public enum EndGameType
    {
        No = 0,
        Win = 1,
        Draw = 2,
        Lose = 3
    }

    //Различные состояния в которых может быть игра
    public enum GameStateType
    {
        Init = 0,
        Stop = 1,
        Pause = 2,
        Play = 3,
        Reload = 4
    }

    //Сложность в игре
    public enum LevelType
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    //Временная переменная с помощью которой будем перекодировать параметры для долговременного сохранения
    private static int n_tmp_flag;

    //Была ли произведена начальная инициализация приложения?
    //true - инициализация была проведена
    //false - предстоит инициализация
    private static bool flInit = false;

    //Доступен ли скрипт с игровым полем
    private static bool flInitUIScript = false;

    //Переменная для ожидания ходя пользователя.
    //Нумерация ячеек идёт с 0 до 8. Число -1 означает, либо что шага ещё не было, либо что шаг обработан
    private static int nWaitUserStep = -1;

    //Состояние игры.
    //Значения:
    //GameStateType.Init, GameStateType.Play, GameStateType.Pause, GameStateType.Stop, GameStateType.Reload
    private static GameStateType GState = GameStateType.Stop;

    //Достигнут конец игры? Переменная определяет кто выиграл
    //Значения:
    //EndGameType.Win, EndGameType.Draw, EndGameType.Lose, EndGameType.No
    //private static EndGameType EndCurrentGame = EndGameType.No;

    //Настройка сложности игры
    public LevelType LevelGame = LevelType.Medium;

    /**************************************************************************************************/
    /*Данные которые сохраняем в системе для дальнейшей загрузки с нуля*/
    /**************************************************************************************************/

    //На глобальном уровне определяем включён ли звук в игре или выключен
    private static bool flOnSound = false;

    //Фоновая музыка включена?
    private static bool flBackgroundMusic = true;

    //Каким знаком изнрает пользователь. Крестиком или ноликом?
    private static bool flCrossOrZero = true;

    //Кто ходит сейчас будет ходить, пользователь или комп?
    //User(true) / AI(false)
    private static bool flCurrentStep = true;

    //Игра находится в процессе
    private static bool flGameInProcess = false;

    //Массив с текущем состоянием на поле
    //Значения:
    //CellType.Empty: пустая ячейка
    //CellType.User: знак юзера
    //CellType.Comp: знак компа
    private static CellType[] GameBoard = new CellType[9];

    /**************************************************************************************************/
    /*Сеттеры и геттеры*/
    /**************************************************************************************************/

    public static bool flagBackgroundMusic
    {
        get
        {


            return flBackgroundMusic;
        }

        set
        {
            flBackgroundMusic = value;

            n_tmp_flag = (value) ? 1 : 0;
            PlayerPrefs.SetInt("flagBackgroundMusic", n_tmp_flag);
        }
    }

    public static bool flagOnSound
    {
        get
        {
            return flOnSound;
        }

        set
        {
            flOnSound = value;

            n_tmp_flag = (value) ? 1 : 0;
            PlayerPrefs.SetInt("flagOnSound", n_tmp_flag);
        }
    }

    /*
    What sign is the user playing with...
    true = cross
    false = zero
    Смена этого значения в настройках должна приводить к созданию новой игры
    */
    public static bool flagCrossOrZero
    {
        get
        {
            return flCrossOrZero;
        }

        set
        {
            flCrossOrZero = value;

            n_tmp_flag = (value) ? 1 : 0;
            PlayerPrefs.SetInt("flagCrossOrZero", n_tmp_flag);
        }
    }

    /**************************************************************************************************/
    /*Загрузка начальных параметров игры*/
    /**************************************************************************************************/

    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("GameController");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);

    }

    void Update()
    {
        //Первоначальная настройка всей игры при запуске приложения
        if (flInit == false)
        {
            flInit = true;

            //Загружаем данные того состояния в котором мы оставили приложение последний раз
            LoadAllParameters();

            //Останавливаем игровой цикл до того момента пока пользователь не зайдёт на поле
            if (flGameInProcess)
            {
                GState = GameStateType.Pause;
            }
            else
            {
                GState = GameStateType.Stop;
            }

            //Настраиваем звуковую среду в соответстствии с загруженными настройками
            SoundManager.Init();

        }

        //Промежуточное состояние во время запуска игры
        //Служит для возможности подгрузки ресурсов, выдать какое-нибудь сообщение пользователю и сделать небольшую задержку перед запуском
        if ((GState == GameStateType.Init) || (GState == GameStateType.Reload))
        {

            //Если экран с игровым полем загружен и игра в процессе, то запускаем главный игровой цикл
            if ((flInitUIScript) && (flGameInProcess))
            {

                //При инициализации и перезагрузке будем реализовывать разные сценарии поведения
                bool flagInitOrReload = (GState == GameStateType.Init) ? true : false;

                //Временно ставим паузу
                //Это нужно, чтобы апдэйт бесконечно не вызывал данную ветку
                GState = GameStateType.Pause;

                //При инициализации мы будем говорить пользователю, каким символом он ходит
                if (flagInitOrReload)
                {

                    //Запуск корутины с небольшой задержкой перед началом игры
                    //Также отображаем в строке на экране кто чем будет ходить
                    StartCoroutine(StartGameCoroutine(true));
                }
                else
                {
                    //При перезагрузке игры он скорее всего и сам это помнит, ничего ненадо выводить

                    //Запуск корутины с небольшой задержкой перед началом игры
                    StartCoroutine(StartGameCoroutine(false));
                }


            }

        }


        //Пошла игра, крутим главный игровой цикл
        if (GState == GameStateType.Play)
        {
            GameLoop();
        }

    }

    //Первоначальная загрузка параметров.
    //Если до этого приложение не запускалось, то берём значения по умолчанию
    private static void LoadAllParameters()
    {
        //Debug.Log("LoadAllParameters");

        n_tmp_flag = PlayerPrefs.GetInt("flagBackgroundMusic", 1);
        flBackgroundMusic = (n_tmp_flag == 1) ? true : false;

        n_tmp_flag = PlayerPrefs.GetInt("flagOnSound", 1);
        flOnSound = (n_tmp_flag == 1) ? true : false;

        n_tmp_flag = PlayerPrefs.GetInt("flagCrossOrZero", 1);
        flCrossOrZero = (n_tmp_flag == 1) ? true : false;

        n_tmp_flag = PlayerPrefs.GetInt("flagCurrentStep", 1);
        flCurrentStep = (n_tmp_flag == 1) ? true : false;

        n_tmp_flag = PlayerPrefs.GetInt("flagGameInProcess", 1);
        flGameInProcess = (n_tmp_flag == 1) ? true : false;

        //Загружаем данные только если это необходимо
        if (flGameInProcess)
        {
            LoadGameField();
        }

        //Debug.Log("flagBackgroundMusic = " + flBackgroundMusic);
        //Debug.Log("flagCrossOrZero = " + flCrossOrZero);
        //Debug.Log("flagCurrentStep = " + flCurrentStep);
        //Debug.Log("flagOnSound = " + flOnSound);
        //Debug.Log("flagGameInProcess = " + flGameInProcess);
    }

    //Инициализуем пустое игровое поле
    private static void InitGameField()
    {
        //Debug.Log("InitGameField");

        for (int i = 0; i < 9; i++)
        {
            GameBoard[i] = CellType.Empty;
        }
    }

    //Инициализуем игровое поле из сохранёнок
    private static bool LoadGameField()
    {
        //Debug.Log("LoadGameField");

        //
        string strGameField = PlayerPrefs.GetString("strGameField", "000000000");

        //Debug.Log("strGameField = " + strGameField);

        if (strGameField.Length == 9)
        {
            for (int i = 0; i < 9; i++)
            {
                if (strGameField[i] == '1')
                {
                    GameBoard[i] = CellType.User;
                }
                else if (strGameField[i] == '2')
                {
                    GameBoard[i] = CellType.Comp;
                }
                else
                {
                    //strGameField[i] == '0'
                    GameBoard[i] = CellType.Empty;
                }
            }

            return true;
        }

        return false;
    }

    //Сохраняем игровое поле в системе
    //Параметр: blEmpty - сохранить чистое поле, по умолчанию нет.
    private static void SaveGameField(bool blEmpty = false)
    {

        //Debug.Log("SaveGameField");
        //Debug.Log("blEmpty = " + blEmpty);

        //Строка для сохранения
        string strGameField = string.Empty;

        if (blEmpty)
        {
            //Создаём строку эквивалентную пустому игровому полю
            strGameField = new String('0', 9);
        }
        else
        {
            //Сохраняем игровое поле в строке
            for (int i = 0; i < 9; i++)
            {
                if (GameBoard[i] == CellType.User)
                {
                    strGameField += '1';
                }
                else if (GameBoard[i] == CellType.Comp)
                {
                    strGameField += '2';
                }
                else
                {
                    //GameBoard[i] == CellType.Empty
                    strGameField += '0';
                }
            }
        }

        //Сохраняем поле в системе
        PlayerPrefs.SetString("strGameField", strGameField);

        //Debug.Log("strGameField = " + strGameField);

    }

    //Загрузка игрового поля в память 
    public static void InitUIGameField()
    {
        //Debug.Log("InitUIGameField");

        //Для вывода результатов наших действий на экран
        GameObject obj = GameObject.FindWithTag("HSController");
        if (obj != null)
        {
            HSFieldScript = obj.GetComponent<HSField>();

            //Приведение игрового поля в соответствии с внутренним представлением
            for (int i = 0; i < 9; i++)
            {

                if (GameBoard[i] == CellType.User)
                {
                    HSFieldScript.DrawSignInCell(i, flagCrossOrZero);
                }
                else if (GameBoard[i] == CellType.Comp)
                {
                    HSFieldScript.DrawSignInCell(i, !flagCrossOrZero);
                }
            }

        }

        //Игровое поле становится доступным для работы с ним
        flInitUIScript = true;

        //Debug.Log("InitUIGameField!!!!!!!!!!!");
    }


    /**************************************************************************************************/
    /*Непосредственно игровые функции*/
    /**************************************************************************************************/

    /********************************************
    Обработка действий пользователя - приём данных из интерфейсной части
        Пользователь щёлкнул на той или иной ячейке, обрабатываем это нажатие
        nCell - номер ячейки с 0 до 8
    ********************************************/
    public static void ClickCell(int nCell)
    {
        //Debug.Log("ClickCell");

        //Далее игровой цикл увидит новое значение и сделает всё остальное
        nWaitUserStep = nCell;
    }

    /********************************************
    Обработка действий пользователя - центральная функция
        Ход пользователя.
        Нумерация идёт с 0 до 8
        Заносим результаты хода юзера во внутренние структуры программы
        Отрисовываем фигуру пользователя на поле
    ********************************************/
    private void UserStep(int nCell)
    {
        //Debug.Log("UserStep");

        //Интерфейс устроен таким образом, что пользователь не смог бы воспользоваться уже занятой ячейкой
        //Также запуск данной функции означает, что сейчас должен следовать ход пользователя
        //Отрисовываем пользовательский знак
        HSFieldScript.DrawSignInCell(nCell, flCrossOrZero);

        //Заносим новое значение во внутренние структуры
        GameBoard[nCell] = CellType.User;

        //Debug.Log("UserStep: end");
    }

    /********************************************
    Логика компьютера - поиск выгодных мест, где можно поставить вторую ячейку к уже имеющейся
    ********************************************/
    private int СompStepTwoCells(CellType UserOrComp)
    {

        //Потенциально выгодные ячейки
        int nStepVariant1 = -1;
        int nStepVariant2 = -1;

        /*
        0 1 2
        3 4 5
        6 7 8
        */

        // Горизонтали
        if ((GameBoard[0] == UserOrComp) && (GameBoard[1] == CellType.Empty) && (GameBoard[2] == CellType.Empty)) nStepVariant1 = 2;
        if ((GameBoard[0] == CellType.Empty) && (GameBoard[1] == CellType.Empty) && (GameBoard[2] == UserOrComp)) nStepVariant1 = 0;
        if ((GameBoard[0] == CellType.Empty) && (GameBoard[1] == UserOrComp) && (GameBoard[2] == CellType.Empty))
        {
            nStepVariant1 = 0;
            nStepVariant2 = 2;
        }

        if ((GameBoard[3] == UserOrComp) && (GameBoard[4] == CellType.Empty) && (GameBoard[5] == CellType.Empty)) nStepVariant1 = 5;
        if ((GameBoard[3] == CellType.Empty) && (GameBoard[4] == CellType.Empty) && (GameBoard[5] == UserOrComp)) nStepVariant1 = 3;
        if ((GameBoard[3] == CellType.Empty) && (GameBoard[4] == UserOrComp) && (GameBoard[5] == CellType.Empty))
        {
            nStepVariant1 = 3;
            nStepVariant2 = 5;
        }

        if ((GameBoard[6] == UserOrComp) && (GameBoard[7] == CellType.Empty) && (GameBoard[8] == CellType.Empty)) nStepVariant1 = 8;
        if ((GameBoard[6] == CellType.Empty) && (GameBoard[7] == CellType.Empty) && (GameBoard[8] == UserOrComp)) nStepVariant1 = 6;
        if ((GameBoard[6] == CellType.Empty) && (GameBoard[7] == UserOrComp) && (GameBoard[8] == CellType.Empty))
        {
            nStepVariant1 = 6;
            nStepVariant2 = 8;
        }

        /*
        0 1 2
        3 4 5
        6 7 8
        */

        // Вертикали
        if ((GameBoard[0] == UserOrComp) && (GameBoard[3] == CellType.Empty) && (GameBoard[6] == CellType.Empty)) nStepVariant1 = 6;
        if ((GameBoard[0] == CellType.Empty) && (GameBoard[3] == CellType.Empty) && (GameBoard[6] == UserOrComp)) nStepVariant1 = 0;
        if ((GameBoard[0] == CellType.Empty) && (GameBoard[3] == UserOrComp) && (GameBoard[6] == CellType.Empty))
        {
            nStepVariant1 = 0;
            nStepVariant2 = 6;
        }

        if ((GameBoard[1] == UserOrComp) && (GameBoard[4] == CellType.Empty) && (GameBoard[7] == CellType.Empty)) nStepVariant1 = 7;
        if ((GameBoard[1] == CellType.Empty) && (GameBoard[4] == CellType.Empty) && (GameBoard[7] == UserOrComp)) nStepVariant1 = 1;
        if ((GameBoard[1] == CellType.Empty) && (GameBoard[4] == UserOrComp) && (GameBoard[7] == CellType.Empty))
        {
            nStepVariant1 = 1;
            nStepVariant2 = 7;
        }

        if ((GameBoard[2] == UserOrComp) && (GameBoard[5] == CellType.Empty) && (GameBoard[8] == CellType.Empty)) nStepVariant1 = 8;
        if ((GameBoard[2] == CellType.Empty) && (GameBoard[5] == CellType.Empty) && (GameBoard[8] == UserOrComp)) nStepVariant1 = 2;
        if ((GameBoard[2] == CellType.Empty) && (GameBoard[5] == UserOrComp) && (GameBoard[8] == CellType.Empty))
        {
            nStepVariant1 = 2;
            nStepVariant2 = 8;
        }

        /*
        0 1 2
        3 4 5
        6 7 8
        */

        // Диагонали
        if ((GameBoard[0] == UserOrComp) && (GameBoard[4] == CellType.Empty) && (GameBoard[8] == CellType.Empty)) nStepVariant1 = 8;
        if ((GameBoard[0] == CellType.Empty) && (GameBoard[4] == CellType.Empty) && (GameBoard[8] == UserOrComp)) nStepVariant1 = 0;
        if ((GameBoard[0] == CellType.Empty) && (GameBoard[4] == UserOrComp) && (GameBoard[8] == CellType.Empty))
        {
            nStepVariant1 = 0;
            nStepVariant2 = 8;
        }

        if ((GameBoard[2] == UserOrComp) && (GameBoard[4] == CellType.Empty) && (GameBoard[6] == CellType.Empty)) nStepVariant1 = 6;
        if ((GameBoard[2] == CellType.Empty) && (GameBoard[4] == CellType.Empty) && (GameBoard[6] == UserOrComp)) nStepVariant1 = 2;
        if ((GameBoard[2] == CellType.Empty) && (GameBoard[4] == UserOrComp) && (GameBoard[6] == CellType.Empty))
        {
            nStepVariant1 = 2;
            nStepVariant2 = 8;
        }

        // Присутствуют потенциально две интересных ячейки, выбираем одну
        if (nStepVariant2 != -1)
        {

            // Если процент пустых клеток большой, то выбираем большую переменную,
            // но если игра только начинается, то большую.
            byte bt_empty_cell_counter = 0;
            for (byte i = 0; i < 9; i++)
            {
                if (GameBoard[i] == CellType.Empty)
                {
                    bt_empty_cell_counter++;
                }
            }

            if (bt_empty_cell_counter >= 5)
            {
                //Debug.Log("СompStepTwoCells: Variant2 = " + nStepVariant2);
                return nStepVariant1;
            }
            else
            {
                //Debug.Log("СompStepTwoCells: Variant1 = " + nStepVariant1);
                return nStepVariant2;
            }

        }

        //Debug.Log("СompStepTwoCells: Variant1 = " + nStepVariant1);
        return nStepVariant1;

    }

    /********************************************
    Логика компьютера - анализ на возможность выиграть компу/юзеру одним ходом
    ********************************************/
    private int СompStepAnaliz(CellType UserOrComp)
    {
        //Наша потенциально интересная ячейка
        int nStep = -1;

        /*
        0 1 2
        3 4 5
        6 7 8
        */

        // Горизонтали
        if ((GameBoard[0] == UserOrComp) && (GameBoard[1] == UserOrComp) && (GameBoard[2] == CellType.Empty)) nStep = 2;
        if ((GameBoard[0] == CellType.Empty) && (GameBoard[1] == UserOrComp) && (GameBoard[2] == UserOrComp)) nStep = 0;
        if ((GameBoard[0] == UserOrComp) && (GameBoard[1] == CellType.Empty) && (GameBoard[2] == UserOrComp)) nStep = 1;

        if ((GameBoard[3] == UserOrComp) && (GameBoard[4] == UserOrComp) && (GameBoard[5] == CellType.Empty)) nStep = 5;
        if ((GameBoard[3] == CellType.Empty) && (GameBoard[4] == UserOrComp) && (GameBoard[5] == UserOrComp)) nStep = 3;
        if ((GameBoard[3] == UserOrComp) && (GameBoard[4] == CellType.Empty) && (GameBoard[5] == UserOrComp)) nStep = 4;

        if ((GameBoard[6] == UserOrComp) && (GameBoard[7] == UserOrComp) && (GameBoard[8] == CellType.Empty)) nStep = 8;
        if ((GameBoard[6] == CellType.Empty) && (GameBoard[7] == UserOrComp) && (GameBoard[8] == UserOrComp)) nStep = 6;
        if ((GameBoard[6] == UserOrComp) && (GameBoard[7] == CellType.Empty) && (GameBoard[8] == UserOrComp)) nStep = 7;

        /*
        0 1 2
        3 4 5
        6 7 8
        */

        // Вертикали
        if ((GameBoard[0] == UserOrComp) && (GameBoard[3] == UserOrComp) && (GameBoard[6] == CellType.Empty)) nStep = 6;
        if ((GameBoard[0] == CellType.Empty) && (GameBoard[3] == UserOrComp) && (GameBoard[6] == UserOrComp)) nStep = 0;
        if ((GameBoard[0] == UserOrComp) && (GameBoard[3] == CellType.Empty) && (GameBoard[6] == UserOrComp)) nStep = 3;

        if ((GameBoard[1] == UserOrComp) && (GameBoard[4] == UserOrComp) && (GameBoard[7] == CellType.Empty)) nStep = 7;
        if ((GameBoard[1] == CellType.Empty) && (GameBoard[4] == UserOrComp) && (GameBoard[7] == UserOrComp)) nStep = 1;
        if ((GameBoard[1] == UserOrComp) && (GameBoard[4] == CellType.Empty) && (GameBoard[7] == UserOrComp)) nStep = 4;

        if ((GameBoard[2] == UserOrComp) && (GameBoard[5] == UserOrComp) && (GameBoard[8] == CellType.Empty)) nStep = 8;
        if ((GameBoard[2] == CellType.Empty) && (GameBoard[5] == UserOrComp) && (GameBoard[8] == UserOrComp)) nStep = 2;
        if ((GameBoard[2] == UserOrComp) && (GameBoard[5] == CellType.Empty) && (GameBoard[8] == UserOrComp)) nStep = 5;

        /*
        0 1 2
        3 4 5
        6 7 8
        */

        // Диагонали
        if ((GameBoard[0] == UserOrComp) && (GameBoard[4] == UserOrComp) && (GameBoard[8] == CellType.Empty)) nStep = 8;
        if ((GameBoard[0] == CellType.Empty) && (GameBoard[4] == UserOrComp) && (GameBoard[8] == UserOrComp)) nStep = 0;
        if ((GameBoard[0] == UserOrComp) && (GameBoard[4] == CellType.Empty) && (GameBoard[8] == UserOrComp)) nStep = 4;

        if ((GameBoard[2] == UserOrComp) && (GameBoard[4] == UserOrComp) && (GameBoard[6] == CellType.Empty)) nStep = 6;
        if ((GameBoard[2] == CellType.Empty) && (GameBoard[4] == UserOrComp) && (GameBoard[6] == UserOrComp)) nStep = 2;
        if ((GameBoard[2] == UserOrComp) && (GameBoard[4] == CellType.Empty) && (GameBoard[6] == UserOrComp)) nStep = 4;

        //Debug.Log("СompStepAnaliz: nStep: " + nStep);
        return nStep;

    }

    /********************************************
    Логика компьютера - первый ход
        Первый ход компьютера. Поиск места для хода проиходит по рандому
        Возвращаемое значение: индекс ячейки для первого хода
    ********************************************/
    private byte СompFirstStep()
    {

        //Debug.Log("Сomp_First_Step");

        byte bt_start_index = 4;
        byte bt_counter_anti_bug = 0;

        while (true)
        {
            bt_start_index = Convert.ToByte(UnityEngine.Random.Range(0, 8));

            // Перед нами возможно первым ходил человек,
            // поэтому проверяем пуста ли ячейка?
            if (GameBoard[bt_start_index] == CellType.Empty) break;

            //Мера предосторожности
            if (bt_counter_anti_bug == 100) break;
            bt_counter_anti_bug++;
        }

        return bt_start_index;
    }

    /********************************************
    Логика компьютера - выбор оптимальной игровой стретегии
    ********************************************/
    private int СompStrategy()
    {

        //Debug.Log("СompStrategy");

        //Наш будущий ход
        int nStep = -1;

        //Если это первый ход компьютера, то ищем по рандому пустую ячейку для хода

        //Проверяем не является ли данный ход первым?
        int bt_comp_step_counter = 0;
        for (int i = 0; i < 9; i++)
        {
            if (GameBoard[i] == CellType.Comp)
            {
                bt_comp_step_counter++;
            }
        }

        //Debug.Log("СompStrategy: bt_comp_step_counter = " + bt_comp_step_counter);

        //Первый ход делаем по рандому
        if (bt_comp_step_counter == 0)
        {

            nStep = СompFirstStep();
            //Debug.Log("СompStrategy, first step: nStep = " + nStep);
            if ((nStep != -1) && (GameBoard[nStep] == CellType.Empty)) return nStep;

        }

        //Второй наш ход.
        //Здесь регулируем сложность
        //Если сложность низкая, то просто ходим по рандому
        //Если сложность средняя, то ходим по рандому только, когда пользователь играет ноликами
        //Если сложность высокая то просто идём дальше и обходимся без рандома
        if ((bt_comp_step_counter == 1) &&
            (
                (LevelGame == LevelType.Easy) ||
                ((LevelGame == LevelType.Medium) && (flCrossOrZero == false))
            )
        )
        {
            nStep = СompFirstStep();
            //Debug.Log("СompStrategy, second step, not hard: nStep = " + nStep + "; LevelGame = " + LevelGame);
            if ((nStep != -1) && (GameBoard[nStep] == CellType.Empty)) return nStep;
        }

        //Debug.Log("СompStrategy: 1");

        //Алгоритм поиска выгодного хода для компьютера

        //Атака. Есть возможность выиграть одним действием
        nStep = СompStepAnaliz(CellType.Comp);
        if ((nStep != -1) && (GameBoard[nStep] == CellType.Empty)) return nStep;

        //Debug.Log("СompStrategy: 2");

        //Защита. Закрываем возможность выиграть противнику одним действием
        nStep = СompStepAnaliz(CellType.User);
        if ((nStep != -1) && (GameBoard[nStep] == CellType.Empty)) return nStep;

        //Debug.Log("СompStrategy: 3");

        //Выбор направления. Стремимся занять места с двумя ячейками, когда уже есть одна в ряду
        nStep = СompStepTwoCells(CellType.Comp);
        if ((nStep != -1) && (GameBoard[nStep] == CellType.Empty)) return nStep;

        //Debug.Log("СompStrategy: 4");

        //Перестраховка
        //Если ничего не помогло говорим, что не справились
        return -1;

    }

    /********************************************
    Логика компьютера - отобразить ход компа с некоторой задержкой
        Параметры: 
            int nCell - Ячейка в которую будем отображать знак компа 
            float fTime - Время в секундах на которое задерживаем появление символа
            По умолчанию равно одной секунде
    ********************************************/
    IEnumerator CompStepDelayCoroutine(int nCell, float fTime = 1)
    {

        yield return new WaitForSeconds(fTime);

        //Отрисовываем на игровом поле знак компа
        HSFieldScript.DrawSignInCell(nCell, !flCrossOrZero);
    }

    /********************************************
    Логика компьютера - центральная функция
        Ход компьютера.
        Делаем ход и сохраняем его в структурах памяти программы
        Отрисовываем фигуру компьютера на поле
    ********************************************/
    private void CompStep()
    {
        //Debug.Log("CompStep");

        //Наша будущая результирующая ячейка
        int nCell = -1;

        //Перебираем различные варианты ходов и получаем результирующую ячейку
        nCell = СompStrategy();

        Debug.Log("CompStep: nCell = " + nCell);
        //Debug.Log("GameBoard[nCell] = " + GameBoard[nCell]);

        //Подстраховочная функция
        //Если ничего из вышеперечисленного не помогло, то просто выбираем первую попавшуюся пустую ячейку
        if ((nCell == -1) || (GameBoard[nCell] != CellType.Empty))
        {
            for (int i = 0; i < 9; i++)
            {
                if (GameBoard[i] == CellType.Empty)
                {
                    nCell = i;
                    break;
                }
            }
        }

        //Если мы имеем всё ещё -1 в nCell это значит, что все ячейки заняты
        //Выдаём ошибку в дебаг-логе и принудительно очищаем игровое поле
        if (nCell == -1)
        {
            Debug.Log("Error game field overflow");
            NewGame();

            //У нас есть корутины с эффектами и сообщением для пользователя, прекращаем их
            StopAllCoroutines();

            //Выбрасываем пользователя назад в меню
            SceneManager.LoadScene("Menu");

        }
        else
        {
            //Всё нормально

            //Заносим новое значение во внутренние структуры
            GameBoard[nCell] = CellType.Comp;

            //Отрисовка на экране знака компа
            //Минимальная задержка, чтобы ход компьютера не отображался мгновенно на экране
            //На самом деле ход уже сделан, поэтому даже если пользователь за это время натыкает несколько ячеек это не отразится находе игры
            StartCoroutine(CompStepDelayCoroutine(nCell, 0.2f));
        }

        //Debug.Log("CompStep: end");
    }

    /********************************************
    Тестирование поля
        Проверка выиграл ли кто-то?
        Параметры: CellType test_znak - какой знак тестируем: пустота, юзер или комп?
        Возвращаемое значение: bool - тестируемый знак точно занял собой на поле три ячейки подряд(истина)/нет не занял(ложь)
    ********************************************/

    public static bool TestZnakWin(CellType test_znak)
    {

        //Debug.Log("TestZnakWin");

        /*
        0 1 2
        3 4 5
        6 7 8
        */

        //Горизонтали + вертикали + диагонали
        if (((GameBoard[0] == test_znak) && (GameBoard[1] == test_znak) && (GameBoard[2] == test_znak)) ||
                ((GameBoard[3] == test_znak) && (GameBoard[4] == test_znak) && (GameBoard[5] == test_znak)) ||
                ((GameBoard[6] == test_znak) && (GameBoard[7] == test_znak) && (GameBoard[8] == test_znak)) ||
                ((GameBoard[0] == test_znak) && (GameBoard[3] == test_znak) && (GameBoard[6] == test_znak)) ||
                ((GameBoard[1] == test_znak) && (GameBoard[4] == test_znak) && (GameBoard[7] == test_znak)) ||
                ((GameBoard[2] == test_znak) && (GameBoard[5] == test_znak) && (GameBoard[8] == test_znak)) ||
                ((GameBoard[0] == test_znak) && (GameBoard[4] == test_znak) && (GameBoard[8] == test_znak)) ||
                ((GameBoard[2] == test_znak) && (GameBoard[4] == test_znak) && (GameBoard[6] == test_znak)))
        {
            return true;
        }

        return false;
    }

    /********************************************
    Тестирование поля
        Тестируем ситуацию на поле. Возможно кто-то выиграл?
    ********************************************/
    private EndGameType TestFieldGame()
    {
        //Debug.Log("TestFieldGame");

        //Пользователь выиграл?
        if (TestZnakWin(CellType.User))
        {
            //Debug.Log("TestFieldGame: Win");
            return EndGameType.Win;
        }

        //Комп выиграл?
        if (TestZnakWin(CellType.Comp))
        {
            //Debug.Log("TestFieldGame: Lose");
            return EndGameType.Lose;
        }

        //Проверка на ничью
        //Пока есть хоть одна не занятая ячейка игра не закончится
        for (byte i = 0; i < 9; i++)
        {

            //Нашли пустую ячейку, продолжаем игру
            if (GameBoard[i] == CellType.Empty)
            {
                break;
            }

            //Дошли до конца массива, а пустых ячеек нет, значит ничья
            if (i == 8)
            {
                //Debug.Log("TestFieldGame: Draw");
                return EndGameType.Draw;
            }

        }

        //Debug.Log("TestFieldGame: No");

        //По умолчанию считаем, что игра продолжается
        return EndGameType.No;

    }

    /**************************************************************************************************
    Главный игровой цикл
    Цикл работает только в том случае если мы находимся на экране с полем игры
    **************************************************************************************************/
    private void GameLoop()
    {

        //Для отслеживания состояния поля
        EndGameType StateGame = EndGameType.No;

        //Определяемся чей сейчас ход
        if (flCurrentStep)
        {
            //Ходит пользователь

            //Wait for user step
            if (nWaitUserStep != -1)
            {
                //Debug.Log("GameLoop - User step: " + nWaitUserStep);

                //Обновляем структуры и отрисовываем ход пользователя
                UserStep(nWaitUserStep);

                //Говорим программме, что следующим ходит компьютер
                flCurrentStep = false;
                nWaitUserStep = -1;

                //Проверяем ситуацию на поле и в случае чего завершаем игру
                StateGame = TestFieldGame();
            }
        }
        else
        {
            //Ходит компьютер

            //Debug.Log("GameLoop - Comp step");

            //Говорим программме, что следующим ходит пользователь
            flCurrentStep = true;
            nWaitUserStep = -1;

            //Логика компьютера, обновление данных и отрисовка хода компьютера
            CompStep();

            //Debug.Log("GameLoop - Comp step end");

            //Проверяем ситуацию на поле и в случае чего завершаем игру
            StateGame = TestFieldGame();

        }

        //Если закончили текущую игру, то выставляем флаги в начальное состояние
        if (StateGame != EndGameType.No)
        {
            GameController.StopGame();

            //Показываем пользователю окошко с результатом            
            HSFieldScript.WndEndGame(StateGame);
        }

    }

    /**************************************************************************************************/
    /*Управление игрой*/
    /**************************************************************************************************/

    //Функция возвращающая текущее состояние игры
    //Значения:
    //GameStateType.Init, GameStateType.Play, GameStateType.Pause, GameStateType.Stop
    public static GameStateType GameState()
    {
        //Debug.Log("GameState");
        //Debug.Log("GState = " + GState);

        return GState;
    }

    //Запускаем игровой цикл
    //Делаем небольшую задержку
    //Также отображаем сообщение о том, кто чем играет
    IEnumerator StartGameCoroutine(bool blMessageForUser = false)
    {

        //Начальная задержка перед стартом игры
        float fDelayStartGame = 0.2f;

        //Небольшая задержка нужна только в том случае если комп ходит первым
        //Но пускай ничего не держит пользователя
        if (flCrossOrZero == false)
        {
            GState = GameStateType.Play;
        }

        //Если эффектов нам не заказывали
        //или же панель с сообщениями уже существует, просто запустим цикл пользователя и на этом всё
        if ((blMessageForUser == false) || (HSFieldScript.flagInitUserMsg))
        {
            //просто обеспечиваем задержку перед началом игры
            //Небольшая задержка нужна особенно в том случае если комп ходит первым
            yield return new WaitForSeconds(fDelayStartGame);

            //Запускаем главный игровой цикл
            GState = GameStateType.Play;
        }
        else
        {
            //Отображение информационной надписи перед стартом

            //Инициализируем панель с сообщением пользователю
            HSFieldScript.InitMessageForUser();

            //Отображаем текстовые условия пользователю
            string strMsgUserSrc = "User:";
            string strMsgCompSrc = "Comp:";
            string strMsgUserRes = "";
            string strMsgCompRes = "";

            //Debug.Log("1");

            //Небольшая задержка перед появлением надписей
            yield return new WaitForSeconds(0.5f);


            //По символьно отображаем всю строку на экране
            for (int i = 0; i < strMsgUserSrc.Length; i++)
            {
                strMsgUserRes += strMsgUserSrc[i];
                strMsgCompRes += strMsgCompSrc[i];
                HSFieldScript.WriteMsgTxt(strMsgUserRes, strMsgCompRes);

                //Debug.Log("2");

                //Пауза после каждой буквы
                yield return new WaitForSeconds(0.1f);

                //Звук изменяется по диапазону создавая эффект нажатия разных клавишь
                SoundManager.TypewriterKeyPlay();

                //Debug.Log("3");

            }

            //Дополнительная пауза перед выводом картинки
            yield return new WaitForSeconds(0.2f);

            //Картинки "X" и "O"
            HSFieldScript.ShowMsgImg();

            //Debug.Log("4");

            //Ещё одна задержка, это нужно для восприятия пользователя
            yield return new WaitForSeconds(fDelayStartGame);

            //Debug.Log("5");

            //Запускаем главный игровой цикл
            GState = GameStateType.Play;

            //После начала, спустя несколько секунд убираем надпись
            yield return new WaitForSeconds(8);
            HSFieldScript.DeleteStartMessage();

            //Debug.Log("6");
        }

    }

    public static void NewGame()
    {
        //Debug.Log("NewGame");

        //Сбрасываем виртуальное поле в начальное состояние
        InitGameField();

        //Если пользователь играет крестиком, то будет ходить первым
        //Иначе предоставляем компу право первого хода
        flCurrentStep = flCrossOrZero;

        //Переменная для дальнейшего ожидания ответа от юзера
        nWaitUserStep = -1;

        //Ждём активации скрипта с полем
        flInitUIScript = false;

        flGameInProcess = true;
        PlayerPrefs.SetInt("flagGameInProcess", 1);

        //Подготовка к запуску главного игрового цикла
        //С помощью флага вызывается функция, которая создаёт незначительную паузу и запускает игровой цикл
        //Также через данную функцию напоминаем пользователю кто чем ходит
        GState = GameStateType.Init;

    }

    //Отличие от NewGame в том, что мы не уходили из сцены
    public static void ReloadGame()
    {
        //Debug.Log("ReloadGame");

        //Сбрасываем виртуальное поле в начальное состояние
        InitGameField();

        //Если пользователь играет крестиком, то будет ходить первым
        //Иначе предоставляем компу право первого хода
        flCurrentStep = flCrossOrZero;

        //Переменная для дальнейшего ожидания ответа от юзера
        nWaitUserStep = -1;

        //Скрипт уже активирован
        flInitUIScript = true;

        flGameInProcess = true;
        PlayerPrefs.SetInt("flagGameInProcess", 1);

        //Подготовка к запуску главного игрового цикла
        //С помощью флага вызывается функция, которая создаёт незначительную паузу и запускает игровой цикл
        //Также через данную функцию напоминаем пользователю кто чем ходит
        GState = GameStateType.Reload;
    }

    //Вернулись в игру после короткого путешествия по меню или неделю спустя
    public static void Resume()
    {
        //Debug.Log("Resume");

        //Проверяем, что у нас под рукой есть все необходимые компоненты
        if ((flGameInProcess) && (flInit))
        {
            //Переменная для дальнейшего ожидания ответа от юзера
            nWaitUserStep = -1;

            //Ждём активации скрипта с полем
            flInitUIScript = false;

            //Подготовка к запуску главного игрового цикла
            GState = GameStateType.Init;
        }
        else
        {
            //Иначе просто создаём новую игру
            NewGame();
        }


    }

    //Вышли в меню или совсем закрываем игру, сохраняем значения
    public static void Pause()
    {
        //Debug.Log("Pause");
        GState = GameStateType.Pause;

        n_tmp_flag = (flCurrentStep) ? 1 : 0;
        PlayerPrefs.SetInt("flagCurrentStep", n_tmp_flag);

        PlayerPrefs.SetInt("flagGameInProcess", 1);
        SaveGameField();

        //Скрипт с полем теперь недоступен
        flInitUIScript = false;
    }

    //Полная остановка текущей игры
    public static void StopGame()
    {
        //Debug.Log("StopGame");

        //Останавливаем игровой цикл 
        GState = GameStateType.Stop;

        //Мы будем вне игровой сцены и скрипт с полем теперь станет недоступен
        flInitUIScript = false;

        flGameInProcess = false;
        PlayerPrefs.SetInt("flagGameInProcess", 0);

        //Сохраняем поле в системе как пустое
        SaveGameField(true);
    }

}

