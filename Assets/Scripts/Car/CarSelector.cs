using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CarData
{
    public string carName;           // Имя машины
    public int price;                // Цена машины
    public bool isPurchased;        // Флаг, указывающий, была ли машина куплена
    public GameObject carPrefab;     // Префаб машины
    public GameObject buyButton;     // Кнопка "Купить"
    public GameObject selectButton;  // Кнопка "Выбрать"
    public GameObject pricePanel;    // Панель с ценой
    public Text priceText;           // Текст с ценой
}

public class CarSelector : MonoBehaviour
{
    [SerializeField] private CarData[] carsData;  // Массив данных о машинах
    private GameManager gameManager;  // Менеджер игры
    [SerializeField] private GameObject prevButton;     // Кнопка "Предыдущая машина"
    [SerializeField] private GameObject nextButton;     // Кнопка "Следующая машина"
    [SerializeField] private Text carPriceText;          // Текст с ценой машины
    private int currentCarIndex;  // Индекс текущей выбранной машины

    private void Start()
    {
        LoadPurchasedStates(); // Load purchased states for all cars
        SelectCar(0);  // Начинаем с выбора первой машины при старте
        // Получаем экземпляр GameManager
        gameManager = GameManager.Instance;

        // Теперь у вас есть доступ к методам и полям GameManager через gameManager
        if (gameManager != null)
        {
            // Пример вызова метода GameManager
            int moneyToAdd = 1; // Например, добавим 500 денег
            gameManager.AddMoney(moneyToAdd); // Вызов метода AddMoney для добавления денег
            int moneyToSub = 1;
            gameManager.SubtractMoney(moneyToSub);
            gameManager.SearchTextMoney();
        }
        else
        {
            Debug.LogError("GameManager не был найден.");
        }
    }

    private void Update()
    {
        // Проверяем, была ли нажата клавиша "A"
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SwitchToPreviousCar(); // Вызываем функцию для переключения на предыдущую машину
        }

        // Проверяем, была ли нажата клавиша "D"
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            SwitchToNextCar(); // Вызываем функцию для переключения на следующую машину
        }
    }


    private void SelectCar(int index)
    {
        currentCarIndex = index;
        UpdateCarUI();
        prevButton.SetActive(index > 0);  // Показываем/скрываем кнопку "Предыдущая машина"
        nextButton.SetActive(index < carsData.Length - 1);  // Показываем/скрываем кнопку "Следующая машина"

        // Отключаем отображение всех машин
        foreach (CarData carData in carsData)
        {
            carData.carPrefab.SetActive(false);
        }

        // Включаем отображение выбранной машины
        carsData[currentCarIndex].carPrefab.SetActive(true);
    }

    // Button: Выбрать
    public void SelectButton()
    {
        PlayerPrefs.SetInt("SelectedCarIndex", currentCarIndex);
        PlayerPrefs.Save();
        SelectCar(currentCarIndex);  // Вызываем метод SelectCar для обновления UI
    }

    public void ChangeCar(int change)
    {
        int newIndex = currentCarIndex + change;

        if (newIndex >= 0 && newIndex < carsData.Length)
        {
            SelectCar(newIndex);  // Вызываем метод SelectCar для смены машины
        }
    }

    public void BuyCar()
    {
        CarData selectedCar = carsData[currentCarIndex];

        if (!selectedCar.isPurchased && gameManager.HasEnoughMoney(selectedCar.price))
        {
            gameManager.SubtractMoney(selectedCar.price);
            selectedCar.isPurchased = true;

            // Save the purchased state in PlayerPrefs
            PlayerPrefs.SetInt("CarPurchased_" + selectedCar.carName, 1);
            PlayerPrefs.Save();

            UpdateCarUI();  // Update the UI for the current car
        }
    }

    private void UpdateCarUI()
    {
        CarData selectedCar = carsData[currentCarIndex];

        if (selectedCar.isPurchased)
        {
            // Если машина куплена, скрываем кнопку "Купить", панель с ценой и активируем кнопку "Выбрать"
            selectedCar.buyButton.gameObject.SetActive(false);
            selectedCar.pricePanel.gameObject.SetActive(false);
            selectedCar.selectButton.gameObject.SetActive(true);
        }
        else
        {
            // Если машина не куплена, активируем кнопку "Купить", панель с ценой и скрываем кнопку "Выбрать"
            selectedCar.buyButton.gameObject.SetActive(true);
            selectedCar.pricePanel.gameObject.SetActive(true);
            selectedCar.selectButton.gameObject.SetActive(false);

            // Форматируем текст цены с добавлением пробелов и символа "$"
            selectedCar.priceText.text = string.Format("${0:n0}", selectedCar.price);
        }
    }
    private void LoadPurchasedStates()
    {
        foreach (CarData carData in carsData)
        {
            int purchased = PlayerPrefs.GetInt("CarPurchased_" + carData.carName, 0);
            carData.isPurchased = purchased == 1;
        }
    }


    public void SwitchToPreviousCar()
    {
        ChangeCar(-1);  // Переключаемся на предыдущую машину
    }

    public void SwitchToNextCar()
    {
        ChangeCar(1);   // Переключаемся на следующую машину
    }
}
