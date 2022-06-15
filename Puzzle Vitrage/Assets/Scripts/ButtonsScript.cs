using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

//����� ��������� ������ ��������� ������� �������������� 
//� �������������� ��������� ����������������� ����������
//����������� ������� ����������� �� ������� ��������
//��� ������ ������������ ��������� Unity
public class ButtonsScript : MonoBehaviour
{
    public string loadScene = ""; //����� ������� ���������� ���������
    public GameObject errorPanel; //���� ������ (���������)

    //����� ��� ������� �� ������ ��������� ����� ����� ������ �������
    //�������������� �������� ����� �������� ����
    public void OnClickToLoadScene() 
    {
        SceneManager.LoadScene(loadScene, LoadSceneMode.Single);
    }

    //����� ��� ������� �� ������ ��������� ����� � ������� �������
    public void OnClickToLoadBasicLevel()
    {
        //���� ������� ������� ������� ������ ���������� �������� ������
        if (CurrentData.Data.ReadBasicLevel(this.name))
        {
            //��������� ����� ���������� �������� ������, ������ ������� �����
            SceneManager.LoadScene(loadScene, LoadSceneMode.Single);
        }
        else
        {
            //��������� �� ������ ������������
            errorPanel.GetComponentInChildren<Text>().text = "This level in the process of being created";
            errorPanel.SetActive(true);
        }
    }

    //����� ��� ������� �� ������ ��������� ����� � ���������������� �������
    //���������: ���� ����� ����� ������
    public void OnClickToLoadCustomLevel(InputField levelNameInputField)
    {
        //���� ������� ��������� ������ ����������������� ������
        if (CurrentData.Data.Read�ustomLevel(levelNameInputField.text))
        {
            //��������� ����� � ������������ ������ ������ ���� �����
            SceneManager.LoadScene(loadScene, LoadSceneMode.Single);                                        
        }
        else
        {
            //��������� ������������, �� ���� � ������� �� ��� ������ � ������������ �����
            string errorMessage = "The file at the address ";
            //����� ������������ ���������� ��������
#if UNITY_ANDROID && !UNITY_EDITOR
            errorMessage += Application.persistentDataPath;
#else
            errorMessage += Application.dataPath;
#endif
            errorMessage += " was not found";
            errorPanel.GetComponentInChildren<Text>().text = errorMessage;
            errorPanel.SetActive(true);
        }
    }

    //����� ��� ������� �� ������ ��������� ���������������� ������� � ����
    //���������: �������, � �������� ���������� ������� ����� ������ �������� �������
    public void OnClickToSaveCustomLevel(Canvas canvas)
    {
        //���� �� ������� ��������� �������
        if (!canvas.GetComponent<CustomLevelCreator>().SaveCustomLevel())
        {
            //��������� ����������
            errorPanel.GetComponentInChildren<Text>().text = "The level must have a name!";
            errorPanel.SetActive(true);
        }
        else 
        {
            //�������� ����������
            errorPanel.GetComponentInChildren<Text>().text = "Saving was successful.";
            errorPanel.SetActive(true);
        }
    }

    //����� ��� ������� �� ������ ������������ ������������ ������� ������
    //���������: ������������ ������� �������
    public void OnClickToInactivatingParentPanel(GameObject parentPanel)
    {
        parentPanel.SetActive(false); //����������� �������� � ����
    }

    //����� ���������� ��� ��������� ������ �������������� �������
    //���������: ������, � �������� ���������� ������� ����� ����������� ��������� ������
    public void OnValueChanged(GameObject canvas)
    {
        //���������� �������� ������
        canvas.GetComponent<CustomLevelCreator>().UpdateLevelSettings();
    }

    //����� ��� ������� �� ������ ���������� �������� � ���������� �������
    public void OnClickToRestartProgress()
    {
        CurrentData.Data.ResetPlayerProgress(0);
    }

    //����� ��� ������� ������ ��� ���������� �� ��������������
    //���������: ��������� ��������� ������
    public void OnClickToChangeRotation(Text buttonText)
    {
        //�������� ��� ���������� �� ���������������
        CurrentData.Data.currentSettings.controlOption = CurrentData.Data.currentSettings.controlOption % 2 + 1;
        //����������������� ����� ���� ���������� �� ������� ������
        buttonText.text = CurrentData.Data.currentSettings.controlOption.ToString();
    }

    //����� ��� ������� ������ ��������� ���������� �������� ����������� � ����
    //���������: ������� �������� �������� ���������
    public void OnClickToChangeGraphic(Slider thisSlider)
    {
        CurrentData.Data.currentSettings.graphicsOption = thisSlider.value;
    }

    //����� ��� ������� �� ������ ������ ���� ������� ����� � ���� �� ���������������
    //���������: ��������� ��������� ������
    public void OnClickToChangeSound(Text buttonText)
    {
        int length = buttonText.text.Length; 
        AudioListener.volume = buttonText.text.Contains("ON") ? 1 : 0;
        //���������� ������� �������� ������ �� ������� ������
        buttonText.text = buttonText.text.Contains("ON") ? buttonText.text.Substring(0, length - 2) + "OFF" : buttonText.text.Substring(0, length - 3) + "ON";
        AudioListener.volume = buttonText.text.Contains("ON") ? 1 : 0;
        //��������� ��������� � ��������� ����� � ����
        CurrentData.Data.currentSettings.audioVolume = AudioListener.volume;
    }
}
