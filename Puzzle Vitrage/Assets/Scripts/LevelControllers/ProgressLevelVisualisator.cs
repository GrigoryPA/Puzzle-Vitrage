using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Класс который призван управлять обхектами меню выбора уровня
//Следит за прогрессом в прохождении уровней и делает активными опредленные кнопки
public class ProgressLevelVisualisator : MonoBehaviour
{
    public bool isProgress = false; //отладочное поле отключающее учет прогресса

    //Унаследованный метод сарта, срабатывает непосредственно перед первым обновлением фреймв
    void Start()
    {
        //получаем все кнопки базовых уровней на сцене
        Button[] buttons = this.GetComponentsInChildren<Button>(true);
        int numberButtons = buttons.Length;

        //если прогресс включен
        if (isProgress)
        {
            //считываем прогресс из реестра в статчиный класс общих текущих данных
            CurrentData.Data.ReadPlayerProgress(buttons.Length);

            //проходимся по всем кнопкам базовых уровней
            for (int i = 1; i < numberButtons; i++)  
            {
                //Управляем интерактивностью кнопки, в зависимости от пройденности предыдущего уровня
                buttons[i].interactable = CurrentData.Data.currentProgress.completedLevels[i - 1] != 0;
            }
        }
        else
        {
            //если нет прогресса, то сбрасываем весь текущий прогресс уровней
            CurrentData.Data.ResetPlayerProgress(buttons.Length);
        }
    }
}
