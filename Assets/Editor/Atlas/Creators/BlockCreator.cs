using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class BlockCreator : EditorWindow
{
    private const string BLOCKS_ROOT = "Assets/Editor/Atlas/Blocks/";
    //���������� ������ �����
    private string blockName = "New Block";
    private string displayName = "����� ����";
    private bool isTransparent = false;
    public bool isParalax = false;
    public bool isRefract = false;
    public bool isReflect = false;
    public bool isSolid = false;
    public bool isLoose = false;
    public bool isLiquid = false;
    public bool isGas = false;
    public bool isThrough = false;
    private TextureType textureType = TextureType.Single;
    private Texture2D singleTexture;
    private Texture2D topTexture;
    private Texture2D bottomTexture;
    private Texture2D sideTexture;
    private Texture2D frontTexture;
    private Texture2D backTexture;
    private Texture2D leftTexture;
    private Texture2D rightTexture;
    //�������������� �����
    private int selectedBlockId = -1;
    private Dictionary<int, string> existingBlocks = new Dictionary<int, string>();
    private Vector2 scrollPosition;

    private int selectedTab = -1;

    private enum TextureType
    {
        Single,
        ThreeSides,
        SixSides
    }

    [MenuItem("Door Tools/Blocks Textures Manager")]
    public static void ShowWindow()
    {
        GetWindow<BlockCreator>("Blocks Textures Manager");
    }

    private void OnEnable()
    {
        RefreshExistingBlocks();
    }

    private void RefreshExistingBlocks()
    {
        existingBlocks.Clear();
        if (Directory.Exists(BLOCKS_ROOT))
        {
            foreach (var dir in Directory.GetDirectories(BLOCKS_ROOT))
            {
                if (int.TryParse(Path.GetFileName(dir), out int id))
                {
                    string metaPath = Path.Combine(dir, "metadata.json");
                    if (File.Exists(metaPath))
                    {
                        string json = File.ReadAllText(metaPath);
                        var meta = JsonUtility.FromJson<BlockMetadata>(json);
                        existingBlocks.Add(id, $"{id}: {meta.displayName}");
                    }
                    else
                    {
                        existingBlocks.Add(id, $"{id}: (no metadata)");
                    }
                }
            }
        }
    }

    [System.Serializable]
    private class BlockMetadata
    {
        public string name;
        public string displayName;
        public bool isTransparent;
        public bool isParalax;
        public bool isRefract;
        public bool isReflect;
        public bool isSolid;
        public bool isLoose;
        public bool isLiquid;
        public bool isGas;
        public bool isThrough;
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // �������
        string[] tabs = { "Add New Block", "Edit Block", "Delete Block" };
        selectedTab = GUILayout.Toolbar(selectedTab, tabs);

        EditorGUILayout.Space(10);

        switch (selectedTab)
        {
            case 0:
            DrawAddNewBlockTab();
            break;
            case 1:
            DrawEditBlockTab();
            break;
            case 2:
            DrawDeleteBlockTab();
            break;
        }

        EditorGUILayout.EndScrollView();
    }

    //������ ���������� �����
    private void DrawAddNewBlockTab()
    {
        GUILayout.Label("�������� ����� ����", EditorStyles.boldLabel);
        blockName = EditorGUILayout.TextField("����������� ��������", blockName);
        displayName = EditorGUILayout.TextField("������������ ��������", displayName);
        isTransparent = EditorGUILayout.Toggle("������������", isTransparent);
        isParalax = EditorGUILayout.Toggle("���������", isParalax);
        isRefract = EditorGUILayout.Toggle("�����������", isRefract);
        isReflect = EditorGUILayout.Toggle("���������", isReflect);
        isSolid = EditorGUILayout.Toggle("�������", isSolid);
        isLoose = EditorGUILayout.Toggle("������", isLoose);
        isLiquid = EditorGUILayout.Toggle("��������", isLiquid);
        isGas = EditorGUILayout.Toggle("���", isGas);
        isThrough = EditorGUILayout.Toggle("����������", isThrough);

        EditorGUILayout.Space();
        GUILayout.Label("��������� ��������", EditorStyles.boldLabel);
        textureType = (TextureType) EditorGUILayout.EnumPopup("��� ��������", textureType);

        switch (textureType)
        {
            case TextureType.Single:
            singleTexture = (Texture2D) EditorGUILayout.ObjectField("��������", singleTexture, typeof(Texture2D), false);
            break;

            case TextureType.ThreeSides:
            topTexture = (Texture2D) EditorGUILayout.ObjectField("������� �������", topTexture, typeof(Texture2D), false);
            bottomTexture = (Texture2D) EditorGUILayout.ObjectField("������ �������", bottomTexture, typeof(Texture2D), false);
            sideTexture = (Texture2D) EditorGUILayout.ObjectField("������� �������", sideTexture, typeof(Texture2D), false);
            break;

            case TextureType.SixSides:
            topTexture = (Texture2D) EditorGUILayout.ObjectField("������� �������", topTexture, typeof(Texture2D), false);
            bottomTexture = (Texture2D) EditorGUILayout.ObjectField("������ �������", bottomTexture, typeof(Texture2D), false);
            frontTexture = (Texture2D) EditorGUILayout.ObjectField("������� �������", frontTexture, typeof(Texture2D), false);
            backTexture = (Texture2D) EditorGUILayout.ObjectField("������� �������", backTexture, typeof(Texture2D), false);
            leftTexture = (Texture2D) EditorGUILayout.ObjectField("����� �������", leftTexture, typeof(Texture2D), false);
            rightTexture = (Texture2D) EditorGUILayout.ObjectField("������ �������", rightTexture, typeof(Texture2D), false);
            break;
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Create Block"))
        {
            CreateBlock();
        }
    }

    private void DrawEditBlockTab()
    {
        GUILayout.Label("�������� ������������ ����", EditorStyles.boldLabel);

        // ����� ����� ��� ��������������
        if (existingBlocks.Count == 0)
        {
            EditorGUILayout.HelpBox("���� ��� �������������� �� ������.", MessageType.Info);
            return;
        }

        string[] blockOptions = existingBlocks.Values.ToArray();
        int selectedIndex = EditorGUILayout.Popup("������� ����",
            selectedBlockId == -1 ? 0 : existingBlocks.Keys.ToList().IndexOf(selectedBlockId),
            blockOptions);

        selectedBlockId = existingBlocks.Keys.ElementAt(selectedIndex);

        if (selectedBlockId == -1)
            return;

        // �������� ������ ���������� �����
        string blockDir = Path.Combine(BLOCKS_ROOT, selectedBlockId.ToString());
        string metaPath = Path.Combine(blockDir, "metadata.json");
        BlockMetadata meta = JsonUtility.FromJson<BlockMetadata>(File.ReadAllText(metaPath));

        // ����������� ���� �������
        var textureFiles = Directory.GetFiles(blockDir, "*.png");
        TextureType detectedType = DetectTextureType(textureFiles);

        // ����������� ����� ��� ��������������
        EditorGUI.BeginChangeCheck();

        meta.name = EditorGUILayout.TextField("����������� ��������", meta.name);
        meta.displayName = EditorGUILayout.TextField("������������ ��������", meta.displayName);
        meta.isTransparent = EditorGUILayout.Toggle("������������", meta.isTransparent);
        meta.isParalax = EditorGUILayout.Toggle("���������", meta.isParalax);
        meta.isRefract = EditorGUILayout.Toggle("�����������", meta.isRefract);
        meta.isReflect = EditorGUILayout.Toggle("���������", meta.isReflect);
        meta.isSolid = EditorGUILayout.Toggle("�������", meta.isSolid);
        meta.isLoose = EditorGUILayout.Toggle("������", meta.isLoose);
        meta.isLiquid = EditorGUILayout.Toggle("��������", meta.isLiquid);
        meta.isGas = EditorGUILayout.Toggle("���", meta.isGas);
        meta.isThrough = EditorGUILayout.Toggle("����������", meta.isThrough);

        EditorGUILayout.Space();
        GUILayout.Label("��������� ��������", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("��� ��������", detectedType.ToString());

        switch (detectedType)
        {
            case TextureType.Single:
            var singleTexPath = Path.Combine(blockDir, "all.png");
            var singleTex = AssetDatabase.LoadAssetAtPath<Texture2D>(singleTexPath);
            singleTexture = (Texture2D) EditorGUILayout.ObjectField("��������", singleTex, typeof(Texture2D), false);
            break;

            case TextureType.ThreeSides:
            topTexture = (Texture2D) EditorGUILayout.ObjectField("������� �������",
                AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(blockDir, "top.png")), typeof(Texture2D), false);
            bottomTexture = (Texture2D) EditorGUILayout.ObjectField("������ �������",
                AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(blockDir, "bottom.png")), typeof(Texture2D), false);
            sideTexture = (Texture2D) EditorGUILayout.ObjectField("������� �������",
                AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(blockDir, "side.png")), typeof(Texture2D), false);
            break;

            case TextureType.SixSides:
            topTexture = (Texture2D) EditorGUILayout.ObjectField("������� �������",
                AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(blockDir, "top.png")), typeof(Texture2D), false);
            bottomTexture = (Texture2D) EditorGUILayout.ObjectField("������ �������",
                AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(blockDir, "bottom.png")), typeof(Texture2D), false);
            frontTexture = (Texture2D) EditorGUILayout.ObjectField("������� �������",
                AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(blockDir, "front.png")), typeof(Texture2D), false);
            backTexture = (Texture2D) EditorGUILayout.ObjectField("������� �������",
                AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(blockDir, "back.png")), typeof(Texture2D), false);
            leftTexture = (Texture2D) EditorGUILayout.ObjectField("����� �������",
                AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(blockDir, "left.png")), typeof(Texture2D), false);
            rightTexture = (Texture2D) EditorGUILayout.ObjectField("������ �������",
                AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(blockDir, "right.png")), typeof(Texture2D), false);
            break;
        }

        EditorGUILayout.Space();
        if (EditorGUI.EndChangeCheck())
        {
            if (GUILayout.Button("Save Changes"))
            {
                SaveBlockChanges(selectedBlockId, meta, detectedType);
            }
        }
    }

    private void DrawDeleteBlockTab()
    {
        GUILayout.Label("������� ����", EditorStyles.boldLabel);

        if (existingBlocks.Count == 0)
        {
            EditorGUILayout.HelpBox("�� ������ ���� ��� ��������.", MessageType.Info);
            return;
        }

        string[] blockOptions = existingBlocks.Values.ToArray();
        int selectedIndex = EditorGUILayout.Popup("������� ���� ��� ��������",
            selectedBlockId == -1 ? 0 : existingBlocks.Keys.ToList().IndexOf(selectedBlockId),
            blockOptions);

        selectedBlockId = existingBlocks.Keys.ElementAt(selectedIndex);

        EditorGUILayout.Space();
        if (GUILayout.Button("������� ��������� ����"))
        {
            if (EditorUtility.DisplayDialog("����������� ��������",
                $"������������� ������� ����: {selectedBlockId}? ���� ����� �������� ������ �������.",
                "�������", "��������"))
            {
                DeleteBlock(selectedBlockId);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("��������: �������� ����� ������������ �������������� ���� ��������������� ��� ���������� ������!", MessageType.Warning);
    }

    private TextureType DetectTextureType(string[] textureFiles)
    {
        if (textureFiles.Length == 1 && Path.GetFileName(textureFiles[0]).Equals("all.png"))
            return TextureType.Single;
        if (textureFiles.Length == 3)
            return TextureType.ThreeSides;
        if (textureFiles.Length == 6)
            return TextureType.SixSides;
        return TextureType.Single;
    }
    private void CreateBlock()
    {
        int newId = GetNextBlockId();
        string blockDir = Path.Combine(BLOCKS_ROOT, newId.ToString());
        Directory.CreateDirectory(blockDir);

        try
        {
            switch (textureType)
            {
                case TextureType.Single:
                if (singleTexture == null)
                    throw new System.Exception("���������� ���� ��������");
                SaveTexture(singleTexture, Path.Combine(blockDir, "all.png"));
                break;

                case TextureType.ThreeSides:
                if (topTexture == null || bottomTexture == null || sideTexture == null)
                    throw new System.Exception("���������� ��� ��������");
                SaveTexture(topTexture, Path.Combine(blockDir, "top.png"));
                SaveTexture(bottomTexture, Path.Combine(blockDir, "bottom.png"));
                SaveTexture(sideTexture, Path.Combine(blockDir, "side.png"));
                break;

                case TextureType.SixSides:
                if (topTexture == null || bottomTexture == null || frontTexture == null ||
                    backTexture == null || leftTexture == null || rightTexture == null)
                    throw new System.Exception("���������� ����� �������");
                SaveTexture(topTexture, Path.Combine(blockDir, "top.png"));
                SaveTexture(bottomTexture, Path.Combine(blockDir, "bottom.png"));
                SaveTexture(frontTexture, Path.Combine(blockDir, "front.png"));
                SaveTexture(backTexture, Path.Combine(blockDir, "back.png"));
                SaveTexture(leftTexture, Path.Combine(blockDir, "left.png"));
                SaveTexture(rightTexture, Path.Combine(blockDir, "right.png"));
                break;
            }

            CreateMetadata(blockDir, newId, new BlockMetadata
            {
                name = blockName,
                displayName = displayName,
                isTransparent = isTransparent,
                isParalax = isParalax,
                isRefract = isRefract,
                isReflect = isReflect,
                isSolid = isSolid,
                isLoose = isLoose,
                isLiquid = isLiquid,
                isGas = isGas,
                isThrough = isThrough
            });

            AssetDatabase.Refresh();
            RefreshExistingBlocks();
            Debug.Log($"������ ���� � ID {newId}");
        }
        catch (System.Exception e)
        {
            Directory.Delete(blockDir, true);
            EditorUtility.DisplayDialog("������!", e.Message, "OK");
        }
    }

    private void SaveBlockChanges(int blockId, BlockMetadata meta, TextureType textureType)
    {
        string blockDir = Path.Combine(BLOCKS_ROOT, blockId.ToString());

        try
        {
            // ��������� ����������
            CreateMetadata(blockDir, blockId, meta);

            // ��������� �������� (���� ��� ���� ��������)
            switch (textureType)
            {
                case TextureType.Single:
                if (singleTexture != null)
                    SaveTexture(singleTexture, Path.Combine(blockDir, "all.png"));
                break;

                case TextureType.ThreeSides:
                if (topTexture != null)
                    SaveTexture(topTexture, Path.Combine(blockDir, "top.png"));
                if (bottomTexture != null)
                    SaveTexture(bottomTexture, Path.Combine(blockDir, "bottom.png"));
                if (sideTexture != null)
                    SaveTexture(sideTexture, Path.Combine(blockDir, "side.png"));
                break;

                case TextureType.SixSides:
                if (topTexture != null)
                    SaveTexture(topTexture, Path.Combine(blockDir, "top.png"));
                if (bottomTexture != null)
                    SaveTexture(bottomTexture, Path.Combine(blockDir, "bottom.png"));
                if (frontTexture != null)
                    SaveTexture(frontTexture, Path.Combine(blockDir, "front.png"));
                if (backTexture != null)
                    SaveTexture(backTexture, Path.Combine(blockDir, "back.png"));
                if (leftTexture != null)
                    SaveTexture(leftTexture, Path.Combine(blockDir, "left.png"));
                if (rightTexture != null)
                    SaveTexture(rightTexture, Path.Combine(blockDir, "right.png"));
                break;
            }

            AssetDatabase.Refresh();
            RefreshExistingBlocks();
            EditorUtility.DisplayDialog("�������!", "���� ��������", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("������!", e.Message, "OK");
        }
    }

    private void DeleteBlock(int blockId)
    {
        string blockDir = Path.Combine(BLOCKS_ROOT, blockId.ToString());

        // ������� ����� �����
        Directory.Delete(blockDir, true);
        File.Delete(blockDir + ".meta");

        // ���������������� ���������� �����
        RenumberBlocks(blockId);

        AssetDatabase.Refresh();
        RefreshExistingBlocks();
        selectedBlockId = -1;
        EditorUtility.DisplayDialog("�������!", "���� ������, ID ��������� ������ ��������������", "OK");
    }

    private void RenumberBlocks(int deletedId)
    {
        var dirs = Directory.GetDirectories(BLOCKS_ROOT)
            .Select(d => new {
                Path = d,
                Id = int.Parse(Path.GetFileName(d))
            })
            .Where(d => d.Id > deletedId)
            .OrderBy(d => d.Id)
            .ToList();

        foreach (var dir in dirs)
        {
            string newPath = Path.Combine(BLOCKS_ROOT, (dir.Id - 1).ToString());
            Directory.Move(dir.Path, newPath);
            File.Move(dir.Path + ".meta", newPath + ".meta");

            // ��������� ID � metadata.json
            string metaPath = Path.Combine(newPath, "metadata.json");
            if (File.Exists(metaPath))
            {
                // ��� ���������� ������ ������ ������ ������ � ���������� �������
                string json = File.ReadAllText(metaPath);
                File.WriteAllText(metaPath, json);
            }
        }
    }

    private int GetNextBlockId()
    {
        if (!Directory.Exists(BLOCKS_ROOT))
        {
            Directory.CreateDirectory(BLOCKS_ROOT);
            return 1;
        }

        var dirs = Directory.GetDirectories(BLOCKS_ROOT)
            .Select(d => Path.GetFileName(d))
            .Where(d => int.TryParse(d, out _))
            .Select(int.Parse)
            .OrderBy(id => id)
            .ToList();

        return dirs.Count == 0 ? 1 : dirs.Last() + 1;
    }

    private void SaveTexture(Texture2D texture, string path)
    {
        if (texture.width != 64 || texture.height != 64)
        {
            throw new System.Exception($"�������� ������ ���� 64x64 ��������. ������� ������: {texture.width}x{texture.height}");
        }

        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
    }

    private void CreateMetadata(string blockDir, int id, BlockMetadata meta)
    {
        string json = JsonUtility.ToJson(meta, true);
        File.WriteAllText(Path.Combine(blockDir, "metadata.json"), json);
    }
}
