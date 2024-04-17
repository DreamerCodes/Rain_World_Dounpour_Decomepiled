using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Kittehface.Framework20;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu.Remix;

internal class InternalOI_Stats : InternalOI
{
	private Texture2D[] illustrationTextures;

	private const string imgTitle = "RemixTitle";

	private const string imgTitleS = "RemixShadow";

	private const int DPscale = 4;

	private ModManager.Mod[] modsDP;

	private Texture2D[] txtModsDP;

	private Texture2D[] txtModsDPg;

	private OpImage[] imgModsDP;

	private OpRect[] rectModsDP;

	private OpLabel lblGameVersion;

	private OpLabel lblModsDP;

	private OpLabel lblModsRegion;

	private OpLabel lblModsOI;

	private OpLabel lblModsFail;

	private OpLabel lblMapTuto;

	private OpLabel lblName;

	private OpLabel lblID;

	private OpLabel lblVersion;

	private OpLabel lblTargetVersion;

	private OpLabel lblAuthors;

	internal OpSimpleImageButton btnAuthorExtra;

	private OpLabelLong lblDescription;

	private OpLabelLong lblRequirements;

	private OpImage imgThumbnail;

	private UIelement boxCredits;

	private OpLabelLong lblCredits;

	internal OpScrollBox boxDescription;

	internal OpSimpleImageButton btnWorkshopUpload;

	private int _previewDelay;

	private float creditsFade;

	private bool creditsActive;

	internal ModManager.Mod previewMod { get; private set; }

	private static Color cRed => MenuModList.ModButton.cRed;

	public InternalOI_Stats()
		: base(new ModManager.Mod
		{
			id = "_Statistics_"
		}, Reason.Statistics)
	{
	}

