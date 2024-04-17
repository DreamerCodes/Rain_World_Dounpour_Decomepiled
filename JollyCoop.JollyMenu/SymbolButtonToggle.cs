using System;
using System.Linq;
using Menu;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace JollyCoop.JollyMenu;

public class SymbolButtonToggle : SimpleButton
{
	public MenuIllustration symbol;

	public MenuLabel belowLabel;

	private float labelFade;

	private float lastLabelFade;

	public int labelFadeCounter;

	public string symbolNameOn;

	public string symbolNameOff;

	private string labelNameOn;

	private string labelNameOff;

	internal bool isToggled;

	public SymbolButtonToggle(global::Menu.Menu menu, MenuObject owner, string signal, Vector2 pos, Vector2 size, string symbolNameOn, string symbolNameOff, bool isOn, bool textAboveButton, string stringLabelOn = null, string stringLabelOff = null, FTextParams textParams = null)
		: base(menu, owner, "", signal, pos, size)
	{
		this.symbolNameOn = symbolNameOn;
		this.symbolNameOff = symbolNameOff;
		signalText = signal;
		if (stringLabelOn != null)
		{
			int num = Math.Max(stringLabelOn?.Count((char f) => f == '\n') ?? 0, stringLabelOff?.Count((char f) => f == '\n') ?? 0);
			float num2 = LabelTest.LineHeight(bigText: false);
			if (textParams != null)
			{
				num2 = Mathf.Max(0f, num2 + textParams.lineHeightOffset);
			}
			if (textAboveButton)
			{
				belowLabel = new MenuLabel(menu, this, isOn ? stringLabelOn : stringLabelOff, new Vector2(size.x / 2f - 60f, size.y + num2 * (float)num / 2f), new Vector2(120f, 30f), bigText: false, textParams);
			}
			else
			{
				belowLabel = new MenuLabel(menu, this, isOn ? stringLabelOn : stringLabelOff, new Vector2(size.x / 2f - 60f, -25f - num2 * (float)num / 2f), new Vector2(120f, 30f), bigText: false, textParams);
			}
			belowLabel.label.alignment = FLabelAlignment.Center;
			subObjects.Add(belowLabel);
			if (stringLabelOff == null)
			{
				throw new Exception("Wrong usage of SymbolButtonToggle, either both or none labels must be specified");
			}
			labelNameOn = stringLabelOn;
			labelNameOff = stringLabelOff;
		}
		symbol = new MenuIllustration(menu, this, "", isOn ? symbolNameOn : symbolNameOff, size / 2f, crispPixels: true, anchorCenter: true);
		subObjects.Add(symbol);
		isToggled = isOn;
		if (isToggled)
		{
			signalText += "_off";
		}
		else
		{
			signalText += "_on";
		}
		LoadIcon();
	}

	public virtual void LoadIcon()
	{
		string text = symbolNameOff;
		if (isToggled)
		{
			text = symbolNameOn;
		}
		symbol.fileName = text;
		symbol.LoadFile();
		symbol.sprite.SetElementByName(text);
	}

	public override void Update()
	{
		base.Update();
		if ((signalText.Contains("on") && symbol.fileName == symbolNameOn) || (signalText.Contains("off") && symbol.fileName == symbolNameOff))
		{
			Toggle();
		}
	}

	public override void Clicked()
	{
		base.Clicked();
		Toggle();
	}

	public virtual void Toggle()
	{
		if (symbol.fileName == symbolNameOn)
		{
			symbol.fileName = symbolNameOff;
			signalText = signalText.Replace("off", "on");
			isToggled = false;
			if (belowLabel != null)
			{
				belowLabel.label.text = labelNameOff;
			}
		}
		else
		{
			symbol.fileName = symbolNameOn;
			signalText = signalText.Replace("on", "off");
			isToggled = true;
			if (belowLabel != null)
			{
				belowLabel.label.text = labelNameOn;
			}
		}
		LoadIcon();
	}
}
