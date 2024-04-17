using System.Collections.Generic;
using System.Globalization;
using System.IO;
using RWCustom;

namespace MoreSlugcats;

public class DebugMTCycler
{
	public Dictionary<string, string> musicTriggers;

	public Room room;

	public int musicTime;

	public string substr(string str, int startIndex, int endIndex)
	{
		return str.Substring(startIndex, endIndex - startIndex);
	}

	public DebugMTCycler(Room room)
	{
		musicTriggers = new Dictionary<string, string>();
		this.room = room;
		string[] directories = Directory.GetDirectories(Custom.RootFolderDirectory() + "World");
		for (int i = 0; i < directories.Length; i++)
		{
			string path = directories[i] + "-Rooms";
			if (!Directory.Exists(path))
			{
				continue;
			}
			string[] files = Directory.GetFiles(path);
			for (int j = 0; j < files.Length; j++)
			{
				if (!files[j].Contains("Settings"))
				{
					continue;
				}
				string[] array = File.ReadAllLines(files[j]);
				for (int k = 0; k < array.Length; k++)
				{
					string text = array[k];
					while (text.Contains("MusicEvent") && !text.Contains("StopMusicEvent"))
					{
						string text2 = substr(text, text.IndexOf("MusicEvent"), text.IndexOf("vol<eB>"));
						string text3 = substr(text2, text2.IndexOf("<eB>") + 4, text2.LastIndexOf("<eA>"));
						string text4 = text.Substring(text.IndexOf("vol<eB>"));
						text4 = substr(text4, 0, text4.IndexOf("prio<eB>"));
						string text5 = substr(text4, text4.IndexOf("<eB>") + 4, text4.LastIndexOf("<eA>"));
						string key = text3 + "," + text5;
						string fileName = Path.GetFileName(files[j]);
						if (musicTriggers.ContainsKey(key))
						{
							musicTriggers[key] = musicTriggers[key] + "," + fileName;
						}
						else
						{
							musicTriggers.Add(key, fileName);
						}
						text = text.Substring(text.IndexOf("vol<eB>") + 1);
					}
				}
			}
		}
	}

	public void Update()
	{
		musicTime++;
		if (musicTime == 1 && musicTriggers.Keys.Count > 0)
		{
			string text = "";
			using (Dictionary<string, string>.KeyCollection.Enumerator enumerator = musicTriggers.Keys.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					text = enumerator.Current;
				}
			}
			string[] array = text.Split(',');
			string songName = array[0];
			float volume = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			Custom.Log("PLAYING --", musicTriggers[text]);
			MusicEvent musicEvent = new MusicEvent();
			musicEvent.songName = songName;
			musicEvent.volume = volume;
			musicEvent.prio = 1f;
			musicEvent.cyclesRest = 0;
			room.game.manager.musicPlayer.GameRequestsSong(musicEvent);
			musicTriggers.Remove(text);
		}
		if (musicTime == 1000)
		{
			StopMusicEvent stopMusicEvent = new StopMusicEvent();
			stopMusicEvent.type = StopMusicEvent.Type.AllSongs;
			stopMusicEvent.fadeOutTime = 120f;
			room.game.manager.musicPlayer.GameRequestsSongStop(stopMusicEvent);
		}
		if (musicTime >= 1200)
		{
			musicTime = 0;
		}
	}
}
