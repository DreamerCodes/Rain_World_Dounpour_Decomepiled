using RWCustom;
using UnityEngine;

namespace ScavengerCosmetic;

public class Tail : Template
{
	public Tail(ScavengerGraphics owner, int firstSprite)
		: base(owner, firstSprite)
	{
		totalSprites = 1;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMesh(base.scavGrphs.tail.Length, pointyTip: true, customColor: false);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.scavGrphs.drawPositions[3, 1], base.scavGrphs.drawPositions[3, 0], timeStacker);
		float num = 3f;
		for (int i = 0; i < base.scavGrphs.tail.Length; i++)
		{
			Vector2 vector2 = Vector2.Lerp(base.scavGrphs.tail[i].lastPos, base.scavGrphs.tail[i].pos, timeStacker);
			Vector2 normalized = (vector2 - vector).normalized;
			Vector2 vector3 = Custom.PerpendicularVector(normalized);
			float num2 = Vector2.Distance(vector2, vector) / 5f;
			if (i == 0)
			{
				num2 = 0f;
			}
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * num + normalized * num2 - camPos);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * num + normalized * num2 - camPos);
			if (i < base.scavGrphs.tail.Length - 1)
			{
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * base.scavGrphs.tail[i].StretchedRad - normalized * num2 - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * base.scavGrphs.tail[i].StretchedRad - normalized * num2 - camPos);
			}
			else
			{
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - camPos);
			}
			num = base.scavGrphs.tail[i].StretchedRad;
			vector = vector2;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[firstSprite].color = base.scavGrphs.BlendedBodyColor;
	}
}
