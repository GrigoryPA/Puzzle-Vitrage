using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//����� ������� ��������� ���������� �������� ���� �������� ����
//������ �� ������������� ������������ ����������
public class GameSettingsVisualisator : MonoBehaviour
{
    public Button controlButton; //������ ������ ����������
    public Slider graphicSlider; //������� �������� �������
    public Button audioButton; //������ ���������� ������

    //����� �����, ����������� ��������������� ����� ������ ����������� ������
    void Start()
    {
        //�������� ��������� ������� ������
        Text controlText = controlButton.GetComponentInChildren<Text>();
        Text audioText = audioButton.GetComponentInChildren<Text>();

        //������������� �������� �������� � ������������ � �������� ����������
        controlText.text = CurrentData.Data.currentSettings.controlOption.ToString();
        graphicSlider.value = CurrentData.Data.currentSettings.graphicsOption;
        audioText.text = CurrentData.Data.currentSettings.audioVolume.Equals(1.0f) ?
            audioText.text.Substring(0, audioText.text.Length - 2) + "ON" : audioText.text.Substring(0, audioText.text.Length - 2) + "OFF";
    }
}
