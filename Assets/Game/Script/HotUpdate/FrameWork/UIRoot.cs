using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace Game.Script.HotUpdate.FrameWork
{
    public class UIRoot : MonoSingleton<UIRoot>
    {
        public enum UILayer
        {
            LOW,
            MID,
            TOP
        }

        [SerializeField] private Transform LOW;
        [SerializeField] private Transform MID;
        [SerializeField] private Transform TOP;
    
        private Dictionary<string, GameObject> DialogDic = new Dictionary<string, GameObject>();

        public async UniTask OpenDialog(string fileName, UILayer layer)
        {
            if (DialogDic.TryGetValue(fileName, out var dialog))
            {
                dialog.gameObject.SetActive(true);
            }
            else
            {
                AssetOperationHandle handle = YooAssets.LoadAssetAsync<GameObject>(fileName);
                await handle.Task;
                dialog = handle.InstantiateSync();
                DialogDic[fileName] = dialog;
            }

            switch (layer)
            {
                case UILayer.LOW:
                    dialog.transform.SetParent(LOW);
                    break;
                case UILayer.MID:
                    dialog.transform.SetParent(MID);
                    break;
                case UILayer.TOP:
                    dialog.transform.SetParent(TOP);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layer), layer, null);
            }

            dialog.transform.localScale = Vector3.one;
            dialog.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            dialog.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            Debug.Log($"Prefab name is {dialog.name} ");
        }

        void Start()
        {
            DontDestroyOnLoad(this);
        }
    }
}