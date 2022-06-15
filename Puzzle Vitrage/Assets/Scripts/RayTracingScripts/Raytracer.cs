using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//����� Raytracer ���������� ������������ ���������� �����������
//� ������: ������������ ������ � �������� ����������
//����������� �������� ����������
//� ���������� ����������� ������� ��� �������� ��������
//����� �������������� ������������ ��� ��������� ������ �����,
//��� ��������� � ��������� �� ����������� 
public class Raytracer : MonoBehaviour
{
    private const float PI_180 = 0.01745329f;//��������� ��
    public Sprite spriteHint = null; //������ ����������� ��������� ��� ������
    public float selectedFace = -10; //���������� �����
    public bool creatingMode = false; //����� �������� ������

    //��� �������� ������ �������, ��������� ����� ���� ������������
    [SerializeField]
    private ComputeShader raytracingShader; //��� �������������� ������ � �������������
    [SerializeField]
    private Texture skyboxTexture; //�������� ���� (����)
    [SerializeField, Range(1f, 20f)]
    private int transparencyCount = 4; //���������� �������� ��� ������������
    [SerializeField]
    private Light spotLight; //������������ �������� �����
    [SerializeField]
    private Light pointLight; //�������� �������� �����


    private RenderTexture target = null; //�������� �����������, ����������� ������� �������� ������
    private Camera componentCamera; //������ �����, ������� ������������� ������ �� �����

    //RAYTRACING OBJECTS
    private static bool meshObjectsNeedRebuilding = false; //���� �� ����������� ��� ������, ���� �������� ����� ������
    private static List<RayTracingObject> rayTracingObjects = new List<RayTracingObject>(); //������ �������� �����������

    //������ ��� ������ � ����������� � ����� ��������
    private static List<MeshRT> meshObjects = new List<MeshRT>(); //������ �������� ����� ��������
    private static List<Vector3> vertices = new List<Vector3>(); //������ ������ ��������
    private static List<Vector2> verticesUV = new List<Vector2>(); //������ ��������� ���������� ������
    private static List<int> indices = new List<int>(); //������ �������� ��������
    private static List<int> colors = new List<int>(); //������ ������ ������
    //������ ��� �������� �������
    private ComputeBuffer meshObjectBuffer; //����� ��������� �����
    private ComputeBuffer vertexBuffer; //����� ������
    private ComputeBuffer vertexUVBuffer; //���� ��������� �������
    private ComputeBuffer indexBuffer; //����� ��������
    private ComputeBuffer colorBuffer; //����� ������ ������


    //===================================================================================


    //������� ���������� ����� ��������� ������� �� �����
    private void OnEnable()
    {
        //�������� ��������� ������
        componentCamera = GetComponent<Camera>();
    }


    //������� ����������� ����� ������ ���������� ����������
    private void OnDisable()
    {
        //������� ���� �������
        meshObjectBuffer?.Release();
        vertexBuffer?.Release();
        vertexUVBuffer?.Release();
        indexBuffer?.Release();
        colorBuffer?.Release();
    }


    //����� ������ ���������� ��������, ��� ��� �����������
    public static void RegisterObject(RayTracingObject obj)
    {
        rayTracingObjects.Add(obj);//�������� ������ � ������ ��������
        meshObjectsNeedRebuilding = true;//��������� ����������� �������
    }


    //����� ��� ������� ������ �� �������
    public static void UnregisterObject(RayTracingObject obj)
    {
        rayTracingObjects.Remove(obj);//������� ������ �� ������
        meshObjectsNeedRebuilding = true;//��������� ����������� �������
    }


    //����� ������� ������������� ������ ����� ����������� ����� � �������� ��������������
    private void RebuildMeshObjectBuffer()
    {
        //�������� �� ���� ��������
        for (int i = 0; i < rayTracingObjects.Count; i++)
        {
            //�������� �������� ������ �������������� � ������ ��� ��������
            MeshRT _meshObject = meshObjects[i];
            _meshObject.localToWorldMatrix = rayTracingObjects[i].transform.localToWorldMatrix;
            meshObjects[i] = _meshObject;
        }
        
        //���������� ������������� ������
        CreateComputeBuffer(ref meshObjectBuffer, meshObjects, MeshRT.GetSize());
    }


