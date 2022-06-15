using UnityEngine;
using System.IO;

//Класс который сопутствует всем объектам, которые участуют в рейтрейсинге
//При добавлени такого обхекта на сцену, заставляет зарегестрировать себя
//и передает все свои параметры классу RayTracer
//Расширяет базовые параметры обхекта сцены прозрачностью и цветами полигонов
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RayTracingObject : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float transparency = 0.0f;//прозрачность объекта
    public int[] facesColor;//цвета граней объекта

    //Функция вызывается после включения обхекта на сцене
    private void OnEnable()
    {
        //определение размера массива цветов
        uint indexCount = GetComponent<MeshFilter>().sharedMesh.GetIndexCount(0);
        //предварительная инициализация всех цветов белым цветом
        facesColor = new int[indexCount];
        for (uint i = 0; i < indexCount; i++)
        {
            facesColor[i] = 255;
        }
        //регистрация обхекта среди обхектов рейтрейсинга
        Raytracer.RegisterObject(this);
    }

    //Функция срабатывает когда объект становится неактивынм
    private void OnDisable()
    {
        //освобождение памяти выделенной под обхект рейстрейсинга
        Raytracer.UnregisterObject(this);
    }
}
