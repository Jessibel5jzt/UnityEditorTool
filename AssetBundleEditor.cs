using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum PlatformType
{
    Windows,
    Android,
    iOS,
    Linux
}

public enum BuildType
{
    Development,
    Release,
}

public class AssetBundleEditor : EditorWindow
{
    //AssetBundle文件后缀
    private const string _abPostFix = "ab";
    //场景文件后缀
    private const string _scenePostFix = "sc";
    //是否保存ab包至发布目录
    private static bool _release = false;

    private static PlatformType _platformType = PlatformType.Windows;
    private static BuildTarget _buildTarget = BuildTarget.StandaloneWindows64;
    private BuildType _buildType = BuildType.Development;
    private BuildOptions _buildOptions = BuildOptions.AllowDebugging | BuildOptions.Development;
    private BuildAssetBundleOptions _buildAssetBundleOptions = BuildAssetBundleOptions.None;

    // 目录结构固定如下：
    private static string _lowerModuleName = "";     // ModuleX
    private static string _v4Base = "";              // x:/.../V4
    private static string _packageAB = "";           // x:/.../V4/PackagedAB/
    private static string _moduleBase = "";          // x:/.../V4/Lobby(Games)/ModuleX
    private static string _moduleResPath = "";       // x:/.../V4/Lobby(Games)/ModuleX/Assets/Module
    private static string _outputSAPath = "";        // x:/.../V4/Lobby(Games)/ModuleX/Assets/StreamingAssets/ModuleX

    static void Log(string log)
    {
        UnityEngine.Debug.Log(log);
    }

    private static void InitPath()
    {
        var saPath = Application.streamingAssetsPath.Replace("\\", "/");
        var idx = saPath.IndexOf("SM/Lobby");
        if (idx < 0)
        {
            idx = saPath.IndexOf("SM/Games");
        }
        var modBegin = saPath.Substring(idx + 9);
        var v4Idx = saPath.IndexOf("/V4");

        _lowerModuleName = modBegin.Substring(0, modBegin.IndexOf("/")).ToLower();
        _v4Base = saPath.Substring(0, v4Idx + 3);
        _moduleBase = saPath.Substring(0, idx + 9 + _lowerModuleName.Length);
        _moduleResPath = $"{_moduleBase}/Assets/Module";
        _outputSAPath = $"{saPath}/{_lowerModuleName}";
        _packageAB = $"{_v4Base}/PackagedAB/{_platformType}/{_lowerModuleName}";
    }

    #region 工具

    [MenuItem("Tools/打包工具")]
    public static void ShowWindow()
    {
#if UNITY_ANDROID
            _platformType = PlatformType.Android;
			_buildTarget = BuildTarget.Android;
#elif UNITY_IOS
            _platformType = PlatformType.IOS;
			_buildTarget = BuildTarget.StandaloneLinux64;
#elif UNITY_LINUX
		    _platformType = PlatformType.Linux;
			_buildTarget = BuildTarget.iOS;
#else
        _platformType = PlatformType.Windows;
        _buildTarget = BuildTarget.StandaloneWindows64;
#endif
        GetWindowWithRect(typeof(BuildEditor), new Rect(0, 0, 500, 300), false, "打包工具");
    }

    private void OnGUI()
    {
        var ctrlH = GUILayout.Height(30);
        var dropDownW = GUILayout.Width(495);

        _platformType = (PlatformType)EditorGUILayout.EnumPopup("选择平台：", _platformType, dropDownW);
        InitPath();
        EditorGUILayout.Space();

        var showPath = _moduleResPath.Substring(_v4Base.Length - 2);
        var newPath = EditorGUILayout.TextField("资源路径：", showPath);

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("重新标记资源", GUILayout.Width(340), ctrlH))
        {
            ClearAssetBundlesName();
            SetAssetBundleLabels();
        }
        if (GUILayout.Button("清除全部标记", GUILayout.Width(150), ctrlH))
        {
            ClearAssetBundlesName();
        }

        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        _buildType = (BuildType)EditorGUILayout.EnumPopup("BuildType: ", _buildType, dropDownW);
        switch (_buildType)
        {
            case BuildType.Development:
                _buildOptions = BuildOptions.Development | BuildOptions.AutoRunPlayer | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;
                _release = false;
                break;
            case BuildType.Release:
                _buildOptions = BuildOptions.None;
                _release = true;
                break;
        }

