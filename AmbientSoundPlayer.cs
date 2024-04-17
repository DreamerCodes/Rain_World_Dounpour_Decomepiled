using RWCustom;
using UnityEngine;

public class AmbientSoundPlayer
{
	public VirtualMicrophone mic;

	public AmbientSound aSound;

	public AudioClip clip;

	public GameObject gameObject;

	public AudioSource audioSource;

	public bool slatedForDeletion;

	private bool initiated;

	private float lastTime;

	private float lastDopplerDist;

	private float lastDopplerReturn;

	public bool waitingToPlay;

	public AmbientSoundPlayer(VirtualMicrophone mic, AmbientSound aSound)
	{
		this.mic = mic;
		this.aSound = aSound;
	}

	public void DrawUpdate(float timeStacker, float timeSpeed, Vector2 currentListenerPos)
	{
		if (!initiated)
		{
			TryInitiation();
		}
		else if (waitingToPlay)
		{
			if (audioSource.clip.loadState == AudioDataLoadState.Loaded)
			{
				audioSource.Play();
				waitingToPlay = false;
			}
		}
		else if (mic.camera.game.manager.rainWorld.OptionsReady)
		{
			audioSource.volume = Mathf.Clamp01(Mathf.Pow(Vol(timeStacker, currentListenerPos) * mic.volumeGroups[0] * mic.volumeGroups[2] * aSound.volume * mic.camera.game.manager.rainWorld.options.soundEffectsVolume, mic.soundLoader.volumeExponent));
			audioSource.pitch = Pitch(timeStacker, currentListenerPos) * timeSpeed * aSound.pitch;
			audioSource.panStereo = Pan(timeStacker, currentListenerPos);
			SetLowPassCutOff(mic.underWater);
		}
	}

	private float Vol(float timeStacker, Vector2 currentListenerPos)
	{
		if (aSound.type == AmbientSound.Type.Spot)
		{
			return Mathf.InverseLerp((aSound as SpotSound).TaperRad, (aSound as SpotSound).rad, Vector2.Distance((aSound as SpotSound).pos, currentListenerPos));
		}
		return 1f;
	}

	private float Pitch(float timeStacker, Vector2 currentListenerPos)
	{
		if (aSound.type == AmbientSound.Type.Omnidirectional)
		{
			return 1f;
		}
		if (aSound.type == AmbientSound.Type.Directional || aSound.type == AmbientSound.Type.Spot)
		{
			if ((aSound as DopplerAffectedSound).dopplerFac == 0f)
			{
				return 1f;
			}
			float num = Time.time - lastTime;
			if (num > 0.05f)
			{
				lastTime = Time.time;
				float num2 = ((!(aSound.type == AmbientSound.Type.Directional)) ? Vector2.Distance((aSound as SpotSound).pos, currentListenerPos) : Custom.RotateAroundOrigo(currentListenerPos, Custom.VecToDeg(new Vector2((aSound as DirectionalSound).direction.x, 0f - (aSound as DirectionalSound).direction.y))).y);
				float num3 = (lastDopplerDist - num2) / num;
				lastDopplerDist = num2;
				lastDopplerReturn = Mathf.Clamp(1f + num3 / 4000f * (aSound as DopplerAffectedSound).dopplerFac * (1f - mic.dopplerBlock), 0.1f, 2f);
			}
			return lastDopplerReturn;
		}
		return 1f;
	}

	private float Pan(float timeStacker, Vector2 currentListenerPos)
	{
		if (aSound.type == AmbientSound.Type.Omnidirectional)
		{
			return 0f;
		}
		if (aSound.type == AmbientSound.Type.Directional)
		{
			return (aSound as DirectionalSound).direction.x;
		}
		if (aSound.type == AmbientSound.Type.Spot)
		{
			return Custom.LerpMap(currentListenerPos.x, (aSound as SpotSound).pos.x - (aSound as SpotSound).TaperRad, (aSound as SpotSound).pos.x + (aSound as SpotSound).TaperRad, 1f, -1f);
		}
		return 0f;
	}

	public void SetLowPassCutOff(float effect)
	{
		if (effect == 0f && gameObject.GetComponent<AudioLowPassFilter>() != null)
		{
			Object.Destroy(gameObject.GetComponent<AudioLowPassFilter>());
		}
		else if (effect > 0f && gameObject.GetComponent<AudioLowPassFilter>() == null)
		{
			gameObject.AddComponent<AudioLowPassFilter>();
		}
		if (effect > 0f)
		{
			gameObject.GetComponent<AudioLowPassFilter>().cutoffFrequency = Mathf.Lerp(22000f, 1500f, Mathf.Pow(effect, 0.5f));
		}
	}

	private void TryInitiation()
	{
		clip = mic.soundLoader.RequestAmbientAudioClip(aSound.sample);
		if (clip != null)
		{
			if (clip.loadType == AudioClipLoadType.Streaming)
			{
				clip.LoadAudioData();
			}
			initiated = true;
			gameObject = new GameObject("amb " + aSound.sample);
			audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.clip = clip;
			audioSource.spatialBlend = 0f;
			audioSource.loop = true;
			audioSource.volume = 0f;
			audioSource.panStereo = 0f;
			if (audioSource.clip.loadState == AudioDataLoadState.Loaded)
			{
				audioSource.Play();
			}
			else
			{
				waitingToPlay = true;
			}
		}
	}

	public void Destroy()
	{
		if (gameObject != null && gameObject.GetComponent<AudioLowPassFilter>() != null)
		{
			Object.Destroy(gameObject.GetComponent<AudioLowPassFilter>());
		}
		if (audioSource != null)
		{
			Object.Destroy(audioSource);
		}
		Object.Destroy(gameObject);
		slatedForDeletion = true;
	}
}
