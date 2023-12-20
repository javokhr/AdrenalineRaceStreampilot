using UnityEngine;
public class FinishPopupManager : MonoBehaviour
{
    [SerializeField] private GameObject finishPopup; // Ссылка на попап финиша
    [SerializeField] private GameObject SpeedText;

    private bool raceFinished = false; // Переменная для отслеживания завершения гонки

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !raceFinished)
        {
            raceFinished = true;
            SpeedText.SetActive(false);
            CheckMobileCanvas();
            ShowFinishPopup();
            AddMoney();
        }

    }

    private void ShowFinishPopup()
    {
        // Останавливаем время
        Time.timeScale = 0f;

        // Активируем попап финиша
        if (finishPopup != null)
        {
            finishPopup.SetActive(true);
        }
    }

    // Метод для закрытия попапа финиша
    public void CloseFinishPopup()
    {
        // Включаем время
        Time.timeScale = 1f;

        // Деактивируем попап финиша
        if (finishPopup != null)
        {
            finishPopup.SetActive(false);
        }
    }

    // Метод для проверки, завершена ли гонка
    public bool IsRaceFinished()
    {
        return raceFinished;
    }

    // Метод для проверки, активно ли мобильное управление, и, если да, деактивации его
    private void CheckMobileCanvas()
    {
        string canvasName = "MobileUICanvas";
        GameObject mobileUI = GameObject.Find(canvasName);
        if (mobileUI != null)
        {
            mobileUI.SetActive(false);
        }
    }

    private void AddMoney()
    {
        GameManager gameManager = GameManager.Instance;
        int moneyToAdd = 5000; // Например, добавим 500 денег
        gameManager.AddMoney(moneyToAdd); // Вызов метода AddMoney для добавления денег
    }
}
