using UnityEngine;

namespace DevInterface;

public class AmbientSoundPanel : Panel
{
	public class SoundPanelSlider : Slider
	{
		public AmbientSound sound => (parentNode as AmbientSoundPanel).sound;

		public SoundPanelSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 25f)
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			float num = 0f;
			switch (IDstring)
			{
			case "Volume_Slider":
				num = sound.volume;
				base.NumberText = (int)(num * 100f) + "%";
				break;
			case "Pitch_Slider":
				num = Mathf.InverseLerp(0.1f, 1.9f, sound.pitch);
				base.NumberText = (int)(sound.pitch * 100f) + "%";
				break;
			case "Doppler_Slider":
				num = (sound as DopplerAffectedSound).dopplerFac;
				base.NumberText = (int)(num * 100f) + "%";
				break;
			case "Taper_Slider":
				num = (sound as SpotSound).taper;
				base.NumberText = (int)(num * 100f) + "%";
				break;
			}
			RefreshNubPos(num);
		}

		public override void NubDragged(float nubPos)
		{
			if (!sound.inherited)
			{
				switch (IDstring)
				{
				case "Volume_Slider":
					sound.volume = nubPos;
					break;
				case "Pitch_Slider":
					sound.pitch = Mathf.Lerp(0.1f, 1.9f, nubPos);
					break;
				case "Doppler_Slider":
					(sound as DopplerAffectedSound).dopplerFac = nubPos;
					break;
				case "Taper_Slider":
					(sound as SpotSound).taper = nubPos;
					break;
				}
			}
			Refresh();
		}
	}

	public AmbientSound sound;

	public AmbientSoundPanel(DevUI owner, DevUINode parentNode, Vector2 pos, AmbientSound sound)
		: base(owner, "Sound_Panel_" + sound.sample, parentNode, pos, new Vector2(170f, 50f), ((sound is OmniDirectionalSound) ? "(O)" : ((sound is DirectionalSound) ? "(D)" : "(S)")) + sound.sample)
	{
		this.sound = sound;
		if (sound is DopplerAffectedSound)
		{
			size.y += 20f;
		}
		if (sound is SpotSound)
		{
			size.y += 20f;
		}
		Move(sound.panelPosition);
		subNodes.Add(new SoundPanelSlider(owner, "Volume_Slider", this, new Vector2(5f, size.y - 25f), "Vol: "));
		subNodes.Add(new SoundPanelSlider(owner, "Pitch_Slider", this, new Vector2(5f, size.y - 45f), "Ptch: "));
		if (sound is DopplerAffectedSound)
		{
			subNodes.Add(new SoundPanelSlider(owner, "Doppler_Slider", this, new Vector2(5f, size.y - 65f), "Dplr: "));
		}
		if (sound is SpotSound)
		{
			subNodes.Add(new SoundPanelSlider(owner, "Taper_Slider", this, new Vector2(5f, size.y - 85f), "Tpr: "));
			subNodes.Add(new SpotSoundHandle(owner, "Spot_Sound_Handle", this, sound as SpotSound, pos + new Vector2(-50f, -100f), "Spot_Sound_Handle_" + sound.sample));
		}
		else if (sound is DirectionalSound)
		{
			subNodes.Add(new DirectionalSoundHandle(owner, "Directional_Sound_Handle", this, sound as DirectionalSound, pos, "Directional_Sound_Handle_" + sound.sample));
		}
	}

	public override void Update()
	{
		base.Update();
		if (dragged && (base.Page as SoundPage).draggedObject == null)
		{
			(base.Page as SoundPage).draggedObject = this;
		}
	}

	public override void Move(Vector2 newPos)
	{
		base.Move(newPos);
		sound.panelPosition = absPos;
	}

	public override void Refresh()
	{
		base.Refresh();
		if (!(sound is SpotSound))
		{
			fSprites[0].color = (sound.overWrite ? new Color(0.5f, 0f, 0f) : (sound.inherited ? new Color(0f, 0f, 0.5f) : new Color(0f, 0.5f, 0f)));
		}
	}
}
