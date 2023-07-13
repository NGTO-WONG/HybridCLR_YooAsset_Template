using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// }}

	public void RefMethods()
	{
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Game.Script.HotUpdate.TestPanel.<<Start>b__1_0>d>(Game.Script.HotUpdate.TestPanel.<<Start>b__1_0>d&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Game.Script.HotUpdate.TitleScene.<Start>d__0>(Game.Script.HotUpdate.TitleScene.<Start>d__0&)
	}
}