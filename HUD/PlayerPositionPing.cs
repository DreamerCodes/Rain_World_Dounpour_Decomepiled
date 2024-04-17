using System;
using RWCustom;
using UnityEngine;

namespace HUD;

public class PlayerPositionPing : HudPart
{
	public HUDCircle circle;

	public Vector2 pos;

	public Vector2 lastPos;

	public RoomCamera cam;

	public int newToRoom;

	public int lastRoom = -1;

	public float ping;

	public int pingDelay;

	public PlayerPositionPing(HUD hud, FContainer fContainer, RoomCamera cam)
		: base(hud)
	{
		this.cam = cam;
		circle = new HUDCircle(hud, HUDCircle.SnapToGraphic.None, fContainer, 0);
		circle.sprite.isVisible = false;
		circle.fade = 0f;
		circle.lastFade = 0f;
		circle.pos = new Vector2(-1000f, -1000f);
		circle.lastPos = circle.pos;
	}

	public override void Update()
	{
		lastPos = pos;
		if (cam.followAbstractCreature.realizedCreature != null && cam.followAbstractCreature.realizedCreature.room == cam.room)
		{
			pos = Vector2.Lerp(cam.followAbstractCreature.realizedCreature.mainBodyChunk.pos, cam.followAbstractCreature.realizedCreature.bodyChunks[1].pos, 0.3f) - cam.pos;
			if (newToRoom > 0 && pingDelay < 1)
			{
				bool flag = false;
				bool flag2 = false;
				IntVector2 tilePosition = cam.followAbstractCreature.realizedCreature.room.GetTilePosition(cam.followAbstractCreature.realizedCreature.bodyChunks[1].pos);
				int num = 0;
				while (!flag2 && num < 9)
				{
					if (cam.followAbstractCreature.realizedCreature.room.GetTile(tilePosition + Custom.eightDirectionsAndZero[num]).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
					{
						flag2 = true;
						IntVector2 intVector = cam.followAbstractCreature.realizedCreature.room.ShorcutEntranceHoleDirection(tilePosition + Custom.eightDirectionsAndZero[num]);
						IntVector2 intVec = RWInput.PlayerInput((cam.followAbstractCreature.realizedCreature as Player).playerState.playerNumber).IntVec;
						if ((intVec.x != intVector.x || intVector.x == 0) && (intVec.y != intVector.y || intVector.y == 0))
						{
							flag = true;
						}
						else
						{
							pingDelay = Mathf.Max(pingDelay, 60);
						}
					}
					num++;
				}
				if (flag && ping == 0f)
				{
					ping = 1f;
				}
				if (!flag2)
				{
					newToRoom = 0;
				}
			}
			if (cam.room.abstractRoom.index != lastRoom)
			{
				newToRoom = 100;
				lastRoom = cam.room.abstractRoom.index;
				pingDelay = 50;
			}
		}
		if (newToRoom > 0)
		{
			newToRoom--;
		}
		if (pingDelay > 0)
		{
			pingDelay--;
		}
		if (ping > 0f)
		{
			ping = Custom.LerpAndTick(ping, 0f, 0.05f, 1f / 30f);
			if (ping == 0f)
			{
				pingDelay = Mathf.Max(pingDelay, 20);
			}
		}
		circle.Update();
		circle.pos = pos;
		circle.rad = Mathf.Lerp(40f, 150f, Mathf.Pow(ping, 1.5f));
		circle.thickness = Mathf.Lerp(3f, 0f, Mathf.Pow(ping, 4f));
		circle.fade = Mathf.Sin(Mathf.Pow(ping, 0.4f) * (float)Math.PI);
	}

	public override void Draw(float timeStacker)
	{
		circle.Draw(timeStacker);
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		circle.ClearSprite();
	}
}
