using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AtlasCreator:EditorWindow
{
    private const int MAX_ATLAS_SIZE = 4096;
    private const int TEXTURE_SIZE = 64;
    private const int MIN_ATLAS_SIZE = 256;
    private const string BASE_TEXTURES_PATH = "Assets/Editor/Atlas/Blocks/";
    private const string BASE_ATLASES_PATH = "Assets/Scripts/WorldGenerator/Resources/Atlases/";
    private const string MODS_ATLASES_PATH = "Mods/Atlases/";

    [System.Serializable]
    public class BlockMetadata
    {
        public string name;
        public string displayName;
        public bool isTransparent;
        public bool isSolid;
    }

    [System.Serializable]
    public class AtlasManifest
    {
        public string atlasName;
        public int atlasSize;
        public List<BlockEntry> blocks = new List<BlockEntry>();

        [System.Serializable]
        public class BlockEntry
        {
            public int blockId;
            public string name;
            public string displayName;
            public bool isTransparent;
            public bool isSolid;
            public List<string> faces;
        }
    }

    [System.Serializable]
    public struct SimpleRect
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public SimpleRect(Rect rect)
        {
            x = rect.x;
            y = rect.y;
            width = rect.width;
            height = rect.height;
        }

        public Rect ToRect()
        {
            return new Rect(x, y, width, height);
        }
    }

    [System.Serializable]
    public class FaceUvData
    {
        public SimpleRect allFaces;
        public Dictionary<string, SimpleRect> faces;
    }

    [System.Serializable]
    private class UvMapWrapper
    {
        public List<UvMapEntry> entries = new List<UvMapEntry>();
    }

    [System.Serializable]
    private class UvMapEntry
    {
        public int materialId;
        public FaceUvData uvData;
    }

    private class TextureData
    {
        public int materialId;
        public Dictionary<string, Texture2D> textures;
        public string sourceDirectory;
    }

    private string lastGeneratedAtlasPath;

    [MenuItem("Door Tools/Base Atlas Generator")]
    public static void ShowWindow()
    {
        var window = GetWindow<AtlasCreator>("Base Atlas Generator");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Base Atlas Generator", EditorStyles.boldLabel);

        if (GUILayout.Button("Сгенерировать базовые атласы"))
        {
            GenerateAllBaseAtlases();
            if (!string.IsNullOrEmpty(lastGeneratedAtlasPath))
            {
                FocusOnAtlas(lastGeneratedAtlasPath);
            }
        }
    }

    private void FocusOnAtlas(string atlasPath)
    {
        var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
        if (atlas != null)
        {
            Selection.activeObject = atlas;
            EditorGUIUtility.PingObject(atlas);
        }
    }

    private void GenerateAllBaseAtlases()
    {
        lastGeneratedAtlasPath = null;

        if (Directory.Exists(BASE_ATLASES_PATH))
        {
            Directory.Delete(BASE_ATLASES_PATH, true);
            Directory.CreateDirectory(BASE_ATLASES_PATH);
        }
        else
        {
            Directory.CreateDirectory(BASE_ATLASES_PATH);
        }

        var allTexturesData = CollectAllTexturesData(BASE_TEXTURES_PATH);
        var atlasGroups = GroupTexturesForAtlases(allTexturesData);

        int atlasIndex = 1;
        foreach (var group in atlasGroups)
        {
            CreateBaseAtlas(group, atlasIndex++);
        }

        AssetDatabase.Refresh();
        Debug.Log($"Успешно сгененировано {atlasGroups.Count} базовых атласов.");
    }

    private void CreateBaseAtlas(List<TextureData> textureDataGroup, int atlasIndex)
    {
        int textureCount = textureDataGroup.Sum(td => td.textures.Count);
        int atlasSize = CalculateAtlasSize(textureCount);

        Texture2D atlas = new Texture2D(atlasSize, atlasSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };

        var uvMap = FillAtlas(atlas, textureDataGroup, atlasSize);
        string atlasName = $"atlas{atlasIndex}";

        var manifest = CreateManifest(textureDataGroup, atlasName, atlasSize);

        SaveAtlas(atlas, atlasName, BASE_ATLASES_PATH);
        SaveUvMap(uvMap, atlasName, BASE_ATLASES_PATH);
        SaveManifest(manifest, $"{atlasName}_manifest.json", BASE_ATLASES_PATH);
    }

    private void SaveUvMap(Dictionary<int, FaceUvData> uvMap, string atlasName, string outputPath)
    {
        string path = Path.Combine(outputPath, $"{atlasName}_uv.json");

        var wrapper = new UvMapWrapper();

        foreach (var entry in uvMap)
        {
            var uvData = new FaceUvData();

            if (!entry.Value.allFaces.Equals(default(SimpleRect)))
            {
                uvData.allFaces = entry.Value.allFaces;
            }

            if (entry.Value.faces != null && entry.Value.faces.Count > 0)
            {
                uvData.faces = new Dictionary<string, SimpleRect>(entry.Value.faces);
            }

            wrapper.entries.Add(new UvMapEntry
            {
                materialId = entry.Key,
                uvData = uvData
            });
        }

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(path, json);
    }

    private Dictionary<int, FaceUvData> FillAtlas(Texture2D atlas, List<TextureData> textureDataGroup, int atlasSize)
    {
        var uvMap = new Dictionary<int, FaceUvData>();
        int x = 0, y = 0;
        int tilesPerRow = atlasSize / TEXTURE_SIZE;

        foreach (var data in textureDataGroup)
        {
            var faceUvs = new FaceUvData();

            if (data.textures.ContainsKey("all"))
            {
                var uv = AddTextureToAtlas(atlas, data.textures["all"], ref x, ref y, tilesPerRow, atlasSize);
                faceUvs.allFaces = new SimpleRect(uv);
            }
            else
            {
                faceUvs.faces = new Dictionary<string, SimpleRect>();
                foreach (var face in data.textures)
                {
                    var uv = AddTextureToAtlas(atlas, face.Value, ref x, ref y, tilesPerRow, atlasSize);
                    faceUvs.faces.Add(face.Key, new SimpleRect(uv));
                }
            }

            uvMap.Add(data.materialId, faceUvs);
        }

        return uvMap;
    }

    private Rect AddTextureToAtlas(Texture2D atlas, Texture2D texture, ref int x, ref int y, int tilesPerRow, int atlasSize)
    {
        if (texture == null)
        {
            Debug.LogError("Попытка добавить текстуры без данных в атлас!");
            return new Rect();
        }

        int invertedY = (tilesPerRow - 1) - y;

        var uv = new Rect(
            x * TEXTURE_SIZE / (float) atlasSize,
            invertedY * TEXTURE_SIZE / (float) atlasSize,
            TEXTURE_SIZE / (float) atlasSize,
            TEXTURE_SIZE / (float) atlasSize
        );

        for (int texY = 0; texY < TEXTURE_SIZE; texY++)
        {
            for (int texX = 0; texX < TEXTURE_SIZE; texX++)
            {
                Color pixel = texture.GetPixel(texX, texY);
                atlas.SetPixel(
                    x * TEXTURE_SIZE + texX,
                    invertedY * TEXTURE_SIZE + texY,
                    pixel);
            }
        }

        if (++x >= tilesPerRow)
        {
            x = 0;
            y++;
        }

        return uv;
    }

    private AtlasManifest CreateManifest(List<TextureData> textureDataGroup, string atlasName, int atlasSize)
    {
        var manifest = new AtlasManifest
        {
            atlasName = atlasName,
            atlasSize = atlasSize
        };

        foreach (var data in textureDataGroup)
        {
            BlockMetadata metadata = LoadBlockMetadata(data.sourceDirectory);

            var entry = new AtlasManifest.BlockEntry
            {
                blockId = data.materialId,
                name = metadata?.name ?? $"block_{data.materialId}",
                displayName = metadata?.displayName ?? $"Block {data.materialId}",
                isTransparent = metadata?.isTransparent ?? false,
                isSolid = metadata?.isSolid ?? true,
                faces = data.textures.Keys.ToList()
            };
            manifest.blocks.Add(entry);
        }

        return manifest;
    }

    private void SaveAtlas(Texture2D atlas, string atlasName, string outputPath)
    {
        atlas.Apply();
        string path = Path.Combine(outputPath, $"{atlasName}.png");
        File.WriteAllBytes(path, atlas.EncodeToPNG());
        lastGeneratedAtlasPath = path;

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private void SaveManifest(AtlasManifest manifest, string fileName, string outputPath)
    {
        string path = Path.Combine(outputPath, fileName);
        File.WriteAllText(path, JsonUtility.ToJson(manifest, true));
    }

    private List<TextureData> CollectAllTexturesData(string basePath)
    {
        var textureData = new List<TextureData>();

        foreach (var materialDir in Directory.GetDirectories(basePath))
        {
            string dirName = Path.GetFileName(materialDir);
            if (!int.TryParse(dirName, out int materialId))
            {
                Debug.LogWarning($"Некорректное название директории с блоком (должна быть численная): {dirName}");
                continue;
            }

            var files = Directory.GetFiles(materialDir, "*.png");
            if (files.Length == 0)
                continue;

            var textures = new Dictionary<string, Texture2D>();

            if (files.Length == 1)
            {
                textures.Add("all", LoadAndConvertTexture(files[0]));
            }
            else if (files.Length == 3)
            {
                textures.Add("top", LoadAndConvertTexture(FindFile(files, "top")));
                textures.Add("bottom", LoadAndConvertTexture(FindFile(files, "bottom")));
                textures.Add("side", LoadAndConvertTexture(FindFile(files, "side")));
            }
            else if (files.Length == 6)
            {
                textures.Add("top", LoadAndConvertTexture(FindFile(files, "top")));
                textures.Add("bottom", LoadAndConvertTexture(FindFile(files, "bottom")));
                textures.Add("front", LoadAndConvertTexture(FindFile(files, "front")));
                textures.Add("back", LoadAndConvertTexture(FindFile(files, "back")));
                textures.Add("left", LoadAndConvertTexture(FindFile(files, "left")));
                textures.Add("right", LoadAndConvertTexture(FindFile(files, "right")));
            }
            else
            {
                Debug.LogError($"Неподдерживаемая конфигурация количества текстур в директории {materialDir}. Должно быть 1, 3 или 6 текстур.");
                continue;
            }

            textureData.Add(new TextureData
            {
                materialId = materialId,
                textures = textures,
                sourceDirectory = materialDir
            });
        }

        return textureData;
    }

    private Texture2D LoadAndConvertTexture(string path)
    {
        var originalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (originalTex == null)
        {
            Debug.LogError($"Не получилось загрузить текстуру: {path}");
            return null;
        }

        RenderTexture rt = RenderTexture.GetTemporary(
            originalTex.width,
            originalTex.height,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.sRGB
        );

        Graphics.Blit(originalTex, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D newTexture = new Texture2D(
            originalTex.width,
            originalTex.height,
            TextureFormat.RGBA32,
            false,
            true
        );

        newTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        newTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return newTexture;
    }

    private List<List<TextureData>> GroupTexturesForAtlases(List<TextureData> allTextures)
    {
        var groups = new List<List<TextureData>>();
        var currentGroup = new List<TextureData>();
        int currentGroupSize = 0;

        foreach (var data in allTextures.OrderBy(d => d.materialId))
        {
            int texturesCount = data.textures.Count;

            if (currentGroupSize + texturesCount > GetMaxTexturesPerAtlas())
            {
                groups.Add(currentGroup);
                currentGroup = new List<TextureData>();
                currentGroupSize = 0;
            }

            currentGroup.Add(data);
            currentGroupSize += texturesCount;
        }

        if (currentGroup.Count > 0)
        {
            groups.Add(currentGroup);
        }

        return groups;
    }

    private int GetMaxTexturesPerAtlas()
    {
        return (MAX_ATLAS_SIZE / TEXTURE_SIZE) * (MAX_ATLAS_SIZE / TEXTURE_SIZE);
    }

    private int CalculateAtlasSize(int textureCount)
    {
        int tilesPerRow = Mathf.CeilToInt(Mathf.Sqrt(textureCount));
        int atlasSize = Mathf.Clamp(
            Mathf.NextPowerOfTwo(tilesPerRow * TEXTURE_SIZE),
            MIN_ATLAS_SIZE,
            MAX_ATLAS_SIZE
        );

        return atlasSize;
    }

    private BlockMetadata LoadBlockMetadata(string blockDirectory)
    {
        string metadataPath = Path.Combine(blockDirectory, "metadata.json");
        if (File.Exists(metadataPath))
        {
            string json = File.ReadAllText(metadataPath);
            return JsonUtility.FromJson<BlockMetadata>(json);
        }
        return null;
    }

    private string FindFile(string[] files, string namePart)
    {
        foreach (var file in files)
            if (Path.GetFileNameWithoutExtension(file).ToLower().Contains(namePart))
                return file;
        Debug.LogError($"Файл для стороны блока: '{namePart}', не найден!");
        return null;
    }
}