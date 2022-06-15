using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Класс реализует поведение секундомера
public class TimeCounterScript : MonoBehaviour
{
    //Текстовый объект пользовательского интерфейса для отображения результата
    public Text text; 
    private int min = 0; //кол-во минут
    private int sec = 0; //кол-во секунд
    private int msec = 0; //кол-вр миллисекунд
    public float timer = 0; //общее время в миллисекундах о момента запуска
    public bool isStopped = false; //флаг остановки

    //Метод запускает секундомер инициализируя все значения нулями
    public void StartTimer()
    {
        timer = 0;
        msec = 0;
        sec = 0;
        min = 0;
    }

    //Метод приостанавливает работу секундомера
    public void Pause()
    {
        isStopped = true;
    }

    //метод запускает работу секундомера
    public void Play()
    {
        isStopped = false;
    }

    //Метод вызывается каждое обновление фрейма экрана
    void Update()
    {
        //увеличить общий счетчик миллисекунд
        timer += isStopped ? 0 : Time.deltaTime;
        msec = (int)(timer*1000 % 1000); //расчет мс
        sec = (int)(timer % 60); //расчет с
        min = (int)(timer/60); //расчет мин
        //формируем строковое значение секундомера
        text.text = min.ToString("D2") + ":" + sec.ToString("D2");
    }

    //Метод формирует строкове текущее значение секундомера
    public string ToString(float time)
    {
        int _msec = (int)(time * 1000 % 1000);
        int _sec = (int)(time % 60);
        int _min = (int)(time / 60);
        return _min.ToString("D2") + ":" + _sec.ToString("D2") + ":" + _msec.ToString("D3");
    }
}
