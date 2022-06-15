using UnityEngine;

//ѕространство имен, в котором находитс€ все дл€ сохранений
namespace SaveData
{
    //—ериализуемый класс, описывающий параметры уровн€ дл€ его сохранени€ в файл
    [System.Serializable]
    public class Level
    {
        public string levelName; //Ќазвание уровн€ и файла
        public string roomMaterialName = "RoomMat1"; //название файла материала комнаты
        public Quaternion finishPosition = Quaternion.Euler(new Vector3(30.0f, 30.0f, 30.0f)); //финишное положение фигуры
        public string objectMeshName = "CUBE"; //название файла меша фигуры
        public Color[] objectFacesColor = { }; //массив цветов граней фигуры
    }
}
