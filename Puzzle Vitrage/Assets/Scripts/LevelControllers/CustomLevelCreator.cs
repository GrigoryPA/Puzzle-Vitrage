using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Класс предназначен для управения игровым миром и 
//элементами пользовательского интерфеса на экране режимо создания уровня
public class CustomLevelCreator : MonoBehaviour
{
    //Режим разработчика (создание и сохранение базовых уровней в папку ресурсов игры)
    public bool isDeveloperMode = false; 
    public InputField levelNameInputField;//поле вода названия уровня
    public Dropdown roomDropdown;//выпадаюий список материалов комнаты
    public Dropdown figureDropdown;//выпадающиъ список форм фигуры
    public Dropdown facesDropdown; //выпаающий список граней фигуры
    public Dropdown colorDropdown; //выпадающий список цветов грани
    public GameObject figure; //Фигура
    public GameObject room; //Комната
    public GameObject mainCamera; //камера, которая занимается рендерингом 
    public GameObject loadingScreen; //Окно загрузки уровня

    private int indexSelectedFace = -1; //индекс выбранной грани
    private SaveData.Level newLevel = new SaveData.Level(); //новый уровень

    //Метод сарта, срабатывает непосредственно перед первым обновлением фрейма
    void Start()
    {
        loadingScreen.SetActive(true); //вкл экран загрузки
        InitializingUI(); //установить данные нового уровня как стартовые
        loadingScreen.SetActive(false); //выкл экран загрузки
    }

    //Метод сохраняет уровень с текущими настройками в файл
    public bool SaveCustomLevel()
    {
        UpdateLevelSettings(); //Обновить параметры уровня
        //Если им уровня не пустое
        if (newLevel.levelName.Length != 0)
        {
            //если это режим разраотчика
            if (isDeveloperMode)
            {
#if UNITY_EDITOR
                //сохранить уровенб в ввиде ресурса базового уровня в папку с ресурсами игры
                SaveManager.SaveTextAsset<SaveData.Level>(newLevel, "/Levels/Resources/BasicLevels/", newLevel.levelName);
#else
                return false; //если это не среда разработки, тогда ошибка
#endif
            }
            //если это обычный режим создания
            else
            {
                //сохранить уровень в виде файла расширения json в файловой системе платформы
                SaveManager.SaveJson<SaveData.Level>(newLevel, newLevel.levelName);
            }
            return true; //успех
        }
        else 
        {
            return false; //неуспех
        }
    }

    //Метод который проиводит стартовую инициализацию настроек уровня
    //и применяет их к игровому миру
    public void InitializingUI()                                                
    {
        newLevel.levelName = levelNameInputField.text; //название уровня из поля ввода текста
        newLevel.roomMaterialName = roomDropdown.captionText.text; //материал комнаты
        newLevel.finishPosition = figure.transform.rotation; //финишна позиция
        newLevel.objectMeshName = figureDropdown.captionText.text; //меш главной фигуры
        SetOptionsColorDropdown(); //инициализируем значения выпадающего списка цветов

        ApplyLevelSettings(); //применить текущие данные уровня к сцене
    }

    //Метод обновляет параметры уровня и применяет их к игровому миру
    public void UpdateLevelSettings()                                          
    {
        newLevel.levelName = levelNameInputField.text; //название уровня
        newLevel.roomMaterialName = roomDropdown.captionText.text; //материал комнаты
        newLevel.finishPosition = figure.transform.rotation; //финишна позиция
        //если была выбрана друга форма фигуры
        if (newLevel.objectMeshName != figureDropdown.captionText.text) 
        {
            newLevel.objectMeshName = figureDropdown.captionText.text; //применить к настройкам уровня
            facesDropdown.value = 0; //установить значение выбранной грани фигуры на 0
        }
        //если была выбрана другая грань фигуры
        if (facesDropdown.value != indexSelectedFace) 
        {
            indexSelectedFace = facesDropdown.value; //применить к настройкам уровня
            //вспомнить какой цвет у данной грани для обновления выпадающего списка цветов
            RememberColorDropdown(); 
        }
        UpdateFaceColor(); //обновить цвет грани в соответствии с выпадающим списком цветов
        ApplyLevelSettings(); //применить текущие данные уровня к сцене
    }

