using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpResourceSelector : OpComboBox
{
	public enum SpecialEnum : byte
	{
		Enum,
		Regions,
		Decals,
		Illustrations,
		Palettes,
		Shaders,
		SlugcatNames
	}

	public class QueueEnum : ConfigQueue
	{
		public OnSignalHandler onListOpen;

		public OnSignalHandler onListClose;

		protected override float sizeY => 30f;

		public QueueEnum(ConfigurableBase configEnum, object sign = null)
			: base(configEnum, sign)
		{
			if (ValueConverter.GetTypeCategory(configEnum.settingType) != ValueConverter.TypeCategory.Enum && ValueConverter.GetTypeCategory(configEnum.settingType) != ValueConverter.TypeCategory.ExtEnum)
			{
				throw new ArgumentException("settingType of Configurable<T> must be either Enum or ExtEnum.");
			}
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, 150f, 50f);
			List<UIelement> list = new List<UIelement>();
			OpResourceSelector opResourceSelector = new OpResourceSelector(config, new Vector2(posX, posY), width)
			{
				sign = sign
			};
			if (onChange != null)
			{
				opResourceSelector.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opResourceSelector.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opResourceSelector.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opResourceSelector.OnValueChanged += onValueChanged;
			}
			if (onListOpen != null)
			{
				opResourceSelector.OnListOpen += onListOpen;
			}
			if (onListClose != null)
			{
				opResourceSelector.OnListClose += onListClose;
			}
			mainFocusable = opResourceSelector;
			list.Add(opResourceSelector);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opResourceSelector.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (!string.IsNullOrEmpty(config.key))
			{
				OpLabel item = new OpLabel(new Vector2(posX + width + 10f, posY), new Vector2(holder.CanvasSize.x - posX - width - 10f, 30f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opResourceSelector.bumpBehav,
					description = opResourceSelector.description
				};
				list.Add(item);
			}
			opResourceSelector.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	public class QueueSpecial : ConfigQueue
	{
		protected readonly SpecialEnum listType;

		public OnSignalHandler onListOpen;

		public OnSignalHandler onListClose;

		protected override float sizeY => 30f;

		public QueueSpecial(Configurable<string> config, SpecialEnum listType, object sign = null)
			: base(config, sign)
		{
			if (listType == SpecialEnum.Enum)
			{
				throw new ArgumentException("listType cannot be Enum with this. Use QueueEnum.");
			}
			this.listType = listType;
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, 150f, 50f);
			List<UIelement> list = new List<UIelement>();
			OpResourceSelector opResourceSelector = new OpResourceSelector(config as Configurable<string>, new Vector2(posX, posY), width, listType)
			{
				sign = sign
			};
			if (onChange != null)
			{
				opResourceSelector.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opResourceSelector.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opResourceSelector.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opResourceSelector.OnValueChanged += onValueChanged;
			}
			if (onListOpen != null)
			{
				opResourceSelector.OnListOpen += onListOpen;
			}
			if (onListClose != null)
			{
				opResourceSelector.OnListClose += onListClose;
			}
			mainFocusable = opResourceSelector;
			list.Add(opResourceSelector);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opResourceSelector.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (!string.IsNullOrEmpty(config.key))
			{
				OpLabel item = new OpLabel(new Vector2(posX + width + 10f, posY), new Vector2(holder.CanvasSize.x - posX - width - 10f, 30f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opResourceSelector.bumpBehav,
					description = opResourceSelector.description
				};
				list.Add(item);
			}
			opResourceSelector.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	public readonly SpecialEnum listType;

	public OpResourceSelector(ConfigurableBase config, Vector2 pos, float width)
		: base(config, pos, width, null)
	{
		listType = SpecialEnum.Enum;
		_itemList = GetEnumNames(this, config.settingType);
		_ResetIndex();
		_Initialize(base.defaultValue);
	}

	public OpResourceSelector(Configurable<string> config, Vector2 pos, float width, SpecialEnum listType)
		: base(config, pos, width, (List<ListItem>)null)
	{
		_itemList = GetResources(this, listType);
		_ResetIndex();
		_Initialize(base.defaultValue);
	}

	public static ListItem[] GetEnumNames(UIconfig caller, Type enumType)
	{
		List<ListItem> list = new List<ListItem>();
		if (enumType.IsEnum)
		{
			string[] names = Enum.GetNames(enumType);
			for (int i = 0; i < names.Length; i++)
			{
				Enum @enum = (Enum)Enum.Parse(enumType, names[i], ignoreCase: true);
				ListItem item = new ListItem(names[i], (int)(object)@enum);
				item.displayName = EnumHelper.GetEnumDesc(@enum) ?? item.name;
				list.Add(item);
			}
		}
		else
		{
			if (!enumType.IsExtEnum())
			{
				throw new ElementFormatException(caller, "config.settingType is neither Enum or ExtEnum!", caller.Key);
			}
			string[] names2 = ExtEnumBase.GetNames(enumType);
			for (int j = 0; j < names2.Length; j++)
			{
				ListItem listItem = new ListItem(names2[j], ((ExtEnumBase)ExtEnumBase.Parse(enumType, names2[j], ignoreCase: false)).Index);
				listItem.displayName = names2[j];
				ListItem item2 = listItem;
				list.Add(item2);
			}
		}
		list.Sort(ListItem.Comparer);
		return list.ToArray();
	}

	public static ListItem[] GetResources(UIconfig caller, SpecialEnum listType)
	{
		List<ListItem> list = new List<ListItem>();
		switch (listType)
		{
		case SpecialEnum.Enum:
			throw new ElementFormatException(caller, "Do NOT use SpecialEnum.Enum. That's for another ctor.", caller.Key);
		case SpecialEnum.Shaders:
		{
			string[] array3 = Custom.rainWorld.Shaders.Keys.ToArray();
			foreach (string name2 in array3)
			{
				list.Add(new ListItem(name2));
			}
			break;
		}
		case SpecialEnum.Palettes:
		{
			string[] array3 = AssetManager.ListDirectory("Palettes" + Path.DirectorySeparatorChar);
			for (int j = 0; j < array3.Length; j++)
			{
				FileInfo fileInfo2 = new FileInfo(array3[j]);
				if (fileInfo2.Exists && (fileInfo2.Name.ToLower().EndsWith(".png") || fileInfo2.Name.ToLower().EndsWith(".jpg")) && fileInfo2.Name.ToLower().StartsWith("palette"))
				{
					string text = fileInfo2.Name.Remove(fileInfo2.Name.Length - 4);
					int result = (int.TryParse(text.Remove(0, 7), NumberStyles.Any, CultureInfo.InvariantCulture, out result) ? result : int.MaxValue);
					list.Add(new ListItem(text.Substring("palette".Length), result)
					{
						displayName = text
					});
				}
			}
			break;
		}
		case SpecialEnum.Illustrations:
		{
			string[] array5 = AssetManager.ListDirectory("Illustrations" + Path.DirectorySeparatorChar);
			List<string> list3 = new List<string>();
			string[] array3 = array5;
			for (int j = 0; j < array3.Length; j++)
			{
				FileInfo fileInfo3 = new FileInfo(array3[j]);
				if (fileInfo3.Exists && (fileInfo3.Name.ToLower().EndsWith(".png") || fileInfo3.Name.ToLower().EndsWith(".jpg")))
				{
					list3.Add(fileInfo3.Name.Remove(fileInfo3.Name.Length - 4));
				}
			}
			list3.Sort();
			for (int m = 0; m < list3.Count; m++)
			{
				list.Add(new ListItem(list3[m], m));
			}
			break;
		}
		case SpecialEnum.Decals:
		{
			string[] array2 = AssetManager.ListDirectory("Decals" + Path.DirectorySeparatorChar);
			List<string> list2 = new List<string>();
			string[] array3 = array2;
			for (int j = 0; j < array3.Length; j++)
			{
				FileInfo fileInfo = new FileInfo(array3[j]);
				if (fileInfo.Exists && (fileInfo.Name.ToLower().EndsWith(".png") || fileInfo.Name.ToLower().EndsWith(".jpg")))
				{
					list2.Add(fileInfo.Name.Remove(fileInfo.Name.Length - 4));
				}
			}
			list2.Sort();
			for (int k = 0; k < list2.Count; k++)
			{
				list.Add(new ListItem(list2[k], k));
			}
			break;
		}
		case SpecialEnum.Regions:
		{
			string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "regions.txt");
			string[] array4 = ((!File.Exists(path)) ? new string[1] { "No Regions" } : File.ReadAllLines(path));
			for (int l = 0; l < array4.Length; l++)
			{
				if (array4[l].Length > 0)
				{
					list.Add(new ListItem(array4[l], l)
					{
						displayName = Custom.rainWorld.inGameTranslator.Translate(Region.GetRegionFullName(array4[l], SlugcatStats.Name.White))
					});
				}
			}
			break;
		}
		case SpecialEnum.SlugcatNames:
		{
			string[] array = ExtEnum<SlugcatStats.Name>.values.entries.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				SlugcatStats.Name name = new SlugcatStats.Name(array[i]);
				if (!SlugcatStats.HiddenOrUnplayableSlugcat(name))
				{
					ListItem listItem = new ListItem(array[i], name.Index);
					listItem.displayName = Custom.rainWorld.inGameTranslator.Translate(SlugcatStats.getSlugcatName(name));
					ListItem item = listItem;
					list.Add(item);
				}
			}
			break;
		}
		}
		list.Sort(ListItem.Comparer);
		return list.ToArray();
	}
}
