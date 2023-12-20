using UnityEngine;

public class SoundToggle : MonoBehaviour
{
    public GameObject soundOff;
    public GameObject soundOn;
    public bool isSoundOn = true;
    private void Start()
    {
        // Начнем с установки начального состояния звука в зависимости от сохраненного значения
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        UpdateSound();
        if (soundOn || soundOff)
        {
            if (isSoundOn == true)
            {
                soundOff.SetActive(false);
                soundOn.SetActive(true);
            }
            else if (isSoundOn == false)
            {
                soundOn.SetActive(false);
                soundOff.SetActive(true);
            }
        }
    }

    public void UpdateSound()
    {
        // Установите состояние AudioListener
        AudioListener.volume = isSoundOn ? 1.0f : 0.0f;
    }

    // Метод для включения звука
    public void On()
    {
        isSoundOn = true;
        UpdateSound();

        soundOff.SetActive(false);
        soundOn.SetActive(true);
        // Сохраняем текущее состояние звука
        PlayerPrefs.SetInt("SoundOn", 1);
        PlayerPrefs.Save();
    }

    // Метод для выключения звука
    public void Off()
    {
        isSoundOn = false;
        UpdateSound();

        soundOn.SetActive(false);
        soundOff.SetActive(true);
        // Сохраняем текущее состояние звука
        PlayerPrefs.SetInt("SoundOn", 0);
        PlayerPrefs.Save();
    }

    // Метод для переключения звука
    private void ToggleSound(bool isFocused)
    {
        if (isFocused)
        {
            // Переключаем звук при изменении фокуса
            if (isSoundOn)
            {
                Off();
            }
            else
            {
                On();
            }
        }
    }
}
