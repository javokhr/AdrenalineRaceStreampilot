using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreenController : MonoBehaviour
{
    public GameObject loadingScreenUI; // Объект для отображения экрана загрузки
    public Slider loadingSlider; // Слайдер для отображения прогресса загрузки
    public Text loadingText; // Текстовое поле для отображения процента загрузки

    public static LoadingScreenController instance; // Создаем статическую ссылку на экземпляр

    void Awake()
    {
        if (instance == null)
        {
            instance = this; // Присваиваем ссылку текущему экземпляру
        }
        else
        {
            Destroy(gameObject); // Уничтожаем объект, если другой экземпляр уже существует
        }
    }

    void Start()
    {
        loadingScreenUI.SetActive(false); // Изначально скрываем экран загрузки
    }

    public void ShowLoadingScreen()
    {
        loadingScreenUI.SetActive(true); // Показываем экран загрузки
    }

    public void HideLoadingScreen()
    {
        loadingScreenUI.SetActive(false); // Скрываем экран загрузки
    }

    public void LoadLevel(string levelName)
    {
        ShowLoadingScreen(); // Показываем экран загрузки
        StartCoroutine(LoadAsync(levelName)); // Запускаем загрузку уровня асинхронно
    }

    IEnumerator LoadAsync(string levelName)
    {
        AsyncOperation loadAsync = SceneManager.LoadSceneAsync(levelName); // Асинхронно загружаем уровень
        loadAsync.allowSceneActivation = false; // Загрузка уровня не будет активирована автоматически

        while (!loadAsync.isDone)
        {
            float progress = loadAsync.progress; // Получаем прогресс загрузки уровня
            if (progress < 0.9f)
            {
                // Загрузка ещё не завершена (меньше 90%)
                while (progress < 0.9f)
                {
                    progress += Time.deltaTime * 1f; // Увеличиваем прогресс загрузки плавно
                    loadingSlider.value = progress * 100; // Обновляем значение слайдера прогресса
                    loadingText.text = Mathf.FloorToInt(progress * 100).ToString() + "%"; // Обновляем текстовое поле с процентом загрузки
                    yield return null; // Даем системе время обработать следующий кадр
                }
            }
            else
            {
                // Загрузка завершена (достигнут 90%)
                loadingSlider.value = 100; // Устанавливаем значение слайдера на 100%
                loadingText.text = " 100%"; // Обновляем текстовое поле для отображения 100%
                yield return new WaitForSeconds(1f); // Пауза в 1 секунду, чтобы показать 100% загрузки
                loadAsync.allowSceneActivation = true; // Активируем загруженный уровень
            }
        }

        HideLoadingScreen(); // Скрываем экран загрузки, когда загрузка завершена
    }
}
