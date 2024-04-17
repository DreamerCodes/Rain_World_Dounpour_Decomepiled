using UnityEngine;
using UnityEngine.UI;

public class ConsoleSpecificCanvasScale : MonoBehaviour
{
	private void Start()
	{
		base.gameObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1366f, 768f);
	}
}
