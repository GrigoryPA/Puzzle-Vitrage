using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//����� ���������� �� �������� �������
//������� ������� �� ���������� �����������
//������ ����� �������� ����������� �������,
//������� �������� ��������� ��� ������ �������� ����
//��� ���� ����������� ������� ������ ������ ������ ���� ��������
public class RotationScript : MonoBehaviour
{
    //������������� ���������� ������, ��� ������������ ���������� ��������
    public float rotationRange = 50.0f; 
    //��� ����������� �������� ������
    public float rotationStep = 5.0f;
    public FloatingJoystick rotateJoystick; //��������������� �������, ��� 1�� ���� ����������
    public FloatingJoystick rotateJoystickVertical; //������������ �������� ��� 2�� ���� ����������
    public FloatingJoystick rotateJoystickHorizontal; //�������������� �������� ��� 2�� ���� ����������

    private Quaternion currentState = Quaternion.identity; //������� ��������� ������
    private Quaternion finalQ; //��������� ��������� ������
    private Transform child; //��������� ��������� � ������������ ������� ��������
    [SerializeField]
    public bool isActive = true; //�������������� �� ���������� ���������

    //����� �����, ����������� ��������������� ����� ������ ����������� ������
    private void Start()
    {
        //���� ������ ��� ���������
        if (CurrentData.Data.currentSettings.controlOption == 1)
        {
            //�� ���������� ���������������� ���������
            rotateJoystickHorizontal.gameObject.SetActive(false); 
            rotateJoystickVertical.gameObject.SetActive(false);
            //���������� ���������������
            rotateJoystick.gameObject.SetActive(true);
        }
        else
        {
            //���� ������ ��� ��������
            //���������� ��� ���������������� ���������
            rotateJoystickHorizontal.gameObject.SetActive(true);
            rotateJoystickVertical.gameObject.SetActive(true);
            //�� ���������� ��������������� ��������
            rotateJoystick.gameObject.SetActive(false);
        }
    }

    //������� �����, ������������� ����� ���������� ����� ����������� ���������� �������
    void FixedUpdate()
    {
        //�������������� ���������� �������� ������������ ��� ������������ ��������� �� ���� �����������
        float joystickX, joystickY; //� ���������� �� ���� ����������
        joystickX = CurrentData.Data.currentSettings.controlOption == 1 ? rotateJoystick.Direction.x : rotateJoystickHorizontal.Direction.x;
        joystickY = CurrentData.Data.currentSettings.controlOption == 1 ? rotateJoystick.Direction.y : rotateJoystickVertical.Direction.y;

        child = transform.GetChild(0); //�������� �������� ������ � ������������
        //���� ���������� ��������� ��������� ��� ������� �� ������������ �������������
        if (joystickY == 0.0f && joystickX == 0.0f || isActive == false)
        {
            //��������� � ��������� ��������� ������
            currentState = child.rotation;  
        }
        else
        {
            Quaternion xQ = Quaternion.AngleAxis(joystickY * rotationRange, Vector3.right); //�������� ������� �� ��� ���
            Quaternion yQ = Quaternion.AngleAxis(-joystickX * rotationRange, Vector3.up); //�������� ������� �� ��� �����
            finalQ = xQ * yQ * currentState; //����������� ����������� � ��������� ��������� ������, �������� ������� ���������
            finalQ = Quaternion.Euler(RoundingModulo(finalQ.eulerAngles.x, rotationStep),
                                    RoundingModulo(finalQ.eulerAngles.y, rotationStep),
                                    RoundingModulo(finalQ.eulerAngles.z, rotationStep)); //����� �������� �� ������ ���� �������������
            child.rotation = finalQ; //��������� �������� � ������
        }
    }

    //����� ��������� �������� � ��������� ������
    //���������� �������� �� ������
    private float RoundingModulo(float x, float m)
    {
        return x - x % m; 
    }
}
