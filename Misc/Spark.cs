using RWCustom;
using UnityEngine;

public class Spark : CosmeticSprite
{
	public float life;

	public int lifeTime;

	public Vector2 lastLastLastPos;

	public Vector2 lastLastPos;

	public Color color;

	public LizardGraphics lizard;

	public float gravity;

	public Spark(Vector2 pos, Vector2 vel, Color color, LizardGraphics lizard, int standardLifeTime, int exceptionalLifeTime)
	{
		life = 1f;
		this.color = color;
		lastPos = pos;
		lastLastPos = pos;
		lastLastLastPos = pos;
		base.vel = vel;
		base.pos = pos + vel.normalized * 30f * Random.value;
		this.lizard = lizard;
		pos += vel * 3f;
		gravity = Mathf.Lerp(0.4f, 0.9f, Random.value);
		lifeTime = Random.Range(0, standardLifeTime);
		if (Random.value < 0.1f)
		{
			lifeTime = Random.Range(standardLifeTime, exceptionalLifeTime);
		}
	}

	public override void Update(bool eu)
	{
		lastLastLastPos = lastLastPos;
		lastLastPos = lastPos;
		vel.y -= gravity;
		life -= 1f / (float)lifeTime;
		if (lizard != null && lizard.whiteFlicker < 5)
		{
			life -= 0.2f;
		}
		if (room.GetTile(pos).Terrain == Room.Tile.TerrainType.Solid)
		{
			if (vel.y < 0f && room.GetTile(pos + new Vector2(0f, 20f)).Terrain == Room.Tile.TerrainType.Air)
			{
				pos.y = room.MiddleOfTile(pos).y + 10f;
				vel.y *= -0.5f;
				if (Mathf.Abs(vel.y) < 3f)
				{
					life -= 1f / 3f;
				}
			}
			else
			{
				Destroy();
			}
		}
		if (life <= 0f)
		{
			Destroy();
		}
		base.Update(eu);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[1]
		{
			new TriangleMesh.Triangle(0, 1, 2)
		};
		TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: false);
		sLeaser.sprites[0] = triangleMesh;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(lastLastLastPos, lastLastPos, timeStacker);
		if (Custom.DistLess(vector, vector2, 9f))
		{
			vector2 = vector + Custom.DirVec(vector, vector2) * 9f;
		}
		vector2 = Vector2.Lerp(vector, vector2, Mathf.InverseLerp(0f, 0.1f, life));
		Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
		(sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, vector + vector3 * 1f - camPos);
		(sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, vector - vector3 * 1f - camPos);
		(sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, vector2 - camPos);
		if (lizard != null && lizard.whiteFlicker > 0)
		{
			sLeaser.sprites[0].color = lizard.HeadColor(timeStacker);
		}
		else
		{
			sLeaser.sprites[0].color = color;
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
