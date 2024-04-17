using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kittehface.Framework20;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

public abstract class OptionInterface
{
	public class ConfigHolder
	{
		public readonly OptionInterface owner;

		internal Dictionary<string, ConfigurableBase> configurables;

		internal Dictionary<string, string> strayConfigurables;

		private static string configDirPath;

		private string configLocalData;

		internal bool pendingReset;

		public ConfigHolder(OptionInterface owner)
		{
			this.owner = owner;
			this.owner.OnConfigChanged += owner.ResetUIelements;
			configurables = new Dictionary<string, ConfigurableBase>();
			strayConfigurables = new Dictionary<string, string>();
		}

		public Configurable<T> Bind<T>(string key, T defaultValue, ConfigAcceptableBase accept)
		{
			return Bind(key, defaultValue, new ConfigurableInfo("", accept, ""));
		}

		public Configurable<T> Bind<T>(string key, T defaultValue, ConfigurableInfo info = null)
		{
			if (string.IsNullOrEmpty(key))
			{
				key = "_";
			}
			if (key != key.Trim())
			{
				throw new ArgumentException("key [" + key + "] is wrong; Cannot use whitespace characters at start or end of key name");
			}
			if (key.Any((char c) => !char.IsLetterOrDigit(c) && c != '_'))
			{
				throw new ArgumentException("key [" + key + "] is wrong; Only IsLetterOrDigit or underscore are allowed for key name");
			}
			if (configurables.ContainsKey(key))
			{
				throw new ArgumentException("key [" + key + "] is already in use");
			}
			if (!ValueConverter.CanConvert(typeof(T)))
			{
				throw new ArgumentException(string.Concat("Type ", typeof(T), " is not supported. Supported types: ", string.Join(", ", (from x in ValueConverter.GetSupportedTypes()
					select x.Name).ToArray())));
			}
			Configurable<T> configurable = new Configurable<T>(owner, key, defaultValue, info);
			if (!configurable.IsCosmetic)
			{
				if (strayConfigurables.ContainsKey(key))
				{
					try
					{
						configurable.Value = ValueConverter.ConvertToValue<T>(strayConfigurables[key]);
					}
					catch (Exception msg)
					{
						MachineConnector.LogWarning($"[{key}] Failed to parse \"{strayConfigurables[key]}\" in {configurable.settingType}! Using default value.");
						MachineConnector.LogWarning(msg);
						configurable.Value = ValueConverter.ConvertToValue<T>(configurable.defaultValue);
					}
					strayConfigurables.Remove(key);
				}
				configurables.Add(key, configurable);
			}
			if (pendingReset)
			{
				configurable.Value = ValueConverter.ConvertToValue<T>(configurable.defaultValue);
			}
			return configurable;
		}

		private string GetFileKey()
		{
			return "configs_" + owner.mod.id;
		}

		private string GetConfigPath()
		{
			if (string.IsNullOrEmpty(configDirPath))
			{
				configDirPath = Path.Combine(Path.GetFullPath(UserData.GetPersistentDataPath()), "ModConfigs");
				DirectoryInfo directoryInfo = new DirectoryInfo(configDirPath);
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
			}
			return Path.Combine(configDirPath, owner.mod.id + ".txt");
		}

