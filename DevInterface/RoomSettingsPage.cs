using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class RoomSettingsPage : Page
{
	public class DevEffectsCategories : ExtEnum<DevEffectsCategories>
	{
		public static readonly DevEffectsCategories Gameplay = new DevEffectsCategories("Gameplay", register: true);

		public static readonly DevEffectsCategories Lighting = new DevEffectsCategories("Lighting", register: true);

		public static readonly DevEffectsCategories WaterAndFlooding = new DevEffectsCategories("WaterAndFlooding", register: true);

		public static readonly DevEffectsCategories Decorations = new DevEffectsCategories("Decorations", register: true);

		public static readonly DevEffectsCategories Insects = new DevEffectsCategories("Insects", register: true);

		public static readonly DevEffectsCategories Unsorted = new DevEffectsCategories("Unsorted", register: true);

		public DevEffectsCategories(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	private Panel effectsPanel;

	public RoomSettings.RoomEffect.Type[] effectTypes;

	public int maxObjectsPerPage = 20;

	public int currObjectsPage;

	public int totalObjectsPages;

	public RoomSettingsPage(DevUI owner, string IDstring, DevUINode parentNode, string name)
		: base(owner, IDstring, parentNode, name)
	{
		subNodes.Add(new DangerTypeCycler(owner, "Game_Over_Type_Cycler", this, new Vector2(170f, 660f), 120f));
		if (owner.room.world.region != null)
		{
			subNodes.Add(new InheritFromTemplateMenu(owner, "Inherit_From_Template_Menu", this, new Vector2(1050f, 730f), new Vector2(100f, 400f)));
			subNodes.Add(new SaveAsTemplateMenu(owner, "Save_As_Template_Menu", this, new Vector2(1200f, 730f), new Vector2(100f, 400f)));
		}
		subNodes.Add(new RoomSettingSlider(owner, "Rain_Intensity_Slider", this, new Vector2(120f, 640f), "Rain Intensity: ", RoomSettingSlider.Type.RainIntensity));
		subNodes.Add(new RoomSettingSlider(owner, "Rumble_Intensity_Slider", this, new Vector2(120f, 620f), "Rumble Intensity: ", RoomSettingSlider.Type.RumbleIntensity));
		subNodes.Add(new RoomSettingSlider(owner, "Ceiling_Drip_Slider", this, new Vector2(120f, 600f), "Ceiling Drips: ", RoomSettingSlider.Type.CeilingDrips));
		subNodes.Add(new RoomSettingSlider(owner, "Wave_Speed_Slider", this, new Vector2(120f, 580f), "Wave Speed: ", RoomSettingSlider.Type.WaveSpeed));
		subNodes.Add(new RoomSettingSlider(owner, "Wave_Length_Slider", this, new Vector2(120f, 560f), "Wave Length: ", RoomSettingSlider.Type.WaveLength));
		subNodes.Add(new RoomSettingSlider(owner, "Wave_Amplitude", this, new Vector2(120f, 540f), "Wave Amplitude: ", RoomSettingSlider.Type.WaveAmplitude));
		subNodes.Add(new RoomSettingSlider(owner, "Second_Wave_Length_Slider", this, new Vector2(120f, 520f), "Rollback Lgth: ", RoomSettingSlider.Type.SecondWaveLength));
		subNodes.Add(new RoomSettingSlider(owner, "Second_Wave_Amplitude", this, new Vector2(120f, 500f), "Rollback amp: ", RoomSettingSlider.Type.SecondWaveAmplitude));
		subNodes.Add(new RoomSettingSlider(owner, "Clouds_Slider", this, new Vector2(120f, 480f), "Clouds: ", RoomSettingSlider.Type.Clouds));
		subNodes.Add(new RoomSettingSlider(owner, "Grime_Slider", this, new Vector2(120f, 460f), "Grime: ", RoomSettingSlider.Type.Grime));
		subNodes.Add(new RoomSettingSlider(owner, "Obj_Density_Slider", this, new Vector2(120f, 440f), "Rndm Itm Density: ", RoomSettingSlider.Type.RandomObjsDens));
		subNodes.Add(new RoomSettingSlider(owner, "Obj_Spear_Chance_Slider", this, new Vector2(120f, 420f), "Rndm Itm Spear %: ", RoomSettingSlider.Type.RandomObjsSpearChance));
		subNodes.Add(new RoomSettingSlider(owner, "Water_Reflection_Alpha_Slider", this, new Vector2(120f, 400f), "Water Light: ", RoomSettingSlider.Type.WaterReflectionAplha));
		subNodes.Add(new Button(owner, "Room_Specific_Script", this, new Vector2(120f, 380f), 170f, base.RoomSettings.roomSpecificScript ? "!Room specific script ACTIVE!" : "No room specific script active"));
		subNodes.Add(new Button(owner, "Wet_Terrain", this, new Vector2(120f, 360f), 170f, base.RoomSettings.wetTerrain ? "Wet terrain ON" : "Wet terrain OFF"));
		Panel panel = new Panel(owner, "Palette_Panel", this, new Vector2(40f, 190f), new Vector2(210f, 95f + 20f * (float)owner.room.cameraPositions.Length), "PALETTE");
		panel.subNodes.Add(new PaletteController(owner, "Palette", panel, new Vector2(5f, panel.size.y - 20f), "Palette : ", 0));
		panel.subNodes.Add(new PaletteController(owner, "Effect_Color_A", panel, new Vector2(5f, panel.size.y - 40f), "Effect Color A: ", 1));
		panel.subNodes.Add(new PaletteController(owner, "Effect_Color_B", panel, new Vector2(5f, panel.size.y - 60f), "Effect Color B: ", 2));
		panel.subNodes.Add(new Panel.HorizontalDivider(owner, "Div", panel, panel.size.y - 65f));
		panel.subNodes.Add(new PaletteController(owner, "Fade_Palette", panel, new Vector2(5f, panel.size.y - 90f), "Fade Palette: ", 3));
		for (int i = 0; i < owner.room.cameraPositions.Length; i++)
		{
			panel.subNodes.Add(new PaletteFadeSlider(owner, "Palette_Fade_Slider_" + i, panel, new Vector2(5f, panel.size.y - 110f - (float)i * 20f), "Screen " + i + ": ", i));
		}
		subNodes.Add(panel);
		effectsPanel = new Panel(owner, "Effects_Panel", this, new Vector2(650f, 40f), new Vector2(400f, 260f), "EFFECTS: ");
		subNodes.Add(effectsPanel);
		for (int j = 0; j < 2; j++)
		{
			effectsPanel.subNodes.Add(new Button(owner, (j == 0) ? "Prev_Button" : "Next_Button", effectsPanel, new Vector2(5f + 100f * (float)j, effectsPanel.size.y - 16f - 5f), 95f, (j == 0) ? "Previous Page" : "Next Page"));
		}
		AssembleEffectsPages();
		RefreshEffectsPage();
	}

	public void RefreshEffectsPage()
	{
		if (totalObjectsPages == 0)
		{
			currObjectsPage = 0;
		}
		for (int num = effectsPanel.subNodes.Count - 1; num >= 2; num--)
		{
			effectsPanel.subNodes[num].ClearSprites();
			effectsPanel.subNodes.RemoveAt(num);
		}
		int num2 = currObjectsPage * maxObjectsPerPage;
		effectsPanel.Title = "EFFECTS: ...";
		for (int i = 0; i < maxObjectsPerPage && i + num2 < effectTypes.Length; i++)
		{
			float num3 = (float)maxObjectsPerPage / 2f;
			float num4 = (float)Mathf.FloorToInt((float)i / num3) * 195f;
			float num5 = 5f;
			num5 += num4;
			float num6 = 20f * num3 * (float)Mathf.FloorToInt((float)i / num3);
			float num7 = effectsPanel.size.y - 16f - 35f - 20f * (float)i;
			num7 += num6;
			if (i == 0)
			{
				effectsPanel.Title = "EFFECTS: " + DevEffectGetCategoryFromEffectType(effectTypes[num2 + i]);
			}
			if (effectTypes[num2 + i] != RoomSettings.RoomEffect.Type.None)
			{
				effectsPanel.subNodes.Add(new AddEffectButton(owner, effectsPanel, new Vector2(num5, num7), 190f, effectTypes[num2 + i]));
			}
		}
	}

	public DevEffectsCategories DevEffectGetCategoryFromEffectType(RoomSettings.RoomEffect.Type type)
	{
		if (type == RoomSettings.RoomEffect.Type.ZeroG || type == RoomSettings.RoomEffect.Type.BrokenZeroG || type == RoomSettings.RoomEffect.Type.SSSwarmers || type == RoomSettings.RoomEffect.Type.SSMusic || type == RoomSettings.RoomEffect.Type.ElectricDeath || type == RoomSettings.RoomEffect.Type.BorderPushBack || type == RoomSettings.RoomEffect.Type.DayNight || (ModManager.MSC && type == MoreSlugcatsEnums.RoomEffectType.RoomWrap) || (ModManager.MSC && type == MoreSlugcatsEnums.RoomEffectType.DustWave))
		{
			return DevEffectsCategories.Gameplay;
		}
		if (type == RoomSettings.RoomEffect.Type.SunBlock || type == RoomSettings.RoomEffect.Type.SuperStructureProjector || type == RoomSettings.RoomEffect.Type.ProjectedScanLines || type == RoomSettings.RoomEffect.Type.SkyBloom || type == RoomSettings.RoomEffect.Type.LightBurn || type == RoomSettings.RoomEffect.Type.SkyAndLightBloom || type == RoomSettings.RoomEffect.Type.Fog || type == RoomSettings.RoomEffect.Type.Bloom || type == RoomSettings.RoomEffect.Type.Darkness || type == RoomSettings.RoomEffect.Type.Brightness || type == RoomSettings.RoomEffect.Type.Contrast || type == RoomSettings.RoomEffect.Type.Desaturation || type == RoomSettings.RoomEffect.Type.Hue || type == RoomSettings.RoomEffect.Type.DarkenLights || type == RoomSettings.RoomEffect.Type.WaterLights)
		{
			return DevEffectsCategories.Lighting;
		}
		if (type == RoomSettings.RoomEffect.Type.SkyDandelions || type == RoomSettings.RoomEffect.Type.Lightning || type == RoomSettings.RoomEffect.Type.BkgOnlyLightning || type == RoomSettings.RoomEffect.Type.ExtraLoudThunder || type == RoomSettings.RoomEffect.Type.GreenSparks || type == RoomSettings.RoomEffect.Type.VoidMelt || type == RoomSettings.RoomEffect.Type.ZeroGSpecks || type == RoomSettings.RoomEffect.Type.CorruptionSpores || type == RoomSettings.RoomEffect.Type.AboveCloudsView || type == RoomSettings.RoomEffect.Type.RoofTopView || type == RoomSettings.RoomEffect.Type.VoidSpawn || type == RoomSettings.RoomEffect.Type.FairyParticles || type == RoomSettings.RoomEffect.Type.Coldness || type == RoomSettings.RoomEffect.Type.HeatWave || type == RoomSettings.RoomEffect.Type.Dustpuffs || (ModManager.MSC && type == MoreSlugcatsEnums.RoomEffectType.Advertisements))
		{
			return DevEffectsCategories.Decorations;
		}
		if (type == RoomSettings.RoomEffect.Type.RockFlea || type == RoomSettings.RoomEffect.Type.Flies || type == RoomSettings.RoomEffect.Type.FireFlies || type == RoomSettings.RoomEffect.Type.TinyDragonFly || type == RoomSettings.RoomEffect.Type.RedSwarmer || type == RoomSettings.RoomEffect.Type.Ant || type == RoomSettings.RoomEffect.Type.Beetle || type == RoomSettings.RoomEffect.Type.WaterGlowworm || type == RoomSettings.RoomEffect.Type.Wasp || type == RoomSettings.RoomEffect.Type.Moth)
		{
			return DevEffectsCategories.Insects;
		}
		if (type == RoomSettings.RoomEffect.Type.LightRain || type == RoomSettings.RoomEffect.Type.HeavyRain || type == RoomSettings.RoomEffect.Type.HeavyRainFlux || type == RoomSettings.RoomEffect.Type.BulletRain || type == RoomSettings.RoomEffect.Type.BulletRainFlux || type == RoomSettings.RoomEffect.Type.WaterFluxFrequency || type == RoomSettings.RoomEffect.Type.WaterFluxMinLevel || type == RoomSettings.RoomEffect.Type.WaterFluxMaxLevel || type == RoomSettings.RoomEffect.Type.WaterFluxMinDelay || type == RoomSettings.RoomEffect.Type.WaterFluxMaxDelay || type == RoomSettings.RoomEffect.Type.LethalWater || type == RoomSettings.RoomEffect.Type.SilenceWater || type == RoomSettings.RoomEffect.Type.WaterViscosity || type == RoomSettings.RoomEffect.Type.WaterDepth || (ModManager.MSC && type == MoreSlugcatsEnums.RoomEffectType.InvertedWater) || type == RoomSettings.RoomEffect.Type.DirtyWater || type == RoomSettings.RoomEffect.Type.WaterFluxSpeed || type == RoomSettings.RoomEffect.Type.WaterFluxOffset || type == RoomSettings.RoomEffect.Type.WaterFluxRumble || (ModManager.MSC && type == MoreSlugcatsEnums.RoomEffectType.FastFloodDrain) || (ModManager.MSC && type == MoreSlugcatsEnums.RoomEffectType.FastFloodPullDown) || type == RoomSettings.RoomEffect.Type.LavaSurface)
		{
			return DevEffectsCategories.WaterAndFlooding;
		}
		return DevEffectsCategories.Unsorted;
	}

	public void AssembleEffectsPages()
	{
		maxObjectsPerPage = 22;
		Dictionary<DevEffectsCategories, List<RoomSettings.RoomEffect.Type>> dictionary = new Dictionary<DevEffectsCategories, List<RoomSettings.RoomEffect.Type>>();
		foreach (string entry in ExtEnum<DevEffectsCategories>.values.entries)
		{
			dictionary[new DevEffectsCategories(entry)] = new List<RoomSettings.RoomEffect.Type>();
		}
		foreach (string entry2 in ExtEnum<RoomSettings.RoomEffect.Type>.values.entries)
		{
			RoomSettings.RoomEffect.Type type = new RoomSettings.RoomEffect.Type(entry2);
			DevEffectsCategories key = DevEffectGetCategoryFromEffectType(type);
			dictionary[key].Add(type);
		}
		int num = 0;
		foreach (string entry3 in ExtEnum<DevEffectsCategories>.values.entries)
		{
			DevEffectsCategories key2 = new DevEffectsCategories(entry3);
			int num2 = maxObjectsPerPage * Mathf.CeilToInt(((float)dictionary[key2].Count + 0.5f) / ((float)maxObjectsPerPage + 1f));
			while (dictionary[key2].Count < num2)
			{
				dictionary[key2].Add(RoomSettings.RoomEffect.Type.None);
			}
			num += num2;
		}
		int num3 = 0;
		effectTypes = new RoomSettings.RoomEffect.Type[num];
		foreach (string entry4 in ExtEnum<DevEffectsCategories>.values.entries)
		{
			DevEffectsCategories key3 = new DevEffectsCategories(entry4);
			for (int i = 0; i < dictionary[key3].Count; i++)
			{
				effectTypes[num3] = dictionary[key3][i];
				num3++;
			}
		}
		totalObjectsPages = 1 + (int)((float)effectTypes.Length / (float)maxObjectsPerPage + 0.5f);
	}

	public override void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (type == DevUISignalType.ButtonClick)
		{
			string iDstring = sender.IDstring;
			if (iDstring == null)
			{
				return;
			}
			switch (iDstring)
			{
			case "Save_Settings":
				base.RoomSettings.Save();
				break;
			case "Export_Sandbox":
				(owner.game.GetArenaGameSession as SandboxGameSession).editor.DevToolsExportConfig();
				break;
			case "Save_Specific":
				base.RoomSettings.Save(owner.game.GetStorySession.saveStateNumber);
				break;
			case "Room_Specific_Script":
				base.RoomSettings.roomSpecificScript = !base.RoomSettings.roomSpecificScript;
				(sender as Button).Text = (base.RoomSettings.roomSpecificScript ? "!Room specific script ACTIVE!" : "No room specific script active");
				break;
			case "Wet_Terrain":
				base.RoomSettings.wetTerrain = !base.RoomSettings.wetTerrain;
				(sender as Button).Text = (base.RoomSettings.wetTerrain ? "Wet terrain ON" : "Wet terrain OFF");
				break;
			case "Prev_Button":
				currObjectsPage--;
				if (currObjectsPage < 0)
				{
					currObjectsPage = totalObjectsPages - 1;
				}
				RefreshEffectsPage();
				break;
			case "Next_Button":
				currObjectsPage++;
				if (currObjectsPage >= totalObjectsPages)
				{
					currObjectsPage = 0;
				}
				RefreshEffectsPage();
				break;
			}
		}
		else
		{
			if (!(type == DevUISignalType.Create))
			{
				return;
			}
			RoomSettings.RoomEffect.Type type2 = new RoomSettings.RoomEffect.Type(message);
			bool flag = true;
			for (int i = 0; i < base.RoomSettings.effects.Count; i++)
			{
				if (!base.RoomSettings.effects[i].inherited && base.RoomSettings.effects[i].type == type2)
				{
					base.RoomSettings.RemoveEffect(type2);
					flag = false;
				}
			}
			if (flag)
			{
				Vector2 vector = new Vector2(20f, 20f);
				for (int j = 0; j < 10; j++)
				{
					bool flag2 = false;
					for (int k = 0; k < tempNodes.Count; k++)
					{
						if (tempNodes[k] is PositionedDevUINode && Custom.DistLess(vector, (tempNodes[k] as PositionedDevUINode).absPos, 10f))
						{
							vector += new Vector2(10f, 10f);
							flag2 = true;
						}
					}
					if (!flag2)
					{
						break;
					}
				}
				RoomSettings.RoomEffect roomEffect = new RoomSettings.RoomEffect(type2, 0f, inherited: false);
				bool overWrite = false;
				for (int num = base.RoomSettings.effects.Count - 1; num >= 0; num--)
				{
					if (base.RoomSettings.effects[num].type == type2)
					{
						base.RoomSettings.effects.RemoveAt(num);
						overWrite = true;
					}
				}
				roomEffect.overWrite = overWrite;
				base.RoomSettings.effects.Add(roomEffect);
				EffectPanel effectPanel = new EffectPanel(owner, this, vector, roomEffect);
				effectPanel.AbsMove(vector);
				tempNodes.Add(effectPanel);
				subNodes.Add(effectPanel);
			}
			Refresh();
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		for (int i = 0; i < base.RoomSettings.effects.Count; i++)
		{
			EffectPanel item = new EffectPanel(owner, this, base.RoomSettings.effects[i].panelPosition, base.RoomSettings.effects[i]);
			tempNodes.Add(item);
			subNodes.Add(item);
		}
	}
}
