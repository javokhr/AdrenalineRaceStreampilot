using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LevelData
{
    public string levelName;     // Название уровня
    public int costToUnlock;    // Стоимость для разблокировки уровня
    public GameObject lockIcon; // Ссылка на объект LockIcon
    public bool unlocked;      // Открыт ли уровень
}

[Serializable]
public class LevelSaveData
{
    public List<LevelData> levelsData;
}

public class LevelManagerScript : MonoBehaviour
{
    [SerializeField] private List<LevelData> levelsData; // Список данных об уровнях

    private void Start()
    {
        LoadLevelData(); // Загрузка данных об уровнях

        // Проверяем каждый уровень и скрываем LockIcon, если уровень разблокирован
        foreach (var levelData in levelsData)
        {
            if (levelData.unlocked)
            {
                // Скрываем LockIcon, если он существует
                GameObject lockIcon = levelData.lockIcon;
                if (lockIcon != null)
                {
                    lockIcon.SetActive(false);
                }
            }
        }
    }


    // Метод для загрузки данных об уровнях из PlayerPrefs
    private void LoadLevelData()
    {
        for (int i = 0; i < levelsData.Count; i++)
        {
            int unlockedValue = PlayerPrefs.GetInt("Level_" + i + "_Unlocked", 0);
            levelsData[i].unlocked = (unlockedValue == 1);
        }
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelsData.Count)
        {
            return levelsData[levelIndex].unlocked;
        }
        return false;
    }
    public void TryToLoadUnlockedLevel(int levelIndex)
    {
        if (IsLevelUnlocked(levelIndex))
        {
            string levelName = levelsData[levelIndex].levelName;
            LoadingScreenController.instance.LoadLevel(levelName); // Вызываем метод загрузки уровня из LoadingScreenController
        }
        else
        {
            Debug.Log("Уровень не разблокирован.");
        }
    }
    public void TryToUnlockLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelsData.Count)
        {
            if (!IsLevelUnlocked(levelIndex))
            {
                // Получите количество денег игрока через GameManager
                GameManager gameManager = GameManager.Instance;
                int playerMoney = gameManager.GetPlayerData().money;

                int costToUnlock = levelsData[levelIndex].costToUnlock;

                if (playerMoney >= costToUnlock)
                {
                    // Вычитаем стоимость разблокировки из денег игрока
                    playerMoney -= costToUnlock;

                    // Скрываем LockIcon, если он существует
                    GameObject lockIcon = levelsData[levelIndex].lockIcon;
                    if (lockIcon != null)
                    {
                        lockIcon.SetActive(false);
                    }

                    levelsData[levelIndex].unlocked = true;

                    // Сохраняем данные об уровнях
                    SaveLevelData();

                    // Обновляем данные о деньгах игрока
                    gameManager.GetPlayerData().money = playerMoney;
                    gameManager.UpdateMoneyUI();
                    string levelName = levelsData[levelIndex].levelName;
                    SceneManager.LoadScene(levelName);
                }
                else
                {
                    Debug.Log("Недостаточно денег для разблокировки уровня.");
                    return; // Если недостаточно денег, не продолжаем разблокировку уровня
                }
            }
            else
            {
                Debug.Log("Уровень уже разблокирован.");
            }
        }
        else
        {
            Debug.Log("Недопустимый индекс уровня.");
        }
    }



    // Метод для сохранения данных об уровнях в PlayerPrefs
    private void SaveLevelData()
    {
        for (int i = 0; i < levelsData.Count; i++)
        {
            PlayerPrefs.SetInt("Level_" + i + "_Unlocked", levelsData[i].unlocked ? 1 : 0);
        }
        PlayerPrefs.Save();
    }
}
