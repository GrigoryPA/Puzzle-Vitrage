using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//����� ������������ ��� ��������� ������� ����� � 
//���������� ����������������� ��������� �� ������ ������ �������� ������
public class CustomLevelCreator : MonoBehaviour
{
    //����� ������������ (�������� � ���������� ������� ������� � ����� �������� ����)
    public bool isDeveloperMode = false; 
    public InputField levelNameInputField;//���� ���� �������� ������
    public Dropdown roomDropdown;//��������� ������ ���������� �������
    public Dropdown figureDropdown;//���������� ������ ���� ������
    public Dropdown facesDropdown; //��������� ������ ������ ������
    public Dropdown colorDropdown; //���������� ������ ������ �����
    public GameObject figure; //������
    public GameObject room; //�������
    public GameObject mainCamera; //������, ������� ���������� ����������� 
    public GameObject loadingScreen; //���� �������� ������

    private int indexSelectedFace = -1; //������ ��������� �����
    private SaveData.Level newLevel = new SaveData.Level(); //����� �������

    //����� �����, ����������� ��������������� ����� ������ ����������� ������
    void Start()
    {
        loadingScreen.SetActive(true); //��� ����� ��������
        InitializingUI(); //���������� ������ ������ ������ ��� ���������
        loadingScreen.SetActive(false); //���� ����� ��������
    }

    //����� ��������� ������� � �������� ����������� � ����
    public bool SaveCustomLevel()
    {
        UpdateLevelSettings(); //�������� ��������� ������
        //���� �� ������ �� ������
        if (newLevel.levelName.Length != 0)
        {
            //���� ��� ����� �����������
            if (isDeveloperMode)
            {
#if UNITY_EDITOR
                //��������� ������� � ����� ������� �������� ������ � ����� � ��������� ����
                SaveManager.SaveTextAsset<SaveData.Level>(newLevel, "/Levels/Resources/BasicLevels/", newLevel.levelName);
#else
                return false; //���� ��� �� ����� ����������, ����� ������
#endif
            }
            //���� ��� ������� ����� ��������
            else
            {
                //��������� ������� � ���� ����� ���������� json � �������� ������� ���������
                SaveManager.SaveJson<SaveData.Level>(newLevel, newLevel.levelName);
            }
            return true; //�����
        }
        else 
        {
            return false; //�������
        }
    }

    //����� ������� ��������� ��������� ������������� �������� ������
    //� ��������� �� � �������� ����
    public void InitializingUI()                                                
    {
        newLevel.levelName = levelNameInputField.text; //�������� ������ �� ���� ����� ������
        newLevel.roomMaterialName = roomDropdown.captionText.text; //�������� �������
        newLevel.finishPosition = figure.transform.rotation; //������� �������
        newLevel.objectMeshName = figureDropdown.captionText.text; //��� ������� ������
        SetOptionsColorDropdown(); //�������������� �������� ����������� ������ ������

        ApplyLevelSettings(); //��������� ������� ������ ������ � �����
    }

    //����� ��������� ��������� ������ � ��������� �� � �������� ����
    public void UpdateLevelSettings()                                          
    {
        newLevel.levelName = levelNameInputField.text; //�������� ������
        newLevel.roomMaterialName = roomDropdown.captionText.text; //�������� �������
        newLevel.finishPosition = figure.transform.rotation; //������� �������
        //���� ���� ������� ����� ����� ������
        if (newLevel.objectMeshName != figureDropdown.captionText.text) 
        {
            newLevel.objectMeshName = figureDropdown.captionText.text; //��������� � ���������� ������
            facesDropdown.value = 0; //���������� �������� ��������� ����� ������ �� 0
        }
        //���� ���� ������� ������ ����� ������
        if (facesDropdown.value != indexSelectedFace) 
        {
            indexSelectedFace = facesDropdown.value; //��������� � ���������� ������
            //��������� ����� ���� � ������ ����� ��� ���������� ����������� ������ ������
            RememberColorDropdown(); 
        }
        UpdateFaceColor(); //�������� ���� ����� � ������������ � ���������� ������� ������
        ApplyLevelSettings(); //��������� ������� ������ ������ � �����
    }

