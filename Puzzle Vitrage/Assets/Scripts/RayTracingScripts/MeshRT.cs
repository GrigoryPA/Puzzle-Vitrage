using UnityEngine;

//��������� ������������� ��� �������� ��������������
//���������� � ����� ��������, ��������� �������� ����������,
//������� � ��������, ���������� ����� ������
struct MeshRT
{
    public Matrix4x4 localToWorldMatrix; //������� �������������
    public int indices_offset; //����� ������ �������� � ������ ��� �������
    public int indices_count; //���������� �������� � ������ �������
    public float transparency; //������������ �������

    //����� ������� ���������� ������ ���������
    public static int GetSize()
    {
        return sizeof(float)*4*4
            + sizeof(int)
            + sizeof(int)
            + sizeof(float);
    }
}

