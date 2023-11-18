using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using YooAsset.Editor;

namespace Script_AOT.Editor
{
    public static class BuildTool
    {

        // [MenuItem("HybridCLR/Build/BuildIOS", priority = 310)]
        // public static void BuildIOS()
        // {
        //     EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        //     Build(BuildTarget.iOS);
        // }
        //
        // [MenuItem("HybridCLR/Build/BuildAndroid", priority = 311)]
        // public static void BuildAndroid()
        // {
        //     EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        //     Build(BuildTarget.Android);
        // }
        //
        // [MenuItem("HybridCLR/Build/BuildPC", priority = 312)]
        // public static void BuildPC()
        // {
        //     EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,
        //         BuildTarget.StandaloneWindows64);
        //     Build(BuildTarget.StandaloneWindows64);
        // }

        private static void Build(BuildTarget buildTarget)
        {
            //2 华佗生成+改名+拷贝dll
            Debug.Log("2 华佗生成dll + 2 改名+拷贝dll");
            BuildAndCopyAndRenameDll();
            //3 yooAsset打包
            Debug.Log("3 yooAsset打包");
            var outputPackageDirectory = YooAssetBuild_ForceRebuild();
            //4 上传到cdn 这里可以替换成你自己的cdn逻辑
            //Debug.Log("4 上传到cdn");
            //UpdateBundleToCDN_UOS();

            // 获取Assets文件夹的父目录，即项目的根目录
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;

            // 设置保存路径为项目根目录的Builds子目录下
            string buildDirectory = Path.Combine(projectRoot, "Builds/Windows");
            string buildPath = Path.Combine(buildDirectory, "MyGame.exe");

            // 确保输出目录存在
            if (!Directory.Exists(buildDirectory))
            {
                Directory.CreateDirectory(buildDirectory);
            }

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = buildTarget,
                options = BuildOptions.None
            };

            // 执行打包操作
            BuildPipeline.BuildPlayer(buildPlayerOptions);

            Debug.Log("Build finished!");
        }

        [MenuItem("HybridCLR/Build/1.GenerateAll+BuildActiveDll+CopyDll", priority = 301)]
        public static void BuildAndCopyAndRenameDll()
        {
            PrebuildCommand.GenerateAll();
            //生成linkFile
            var xmlPath = Application.dataPath + "/HybridCLRGenerate/link.xml";
            BuildLinkFilev2.GenerateLinkfile(xmlPath);
            //热更新dll
            CompileDllCommand.CompileDllActiveBuildTarget();
            var target = EditorUserBuildSettings.activeBuildTarget;
            string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            {
                string sourcePath = $"{hotfixDllSrcDir}/{dll}";
                string dstPath = $"{GlobalConfig.HotfixAssembliesDstDir}/{dll}.bytes";
                File.Copy(sourcePath, dstPath, true);
                Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {sourcePath} -> {dstPath}");
            }

            //补充AOT范型dll
            string aotDllSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            foreach (var dll in AOTGenericReferences.PatchedAOTAssemblyList)
            {
                string sourcePath = $"{aotDllSrcDir}/{dll}";
                string dstPath = $"{GlobalConfig.HotfixAssembliesDstDir}/{dll}.bytes";
                File.Copy(sourcePath, dstPath, true);
                Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {sourcePath} -> {dstPath}");
            }
            
        }

        /// <summary>
        /// build资源
        /// </summary>
        /// <returns></returns>
        [MenuItem("HybridCLR/Build/YooAsset打全量包", priority = 302)]
        public static string YooAssetBuild_ForceRebuild()
        {
            if (!Directory.Exists(GlobalConfig.BundlePath))
            {
                Directory.CreateDirectory(GlobalConfig.BundlePath);
            }

            Directory.Delete(GlobalConfig.BundlePath, true);
            return YooAssetBuild(EBuildMode.ForceRebuild, GlobalConfig.BuildVersion);
        }

        // /// <summary>
        // /// 上传到uos的cdn 必须先手动创建buckets 才能load
        // /// </summary>
        // /// <returns></returns>
        // [MenuItem("HybridCLR/Build/4.UpdateBundleToCDN_UOS", priority = 104)]
        // public static void UpdateBundleToCDN_UOS()
        // {
        //     BucketController.LoadBuckets();
        //     EntryController.LoadEntries(0);
        //     var pb = EntryController.pb;
        //     pb.selectedBucketUuid = pb.bucketList[0].id;
        //     EntryController.SyncEntries(BundlePath);
        // }

