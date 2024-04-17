using System.Linq;
using HUD;
using UnityEngine;

namespace Expedition;

public class EggSpot : UpdatableAndDeletable
{
	public Vector2 inRoomPos;

	public SlugcatStats.Name slugcat;

	public bool collected;

	public bool ping;

	public int activeCounter;

	public float colorCounter;

	public float scale;

	public float lastScale;

	public FadeCircle fadeCircle;

	public EggSpot(Vector2 pos, SlugcatStats.Name slug)
	{
		inRoomPos = pos;
		slugcat = slug;
		activeCounter = 0;
		colorCounter = 0f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		colorCounter += 1f;
		if (room == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room && ExpeditionData.slugcatPlayer == slugcat && Vector2.Distance(room.game.Players[i].realizedCreature.mainBodyChunk.pos, inRoomPos) < 20f)
			{
				flag = true;
			}
		}
		if (flag)
		{
			activeCounter++;
			if (activeCounter > 200 && !ping)
			{
				if (ExpeditionGame.ExIndex(slugcat) == -1)
				{
					return;
				}
				ExpeditionData.ints[ExpeditionGame.ExIndex(slugcat)] = 1;
				room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, inRoomPos);
				fadeCircle = new FadeCircle(room.game.cameras[0].hud, 20f, 30f, 0.94f, 60f, 4f, inRoomPos - room.game.cameras[0].pos, room.game.cameras[0].hud.fContainers[1]);
				fadeCircle.alphaMultiply = 0.5f;
				fadeCircle.fadeThickness = false;
				fadeCircle.circle.circleShader = room.game.rainWorld.Shaders["VectorCircle"];
				fadeCircle.circle.sprite.shader = fadeCircle.circle.circleShader;
				room.game.cameras[0].hud.fadeCircles.Add(fadeCircle);
				if (ExpeditionData.ints.Sum() >= 8 && ExpeditionGame.egg == null)
				{
					room.PlaySound(SoundID.HUD_Karma_Reinforce_Bump, inRoomPos);
					ExpeditionData.ints[ExpeditionGame.ExIndex(slugcat)] = 2;
					ExpeditionGame.egg = new Eggspedition(room.game);
				}
				ping = true;
			}
		}
		else
		{
			activeCounter = 0;
		}
		if (ping && fadeCircle != null && fadeCircle != null)
		{
			fadeCircle.circle.forceColor = new HSLColor(Mathf.Sin(colorCounter / 20f), 1f, 0.75f).rgb;
		}
	}
}
