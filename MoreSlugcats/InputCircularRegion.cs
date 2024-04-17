using System;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class InputCircularRegion : CosmeticSprite
{
	public float radius;

	public float minAngle;

	public float maxAngle;

	public float speed;

	public new Vector2 pos;

	public Vector2 drawDebugPos;

	public InputCircularRegion(float radius, float minAngle, float maxAngle, float speed, Vector2 startOffset)
	{
		this.radius = radius;
		this.minAngle = Custom.Mod(minAngle, 360f);
		this.maxAngle = Custom.Mod(maxAngle, 360f);
		this.speed = speed;
		pos = startOffset;
		drawDebugPos = Vector2.zero;
	}

	public bool ClampPosToWedge()
	{
		float num = Custom.Mod(Mathf.Atan2(pos.y, pos.x) * (180f / (float)Math.PI), 360f);
		bool result = false;
		if (((num < minAngle || num > maxAngle) && minAngle < maxAngle) || (num < minAngle && num > maxAngle && minAngle > maxAngle))
		{
			float num2 = Mathf.DeltaAngle(num, minAngle);
			float num3 = Mathf.DeltaAngle(num, maxAngle);
			if (num2 < num3)
			{
				pos = new Vector2(pos.magnitude * Mathf.Cos((float)Math.PI / 180f * minAngle), pos.magnitude * Mathf.Sin((float)Math.PI / 180f * minAngle));
			}
			else
			{
				pos = new Vector2(pos.magnitude * Mathf.Cos((float)Math.PI / 180f * maxAngle), pos.magnitude * Mathf.Sin((float)Math.PI / 180f * maxAngle));
			}
			result = true;
		}
		if (pos.magnitude > radius)
		{
			pos = Vector2.ClampMagnitude(pos, radius);
			result = true;
		}
		return result;
	}

	public void Update(Player.InputPackage? input)
	{
		if (input.HasValue)
		{
			pos.x += (float)input.Value.x * speed;
			pos.y += (float)input.Value.y * speed;
		}
		ClampPosToWedge();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[5];
		for (int i = 1; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i] = new FSprite("Futile_White");
			sLeaser.sprites[i].scaleX = 8f / sLeaser.sprites[i].width;
			sLeaser.sprites[i].scaleY = 8f / sLeaser.sprites[i].height;
			if (i == sLeaser.sprites.Length - 1)
			{
				sLeaser.sprites[i].color = Color.red;
			}
		}
		sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(15, pointyTip: true, customColor: true);
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[1].x = drawDebugPos.x - camPos.x;
		sLeaser.sprites[1].y = drawDebugPos.y - camPos.y;
		Vector2 vector = new Vector2(radius * Mathf.Cos((float)Math.PI / 180f * minAngle), radius * Mathf.Sin((float)Math.PI / 180f * minAngle));
		sLeaser.sprites[2].x = drawDebugPos.x + vector.x - camPos.x;
		sLeaser.sprites[2].y = drawDebugPos.y + vector.y - camPos.y;
		Vector2 vector2 = new Vector2(radius * Mathf.Cos((float)Math.PI / 180f * maxAngle), radius * Mathf.Sin((float)Math.PI / 180f * maxAngle));
		sLeaser.sprites[3].x = drawDebugPos.x + vector2.x - camPos.x;
		sLeaser.sprites[3].y = drawDebugPos.y + vector2.y - camPos.y;
		sLeaser.sprites[4].x = drawDebugPos.x + pos.x - camPos.x;
		sLeaser.sprites[4].y = drawDebugPos.y + pos.y - camPos.y;
		Vector2[,] array = new Vector2[15, 3];
		float num = 4f;
		for (int i = 0; i < array.GetLength(0); i++)
		{
			array[i, 0] = drawDebugPos;
			array[i, 0].x += Mathf.Sin((float)Math.PI * 2f / (float)(array.GetLength(0) - 1) * (float)i) * radius;
			array[i, 0].y += Mathf.Cos((float)Math.PI * 2f / (float)(array.GetLength(0) - 1) * (float)i) * radius;
			array[i, 1] = array[i, 0];
		}
		Vector2 vector3 = Vector2.Lerp(array[0, 1], array[0, 0], timeStacker);
		for (int j = 0; j < array.GetLength(0); j++)
		{
			Vector2 vector4 = Vector2.Lerp(array[j, 1], array[j, 0], timeStacker);
			Vector2 normalized = (vector4 - vector3).normalized;
			Vector2 vector5 = Custom.PerpendicularVector(normalized);
			float num2 = Vector2.Distance(vector4, vector3) / 5f;
			float num3 = 1f;
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4, vector3 - vector5 * (num + num3) * 0.5f + normalized * num2 - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4 + 1, vector3 + vector5 * (num + num3) * 0.5f + normalized * num2 - camPos);
			if (j < array.GetLength(0) - 1)
			{
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4 + 2, vector4 - vector5 * num3 - normalized * num2 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4 + 3, vector4 + vector5 * num3 - normalized * num2 - camPos);
			}
			else
			{
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4 + 2, vector4 - camPos);
			}
			num = num3;
			vector3 = vector4;
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
