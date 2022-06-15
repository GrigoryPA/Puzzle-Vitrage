using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Класс призван управлять элементами интерфса меню настроек игры
//Следит за актуальностью отображаемой информации
public class GameSettingsVisualisator : MonoBehaviour
{
    public Button controlButton; //кнопка режима управления
    public Slider graphicSlider; //слайдер качества графики
    public Button audioButton; //кнопка управления звуком

    //Метод сарта, срабатывает непосредственно перед первым обновлением фрейма
    void Start()
    {
        //Получаем текстовые объекты кнопок
        Text controlText = controlButton.GetComponentInChildren<Text>();
        Text audioText = audioButton.GetComponentInChildren<Text>();

        //Устанавливаем значения обхектов в соответствии с текущими настроками
        controlText.text = CurrentData.Data.currentSettings.controlOption.ToString();
        graphicSlider.value = CurrentData.Data.currentSettings.graphicsOption;
        audioText.text = CurrentData.Data.currentSettings.audioVolume.Equals(1.0f) ?
            audioText.text.Substring(0, audioText.text.Length - 2) + "ON" : audioText.text.Substring(0, audioText.text.Length - 2) + "OFF";
    }
}
