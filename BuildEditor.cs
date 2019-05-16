using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace BuildEditor
{
    public enum PlatformType
    {
        None,
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

    public class BuildEditor : EditorWindow
    {
        //资源文件夹名
        private static string _aseetFileName = "MyRes";
        //资源路径
        private static string _assetDirectory;
        //是否使用默认路径
        private bool _useDefaultPath = true;
        //是否将资源打进exe
        private bool _isContainAB;

        private PlatformType _platformType;
        private BuildType _buildType;
        private BuildOptions _buildOptions = BuildOptions.AllowDebugging | BuildOptions.Development;
        private BuildAssetBundleOptions _buildAssetBundleOptions = BuildAssetBundleOptions.None;

        #region 工具

        //[MenuItem("Tools/编译生成热更dll")]
        public static void BuildHotfix()
        {
            //TODO 待完成：一键编译生成所有模块dll
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            string unityDir = System.Environment.GetEnvironmentVariable("Unity");
            if (string.IsNullOrEmpty(unityDir))
            {
                Debug.LogError("没有设置Unity环境变量!");
                return;
            }
            process.StartInfo.FileName = $@"{unityDir}\Editor\Data\MonoBleedingEdge\bin\mono.exe";
            process.StartInfo.Arguments = $@"{unityDir}\Editor\Data\MonoBleedingEdge\lib\mono\xbuild\14.0\bin\xbuild.exe .\Hotfix\Unity.Hotfix.csproj";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = @".\";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            string info = process.StandardOutput.ReadToEnd();
            process.Close();
            Debug.Log(info);
        }
        [MenuItem("Tools/打包工具")]
        public static void ShowWindow()
        {
            //BuildFolder = PathUtil.GetAssetBundleOutPath();
            //AssetDirectory = UnityEngine.Application.dataPath + "/" + AseetFileName;
            GetWindowWithRect(typeof(BuildEditor), new Rect(0, 0, 600, 400), false, "打包工具");
            //GetWindow(typeof(BuildEditor), false, "打包工具", true);
        }

        private void OnGUI()
        {
            _platformType = (PlatformType)EditorGUILayout.EnumPopup("选择平台：", _platformType, GUILayout.Width(595));
#if UNITY_EDITOR && UNITY_ANDROID
            _platformType = PlatformType.Android;
#elif UNITY_EDITOR && UNITY_IOS
            _platformType = PlatformType.IOS;
#endif
            EditorGUILayout.Space();
            if (_useDefaultPath)
            {
                ResetPathToDefault();
            }
            var newPath = EditorGUILayout.TextField("资源路径：", _assetDirectory);
            if (!System.String.IsNullOrEmpty(newPath) && newPath != _assetDirectory)
            {
                _useDefaultPath = false;
                _assetDirectory = newPath;
                //EditorUserBuildSettings.SetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath", m_OutputPath);
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("选择文件夹", GUILayout.Width(100), GUILayout.Height(20)))
            {
                _useDefaultPath = false;
                BrowseForFolder();
            }
            if (GUILayout.Button("默认文件夹", GUILayout.Width(100), GUILayout.Height(20)))
            {
                ResetPathToDefault();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("标记：");
            if (GUILayout.Button("清除全部标记", GUILayout.Width(190), GUILayout.Height(20)))
            {
                ClearAssetBundlesName();
            }
            if (GUILayout.Button("标记资源", GUILayout.Width(190), GUILayout.Height(20)))
            {
                SetAssetBundleLabels();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            _isContainAB = EditorGUILayout.Toggle("是否将资源打进exe: ", _isContainAB);
            //EditorGUILayout.Space();
            //_resVersion = EditorGUILayout.TextField("资源包版本", _resVersion);
            EditorGUILayout.Space();
            _buildType = (BuildType)EditorGUILayout.EnumPopup("BuildType: ", _buildType, GUILayout.Width(595));
            switch (_buildType)
            {
                case BuildType.Development:
                    _buildOptions = BuildOptions.Development | BuildOptions.AutoRunPlayer | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;
                    break;
                case BuildType.Release:
                    _buildOptions = BuildOptions.None;
                    break;
            }
            EditorGUILayout.Space();
            _buildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField("BuildOptions(可多选): ", _buildAssetBundleOptions, GUILayout.Width(595));
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("删除工程外AB包", GUILayout.Width(295), GUILayout.Height(30)))
            {
                DeleteAssetBundle();
            }
            if (GUILayout.Button("删除工程内AB包", GUILayout.Width(295), GUILayout.Height(30)))
            {
                DeleteAssetBundleInPorjecgt();
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("开始打包", GUILayout.Width(595), GUILayout.Height(40)))
            {
                if (_platformType == PlatformType.None)
                {
                    Debug.LogError("请选择打包平台!");
                    return;
                }
                Build(_platformType, _buildAssetBundleOptions, _buildOptions, _isContainAB);
            }
        }

        #endregion

        #region 文件
        /// <summary>
        /// 选择文件夹路径
        /// </summary>
        private void BrowseForFolder()
        {
            var newPath = EditorUtility.OpenFolderPanel("资源文件夹", _assetDirectory, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
                //AseetFileName = newPath.Substring(newPath.LastIndexOf("/") + 1);
                _assetDirectory = gamePath + "/" + newPath;
                //Debug.Log(AssetDirectory + "----------" + AseetFileName);
            }
        }

        /// <summary>
        /// 恢复至默认路径
        /// </summary>
        private void ResetPathToDefault()
        {
            _useDefaultPath = true;
            _assetDirectory = Application.dataPath + "/" + _aseetFileName;
        }

        /// <summary>
        /// 生成资源清单
        /// </summary>
        /// <param name="dir"></param>
        private static void GenerateVersionInfo(string moduleName, string dir)
        {
            PackageConfig versionProto = new PackageConfig();
            //Debug.Log(dir);
            GenerateVersionProto(dir, versionProto, "");

            using (FileStream fileStream = new FileStream($"{dir}/{moduleName}.txt", FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fileStream);
                //sw.Write(versionProto.ResVersion.ToString() + "\n");
                foreach (var item in versionProto.FileInfoDict)
                {
                    //byte[] bytes = string.Format("{0},{1},{2}", item.Value.File, item.Value.MD5, item.Value.Size).ToByteArray();
                    sw.Write($"{item.Key},{item.Value.MD5},{item.Value.Size}\n");
                }
                sw.Flush();
                //关闭写数据流 
                sw.Close();
                //byte[] bytes = JsonHelper.ToJson(versionProto).ToByteArray();
                //fileStream.Write(bytes, 0, bytes.Length);
            }

            //using (FileStream fileStream = new FileStream($"{dir}/Version.txt", FileMode.Create))
            //{
            //    StreamWriter sw = new StreamWriter(fileStream);
            //    sw.Write(versionProto.AppVersion.ToString() + "\n");
            //    sw.Write(versionProto.ResVersion.ToString() + "\n");
            //    sw.Write(versionProto.TotalSize.ToString() + "\n");
            //    sw.Write(versionProto.Appdownload_url.ToString() + "\n");
            //    sw.Write(versionProto.Serverconfig_url.ToString() + "\n");
            //    sw.Write("true");
            //    sw.Flush();
            //    //关闭写数据流 
            //    sw.Close();
            //    //byte[] bytes = JsonHelper.ToJson(versionProto).ToByteArray();
            //    //fileStream.Write(bytes, 0, bytes.Length);
            //}
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
                if (fi.Extension == ".assetbundle" || fi.Extension == ".u3d" || fi.Extension == ".manifest")
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
            // oriDirectory = AssetDirectory;
            DirectoryInfo directoryInfo = new DirectoryInfo(_assetDirectory);
            DirectoryInfo[] parentDirectories = directoryInfo.GetDirectories();
            TraverseDirectory(parentDirectories, _assetDirectory);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            Debug.Log("资源标记成功");
        }

        /// <summary>
        /// 遍历资源文件
        /// </summary>
        /// <param name="parentDirectories"></param>
        /// <param name="oriDirectory"></param>
        private static void TraverseDirectory(DirectoryInfo[] parentDirectories, string oriDirectory)
        {
            foreach (DirectoryInfo tempDirectoryInfo in parentDirectories)
            {
                string curDirectory = oriDirectory + @"\" + tempDirectoryInfo.Name;
                //Debug.Log(curDirectory);
                DirectoryInfo curDirectoryInfo = new DirectoryInfo(curDirectory);
                if (curDirectoryInfo == null)
                {
                    Debug.LogError(curDirectoryInfo + "不存在");
                    return;
                }
                else
                {
                    DirectoryInfo[] subdirectory = curDirectoryInfo.GetDirectories();
                    if (subdirectory.Length > 0)
                    {
                        string sDirectory = curDirectoryInfo.FullName;
                        //Debug.Log(sDirectory);
                        TraverseDirectory(subdirectory, sDirectory);
                    }

                    if (!curDirectory.Contains("/"))
                    {
                        Dictionary<string, string> namePathDict = new Dictionary<string, string>();
                        //Debug.Log(fileName);
                        OnModuleFileSystemInfo(curDirectoryInfo, namePathDict);
                        //OnWriteConfig(fileName, namePathDict);
                    }
                }
            }
        }

        /// <summary>
        /// 遍历文件夹中的文件系统
        /// </summary>
        private static void OnModuleFileSystemInfo(FileSystemInfo fileSystemInfo, Dictionary<string, string> namePathDict)
        {
            if (!fileSystemInfo.Exists)
            {
                Debug.LogError(fileSystemInfo.FullName + "不存在");
                return;
            }

            DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
            FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
            foreach (var tempFileSystemInfos in fileSystemInfos)
            {
                FileInfo fileInfo = tempFileSystemInfos as FileInfo;
                if (fileInfo != null)
                {
                    //强转成功，目标为文件，修改其Label
                    SetLabels(fileInfo, namePathDict);
                }
            }
        }

        ///// <summary>
        ///// 记录配置文件
        ///// </summary>
        ///// <param name="sceneDirectory"></param>
        ///// <param name="namePathDict"></param>
        //private static void OnWriteConfig(string moduleName, Dictionary<string, string> namePathDict)
        //{
        //    //string outPath = "Assets/AssetBundles";
        //    //if (!Directory.Exists(outPath))
        //    //{
        //    //    Directory.CreateDirectory(outPath);
        //    //}
        //    string path = PathUtil.GetAssetBundleOutPath() + "/" + moduleName + "Record.txt";
        //    //Debug.Log(path);
        //    using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
        //    {
        //        //写二进制
        //        using (StreamWriter sw = new StreamWriter(fs))
        //        {
        //            sw.WriteLine(namePathDict.Count);
        //            foreach (KeyValuePair<string, string> kv in namePathDict)
        //            {
        //                sw.WriteLine(kv.Key + " " + kv.Value);
        //            } 
        //        }
        //    }
        //}

        /// <summary>
        /// 修改资源文件的assetbundle Labels
        /// </summary>
        private static void SetLabels(FileInfo fileInfo, Dictionary<string, string> namePathDict)
        {
            if (fileInfo.Extension == ".meta" || fileInfo.Extension == ".cs")
                return;

            //Debug.Log("11111111");
            string fileName = fileInfo.Name.Remove(fileInfo.Name.LastIndexOf("."));
            string bundleName = GetBundleName(fileInfo, fileName);

            string assetPath = fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets"));
            //Debug.Log(assetPath);
            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);

            assetImporter.assetBundleName = bundleName.ToLower();

            if (fileInfo.Extension == ".unity")
            {
                assetImporter.assetBundleVariant = "u3d";
            }
            else
            {
                assetImporter.assetBundleVariant = "assetbundle";
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
            int length = AssetDatabase.GetAllAssetBundleNames().Length;
            string[] oldAssetBundleNames = new string[length];
            for (int i = 0; i < length; i++)
            {
                oldAssetBundleNames[i] = AssetDatabase.GetAllAssetBundleNames()[i];
            }

            for (int j = 0; j < oldAssetBundleNames.Length; j++)
            {
                AssetDatabase.RemoveAssetBundleName(oldAssetBundleNames[j], true);
            }
            Debug.Log("标记清除成功");
        }

        /// <summary>
        /// 获取包名
        /// </summary>
        private static string GetBundleName(FileInfo fileInfo, string fileName)
        {
            string windowsPath = fileInfo.FullName;
            string unityPath = windowsPath.Replace(@"\", "/");
            string bundlePath = unityPath.Remove(unityPath.LastIndexOf(".")).Substring(unityPath.IndexOf(_aseetFileName) + _aseetFileName.Length + 1);
            return bundlePath;
            //int index = unityPath.IndexOf(fileName) + fileName.Length;
            //string bundlePath = unityPath.Substring(index + 1);
            //if (bundlePath.Contains("/"))
            //{
            //    string[] temp = bundlePath.Split('/');
            //    Debug.Log(fileName + "/" + temp[0]);
            //    return fileName + "/" + temp[0];
            //}
            //else
            //{
            //    Debug.Log(fileName);
            //    return fileName;

            //}
        }

        #endregion

        #region 打包

        //[MenuItem("AssetBundle/Build AssetBundles（打包）")]
        static void BuildAllAssetBundles()
        {
            // string outPath = PathUtil.GetAssetBundleOutPath();

            //BuildPipeline.BuildAssetBundles(outPath, 0, BuildTarget.StandaloneWindows64);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log("打包成功");
        }



        /// <summary>
        /// 打包
        /// </summary>
        /// <param name="type"></param>
        /// <param name="buildAssetBundleOptions"></param>
        /// <param name="buildOptions"></param>
        /// <param name="isContainAB"></param>
        public static void Build(PlatformType type, BuildAssetBundleOptions buildAssetBundleOptions, BuildOptions buildOptions, bool isContainAB)
        {
            BuildTarget buildTarget = BuildTarget.StandaloneWindows64;

            switch (type)
            {
                case PlatformType.Windows:
                    buildTarget = BuildTarget.StandaloneWindows64;
                    break;
                case PlatformType.Android:
                    buildTarget = BuildTarget.Android;
                    break;
                case PlatformType.iOS:
                    buildTarget = BuildTarget.iOS;
                    break;
            }
            string fold = string.Format(PathUtil.BuildPath + "{0}/", type);
            if (!Directory.Exists(fold))
            {
                Directory.CreateDirectory(fold);
            }
            Debug.Log("开始资源打包");
            BuildPipeline.BuildAssetBundles(fold, buildAssetBundleOptions, buildTarget);

            foreach (string modulePath in Directory.GetDirectories(fold))
            {
                string moduleName = modulePath.Substring(modulePath.LastIndexOf("/") + 1);
                //Debug.Log(moduleName);
                DirectoryInfo dinfo = new DirectoryInfo(modulePath);
                GenerateVersionInfo(moduleName, dinfo.FullName);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            if (isContainAB)
            {
                string localfold = string.Format("Assets/StreamingAssets/{0}/", type);
                if (!Directory.Exists(localfold))
                {
                    Directory.CreateDirectory(localfold);
                }
                FileHelper.CleanDirectory(localfold);
                FileHelper.CopyDirectory(fold, localfold);
                Debug.Log("完成文件复制");
            }
            RestValue();
            Debug.Log("完成资源打包");
        }

        #endregion

        #region 删除包

        static void DeleteAssetBundle()
        {
            if (Directory.Exists(PathUtil.BuildPath))
            {
                Directory.Delete(PathUtil.BuildPath, true);
                //File.Delete(PathUtil.BuildPath + ".meta");
                AssetDatabase.Refresh();
            }
            Debug.Log("删除成功");
        }

        static void DeleteAssetBundleInPorjecgt()
        {
            string tarPlatform;
#if UNITY_EDITOR && UNITY_STANDALONE_WIN
            tarPlatform = "Windows";
#elif UNITY_EDITOR && UNITY_ANDROID
            tarPlatform = "Android";
#elif UNITY_EDITOR && UNITY_IOS
            tarPlatform = "iOS";
#elif UNITY_EDITOR && UNITY_STANDALONE_LINUX
            tarPlatform = "iOS";
#endif
            string path = Application.streamingAssetsPath + "/" + tarPlatform;
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
                //File.Delete(PathUtil.BuildPath + ".meta");
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
            else
            {
                Directory.CreateDirectory(path);
            }
            Debug.Log("删除成功");
        }

        #endregion

    }

}