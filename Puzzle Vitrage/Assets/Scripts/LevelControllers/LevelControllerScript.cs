using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

//Класс призван управлять всей логикой и обхектами игрового мира
//во время прохождения уровня
public class LevelControllerScript : MonoBehaviour
{
    public GameObject figure; //Фигуры
    public GameObject room; //комната
    public GameObject mainCamera; //главная камера
    public GameObject hint; //Подсказка
    public GameObject loadingScreen; //окно загружки уровня
    public GameObject timeCounter; //Счетчик
    public GameObject finishScreen; //Окно победы в уровне
    public Text result; //Поле результата прохождения уровня
    public Slider LevelProgressSlider; //Слайдер близости к финишу
    public Image LevelProgressSliderFill; //Изображение сладера блихости к финишу
    [Range(5, 30)]
    public int acceptableDelta = 10; //Расстояние до финиша в углах, когда включается автоматическая доводка
    [Range(0.0f, 1.0f)]
    public float smooth = 1.0f; //плавность автоматической доводки

    public Quaternion finishPosition; //финишная позиция для данного уровня
    private bool prepared = false; //Подготовитьельный этап уровня окончен
    private bool finished = false; //Уровень пройден

    //Метод сарта, срабатывает непосредственно перед первым обновлением фрейма
    private void Start()
    {
        loadingScreen.SetActive(true); //Активириуем окно загрузки
        finishScreen.SetActive(false); //Деактивируем окно финиша
        finishPosition = CurrentData.Data.currentLevel.finishPosition; //считываем финишную позицию

        //Настраиваем фигуры в соответствии параметрами уровня
        figure.GetComponent<MeshFilter>().sharedMesh = CurrentData.Data.objectMesh;
        figure.GetComponent<RayTracingObject>().facesColor = new int[CurrentData.Data.currentLevel.objectFacesColor.Length * 3];
        for (int i = 0; i < CurrentData.Data.currentLevel.objectFacesColor.Length; i++)
        {
            figure.GetComponent<RayTracingObject>().facesColor[3 * i] = (int)(255 * CurrentData.Data.currentLevel.objectFacesColor[i].r);
            figure.GetComponent<RayTracingObject>().facesColor[3 * i + 1] = (int)(255 * CurrentData.Data.currentLevel.objectFacesColor[i].g);
            figure.GetComponent<RayTracingObject>().facesColor[3 * i + 2] = (int)(255 * CurrentData.Data.currentLevel.objectFacesColor[i].b);
        }
        //Настраиваем комнату в соответствии с параметрами уровня
        room.GetComponent<MeshRenderer>().material = CurrentData.Data.roomMaterial;
        //Крутим фигуру в финишное положение, чтобы создать подсказу пользователю
        figure.transform.rotation = finishPosition;
        //Выставляем время затраченное на прохождение этого уровня в 0:0:00
        CurrentData.Data.currentLevelTime = 0.0f;
    }


    //Базовый метод, срабатывающий через одинаковые малые опредленные промежутки времени
    private void FixedUpdate()
    {
        //Подготовительный этап уровня, создание подсказки
        if (!prepared && !finished && mainCamera.GetComponent<Raytracer>().spriteHint != null)                          
        {
            //Получаем спрайт подсказки от класса управляющего рендерингом
            hint.GetComponent<Image>().sprite = mainCamera.GetComponent<Raytracer>().spriteHint;
            //вращаем фигуру в начальное положение
            figure.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            prepared = true; //Режим подготовки уровня окончен
            loadingScreen.SetActive(false); //Деактивация окна загрузки уровня
            timeCounter.GetComponent<TimeCounterScript>().StartTimer(); //Запускаме секундомер
        }

        //регулирование слайдера, показывающего близость к финишному положению фигуры
        LevelProgressSlider.value = 1 - (Quaternion.Angle(finishPosition, figure.transform.rotation) / 180);
        LevelProgressSliderFill.color = Color.Lerp(new Color(0.68f, 0.84f, 1.0f, 1.0f), 
                                                new Color(1.0f, 0.67f, 0.4f, 1.0f), 
                                                LevelProgressSlider.value);//интерполируем цвет

        //Если подготовка заверщена и угол между финишным положением и текущим положением меньше 1,
        //тогда финиш
        if (prepared && !finished && Quaternion.Angle(finishPosition, figure.transform.rotation) < 1)                                  
        {
            timeCounter.GetComponent<TimeCounterScript>().isStopped = true; //Останавливаем секундомер
            //запоминаем время затраченное на прохождение этого уровня
            CurrentData.Data.currentLevelTime = timeCounter.GetComponent<TimeCounterScript>().timer; 
            //Если прогресс учитывается и это базовый уровень
            if (CurrentData.Data.isProgress && CurrentData.Data.isBasicLevel)
            {
                //тогда обновляем и сохраняем данные прогресса 
                CurrentData.Data.UpdateAndSavePlayerProgress(int.Parse(CurrentData.Data.currentLevel.levelName.Split(' ')[1]) - 1);
            }
            CurrentData.Data.isSet = false; 
            GetComponent<AudioSource>().Stop();//останавливаем фоновую музыку в игре
            //Отображаем время затраченное на прохождение на экране
            result.text += timeCounter.GetComponent<TimeCounterScript>().ToString(CurrentData.Data.currentLevelTime);
            finishScreen.SetActive(true);//активируем окно финиша
            finished = true;//устанавливаем флаг окончания уровня
        }

        //Если подготовка завершена и угол меджу винишным и текущим положением меньше допустимой дельты,
        //тогда необходимо автоматически докрутить фигуру до финишного положения
        if (prepared && !finished && Quaternion.Angle(finishPosition, figure.transform.rotation) <= acceptableDelta) 
        {
            //вращаем интерполяцией из текущего опложения к инишному с определенной скоростью
            figure.transform.rotation = Quaternion.RotateTowards(figure.transform.rotation, finishPosition, smooth);
            //Отключаем возможность вращения фигуры, чтобы пользователь не мешал
            GameObject.Find("RotationBase").GetComponent<RotationScript>().isActive = false;
        }
    }
}
