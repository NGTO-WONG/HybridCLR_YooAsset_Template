using System.Text;
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
            Debug.Log(sb.ToString());
		}
	}
}