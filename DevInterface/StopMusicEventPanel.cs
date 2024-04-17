using UnityEngine;

namespace DevInterface;

public class StopMusicEventPanel : StandardEventPanel, IDevUISignals
{
	public SelectSongPanel songsPanel;

	public Button songButton;

	public Button typeButton;

	public StopMusicEvent stopMusicEvent => base.tEvent as StopMusicEvent;

	public StopMusicEventPanel(DevUI owner, DevUINode parentNode)
		: base(owner, parentNode, 120f)
	{
		songButton = new Button(owner, "Song_Button", this, new Vector2(5f, size.y - 45f), 235f, stopMusicEvent.songName);
		subNodes.Add(songButton);
		subNodes.Add(new MusicEventPanel.MusicEventSlider(owner, "Stop_Song_Priority_Slider", this, new Vector2(5f, size.y - 65f), "Priority: "));
		subNodes.Add(new MusicEventPanel.MusicEventSlider(owner, "Stop_Song_Fade_Out_Slider", this, new Vector2(5f, size.y - 85f), "Fade out: "));
		typeButton = new Button(owner, "Type_Button", this, new Vector2(5f, 5f), 235f, "");
		subNodes.Add(typeButton);
	}

	public override void Refresh()
	{
		base.Refresh();
		if (songButton != null)
		{
			songButton.Text = ((stopMusicEvent.type == StopMusicEvent.Type.AllSongs) ? "N/A" : stopMusicEvent.songName);
		}
		if (typeButton != null)
		{
			typeButton.Text = ((stopMusicEvent.type == StopMusicEvent.Type.AllSongs) ? "Stop all songs" : ((stopMusicEvent.type == StopMusicEvent.Type.SpecificSong) ? "Stop specific song" : "Stop all but specific song"));
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
		if (sender == typeButton)
		{
			int num = stopMusicEvent.type.Index;
			if (num < 0)
			{
				num = 0;
			}
			num++;
			if (num >= ExtEnum<StopMusicEvent.Type>.values.Count)
			{
				num = 0;
			}
			stopMusicEvent.type = new StopMusicEvent.Type(ExtEnum<StopMusicEvent.Type>.values.GetEntry(num));
			Refresh();
		}
		if (sender.parentNode == songsPanel)
		{
			songsPanel.ClearSprites();
			subNodes.Remove(songsPanel);
			songsPanel = null;
			stopMusicEvent.songName = sender.IDstring;
			Refresh();
		}
	}
}
