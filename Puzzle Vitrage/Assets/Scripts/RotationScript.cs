using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Класс оттвечающи за вращение объекта
//которое зависит от управления джойстиками
//Данный класс является компонентом объекта,
//который является родителем для фигуры игрового мира
//Что дает возможность вращать всегда вокруг нужных осей вращения
public class RotationScript : MonoBehaviour
{
    //максимальугол отклонения фигуры, при максимальной отклонении дойстика
    public float rotationRange = 50.0f; 
    //шаг дискретного поворота фигуры
    public float rotationStep = 5.0f;
    public FloatingJoystick rotateJoystick; //Двунаправленный джостик, для 1го типа управления
    public FloatingJoystick rotateJoystickVertical; //вертикальный джойстик для 2го типа управления
    public FloatingJoystick rotateJoystickHorizontal; //горизонтальный джойстик для 2го типа управления

    private Quaternion currentState = Quaternion.identity; //текущее положения фигуры
    private Quaternion finalQ; //финальное положение фигуры
    private Transform child; //компонент положения в пространстве обхекта вращения
    [SerializeField]
    public bool isActive = true; //активизировано ли управление вращением

    //Метод сарта, срабатывает непосредственно перед первым обновлением фрейма
    private void Start()
    {
        //если первый тип правления
        if (CurrentData.Data.currentSettings.controlOption == 1)
        {
            //не используем однонаправленные джойстики
            rotateJoystickHorizontal.gameObject.SetActive(false); 
            rotateJoystickVertical.gameObject.SetActive(false);
            //используем двунаправленный
            rotateJoystick.gameObject.SetActive(true);
        }
        else
        {
            //если второй тип вращения
            //используем два однонаправленных джойстика
            rotateJoystickHorizontal.gameObject.SetActive(true);
            rotateJoystickVertical.gameObject.SetActive(true);
            //не используем двунаправленный джойстик
            rotateJoystick.gameObject.SetActive(false);
        }
    }

    //Базовый метод, срабатывающий через одинаковые малые опредленные промежутки времени
    void FixedUpdate()
    {
        //инициализируем отклонение джостика относительно его изначального положения по двум координатам
        float joystickX, joystickY; //в зависиомти от типа управления
        joystickX = CurrentData.Data.currentSettings.controlOption == 1 ? rotateJoystick.Direction.x : rotateJoystickHorizontal.Direction.x;
        joystickY = CurrentData.Data.currentSettings.controlOption == 1 ? rotateJoystick.Direction.y : rotateJoystickVertical.Direction.y;

        child = transform.GetChild(0); //получаем положние фигуры в пространстве
        //если управление вращением отключено или джостик не задействован пользователем
        if (joystickY == 0.0f && joystickX == 0.0f || isActive == false)
        {
            //Запомнить в перменную положение фигуры
            currentState = child.rotation;  
        }
        else
        {
            Quaternion xQ = Quaternion.AngleAxis(joystickY * rotationRange, Vector3.right); //получаем поворот по оси икс
            Quaternion yQ = Quaternion.AngleAxis(-joystickX * rotationRange, Vector3.up); //получаем поворот по оси игрек
            finalQ = xQ * yQ * currentState; //перемножаем кватернионы и последнее положение фигуры, получаем текущее положение
            finalQ = Quaternion.Euler(RoundingModulo(finalQ.eulerAngles.x, rotationStep),
                                    RoundingModulo(finalQ.eulerAngles.y, rotationStep),
                                    RoundingModulo(finalQ.eulerAngles.z, rotationStep)); //берем вращение по модулю шага дискретизации
            child.rotation = finalQ; //применяем вращение к фигуре
        }
    }

    //Метод принимает значение и основание модуля
    //возвращает значение по модулю
    private float RoundingModulo(float x, float m)
    {
        return x - x % m; 
    }
}
