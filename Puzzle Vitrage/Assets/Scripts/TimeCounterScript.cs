using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//����� ��������� ��������� �����������
public class TimeCounterScript : MonoBehaviour
{
    //��������� ������ ����������������� ���������� ��� ����������� ����������
    public Text text; 
    private int min = 0; //���-�� �����
    private int sec = 0; //���-�� ������
    private int msec = 0; //���-�� �����������
    public float timer = 0; //����� ����� � ������������� � ������� �������
    public bool isStopped = false; //���� ���������

    //����� ��������� ���������� ������������� ��� �������� ������
    public void StartTimer()
    {
        timer = 0;
        msec = 0;
        sec = 0;
        min = 0;
    }

    //����� ���������������� ������ �����������
    public void Pause()
    {
        isStopped = true;
    }

    //����� ��������� ������ �����������
    public void Play()
    {
        isStopped = false;
    }

    //����� ���������� ������ ���������� ������ ������
    void Update()
    {
        //��������� ����� ������� �����������
        timer += isStopped ? 0 : Time.deltaTime;
        msec = (int)(timer*1000 % 1000); //������ ��
        sec = (int)(timer % 60); //������ �
        min = (int)(timer/60); //������ ���
        //��������� ��������� �������� �����������
        text.text = min.ToString("D2") + ":" + sec.ToString("D2");
    }

    //����� ��������� �������� ������� �������� �����������
    public string ToString(float time)
    {
        int _msec = (int)(time * 1000 % 1000);
        int _sec = (int)(time % 60);
        int _min = (int)(time / 60);
        return _min.ToString("D2") + ":" + _sec.ToString("D2") + ":" + _msec.ToString("D3");
    }
}
