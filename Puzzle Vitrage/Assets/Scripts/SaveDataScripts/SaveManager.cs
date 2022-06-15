using System.IO;
using UnityEngine;

//Класс предоставляющий методы для сохранения различных типов данных
public static class SaveManager
{
    //Метод сохраняющий данные в реестр по ключу
    //Параметры: ключб данные
    public static void SavePP<T>(string key, T saveData)
    {
        //Переводим данные в строку формата Json
        string jsonDataString = JsonUtility.ToJson(saveData, true);
        //сохраняем в реестр под опредлеенным ключом
        PlayerPrefs.SetString(key, jsonDataString);
    }

    //Методв для загрузки данных из реестра по ключу
    //Параметры: ключ
    public static T LoadPP<T>(string key) where T: new()
    {
        //Если в реестре есть запись под таким ключом
        if (PlayerPrefs.HasKey(key))
        {
            //Считываем данные по ключу из реестра
            string loadedString = PlayerPrefs.GetString(key);
            //Переводим строку json  в нужный формат данных 
            return JsonUtility.FromJson<T>(loadedString);
        }
        else 
        {
            //иначе пустой конструктор данных
            return new T();
        }
    }

    //Метод для сохранения данных в файл формата json 
    //Параметры: данные, имя файла для сохранения
    public static void SaveJson<T>(T saveData, string name)
    {
        string path; //путь к папке сохранения файла
        //формируем пусть к файлу с именем файла
#if UNITY_ANDROID && !UNITY_EDITOR 
        path = Path.Combine(Application.persistentDataPath, name + ".json"); //если запускается на анроиде
#else
        path = Path.Combine(Application.dataPath, name + ".json"); //если запускается на другой платформе или в среде разработки
#endif
        File.WriteAllText(path, JsonUtility.ToJson(saveData)); //записываем данные в файл строкой типа json
    }

    //Метод для чтения данных из файла типа json
    //Параметры: имя файла
    public static T LoadJson<T>(string name) where T : new()
    {
        string path;//полный путь к файлу
#if UNITY_ANDROID && !UNITY_EDITOR
        path = Path.Combine(Application.persistentDataPath, name + ".json"); //если на андроиде
#else
        path = Path.Combine(Application.dataPath, name + ".json");//если на другой платформе
#endif
        //Ищем файл с таким названием и путем
        if (File.Exists(path))
        {
            //считываем текст файла и переводим json обратно в формат данных
            return JsonUtility.FromJson<T>(File.ReadAllText(path)); 
        }
        else 
        {
            return new T(); //пустой конструктор если не нашли
        }
    }

    //Методв для поиска файла типа json по названию
    //Параметры: название файла
    public static bool FindJson<T>(string name)
    {
        string path;//полный путь к файлу
#if UNITY_ANDROID && !UNITY_EDITOR
        path = Path.Combine(Application.persistentDataPath, name + ".json"); //если андроид
#else
        path = Path.Combine(Application.dataPath, name + ".json"); //если другая платформа
#endif
        //Ищем файл по названию в нужной папке
        if (File.Exists(path))
        {
            return true;//если найден
        }
        else
        {
            return false;//если не найден
        }
    }

    //Метод для загрузки текстового ресурса игры
    //Параметры: имя файла
    public static T LoadTextAsset<T>(string name)
    {
        var levelTextAsset = Resources.Load<TextAsset>(name);//загрузка ресурса
        //получение текста из ресурса и переформатирование из json в нужный тип данных
        return JsonUtility.FromJson<T>(levelTextAsset.text);
    }

    //Метод для схранения данных в ресурсы игры
    //Параметры: данные, подпуть к файлу, имя файла
    public static void SaveTextAsset<T>(T saveData, string subpath, string name)
    {
        string path;//полный путь к файлу
        path = Path.Combine(Application.dataPath + subpath, name + ".json");//получаем нужный путь
        //сохраняем в нужное место данные ввиде файла формата jspn
        File.WriteAllText(path, JsonUtility.ToJson(saveData));
    }
}
