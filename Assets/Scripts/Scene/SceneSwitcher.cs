using UnityEngine;
public class SceneSwitcher : MonoBehaviour
{
    public string targetSceneName; // Имя целевой сцены для перехода

    [SerializeField] private GameObject gameOverPopUpPrefab;

    public void SwitchToTargetScene() // возвращение домой
    {
        CheckMobileCanavas();
        LoadingScreenController.instance.LoadLevel(targetSceneName); // Вызываем метод загрузки уровня из LoadingScreenController
        // Включаем время
        Time.timeScale = 1f;

        Debug.Log("Переход на: " + targetSceneName);
    }

    public void RestartGame()
    {
        // Здесь вы можете добавить логику для перезапуска игры
        // Например, сбросить позицию и состояние машины, сбросить время и т.д.
        GameObject gameOverPopUp = gameOverPopUpPrefab;
        gameOverPopUp.SetActive(false); // Активируем попап Game Over
        CheckMobileCanavas();
        // Включите Time.timeScale, если вы использовали его для паузы.
        Time.timeScale = 1;

        // Загрузите текущую сцену заново.
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        Debug.Log("Рестарт");
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
