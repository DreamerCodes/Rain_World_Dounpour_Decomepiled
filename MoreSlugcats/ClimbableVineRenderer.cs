using System.Collections.Generic;
using UnityEngine;

namespace MoreSlugcats;

public class ClimbableVineRenderer : UpdatableAndDeletable, INotifyWhenRoomIsReady, IDrawable
{
	private int totalSprites;

	public List<ClimbableVine> climbVines;

	public ClimbableVineRenderer(Room room)
	{
		base.room = room;
		climbVines = new List<ClimbableVine>();
	}

	public void AIMapReady()
	{
		for (int i = 0; i < room.roomSettings.placedObjects.Count; i++)
		{
			if (room.roomSettings.placedObjects[i].active && room.roomSettings.placedObjects[i].type == PlacedObject.Type.Vine)
			{
				ClimbableVine climbableVine = new ClimbableVine(room, totalSprites, room.roomSettings.placedObjects[i]);
				room.AddObject(climbableVine);
				climbVines.Add(climbableVine);
				totalSprites += climbableVine.graphic.sprites;
			}
		}
	}

	public void ShortcutsReady()
	{
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		for (int i = 0; i < climbVines.Count; i++)
		{
			climbVines[i].graphic.InitiateSprites(sLeaser, rCam);
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (sLeaser.sprites.Length == 0)
		{
			InitiateSprites(sLeaser, rCam);
		}
		for (int i = 0; i < climbVines.Count; i++)
		{
			climbVines[i].graphic.DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (sLeaser.sprites.Length != 0)
		{
			for (int i = 0; i < climbVines.Count; i++)
			{
				climbVines[i].graphic.ApplyPalette(sLeaser, rCam, palette);
			}
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
		}
		for (int j = 0; j < climbVines.Count; j++)
		{
			for (int k = 0; k < climbVines[j].graphic.sprites; k++)
			{
				sLeaser.sprites[climbVines[j].graphic.firstSprite + k].RemoveFromContainer();
				rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[climbVines[j].graphic.firstSprite + k]);
			}
		}
	}
}