        /// <summary>
        /// build资源
        /// </summary>
        /// <returns></returns>
        [MenuItem("HybridCLR/Build/1.编译当前平台的热更新程序集dll+yooAsset打增量包",
            priority = 403)]
        public static void YooAssetBuild_IncrementalBuild()
        {
            //生成热更新dll
            CompileDllCommand.CompileDllActiveBuildTarget();

            //拷贝dll
            var target = EditorUserBuildSettings.activeBuildTarget;
            string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            foreach (var hotUpdateDll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            {
                string sourcePath = $"{hotfixDllSrcDir}/{hotUpdateDll}";
                string dstPath = $"{GlobalConfig.HotfixAssembliesDstDir}/{hotUpdateDll}.bytes";
                File.Copy(sourcePath, dstPath, true);
                Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {sourcePath} -> {dstPath}");
            }

            //yooAsset打包
            var hotUpdateVersion = GlobalConfig.HotUpdateVersion;
            var outputPackageDirectory = YooAssetBuild(EBuildMode.IncrementalBuild, hotUpdateVersion);
            if (outputPackageDirectory != "")
            {
                var files = Directory.GetFiles(outputPackageDirectory);
                var targetDirectory = Directory.GetParent(outputPackageDirectory) + "/" + GlobalConfig.BuildVersion;
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    File.Copy(file, targetDirectory + "/" + fileName, true);
                }

                Debug.Log(files + " to " + targetDirectory);
            }

            //UpdateBundleToCDN_UOS();
        }

