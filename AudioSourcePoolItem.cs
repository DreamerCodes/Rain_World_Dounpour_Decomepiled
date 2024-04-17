using UnityEngine;

public class AudioSourcePoolItem : MonoBehaviour
{
	public AudioSource audioSource;

	public AudioLowPassFilter alpf;

	public bool slatedForPlay;
}
