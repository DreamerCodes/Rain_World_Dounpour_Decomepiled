using System.Collections.Generic;
using System.Globalization;
using RWCustom;
using UnityEngine;

public abstract class IconSymbol
{
	public struct IconSymbolData
	{
		public CreatureTemplate.Type critType;

		public int intData;

		public AbstractPhysicalObject.AbstractObjectType itemType;

		public IconSymbolData(CreatureTemplate.Type critType, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
		{
			this.critType = critType;
			this.itemType = itemType;
			this.intData = intData;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}", critType.value, itemType.value, intData);
		}

		public static IconSymbolData IconSymbolDataFromString(string s)
		{
			string[] array = s.Split('-');
			string text = null;
			string text2 = null;
			int result = -1;
			int result2 = -1;
			int result3 = -1;
			if (int.TryParse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				List<CreatureTemplate.Type> list = new List<CreatureTemplate.Type>();
				BackwardsCompatibilityRemix.ParseCreatureTypes(array[0], list);
				text = ((list.Count != 0) ? list[0].value : CreatureTemplate.Type.StandardGroundCreature.value);
			}
			else
			{
				text = array[0];
			}
			if (int.TryParse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture, out result2))
			{
				List<AbstractPhysicalObject.AbstractObjectType> list2 = new List<AbstractPhysicalObject.AbstractObjectType>();
				BackwardsCompatibilityRemix.ParseItemTypes(array[1], list2);
				text2 = ((list2.Count != 0) ? list2[0].value : AbstractPhysicalObject.AbstractObjectType.Creature.value);
			}
			else
			{
				text2 = array[1];
			}
			int.TryParse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture, out result3);
			return new IconSymbolData((text == null) ? null : new CreatureTemplate.Type(text), (text2 == null) ? null : new AbstractPhysicalObject.AbstractObjectType(text2), result3);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is IconSymbolData))
			{
				return false;
			}
			return Equals((IconSymbolData)obj);
		}

		public bool Equals(IconSymbolData data)
		{
			if (critType == data.critType && intData == data.intData)
			{
				return itemType == data.itemType;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(IconSymbolData a, IconSymbolData b)
		{
			if (a.critType == b.critType && a.intData == b.intData)
			{
				return a.itemType == b.itemType;
			}
			return false;
		}

		public static bool operator !=(IconSymbolData a, IconSymbolData b)
		{
			if (!(a.critType != b.critType) && a.intData == b.intData)
			{
				return a.itemType != b.itemType;
			}
			return true;
		}
	}

	public IconSymbolData iconData;

	public FSprite symbolSprite;

	public FSprite shadowSprite1;

	public FSprite shadowSprite2;

	public Color myColor;

	public float graphWidth;

	private FContainer container;

	public float showFlash;

	public float lastShowFlash;

	public string spriteName;

	public IconSymbol(IconSymbolData iconData, FContainer container)
	{
		this.iconData = iconData;
		this.container = container;
	}

	public void Update()
	{
		lastShowFlash = showFlash;
		showFlash = Custom.LerpAndTick(showFlash, 0f, 0.08f, 0.1f);
	}

	public void Show(bool showShadowSprites)
	{
		if (showShadowSprites)
		{
			shadowSprite1 = new FSprite(spriteName);
			container.AddChild(shadowSprite1);
			shadowSprite1.color = new Color(0f, 0f, 0f);
			shadowSprite2 = new FSprite(spriteName);
			container.AddChild(shadowSprite2);
			shadowSprite2.color = new Color(0f, 0f, 0f);
		}
		symbolSprite = new FSprite(spriteName);
		container.AddChild(symbolSprite);
		showFlash = 1f;
		lastShowFlash = 1f;
	}

	public void Draw(float timeStacker, Vector2 drawPos)
	{
		float f = Mathf.Lerp(lastShowFlash, showFlash, timeStacker);
		if (symbolSprite != null)
		{
			symbolSprite.color = Color.Lerp(myColor, new Color(1f, 1f, 1f), Mathf.Pow(f, 3f));
			symbolSprite.x = drawPos.x;
			symbolSprite.y = drawPos.y;
		}
		if (shadowSprite1 != null)
		{
			shadowSprite1.x = drawPos.x - 2f;
			shadowSprite1.y = drawPos.y - 1f;
			shadowSprite2.x = drawPos.x - 1f;
			shadowSprite2.y = drawPos.y + 1f;
		}
	}

	public void RemoveSprites()
	{
		if (symbolSprite != null)
		{
			symbolSprite.RemoveFromContainer();
		}
		if (shadowSprite1 != null)
		{
			shadowSprite1.RemoveFromContainer();
		}
		if (shadowSprite2 != null)
		{
			shadowSprite2.RemoveFromContainer();
		}
	}

	public static IconSymbol CreateIconSymbol(IconSymbolData iconData, FContainer container)
	{
		if (iconData.itemType == AbstractPhysicalObject.AbstractObjectType.Creature)
		{
			return new CreatureSymbol(iconData, container);
		}
		return new ItemSymbol(iconData, container);
	}
}
