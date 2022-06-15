using UnityEngine;

//������������ ����, � ������� ��������� ��� ��� ����������
namespace SaveData
{
    //������������� �����, ����������� ������� ��������� �������� ����
    [System.Serializable]
    public class Settings
    {
        public int controlOption = 1; //���� ���������� (1 - 1 ��������, 2 - 2 ���������)
        public float graphicsOption = 1.0f; //����-� ���������� �������� ���������� �����������
        public float audioVolume = 1.0f; //������� ��������� ������ (1 - max, 0 - min)
    }
}
