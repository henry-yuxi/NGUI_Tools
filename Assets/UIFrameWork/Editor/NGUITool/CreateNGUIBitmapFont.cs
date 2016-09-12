using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateNGUIBitmapFont {

    [MenuItem("UIFrameWork/NGUI工具/批量创建NGUI美术字体", false, 50)]
    static void BatchCreateFontPrefabs()
    {
        Object[] objs = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
        foreach (Object ob in objs)
        {
            CreateFontPrefab(ob);
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("UIFrameWork/NGUI工具/单个创建NGUI美术字体", false, 51)]
    static void SingleCreateFontPrefab()
    {
        CreateFontPrefab(Selection.activeObject);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 创建字体预制体
    /// </summary>
    /// <param name="ob"></param>
    private static void CreateFontPrefab(Object ob)
    {
        string path = AssetDatabase.GetAssetPath(ob);
        if (string.IsNullOrEmpty(path) || !IsTextureFile(path))  
        {
            Debug.LogError("未选中对象或者选择的对象不是图片");
            return;
        }
        if (Path.GetExtension(path) == ".png")
        {
            ShowProgress(path, 0.5f);
            TextureSetting(path, TextureImporterType.Advanced, TextureImporterFormat.RGBA32, false);

            #region 第一步：根据图片创建材质对象
            Material mat = new Material(Shader.Find("Unlit/Transparent Colored"));
            mat.name = ob.name;
            AssetDatabase.CreateAsset(mat, path.Replace(".png", ".mat"));
            mat.mainTexture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
            #endregion

            GameObject go = null;
            UIFont uiFont = null;
            if ((go = AssetDatabase.LoadAssetAtPath(path.Replace(".png", ".prefab"), typeof(GameObject)) as GameObject) != null)
            {
                uiFont = SetAtlasInfo(go, path, mat);
            }
            else
            {
                go = new GameObject(ob.name);
                go.AddComponent<UIFont>();
                uiFont = SetAtlasInfo(go, path, mat);

                #region 第三步：创建预设
                CreatePrefab(go, ob.name, path);
                #endregion
            }
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }
    }

    /// <summary>
    /// 第二步：给对象添加组件、给材质球关联着色器及纹理同时关联tp产生的坐标信息文件
    /// </summary>
    /// <param name="go"></param>
    /// <param name="path"></param>
    /// <param name="mat"></param>
    /// <returns></returns>
    private static UIFont SetAtlasInfo(GameObject go, string path, Material mat)
    {
        if (AssetDatabase.LoadAssetAtPath(path.Replace(".png", ".fnt"), typeof(TextAsset)))
        {
            UIFont uiFont = go.GetComponent<UIFont>();
            uiFont.material = mat;
            TextAsset data = AssetDatabase.LoadAssetAtPath(path.Replace(".png", ".fnt"), typeof(TextAsset)) as TextAsset;
            BMFontReader.Load(uiFont.bmFont, NGUITools.GetHierarchy(uiFont.gameObject), data.bytes);
            uiFont.MarkAsChanged();
            return uiFont;
        }
        return null;
    }

    /// <summary>
    /// 显示进度条
    /// </summary>
    /// <param name="path"></param>
    /// <param name="val"></param>
    static public void ShowProgress(string path, float val)
    {
        EditorUtility.DisplayProgressBar("批量处理中...", string.Format("Please wait...  Path:{0}", path), val);
    }

    /// <summary>
    /// 创建临时预设
    /// </summary>
    public static Object CreatePrefab(GameObject go, string name, string path)
    {
        Object tmpPrefab = PrefabUtility.CreateEmptyPrefab(path.Replace(".png", ".prefab"));
        tmpPrefab = PrefabUtility.ReplacePrefab(go, tmpPrefab, ReplacePrefabOptions.ConnectToPrefab);
        Object.DestroyImmediate(go);
        return tmpPrefab;
    }

    /// <summary>
    /// 设置图片格式
    /// </summary>
    /// <param name="path"></param>
    /// <param name="mTextureImporterType"></param>
    /// <param name="mTextureImporterFormat"></param>
    /// <param name="readEnable"></param>
    static void TextureSetting(string path, TextureImporterType mTextureImporterType = TextureImporterType.Advanced, TextureImporterFormat mTextureImporterFormat = TextureImporterFormat.RGBA32, bool readEnable = false)
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        if (textureImporter == null) return;
        textureImporter.textureType = mTextureImporterType;
        if (textureImporter.textureType == TextureImporterType.Advanced)
        {
            textureImporter.spriteImportMode = SpriteImportMode.None;
            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = readEnable;
            textureImporter.alphaIsTransparency = false;
        }
        else if (textureImporter.textureType == TextureImporterType.Sprite)
        {
            textureImporter.mipmapEnabled = false;
        }
        textureImporter.SetPlatformTextureSettings("Android", 2048, mTextureImporterFormat);
        textureImporter.SetPlatformTextureSettings("Windows", 2048, mTextureImporterFormat);
        textureImporter.SetPlatformTextureSettings("iPhone", 2048, TextureImporterFormat.PVRTC_RGB4);
        textureImporter.SetAllowsAlphaSplitting(false);

        AssetDatabase.ImportAsset(path);
        //AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 判断是否是图片格式
    /// </summary>
    /// <param name="_path"></param>
    /// <returns></returns>
    static bool IsTextureFile(string _path)
    {
        string path = _path.ToLower();
        return path.EndsWith(".psd") || path.EndsWith(".tga") || path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".dds") || path.EndsWith(".bmp") || path.EndsWith(".tif") || path.EndsWith(".gif");
    }
}
