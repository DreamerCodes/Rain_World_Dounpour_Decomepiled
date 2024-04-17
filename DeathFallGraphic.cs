using MoreSlugcats;
using UnityEngine;

public class DeathFallGraphic : UpdatableAndDeletable, IDrawable
{
	public float height;

	public int counter;

	public float defaultHeight;

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		FAtlasElement elementWithName = Futile.atlasManager.GetElementWithName("Futile_White");
		if (Futile.atlasManager.DoesContainElementWithName("RainMask_" + room.abstractRoom.name))
		{
			elementWithName = Futile.atlasManager.GetElementWithName("RainMask_" + room.abstractRoom.name);
		}
		sLeaser.sprites[0] = new FSprite(elementWithName);
		sLeaser.sprites[0].scaleX = (rCam.sSize.x + 20f) / elementWithName.sourcePixelSize.x;
		sLeaser.sprites[0].scaleY = 180f / elementWithName.sourcePixelSize.y;
		sLeaser.sprites[0].anchorY = 0f;
		bool flag = ModManager.MMF && MMF.cfgClearerDeathGradients.Value;
		bool flag2 = rCam.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.AboveCloudsView) != null;
		bool flag3 = rCam.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.RoofTopView) != null;
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders[(flag || flag3) ? "DeathFallHeavy" : "DeathFall"];
		height = float.MaxValue;
		for (int i = 0; i < rCam.room.cameraPositions.Length; i++)
		{
			if (rCam.room.cameraPositions[i].y < height)
			{
				height = rCam.room.cameraPositions[i].y;
			}
		}
		defaultHeight = height;
		string layerName = "Foreground";
		if (flag2 || flag3)
		{
			layerName = "Bloom";
		}
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer(layerName));
	}

	public override void Update(bool eu)
	{
		counter++;
		height = defaultHeight;
		for (int i = 0; i < room.deathFallFocalPoints.Count; i++)
		{
			if (room.ViewedByAnyCamera(room.deathFallFocalPoints[i], 0f))
			{
				int num = room.CameraViewingPoint(room.deathFallFocalPoints[i]);
				if (num != -1)
				{
					height = room.cameraPositions[num].y;
					break;
				}
			}
		}
		base.Update(eu);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = rCam.sSize.x / 2f;
		sLeaser.sprites[0].y = height - camPos.y;
		sLeaser.sprites[0].alpha = 0.85f + 0.15f * Mathf.Sin(((float)counter + timeStacker) / 25f);
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}
}
