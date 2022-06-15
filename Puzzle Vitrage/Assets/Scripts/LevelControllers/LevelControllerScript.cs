using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

//����� ������� ��������� ���� ������� � ��������� �������� ����
//�� ����� ����������� ������
public class LevelControllerScript : MonoBehaviour
{
    public GameObject figure; //������
    public GameObject room; //�������
    public GameObject mainCamera; //������� ������
    public GameObject hint; //���������
    public GameObject loadingScreen; //���� �������� ������
    public GameObject timeCounter; //�������
    public GameObject finishScreen; //���� ������ � ������
    public Text result; //���� ���������� ����������� ������
    public Slider LevelProgressSlider; //������� �������� � ������
    public Image LevelProgressSliderFill; //����������� ������� �������� � ������
    [Range(5, 30)]
    public int acceptableDelta = 10; //���������� �� ������ � �����, ����� ���������� �������������� �������
    [Range(0.0f, 1.0f)]
    public float smooth = 1.0f; //��������� �������������� �������

    public Quaternion finishPosition; //�������� ������� ��� ������� ������
    private bool prepared = false; //����������������� ���� ������ �������
    private bool finished = false; //������� �������

    //����� �����, ����������� ��������������� ����� ������ ����������� ������
    private void Start()
    {
        loadingScreen.SetActive(true); //����������� ���� ��������
        finishScreen.SetActive(false); //������������ ���� ������
        finishPosition = CurrentData.Data.currentLevel.finishPosition; //��������� �������� �������

        //����������� ������ � ������������ ����������� ������
        figure.GetComponent<MeshFilter>().sharedMesh = CurrentData.Data.objectMesh;
        figure.GetComponent<RayTracingObject>().facesColor = new int[CurrentData.Data.currentLevel.objectFacesColor.Length * 3];
        for (int i = 0; i < CurrentData.Data.currentLevel.objectFacesColor.Length; i++)
        {
            figure.GetComponent<RayTracingObject>().facesColor[3 * i] = (int)(255 * CurrentData.Data.currentLevel.objectFacesColor[i].r);
            figure.GetComponent<RayTracingObject>().facesColor[3 * i + 1] = (int)(255 * CurrentData.Data.currentLevel.objectFacesColor[i].g);
            figure.GetComponent<RayTracingObject>().facesColor[3 * i + 2] = (int)(255 * CurrentData.Data.currentLevel.objectFacesColor[i].b);
        }
        //����������� ������� � ������������ � ����������� ������
        room.GetComponent<MeshRenderer>().material = CurrentData.Data.roomMaterial;
        //������ ������ � �������� ���������, ����� ������� �������� ������������
        figure.transform.rotation = finishPosition;
        //���������� ����� ����������� �� ����������� ����� ������ � 0:0:00
        CurrentData.Data.currentLevelTime = 0.0f;
    }


    //������� �����, ������������� ����� ���������� ����� ����������� ���������� �������
    private void FixedUpdate()
    {
        //���������������� ���� ������, �������� ���������
        if (!prepared && !finished && mainCamera.GetComponent<Raytracer>().spriteHint != null)                          
        {
            //�������� ������ ��������� �� ������ ������������ �����������
            hint.GetComponent<Image>().sprite = mainCamera.GetComponent<Raytracer>().spriteHint;
            //������� ������ � ��������� ���������
            figure.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            prepared = true; //����� ���������� ������ �������
            loadingScreen.SetActive(false); //����������� ���� �������� ������
            timeCounter.GetComponent<TimeCounterScript>().StartTimer(); //��������� ����������
        }

        //������������� ��������, ������������� �������� � ��������� ��������� ������
        LevelProgressSlider.value = 1 - (Quaternion.Angle(finishPosition, figure.transform.rotation) / 180);
        LevelProgressSliderFill.color = Color.Lerp(new Color(0.68f, 0.84f, 1.0f, 1.0f), 
                                                new Color(1.0f, 0.67f, 0.4f, 1.0f), 
                                                LevelProgressSlider.value);//������������� ����

        //���� ���������� ��������� � ���� ����� �������� ���������� � ������� ���������� ������ 1,
        //����� �����
        if (prepared && !finished && Quaternion.Angle(finishPosition, figure.transform.rotation) < 1)                                  
        {
            timeCounter.GetComponent<TimeCounterScript>().isStopped = true; //������������� ����������
            //���������� ����� ����������� �� ����������� ����� ������
            CurrentData.Data.currentLevelTime = timeCounter.GetComponent<TimeCounterScript>().timer; 
            //���� �������� ����������� � ��� ������� �������
            if (CurrentData.Data.isProgress && CurrentData.Data.isBasicLevel)
            {
                //����� ��������� � ��������� ������ ��������� 
                CurrentData.Data.UpdateAndSavePlayerProgress(int.Parse(CurrentData.Data.currentLevel.levelName.Split(' ')[1]) - 1);
            }
            CurrentData.Data.isSet = false; 
            GetComponent<AudioSource>().Stop();//������������� ������� ������ � ����
            //���������� ����� ����������� �� ����������� �� ������
            result.text += timeCounter.GetComponent<TimeCounterScript>().ToString(CurrentData.Data.currentLevelTime);
            finishScreen.SetActive(true);//���������� ���� ������
            finished = true;//������������� ���� ��������� ������
        }

        //���� ���������� ��������� � ���� ����� �������� � ������� ���������� ������ ���������� ������,
        //����� ���������� ������������� ��������� ������ �� ��������� ���������
        if (prepared && !finished && Quaternion.Angle(finishPosition, figure.transform.rotation) <= acceptableDelta) 
        {
            //������� ������������� �� �������� ��������� � �������� � ������������ ���������
            figure.transform.rotation = Quaternion.RotateTowards(figure.transform.rotation, finishPosition, smooth);
            //��������� ����������� �������� ������, ����� ������������ �� �����
            GameObject.Find("RotationBase").GetComponent<RotationScript>().isActive = false;
        }
    }
}
