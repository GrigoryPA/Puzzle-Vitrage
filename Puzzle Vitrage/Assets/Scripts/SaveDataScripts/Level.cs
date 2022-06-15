using UnityEngine;

//������������ ����, � ������� ��������� ��� ��� ����������
namespace SaveData
{
    //������������� �����, ����������� ��������� ������ ��� ��� ���������� � ����
    [System.Serializable]
    public class Level
    {
        public string levelName; //�������� ������ � �����
        public string roomMaterialName = "RoomMat1"; //�������� ����� ��������� �������
        public Quaternion finishPosition = Quaternion.Euler(new Vector3(30.0f, 30.0f, 30.0f)); //�������� ��������� ������
        public string objectMeshName = "CUBE"; //�������� ����� ���� ������
        public Color[] objectFacesColor = { }; //������ ������ ������ ������
    }
}
