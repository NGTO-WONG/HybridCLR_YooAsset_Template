using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"UniTask.dll",
		"UnityEngine.CoreModule.dll",
		"YooAsset.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// System.Collections.Generic.Dictionary<object,object>
	// }}

	public void RefMethods()
	{
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,Game.Script.HotUpdate.FrameWork.UIRoot.<OpenDialog>d__5>(System.Runtime.CompilerServices.TaskAwaiter&,Game.Script.HotUpdate.FrameWork.UIRoot.<OpenDialog>d__5&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Game.Script.HotUpdate.FrameWork.UIRoot.<OpenDialog>d__5>(Game.Script.HotUpdate.FrameWork.UIRoot.<OpenDialog>d__5&)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.Object.FindObjectOfType<object>()
		// YooAsset.AssetOperationHandle YooAsset.YooAssets.LoadAssetAsync<object>(string)
	}
}