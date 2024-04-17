using RWCustom;
using UnityEngine;

public class DebugMouse : CosmeticSprite, IDrawable
{
	public FLabel label;

	public FLabel label2;

	private Vector2 dataPos = Vector2.zero;

	public DebugMouse()
	{
		label = new FLabel(Custom.GetFont(), "0");
		label.alignment = FLabelAlignment.Left;
		label.color = new Color(1f, 0f, 0f);
		label2 = new FLabel(Custom.GetFont(), "0");
		label2.alignment = FLabelAlignment.Left;
		label2.color = new Color(0f, 0f, 0f);
	}

	public override void Update(bool eu)
	{
		pos = (Vector2)Futile.mousePosition + room.game.cameras[0].pos;
		string text = "pos: x=" + pos.x + " y=" + pos.y + "     TPOS: X=" + room.GetTilePosition(pos).x + " Y=" + room.GetTilePosition(pos).y;
		text += "\r\n";
		if (room.readyForAI)
		{
			text = text + "vis: " + room.aimap.Visibility(room.GetTilePosition(pos)) + "\r\n";
			text = text + "solidCeiling: " + room.GetTile(room.GetTilePosition(pos).x, room.TileHeight - 1).Solid;
		}
		label.text = text;
		label2.text = label.text;
		base.Update(eu);
		if (!room.BeingViewed)
		{
			Destroy();
		}
	}

	public override void Destroy()
	{
		label.RemoveFromContainer();
		label2.RemoveFromContainer();
		base.Destroy();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[6];
		sLeaser.sprites[0] = new FSprite("pixel");
		sLeaser.sprites[0].color = new Color(1f, 0f, 0f);
		sLeaser.sprites[0].scale = 10f;
		sLeaser.sprites[0].anchorX = 0f;
		sLeaser.sprites[0].anchorY = 1f;
		rCam.ReturnFContainer("HUD2").AddChild(sLeaser.sprites[0]);
		rCam.ReturnFContainer("HUD2").AddChild(label2);
		rCam.ReturnFContainer("HUD2").AddChild(label);
		for (int i = 1; i < 5; i++)
		{
			sLeaser.sprites[i] = new FSprite("pixel");
			sLeaser.sprites[i].color = new Color(0f, 1f, 0f);
			sLeaser.sprites[i].alpha = 0.5f;
			rCam.ReturnFContainer("HUD2").AddChild(sLeaser.sprites[i]);
		}
		sLeaser.sprites[1].scaleY = 21f;
		sLeaser.sprites[2].scaleX = 21f;
		sLeaser.sprites[3].scaleY = 21f;
		sLeaser.sprites[4].scaleX = 21f;
		sLeaser.sprites[5] = new FSprite("pixel");
		sLeaser.sprites[5].color = new Color(0f, 1f, 0f);
		sLeaser.sprites[5].scale = 7f;
		rCam.ReturnFContainer("HUD2").AddChild(sLeaser.sprites[5]);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = pos.x - camPos.x;
		sLeaser.sprites[0].y = pos.y - camPos.y;
		if (pos.x > camPos.x + 800.1f)
		{
			label.x = 800.1f;
			label.y = pos.y - camPos.y;
		}
		else
		{
			label.x = pos.x - camPos.x + 40.1f;
			label.y = pos.y - camPos.y;
		}
		label2.x = label.x + 1f;
		label2.y = label.y - 1f;
		Vector2 vector = rCam.room.MiddleOfTile(pos);
		sLeaser.sprites[1].x = vector.x - camPos.x - 10f;
		sLeaser.sprites[1].y = vector.y - camPos.y;
		sLeaser.sprites[2].x = vector.x - camPos.x;
		sLeaser.sprites[2].y = vector.y - camPos.y + 10f;
		sLeaser.sprites[3].x = vector.x - camPos.x + 10f;
		sLeaser.sprites[3].y = vector.y - camPos.y;
		sLeaser.sprites[4].x = vector.x - camPos.x;
		sLeaser.sprites[4].y = vector.y - camPos.y - 10f;
		sLeaser.sprites[5].x = dataPos.x - camPos.x;
		sLeaser.sprites[5].y = dataPos.y - camPos.y - 10f;
		for (int i = 0; i < 6; i++)
		{
			sLeaser.sprites[i].isVisible = rCam.room.game.mapVisible;
		}
		label.isVisible = rCam.room.game.mapVisible;
		label2.isVisible = rCam.room.game.mapVisible;
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
