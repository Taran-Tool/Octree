using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ShaderTextureGenerator:EditorWindow
{
    private Shader selectedShader;
    private Material previewMaterial;
    private Texture2D generatedTexture;
    private bool showTiledPreview = false;
    private string textureName = "";
    private Dictionary<string, bool> randomizeFlags = new Dictionary<string, bool>();
    private Vector2 scrollPosition;
    private RenderTexture renderTexture;
    private Vector2 shaderListScroll;
    private List<Shader> availableShaders = new List<Shader>();
    private bool showShaderSelection;

    // Quality settings
    private int superSampleFactor = 2;
    private float sharpness = 0.3f;
    private bool useSharpening = true;
    private FilterMode textureFilterMode = FilterMode.Bilinear;

    [MenuItem("Door Tools/Shader Texture Generator")]
    public static void ShowWindow()
    {
        GetWindow<ShaderTextureGenerator>("Shader Texture Generator");
    }

    private void OnEnable()
    {
        RefreshShaderList();
    }

    private void OnDisable()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            DestroyImmediate(renderTexture);
        }
        if (previewMaterial != null)
        {
            DestroyImmediate(previewMaterial);
        }
    }

    private void RefreshShaderList()
    {
        availableShaders.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Shader", new[] { "Assets/Editor/Atlas/DoorShaders" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (shader != null)
            {
                availableShaders.Add(shader);
            }
        }
    }

    private void OnGUI()
    {
        // Главный вертикальный контейнер
        EditorGUILayout.BeginVertical();

        GUILayout.Label("Генератор текстур", EditorStyles.boldLabel);

        // Шаг 0: Выбор шейдера
        EditorGUILayout.Space(3f);
        GUILayout.Label("Выбор шейдера:", EditorStyles.boldLabel);

        if (selectedShader != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Выбранный шейдер:", selectedShader.name);
            if (GUILayout.Button("Изменить", GUILayout.Width(80)))
            {
                showShaderSelection = !showShaderSelection;
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            if (GUILayout.Button("Выбрать шейдер"))
            {
                showShaderSelection = true;
            }
        }

        if (showShaderSelection)
        {
            EditorGUILayout.Space(3f);
            EditorGUILayout.BeginVertical("Box");
            shaderListScroll = EditorGUILayout.BeginScrollView(shaderListScroll, GUILayout.ExpandHeight(false));

            if (availableShaders.Count == 0)
            {
                EditorGUILayout.HelpBox("Шейдеры в DoorShaders не найдены", MessageType.Info);
            }
            else
            {
                foreach (var shader in availableShaders)
                {
                    if (GUILayout.Button(shader.name, EditorStyles.miniButton))
                    {
                        selectedShader = shader;
                        previewMaterial = new Material(selectedShader);
                        textureName = Path.GetFileNameWithoutExtension(selectedShader.name);
                        InitializeShaderProperties();
                        showShaderSelection = false;
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Обновить список"))
            {
                RefreshShaderList();
            }

            EditorGUILayout.EndVertical();
        }

        if (selectedShader == null)
        {
            EditorGUILayout.HelpBox("Выберите шейдер из DoorShaders", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        // Блок параметров с фиксированной высотой и скроллом
        EditorGUILayout.Space(3f);
        GUILayout.Label("Параметры шейдера:", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(false));

        if (previewMaterial != null)
        {
            int propertyCount = ShaderUtil.GetPropertyCount(selectedShader);
            for (int i = 0; i < propertyCount; i++)
            {
                string propName = ShaderUtil.GetPropertyName(selectedShader, i);
                string displayName = ShaderUtil.GetPropertyDescription(selectedShader, i);

                if (!displayName.Contains("__"))
                    continue;

                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                // Название параметра
                EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.labelWidth));
                EditorGUILayout.LabelField(displayName, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndVertical();
                // Поле ввода
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                switch (ShaderUtil.GetPropertyType(selectedShader, i))
                {
                    case ShaderUtil.ShaderPropertyType.Color:
                    previewMaterial.SetColor(propName, EditorGUILayout.ColorField(previewMaterial.GetColor(propName)));
                    break;
                    case ShaderUtil.ShaderPropertyType.Float:
                    previewMaterial.SetFloat(propName, EditorGUILayout.FloatField(previewMaterial.GetFloat(propName)));
                    break;
                    case ShaderUtil.ShaderPropertyType.Range:
                    float min = ShaderUtil.GetRangeLimits(selectedShader, i, 0);
                    float max = ShaderUtil.GetRangeLimits(selectedShader, i, 1);
                    previewMaterial.SetFloat(propName, EditorGUILayout.Slider(previewMaterial.GetFloat(propName), min, max));
                    break;
                    case ShaderUtil.ShaderPropertyType.Vector:
                    previewMaterial.SetVector(propName, EditorGUILayout.Vector4Field("", previewMaterial.GetVector(propName)));
                    break;
                    case ShaderUtil.ShaderPropertyType.TexEnv:
                    previewMaterial.SetTexture(propName, (Texture) EditorGUILayout.ObjectField(previewMaterial.GetTexture(propName), typeof(Texture), false));
                    break;
                }
                EditorGUILayout.EndVertical();
                // Toggle Random
                EditorGUILayout.BeginVertical(GUILayout.Width(60));
                randomizeFlags[propName] = EditorGUILayout.Toggle("Случайные", randomizeFlags.ContainsKey(propName) && randomizeFlags[propName]);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();

        // Настройки качества
        EditorGUILayout.Space(3f);
        GUILayout.Label("Настройки качества:", EditorStyles.boldLabel);

        EditorGUIUtility.labelWidth = 150;
        superSampleFactor = EditorGUILayout.IntSlider("Семплирование", superSampleFactor, 1, 4);
        useSharpening = EditorGUILayout.Toggle("Резкость", useSharpening);
        if (useSharpening)
            sharpness = EditorGUILayout.Slider("Коэфициент резкости", sharpness, 0.1f, 1f);

        textureFilterMode = (FilterMode) EditorGUILayout.EnumPopup("Фильтрация", textureFilterMode);

        // Название текстуры и превью
        EditorGUILayout.Space(3f);
        textureName = EditorGUILayout.TextField("Название текстуры:", textureName);
        showTiledPreview = EditorGUILayout.Toggle("3x3 плитка текстур", showTiledPreview);

        // Кнопки управления
        EditorGUILayout.Space(5f);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Сгенерировать текстуру", GUILayout.Height(30)))
            GenerateTexture();

        if (GUILayout.Button("Сохранить текстуру", GUILayout.Height(30)))
            SaveTexture();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(50f);
        // Просмотр результата
        if (generatedTexture != null)
        {
            float size = 128f;
            Rect area = EditorGUILayout.GetControlRect(false, showTiledPreview ? size * 3 : size);
            Rect previewRect = new Rect(
                area.x + (area.width - (showTiledPreview ? size * 3 : size)) / 2,
                area.y,
                showTiledPreview ? size * 3 : size,
                showTiledPreview ? size * 3 : size
            );

            if (showTiledPreview)
            {
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        Rect tileRect = new Rect(
                            previewRect.x + x * size,
                            previewRect.y + y * size,
                            size,
                            size
                        );
                        EditorGUI.DrawPreviewTexture(tileRect, generatedTexture);
                    }
                }
            }
            else
            {
                EditorGUI.DrawPreviewTexture(previewRect, generatedTexture);
            }
        }

        // Завершаем главный контейнер
        EditorGUILayout.EndVertical();
    }

    private void InitializeShaderProperties()
    {
        randomizeFlags.Clear();
        if (selectedShader != null)
        {
            int count = ShaderUtil.GetPropertyCount(selectedShader);
            for (int i = 0; i < count; i++)
            {
                string propName = ShaderUtil.GetPropertyName(selectedShader, i);
                string displayName = ShaderUtil.GetPropertyDescription(selectedShader, i);
                if (displayName.Contains("__"))
                {
                    randomizeFlags[propName] = false;
                }
            }
        }
    }

    private void GenerateTexture()
    {
        if (previewMaterial == null)
            previewMaterial = new Material(selectedShader);

        ApplyRandomValues();

        int renderSize = 64 * superSampleFactor;

        if (renderTexture == null || renderTexture.width != renderSize)
        {
            if (renderTexture != null)
                renderTexture.Release();
            renderTexture = new RenderTexture(renderSize, renderSize, 0);
        }

        RenderTexture.active = renderTexture;
        Graphics.Blit(null, renderTexture, previewMaterial);

        Texture2D hiResTex = new Texture2D(renderSize, renderSize, TextureFormat.RGBA32, false);
        hiResTex.ReadPixels(new Rect(0, 0, renderSize, renderSize), 0, 0);
        hiResTex.Apply();

        generatedTexture = DownsampleTexture(hiResTex, 64, useSharpening);
        RenderTexture.active = null;

        DestroyImmediate(hiResTex);
    }

    private Texture2D DownsampleTexture(Texture2D source, int targetSize, bool sharpen)
    {
        Texture2D result = new Texture2D(targetSize, targetSize, TextureFormat.RGBA32, false);
        float ratio = (float) source.width / targetSize;

        for (int y = 0; y < targetSize; y++)
        {
            for (int x = 0; x < targetSize; x++)
            {
                Color pixel = source.GetPixelBilinear(
                    (x + 0.5f) * ratio / source.width,
                    (y + 0.5f) * ratio / source.height
                );

                if (sharpen)
                {
                    Color blur = (
                        source.GetPixelBilinear((x + 0.5f + 1) * ratio / source.width, (y + 0.5f) * ratio / source.height) +
                        source.GetPixelBilinear((x + 0.5f - 1) * ratio / source.width, (y + 0.5f) * ratio / source.height) +
                        source.GetPixelBilinear((x + 0.5f) * ratio / source.width, (y + 0.5f + 1) * ratio / source.height) +
                        source.GetPixelBilinear((x + 0.5f) * ratio / source.width, (y + 0.5f - 1) * ratio / source.height)
                    ) * 0.25f;

                    pixel += (pixel - blur) * sharpness;
                    pixel = Color.Lerp(pixel, blur, -Mathf.Min(0, sharpness));
                }

                result.SetPixel(x, y, pixel);
            }
        }
        result.Apply();
        return result;
    }

    private void ApplyRandomValues()
    {
        int count = ShaderUtil.GetPropertyCount(selectedShader);
        for (int i = 0; i < count; i++)
        {
            string name = ShaderUtil.GetPropertyName(selectedShader, i);
            if (randomizeFlags.ContainsKey(name) && randomizeFlags[name])
            {
                switch (ShaderUtil.GetPropertyType(selectedShader, i))
                {
                    case ShaderUtil.ShaderPropertyType.Color:
                    previewMaterial.SetColor(name, Random.ColorHSV());
                    break;
                    case ShaderUtil.ShaderPropertyType.Float:
                    previewMaterial.SetFloat(name, Random.value);
                    break;
                    case ShaderUtil.ShaderPropertyType.Range:
                    float min = ShaderUtil.GetRangeLimits(selectedShader, i, 0);
                    float max = ShaderUtil.GetRangeLimits(selectedShader, i, 1);
                    previewMaterial.SetFloat(name, Random.Range(min, max));
                    break;
                    case ShaderUtil.ShaderPropertyType.Vector:
                    previewMaterial.SetVector(name, new Vector4(Random.value, Random.value, Random.value, Random.value));
                    break;
                    case ShaderUtil.ShaderPropertyType.TexEnv:
                    previewMaterial.SetTexture(name, null);
                    break;
                }
            }
        }
    }

    private void SaveTexture()
    {
        if (generatedTexture == null)
            return;

        string folder = "Assets/Editor/Atlas/Textures";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string name = string.IsNullOrEmpty(textureName) ? "Texture_" + Random.Range(1000, 9999) : textureName;
        string path = Path.Combine(folder + "/"+name+"/", $"{name}.png");

        File.WriteAllBytes(path, generatedTexture.EncodeToPNG());
        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.filterMode = textureFilterMode;
            importer.mipmapEnabled = false;
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            AssetDatabase.ImportAsset(path);
        }

        Debug.Log($"Сохранено в: {path}");
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
    }
}