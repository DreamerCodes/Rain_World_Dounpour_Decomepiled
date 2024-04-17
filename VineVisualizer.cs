using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class VineVisualizer
{
	public Room room;

	public ClimbableVinesSystem vineSystem;

	public List<List<DebugSprite>> dbSprs;

	public VineVisualizer(Room room, ClimbableVinesSystem vineSystem)
	{
		this.room = room;
		this.vineSystem = vineSystem;
		dbSprs = new List<List<DebugSprite>>();
	}

	public void Update()
	{
		if (dbSprs.Count < vineSystem.vines.Count)
		{
			dbSprs.Add(new List<DebugSprite>());
		}
		else if (dbSprs.Count > vineSystem.vines.Count)
		{
			for (int i = 0; i < dbSprs[dbSprs.Count - 1].Count; i++)
			{
				dbSprs[dbSprs.Count - 1][i].Destroy();
			}
			dbSprs.RemoveAt(dbSprs.Count - 1);
		}
		for (int j = 0; j < dbSprs.Count; j++)
		{
			if (dbSprs[j].Count < vineSystem.vines[j].TotalPositions())
			{
				DebugSprite debugSprite = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel"), room);
				dbSprs[j].Add(debugSprite);
				room.AddObject(debugSprite);
				debugSprite.sprite.scaleX = 2f;
				debugSprite.sprite.color = new Color(1f, 0f, 0f);
				debugSprite.sprite.anchorY = 0f;
			}
			for (int k = 0; k < dbSprs[j].Count; k++)
			{
				Vector2 vector = vineSystem.vines[j].Pos(k);
				Vector2 vector2 = vineSystem.vines[j].Pos(Math.Min(k + 1, vineSystem.vines[j].TotalPositions() - 1));
				dbSprs[j][k].pos = vineSystem.vines[j].Pos(k);
				dbSprs[j][k].sprite.rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
				dbSprs[j][k].sprite.scaleY = Vector2.Distance(vector, vector2);
			}
		}
	}
}
