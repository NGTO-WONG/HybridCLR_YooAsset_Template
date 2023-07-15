using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Script.HotUpdate
{
    public class TestPanel : MonoBehaviour
    {
        [SerializeField] private Button button;

        [SerializeField] private Text text;

        private int count = 1;
        
        private void Start()
        {
            button.onClick.AddListener(async ()=>
            {
                count *=2;
                text.text = count.ToString();
                //this.gameObject.SetActive(false);
            });
        }
    }
}