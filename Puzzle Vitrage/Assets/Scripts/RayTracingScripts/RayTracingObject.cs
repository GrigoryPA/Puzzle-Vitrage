using UnityEngine;
using System.IO;

//����� ������� ����������� ���� ��������, ������� �������� � ������������
//��� ��������� ������ ������� �� �����, ���������� ���������������� ����
//� �������� ��� ���� ��������� ������ RayTracer
//��������� ������� ��������� ������� ����� ������������� � ������� ���������
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RayTracingObject : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float transparency = 0.0f;//������������ �������
    public int[] facesColor;//����� ������ �������

    //������� ���������� ����� ��������� ������� �� �����
    private void OnEnable()
    {
        //����������� ������� ������� ������
        uint indexCount = GetComponent<MeshFilter>().sharedMesh.GetIndexCount(0);
        //��������������� ������������� ���� ������ ����� ������
        facesColor = new int[indexCount];
        for (uint i = 0; i < indexCount; i++)
        {
            facesColor[i] = 255;
        }
        //����������� ������� ����� �������� ������������
        Raytracer.RegisterObject(this);
    }

    //������� ����������� ����� ������ ���������� ����������
    private void OnDisable()
    {
        //������������ ������ ���������� ��� ������ �������������
        Raytracer.UnregisterObject(this);
    }
}
