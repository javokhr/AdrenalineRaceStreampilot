using UnityEngine;
using UnityEngine.UI;
//using Lean.Localization;
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel; // Панель настроек
    [SerializeField] private GameObject carSelectionPanel; // Панель выбора автомобиля
    [SerializeField] private GameObject carScreen; // Экран с информацией об автомобиле
    [SerializeField] private GameObject mapSelectionPanel; // Панель выбора карты
    [SerializeField] private GameObject adRewardPanel; // Панель с вознаграждением за просмотр рекламы
                                                       //[SerializeField] private GameObject moreGamesPanel;
                                                       //[SerializeField] LeanLocalization _local;
    /*
        private void Start()
        {
            MoreGamesPanel();
        }
        */
    // Методы для открытия и закрытия панелей настроек, выбора автомобиля, информации об автомобиле, выбора карты и панели с вознаграждением за просмотр рекламы
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void OpenCarSelection()
    {
        carSelectionPanel.SetActive(true);
    }

    public void CloseCarSelection()
    {
        carSelectionPanel.SetActive(false);
    }

    public void OpenCarScreen()
    {
        carScreen.SetActive(true);
    }

    public void CloseCarScreen()
    {
        carScreen.SetActive(false);
    }

    public void OpenMapSelection()
    {
        mapSelectionPanel.SetActive(true);
    }

    public void CloseMapSelection()
    {
        mapSelectionPanel.SetActive(false);
    }

    public void OpenRewardPanel()
    {
        adRewardPanel.SetActive(true);
    }

    public void CloseRewardPanel()
    {
        adRewardPanel.SetActive(false);
    }
    /*
        public void MoreGamesPanel()
        {
            if (_local.CurrentLanguage == "Russian" || _local.CurrentLanguage == "Turkish")
            {
                moreGamesPanel.SetActive(true);
            }
            else
            {
                moreGamesPanel.SetActive(false);
            }
        }
    */
}
