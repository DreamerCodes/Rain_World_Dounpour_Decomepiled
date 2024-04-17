using System.Collections.Generic;
using System.IO;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class SoundPage : Page
{
	public class SoundTypes : ExtEnum<SoundTypes>
	{
		public static readonly SoundTypes Omnidirectional = new SoundTypes("Omnidirectional", register: true);

		public static readonly SoundTypes Directional = new SoundTypes("Directional", register: true);

		public static readonly SoundTypes Spot = new SoundTypes("Spot", register: true);

		public SoundTypes(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public DevUINode draggedObject;

	public DevUINode removeIfReleaseObject;

	private TrashBin trashBin;

	public AddSoundType[] typeButtons;

	public int soundType;

	public int maxFilesPerPage = 28;

	public int currFilesPage;

	public int totalFilePages;

	private string[] fileNames;

	private Panel soundsPanel;

	public SoundPage(DevUI owner, string IDstring, DevUINode parentNode, string name)
		: base(owner, IDstring, parentNode, name)
	{
		subNodes.Add(new RoomSettingSlider(owner, "Drone_Volume_Slider", this, new Vector2(120f, 640f), "Bkg Drone: ", RoomSettingSlider.Type.BkgDroneVolume));
		subNodes.Add(new RoomSettingSlider(owner, "Drone_No_Threat_Vol", this, new Vector2(120f, 620f), "No threat drone vol: ", RoomSettingSlider.Type.BkgDroneNoThreatVol));
		DirectoryInfo directoryInfo = null;
		FileInfo[] files = ((!Directory.Exists("./Assets/LoadedSoundEffects/Ambient/")) ? new DirectoryInfo(AssetManager.ResolveDirectory("LoadedSoundEffects" + Path.DirectorySeparatorChar + "Ambient")) : new DirectoryInfo("./Assets/LoadedSoundEffects/Ambient/")).GetFiles();
		List<string> list = new List<string>();
		for (int i = 0; i < files.Length; i++)
		{
			list.Add(files[i].Name);
		}
		string[] array = AssetManager.ListDirectory("soundeffects/ambient");
		for (int j = 0; j < array.Length; j++)
		{
			bool flag = true;
			for (int k = 0; k < list.Count; k++)
			{
				if (list[k] == Path.GetFileName(array[j]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				list.Add(Path.GetFileName(array[j]));
			}
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].Length > 5 && list[num].Substring(list[num].Length - 5, 5) == ".meta")
			{
				list.RemoveAt(num);
			}
		}
		fileNames = list.ToArray();
		totalFilePages = 1 + (int)((float)fileNames.Length / (float)maxFilesPerPage + 0.5f);
		if (owner.room.world.region != null)
		{
			subNodes.Add(new InheritFromTemplateMenu(owner, "Inherit_From_Template_Menu", this, new Vector2(1050f, 730f), new Vector2(100f, 400f)));
			subNodes.Add(new SaveAsTemplateMenu(owner, "Save_As_Template_Menu", this, new Vector2(1200f, 730f), new Vector2(100f, 400f)));
		}
		typeButtons = new AddSoundType[3];
		soundsPanel = new Panel(owner, "Sounds_Panel", this, new Vector2(1050f, 20f), new Vector2(300f, (float)maxFilesPerPage * 20f + 55f), "CREATE SOUND");
		for (int l = 0; l < ExtEnum<SoundTypes>.values.Count; l++)
		{
			soundsPanel.subNodes.Add(new AddSoundType(owner, soundsPanel, new Vector2(5f + 93.333336f * (float)l, soundsPanel.size.y - 16f - 5f), 90f, l, ExtEnum<SoundTypes>.values.GetEntry(l)));
		}
		for (int m = 0; m < 2; m++)
		{
			soundsPanel.subNodes.Add(new Button(owner, (m == 0) ? "Prev_Button" : "Next_Button", soundsPanel, new Vector2(5f + 140f * (float)m, soundsPanel.size.y - 16f - 25f), 135f, (m == 0) ? "Previous Page" : "Next Page"));
		}
		RefreshFilesPage();
		subNodes.Add(soundsPanel);
		trashBin = new TrashBin(owner, "Trash_Bin", this, new Vector2(40f, 40f));
		subNodes.Add(trashBin);
	}

	public override void Update()
	{
		draggedObject = null;
		base.Update();
		if (draggedObject != null && trashBin.MouseOver)
		{
			trashBin.LineColor = ((Random.value < 0.5f) ? new Color(1f, 0f, 0f) : new Color(1f, 1f, 1f));
			removeIfReleaseObject = draggedObject;
		}
		else
		{
			trashBin.LineColor = new Color(1f, 1f, 1f);
			if (!owner.mouseDown && removeIfReleaseObject != null)
			{
				RemoveObject(removeIfReleaseObject);
			}
			removeIfReleaseObject = null;
		}
		if (owner.game.manager.musicPlayer != null && owner.game.manager.musicPlayer.threatTracker != null)
		{
			owner.game.manager.musicPlayer.threatTracker.currentThreat = Mathf.InverseLerp(0f, 1300f, Futile.mousePosition.x);
		}
	}

	public void RefreshFilesPage()
	{
		if (totalFilePages == 0)
		{
			currFilesPage = 0;
		}
		for (int num = soundsPanel.subNodes.Count - 1; num >= 5; num--)
		{
			soundsPanel.subNodes[num].ClearSprites();
			soundsPanel.subNodes.RemoveAt(num);
		}
		int num2 = currFilesPage * maxFilesPerPage;
		for (int i = 0; i < maxFilesPerPage && i + num2 < fileNames.Length; i++)
		{
			soundsPanel.subNodes.Add(new AddSoundButton(owner, soundsPanel, new Vector2(5f, soundsPanel.size.y - 16f - 55f - 20f * (float)i), 290f, fileNames[num2 + i]));
		}
	}

	public override void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (type == DevUISignalType.ButtonClick)
		{
			switch (sender.IDstring)
			{
			case "Save_Settings":
				base.RoomSettings.Save();
				break;
			case "Export_Sandbox":
				(owner.game.GetArenaGameSession as SandboxGameSession).editor.DevToolsExportConfig();
				break;
			case "Prev_Button":
				currFilesPage--;
				if (currFilesPage < 0)
				{
					currFilesPage = totalFilePages - 1;
				}
				RefreshFilesPage();
				break;
			case "Next_Button":
				currFilesPage++;
				if (currFilesPage >= totalFilePages)
				{
					currFilesPage = 0;
				}
				RefreshFilesPage();
				break;
			case "Save_Specific":
				base.RoomSettings.Save(owner.game.GetStorySession.saveStateNumber);
				break;
			}
		}
		else if (type == DevUISignalType.Create)
		{
			CreateSoundRep(message);
		}
	}

	private void RemoveObject(DevUINode objRep)
	{
		if (objRep is AmbientSoundPanel)
		{
			base.RoomSettings.ambientSounds.Remove((objRep as AmbientSoundPanel).sound);
		}
		Refresh();
	}

	public override void Refresh()
	{
		base.Refresh();
		for (int i = 0; i < base.RoomSettings.ambientSounds.Count; i++)
		{
			AmbientSoundPanel ambientSoundPanel = new AmbientSoundPanel(owner, this, base.RoomSettings.ambientSounds[i].panelPosition, base.RoomSettings.ambientSounds[i]);
			ambientSoundPanel.Move(base.RoomSettings.ambientSounds[i].panelPosition);
			tempNodes.Add(ambientSoundPanel);
			subNodes.Add(ambientSoundPanel);
		}
		for (int j = 0; j < owner.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers.Count; j++)
		{
			owner.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[j].slatedForDeletion = true;
		}
		for (int k = 0; k < base.RoomSettings.ambientSounds.Count; k++)
		{
			owner.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers.Add(new AmbientSoundPlayer(owner.room.game.cameras[0].virtualMicrophone, base.RoomSettings.ambientSounds[k]));
		}
	}

	private void CreateSoundRep(string sampleName)
	{
		bool flag = true;
		if (soundType != AmbientSound.Type.Spot.Index)
		{
			for (int i = 0; i < base.RoomSettings.ambientSounds.Count; i++)
			{
				if (!base.RoomSettings.ambientSounds[i].inherited && base.RoomSettings.ambientSounds[i].sample == sampleName && base.RoomSettings.ambientSounds[i].type.Index == soundType)
				{
					base.RoomSettings.RemoveAmbientSound(new AmbientSound.Type(ExtEnum<AmbientSound.Type>.values.GetEntry(soundType)), sampleName);
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			Vector2 vector = Vector2.Lerp(owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(Random.value * 360f) * 0.2f + new Vector2(40f, 40f);
			AmbientSound ambientSound;
			switch (soundType)
			{
			case 0:
				ambientSound = new OmniDirectionalSound(sampleName, inherited: false);
				break;
			case 1:
				ambientSound = new DirectionalSound(sampleName, inherited: false);
				break;
			default:
				ambientSound = new SpotSound(sampleName, inherited: false);
				(ambientSound as SpotSound).pos = vector + owner.room.game.cameras[0].pos;
				break;
			}
			if (soundType != AmbientSound.Type.Spot.Index)
			{
				bool overWrite = false;
				for (int num = base.RoomSettings.ambientSounds.Count - 1; num >= 0; num--)
				{
					if (base.RoomSettings.ambientSounds[num].type.Index == soundType && base.RoomSettings.ambientSounds[num].sample == sampleName)
					{
						vector = base.RoomSettings.ambientSounds[num].panelPosition;
						base.RoomSettings.ambientSounds.RemoveAt(num);
						overWrite = true;
					}
				}
				ambientSound.overWrite = overWrite;
			}
			base.RoomSettings.ambientSounds.Add(ambientSound);
			ambientSound.panelPosition = vector;
		}
		Refresh();
	}
}
