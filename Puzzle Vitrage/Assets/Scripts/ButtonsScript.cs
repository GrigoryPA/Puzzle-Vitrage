using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

//Класс описывает методы обработки событий взаимодействия 
//с интерактивными объектами пользовательского интерфейса
//Большинство методов назначаются на события объектов
//при помощи графического редактора Unity
public class ButtonsScript : MonoBehaviour
{
    public string loadScene = ""; //Сцена которую необходимо загрузить
    public GameObject errorPanel; //окно ошибки (сообщения)

    //Метод при нажатии на кнопку загружает новую сцену вместо текущей
    //осуществляется переходи между экранами игры
    public void OnClickToLoadScene() 
    {
        SceneManager.LoadScene(loadScene, LoadSceneMode.Single);
    }

    //Метод при нажатии на кнопку загружает сцену с базовым уровнем
    public void OnClickToLoadBasicLevel()
    {
        //Если удалось успешно считать данные выбранного базового уровня
        if (CurrentData.Data.ReadBasicLevel(this.name))
        {
            //Загрузать сцену прохожденя базового уровня, вместо текущей сцены
            SceneManager.LoadScene(loadScene, LoadSceneMode.Single);
        }
        else
        {
            //Сообщение об ошибке пользователю
            errorPanel.GetComponentInChildren<Text>().text = "This level in the process of being created";
            errorPanel.SetActive(true);
        }
    }

    //Метод при нажатии на кнопку загружает сцену с пользовательским уровнем
    //Параметры: поле воода имени уровня
    public void OnClickToLoadCustomLevel(InputField levelNameInputField)
    {
        //если удалось прочитать данные пользовательского уровня
        if (CurrentData.Data.ReadСustomLevel(levelNameInputField.text))
        {
            //Запустить сцену с прохождением уровня вместо этой сцены
            SceneManager.LoadScene(loadScene, LoadSceneMode.Single);                                        
        }
        else
        {
            //Сообщение пользователю, то файл с уровней не был найден в определенной папке
            string errorMessage = "The file at the address ";
            //папка определяется платформой запускаЫ
#if UNITY_ANDROID && !UNITY_EDITOR
            errorMessage += Application.persistentDataPath;
#else
            errorMessage += Application.dataPath;
#endif
            errorMessage += " was not found";
            errorPanel.GetComponentInChildren<Text>().text = errorMessage;
            errorPanel.SetActive(true);
        }
    }

    //Метод при нажатии на кнопку сохраняет пользовательский уровень в файл
    //Параметры: полотно, к которому прикреплен главный класс режима создания уровней
    public void OnClickToSaveCustomLevel(Canvas canvas)
    {
        //Если не удалось сохранить уровень
        if (!canvas.GetComponent<CustomLevelCreator>().SaveCustomLevel())
        {
            //Неусешное сохранение
            errorPanel.GetComponentInChildren<Text>().text = "The level must have a name!";
            errorPanel.SetActive(true);
        }
        else 
        {
            //Успешное сохранение
            errorPanel.GetComponentInChildren<Text>().text = "Saving was successful.";
            errorPanel.SetActive(true);
        }
    }

    //Мтеод при нажатии на кнопку деактивирует родительский элемент кнопки
    //Параметры: родительский элемент объекта
    public void OnClickToInactivatingParentPanel(GameObject parentPanel)
    {
        parentPanel.SetActive(false); //деактивация родителя и себя
    }

    //Метод вызывается при изменении данных интерактивного объекта
    //Параметры: обхект, к которому приклеплен главный класс управляющий созданием уровня
    public void OnValueChanged(GameObject canvas)
    {
        //обновление настроек уровня
        canvas.GetComponent<CustomLevelCreator>().UpdateLevelSettings();
    }

    //Метод при нажатии на кнопку сбрасывает прогресс в прохождени уровней
    public void OnClickToRestartProgress()
    {
        CurrentData.Data.ResetPlayerProgress(0);
    }

    //Метод при нажатии меняет тип управления на противположный
    //Параметры: текстовый компонент кнопки
    public void OnClickToChangeRotation(Text buttonText)
    {
        //изменить тип управления на противоположный
        CurrentData.Data.currentSettings.controlOption = CurrentData.Data.currentSettings.controlOption % 2 + 1;
        //отобразитьтекущий выбор типа управления на надписи кнопки
        buttonText.text = CurrentData.Data.currentSettings.controlOption.ToString();
    }

    //Метод при нажатии меняет множитель уменьшения качества изображения в игре
    //Параметры: слайдер задающий значение множителя
    public void OnClickToChangeGraphic(Slider thisSlider)
    {
        CurrentData.Data.currentSettings.graphicsOption = thisSlider.value;
    }

    //Метод при нажатии на кнопку меняет факт наличия звука в игре на противоположный
    //Параметры: текстовый компонент кнопки
    public void OnClickToChangeSound(Text buttonText)
    {
        int length = buttonText.text.Length; 
        AudioListener.volume = buttonText.text.Contains("ON") ? 1 : 0;
        //отобразить текущее значение выбора на надписи кнопки
        buttonText.text = buttonText.text.Contains("ON") ? buttonText.text.Substring(0, length - 2) + "OFF" : buttonText.text.Substring(0, length - 3) + "ON";
        AudioListener.volume = buttonText.text.Contains("ON") ? 1 : 0;
        //применить настройку к парамерам звука в игре
        CurrentData.Data.currentSettings.audioVolume = AudioListener.volume;
    }
}
