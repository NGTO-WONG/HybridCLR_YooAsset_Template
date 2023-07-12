using System.Collections.Generic;
using UnityEngine;

namespace Game.Script.AOT.YooAsset
{
    /// <summary>
    /// 内置资源清单
    /// </summary>
    public class BuildinFileManifest : ScriptableObject
    {
        public List<string> BuildinFiles = new List<string>();
    }
}