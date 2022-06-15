using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Класс Raytracer занимается организацией рендеринга изображения
//А именно: регистрацией данных о объектах рендеринга
//Назначением текстуры рендеринга
//и управление параметрами шейдера для обраотки текстуры
//Класс предпологается использовать как компонент камеры сцены,
//для перехвата и обработки ее изображений 
public class Raytracer : MonoBehaviour
{
    private const float PI_180 = 0.01745329f;//константа пи
    public Sprite spriteHint = null; //спрайт изображение подсказки для уровня
    public float selectedFace = -10; //выделенная грань
    public bool creatingMode = false; //режим создания уровня

    //ДЛя передачи данных шейдеры, параметры дожны быть сериализуемы
    [SerializeField]
    private ComputeShader raytracingShader; //сам вычислительный шейдер с рейстреиснгом
    [SerializeField]
    private Texture skyboxTexture; //текстура фона (неба)
    [SerializeField, Range(1f, 20f)]
    private int transparencyCount = 4; //количество итерации для прозрачности
    [SerializeField]
    private Light spotLight; //направленный источник света
    [SerializeField]
    private Light pointLight; //точечный источник света


    private RenderTexture target = null; //текстура ренедеринга, вычислением которой займется шейдер
    private Camera componentCamera; //камера сцены, которая символизирует взгляд на сцену

    //RAYTRACING OBJECTS
    private static bool meshObjectsNeedRebuilding = false; //надо ли пересобрать все буферы, если появился новый объект
    private static List<RayTracingObject> rayTracingObjects = new List<RayTracingObject>(); //список объектов рейтресинга

    //Списки для работы с информацией о мешах объектов
    private static List<MeshRT> meshObjects = new List<MeshRT>(); //список структур мешей объектов
    private static List<Vector3> vertices = new List<Vector3>(); //список вершин объектов
    private static List<Vector2> verticesUV = new List<Vector2>(); //список текстурын хкоординат вершин
    private static List<int> indices = new List<int>(); //список индексов объектов
    private static List<int> colors = new List<int>(); //список цветов граней
    //Буферы для передачи шейдеру
    private ComputeBuffer meshObjectBuffer; //буфер структуры мешей
    private ComputeBuffer vertexBuffer; //буфер вершин
    private ComputeBuffer vertexUVBuffer; //уфер координат текстур
    private ComputeBuffer indexBuffer; //буфер индексов
    private ComputeBuffer colorBuffer; //буфер цветов граней


    //===================================================================================


    //Функция вызывается после включения обхекта на сцене
    private void OnEnable()
    {
        //получить компонент камеры
        componentCamera = GetComponent<Camera>();
    }


    //Функция срабатывает когда объект становится неактивынм
    private void OnDisable()
    {
        //очистка всех буферов
        meshObjectBuffer?.Release();
        vertexBuffer?.Release();
        vertexUVBuffer?.Release();
        indexBuffer?.Release();
        colorBuffer?.Release();
    }


    //Метод которы вызывается обхектом, для его регистрации
    public static void RegisterObject(RayTracingObject obj)
    {
        rayTracingObjects.Add(obj);//добавить обхект в список обхектов
        meshObjectsNeedRebuilding = true;//запросить перестройку буферов
    }


    //Метод для очистки памяти об объекте
    public static void UnregisterObject(RayTracingObject obj)
    {
        rayTracingObjects.Remove(obj);//удалить обхект из списка
        meshObjectsNeedRebuilding = true;//запросить перестройку буферов
    }


    //Метод который перестраивает только самый необходимый буфер с матрицей преобразования
    private void RebuildMeshObjectBuffer()
    {
        //пройтись по всем объектам
        for (int i = 0; i < rayTracingObjects.Count; i++)
        {
            //обновить значения матриц преобразования в списке для объектов
            MeshRT _meshObject = meshObjects[i];
            _meshObject.localToWorldMatrix = rayTracingObjects[i].transform.localToWorldMatrix;
            meshObjects[i] = _meshObject;
        }
        
        //обновление передаваемого буфера
        CreateComputeBuffer(ref meshObjectBuffer, meshObjects, MeshRT.GetSize());
    }


