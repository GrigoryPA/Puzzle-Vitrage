using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Данный класс содержит всю актуальную и ообщую информацию,
//которая может понадобиться другим классам и объектам игры
//Доступ к информации осуществляется через статичное поле того же типа что и класс
public class CurrentData
{
    //Общая информация (должна быть проинициализирована)
    public static CurrentData Data = new CurrentData();

    public bool isProgress = false; //включен ли прогресс
    public bool isBasicLevel = true; //запущен базовый уровень или пользвоательский
    public bool isSet = false; //актуальны ли значения класса
    public Mesh objectMesh; //Меш фигуры уровня
    public Material roomMaterial; //материал комнаты уровня
    public SaveData.Level currentLevel = new SaveData.Level(); //параметры выбранного уровня
    public static Color[] COLORS = {new Color(0.2f, 0, 0),
                                new Color(0.4f, 0, 0),
                                new Color(0.6f, 0, 0),
                                new Color(0.8f, 0, 0),
                                new Color(1.0f, 0, 0),
                                new Color(0, 0.2f, 0),
                                new Color(0, 0.4f, 0),
                                new Color(0, 0.6f, 0),
                                new Color(0, 0.8f, 0),
                                new Color(0, 1.0f, 0),
                                new Color(0, 0, 0.2f),
                                new Color(0, 0, 0.4f),
                                new Color(0, 0, 0.6f),
                                new Color(0, 0, 0.8f),
                                new Color(0, 0, 1.0f),
                                new Color(0.2f, 0.2f, 0),
                                new Color(0.2f, 0, 0.2f),
                                new Color(0, 0.2f, 0.2f),
                                new Color(0.4f, 0.4f, 0),
                                new Color(0.4f, 0, 0.4f),
                                new Color(0, 0.4f, 0.4f),
                                new Color(0.6f, 0.6f, 0),
                                new Color(0.6f, 0, 0.6f),
                                new Color(0, 0.6f, 0.6f),
                                new Color(0.04f, 0.04f, 0.04f),
                                new Color(0.1f, 0.1f, 0.1f),
                                new Color(0.2f, 0.2f, 0.2f),
                                new Color(0.3f, 0.3f, 0.3f)}; //Дискретные цвета для граней фигуры
    public SaveData.Progress currentProgress = new SaveData.Progress(); //текущий прогресс
    public SaveData.Settings currentSettings = new SaveData.Settings(); //текущие настройки
    public float currentLevelTime; //время в секундах завтраченное на прохождение текущего уровня

    //Пути к папкам с ресурсами мешей, материалов и базовых уровней
    private const string meshesRootPath = "ObjectMeshes/";
    private const string roomMaterialsRootPath = "RoomMaterials/";
    private const string basicLevelPath = "BasicLevels/";
    //Ключ к записи в реестре о прогрессе пользователя
    private const string progressKey = "PLAYER_PROGRESS_KEY";

    //Метод читает и дополняет информацию о выбранном базовом уровне
    //Параметры: имя базового уровня
    public bool ReadBasicLevel(string levelName)
    {
        try
        {
            //Считали уровень из ресурсов игры
            currentLevel = SaveManager.LoadTextAsset<SaveData.Level>(basicLevelPath + levelName);
            LoadMeshAndMaterial(); //поиск и чтение ресурсов материала и меша уровня
            isBasicLevel = true; //установка флага базового уровня

            return true; //уровень успешно прочитан
        }
        catch
        {
            return false; //уровень не был найден
        }
    }

    //Метод читает и дополняет информация о выбранном пользовательском уровне
    //Параметры: имя уровня
    public bool ReadСustomLevel(string levelName)
    {
        //Если файл с таким названием найден
        if (SaveManager.FindJson<SaveData.Level>(levelName))
        {
            //Загрузить данные пользовательского уровня
            currentLevel = SaveManager.LoadJson<SaveData.Level>(levelName); 
            LoadMeshAndMaterial(); //поиск и чтение ресурсов материала и меша уровня
            isBasicLevel = false; //установка флага пользовательского уровня

            return true; //успех
        }
        else
        {
            return false; //файл не был найден
        }
    }

    //Метод который загружает ресурсы материалов и мешей по их названиям
    //а также проверяет массив цветов гарней, на удовлетворение мешу
    public void LoadMeshAndMaterial()
    {
        //загрузка ресурсов матерала и меша по их названию и пути
        roomMaterial = Resources.Load<Material>(roomMaterialsRootPath + currentLevel.roomMaterialName);
        objectMesh = Resources.Load<Mesh>(meshesRootPath + currentLevel.objectMeshName);

        //определение количества цветов в массиве
        uint indexCount = objectMesh.GetIndexCount(0) / 3;
        //если не совпадает количество цветов и граней меша
        if (currentLevel.objectFacesColor.Length != indexCount)
        {
            //запоминаем старые цвета
            var bufObjectFacesColor = currentLevel.objectFacesColor;
            //инициализируем новые
            currentLevel.objectFacesColor = new Color[indexCount];
            //проходимся по массиву цветов
            for (uint i = 0; i < indexCount; i++)
            {
                //Если цвет для этой грани есть
                if (i < bufObjectFacesColor.Length)
                {
                    //использовать этот цвет
                    currentLevel.objectFacesColor[i] = bufObjectFacesColor[i];
                }
                //если для гарни цвета еще нет
                else
                {
                    //установить дефолтный цвет грани
                    currentLevel.objectFacesColor[i] = COLORS[COLORS.Length - 1];
                }
            }
        }
        isSet = true;//значение общей структуры актуальны
    }

    //Метод который читает значения прогресса из реестра
    //Параметры: количество базовых уровней
    public void ReadPlayerProgress(int numberLevels)
    {
        isProgress = true; //прогресс учитывается
        currentProgress = SaveManager.LoadPP<SaveData.Progress>(progressKey);//подгружаем прогресс из реестра
        bool found = currentProgress.completedLevels.Length != 0; //проверяем существовал ли до этого прогресс
        //если прогресс не был найден, выделяем память под него
        currentProgress.completedLevels = found ? currentProgress.completedLevels : new float[numberLevels];
    }

    //Метод обновляет и сохраняет текущие прогресс в прохождении уровней
    //Параметры: индекс обновляемого уровня
    public void UpdateAndSavePlayerProgress(int index)
    {
        //Если прогресс включен
        if (isProgress)
        {
            currentProgress.completedLevels[index] = currentLevelTime; //обновить время затраченное на проождение уровня
            SaveManager.SavePP<SaveData.Progress>(progressKey, currentProgress); //Сохраняем прогресс в реестре
        }
    }

    //Метод сбрасывает прогресс в прохождении уровней до стартового
    //Параметры: количество базовых уровней
    public void ResetPlayerProgress(int numberLevels)
    {
        currentProgress.completedLevels = new float[numberLevels]; //создать новый массив данных о пройденных уровнях
        SaveManager.SavePP<SaveData.Progress>(progressKey, currentProgress); //Сохраняем пустой прогресс в реестр
    }
}
