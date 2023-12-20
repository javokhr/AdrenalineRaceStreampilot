using UnityEngine;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public class PlayerData
{
    public int money;

    public PlayerData()
    {
        // Ничего не делаем здесь
    }

    public void InitializeMoney()
    {
        // Проверяем, существует ли значение "PlayerMoney" в PlayerPrefs
        if (PlayerPrefs.HasKey("PlayerMoney"))
        {
            // Если есть, читаем его
            money = PlayerPrefs.GetInt("PlayerMoney");
        }
        else
        {
            // Если файл не существует, устанавливаем начальное значение в 20000 и сохраняем его
            money = 15000;
            SavePlayerData();
        }
    }

    // Метод для сохранения данных в файл
    public void SavePlayerData()
    {
        PlayerPrefs.SetInt("PlayerMoney", money);
        PlayerPrefs.Save();
    }
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private Text moneyText; // Текст для отображения денег в UI
    public static GameManager Instance { get; private set; }

    private PlayerData playerData = new PlayerData();

    private void Awake()
    {
        playerData.InitializeMoney();
        // Если экземпляр GameManager ещё не существует
        if (Instance == null)
        {
            // Сделайте текущий объект (this) единственным экземпляром
            Instance = this;

            // Сохраните этот объект при переходе между сценами
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager создан.");
        }
        else
        {
            // Если экземпляр GameManager уже существует (дубликат)
            if (Instance != this)
            {
                // Уничтожьте дубликат
                Destroy(gameObject);
                Debug.Log("Дубликат GameManager уничтожен.");
            }
        }
    }
    /*
        private void Start()
        {
            //PlayerPrefs.DeleteAll();
            UpdateMoneyUI();
        }
    */
    public PlayerData GetPlayerData()
    {
        return playerData;
    }

    public bool HasEnoughMoney(int amount)
    {
        return playerData.money >= amount;
    }

    public void SubtractMoney(int amount)
    {
        playerData.money -= amount;
        UpdateMoneyUI();
        playerData.SavePlayerData(); // Сохраняем измененные данные
    }

    public void AddMoney(int amount)
    {
        playerData.money += amount;
        UpdateMoneyUI();
        playerData.SavePlayerData(); // Сохраняем измененные данные
    }

    public void SearchForMoneyText()
    {
        SearchTextMoney();
        UpdateMoneyUI();
    }

    public void UpdateMoneyUI()
    {
        SearchTextMoney();
        if (moneyText != null)
        {
            moneyText.text = string.Format("{0:N0}", playerData.money);
        }
    }

    public void SearchTextMoney()
    {
        GameObject Text_Money = GameObject.Find("Text_Money");
        moneyText = Text_Money.GetComponent<Text>();
    }
}
