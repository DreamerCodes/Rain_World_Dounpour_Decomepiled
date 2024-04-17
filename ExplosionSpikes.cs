using System;
using RWCustom;
using UnityEngine;

public class ExplosionSpikes : CosmeticSprite
{
	public float[,] values;

	public Vector2[] dirs;

	private Color color;

	private int time;

	private int spikes;

	private float innerRad;

	private float lifeTime;

	public ExplosionSpikes(Room room, Vector2 pos, int _spikes, float innerRad, float lifeTime, float width, float length, Color color)
	{
		base.room = room;
		this.innerRad = innerRad;
		base.pos = pos;
		this.color = color;
		this.lifeTime = lifeTime;
		spikes = (int)((float)_spikes * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value));
		values = new float[spikes, 3];
		dirs = new Vector2[spikes];
		float num = UnityEngine.Random.value * 360f;
		for (int i = 0; i < spikes; i++)
		{
			float num2 = (float)i / (float)spikes * 360f + num;
			dirs[i] = Custom.DegToVec(num2 + Mathf.Lerp(-0.5f, 0.5f, UnityEngine.Random.value) * 360f / (float)spikes);
			if (room.GetTile(pos + dirs[i] * (innerRad + length * 0.4f)).Solid)
			{
				values[i, 2] = lifeTime * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value) * 0.5f;
				values[i, 0] = length * Mathf.Lerp(0.6f, 1.4f, UnityEngine.Random.value) * 0.5f;
			}
			else
			{
				values[i, 2] = lifeTime * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value);
				values[i, 0] = length * Mathf.Lerp(0.6f, 1.4f, UnityEngine.Random.value);
			}
			values[i, 1] = width * Mathf.Lerp(0.6f, 1.4f, UnityEngine.Random.value);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		time++;
		if ((float)time > lifeTime * 2f)
		{
			Destroy();
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[spikes];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new TriangleMesh.Triangle(i * 3, i * 3 + 1, i * 3 + 2);
		}
		TriangleMesh triangleMesh = new TriangleMesh("Futile_White", array, customColor: true);
		sLeaser.sprites[0] = triangleMesh;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float value = (float)time + timeStacker;
		for (int i = 0; i < spikes; i++)
		{
			float num = Mathf.InverseLerp(0f, values[i, 2], value);
			float num2 = ((time == 0) ? timeStacker : Mathf.InverseLerp(values[i, 2], 0f, value));
			float num3 = Mathf.Lerp(values[i, 0] * 0.1f, values[i, 0], Mathf.Pow(num, 0.45f));
			float num4 = values[i, 1] * (0.5f + 0.5f * Mathf.Sin(num * (float)Math.PI)) * Mathf.Pow(num2, 0.3f);
			Color col = Color.Lerp(color, Color.white, Mathf.Pow(num2, Mathf.Lerp(0.2f, 1.5f, num)));
			Vector2 vector = pos + dirs[i] * (innerRad + num3);
			if (room != null && room.GetTile(vector).Solid)
			{
				num2 *= 0.5f;
			}
			Vector2 vector2 = pos + dirs[i] * (innerRad + num3 * 0.1f);
			Vector2 vector3 = Custom.PerpendicularVector(vector, vector2);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 3, vector - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 3 + 1, vector2 - vector3 * num4 * 0.5f - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 3 + 2, vector2 + vector3 * num4 * 0.5f - camPos);
			(sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 3] = Custom.RGB2RGBA(col, Mathf.Pow(num2, 0.75f));
			(sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 3 + 1] = Custom.RGB2RGBA(col, 0f);
			(sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 3 + 2] = Custom.RGB2RGBA(col, 0f);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Foreground");
		}
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