	private void LoadIllustrations()
	{
		string[] array = new string[2] { "RemixTitle", "RemixShadow" };
		illustrationTextures = new Texture2D[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			if (Futile.atlasManager.GetAtlasWithName(text) != null)
			{
				break;
			}
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			string text2 = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + text + ".png");
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text2, clampWrapMode: true, crispPixels: true);
			HeavyTexturesCache.LoadAndCacheAtlasFromTexture(text, texture2D, textureFromAsset: false);
			illustrationTextures[i] = texture2D;
		}
	}

	public override void Initialize()
	{
		LoadIllustrations();
		Tabs = new OpTab[2];
		Tabs[0] = new OpTab(this);
		Tabs[1] = new OpTab(this);
		OpImage opImage = new OpImage(new Vector2(-383f, 116f), "RemixShadow");
		Tabs[0].AddItems(opImage);
		opImage = new OpImage(new Vector2(-383f, 116f), "RemixTitle");
		opImage.sprite.shader = Custom.rainWorld.Shaders["MenuText"];
		Tabs[0].AddItems(opImage);
		float num = _GetDPTexture();
		lblGameVersion = new OpLabel(new Vector2(510f, 580f), new Vector2(100f, 30f), "v1.9.15b", FLabelAlignment.Right)
		{
			verticalAlignment = OpLabel.LabelVAlignment.Top
		};
		lblModsDP = new OpLabel(new Vector2(20f, 440f), new Vector2(560f, 30f), OptionalText.GetText(OptionalText.ID.MenuModStat_ModsDownpour), FLabelAlignment.Left);
		lblModsRegion = new OpLabel(new Vector2(20f, 340f - num), new Vector2(560f, 30f), "", FLabelAlignment.Left);
		lblModsOI = new OpLabel(new Vector2(20f, 300f - num), new Vector2(560f, 30f), "", FLabelAlignment.Left);
		lblModsFail = new OpLabel(new Vector2(20f, 260f - num), new Vector2(560f, 30f), OptionalText.GetText(OptionalText.ID.MenuModStat_ModsFail), FLabelAlignment.Left);
		Tabs[0].AddItems(lblGameVersion, lblModsDP, lblModsRegion, lblModsOI, lblModsFail);
		lblMapTuto = new OpLabel(new Vector2(20f, 0f), new Vector2(560f, 30f), OptionalText.GetText(OptionalText.ID.MenuModStat_MapTuto).Replace("<Button>", OptionalText.GetButtonName_Map()))
		{
			alpha = 0f
		};
		Tabs[0].AddItems(lblMapTuto);
		previewMod = base.mod;
		lblName = new OpLabel(Vector2.zero, Vector2.one, "", FLabelAlignment.Left, bigText: true)
		{
			text = ""
		};
		lblID = new OpLabel(Vector2.zero, Vector2.one, "ID", FLabelAlignment.Left)
		{
			text = ""
		};
		Tabs[1].AddItems(lblID);
		lblVersion = new OpLabel(Vector2.zero, Vector2.one, "", FLabelAlignment.Left);
		lblTargetVersion = new OpLabel(Vector2.zero, Vector2.one, "", FLabelAlignment.Left)
		{
			color = MenuModList.ModButton.cOutdated
		};
		lblAuthors = new OpLabel(Vector2.zero, Vector2.one, "", FLabelAlignment.Left);
		lblDescription = new OpLabelLong(Vector2.zero, Vector2.one, "", autoWrap: true, FLabelAlignment.Center)
		{
			text = "",
			autoWrap = false
		};
		lblRequirements = new OpLabelLong(Vector2.zero, Vector2.one, "", autoWrap: true, FLabelAlignment.Center);
		btnAuthorExtra = new OpSimpleImageButton(Vector2.zero, Vector2.one * 30f, "Menu_InfoI")
		{
			description = ModdingMenu.instance.Translate("mod_menu_ext_authors")
		};
		btnAuthorExtra.OnClick += ShowAuthorExtra;
		Texture2D image = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		imgThumbnail = new OpImage(default(Vector2), image);
		Tabs[1].AddItems(lblName, lblVersion, lblTargetVersion, lblAuthors, lblDescription, lblRequirements, btnAuthorExtra, imgThumbnail);
		if (SteamManager.Initialized)
		{
			btnWorkshopUpload = new OpSimpleImageButton(new Vector2(560f, 440f), new Vector2(30f, 30f), "keyShiftB")
			{
				description = ModdingMenu.instance.Translate("mod_workshop_upload_buttondesc")
			};
			btnWorkshopUpload.OnClick += ShowDialogWorkshopUpload;
			Tabs[1].AddItems(btnWorkshopUpload);
		}
		base.OnUnload += UnloadOI;
	}

	private void UnloadOI()
	{
		for (int i = 0; i < txtModsDP.Length; i++)
		{
			Object.Destroy(txtModsDP[i]);
			txtModsDP[i] = null;
		}
		for (int j = 0; j < txtModsDPg.Length; j++)
		{
			Object.Destroy(txtModsDPg[j]);
			txtModsDPg[j] = null;
		}
		for (int k = 0; k < illustrationTextures.Length; k++)
		{
			Object.Destroy(illustrationTextures[k]);
			illustrationTextures[k] = null;
		}
		if (imgThumbnail.currentSpriteTexture != null)
		{
			Object.Destroy(imgThumbnail.currentSpriteTexture);
			imgThumbnail.currentSpriteTexture = null;
		}
	}

	private float _GetDPTexture()
	{
		IList<string> prePackagedModIDs = ModManager.PrePackagedModIDs;
		List<ModManager.Mod> list = new List<ModManager.Mod>();
		for (int i = 0; i < prePackagedModIDs.Count; i++)
		{
			for (int j = 0; j < ModManager.InstalledMods.Count; j++)
			{
				if (ModManager.InstalledMods[j].id == prePackagedModIDs[i] && !ModManager.InstalledMods[j].DLCMissing)
				{
					list.Add(ModManager.InstalledMods[j]);
					break;
				}
			}
		}
		modsDP = list.ToArray();
		txtModsDP = new Texture2D[modsDP.Length];
		txtModsDPg = new Texture2D[modsDP.Length];
		for (int k = 0; k < modsDP.Length; k++)
		{
			FAtlas atlasWithName = Futile.atlasManager.GetAtlasWithName(ConfigContainer._GetThumbnailName(modsDP[k].id));
			if (atlasWithName != null)
			{
				txtModsDP[k] = (atlasWithName.texture as Texture2D).Clone();
				TrimThumbnail(ref txtModsDP[k]);
				txtModsDPg[k] = txtModsDP[k].Clone();
				MenuColorEffect.TextureGreyscale(ref txtModsDPg[k]);
			}
			else
			{
				txtModsDP[k] = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
				txtModsDPg[k] = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			}
		}
		rectModsDP = new OpRect[modsDP.Length];
		imgModsDP = new OpImage[modsDP.Length];
		int num = 5;
		for (int l = 0; l < modsDP.Length; l++)
		{
			int num2 = l / num;
			rectModsDP[l] = new OpRect(new Vector2(20f + 116f * (float)(l % num), 378f - (float)num2 * 69f), new Vector2(111f, 64f));
			imgModsDP[l] = new OpImage(new Vector2(22f + 116f * (float)(l % num), 380f - (float)num2 * 69f), txtModsDP[l])
			{
				scale = Vector2.one / 4f
			};
		}
		OpTab obj = Tabs[0];
		UIelement[] elements = rectModsDP;
		obj.AddItems(elements);
		OpTab obj2 = Tabs[0];
		elements = imgModsDP;
		obj2.AddItems(elements);
		return (float)((modsDP.Length - 1) / num) * 69f;
		static void ClearChunk(ref Texture2D texture, int x, int y)
		{
			for (int m = x * 4; m < x * 4 + 4; m++)
			{
				for (int n = y * 4; n < y * 4 + 4; n++)
				{
					texture.SetPixel(m, n, Color.clear);
				}
			}
		}
		static void TrimThumbnail(ref Texture2D texture)
		{
			texture.wrapMode = TextureWrapMode.Clamp;
			for (int num3 = 0; num3 < 4; num3++)
			{
				for (int num4 = 0; num4 < 5; num4++)
				{
					ClearChunk(ref texture, (num3 <= 1) ? 106 : 0, (num3 % 2 == 0) ? num4 : (59 - num4));
				}
				for (int num5 = 0; num5 < 3; num5++)
				{
					ClearChunk(ref texture, (num3 > 1) ? 1 : 105, (num3 % 2 == 0) ? num5 : (59 - num5));
				}
				for (int num6 = 0; num6 < 2; num6++)
				{
					ClearChunk(ref texture, (num3 > 1) ? 2 : 104, (num3 % 2 == 0) ? num6 : (59 - num6));
				}
				ClearChunk(ref texture, (num3 > 1) ? 3 : 103, (num3 % 2 != 0) ? 59 : 0);
				ClearChunk(ref texture, (num3 > 1) ? 4 : 102, (num3 % 2 != 0) ? 59 : 0);
			}
			texture.filterMode = FilterMode.Point;
			texture.Apply();
		}
	}

	internal void _RefreshStats()
	{
		for (int i = 0; i < modsDP.Length; i++)
		{
			rectModsDP[i].colorEdge = (modsDP[i].enabled ? MenuColorEffect.rgbMediumGrey : MenuColorEffect.rgbDarkGrey);
			FSprite[] sprites = rectModsDP[i].rect.sprites;
			for (int j = 0; j < sprites.Length; j++)
			{
				sprites[j].shader = Custom.rainWorld.Shaders[modsDP[i].enabled ? "MenuText" : "Basic"];
			}
			imgModsDP[i].ChangeImage(modsDP[i].enabled ? txtModsDP[i] : txtModsDPg[i]);
			imgModsDP[i].alpha = (modsDP[i].enabled ? 1f : 0.5f);
			imgModsDP[i].scale = Vector2.one / 4f;
		}
		int num = 0;
		int num2 = 0;
		for (int k = 0; k < ModManager.ActiveMods.Count; k++)
		{
			if (ModManager.ActiveMods[k].modifiesRegions)
			{
				num++;
			}
		}
		lblModsRegion.text = OptionalText.GetText(OptionalText.ID.MenuModStat_ModsRegion).Replace("<Number>", num.ToString("N0", NumberFormatInfo.InvariantInfo));
		num = 0;
		for (int l = 0; l < ConfigContainer.menuTab.modList.modButtons.Length; l++)
		{
			if (ConfigContainer.menuTab.modList.modButtons[l].type != 0 && !ConfigContainer.menuTab.modList.modButtons[l].itf.mod.DLCMissing)
			{
				num++;
				if (ConfigContainer.menuTab.modList.modButtons[l].type == MenuModList.ModButton.ItfType.Configurable)
				{
					num2++;
				}
			}
		}
		lblModsOI.text = Custom.ReplaceLineDelimeters(OptionalText.GetText(OptionalText.ID.MenuModStat_ModsInterface).Replace("<Number1>", num.ToString("N0", NumberFormatInfo.InvariantInfo)).Replace("<Number2>", num2.ToString("N0", NumberFormatInfo.InvariantInfo)));
		lblModsFail.Hide();
	}

	private void ShowDialogWorkshopUpload(UIfocusable trigger)
	{
		ModdingMenu.instance.DisplayWorkshopUploadConfirmDialog(previewMod);
	}

	private bool _DelayPreview(MenuModList.ModButton button)
	{
		if (previewMod.id == button.itf.mod.id)
		{
			_previewDelay = 0;
			return true;
		}
		if (ModManager.MMF && Custom.rainWorld.options.quality != Options.Quality.HIGH)
		{
			if (lblName.CtlrInput.AnyDirectionalInput || Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f)
			{
				_previewDelay = 0;
				return true;
			}
			_previewDelay++;
			if (Custom.rainWorld.options.quality == Options.Quality.LOW && _previewDelay < ModdingMenu.DASinit)
			{
				return true;
			}
		}
		return false;
	}

	private void _PreviewMod(MenuModList.ModButton button)
	{
		ModManager.Mod mod = button.itf.mod;
		_previewDelay = 0;
		previewMod = mod;
		if (boxCredits != null)
		{
			DestroyCredits();
		}
		if (boxDescription != null)
		{
			DestroyBoxDesc();
		}
		string name = ConfigContainer._GetThumbnailName(mod.id);
		FAtlas atlasWithName = Futile.atlasManager.GetAtlasWithName(name);
		if (atlasWithName != null)
		{
			Texture2D texture = (atlasWithName.texture as Texture2D).Clone();
			if (button.itf.mod.DLCMissing)
			{
				MenuColorEffect.TextureGreyscale(ref texture);
			}
			Texture2D currentSpriteTexture = imgThumbnail.currentSpriteTexture;
			imgThumbnail.ChangeImage(texture);
			if (currentSpriteTexture != null)
			{
				Object.Destroy(currentSpriteTexture);
				currentSpriteTexture = null;
			}
			imgThumbnail.Show();
		}
		else
		{
			if (!button._thumbLoaded)
			{
				ConfigContainer.instance._LoadModThumbnail(button);
			}
			if (button._thumbBlank || atlasWithName == null)
			{
				imgThumbnail.Hide();
			}
			else
			{
				Texture2D texture2 = (atlasWithName.texture as Texture2D).Clone();
				if (button.itf.mod.DLCMissing)
				{
					MenuColorEffect.TextureGreyscale(ref texture2);
				}
				Texture2D currentSpriteTexture2 = imgThumbnail.currentSpriteTexture;
				imgThumbnail.ChangeImage(texture2);
				if (currentSpriteTexture2 != null)
				{
					Object.Destroy(currentSpriteTexture2);
					currentSpriteTexture2 = null;
				}
				imgThumbnail.Show();
			}
		}
		if (mod.DLCMissing)
		{
			lblName.color = MenuColorEffect.rgbDarkGrey;
		}
		else
		{
			lblName.color = MenuColorEffect.rgbMediumGrey;
		}
		lblName.size = new Vector2(560f, 40f);
		lblName.text = mod.LocalizedName;
		lblName.SetPos(new Vector2(20f, 600f - lblName.size.y - 10f));
		float num = lblName.PosY - 5f;
		if (btnWorkshopUpload != null)
		{
			if (Custom.rainWorld.processManager.mySteamManager != null && RainWorldSteamManager.ValidateModApplicableForWorkshopUpload(previewMod))
			{
				btnWorkshopUpload.Show();
				btnWorkshopUpload.PosY = lblName.PosY;
			}
			else
			{
				btnWorkshopUpload.Hide();
			}
		}
		if (mod.id != "RainWorld_BaseGame")
		{
			lblID.Show();
			if (mod.DLCMissing)
			{
				lblID.color = MenuColorEffect.rgbDarkGrey;
			}
			else
			{
				lblID.color = MenuColorEffect.rgbMediumGrey;
			}
			lblID.size = new Vector2(560f, 30f);
			lblID.text = "(" + OptionalText.GetText(OptionalText.ID.MenuModStat_ModID).Replace("<ModID>", mod.id) + ")";
			lblID.SetPos(new Vector2(20f, num - lblID.size.y + 10f));
			num -= lblID.size.y;
		}
		else
		{
			lblID.Hide();
		}
		if (string.IsNullOrEmpty(mod.version) || mod.hideVersion)
		{
			lblVersion.Hide();
		}
		else
		{
			lblVersion.Show();
			if (mod.DLCMissing)
			{
				lblVersion.color = MenuColorEffect.rgbDarkGrey;
			}
			else
			{
				lblVersion.color = MenuColorEffect.rgbMediumGrey;
			}
			lblVersion.size = new Vector2(560f, 30f);
			lblVersion.text = ModdingMenu.instance.Translate("Version") + ": " + mod.version;
			lblVersion.SetPos(new Vector2(20f, num - lblVersion.size.y));
			num -= lblVersion.size.y + 10f;
		}
		if (mod.targetGameVersion == "v1.9.15b")
		{
			lblTargetVersion.Hide();
		}
		else
		{
			lblTargetVersion.Show();
			if (mod.DLCMissing)
			{
				lblTargetVersion.color = MenuColorEffect.rgbDarkGrey;
			}
			else if ("v1.9.15b".StartsWith(mod.targetGameVersion))
			{
				lblTargetVersion.color = MenuColorEffect.rgbMediumGrey;
			}
			else
			{
				lblTargetVersion.color = MenuModList.ModButton.cOutdated;
			}
			lblTargetVersion.size = new Vector2(560f, 30f);
			lblTargetVersion.text = OptionalText.GetText(OptionalText.ID.MenuModStat_ModTargetVersion).Replace("<TargetVersion>", mod.targetGameVersion).Replace("<GameVersion>", "v1.9.15b");
			lblTargetVersion.SetPos(new Vector2(20f, num - lblTargetVersion.size.y));
			num -= lblTargetVersion.size.y + 10f;
		}
		if (string.IsNullOrEmpty(mod.authors) || mod.authors == "Unknown")
		{
			lblAuthors.Hide();
			btnAuthorExtra.Hide();
		}
		else
		{
			string text = ModdingMenu.instance.Translate("Author(s)");
			string text2 = (text + ": " + OptionInterface.Translate(mod.authors).Replace("<LINE>", " <LINE>")).WrapText(bigText: false, 560f);
			if (mod.DLCMissing)
			{
				lblAuthors.color = MenuColorEffect.rgbDarkGrey;
				btnAuthorExtra.colorEdge = MenuColorEffect.rgbDarkGrey;
			}
			else
			{
				lblAuthors.color = MenuColorEffect.rgbMediumGrey;
				btnAuthorExtra.colorEdge = MenuColorEffect.rgbMediumGrey;
			}
			if (text2.Split('\n').Length <= 2)
			{
				lblAuthors.size = new Vector2(560f, 100f);
				lblAuthors.text = text2;
				lblAuthors.size = new Vector2(560f, Mathf.Max(30f, lblAuthors.GetDisplaySize().y));
				lblAuthors.SetPos(new Vector2(20f, num - lblAuthors.size.y));
				btnAuthorExtra.Hide();
			}
			else
			{
				lblAuthors.size = new Vector2(560f, 30f);
				lblAuthors.text = text + ": ";
				lblAuthors.SetPos(new Vector2(20f, num - lblAuthors.size.y));
				btnAuthorExtra.Show();
				btnAuthorExtra.SetPos(new Vector2(lblAuthors.PosX + lblAuthors.label.textRect.width + 10f, lblAuthors.PosY));
			}
			num -= lblAuthors.size.y + 20f;
			lblAuthors.Show();
		}
		if (string.IsNullOrEmpty(mod.LocalizedDescription) || mod.LocalizedDescription == mod.descBlank)
		{
			lblDescription.Hide();
		}
		else
		{
			string text3 = mod.LocalizedDescription.Replace("<LINE>", " <LINE>").WrapText(bigText: false, 560f);
			if (mod.DLCMissing)
			{
				lblDescription.color = MenuColorEffect.rgbDarkGrey;
			}
			else
			{
				lblDescription.color = MenuColorEffect.rgbMediumGrey;
			}
			if (text3.Split('\n').Length <= 5)
			{
				lblDescription.size = new Vector2(560f, 600f);
				lblDescription.text = text3;
				lblDescription.size = new Vector2(560f, Mathf.Max(30f, lblDescription.GetDisplaySize().y));
				float y = lblDescription.size.y;
				lblDescription.SetPos(new Vector2(20f, num - y));
				lblDescription.Show();
				num -= lblDescription.size.y + 20f;
			}
			else
			{
				lblDescription.size = new Vector2(560f, 600f);
				lblDescription.text = text3;
				lblDescription.size = new Vector2(560f, Mathf.Max(30f, lblDescription.GetDisplaySize().y));
				boxDescription = new OpScrollBox(new Vector2(5f, num - 100f), new Vector2(590f, 100f), lblDescription.size.y + 10f);
				Tabs[1].AddItems(boxDescription);
				boxDescription.AddItems(lblDescription);
				lblDescription.SetPos(new Vector2(10f, 5f));
				lblDescription.Show();
				num -= boxDescription.size.y + 20f;
			}
		}
		if (!imgThumbnail.IsInactive)
		{
			imgThumbnail.SetPos(new Vector2(87f, num - 240f));
			num -= 260f;
		}
		lblRequirements.text = "";
		lblRequirements.size = new Vector2(560f, 600f);
		if (mod.DLCMissing)
		{
			string text4 = ModdingMenu.instance.Translate("remix_dlc_locked");
			bool flag = false;
			if (SteamManager.Initialized)
			{
				text4 = text4 + "<LINE>" + ModdingMenu.instance.Translate("remix_dlc_redirect");
				flag = true;
			}
			if (!flag && GogGalaxyManager.IsFullyInitialized())
			{
				text4 = text4 + "<LINE>" + ModdingMenu.instance.Translate("remix_dlc_redirect");
				flag = true;
			}
			if (!flag && AOC.HasStoreContentImpl())
			{
				text4 = text4 + "<LINE>" + ModdingMenu.instance.Translate("remix_dlc_redirect");
				flag = true;
			}
			string text5 = text4.Replace("<LINE>", " <LINE>").WrapText(bigText: false, 560f);
			lblRequirements.text = text5;
			lblRequirements.color = cRed;
		}
		else if (ModManager.FailedRequirementIds.Contains(mod.id))
		{
			string text6 = ModdingMenu.instance.Translate("mod_menu_fail_requirements") + "\r\n" + FailedRequirementsString(mod);
			lblRequirements.text = text6;
			lblRequirements.color = cRed;
		}
		else if (mod.requirements != null && mod.requirements.Length != 0)
		{
			string text7 = "(" + ModdingMenu.instance.Translate("mod_menu_requirement") + " " + RequirementsString(mod) + ")";
			lblRequirements.text = text7;
			lblRequirements.color = MenuColorEffect.rgbMediumGrey;
		}
		if (lblRequirements.text != "")
		{
			lblRequirements.size = new Vector2(560f, Mathf.Max(30f, lblRequirements.GetDisplaySize().y));
			lblRequirements.SetPos(new Vector2(20f, num - lblRequirements.size.y));
			lblRequirements.Show();
			num -= lblRequirements.size.y + 20f;
		}
		else
		{
			lblRequirements.Hide();
		}
		Tabs[1]._Update();
		Tabs[1]._GrafUpdate(0f);
	}

	private static string RequirementsString(ModManager.Mod mod)
	{
		MenuModList.ModButton modButton = ConfigContainer.menuTab.modList.GetModButton(mod.id);
		HashSet<string> hashSet = new HashSet<string>();
		foreach (int requirementIndex in modButton.requirementIndexes)
		{
			hashSet.Add(ConfigContainer.menuTab.modList.modButtons[requirementIndex].itf.mod.LocalizedName);
		}
		return string.Join(", ", hashSet);
	}

	private static string FailedRequirementsString(ModManager.Mod mod)
	{
		MenuModList.ModButton modButton = ConfigContainer.menuTab.modList.GetModButton(mod.id);
		HashSet<string> hashSet = new HashSet<string>();
		foreach (int requirementIndex in modButton.requirementIndexes)
		{
			hashSet.Add(ConfigContainer.menuTab.modList.modButtons[requirementIndex].itf.mod.LocalizedName);
		}
		for (int i = 0; i < mod.requirements.Length; i++)
		{
			string text = OptionInterface.Translate(mod.requirements[i] + "-name");
			if (text == mod.requirements[i] + "-name")
			{
				text = ((i >= mod.requirementsNames.Length || string.IsNullOrEmpty(mod.requirementsNames[i])) ? mod.requirements[i] : mod.requirementsNames[i]);
			}
			hashSet.Add(text);
		}
		return string.Join(", ", hashSet);
	}

	public override void Update()
	{
		base.Update();
		if (ConfigContainer.ActiveTabIndex == 0)
		{
			lblMapTuto.alpha = ConfigContainer.instance._cursorAlpha;
		}
		if (boxDescription != null && ConfigContainer.ActiveTabIndex == 0)
		{
			DestroyBoxDesc();
		}
		if (boxCredits != null)
		{
			if (ConfigContainer.ActiveTabIndex == 0)
			{
				DestroyCredits();
				return;
			}
			if (boxCredits is OpScrollBox && !boxCredits.MenuMouseMode && creditsActive && ((boxCredits as OpScrollBox).bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Jump) || (boxCredits as OpScrollBox).bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Throw) || (boxCredits as OpScrollBox).bumpBehav.JoystickPressAxis(vertical: false) != 0))
			{
				ShowAuthorExtra(null);
			}
			creditsFade = Custom.LerpAndTick(creditsFade, creditsActive ? 1f : 0f, 0.05f, 0.1f / UIelement.frameMulti);
			float num = Custom.SCurve(creditsFade, 0.65f);
			if (Mathf.Approximately(num, 0f))
			{
				boxCredits.Hide();
				lblCredits.Hide();
			}
			else
			{
				boxCredits.Show();
				lblCredits.Show();
				lblCredits.alpha = num;
				if (boxCredits is OpRect)
				{
					(boxCredits as OpRect).colorEdge = Color.Lerp(Color.clear, btnAuthorExtra.colorEdge, num);
					(boxCredits as OpRect).fillAlpha = num * 0.9f;
				}
				else
				{
					(boxCredits as OpScrollBox).colorEdge = Color.Lerp(Color.clear, btnAuthorExtra.colorEdge, num);
					(boxCredits as OpScrollBox).fillAlpha = num * 0.9f;
				}
			}
		}
		if (ConfigContainer.FocusedElement is MenuModList.IAmPartOfModList && ConfigContainer.FocusedElement is MenuModList.ModButton button)
		{
			_TryPreview(button);
		}
	}

	internal void _TryPreview(MenuModList.ModButton button)
	{
		if (!_DelayPreview(button))
		{
			if (ConfigContainer.ActiveTabIndex != 1)
			{
				ConfigContainer._ChangeActiveTab(1);
			}
			_PreviewMod(button);
		}
	}

	private void DestroyCredits()
	{
		OpTab.DestroyItems(lblCredits, boxCredits);
		lblCredits = null;
		boxCredits = null;
		btnAuthorExtra.soundClick = SoundID.MENU_Button_Standard_Button_Pressed;
	}

	private void DestroyBoxDesc()
	{
		OpScrollBox.RemoveItemsFromScrollBox(lblDescription);
		OpTab.DestroyItems(boxDescription);
		boxDescription = null;
	}

	private void ShowAuthorExtra(UIfocusable trigger)
	{
		if (boxCredits != null)
		{
			creditsActive = !creditsActive;
			btnAuthorExtra.soundClick = (creditsActive ? SoundID.MENU_Remove_Level : SoundID.MENU_Button_Standard_Button_Pressed);
			if (!(boxCredits is OpScrollBox))
			{
				return;
			}
			if (creditsActive)
			{
				ConfigContainer.instance._FocusNewElement(boxCredits as OpScrollBox, silent: true);
				if (!boxCredits.MenuMouseMode)
				{
					(boxCredits as OpScrollBox).NonMouseSetHeld(newHeld: true);
				}
				ConfigContainer.instance._allowFocusMove = false;
			}
			else
			{
				if (!boxCredits.MenuMouseMode)
				{
					(boxCredits as OpScrollBox).NonMouseSetHeld(newHeld: false);
				}
				ConfigContainer.instance._FocusNewElement(btnAuthorExtra, silent: true);
				ConfigContainer.instance._allowFocusMove = false;
			}
			return;
		}
		creditsActive = true;
		creditsFade = 0f;
		string text = OptionInterface.Translate(previewMod.authors).Replace("<LINE>", " <LINE>").WrapText(bigText: false, 400f, forceWrapping: true);
		btnAuthorExtra.soundClick = SoundID.MENU_Remove_Level;
		if (text.Split('\n').Length > 20)
		{
			lblCredits = new OpLabelLong(new Vector2(20f, 20f), new Vector2(400f, 600f), "Credits", autoWrap: true, FLabelAlignment.Center)
			{
				verticalAlignment = OpLabel.LabelVAlignment.Center,
				autoWrap = false
			};
			lblCredits.text = text;
			lblCredits.size = new Vector2(Mathf.Clamp(lblCredits.GetDisplaySize().x, 50f, 400f), Mathf.Max(25f, lblCredits.GetDisplaySize().y));
			float x = btnAuthorExtra.PosX + btnAuthorExtra.size.x;
			boxCredits = new OpScrollBox(new Vector2(x, 50f), new Vector2(lblCredits.size.x + 40f, 500f), lblCredits.size.y + 40f);
			Tabs[1].AddItems(boxCredits);
			(boxCredits as OpScrollBox).AddItems(lblCredits);
			ConfigContainer.instance._FocusNewElement(boxCredits as OpScrollBox, silent: true);
			if (!boxCredits.MenuMouseMode)
			{
				(boxCredits as OpScrollBox).NonMouseSetHeld(newHeld: true);
			}
			lblCredits.Update();
			lblCredits.GrafUpdate(0f);
		}
		else
		{
			boxCredits = new OpRect(Vector2.zero, Vector2.one, 0.9f);
			lblCredits = new OpLabelLong(Vector2.zero, new Vector2(400f, 600f), "Credits", autoWrap: true, FLabelAlignment.Center)
			{
				verticalAlignment = OpLabel.LabelVAlignment.Center,
				autoWrap = false
			};
			lblCredits.text = text;
			lblCredits.size = new Vector2(Mathf.Max(50f, lblCredits.GetDisplaySize().x), Mathf.Max(25f, lblCredits.GetDisplaySize().y));
			float num = btnAuthorExtra.PosX + btnAuthorExtra.size.x;
			float num2 = btnAuthorExtra.PosY + btnAuthorExtra.size.y;
			num2 = Mathf.Max(num2 - lblCredits.size.y - 40f, 20f);
			lblCredits.SetPos(new Vector2(num + 20f, num2 + 20f));
			boxCredits.size = new Vector2(lblCredits.size.x + 40f, lblCredits.size.y + 40f);
			boxCredits.pos = new Vector2(num, num2);
			Tabs[1].AddItems(boxCredits, lblCredits);
		}
	}
}
