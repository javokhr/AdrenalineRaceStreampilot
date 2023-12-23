using GamePush;
using UnityEngine;
public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }
    public void Preloader() => GP_Ads.ShowPreloader();
    public void Fullscreen() => GP_Ads.ShowFullscreen();
    private void Start()
    {
        if (!GP_Device.IsMobile())
        {
            Sticky();
        }
    }

    private void OnEnable()
    {
        GP_Game.OnPause += Pause;
        GP_Game.OnResume += Resume;
        GP_Ads.OnRewardedReward += OnRewarded;
    }

    private void OnDisable()
    {
        GP_Game.OnPause -= Pause;
        GP_Game.OnResume -= Resume;
        GP_Ads.OnRewardedReward -= OnRewarded;
    }

    private void Resume()
    {
        Time.timeScale = 1;
        // Установите состояние AudioListener
        AudioListener.pause = false;
        if (PlayerPrefs.GetInt("SoundOn", 1) == 1)
        {
            // Включаем звук
            AudioListener.volume = 1;
        }
    }

    private void Pause()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
        AudioListener.volume = 0;
    }

    private void Sticky()
    {
        // Показать стики баннер
        GP_Ads.ShowSticky();

        // Закрыть стики баннер
        GP_Ads.CloseSticky();

        // Обновить стики баннер
        GP_Ads.RefreshSticky();

        // Стики баннеры обновляются автоматически раз в 30 секунд.
    }

    // Метод для показа вознагражденной рекламы
    public void Rewarded() => GP_Ads.ShowRewarded("Money_Bonus");

    // Метод, вызываемый при получении вознаграждения за просмотр рекламы
    private void OnRewarded(string value)
    {
        if (value == "Money_Bonus")
        { // Получаем экземпляр GameManager
            GameManager gameManager = GameManager.Instance;

            if (gameManager != null)
            {

                int moneyToAdd = 2000; // Например, добавим 500 денег
                gameManager.AddMoney(moneyToAdd); // Вызов метода AddMoney для добавления денег
            }
            else
            {
                Debug.LogError("GameManager не найден.");
            }

        }
    }

    public void MoreGames()
    {
        // By custom TAG
        //GP_GamesCollections.Open({ tag: 'OurGames' });

        // By ID

        //GP_GamesCollections.Open("219");
        GP_GamesCollections.Open("243");
    }
}
