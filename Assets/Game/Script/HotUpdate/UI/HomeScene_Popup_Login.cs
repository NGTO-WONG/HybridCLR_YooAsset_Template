using Cysharp.Threading.Tasks;
using Game.Script.HotUpdate.FrameWork;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Script.HotUpdate.UI
{
    public class HomeScene_Popup_Login : MonoBehaviour
    {
        [SerializeField] private Button button;

        void Start()
        {
            button.onClick.AddListener(() =>
            {
                this.gameObject.SetActive(false);
                UIRoot.Instance.OpenDialog("Panel_Heroes", UIRoot.UILayer.LOW).Forget();
            });
        }
    }
}