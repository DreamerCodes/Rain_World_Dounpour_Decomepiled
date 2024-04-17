using System;
using System.Collections.Generic;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpResourceList : OpListBox
{
	public class QueueEnum : ConfigQueue
	{
		protected readonly ushort lineCount;

		protected readonly bool downward;

		protected override float sizeY => 20f * (float)(int)lineCount + 34f;

		public QueueEnum(ConfigurableBase configEnum, ushort lineCount = 5, bool downward = true, object sign = null)
			: base(configEnum, sign)
		{
			if (ValueConverter.GetTypeCategory(configEnum.settingType) != ValueConverter.TypeCategory.Enum && ValueConverter.GetTypeCategory(configEnum.settingType) != ValueConverter.TypeCategory.ExtEnum)
			{
				throw new ArgumentException("settingType of Configurable<T> must be either Enum or ExtEnum.");
			}
			this.lineCount = lineCount;
			this.downward = downward;
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, 150f, 50f);
			List<UIelement> list = new List<UIelement>();
			OpResourceList opResourceList = new OpResourceList(config, new Vector2(posX, posY), width, lineCount, downward)
			{
				sign = sign
			};
			if (onChange != null)
			{
				opResourceList.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opResourceList.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opResourceList.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opResourceList.OnValueChanged += onValueChanged;
			}
			mainFocusable = opResourceList;
			list.Add(opResourceList);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opResourceList.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (!string.IsNullOrEmpty(config.key))
			{
				OpLabel item = new OpLabel(new Vector2(posX + width + 10f, posY + sizeY - 30f), new Vector2(holder.CanvasSize.x - posX - width - 10f, 30f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opResourceList.bumpBehav,
					description = opResourceList.description
				};
				list.Add(item);
			}
			opResourceList.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	public class QueueSpecial : ConfigQueue
	{
		protected readonly OpResourceSelector.SpecialEnum listType;

		protected readonly ushort lineCount;

		protected readonly bool downward;

		protected override float sizeY => 20f * (float)(int)lineCount + 34f;

		public QueueSpecial(Configurable<string> config, OpResourceSelector.SpecialEnum listType, ushort lineCount = 5, bool downward = true, object sign = null)
			: base(config, sign)
		{
			if (listType == OpResourceSelector.SpecialEnum.Enum)
			{
				throw new ArgumentException("listType cannot be Enum with this. Use QueueEnum.");
			}
			this.listType = listType;
			this.lineCount = lineCount;
			this.downward = downward;
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, 150f, 50f);
			List<UIelement> list = new List<UIelement>();
			OpResourceList opResourceList = new OpResourceList(config as Configurable<string>, new Vector2(posX, posY), width, listType, lineCount, downward)
			{
				sign = sign
			};
			if (onChange != null)
			{
				opResourceList.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opResourceList.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opResourceList.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opResourceList.OnValueChanged += onValueChanged;
			}
			mainFocusable = opResourceList;
			list.Add(opResourceList);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opResourceList.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (!string.IsNullOrEmpty(config.key))
			{
				OpLabel item = new OpLabel(new Vector2(posX + width + 10f, posY + sizeY - 30f), new Vector2(holder.CanvasSize.x - posX - width - 10f, 30f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opResourceList.bumpBehav,
					description = opResourceList.description
				};
				list.Add(item);
			}
			opResourceList.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	public readonly OpResourceSelector.SpecialEnum listType;

	public OpResourceList(ConfigurableBase config, Vector2 pos, float width, ushort lineCount = 5, bool downward = true)
		: base(config, pos, width, null, lineCount, downward)
	{
		listType = OpResourceSelector.SpecialEnum.Enum;
		_itemList = OpResourceSelector.GetEnumNames(this, config.settingType);
		_ResetIndex();
		_Initialize(base.defaultValue);
		_OpenList();
	}

	public OpResourceList(Configurable<string> config, Vector2 pos, float width, OpResourceSelector.SpecialEnum listType, ushort lineCount = 5, bool downward = true)
		: base(config, pos, width, (List<ListItem>)null, lineCount, downward)
	{
		_itemList = OpResourceSelector.GetResources(this, listType);
		_ResetIndex();
		_Initialize(base.defaultValue);
		_OpenList();
	}
}