    //����� ������� ������������� ��� ������ ������� � ��������
    private void RebuildAllObjectBuffers()
    {
        //������� ���� ������� 
        meshObjects.Clear();
        vertices.Clear();
        verticesUV.Clear();
        indices.Clear();
        colors.Clear();

        //����� ����� ������� ������� �  ������ ���� � ����� � ��� �� ������� ������
        if (rayTracingObjects[0].gameObject.name == "Object")
            rayTracingObjects.Reverse();

        //���������� �� ���� ������������������ �������� � ������
        foreach (RayTracingObject obj in rayTracingObjects)
        {
            Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;

            // ��������� � ������ ������ � ����������� ������ � �������
            int firstVertex = vertices.Count;
            vertices.AddRange(mesh.vertices);
            verticesUV.AddRange(mesh.uv);

            //���������� ������ � ��������� ���� �� �������� � ������
            int firstIndex = Raytracer.indices.Count;
            var indices = mesh.GetIndices(0);
            Raytracer.indices.AddRange(indices.Select(index => index + firstVertex));

            //��������� ���� � ������ ������ � ������
            var colors = obj.facesColor;
            Raytracer.colors.AddRange(colors);

            //�������������� ��������� meshRT � ��������� � ������
            meshObjects.Add(new MeshRT()
            {
                localToWorldMatrix = obj.transform.localToWorldMatrix,
                indices_offset = firstIndex,
                indices_count = indices.Length,
                transparency = obj.transparency,
            });
        }

        //�� ���������� ����� ������ ������� ������
        CreateComputeBuffer(ref meshObjectBuffer, meshObjects, MeshRT.GetSize()); //sizeof(float*4*4 + int + int + float + float)
        CreateComputeBuffer(ref vertexBuffer, vertices, sizeof(float) * 3); //sizeof(float*3)
        CreateComputeBuffer(ref vertexUVBuffer, verticesUV, sizeof(float) * 2); //sizeof(float*3)
        CreateComputeBuffer(ref indexBuffer, indices, sizeof(int)); //sizeof(float)
        CreateComputeBuffer(ref colorBuffer, colors, sizeof(int)); //sizeof(float)
    }


