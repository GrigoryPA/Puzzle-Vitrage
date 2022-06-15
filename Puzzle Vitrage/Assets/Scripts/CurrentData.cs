using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//������ ����� �������� ��� ���������� � ������ ����������,
//������� ����� ������������ ������ ������� � �������� ����
//������ � ���������� �������������� ����� ��������� ���� ���� �� ���� ��� � �����
public class CurrentData
{
    //����� ���������� (������ ���� �������������������)
    public static CurrentData Data = new CurrentData();

    public bool isProgress = false; //������� �� ��������
    public bool isBasicLevel = true; //������� ������� ������� ��� ����������������
    public bool isSet = false; //��������� �� �������� ������
    public Mesh objectMesh; //��� ������ ������
    public Material roomMaterial; //�������� ������� ������
    public SaveData.Level currentLevel = new SaveData.Level(); //��������� ���������� ������
    public static Color[] COLORS = {new Color(0.2f, 0, 0),
                                new Color(0.4f, 0, 0),
                                new Color(0.6f, 0, 0),
                                new Color(0.8f, 0, 0),
                                new Color(1.0f, 0, 0),
                                new Color(0, 0.2f, 0),
                                new Color(0, 0.4f, 0),
                                new Color(0, 0.6f, 0),
                                new Color(0, 0.8f, 0),
                                new Color(0, 1.0f, 0),
                                new Color(0, 0, 0.2f),
                                new Color(0, 0, 0.4f),
                                new Color(0, 0, 0.6f),
                                new Color(0, 0, 0.8f),
                                new Color(0, 0, 1.0f),
                                new Color(0.2f, 0.2f, 0),
                                new Color(0.2f, 0, 0.2f),
                                new Color(0, 0.2f, 0.2f),
                                new Color(0.4f, 0.4f, 0),
                                new Color(0.4f, 0, 0.4f),
                                new Color(0, 0.4f, 0.4f),
                                new Color(0.6f, 0.6f, 0),
                                new Color(0.6f, 0, 0.6f),
                                new Color(0, 0.6f, 0.6f),
                                new Color(0.04f, 0.04f, 0.04f),
                                new Color(0.1f, 0.1f, 0.1f),
                                new Color(0.2f, 0.2f, 0.2f),
                                new Color(0.3f, 0.3f, 0.3f)}; //���������� ����� ��� ������ ������
    public SaveData.Progress currentProgress = new SaveData.Progress(); //������� ��������
    public SaveData.Settings currentSettings = new SaveData.Settings(); //������� ���������
    public float currentLevelTime; //����� � �������� ������������ �� ����������� �������� ������

    //���� � ������ � ��������� �����, ���������� � ������� �������
    private const string meshesRootPath = "ObjectMeshes/";
    private const string roomMaterialsRootPath = "RoomMaterials/";
    private const string basicLevelPath = "BasicLevels/";
    //���� � ������ � ������� � ��������� ������������
    private const string progressKey = "PLAYER_PROGRESS_KEY";

    //����� ������ � ��������� ���������� � ��������� ������� ������
    //���������: ��� �������� ������
    public bool ReadBasicLevel(string levelName)
    {
        try
        {
            //������� ������� �� �������� ����
            currentLevel = SaveManager.LoadTextAsset<SaveData.Level>(basicLevelPath + levelName);
            LoadMeshAndMaterial(); //����� � ������ �������� ��������� � ���� ������
            isBasicLevel = true; //��������� ����� �������� ������

            return true; //������� ������� ��������
        }
        catch
        {
            return false; //������� �� ��� ������
        }
    }

    //����� ������ � ��������� ���������� � ��������� ���������������� ������
    //���������: ��� ������
    public bool Read�ustomLevel(string levelName)
    {
        //���� ���� � ����� ��������� ������
        if (SaveManager.FindJson<SaveData.Level>(levelName))
        {
            //��������� ������ ����������������� ������
            currentLevel = SaveManager.LoadJson<SaveData.Level>(levelName); 
            LoadMeshAndMaterial(); //����� � ������ �������� ��������� � ���� ������
            isBasicLevel = false; //��������� ����� ����������������� ������

            return true; //�����
        }
        else
        {
            return false; //���� �� ��� ������
        }
    }

    //����� ������� ��������� ������� ���������� � ����� �� �� ���������
    //� ����� ��������� ������ ������ ������, �� �������������� ����
    public void LoadMeshAndMaterial()
    {
        //�������� �������� �������� � ���� �� �� �������� � ����
        roomMaterial = Resources.Load<Material>(roomMaterialsRootPath + currentLevel.roomMaterialName);
        objectMesh = Resources.Load<Mesh>(meshesRootPath + currentLevel.objectMeshName);

        //����������� ���������� ������ � �������
        uint indexCount = objectMesh.GetIndexCount(0) / 3;
        //���� �� ��������� ���������� ������ � ������ ����
        if (currentLevel.objectFacesColor.Length != indexCount)
        {
            //���������� ������ �����
            var bufObjectFacesColor = currentLevel.objectFacesColor;
            //�������������� �����
            currentLevel.objectFacesColor = new Color[indexCount];
            //���������� �� ������� ������
            for (uint i = 0; i < indexCount; i++)
            {
                //���� ���� ��� ���� ����� ����
                if (i < bufObjectFacesColor.Length)
                {
                    //������������ ���� ����
                    currentLevel.objectFacesColor[i] = bufObjectFacesColor[i];
                }
                //���� ��� ����� ����� ��� ���
                else
                {
                    //���������� ��������� ���� �����
                    currentLevel.objectFacesColor[i] = COLORS[COLORS.Length - 1];
                }
            }
        }
        isSet = true;//�������� ����� ��������� ���������
    }

    //����� ������� ������ �������� ��������� �� �������
    //���������: ���������� ������� �������
    public void ReadPlayerProgress(int numberLevels)
    {
        isProgress = true; //�������� �����������
        currentProgress = SaveManager.LoadPP<SaveData.Progress>(progressKey);//���������� �������� �� �������
        bool found = currentProgress.completedLevels.Length != 0; //��������� ����������� �� �� ����� ��������
        //���� �������� �� ��� ������, �������� ������ ��� ����
        currentProgress.completedLevels = found ? currentProgress.completedLevels : new float[numberLevels];
    }

    //����� ��������� � ��������� ������� �������� � ����������� �������
    //���������: ������ ������������ ������
    public void UpdateAndSavePlayerProgress(int index)
    {
        //���� �������� �������
        if (isProgress)
        {
            currentProgress.completedLevels[index] = currentLevelTime; //�������� ����� ����������� �� ���������� ������
            SaveManager.SavePP<SaveData.Progress>(progressKey, currentProgress); //��������� �������� � �������
        }
    }

    //����� ���������� �������� � ����������� ������� �� ����������
    //���������: ���������� ������� �������
    public void ResetPlayerProgress(int numberLevels)
    {
        currentProgress.completedLevels = new float[numberLevels]; //������� ����� ������ ������ � ���������� �������
        SaveManager.SavePP<SaveData.Progress>(progressKey, currentProgress); //��������� ������ �������� � ������
    }
}
