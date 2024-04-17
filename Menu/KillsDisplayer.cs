using System.Collections.Generic;
using Menu.Remix;
using UnityEngine;

namespace Menu;

public class KillsDisplayer : PositionedMenuObject
{
	public List<KeyValuePair<IconSymbol.IconSymbolData, int>> kills;

	public KillsDisplayer(Menu menu, MenuObject owner, Vector2 pos, List<KeyValuePair<IconSymbol.IconSymbolData, int>> kills)
		: base(menu, owner, pos)
	{
		this.kills = kills;
		base.pos = pos;
		int num = 0;
		int num2 = 0;
		if (kills == null || kills.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < kills.Count; i++)
		{
			MenuLabel menuLabel = new MenuLabel(menu, this, ValueConverter.ConvertToString(kills[i].Value), new Vector2(45f + 80f * (float)num, 200f - 35f * (float)num2), default(Vector2), bigText: true)
			{
				label = 
				{
					color = CreatureSymbol.ColorOfCreature(kills[i].Key),
					alignment = FLabelAlignment.Left
				}
			};
			subObjects.Add(menuLabel);
			FSprite node = new FSprite(CreatureSymbol.SpriteNameOfCreature(kills[i].Key))
			{
				color = CreatureSymbol.ColorOfCreature(kills[i].Key),
				x = base.pos.x + menuLabel.pos.x - 30f,
				y = base.pos.y + menuLabel.pos.y
			};
			Container.AddChild(node);
			num++;
			if (num == 7)
			{
				num = 0;
				num2++;
			}
		}
	}
}