    //Метод который перестраивает все буферы общения с шейдером
    private void RebuildAllObjectBuffers()
    {
        //Очистка всех списков 
        meshObjects.Clear();
        vertices.Clear();
        verticesUV.Clear();
        indices.Clear();
        colors.Clear();

        //важно чтобы объекты комнаты и  фигуры были в одном и том же порядке всегда
        if (rayTracingObjects[0].gameObject.name == "Object")
            rayTracingObjects.Reverse();

        //Проходимся по всем зарегестрированным обхектам в списке
        foreach (RayTracingObject obj in rayTracingObjects)
        {
            Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;

            // Добавляем в списки данные о координатах вершин и текстур
            int firstVertex = vertices.Count;
            vertices.AddRange(mesh.vertices);
            verticesUV.AddRange(mesh.uv);

            //Запоминаем сдвиги и добавляем инфу об индексах в список
            int firstIndex = Raytracer.indices.Count;
            var indices = mesh.GetIndices(0);
            Raytracer.indices.AddRange(indices.Select(index => index + firstVertex));

            //добавляем инфу о цветах граней в список
            var colors = obj.facesColor;
            Raytracer.colors.AddRange(colors);

            //инициализируем структуру meshRT и добавляем в список
            meshObjects.Add(new MeshRT()
            {
                localToWorldMatrix = obj.transform.localToWorldMatrix,
                indices_offset = firstIndex,
                indices_count = indices.Length,
                transparency = obj.transparency,
            });
        }

        //По полученным новым списка создаем буферы
        CreateComputeBuffer(ref meshObjectBuffer, meshObjects, MeshRT.GetSize()); //sizeof(float*4*4 + int + int + float + float)
        CreateComputeBuffer(ref vertexBuffer, vertices, sizeof(float) * 3); //sizeof(float*3)
        CreateComputeBuffer(ref vertexUVBuffer, verticesUV, sizeof(float) * 2); //sizeof(float*3)
        CreateComputeBuffer(ref indexBuffer, indices, sizeof(int)); //sizeof(float)
        CreateComputeBuffer(ref colorBuffer, colors, sizeof(int)); //sizeof(float)
    }


