using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public GameObject go_background_music;

    static AudioClip ac_click_sound;
    static AudioClip ac_win_sound;
    static AudioClip ac_draw_sound;
    static AudioClip ac_lose_sound;
    static AudioClip ac_typewriter_sound;


    static AudioSource as_background_music;
    static AudioSource as_effect_sound;


    void Start()
    {
        as_background_music = go_background_music.GetComponent<AudioSource>();
        as_effect_sound = GetComponent<AudioSource>();
        ac_click_sound = Resources.Load<AudioClip>("Audio/click");
        ac_win_sound = Resources.Load<AudioClip>("Audio/win");
        ac_draw_sound = Resources.Load<AudioClip>("Audio/draw");
        ac_lose_sound = Resources.Load<AudioClip>("Audio/lose");
        ac_typewriter_sound = Resources.Load<AudioClip>("Audio/typewriter");
    }

    //Приводим среду в соответсттвии с настройками из скрипта-хранителя
    public static void Init()
    {
        AudioListener.volume = (GameController.flagOnSound) ? 1f : 0f;
        if (GameController.flagBackgroundMusic)
        {
            as_background_music.volume = 0.5f;
            as_effect_sound.pitch = 1f;
            as_background_music.Play();
        }
        else
        {
            as_background_music.Stop();
        }

    }

    public static void BackgroundPlay(bool flagOnOff)
    {
        GameController.flagBackgroundMusic = flagOnOff;

        if (flagOnOff)
        {
            as_background_music.volume = 0.8f;
            as_effect_sound.pitch = 1f;
            as_background_music.Play();
        }
        else
        {
            as_background_music.Stop();
        }
    }

    public static void SoundOnOff(bool flagOnOff)
    {
        GameController.flagOnSound = flagOnOff;
        AudioListener.volume = (flagOnOff) ? 1f : 0f;
    }

    public static bool GetSoundState()
    {
        return GameController.flagOnSound;
    }

    public static void ClickPlay()
    {
        as_effect_sound.clip = ac_click_sound;
        as_effect_sound.volume = 0.5f;
        as_effect_sound.pitch = 1.5f;
        as_effect_sound.Play();
    }

    public static void TypewriterKeyPlay()
    {
        as_effect_sound.clip = ac_typewriter_sound;
        as_effect_sound.volume = 0.1f;
        as_effect_sound.pitch = UnityEngine.Random.Range(0.5f, 1.8f);
        as_effect_sound.Play();
    }

    public static void EndGamePlay(GameController.EndGameType EndGameSound)
    {

        if (EndGameSound == GameController.EndGameType.Win)
        {
            as_effect_sound.clip = ac_win_sound;
            as_effect_sound.volume = 0.5f;
            as_effect_sound.pitch = 1;

        }
        else if (EndGameSound == GameController.EndGameType.Draw)
        {
            as_effect_sound.clip = ac_draw_sound;
            as_effect_sound.volume = 1;
            as_effect_sound.pitch = 1;

        }
        else
        {
            //Lose game
            as_effect_sound.clip = ac_lose_sound;
            as_effect_sound.volume = 0.5f;
            as_effect_sound.pitch = 1;

        }

        as_effect_sound.Play();

    }
}
