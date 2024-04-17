using UnityEngine;

namespace Menu;

public class MusicVisualizer : PositionedMenuObject
{
	public float[] audioSpectrum;

	public float[] spectrumValues;

	public FSprite[] visualizerSprites;

	public float[] spectrumTimers;

	public bool[] spectrumBeats;

	public float[] spectrumBiases;

	public float beatDelay;

	public float timer;

	public bool isBeat;

	public float lastScale;

	public bool isPlaying;

	public MusicVisualizer(Menu menu, MenuObject owner, Vector2 pos)
		: base(menu, owner, pos)
	{
		audioSpectrum = new float[64];
		visualizerSprites = new FSprite[15];
		spectrumBeats = new bool[15];
		spectrumTimers = new float[15];
		spectrumValues = new float[15];
		beatDelay = 0.15f;
		for (int i = 0; i < visualizerSprites.Length; i++)
		{
			visualizerSprites[i] = new FSprite("Futile_White");
			visualizerSprites[i].shader = menu.manager.rainWorld.Shaders["MenuText"];
			visualizerSprites[i].scaleX = 1.6f;
			visualizerSprites[i].SetAnchor(0f, 0f);
			Container.AddChild(visualizerSprites[i]);
		}
		spectrumBiases = new float[16]
		{
			9f, 7f, 5f, 3f, 2.7f, 2.3f, 2.1f, 1.9f, 1.7f, 1.5f,
			1f, 0.8f, 0.5f, 0.4f, 0.3f, 0.2f
		};
	}

	public override void Update()
	{
		base.Update();
		if (menu.manager.musicPlayer != null && menu.manager.musicPlayer.song != null && menu.manager.musicPlayer.song.subTracks[0] != null)
		{
			menu.manager.musicPlayer.song.subTracks[0].source.GetSpectrumData(audioSpectrum, 0, FFTWindow.BlackmanHarris);
			if (audioSpectrum != null && audioSpectrum.Length != 0)
			{
				spectrumValues[0] = audioSpectrum[0] * 100f;
				spectrumValues[1] = audioSpectrum[0] * 100f;
				spectrumValues[2] = audioSpectrum[0] * 100f;
				spectrumValues[3] = audioSpectrum[0] * 100f;
				spectrumValues[4] = audioSpectrum[0] * 100f;
				spectrumValues[5] = audioSpectrum[0] * 100f;
				spectrumValues[6] = audioSpectrum[0] * 100f;
				spectrumValues[7] = audioSpectrum[0] * 100f;
				spectrumValues[8] = audioSpectrum[0] * 100f;
				spectrumValues[9] = audioSpectrum[0] * 100f;
				spectrumValues[10] = audioSpectrum[0] * 100f;
				spectrumValues[11] = audioSpectrum[0] * 100f;
				spectrumValues[12] = audioSpectrum[1] * 100f;
				spectrumValues[13] = audioSpectrum[1] * 100f;
				spectrumValues[14] = audioSpectrum[1] * 100f;
			}
			isPlaying = true;
		}
		else
		{
			isPlaying = false;
			for (int i = 0; i < spectrumBeats.Length; i++)
			{
				spectrumBeats[i] = false;
			}
		}
	}

	public void OnBeat()
	{
		for (int i = 0; i < spectrumBeats.Length; i++)
		{
			float num = spectrumBiases[i];
			if (spectrumTimers[i] < beatDelay)
			{
				spectrumBeats[i] = false;
				spectrumTimers[i] += Time.deltaTime;
			}
			if (!spectrumBeats[i] && spectrumValues[i] > num && spectrumTimers[i] > beatDelay)
			{
				spectrumBeats[i] = true;
				spectrumTimers[i] = 0f;
			}
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (visualizerSprites == null)
		{
			return;
		}
		if (isPlaying)
		{
			OnBeat();
		}
		for (int i = 0; i < visualizerSprites.Length; i++)
		{
			float num = 39f * (float)i;
			visualizerSprites[i].x = owner.page.pos.x + pos.x + num;
			visualizerSprites[i].y = owner.page.pos.y + pos.y;
			if (spectrumBeats[i])
			{
				visualizerSprites[i].scaleY = Mathf.Lerp(visualizerSprites[i].scaleY, 10f, 10f * (0.5f * spectrumBiases[i]) * Time.deltaTime);
			}
			else
			{
				visualizerSprites[i].scaleY = Mathf.Lerp(visualizerSprites[i].scaleY, 0.1f, 3f * Time.deltaTime);
			}
		}
	}
}
