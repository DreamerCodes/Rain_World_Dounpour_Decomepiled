using UnityEngine;

namespace DevInterface;

public class MusicEventPanel : StandardEventPanel, IDevUISignals
{
	public class MusicEventSlider : Slider
	{
		public MusicEventPanel musicPanel => parentNode as MusicEventPanel;

		public MusicEvent musicEvent => musicPanel.musicEvent;

		public MusicEventSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 100f)
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			float num = 0f;
			switch (IDstring)
			{
			case "Priority_Slider":
				num = musicEvent.prio;
				base.NumberText = ((int)(num * 100f)).ToString();
				break;
			case "Threat_Slider":
				num = musicEvent.maxThreatLevel;
				if (num == 1f)
				{
					base.NumberText = "NA";
				}
				else
				{
					base.NumberText = (int)(num * 100f) + "%";
				}
				break;
			case "Rooms_Range_Slider":
				num = (float)musicEvent.roomsRange / 40f;
				if (musicEvent.roomsRange < 0)
				{
					base.NumberText = "NA";
				}
				else
				{
					base.NumberText = musicEvent.roomsRange.ToString();
				}
				break;
			case "Drone_Remain_Slider":
				num = musicEvent.droneTolerance;
				base.NumberText = (int)(num * 100f) + "%";
				break;
			case "Volume_Slider":
				num = musicEvent.volume;
				base.NumberText = (int)(num * 100f) + "%";
				break;
			case "Cycles_Rest_Slider":
				num = (float)musicEvent.cyclesRest / 80f;
				if (musicEvent.cyclesRest < 0)
				{
					base.NumberText = "1tm";
					num = 1f;
				}
				else
				{
					base.NumberText = musicEvent.cyclesRest.ToString();
				}
				break;
			case "Fade_In_Slider":
				num = musicEvent.fadeInTime / 600f;
				base.NumberText = num * 15f + "s";
				break;
			case "Stop_Song_Priority_Slider":
				num = (parentNode as StopMusicEventPanel).stopMusicEvent.prio;
				base.NumberText = ((int)(num * 100f)).ToString();
				break;
			case "Stop_Song_Fade_Out_Slider":
				num = (parentNode as StopMusicEventPanel).stopMusicEvent.fadeOutTime / 1200f;
				base.NumberText = num * 30f + "s";
				break;
			}
			RefreshNubPos(num);
		}

		public override void NubDragged(float nubPos)
		{
			switch (IDstring)
			{
			case "Priority_Slider":
				musicEvent.prio = nubPos;
				break;
			case "Threat_Slider":
				musicEvent.maxThreatLevel = nubPos;
				break;
			case "Rooms_Range_Slider":
				musicEvent.roomsRange = (int)(nubPos * 40f);
				if (nubPos == 1f)
				{
					musicEvent.roomsRange = -1;
				}
				break;
			case "Drone_Remain_Slider":
				musicEvent.droneTolerance = nubPos;
				break;
			case "Volume_Slider":
				musicEvent.volume = nubPos;
				break;
			case "Cycles_Rest_Slider":
				musicEvent.cyclesRest = (int)(nubPos * 80f);
				if (nubPos == 1f)
				{
					musicEvent.cyclesRest = -1;
				}
				break;
			case "Fade_In_Slider":
				musicEvent.fadeInTime = nubPos * 600f;
				break;
			case "Stop_Song_Priority_Slider":
				(parentNode as StopMusicEventPanel).stopMusicEvent.prio = nubPos;
				break;
			case "Stop_Song_Fade_Out_Slider":
				(parentNode as StopMusicEventPanel).stopMusicEvent.fadeOutTime = nubPos * 1200f;
				break;
			}
			Refresh();
		}
	}

	public SelectSongPanel songsPanel;

	public Button songButton;

	public Button loopButton;

	public Button onePerCycleButton;

	public Button stopAtDeathButton;

	public Button stopAtGateButton;

	public MusicEvent musicEvent => base.tEvent as MusicEvent;

	public MusicEventPanel(DevUI owner, DevUINode parentNode)
		: base(owner, parentNode, 240f)
	{
		songButton = new Button(owner, "Song_Button", this, new Vector2(5f, size.y - 45f), 235f, musicEvent.songName);
		subNodes.Add(songButton);
		subNodes.Add(new MusicEventSlider(owner, "Volume_Slider", this, new Vector2(5f, size.y - 70f), "Play volume: "));
		subNodes.Add(new MusicEventSlider(owner, "Fade_In_Slider", this, new Vector2(5f, size.y - 90f), "Fade in: "));
		subNodes.Add(new MusicEventSlider(owner, "Priority_Slider", this, new Vector2(5f, size.y - 110f), "Song priority: "));
		subNodes.Add(new MusicEventSlider(owner, "Drone_Remain_Slider", this, new Vector2(5f, size.y - 130f), "Drone volume: "));
		subNodes.Add(new MusicEventSlider(owner, "Threat_Slider", this, new Vector2(5f, size.y - 150f), "Fade out at threat: "));
		subNodes.Add(new MusicEventSlider(owner, "Cycles_Rest_Slider", this, new Vector2(5f, size.y - 170f), "Rest cycles: "));
		subNodes.Add(new MusicEventSlider(owner, "Rooms_Range_Slider", this, new Vector2(5f, size.y - 190f), "Room transitions: "));
		loopButton = new Button(owner, "Loop_Button", this, new Vector2(size.x - 70f, 25f), 65f, "");
		subNodes.Add(loopButton);
		onePerCycleButton = new Button(owner, "Once_Per_Cycle_Button", this, new Vector2(5f, 25f), size.x - 80f, "");
		subNodes.Add(onePerCycleButton);
		stopAtDeathButton = new Button(owner, "Stop_At_Death_Button", this, new Vector2(5f, 5f), 100f, "");
		subNodes.Add(stopAtDeathButton);
		stopAtGateButton = new Button(owner, "Stop_At_Gate_Button", this, new Vector2(110f, 5f), 130f, "");
		subNodes.Add(stopAtGateButton);
	}

	public override void Refresh()
	{
		base.Refresh();
		if (songButton != null)
		{
			songButton.Text = musicEvent.songName;
		}
		if (loopButton != null)
		{
			loopButton.Text = (musicEvent.loop ? "Loop" : "Play Once");
		}
		if (onePerCycleButton != null)
		{
			onePerCycleButton.Text = (musicEvent.oneSongPerCycle ? "One song per cycle ON" : "One song per cycle OFF");
		}
		if (stopAtDeathButton != null)
		{
			stopAtDeathButton.Text = (musicEvent.stopAtDeath ? "Stop at death" : "Continue at death");
		}
		if (stopAtGateButton != null)
		{
			stopAtGateButton.Text = (musicEvent.stopAtGate ? "Stop at gate" : "Continue through gate");
		}
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (sender == songButton)
		{
			if (songsPanel != null)
			{
				songsPanel.ClearSprites();
				subNodes.Remove(songsPanel);
				songsPanel = null;
			}
			else
			{
				songsPanel = new SelectSongPanel(owner, this, new Vector2(10f, 10f), (owner.activePage as TriggersPage).songNames);
				subNodes.Add(songsPanel);
				songsPanel.AbsMove(new Vector2(10f, 10f));
			}
		}
		else if (sender == loopButton)
		{
			musicEvent.loop = !musicEvent.loop;
			Refresh();
		}
		else if (sender == onePerCycleButton)
		{
			musicEvent.oneSongPerCycle = !musicEvent.oneSongPerCycle;
			Refresh();
		}
		else if (sender == stopAtDeathButton)
		{
			musicEvent.stopAtDeath = !musicEvent.stopAtDeath;
			Refresh();
		}
		else if (sender == stopAtGateButton)
		{
			musicEvent.stopAtGate = !musicEvent.stopAtGate;
			Refresh();
		}
		if (sender.parentNode == songsPanel)
		{
			songsPanel.ClearSprites();
			subNodes.Remove(songsPanel);
			songsPanel = null;
			musicEvent.songName = sender.IDstring;
			Refresh();
		}
	}
}
