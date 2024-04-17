using System.Collections.Generic;
using System.Globalization;
using Menu;
using UnityEngine;

namespace JollyCoop.JollyMenu;

public class ColorChangeDialog : DialogNotify, Slider.ISliderOwner
{
	private class ColorSlider : PositionedMenuObject
	{
		public HorizontalSlider hueSlider;

		public HorizontalSlider satSlider;

		public HorizontalSlider litSlider;

		public Slider.SliderID Hue;

		public Slider.SliderID Sat;

		public Slider.SliderID Lit;

		private MenuLabel titleLabel;

		private readonly MenuIllustration colorDisplay;

		public RoundedRect colorBorder;

		public Color color;

		public HSLColor hslColor;

		public void RGB2HSL()
		{
			hslColor = JollyCustom.RGB2HSL(color);
		}

		public void HSL2RGB()
		{
			color = hslColor.rgb;
		}

		public ColorSlider(global::Menu.Menu menu, MenuObject owner, Vector2 pos, int playerNumber, int bodyPart, string sliderTitle)
			: base(menu, owner, pos)
		{
			Hue = new Slider.SliderID(bodyPart + "_JOLLY_HUE_" + playerNumber, register: true);
			Sat = new Slider.SliderID(bodyPart + "_JOLLY_SAT_" + playerNumber, register: true);
			Lit = new Slider.SliderID(bodyPart + "_JOLLY_LIT_" + playerNumber, register: true);
			float sliderWidth = GetSliderWidth(menu.CurrLang);
			if (hueSlider == null)
			{
				hueSlider = new HorizontalSlider(menu, this, menu.Translate("HUE"), pos, new Vector2(sliderWidth, 30f), Hue, subtleSlider: false);
				subObjects.Add(hueSlider);
			}
			if (satSlider == null)
			{
				satSlider = new HorizontalSlider(menu, this, menu.Translate("SAT"), pos + new Vector2(0f, -40f), new Vector2(sliderWidth, 30f), Sat, subtleSlider: false);
				subObjects.Add(satSlider);
			}
			if (litSlider == null)
			{
				litSlider = new HorizontalSlider(menu, this, menu.Translate("LIT"), pos + new Vector2(0f, -80f), new Vector2(sliderWidth, 30f), Lit, subtleSlider: false);
				subObjects.Add(litSlider);
			}
			colorBorder = new RoundedRect(menu, this, pos + new Vector2(40f, 40f), new Vector2(40f, 40f), filled: false);
			subObjects.Add(colorBorder);
			colorDisplay = new MenuIllustration(menu, this, "", "square", colorBorder.pos + new Vector2(2f, 2f), crispPixels: false, anchorCenter: false);
			subObjects.Add(colorDisplay);
			titleLabel = new MenuLabel(menu, this, menu.Translate(sliderTitle), colorBorder.pos + new Vector2(45f, 2.5f), new Vector2(100f, 40f), bigText: true);
			titleLabel.label.alignment = FLabelAlignment.Left;
			subObjects.Add(titleLabel);
		}

		private static float GetSliderWidth(InGameTranslator.LanguageID lang)
		{
			float result = 200f;
			if (lang == InGameTranslator.LanguageID.Japanese || lang == InGameTranslator.LanguageID.French || lang == InGameTranslator.LanguageID.Chinese)
			{
				result = 180f;
			}
			else if (lang == InGameTranslator.LanguageID.German || lang == InGameTranslator.LanguageID.Russian)
			{
				result = 160f;
			}
			return result;
		}

		public override void RemoveSprites()
		{
			base.RemoveSprites();
			(menu as ColorChangeDialog).jollyDialog.slidingMenu.SetPortraitsDirty();
			if (Hue != null)
			{
				Hue.Unregister();
				Hue = null;
			}
			if (Sat != null)
			{
				Sat.Unregister();
				Sat = null;
			}
			if (Lit != null)
			{
				Lit.Unregister();
				Lit = null;
			}
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			colorDisplay.color = color;
		}
	}

	private readonly ColorSlider body;

	private readonly ColorSlider face;

	private readonly ColorSlider unique;

	private readonly SimpleButton resetButton;

	private readonly SlugcatStats.Name playerClass;

	private readonly int playerNumber;

	public JollySetupDialog jollyDialog;

	private static ColorChangeDialog instance;

	private JollyPlayerOptions JollyOptions => jollyDialog.JollyOptions(playerNumber);

	private static void SaveColorChange()
	{
		instance.jollyDialog.PlaySound(SoundID.MENU_Remove_Level);
		instance.ActualSavingColor();
	}

	private void ActualSavingColor()
	{
		body.HSL2RGB();
		JollyOptions.SetBodyColor(body.color);
		face.HSL2RGB();
		JollyOptions.SetFaceColor(face.color);
		if (unique != null)
		{
			unique.HSL2RGB();
			JollyOptions.SetUniqueColor(unique.color);
			JollyCustom.Log($"Changed Player {playerNumber} color: {body.color} / {face.color} / {unique.color}");
		}
		else
		{
			JollyCustom.Log($"Changed Player {playerNumber} color: {body.color} / {face.color}");
		}
	}

