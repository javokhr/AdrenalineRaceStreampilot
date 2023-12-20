using UnityEngine;
using UnityEngine.UI;

public class GOTextManager : MonoBehaviour
{
    [SerializeField] private GameObject goText;

    private void Start()
    {
        //goText.gameObject.SetActive(true); // Начинаем с текста, который не активен        
        Invoke("DeactivateGOText", 1.0f); // Деактивируем текст через еще 1 секунду
    }

    private void DeactivateGOText()
    {
        //goText.gameObject.SetActive(false); // Деактивируем текст
    }
}
