using RWCustom;
using UnityEngine;

public class WaterDrip : CosmeticSprite
{
	public float life;

	private float lastLife;

	public int lifeTime;

	public Vector2 lastLastPos;

	public Vector2 lastLastLastPos;

	private Color[] colors;

	private float randomLightness;

	private float lastRandomLightness;

	private bool waterColor;

	private float width;

	public bool mustExitTerrainOnceToBeDestroyedByTerrain;

	public WaterDrip(Vector2 pos, Vector2 vel, bool waterColor)
	{
		life = 1f;
		lastLife = 1f;
		base.pos = pos;
		lastPos = pos;
		lastLastPos = pos;
		lastLastLastPos = pos;
		base.vel = vel;
		this.waterColor = waterColor;
		width = (waterColor ? 1.5f : 1f);
		lifeTime = Random.Range(10, 120);
	}

	public override void Update(bool eu)
	{
		lastLastLastPos = lastLastPos;
		lastLastPos = lastPos;
		vel.y -= 0.9f * room.gravity;
		lastLife = life;
		life -= 1f / (float)lifeTime;
		if (lastLife <= 0f || pos.y < room.FloatWaterLevel(pos.x) || (room.GetTile(pos).Terrain == Room.Tile.TerrainType.Solid && !mustExitTerrainOnceToBeDestroyedByTerrain))
		{
			Destroy();
		}
		if (mustExitTerrainOnceToBeDestroyedByTerrain && !room.GetTile(pos).Solid)
		{
			mustExitTerrainOnceToBeDestroyedByTerrain = false;
		}
		lastRandomLightness = randomLightness;
		randomLightness = Mathf.Lerp(Random.value, 1f, room.WaterShinyness(pos, 1f) * 0.5f);
		base.Update(eu);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[1]
		{
			new TriangleMesh.Triangle(0, 1, 2)
		};
		TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: false);
		sLeaser.sprites[0] = triangleMesh;
		sLeaser.sprites[1] = new FSprite("Circle20");
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(lastLastLastPos, lastLastPos, timeStacker);
		if (lastLife > 0f && life <= 0f)
		{
			vector2 = Vector2.Lerp(vector2, vector, timeStacker);
		}
		Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
		(sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, vector + vector3 * width - camPos);
		(sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, vector - vector3 * width - camPos);
		(sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, vector2 - camPos);
		float num = rCam.room.WaterShinyness(vector, timeStacker) * Mathf.Lerp(lastRandomLightness, randomLightness, timeStacker) * Mathf.InverseLerp(1f, 0.5f, rCam.currentPalette.darkness);
		if (num > 0.5f && Random.value < 0.3f)
		{
			sLeaser.sprites[1].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[1].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[1].scale = Random.value * 0.5f * Mathf.InverseLerp(0.5f, 1f, num) * width;
			sLeaser.sprites[1].isVisible = true;
		}
		else
		{
			sLeaser.sprites[1].isVisible = false;
		}
		float num2 = Mathf.InverseLerp(0f, 0.5f, num);
		if (num2 < 0.5f)
		{
			sLeaser.sprites[0].color = Color.Lerp(colors[0], colors[1], Mathf.InverseLerp(0f, 0.5f, num2));
		}
		else
		{
			sLeaser.sprites[0].color = Color.Lerp(colors[1], colors[2], Mathf.InverseLerp(0.5f, 1f, num2));
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (waterColor)
		{
			colors = new Color[3]
			{
				Color.Lerp(palette.waterColor2, palette.waterColor1, 0.5f),
				palette.waterColor1,
				new Color(1f, 1f, 1f)
			};
		}
		else
		{
			colors = new Color[3]
			{
				palette.blackColor,
				Color.Lerp(palette.blackColor, new Color(1f, 1f, 1f), 0.5f),
				new Color(1f, 1f, 1f)
			};
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}

	public void WhiteDrip()
	{
		if (PlayerGraphics.CustomColorsEnabled())
		{
			colors = new Color[3]
			{
				PlayerGraphics.CustomColorSafety(2),
				PlayerGraphics.CustomColorSafety(2),
				PlayerGraphics.CustomColorSafety(2)
			};
		}
		else
		{
			colors = new Color[3]
			{
				new Color(1f, 1f, 1f),
				new Color(1f, 1f, 1f),
				new Color(1f, 1f, 1f)
			};
		}
	}
}