    //Метод для создания буфера на основе его ссылки, списка с данными и размера данных
    private static void CreateComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride)
    where T : struct
    {
        //если буфер уже имеется
        if (buffer != null)
        {
            //Если буфер не имеет нужную структуру, то сбросить буфер
            if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride)
            {
                buffer.Release();
                buffer = null;
            }
        }

        //если список не пустой
        if (data.Count != 0)
        {
            //если буфер пустой, тогда создать его
            if (buffer == null)
            {
                buffer = new ComputeBuffer(data.Count, stride);
            }

            //Занести данные в буфер
            buffer.SetData(data);
        }
    }


    //Метод передает буфер в шейдер по имени поля в шейдере и буферу
    private void SetComputeBuffer(string name, ComputeBuffer buffer)
    {
        //если буфер не пустой
        if (buffer != null)
        {
            //установить поле шейдера такого же типа в значение данного буфера
            raytracingShader.SetBuffer(0, name, buffer);
        }
    }


    //Метод устанавливает только важные параметры шейдера
    private void SetUpdatableShaderParameters()
    {
        //Установка значения буфера с MeshRT, где есть важная матрица преобразований
        SetComputeBuffer("_MeshObjects", meshObjectBuffer);
    }


    //Уставнока всех параметров шейдера
    private void SetAllShaderParameters()
    {
        //Установка парамтрой камеры
        raytracingShader.SetMatrix("_CameraToWorld",
            componentCamera.cameraToWorldMatrix);
        raytracingShader.SetMatrix("_CameraInverseProjection",
            componentCamera.projectionMatrix.inverse);
        raytracingShader.SetTexture(0, "_SkyboxTexture",
            skyboxTexture);
        raytracingShader.SetInt("_TransparencyCount",
            transparencyCount);

        //Установка параметров света
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

        //Уставнока параметров мешей объектов
        SetUpdatableShaderParameters();
        raytracingShader.SetFloat("_selectedFace", selectedFace);
        SetComputeBuffer("_Vertices", vertexBuffer);
        SetComputeBuffer("_VerticesUV", vertexUVBuffer);
        SetComputeBuffer("_Indices", indexBuffer);
        SetComputeBuffer("_Colors", colorBuffer);

        //Установка текстур и их свойств комнаты
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


    //Метод иницииализирует текстуру рейндеринга, которую будет считать шейдер
    private void Init()
    {
        int width = Screen.width > 1920 ? 1920 : Screen.width;
        int height = Screen.height > 960 ? 960 : Screen.height;
        //Есл текстура еще не была проинициализирвоана или ее параметры не соответствую текущему экрану*натсройка качества
        if (target == null || target.width != (int)(width * CurrentData.Data.currentSettings.graphicsOption) 
            || target.height != (int)(height * CurrentData.Data.currentSettings.graphicsOption))
        {
            //если текстура не пуста, очистить ее
            if (target != null)
            {
                target.Release();
            }

           //инициализируем текстуру рендеринга размерам и иансттройками цветапередачи
            target = new RenderTexture((int)(width * CurrentData.Data.currentSettings.graphicsOption),
                (int)(height * CurrentData.Data.currentSettings.graphicsOption),
                0,
                RenderTextureFormat.ARGBFloat,
                RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }
    }


    //Метод занимается рендером текстуры 
    private void Render(RenderTexture destination)
    {
        //инициализируем текстуру о необходимости
        Init();

        int width = Screen.width > 1920 ? 1920 : Screen.width;
        int height = Screen.height > 960 ? 960 : Screen.height;

        //Устанавливаем текстуру рендеринга шейдеру как оле для записи результата
        raytracingShader.SetTexture(0, "Result", target);
        //делим экран на части для улучшения многопоточности
        int threadGroupsX = Mathf.CeilToInt((int)(width * CurrentData.Data.currentSettings.graphicsOption) / 8.0f);
        int threadGroupsY = Mathf.CeilToInt((int)(height * CurrentData.Data.currentSettings.graphicsOption) / 8.0f);
        //запускаем расчет шейдера
        raytracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        //копируем полученную текстуру в текстуру, которую отображает камера на экран
        Graphics.Blit(target, destination);
    }


    //Базовый метод Unity который вызывается после прорисовки для постобработки изображения на экране.
    //параметры: исходное ихображение и изображение для изменений
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //если надо перестроить буферы или это режим создания
        if (meshObjectsNeedRebuilding || creatingMode)
        {
            meshObjectsNeedRebuilding = false; //без повторной перестройки
            RebuildAllObjectBuffers(); //перестроить все буферы
            SetAllShaderParameters(); //уставноить все параметры
        }
        else
        {
            RebuildMeshObjectBuffer(); //перстроить только матрицу преобразований
            SetUpdatableShaderParameters(); //обновить данные о матрице в шейдере
        }
        //получаем изображение используя  шейдер
        Render(destination); 
        //Если еще не построили подсказку, но строим ее
        if (spriteHint == null) 
        {
            CreateSpriteHint();
        }
    }


    //Метод который вырезает из результирующей текстуры кусок с тенью для подсказки
    private void CreateSpriteHint()
    {
        //задаем размеры текстуры
        int width = Screen.width, height = Screen.height;
        int right = (int)(width * 0.425), left = (int)(width * 0.225);
        int down = (int)(height * 0.3), up = (int)(height * 0.7);
        width = right - left;
        height = up - down;
        //создаем текстуру
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        //читаем пискели с экрана в указанной области
        tex.ReadPixels(new Rect(left, down, width, height), 0, 0);
        //применяем
        tex.Apply();
        //создаем спрайт
        spriteHint = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }
}
  