	public ColorChangeDialog(JollySetupDialog jollyDialog, SlugcatStats.Name playerClass, int playerNumber, ProcessManager manager, List<string> names)
		: base("", jollyDialog.Translate("COLOR CONFIGURATION"), new Vector2((names.Count > 2) ? 885f : 650f, 307.2f), manager, SaveColorChange)
	{
		instance = this;
		this.jollyDialog = jollyDialog;
		this.playerClass = playerClass;
		this.playerNumber = playerNumber;
		Vector2 vector = new Vector2((names.Count <= 2) ? 205f : 135f, 190f);
		resetButton = new SimpleButton(this, pages[0], jollyDialog.Translate("RESET"), $"RESET_COLOR_DIALOG_{this.playerNumber}", vector + new Vector2(180f, 291.84f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(resetButton);
		AddSlider(ref body, jollyDialog.Translate(names[0]), vector, this.playerNumber, 0);
		body.color = JollyOptions.GetBodyColor();
		body.RGB2HSL();
		okButton.nextSelectable[0] = body.litSlider;
		AddSlider(ref face, jollyDialog.Translate(names[1]), vector + new Vector2(140f, 0f), this.playerNumber, 1);
		face.color = JollyOptions.GetFaceColor();
		face.RGB2HSL();
		okButton.nextSelectable[1] = face.litSlider;
		okButton.nextSelectable[2] = face.litSlider;
		if (names.Count > 2)
		{
			AddSlider(ref unique, jollyDialog.Translate(names[2]), vector + new Vector2(280f, 0f), this.playerNumber, 2);
			unique.color = JollyOptions.GetUniqueColor();
			unique.RGB2HSL();
			okButton.nextSelectable[2] = unique.litSlider;
		}
		resetButton.nextSelectable[3] = body.hueSlider;
		okButton.nextSelectable[3] = okButton;
		resetButton.nextSelectable[1] = resetButton;
		try
		{
			Update();
			GrafUpdate(0f);
		}
		catch
		{
		}
	}

	private void AddSlider(ref ColorSlider slider, string labelString, Vector2 position, int playerNumber, int bodyPart)
	{
		slider = new ColorSlider(this, pages[0], position, playerNumber, bodyPart, labelString);
		pages[0].subObjects.Add(slider);
		MutualVerticalButtonBind(slider.satSlider, slider.hueSlider);
		MutualVerticalButtonBind(slider.litSlider, slider.satSlider);
		slider.hueSlider.nextSelectable[1] = resetButton;
		slider.litSlider.nextSelectable[3] = okButton;
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		if (message.StartsWith("RESET_COLOR_DIALOG_"))
		{
			PlaySound(SoundID.MENU_Remove_Level);
			JollyCustom.Log("Resetting color for Player " + int.Parse(message.Split('_')[3], NumberFormatInfo.InvariantInfo));
			JollyOptions.SetColorsToDefault(playerClass);
			body.color = JollyOptions.GetBodyColor();
			body.RGB2HSL();
			face.color = JollyOptions.GetFaceColor();
			face.RGB2HSL();
			if (unique != null)
			{
				unique.color = JollyOptions.GetUniqueColor();
				unique.RGB2HSL();
			}
		}
	}

	public override void SliderSetValue(Slider slider, float f)
	{
		if (slider.ID.value.Contains("JOLLY"))
		{
			string[] array = slider.ID.value.Split('_');
			if (int.TryParse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
			{
				int bodyPart2 = array[2] switch
				{
					"SAT" => 1, 
					"LIT" => 2, 
					_ => 0, 
				};
				ColorSlider colorSlider = result switch
				{
					1 => face, 
					2 => unique, 
					_ => body, 
				};
				AssignCorrectColorDimension(f, ref colorSlider.hslColor, bodyPart2);
				colorSlider.HSL2RGB();
				selectedObject = slider;
			}
		}
		static void AssignCorrectColorDimension(float f, ref HSLColor colorHSL, int bodyPart)
		{
			switch (bodyPart)
			{
			default:
				colorHSL.hue = Mathf.Clamp(f, 0f, 0.99f);
				break;
			case 1:
				colorHSL.saturation = Mathf.Clamp(f, 0f, 1f);
				break;
			case 2:
				colorHSL.lightness = Mathf.Clamp(f, 0.01f, 1f);
				break;
			}
		}
	}

	public override float ValueOfSlider(Slider slider)
	{
		if (!slider.ID.value.Contains("JOLLY"))
		{
			return 0f;
		}
		string[] array = slider.ID.value.Split('_');
		if (!int.TryParse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
		{
			return 0f;
		}
		int bodyPart2 = array[2] switch
		{
			"SAT" => 1, 
			"LIT" => 2, 
			_ => 0, 
		};
		return GetCorrectColorDimension((result switch
		{
			1 => face, 
			2 => unique, 
			_ => body, 
		}).hslColor, bodyPart2);
		static float GetCorrectColorDimension(HSLColor colorHSL, int bodyPart)
		{
			return bodyPart switch
			{
				1 => Mathf.Clamp(colorHSL.saturation, 0f, 1f), 
				2 => Mathf.Clamp(colorHSL.lightness, 0.01f, 1f), 
				_ => Mathf.Clamp(colorHSL.hue, 0f, 0.99f), 
			};
		}
	}
}