        EditorGUILayout.Space();
        _buildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField("BuildOptions(多选): ", _buildAssetBundleOptions, dropDownW);
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("删除并重新打包", GUILayout.Width(340), ctrlH))
        {
            Build(_platformType, _buildAssetBundleOptions, _buildOptions, _release);
        }

        if (GUILayout.Button("删除AB包", GUILayout.Width(150), ctrlH))
        {
            ClearFolder();
        }
        GUILayout.EndHorizontal();
    }

    #endregion

    #region 文件

    /// <summary>
    /// 生成资源清单
    /// </summary>
    /// <param name="dir"></param>
    private static FileInfo GenerateVersionInfo(string dir)
    {
        PackageConfig versionProto = new PackageConfig();;
        GenerateVersionProto(dir, versionProto, "");

        using (FileStream fileStream = new FileStream($"{dir}/{_lowerModuleName}.txt", FileMode.Create))
        {
            StreamWriter sw = new StreamWriter(fileStream);
            foreach (var item in versionProto.FileInfoDict)
            {
                sw.Write($"{item.Key},{item.Value.MD5},{item.Value.Size}\n");
            }
            sw.Flush();
            //关闭写数据流 
            sw.Close();
            return new FileInfo($"{dir}/{_lowerModuleName}.txt");
        }
    }

    /// <summary>
    /// 写入资源包信息字典
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="versionProto"></param>
    /// <param name="relativePath"></param>
    private static void GenerateVersionProto(string dir, PackageConfig versionProto, string relativePath)
    {
        foreach (string file in Directory.GetFiles(dir))
        {
            FileInfo fi = new FileInfo(file);
            if (fi.Name.Equals($"{_lowerModuleName}.manifest")) continue;

            if (fi.Extension == $".{_abPostFix}" || fi.Extension == $".{_scenePostFix}" || fi.Extension == ".manifest")
            {
                string md5 = MD5Helper.FileMD5(file);
                long size = fi.Length;
                string filePath = relativePath == "" ? fi.Name : $"{relativePath}/{fi.Name}";
                versionProto.Size += size;
                versionProto.FileInfoDict.Add(filePath, new FileVersionInfo
                {
                    File = filePath,
                    MD5 = md5,
                    Size = size,
                });
            }
        }

        foreach (string directory in Directory.GetDirectories(dir))
        {
            DirectoryInfo dinfo = new DirectoryInfo(directory);
            string rel = relativePath == "" ? dinfo.Name : $"{relativePath}/{dinfo.Name}";
            GenerateVersionProto($"{dir}/{dinfo.Name}", versionProto, rel);
        }
    }

    #endregion

    #region 标记
    //[MenuItem("AssetBundle/Set AssetBundle Labels（标记）")]
    public static void SetAssetBundleLabels()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
        //1.找到资源文件夹
        //DirectoryInfo directoryInfo = new DirectoryInfo(_moduleResPath);
        DirectoryInfo[] modResFolders = new DirectoryInfo(_moduleResPath).GetDirectories();
        TraverseDirectory(modResFolders);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        Log("资源标记成功");
    }

    /// <summary>
    /// 遍历资源文件
    /// </summary>
    /// <param name="parentDirectories"></param>
    /// <param name="oriDirectory"></param>
    private static void TraverseDirectory(DirectoryInfo[] parentDirectories)
    {
        foreach (DirectoryInfo curDirectoryInfo in parentDirectories)
        {
            if (curDirectoryInfo.Name.EndsWith("Scripts", StringComparison.CurrentCultureIgnoreCase)) continue;
            if (curDirectoryInfo.Name.EndsWith(".vs", StringComparison.CurrentCultureIgnoreCase)) continue;

            DirectoryInfo[] subdirectory = curDirectoryInfo.GetDirectories();
            if (subdirectory.Length > 0)
            {
                TraverseDirectory(subdirectory);
            }

            if (!curDirectoryInfo.Exists) continue;

            Dictionary<string, string> namePathDict = new Dictionary<string, string>();
            OnModuleFileSystemInfo(curDirectoryInfo, namePathDict);
        }

    }

    /// <summary>
    /// 遍历文件夹中的文件系统
    /// </summary>
    private static void OnModuleFileSystemInfo(DirectoryInfo directoryInfo, Dictionary<string, string> namePathDict)
    {
        FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
        foreach (var FSI in fileSystemInfos)
        {
            if (FSI.Attributes == FileAttributes.Directory) continue;
            SetLabels(FSI, namePathDict);
        }
    }

    /// <summary>
    /// 修改资源文件的assetbundle Labels
    /// </summary>
    private static void SetLabels(FileSystemInfo fileInfo, Dictionary<string, string> namePathDict)
    {
        if (fileInfo.Extension == ".meta" || fileInfo.Extension == ".cs")
            return;

        string bundleName = GetBundleName(fileInfo);
        string assetPath = fileInfo.FullName.Substring(fileInfo.FullName.IndexOf(@"\Assets\") + 1);
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
        assetImporter.assetBundleName = $"{bundleName}_{fileInfo.Extension.Substring(1)}_{_lowerModuleName}";

        if (fileInfo.Extension == ".unity")
        {
            assetImporter.assetBundleVariant = _scenePostFix;
        }
        else
        {
            assetImporter.assetBundleVariant = _abPostFix;
        }

        //添加到字典 
        string floderName = "";
        if (bundleName.Contains("/"))
        {
            floderName = bundleName.Split('/')[1];
        }
        else
        {
            floderName = bundleName;
        }

        string bundlePath = assetImporter.assetBundleName + "." + assetImporter.assetBundleVariant;
        if (!namePathDict.ContainsKey(floderName))
        {
            namePathDict.Add(floderName, bundlePath);
        }
    }

    /// <summary>
    /// 清除之前设置过的AssetBundleName，避免不必要的资源也打包
    /// </summary>
    private static void ClearAssetBundlesName()
    {
        foreach (var item in AssetDatabase.GetAllAssetBundleNames())
        {
            AssetDatabase.RemoveAssetBundleName(item, true);
        }
        Log("标记清除成功");
    }

    /// <summary>
    /// 获取包名 从ModulesResPath 之后开始的路径
    /// </summary>
    private static string GetBundleName(FileSystemInfo fileInfo)
    {
        string unityPath = FileHelper.NormalizePath(fileInfo.FullName);
        string bundlePath = unityPath.Remove(unityPath.LastIndexOf(".")).Substring(_moduleResPath.Length + 1).ToLower();
        return bundlePath;
    }

    #endregion

    #region 打包

    /// <summary>
    /// 打包
    /// </summary>
    /// <param name="type"></param>
    /// <param name="buildAssetBundleOptions"></param>
    /// <param name="buildOptions"></param>
    /// <param name="release"></param>
    public static void Build(PlatformType type, BuildAssetBundleOptions buildAssetBundleOptions, BuildOptions buildOptions, bool release)
    {
        ClearFolder();
        Log("资源打包-开始");
        BuildPipeline.BuildAssetBundles(_outputSAPath, buildAssetBundleOptions, _buildTarget);
        DirectoryInfo dinfo = new DirectoryInfo(_outputSAPath);
        GenerateVersionInfo(new DirectoryInfo(_outputSAPath).FullName);
        Log("资源打包-完成");

        if (release)
        {
            FileHelper.CopyDirectory(_outputSAPath, _packageAB);
            Log("资源复制到开发目录-完成");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
        Log("资源打包-结束");
    }

    #endregion

    private static void ClearFolder()
    {
        if (_release)
        {
            DeleteFolder(_packageAB);
        }
        DeleteFolder(_outputSAPath);
        AssetDatabase.Refresh();
    }

    private static void DeleteFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        else
        {
            FileHelper.CleanDirectory(path);
        }
        Log("PackagedAB已清理");
    }
}