		internal void Reload()
		{
			strayConfigurables.Clear();
			pendingReset = false;
			string configPath = GetConfigPath();
			if (!File.Exists(configPath))
			{
				return;
			}
			string[] array = File.ReadAllText(configPath, Encoding.UTF8).Split('\n');
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].Trim();
				if (text.StartsWith("#"))
				{
					continue;
				}
				string[] array2 = text.Split(new char[1] { '=' }, 2);
				if (array2.Length != 2)
				{
					continue;
				}
				string text2 = array2[0].Trim();
				string text3 = array2[1].Trim();
				configurables.TryGetValue(text2, out var value);
				if (value != null)
				{
					try
					{
						value.BoxedValue = ValueConverter.ConvertToValue(text3, value.settingType);
					}
					catch (Exception msg)
					{
						MachineConnector.LogWarning($"[{text2}] Failed to parse \"{text3}\" in {value.settingType}! Using default value.");
						MachineConnector.LogWarning(msg);
						value.BoxedValue = ValueConverter.ConvertToValue(value.defaultValue, value.settingType);
					}
				}
				else
				{
					strayConfigurables[text2] = text3;
				}
			}
			try
			{
				owner.OnConfigChanged?.Invoke();
			}
			catch (Exception msg2)
			{
				MachineConnector.LogError(msg2);
			}
		}

		internal void Save()
		{
			StringBuilder stringBuilder = new StringBuilder();
			var source = configurables.Select((KeyValuePair<string, ConfigurableBase> x) => new
			{
				Key = x.Key,
				entry = x.Value,
				value = ValueConverter.ConvertToString(x.Value.BoxedValue, x.Value.settingType)
			}).Concat(strayConfigurables.Select((KeyValuePair<string, string> x) => new
			{
				Key = x.Key,
				entry = (ConfigurableBase)null,
				value = x.Value
			}));
			string text = (string.IsNullOrEmpty(owner.mod.name) ? owner.mod.id : (owner.mod.name + " (" + owner.mod.id + ")"));
			stringBuilder.AppendLine("## Config file for " + text);
			string text2 = owner.mod.authors;
			if (!string.IsNullOrEmpty(text2))
			{
				if (text2.Contains("<LINE>"))
				{
					text2 = Regex.Split(text2, "<LINE>")[0];
				}
				stringBuilder.AppendLine("## Mod Author(s): " + text2);
			}
			stringBuilder.AppendLine("## Mod Version: " + owner.mod.version);
			string text3 = owner.mod.description;
			if (!string.IsNullOrEmpty(text3))
			{
				if (text3.Contains("<LINE>"))
				{
					text3 = Regex.Split(text3, "<LINE>")[0];
				}
				stringBuilder.AppendLine("## Mod Description: " + text3);
			}
			stringBuilder.AppendLine();
			foreach (var item in source.OrderBy(x => x.Key))
			{
				if (item.entry != null)
				{
					if (item.entry.info != null && !string.IsNullOrEmpty(item.entry.info.description))
					{
						stringBuilder.AppendLine("## " + item.entry.info.description.Replace("\n", "\n## "));
					}
					stringBuilder.AppendLine("# Setting type: " + item.entry.settingType.Name);
					stringBuilder.AppendLine("# Default value: " + item.entry.defaultValue);
					if (item.entry.info != null && item.entry.info.acceptable != null)
					{
						stringBuilder.AppendLine(item.entry.info.acceptable.ToDescriptionString());
					}
					else if (item.entry.settingType.IsEnum)
					{
						stringBuilder.AppendLine("# Acceptable values: " + string.Join(", ", Enum.GetNames(item.entry.settingType)));
					}
				}
				stringBuilder.AppendLine(item.Key + " = " + item.value);
				stringBuilder.AppendLine();
			}
			File.WriteAllText(GetConfigPath(), stringBuilder.ToString(), Encoding.UTF8);
			Options options = Custom.rainWorld.options;
			if (options.optionsLoaded && options.optionsFile != null)
			{
				options.optionsFile.Remove(GetFileKey());
			}
			pendingReset = false;
		}

		public string LoadConfig()
		{
			Options options = Custom.rainWorld.options;
			if (options.optionsLoaded)
			{
				if (options.optionsFile != null)
				{
					return options.optionsFile.Get(GetFileKey(), "");
				}
				if (configLocalData != null)
				{
					return configLocalData;
				}
			}
			return "";
		}

		public void SaveConfig(string s)
		{
			Options options = Custom.rainWorld.options;
			if (options.optionsLoaded)
			{
				if (options.optionsFile != null)
				{
					options.optionsFile.Set(GetFileKey(), s, (!options.optionsFileCanSave) ? UserData.WriteMode.Deferred : UserData.WriteMode.Immediate);
				}
				else
				{
					configLocalData = s;
				}
			}
		}

		public bool ContainsConfig()
		{
			Options options = Custom.rainWorld.options;
			if (options.optionsLoaded)
			{
				if (options.optionsFile != null)
				{
					return options.optionsFile.Contains(GetFileKey());
				}
				return configLocalData != null;
			}
			return false;
		}
	}

	public OpTab[] Tabs;

	public ConfigHolder config;

	internal static readonly Color errorBlue = new Color(0.1216f, 0.4039f, 0.6941f, 1f);

	internal bool error;

	public ModManager.Mod mod { get; internal set; }

	public event OnEventHandler OnConfigChanged;

	public event OnEventHandler OnConfigReset;

	public event OnEventHandler OnActivate;

	public event OnEventHandler OnDeactivate;

	public event OnEventHandler OnUnload;

	public OptionInterface()
		: this(new ModManager.Mod())
	{
	}

	public virtual void Initialize()
	{
		error = false;
		Tabs = null;
	}

	public virtual void Update()
	{
	}

	public virtual string ValidationString()
	{
		string text = ValidationString_ID();
		if (!HasConfigurables())
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, ConfigurableBase> configurable in config.configurables)
		{
			if (!configurable.Value.IsCosmetic)
			{
				stringBuilder.Append(ValueConverter.ConvertToString(configurable.Value.BoxedValue, configurable.Value.settingType));
			}
		}
		return text + " " + stringBuilder.ToString().GetHashCode().ToString(NumberFormatInfo.InvariantInfo);
	}

	protected string ValidationString_ID()
	{
		string text = "[" + mod.id;
		if (!string.IsNullOrEmpty(mod.version) && !mod.hideVersion)
		{
			text = text + "~" + mod.version;
		}
		return text + "]";
	}

	public static string Translate(string text)
	{
		string text2 = Custom.rainWorld.inGameTranslator.Translate(text);
		if (string.IsNullOrEmpty(text2) || text2 == "!NO TRANSLATION!")
		{
			return text;
		}
		return text2;
	}

	internal bool HasConfigurables()
	{
		return config.configurables.Count > 0;
	}

	internal OptionInterface(ModManager.Mod rwMod)
	{
		mod = rwMod;
		config = new ConfigHolder(this);
	}

	internal void ShowConfigs()
	{
		if (!HasConfigurables())
		{
			return;
		}
		foreach (UIconfig item in GrabUIConfigs())
		{
			item.ShowConfig();
		}
	}

	internal HashSet<UIconfig> GrabUIConfigs()
	{
		HashSet<UIconfig> hashSet = new HashSet<UIconfig>();
		if (Tabs == null)
		{
			return hashSet;
		}
		for (int i = 0; i < Tabs.Length; i++)
		{
			if (Tabs[i] == null)
			{
				continue;
			}
			UIelement[] array = Tabs[i].items.ToArray();
			foreach (UIelement uIelement in array)
			{
				if (uIelement.GetType().IsSubclassOf(typeof(UIconfig)) && !(uIelement as UIconfig).cosmetic)
				{
					hashSet.Add(uIelement as UIconfig);
				}
			}
		}
		return hashSet;
	}

	internal void _LoadConfigFile()
	{
		if (HasConfigurables())
		{
			config.Reload();
		}
	}

	internal void _SaveConfigFile()
	{
		if (!HasConfigurables())
		{
			return;
		}
		HashSet<UIconfig> hashSet = GrabUIConfigs();
		foreach (ConfigurableBase value in config.configurables.Values)
		{
			if (!value.IsCosmetic)
			{
				if (value.BoundUIconfig != null && hashSet.Contains(value.BoundUIconfig))
				{
					value.BoxedValue = ValueConverter.ConvertToValue(value.BoundUIconfig.value, value.settingType);
				}
				else if (config.pendingReset)
				{
					value.BoxedValue = ValueConverter.ConvertToValue(value.defaultValue, value.settingType);
				}
			}
		}
		try
		{
			this.OnConfigChanged?.Invoke();
		}
		catch (Exception msg)
		{
			MachineConnector.LogError(msg);
		}
		config.Save();
	}

	private void ResetUIelements()
	{
		if (Tabs == null || Tabs.Length < 1)
		{
			return;
		}
		OpTab[] tabs = Tabs;
		for (int i = 0; i < tabs.Length; i++)
		{
			foreach (UIelement item in tabs[i].items)
			{
				if (item is UIconfig && (item as UIconfig).cosmetic)
				{
					item.Reset();
				}
			}
		}
	}

	internal void _TriggerOnConfigReset()
	{
		try
		{
			this.OnConfigReset?.Invoke();
		}
		catch (Exception msg)
		{
			MachineConnector.LogError(msg);
		}
	}

	internal void _TriggerOnActivate()
	{
		try
		{
			this.OnActivate?.Invoke();
		}
		catch (Exception msg)
		{
			MachineConnector.LogError(msg);
		}
	}

	internal void _TriggerOnDeactivate()
	{
		try
		{
			this.OnDeactivate?.Invoke();
		}
		catch (Exception msg)
		{
			MachineConnector.LogError(msg);
		}
	}

	internal void _TriggerOnUnload()
	{
		try
		{
			this.OnUnload?.Invoke();
		}
		catch (Exception msg)
		{
			MachineConnector.LogError(msg);
		}
	}

	internal void ErrorScreen(Exception ex, bool isInit)
	{
		MachineConnector.LogError(ex);
		error = true;
		if (Tabs != null)
		{
			for (int i = 0; i < Tabs.Length; i++)
			{
				if (Tabs[i] != null)
				{
					try
					{
						Tabs[i]._Unload();
					}
					catch
					{
					}
				}
			}
		}
		ConfigContainer.activeTab = null;
		Tabs = new OpTab[1]
		{
			new OpTab(this)
		};
		float num = InternalOI_Auto.AddBasicProfile(Tabs[0], mod) - 20f;
		OpRect opRect = new OpRect(new Vector2(30f, 20f), new Vector2(540f, num - 20f))
		{
			fillAlpha = 0.7f,
			colorFill = errorBlue
		};
		Tabs[0].AddItems(opRect);
		OpLabel opLabel = new OpLabel(new Vector2(100f, opRect.size.y - 25f), new Vector2(30f, 25f), ":(", FLabelAlignment.Left, bigText: true)
		{
			color = MenuColorEffect.rgbWhite
		};
		OpLabel opLabel2 = new OpLabel(new Vector2(150f, opRect.size.y - 20f), new Vector2(300f, 20f), OptionalText.GetText(isInit ? OptionalText.ID.OIError_Message_Initialize : OptionalText.ID.OIError_Message_Update))
		{
			color = MenuColorEffect.rgbWhite
		};
		OpLabel opLabel3 = new OpLabelLong(new Vector2(50f, 40f), new Vector2(500f, opRect.size.y - 70f), ex.ToString())
		{
			color = MenuColorEffect.rgbWhite,
			allowOverflow = true
		};
		if (opLabel3.GetDisplaySize().y > opRect.size.y - 90f)
		{
			OpScrollBox opScrollBox = new OpScrollBox(opRect.pos, opRect.size, opLabel3.GetDisplaySize().y + 90f, horizontal: false, hasBack: false);
			Tabs[0].AddItems(opScrollBox);
			opScrollBox.AddItems(opLabel, opLabel2, opLabel3);
			opLabel.SetPos(new Vector2(70f, opScrollBox.contentSize - 45f));
			opLabel2.SetPos(new Vector2(120f, opScrollBox.contentSize - 40f));
			opLabel3.size = new Vector2(500f, opScrollBox.contentSize - 70f);
			opLabel3.SetPos(new Vector2(10f, 10f));
		}
		else
		{
			Tabs[0].AddItems(opLabel, opLabel2, opLabel3);
		}
		ModdingMenu.instance.PlaySound(SoundID.MENU_Continue_Game);
		ConfigContainer._ChangeActiveTab(0);
		ConfigContainer.menuTab.tabCtrler.Change();
		ConfigContainer.instance._FocusNewElement(ConfigContainer.menuTab.BackButton, silent: true);
	}
}
