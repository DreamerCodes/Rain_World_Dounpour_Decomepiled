using System.Text.RegularExpressions;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace JollyCoop.JollyManual;

public class JollyManualPage : ManualPage
{
	public FSprite headingSeparator;

	public static readonly int rectHeight = 550;

	public static readonly int rectWidth = 600;

	public Vector2 belowHeaderPos;

	public Vector2 belowHeaderPosCentered;

	public float textWidth;

	public float textLeftMargin;

	public float spaceBuffer;

	public Vector2 verticalBuffer;

	public JollyManualPage(global::Menu.Menu menu, MenuObject owner)
		: base(menu, owner)
	{
		topicName = menu.Translate((menu as JollyManualDialog).TopicName((menu as JollyManualDialog).currentTopic));
		MenuLabel item = new MenuLabel(menu, owner, topicName, new Vector2(15f + (menu as JollyManualDialog).contentOffX, 475f), default(Vector2), bigText: true)
		{
			label = 
			{
				alignment = FLabelAlignment.Left
			}
		};
		subObjects.Add(item);
		headingSeparator = new FSprite("pixel");
		headingSeparator.scaleX = 594f;
		headingSeparator.scaleY = 2f;
		headingSeparator.color = new Color(0.7f, 0.7f, 0.7f);
		Container.AddChild(headingSeparator);
		belowHeaderPos = new Vector2(-2f + (menu as JollyManualDialog).contentOffX, 448f);
		belowHeaderPosCentered = new Vector2((float)rectWidth / 2f + (menu as JollyManualDialog).contentOffX, 448f);
		textWidth = (float)rectWidth * 0.92f;
		textLeftMargin = ((float)rectWidth - textWidth) / 2f + (menu as JollyManualDialog).contentOffX;
		spaceBuffer = 40f;
		if (menu.CurrLang == InGameTranslator.LanguageID.Spanish)
		{
			spaceBuffer *= 0.9f;
		}
		verticalBuffer = new Vector2(0f, spaceBuffer);
	}

	public float AddManualText(string text, float startY, bool bigText = true, bool centered = true, float? customTextWidth = null)
	{
		float num = customTextWidth ?? textWidth;
		float num2 = ((float)rectWidth - num) / 2f;
		float num3 = 0f;
		string[] array = Regex.Split(text.WrapText(bigText, num), "\n");
		for (int i = 0; i < array.Length; i++)
		{
			num3 = startY - 25f * (float)i;
			MenuLabel menuLabel = new MenuLabel(menu, owner, array[i], new Vector2(centered ? (num2 + num * 0.5f) : num2, num3), default(Vector2), bigText);
			menuLabel.label.SetAnchor(centered ? 0.5f : 0f, 0.5f);
			menuLabel.label.color = new Color(0.7f, 0.7f, 0.7f);
			subObjects.Add(menuLabel);
		}
		return num3 - 5f - LabelTest.LineHeight(bigText);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		headingSeparator.x = base.page.pos.x + 295f + (menu as JollyManualDialog).contentOffX;
		headingSeparator.y = base.page.pos.y + 450f;
	}

	public string TranslateManual(string oldString)
	{
		string text = menu.Translate(oldString);
		string buttonName_Map = OptionalText.GetButtonName_Map();
		string text2 = text.Replace("<MAP>", "[" + buttonName_Map + "]");
		string buttonName_PickUp = OptionalText.GetButtonName_PickUp();
		return text2.Replace("<PICK UP>", "[" + buttonName_PickUp + "]");
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		headingSeparator.RemoveFromContainer();
	}
}
