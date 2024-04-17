using System;
using RWCustom;

namespace MoreSlugcats;

public class ConsoleVisualizer
{
	public FLabel infoText;

	public ConsoleVisualizer()
	{
		infoText = new FLabel(Custom.GetFont(), string.Empty);
		Futile.stage.AddChild(infoText);
		infoText.x = 19.666666f;
		infoText.y = 199.66667f;
		infoText.alignment = FLabelAlignment.Left;
	}

	public void Visibility(bool visibility)
	{
		infoText.isVisible = visibility;
	}

	public void Update()
	{
		string text = "";
		for (int i = 0; i < RainWorld.recentConsoleLog.Length; i++)
		{
			text += RainWorld.recentConsoleLog[i];
			text += Environment.NewLine;
		}
		infoText.text = text;
	}
}
