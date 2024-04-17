using UnityEngine;

public class RXDestroyOnAwake : MonoBehaviour
{
	public void Awake()
	{
		Object.Destroy(base.gameObject);
	}
}
