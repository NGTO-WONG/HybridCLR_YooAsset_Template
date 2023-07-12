using Cysharp.Threading.Tasks;
using Game.Script.HotUpdate.FrameWork;
using UnityEngine;

namespace Game.Script.HotUpdate.Scene
{
    public class TitleScene : MonoBehaviour
    {
        void Start()
        {
            UIRoot.Instance.OpenDialog("HomeScene_Popup_Login", UIRoot.UILayer.LOW).Forget();
        }
    }
}