    //����� ��� �������� ������ �� ������ ��� ������, ������ � ������� � ������� ������
    private static void CreateComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride)
    where T : struct
    {
        //���� ����� ��� �������
        if (buffer != null)
        {
            //���� ����� �� ����� ������ ���������, �� �������� �����
            if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride)
            {
                buffer.Release();
                buffer = null;
            }
        }

        //���� ������ �� ������
        if (data.Count != 0)
        {
            //���� ����� ������, ����� ������� ���
            if (buffer == null)
            {
                buffer = new ComputeBuffer(data.Count, stride);
            }

            //������� ������ � �����
            buffer.SetData(data);
        }
    }


    //����� �������� ����� � ������ �� ����� ���� � ������� � ������
    private void SetComputeBuffer(string name, ComputeBuffer buffer)
    {
        //���� ����� �� ������
        if (buffer != null)
        {
            //���������� ���� ������� ������ �� ���� � �������� ������� ������
            raytracingShader.SetBuffer(0, name, buffer);
        }
    }


    //����� ������������� ������ ������ ��������� �������
    private void SetUpdatableShaderParameters()
    {
        //��������� �������� ������ � MeshRT, ��� ���� ������ ������� ��������������
        SetComputeBuffer("_MeshObjects", meshObjectBuffer);
    }


    //��������� ���� ���������� �������
    private void SetAllShaderParameters()
    {
        //��������� ��������� ������
        raytracingShader.SetMatrix("_CameraToWorld",
            componentCamera.cameraToWorldMatrix);
        raytracingShader.SetMatrix("_CameraInverseProjection",
            componentCamera.projectionMatrix.inverse);
        raytracingShader.SetTexture(0, "_SkyboxTexture",
            skyboxTexture);
        raytracingShader.SetInt("_TransparencyCount",
            transparencyCount);

        //��������� ���������� �����
        Vector3 dir = spotLight.transform.forward;
        Vector3 posS = spotLight.transform.position;
        Vector3 posP = pointLight.transform.position;
        raytracingShader.SetVector("_SpotLightDirection",
            new Vector4(dir.x, dir.y, dir.z, spotLight.intensity));
        raytracingShader.SetVector("_SpotLightPosition",
            new Vector4(posS.x, posS.y, posS.z, PI_180 * spotLight.spotAngle));
        raytracingShader.SetFloat("_LightRange", pointLight.range);
        raytracingShader.SetVector("_PointLightPosition",
            new Vector4(posP.x, posP.y, posP.z, pointLight.intensity));

        //��������� ���������� ����� ��������
        SetUpdatableShaderParameters();
        raytracingShader.SetFloat("_selectedFace", selectedFace);
        SetComputeBuffer("_Vertices", vertexBuffer);
        SetComputeBuffer("_VerticesUV", vertexUVBuffer);
        SetComputeBuffer("_Indices", indexBuffer);
        SetComputeBuffer("_Colors", colorBuffer);

        //��������� ������� � �� ������� �������
        GameObject room = GameObject.Find("Room");
        raytracingShader.SetTexture(0, "_RoomMaskMap",
            room.GetComponent<MeshRenderer>().material.GetTexture("_MaskMap"));
        raytracingShader.SetTexture(0, "_RoomColorTexture1",
            room.GetComponent<MeshRenderer>().material.GetTexture("_MainTex1"));
        raytracingShader.SetTexture(0, "_RoomColorTexture2",
            room.GetComponent<MeshRenderer>().material.GetTexture("_MainTex2"));
        raytracingShader.SetTexture(0, "_RoomBumpMap1",
            room.GetComponent<MeshRenderer>().material.GetTexture("_BumpMap1"));
        raytracingShader.SetTexture(0, "_RoomBumpMap2",
            room.GetComponent<MeshRenderer>().material.GetTexture("_BumpMap2"));
        raytracingShader.SetFloat("_BumpScale1",
            room.GetComponent<MeshRenderer>().material.GetFloat("_BumpScale1"));
        raytracingShader.SetFloat("_BumpScale2",
            room.GetComponent<MeshRenderer>().material.GetFloat("_BumpScale2"));
        raytracingShader.SetVector("_TexTiling1",
            room.GetComponent<MeshRenderer>().material.GetTextureScale("_MainTex1"));
        raytracingShader.SetVector("_TexTiling2",
            room.GetComponent<MeshRenderer>().material.GetTextureScale("_MainTex2"));
    }


    //����� ��������������� �������� �����������, ������� ����� ������� ������
    private void Init()
    {
        int width = Screen.width > 1920 ? 1920 : Screen.width;
        int height = Screen.height > 960 ? 960 : Screen.height;
        //��� �������� ��� �� ���� ������������������� ��� �� ��������� �� ������������ �������� ������*��������� ��������
        if (target == null || target.width != (int)(width * CurrentData.Data.currentSettings.graphicsOption) 
            || target.height != (int)(height * CurrentData.Data.currentSettings.graphicsOption))
        {
            //���� �������� �� �����, �������� ��
            if (target != null)
            {
                target.Release();
            }

           //�������������� �������� ���������� �������� � ������������� �������������
            target = new RenderTexture((int)(width * CurrentData.Data.currentSettings.graphicsOption),
                (int)(height * CurrentData.Data.currentSettings.graphicsOption),
                0,
                RenderTextureFormat.ARGBFloat,
                RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }
    }


    //����� ���������� �������� �������� 
    private void Render(RenderTexture destination)
    {
        //�������������� �������� � �������������
        Init();

        int width = Screen.width > 1920 ? 1920 : Screen.width;
        int height = Screen.height > 960 ? 960 : Screen.height;

        //������������� �������� ���������� ������� ��� ��� ��� ������ ����������
        raytracingShader.SetTexture(0, "Result", target);
        //����� ����� �� ����� ��� ��������� ���������������
        int threadGroupsX = Mathf.CeilToInt((int)(width * CurrentData.Data.currentSettings.graphicsOption) / 8.0f);
        int threadGroupsY = Mathf.CeilToInt((int)(height * CurrentData.Data.currentSettings.graphicsOption) / 8.0f);
        //��������� ������ �������
        raytracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        //�������� ���������� �������� � ��������, ������� ���������� ������ �� �����
        Graphics.Blit(target, destination);
    }


    //������� ����� Unity ������� ���������� ����� ���������� ��� ������������� ����������� �� ������.
    //���������: �������� ����������� � ����������� ��� ���������
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //���� ���� ����������� ������ ��� ��� ����� ��������
        if (meshObjectsNeedRebuilding || creatingMode)
        {
            meshObjectsNeedRebuilding = false; //��� ��������� �����������
            RebuildAllObjectBuffers(); //����������� ��� ������
            SetAllShaderParameters(); //���������� ��� ���������
        }
        else
        {
            RebuildMeshObjectBuffer(); //���������� ������ ������� ��������������
            SetUpdatableShaderParameters(); //�������� ������ � ������� � �������
        }
        //�������� ����������� ���������  ������
        Render(destination); 
        //���� ��� �� ��������� ���������, �� ������ ��
        if (spriteHint == null) 
        {
            CreateSpriteHint();
        }
    }


    //����� ������� �������� �� �������������� �������� ����� � ����� ��� ���������
    private void CreateSpriteHint()
    {
        //������ ������� ��������
        int width = Screen.width, height = Screen.height;
        int right = (int)(width * 0.425), left = (int)(width * 0.225);
        int down = (int)(height * 0.3), up = (int)(height * 0.7);
        width = right - left;
        height = up - down;
        //������� ��������
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        //������ ������� � ������ � ��������� �������
        tex.ReadPixels(new Rect(left, down, width, height), 0, 0);
        //���������
        tex.Apply();
        //������� ������
        spriteHint = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }
}
  