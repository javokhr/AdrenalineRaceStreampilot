using UnityEngine;
using GamePush;
public class InstructionPanel : MonoBehaviour
{
    public GameObject instructionPanel;

    private void Start()
    {
        // Проверяем, является ли устройство не мобильным топом
        if (GP_Device.IsMobile())
        {
            // Скрываем панель инструкций
            instructionPanel.SetActive(false);

            // Возобновляем игру
            Time.timeScale = 1f;
        }
        else
        {
            // Показываем панель инструкций
            instructionPanel.SetActive(true);

            // Останавливаем игру
            Time.timeScale = 0f;
        }
    }
    public void ButtonResume()
    {
        // Скрываем панель инструкций
        instructionPanel.SetActive(false);

        // Возобновляем игру
        Time.timeScale = 1f;
    }
}
