using UnityEngine;

//Структура предназначена для хранения дополнительной
//информации о мешах объектов, поскольку основная информация,
//вершины и полигоны, передаются через буферы
struct MeshRT
{
    public Matrix4x4 localToWorldMatrix; //матрица преобрзования
    public int indices_offset; //место записи индексов в буфере для обхекта
    public int indices_count; //количество индексов в буфере объекта
    public float transparency; //прозрачность объекта

    //метод который возвращает размер структуры
    public static int GetSize()
    {
        return sizeof(float)*4*4
            + sizeof(int)
            + sizeof(int)
            + sizeof(float);
    }
}

