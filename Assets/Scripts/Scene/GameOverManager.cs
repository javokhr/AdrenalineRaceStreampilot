using UnityEngine;
public class GameOverManager : MonoBehaviour
{

    [SerializeField] private GameObject gameOverPopUpPrefab;

    [SerializeField] private bool raceFinished = false;
    [SerializeField] private GameObject SpeedText;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !raceFinished)
        {
            raceFinished = true;
            SpeedText.SetActive(false);
            CheckMobileCanavas();
            GameOver();
            Debug.Log("Game Over");
        }
        if (other.CompareTag("Bot"))
        {
            Debug.Log("Bot collided");
            Destroy(other.gameObject); // Попробуйте удалить объект и выведите сообщение в консоль
        }

    }

    private void GameOver()
    {
        GameObject gameOverPopUp = gameOverPopUpPrefab;
        gameOverPopUp.SetActive(true); // Активируем попап Game Over
        raceFinished = true;

        Time.timeScale = 0; // Ставим игру на паузу
        Debug.Log("Popub: " + gameOverPopUp);
        // Настройте кнопки попапа и логику их действий
        // Например, кнопка "Restart" может перезагрузить текущий уровень
        // Кнопка "Menu" может вернуть вас в меню
    }
    private void CheckMobileCanavas() //проверка активно ли мобильная управление если да то деактивировать
    {
        string canvasName = "MobileUICanvas";
        GameObject mobileUI = GameObject.Find(canvasName);
        if (mobileUI != null)
        {
            mobileUI.SetActive(false);
        }
    }
}