    //����� ��������� ������� �������� ������ � �������� ����
    private void ApplyLevelSettings()                                        
    {
        CurrentData.Data.currentLevel = newLevel; //������ ������� ����� ������
        CurrentData.Data.LoadMeshAndMaterial(); //��������� ������ ��������� �������� ���������� � �����

        room.GetComponent<MeshRenderer>().material = CurrentData.Data.roomMaterial; //��������� �������� �������

        //�������������� ������ ������ ������ ������
        figure.GetComponent<RayTracingObject>().facesColor = new int[CurrentData.Data.currentLevel.objectFacesColor.Length * 3];
        //��������� ����� ���������� ������ � ������
        for (int i = 0; i < CurrentData.Data.currentLevel.objectFacesColor.Length; i++)
        {
            figure.GetComponent<RayTracingObject>().facesColor[3 * i] = (int)(255 * CurrentData.Data.currentLevel.objectFacesColor[i].r);
            figure.GetComponent<RayTracingObject>().facesColor[3 * i + 1] = (int)(255 * CurrentData.Data.currentLevel.objectFacesColor[i].g);
            figure.GetComponent<RayTracingObject>().facesColor[3 * i + 2] = (int)(255 * CurrentData.Data.currentLevel.objectFacesColor[i].b);
        }

        //���� ��� ������ �� ������������� ���� ������
        if (figure.GetComponent<MeshFilter>().sharedMesh != CurrentData.Data.objectMesh)
        {
            //�������� ��� ������ ��� � ������
            figure.GetComponent<MeshFilter>().sharedMesh = CurrentData.Data.objectMesh;
            UpdateFacesDropdown(); //�������� ���������� ������ ������ ��� ����� ���
        }
    }

    //����� ��������� ���������� ������ ������ � ������������ ���������� ���� ������
    public void UpdateFacesDropdown()
    {
        uint sizeList = figure.GetComponent<MeshFilter>().sharedMesh.GetIndexCount(0) / 3; //���������� ������ ����
        List<Dropdown.OptionData> newFacesList = new List<Dropdown.OptionData>((int)sizeList); //������ ����� ����������� ������
        for (int i = 1; i <= sizeList; i++) //���������� �� ����� ������
        {
            newFacesList.Add(new Dropdown.OptionData(i.ToString())); //��������� ���� � ����� ������ � ������
        }
        facesDropdown.options = newFacesList; //��������� ����� ����� � ����������� ������
        indexSelectedFace = facesDropdown.value; //������� �������� ������ - ��������� �����
        RememberColorDropdown(); //��������� ��������� �������� ����������� ������ ������ � ����� �����
    }

    //����� ���������� ����� ���� ���������� ���������� ��� ����������� ������ ������ ������
    public void RememberColorDropdown()
    {
        //��������� ����������� ����� ����� ������ ����������
        mainCamera.GetComponent<Raytracer>().selectedFace = indexSelectedFace * 3; 

        int i = 0; //������ ��������� ����� � �������� �� ����������
        if (indexSelectedFace < newLevel.objectFacesColor.Length)
        {
            //���� ���������� ����� ��������� ����� � ����� � ���������� ������
            for (i = CurrentData.COLORS.Length - 1; i >= 0 &&
                !CurrentData.COLORS[i].Equals(newLevel.objectFacesColor[indexSelectedFace]); i--)
            { }
        }
        colorDropdown.value = i; //���������� ��������� �������� �����
    }

    //����� ��������� ���� ����� ������ � ������������ � ������� ����������� ������ ������
    public void UpdateFaceColor()
    {
        newLevel.objectFacesColor[indexSelectedFace] = CurrentData.COLORS[colorDropdown.value];
    }

    //����� ������������� �������� ����������� ������ ������
    public void SetOptionsColorDropdown()
    {
        int sizeList = CurrentData.COLORS.Length; //���������� ���������� ������
        //�������������� ������ �����
        List<Dropdown.OptionData> ColorList = new List<Dropdown.OptionData>(sizeList);
        for (int i = 1; i < sizeList; i++)
        {
            //������� �������� ���������������� ����� � ���������
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, CurrentData.COLORS[i - 1]);
            texture.Apply();
            Sprite image = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.0f, 0.0f));
            ColorList.Add(new Dropdown.OptionData(i.ToString(), image));
        }
        //��������� ����� � ����������� ������
        colorDropdown.options = ColorList;
    }
}
