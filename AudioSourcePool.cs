using UnityEngine;

public class AudioSourcePool : SimplePool<AudioSourcePoolItem>
{
	protected override AudioSourcePoolItem CreateInstance(int i)
	{
		GameObject gameObject = new GameObject($"AudioSourcePoolItem {i}");
		AudioSourcePoolItem audioSourcePoolItem = gameObject.AddComponent<AudioSourcePoolItem>();
		audioSourcePoolItem.audioSource = gameObject.AddComponent<AudioSource>();
		audioSourcePoolItem.alpf = gameObject.AddComponent<AudioLowPassFilter>();
		return audioSourcePoolItem;
	}

	protected override void BeforePop(AudioSourcePoolItem obj)
	{
		obj.audioSource.enabled = true;
		obj.gameObject.SetActive(value: true);
	}

	protected override void BeforePush(AudioSourcePoolItem obj)
	{
		obj.alpf.enabled = false;
		obj.audioSource.enabled = false;
		obj.audioSource.clip = null;
		obj.audioSource.playOnAwake = false;
		obj.gameObject.SetActive(value: false);
	}
}
