using System.Collections.Generic;
using System.Globalization;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Menu.Remix;

public class InternalOI_Auto : InternalOI
{
	public bool automated = true;

	internal bool IsAutomated
	{
		get
		{
			if (automated)
			{
				return HasConfigurables();
			}
			return false;
		}
	}

	internal InternalOI_Auto(ModManager.Mod rwMod)
		: base(rwMod, Reason.NoInterface)
	{
	}

	public override void Initialize()
	{
		if (!IsAutomated)
		{
			base.Initialize();
			return;
		}
		Dictionary<string, List<ConfigurableBase>> dictionary = new Dictionary<string, List<ConfigurableBase>>();
		foreach (ConfigurableBase value2 in config.configurables.Values)
		{
			string key = "";
			if (value2.info != null && !string.IsNullOrEmpty(value2.info.autoTab))
			{
				key = value2.info.autoTab;
			}
			if (dictionary.TryGetValue(key, out var value))
			{
				value.Add(value2);
				dictionary.Remove(key);
				dictionary.Add(key, value);
			}
			else
			{
				value = new List<ConfigurableBase> { value2 };
				dictionary.Add(key, value);
			}
		}
		Tabs = new OpTab[dictionary.Count];
		List<string> list = new List<string>(dictionary.Keys);
		if (list.Contains(""))
		{
			list.Remove("");
			list.Sort(CompareNames);
			list.Insert(0, "");
		}
		else
		{
			list.Sort(CompareNames);
		}
		for (int i = 0; i < list.Count; i++)
		{
			Tabs[i] = new OpTab(this, list[i]);
			List<ConfigurableBase> list2 = dictionary[list[i]];
			list2.Sort(CompareCfgBase);
			List<UIQueue> list3 = new List<UIQueue>();
			for (int j = 0; j < list2.Count; j++)
			{
				list3.Add(_CreateQueue(list2[j]));
			}
			float offsetY = 20f;
			UIQueue.InitializeQueues(Tabs[i], 20f, ref offsetY, list3.ToArray());
		}
	}

	private UIQueue _CreateQueue(ConfigurableBase cfg)
	{
		UIQueue result = null;
		switch (ValueConverter.GetConverter(cfg.settingType).category)
		{
		case ValueConverter.TypeCategory.Boolean:
			if (cfg is Configurable<bool>)
			{
				result = new OpCheckBox.Queue(cfg as Configurable<bool>);
			}
			break;
		case ValueConverter.TypeCategory.Integrals:
			if (cfg.info != null && cfg.info.acceptable != null)
			{
				int num = (int)cfg.info.acceptable.Clamp(int.MaxValue) - (int)cfg.info.acceptable.Clamp(int.MinValue) + 1;
				if (num <= 21)
				{
					result = new OpSliderTick.Queue(cfg);
					break;
				}
				if (num <= 101)
				{
					result = new OpSlider.Queue(cfg);
					break;
				}
			}
			if (cfg is Configurable<int>)
			{
				result = new OpUpdown.QueueInt(cfg as Configurable<int>);
			}
			break;
		case ValueConverter.TypeCategory.Floats:
			if (cfg.info != null && cfg.info.acceptable != null)
			{
				if ((float)cfg.info.acceptable.Clamp(float.MaxValue) - (float)cfg.info.acceptable.Clamp(float.MinValue) <= 51f)
				{
					result = new OpFloatSlider.Queue(cfg);
				}
				else if (cfg is Configurable<float>)
				{
					result = new OpUpdown.QueueFloat(cfg as Configurable<float>, 1);
				}
			}
			else if (cfg is Configurable<float>)
			{
				result = new OpUpdown.QueueFloat(cfg as Configurable<float>, 1);
			}
			break;
		case ValueConverter.TypeCategory.String:
			if (cfg is Configurable<string>)
			{
				result = ((cfg.info == null || cfg.info.acceptable == null || !(cfg.info.acceptable is ConfigAcceptableList<string>)) ? new OpTextBox.Queue(cfg) : (((cfg.info.acceptable as ConfigAcceptableList<string>).AcceptableValues.Length >= 5) ? ((UIconfig.ConfigQueue)new OpComboBox.Queue(cfg as Configurable<string>)) : ((UIconfig.ConfigQueue)new OpListBox.Queue(cfg as Configurable<string>, (ushort)(cfg.info.acceptable as ConfigAcceptableList<string>).AcceptableValues.Length))));
			}
			break;
		case ValueConverter.TypeCategory.Misc:
			if (cfg is Configurable<Color>)
			{
				result = new OpColorPicker.Queue(cfg as Configurable<Color>);
			}
			break;
		case ValueConverter.TypeCategory.Enum:
			result = ((!(cfg is Configurable<KeyCode>)) ? ((cfg.settingType.GetEnumNames().Length >= 5) ? ((UIconfig.ConfigQueue)new OpResourceSelector.QueueEnum(cfg)) : ((UIconfig.ConfigQueue)new OpResourceList.QueueEnum(cfg, (ushort)cfg.settingType.GetEnumNames().Length))) : new OpKeyBinder.Queue(cfg as Configurable<KeyCode>));
			break;
		case ValueConverter.TypeCategory.ExtEnum:
			result = ((ExtEnumBase.GetNames(cfg.settingType).Length >= 5) ? ((UIconfig.ConfigQueue)new OpResourceSelector.QueueEnum(cfg)) : ((UIconfig.ConfigQueue)new OpResourceList.QueueEnum(cfg, (ushort)ExtEnumBase.GetNames(cfg.settingType).Length)));
			break;
		}
		return result;
	}

	private static int CompareNames(string x, string y)
	{
		return ConfigContainer.comInfo.Compare(x, y, CompareOptions.StringSort);
	}

	private static int CompareCfgBase(ConfigurableBase x, ConfigurableBase y)
	{
		return ConfigContainer.comInfo.Compare(x.key, y.key, CompareOptions.StringSort);
	}

	public static float AddBasicProfile(OpTab tab, ModManager.Mod mod)
	{
		OpLabel opLabel = new OpLabel(Vector2.zero, Vector2.one, "Name", FLabelAlignment.Left, bigText: true)
		{
			text = ""
		};
		opLabel.size = new Vector2(560f, 40f);
		opLabel.text = mod.LocalizedName;
		opLabel.SetPos(new Vector2(20f, 600f - opLabel.size.y - 10f));
		float num = opLabel.PosY - 5f;
		OpLabel opLabel2 = new OpLabel(Vector2.zero, Vector2.one, "", FLabelAlignment.Left);
		if (string.IsNullOrEmpty(mod.version) || mod.hideVersion)
		{
			opLabel2.Hide();
		}
		else
		{
			opLabel2.Show();
			opLabel2.size = new Vector2(560f, 30f);
			opLabel2.text = ModdingMenu.instance.Translate("Version") + ": " + mod.version;
			opLabel2.SetPos(new Vector2(20f, num - opLabel2.size.y));
			num -= opLabel2.size.y + 10f;
		}
		tab.AddItems(opLabel, opLabel2);
		return num;
	}
}