    //Метод применяет текущие параетры уровня к игровому миру
    private void ApplyLevelSettings()                                        
    {
        CurrentData.Data.currentLevel = newLevel; //Теущий уровень равен новому
        CurrentData.Data.LoadMeshAndMaterial(); //Прочитать данные указанных названий материалов и мешей

        room.GetComponent<MeshRenderer>().material = CurrentData.Data.roomMaterial; //Применить материал комнаты

        //инициализируем массив цветов граней фигуры
        figure.GetComponent<RayTracingObject>().facesColor = new int[CurrentData.Data.currentLevel.objectFacesColor.Length * 3];
        //применяем цвета параметров уровня к фигуре
        for (int i = 0; i < CurrentData.Data.currentLevel.objectFacesColor.Length; i++)
        {
            figure.GetComponent<RayTracingObject>().facesColor[3 * i] = (int)(255 * CurrentData.Data.currentLevel.objectFacesColor[i].r);
            figure.GetComponent<RayTracingObject>().facesColor[3 * i + 1] = (int)(255 * CurrentData.Data.currentLevel.objectFacesColor[i].g);
            figure.GetComponent<RayTracingObject>().facesColor[3 * i + 2] = (int)(255 * CurrentData.Data.currentLevel.objectFacesColor[i].b);
        }

        //если меш фигуры не соответствует мешу уровня
        if (figure.GetComponent<MeshFilter>().sharedMesh != CurrentData.Data.objectMesh)
        {
            //обновить меш фигуры как в уровне
            figure.GetComponent<MeshFilter>().sharedMesh = CurrentData.Data.objectMesh;
            UpdateFacesDropdown(); //обновить выпадающий список граней под новый меш
        }
    }

    //Метод обновляет выпадающий список граней в соответствии выбранному мешу фигуры
    public void UpdateFacesDropdown()
    {
        uint sizeList = figure.GetComponent<MeshFilter>().sharedMesh.GetIndexCount(0) / 3; //количество граней меша
        List<Dropdown.OptionData> newFacesList = new List<Dropdown.OptionData>((int)sizeList); //Список опций выпадающего списка
        for (int i = 1; i <= sizeList; i++) //проходимся по всему списку
        {
            newFacesList.Add(new Dropdown.OptionData(i.ToString())); //добавляем инфу о грани фигуры в список
        }
        facesDropdown.options = newFacesList; //Применяем новые опции к выпадающему списку
        indexSelectedFace = facesDropdown.value; //текущее значение списка - выбранная грань
        RememberColorDropdown(); //обновляем выбранное значение выпадающего списка цветов к новой грани
    }

    //Метод определяет какой цвет необходимо установить для выпадающего списка цветов граней
    public void RememberColorDropdown()
    {
        //Указываем ренедерингу какую грань фигуры подсветить
        mainCamera.GetComponent<Raytracer>().selectedFace = indexSelectedFace * 3; 

        int i = 0; //инжекс выбранной грани в пределах их количества
        if (indexSelectedFace < newLevel.objectFacesColor.Length)
        {
            //ищем совпадение цвета выбранной грани и цвета в выпадающем списке
            for (i = CurrentData.COLORS.Length - 1; i >= 0 &&
                !CurrentData.COLORS[i].Equals(newLevel.objectFacesColor[indexSelectedFace]); i--)
            { }
        }
        colorDropdown.value = i; //выставляем найденное значение цвета
    }

    //Метод обновляет цвет грани фигуры в соответствии с выборов выпадающего списка цветов
    public void UpdateFaceColor()
    {
        newLevel.objectFacesColor[indexSelectedFace] = CurrentData.COLORS[colorDropdown.value];
    }

    //Метод устанавливает значения выпадающего списка цветов
    public void SetOptionsColorDropdown()
    {
        int sizeList = CurrentData.COLORS.Length; //количество дискретных цветов
        //Инициализируем список опций
        List<Dropdown.OptionData> ColorList = new List<Dropdown.OptionData>(sizeList);
        for (int i = 1; i < sizeList; i++)
        {
            //Создаем картинку соответствующего цвета и назначаем
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, CurrentData.COLORS[i - 1]);
            texture.Apply();
            Sprite image = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.0f, 0.0f));
            ColorList.Add(new Dropdown.OptionData(i.ToString(), image));
        }
        //применяем опции к выпадающему списку
        colorDropdown.options = ColorList;
    }
}
