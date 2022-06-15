using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//����� ������� ������� ��������� ��������� ���� ������ ������
//������ �� ���������� � ����������� ������� � ������ ��������� ����������� ������
public class ProgressLevelVisualisator : MonoBehaviour
{
    public bool isProgress = false; //���������� ���� ����������� ���� ���������

    //�������������� ����� �����, ����������� ��������������� ����� ������ ����������� ������
    void Start()
    {
        //�������� ��� ������ ������� ������� �� �����
        Button[] buttons = this.GetComponentsInChildren<Button>(true);
        int numberButtons = buttons.Length;

        //���� �������� �������
        if (isProgress)
        {
            //��������� �������� �� ������� � ��������� ����� ����� ������� ������
            CurrentData.Data.ReadPlayerProgress(buttons.Length);

            //���������� �� ���� ������� ������� �������
            for (int i = 1; i < numberButtons; i++)  
            {
                //��������� ���������������� ������, � ����������� �� ������������ ����������� ������
                buttons[i].interactable = CurrentData.Data.currentProgress.completedLevels[i - 1] != 0;
            }
        }
        else
        {
            //���� ��� ���������, �� ���������� ���� ������� �������� �������
            CurrentData.Data.ResetPlayerProgress(buttons.Length);
        }
    }
}