        private static string YooAssetBuild(EBuildMode eBuildMode, string packageVersion)
        {
            // 构建参数
            BuildParameters buildParameters = new BuildParameters
            {
                SBPParameters = null,
                StreamingAssetsRoot = Application.streamingAssetsPath,
                BuildOutputRoot = GlobalConfig.BundlePath,
                BuildTarget = EditorUserBuildSettings.activeBuildTarget,
                BuildPipeline = EBuildPipeline.BuiltinBuildPipeline,
                BuildMode = eBuildMode,
                PackageName = "DefaultPackage",
                PackageVersion = packageVersion,
                EnableLog = true,
                VerifyBuildingResult = true,
                SharedPackRule = new ZeroRedundancySharedPackRule(),
                EncryptionServices = null,
                OutputNameStyle = EOutputNameStyle.BundleName_HashName,
                CopyBuildinFileOption = ECopyBuildinFileOption.None,
                CopyBuildinFileTags = null,
                CompressOption = ECompressOption.LZ4,
                DisableWriteTypeTree = false,
                IgnoreTypeTreeChanges = false
            };

            // 执行构建
            AssetBundleBuilder builder = new AssetBundleBuilder();
            var buildResult = builder.Run(buildParameters);
            if (buildResult.Success)
            {
                Debug.Log($"构建成功 : {buildResult.OutputPackageDirectory}");
                return buildResult.OutputPackageDirectory;
            }
            else
            {
                Debug.LogError($"构建失败 : {buildResult.ErrorInfo}");
                return "";
            }
        }
    }


    public static class BuildLinkFilev2
    {
        private static string _il2cppManagedPath = string.Empty;

        private static string il2cppManagedPath
        {
            get
            {
                if (string.IsNullOrEmpty(_il2cppManagedPath))
                {
                    var contentsPath = EditorApplication.applicationContentsPath;
                    var extendPath = "";

                    var buildTarget = EditorUserBuildSettings.activeBuildTarget;
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX
                    switch (buildTarget)
                    {
                        case BuildTarget.StandaloneWindows64:
                        case BuildTarget.StandaloneWindows:
                            extendPath = "PlaybackEngines/windowsstandalonesupport/Variations/il2cpp/Managed/";
                            break;
                        case BuildTarget.iOS:
                            extendPath = "PlaybackEngines/iOSSupport/Variations/il2cpp/Managed/";
                            break;
                        case BuildTarget.Android:
                            extendPath = "PlaybackEngines/AndroidPlayer/Variations/il2cpp/Managed/";
                            break;
                        case BuildTarget.WebGL:
                            extendPath = "PlaybackEngines/WebGLSupport/Variations/nondevelopment/Data/Managed/";
                            break;
                        default:
                            throw new Exception($"[BuildPipeline::GenerateLinkfile] 请选择合适的平台, 目前是:{buildTarget}");
                    }
#elif UNITY_EDITOR_OSX
                switch (buildTarget)
                {
                    case BuildTarget.StandaloneOSX:
                        extendPath = "PlaybackEngines/MacStandaloneSupport/Variations/il2cpp/Managed/";
                        break;
                    case BuildTarget.iOS:
                        extendPath = "../../PlaybackEngines/iOSSupport/Variations/il2cpp/Managed/";
                        break;
                    case BuildTarget.Android:
                        extendPath = "../../PlaybackEngines/AndroidPlayer/Variations/il2cpp/Managed/";
                        break;                        
                    case BuildTarget.WebGL:
                        extendPath = "PlaybackEngines/WebGLSupport/Variations/nondevelopment/Data/Managed/";
                        break;
                    default:
                        throw new Exception($"[BuildPipeline::GenerateLinkfile] 请选择合适的平台, 目前是:{buildTarget}");
                }
#endif
                    if (string.IsNullOrEmpty(extendPath))
                    {
                        throw new Exception($"[BuildPipeline::GenerateLinkfile] 请选择合适的平台, 目前是:{buildTarget}");
                    }

                    _il2cppManagedPath = Path.Combine(contentsPath, extendPath).Replace('\\', '/');
                }

                return _il2cppManagedPath;
            }
        }

        private static List<string> IgnoreClass = new()
        {
            "editor", "netstandard", "Bee.", "dnlib", ".framework", "Test", "plastic", "Gradle", "log4net", "Analytics", "System.Drawing",
            "NVIDIA", "VisualScripting", "UIElements", "IMGUIModule", ".Cecil", "GIModule", "GridModule", "HotReloadModule", "StreamingModule",
            "TLSModule", "XRModule", "WindModule", "VRModule", "VirtualTexturingModule", "compiler", "BuildProgram", "NiceIO", "ClothModule",
            "VFXModule", "ExCSS", "GeneratedCode", "mscorlib", "System", "SyncToolsDef", "ReportGeneratorMerged"
        };
        private static bool IsIngoreClass(string classFullName)
        {
            var tmpName = classFullName.ToLower();
            foreach (var ic in IgnoreClass)
            {
                if (tmpName.Contains(ic.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<string> IgnoreType = new()
        {
            "jetbrain", "editor", "PrivateImplementationDetails", "experimental", "microsoft.", "compiler"
        };
        private static bool IsIgnoreType(string typeFullName)
        {
            var tmpName = typeFullName.ToLower();
            foreach (var ic in IgnoreType)
            {
                if (tmpName.Contains(ic.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        public static void GenerateLinkfile(string outPath)
        {
            var basePath = il2cppManagedPath;
            if (!Directory.Exists(basePath))
            {
                Debug.LogWarning($"[BuildPipeline::GenerateLinkfile] can't find il2cpp's dlls [{basePath}]");
                basePath = basePath.Replace("/il2cpp/", "/mono/");
            }

            if (!Directory.Exists(basePath))
            {
                Debug.LogWarning($"[BuildPipeline::GenerateLinkfile] can't find il2cpp's dlls [{basePath}]");
                return;
            }

            Dictionary<string, Assembly> AllAssemblies = new();

            var hashAss = new HashSet<string>();
            var files = new List<string>(Directory.GetFiles(basePath, "*.dll"));
            foreach (var file in files)
            {
                var ass = Assembly.LoadFile(file);
                if (ass != null)
                {
                    var name = ass.GetName().Name;
                    if (IsIngoreClass(name))
                    {
                        continue;
                    }

                    if (!hashAss.Contains(name))
                    {
                        hashAss.Add(name);

                        AllAssemblies[name] = ass;
                    }
                }
            }

            var names = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;
            var localAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var localPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..")).Replace('\\', '/');
            foreach (var ass in localAssemblies)
            {
                if (ass.IsDynamic)
                {
                    Debug.LogWarning($"[BuildPipeline::GenerateLinkfile] {ass.FullName} is dynamic!!!");
                    continue;
                }

                try
                {
                    var assPath = Path.GetFullPath(ass.Location).Replace('\\', '/');
                    if (assPath.Contains(localPath) && assPath.ToLower().Contains("/editor/"))
                    {
                        continue;
                    }

                    var name = ass.GetName().Name;
                    if (hashAss.Contains(name))
                    {
                        continue;
                    }

                    var ignore = false;
                    foreach (var n in names)
                    {
                        if (name.Contains(n))
                        {
                            ignore = true;
                            break;
                        }
                    }
                    if (ignore)
                    {
                        continue;
                    }

                    hashAss.Add(name);
                    AllAssemblies[name] = ass;
                }
                catch (Exception ex)
                {
                }
            }

            var fullPreserve = new List<string>();
            var otherAss = new List<string>();
            var otherAssemblies = new Dictionary<string, List<string>>();

            foreach (var ass in AllAssemblies)
            {
                if (IsIngoreClass(ass.Key))
                {
                    continue;
                }

                var allTypes = ass.Value.GetTypes();
                var stripTypes = new List<string>();
                foreach (var type in allTypes)
                {
                    if (IsIgnoreType(type.FullName))
                    {
                        continue;
                    }

                    stripTypes.Add(type.FullName);
                }

                if (stripTypes.Count == 0)
                {
                    continue;
                }
                else if (allTypes.Length < 5)
                {
                    fullPreserve.Add(ass.Key);
                }
                else if (allTypes.Length - stripTypes.Count > allTypes.Length * 0.1f)
                {
                    otherAssemblies.Add(ass.Key, stripTypes);
                    otherAss.Add(ass.Key);
                }
                else
                {
                    fullPreserve.Add(ass.Key);
                }
            }

            fullPreserve.Sort();
            otherAss.Sort();

            var fileName = outPath;
            var writer = System.Xml.XmlWriter.Create(fileName, new()
            {
                Encoding = new UTF8Encoding(false),
                Indent = true
            });

            writer.WriteStartDocument();
            writer.WriteStartElement("linker");

            foreach (var fp in fullPreserve)
            {
                writer.WriteStartElement("assembly");
                writer.WriteAttributeString("fullname", fp);
                writer.WriteAttributeString("preserve", "all");
                writer.WriteEndElement();
            }

            foreach (var fp in otherAss)
            {
                writer.WriteStartElement("assembly");
                writer.WriteAttributeString("fullname", fp);

                foreach (var type in otherAssemblies[fp])
                {
                    writer.WriteStartElement("type");
                    writer.WriteAttributeString("fullname", type);
                    writer.WriteAttributeString("preserve", "all");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();

            checkass(Path.Combine(Application.dataPath, "Plugins", "HybridCLR", "Generated", "ReservedAssembly.cs"));
        }

        public class AssemblyComp : IComparable
        {
            public int CompareTo(object obj)
            {
                throw new NotImplementedException();
            }
        }

        private class AsmDefHeader
        {
            public string name;
            public List<string> includePlatforms;
            public List<string> defineConstraints;
        }

        private static List<AsmDefHeader> GetAssemblyDefinitionAsset()
        {
            var ret = new List<AsmDefHeader>();
            string[] folderContents = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset");

            foreach (var asset in folderContents)
            {
                var tmp = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(AssetDatabase.GUIDToAssetPath(asset));
                if (tmp != null)
                {
                    var jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<AsmDefHeader>(tmp.text);
                    if (jsonObj != null && jsonObj.includePlatforms != null)
                    {
                        if (jsonObj.includePlatforms.Count == 0)
                        {
                            if (jsonObj.defineConstraints == null || jsonObj.defineConstraints.Count == 0)
                            {
                                ret.Add(jsonObj);
                            }
                        }
                        else
                        {
                            var hasEditor = false;
                            foreach (var p in jsonObj.includePlatforms)
                            {
                                if (p.ToLower() == "editor")
                                {
                                    hasEditor = true;
                                    break;
                                }
                            }

                            if (hasEditor)
                            {
                                continue;
                            }

                            if (jsonObj.defineConstraints == null || jsonObj.defineConstraints.Count == 0)
                            {
                                ret.Add(jsonObj);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        private static void checkass(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var codeTemplate = @"using System.Text;
using UnityEngine;

namespace GameMain.Scripts.HybridCLR
{
    public class ReservedAssembly : MonoBehaviour
	{
	    private void Awake()
		{
            var sb = new StringBuilder();

			void Reserved<T>()
			{
				sb.AppendLine(typeof(T).ToString());
			}

//Replace This
            Debug.Log(sb.ToString());
		}
	}
}";
            var assemblyDefinitionAssets = GetAssemblyDefinitionAsset();

            var watchAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var successAssemblies = new Dictionary<string, Assembly>();
            foreach (var wa in watchAssemblies)
            {
				if (wa.IsDynamic)
                {
                    continue;
                }
				
                var locName = Path.GetFileName(wa.Location).ToLower();
                if (successAssemblies.ContainsKey(locName))
                {
                    continue;
                }

                foreach (var ada in assemblyDefinitionAssets)
                {
                    if (locName == ada.name.ToLower() + ".dll")
                    {
                        successAssemblies.Add(locName, wa);
                        break;
                    }
                }
            }

            var assCsharp = watchAssemblies.First(a => a.GetName().Name == "HotUpdate");
            if (assCsharp != null)
            {
                successAssemblies.Add("HotUpdate.dll", assCsharp);
            }

            var distAssembly = new Dictionary<string, Assembly>();            
            foreach (var sa in successAssemblies)
            {
                var ass = sa.Value;
                if (ass.IsDynamic)
                {
                    continue;
                }

                if (ass.FullName.Contains("Newtonsoft"))
                {
                    continue;
                }

                if (ass.FullName.Contains("Editor"))
                {
                    continue;
                }

                var hasEditor = false;
                var ras = ass.GetReferencedAssemblies();
                foreach (var r in ras)
                {
                //    if (r.Name.Contains("UnityEditor"))
                    {
                 //       hasEditor = true;
               //         break;
                    }
                }

                if (hasEditor)
                {
         //           continue;
                }

                if (!distAssembly.ContainsKey(ass.FullName))
                {
                    distAssembly.Add(ass.FullName, ass);
                }

                foreach (var r in ras)
                {
                    if (r.Name.Contains("UnityEditor"))
                    {
                        continue;
                    }

                    foreach (var wa in watchAssemblies)
                    {
                        if (wa.FullName == r.FullName && !distAssembly.ContainsKey(wa.FullName))
                        {
                            distAssembly.Add(wa.FullName, wa);
                        }
                    }
                }
            }

            var info = new Dictionary<string, string>();
            foreach (var _ass in distAssembly.Values)
            {
                var ass = _ass;
                if (ass.IsDynamic)
                {
                    continue;
                }

                if (ass.FullName.Contains("Assembly-CSharp") 
                    || ass.FullName.Contains("TestRunner")
                    || ass.FullName.Contains("HybridCLR")
                    || ass.FullName.Contains("nunit")
                    )
                {
                    continue;
                }               

                Debug.Log($"{ass.FullName}");

                var files = Directory.GetFiles(il2cppManagedPath, Path.GetFileName(ass.Location), SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    ass = Assembly.LoadFile(files[0]);
                }

                var classOk = false;
                var allTypes = ass.GetExportedTypes();
                foreach (var type in allTypes)
                {
                    if (!type.IsPublic || type.IsAbstract || type.IsGenericType)
                    {
                        continue;
                    }

                    var name = type.FullName;
                    if (name.Contains("System.") && ass.FullName.Contains("Newt"))
                    {
                        continue;
                    }

                    var atts = new List<CustomAttributeData>(type.CustomAttributes.Where(a => a.AttributeType.Name.Contains("Obsolete")));
                    if (atts.Count > 0)
                    {
                        continue;
                    }

                    if (classOk)
                    {
                        break;
                    }

                    var con = type.GetConstructors();
                    if (con != null && con.Length > 0)
                    {
                        foreach (var c in con)
                        {
                            if (c.IsConstructor && c.IsPublic)
                            {
                                Debug.Log("con true:" +type.FullName);
                                if (info.TryGetValue(type.FullName, out var val))
                                {
                                    Debug.LogError($"{type.FullName} {val} {ass.Location}");
                                }
                                else
                                {

                                    info.Add(type.FullName, ass.Location);
                                }
                                classOk = true;
                                break;
                            }
                            else
                            {
                                Debug.Log("con false:" +type.FullName);
                            }
                        }
                    }
                }
            }


            var sb = new StringBuilder();
            var keys = new HashSet<string>(info.Keys);
            foreach (var k in keys)
            {
                var x = info[k];
                if (k is "UnityEngine.InputManagerEntry")
                {
                    continue;
                }
                
                if (Path.GetFileName(x) is "HotUpdate.dll")
                {
                    continue;
                }
                sb.AppendLine($"\t\t\tReserved<{k}>(); // {Path.GetFileName(x)}");
            }

            var temp = "System.Text.RegularExpressions.Regex";
            sb.AppendLine($"\t\t\tReserved<{temp}>(); // {Path.GetFileName(temp)}");

            File.WriteAllText(fileName, codeTemplate.Replace("//Replace This", sb.ToString()));
        }
    }
}