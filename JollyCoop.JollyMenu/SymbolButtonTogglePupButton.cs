using Menu;
using UnityEngine;

namespace JollyCoop.JollyMenu;

public class SymbolButtonTogglePupButton : SymbolButtonToggle
{
	public MenuIllustration faceSymbol;

	public MenuIllustration uniqueSymbol;

	public bool HasUniqueSprite()
	{
		bool flag = symbolNameOff.Contains("spear") && !isToggled;
		return symbolNameOff.Contains("artificer") || symbolNameOff.Contains("rivulet") || flag;
	}

	public SymbolButtonTogglePupButton(global::Menu.Menu menu, MenuObject owner, string signal, Vector2 pos, Vector2 size, string symbolNameOn, string symbolNameOff, bool isOn, string stringLabelOn = null, string stringLabelOff = null)
		: base(menu, owner, signal, pos, size, symbolNameOn, symbolNameOff, isOn, textAboveButton: false, stringLabelOn, stringLabelOff)
	{
		faceSymbol = new MenuIllustration(menu, this, "", "face_" + (isOn ? symbolNameOn : symbolNameOff), size / 2f, crispPixels: true, anchorCenter: true);
		subObjects.Add(faceSymbol);
		if (HasUniqueSprite())
		{
			string fileName = "unique_" + symbolNameOff;
			uniqueSymbol = new MenuIllustration(menu, this, "", fileName, size / 2f, crispPixels: true, anchorCenter: true);
			subObjects.Add(uniqueSymbol);
		}
		LoadIcon();
	}

	public override void LoadIcon()
	{
		base.LoadIcon();
		if (faceSymbol != null)
		{
			faceSymbol.fileName = "face_" + symbol.fileName;
			faceSymbol.LoadFile();
			faceSymbol.sprite.SetElementByName(faceSymbol.fileName);
		}
		if (uniqueSymbol != null && HasUniqueSprite())
		{
			uniqueSymbol.fileName = "unique_" + symbolNameOff;
			uniqueSymbol.LoadFile();
			uniqueSymbol.sprite.SetElementByName(uniqueSymbol.fileName);
			if (symbol.fileName.Contains("on"))
			{
				uniqueSymbol.pos.y = size.y / 4f;
			}
			else
			{
				uniqueSymbol.pos.y = size.y / 2f;
			}
		}
	}

	public override void Toggle()
	{
		base.Toggle();
		faceSymbol.fileName = "face_" + symbol.fileName;
		LoadIcon();
	}

	public override void Update()
	{
		if (!HasUniqueSprite())
		{
			if (uniqueSymbol != null)
			{
				uniqueSymbol.RemoveSprites();
				subObjects.Remove(uniqueSymbol);
				uniqueSymbol = null;
			}
		}
		else if (uniqueSymbol == null)
		{
			uniqueSymbol = new MenuIllustration(menu, this, "", "unique_" + symbolNameOff, size / 2f, crispPixels: true, anchorCenter: true);
			subObjects.Add(uniqueSymbol);
			LoadIcon();
		}
		base.Update();
	}
}
