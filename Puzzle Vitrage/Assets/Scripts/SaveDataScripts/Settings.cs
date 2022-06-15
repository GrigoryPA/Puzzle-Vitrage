using UnityEngine;

//Пространство имен, в котором находится все для сохранений
namespace SaveData
{
    //Сериализуемый класс, описывающий текущие параметры настроек игры
    [System.Serializable]
    public class Settings
    {
        public int controlOption = 1; //типа управления (1 - 1 джойстик, 2 - 2 джойстика)
        public float graphicsOption = 1.0f; //коэф-т уменьшения качества разрашения изображения
        public float audioVolume = 1.0f; //уровень громкости звуков (1 - max, 0 - min)
    }
}
