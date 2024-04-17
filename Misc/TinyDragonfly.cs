using RWCustom;
using UnityEngine;

public class TinyDragonfly : CosmeticInsect
{
	public Vector2 hoverPos;

	public int hoverTimer;

	public Vector2 dir;

	public Vector2 lastDir;

	public Vector2 goalDir;

	private float hue;

	private bool SIColors;

	public Color paletteBlack;

	private float paletteDarkness;

	public TinyDragonfly(Room room, Vector2 pos)
		: base(room, pos, Type.TinyDragonFly)
	{
		hoverPos = pos;
		hoverTimer = Random.Range(20, 120);
		creatureAvoider = new CreatureAvoider(this, 10, 200f, 0.2f);
		hue = Random.value;
		SIColors = room.world.region != null && room.world.region.name == "SI";
		goalDir = Custom.RNV();
	}

	public override void Update(bool eu)
	{
		lastDir = dir;
		base.Update(eu);
		vel.y -= 0.5f;
		vel *= 0.9f;
		dir -= vel * 0.2f;
		dir.Normalize();
		if (submerged)
		{
			vel *= 0.8f;
		}
	}

	public override void Act()
	{
		base.Act();
		vel *= 0.9f;
		vel.y += 0.5f;
		vel += Vector2.ClampMagnitude(hoverPos - pos, 10f) / 10f * 1.2f;
		vel += Custom.RNV() * Random.value * 0.6f;
		dir = Vector3.RotateTowards(dir, goalDir, 0.4f, 1f);
		if (wantToBurrow)
		{
			hoverPos -= Custom.DegToVec(Mathf.Lerp(-20f, 20f, Random.value)) * Random.value * 30f;
			return;
		}
		hoverTimer--;
		if (hoverTimer < 0)
		{
			Vector2 vector = pos + Custom.RNV() * Mathf.Lerp(30f, 60f, Random.value);
			if (!room.GetTile(vector).Solid && room.IsPositionInsideBoundries(room.GetTilePosition(vector)) && !room.PointSubmerged(vector + new Vector2(0f, -20f)) && SharedPhysics.RayTraceTilesForTerrain(room, pos, vector))
			{
				hoverPos = vector;
				hoverTimer = Random.Range(20, 120);
				goalDir = Custom.RNV();
			}
		}
		IntVector2 intVector = Custom.eightDirections[Random.Range(0, 8)];
		if (TileScore(room.GetTilePosition(hoverPos) + intVector) < TileScore(room.GetTilePosition(hoverPos)))
		{
			hoverPos += intVector.ToVector2().normalized * Random.value * 5f;
		}
		if (creatureAvoider.currentWorstCrit != null && Random.value < creatureAvoider.FleeSpeed)
		{
			hoverPos = pos + Custom.DirVec(creatureAvoider.currentWorstCrit.DangerPos, pos) * creatureAvoider.FleeSpeed * 200f;
		}
		if (base.OutOfBounds)
		{
			hoverPos += Custom.DirVec(hoverPos, mySwarm.placedObject.pos);
		}
		if (submerged)
		{
			vel.y += 4f * Random.value;
		}
		else if (room.PointSubmerged(pos + new Vector2(0f, -20f)))
		{
			vel.y += 0.2f;
		}
	}

	public override void WallCollision(IntVector2 dir, bool first)
	{
		vel -= dir.ToVector2() * 6f + Custom.RNV() * Mathf.Lerp(2f, 5f, Random.value);
		creatureAvoider.Reset();
	}

	public override void EmergeFromGround(Vector2 emergePos)
	{
		base.EmergeFromGround(emergePos);
		pos = emergePos + new Vector2(0f, 3f);
		vel = Custom.DegToVec(Mathf.Lerp(-20f, 20f, Random.value)) * Mathf.Lerp(2f, 6f, Random.value);
		hoverPos = emergePos + vel * 2f;
	}

	private float TileScore(IntVector2 testTile)
	{
		if (room.GetTile(testTile).Solid)
		{
			return float.MaxValue;
		}
		if (!room.readyForAI)
		{
			return Random.value;
		}
		float num = 0f;
		num += Mathf.Abs(5f - (float)room.aimap.getTerrainProximity(testTile));
		num += Mathf.Abs(5f - (float)room.aimap.getAItile(testTile).smoothedFloorAltitude);
		if (room.water)
		{
			if (testTile.y < room.defaultWaterLevel)
			{
				return float.MaxValue;
			}
			num += Mathf.Abs(5f - (float)room.defaultWaterLevel) * 4f;
		}
		return num;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[3];
		sLeaser.sprites[0] = new FSprite("pixel");
		sLeaser.sprites[0].anchorY = 0.2f;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[1 + i] = new FSprite("pixel");
			sLeaser.sprites[1 + i].anchorY = -0.5f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		float num = Mathf.Lerp(lastInGround, inGround, timeStacker);
		Vector2 vector2 = Vector3.Slerp(lastDir, dir, timeStacker);
		vector.y -= 5f * num;
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[0].scaleY = 7f * (1f - num);
		sLeaser.sprites[0].scaleX = 2f * (1f - num);
		sLeaser.sprites[0].rotation = Custom.VecToDeg(vector2);
		float num2 = Mathf.Pow(Mathf.InverseLerp(0.6f, 1f, Vector2.Dot(vector2, Custom.DegToVec(47f))), 2f) * Mathf.Pow(Random.value, 0.5f);
		if (SIColors)
		{
			sLeaser.sprites[0].color = Color.Lerp(Custom.HSL2RGB(Mathf.Lerp(0.6f, 0.7f, hue), 1f, 0.3f + 0.4f * num2), paletteBlack, 0.3f * (1f - num2) + 0.5f * paletteDarkness);
		}
		else
		{
			sLeaser.sprites[0].color = Color.Lerp(Custom.HSL2RGB(Mathf.Lerp(0.35f, 0.45f, hue), 0.8f + 0.2f * Mathf.Pow(num2, 0.3f), 0.5f + 0.4f * num2), paletteBlack, 0.2f * (1f - num2) + 0.5f * paletteDarkness);
		}
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[1 + i].x = vector.x - camPos.x;
			sLeaser.sprites[1 + i].y = vector.y - camPos.y;
			sLeaser.sprites[1 + i].scaleY = 5f * (1f - num);
			sLeaser.sprites[1 + i].scaleX = 1f - num;
			sLeaser.sprites[1 + i].rotation = Mathf.Lerp(-30f, 30f, Random.value) + ((i == 0) ? (-90f) : 90f);
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		paletteBlack = palette.blackColor;
		paletteDarkness = palette.darkness;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[1 + i].color = Color.Lerp(Color.Lerp(new Color(1f, 1f, 1f), palette.fogColor, 0.5f), palette.blackColor, 0.5f * palette.darkness);
		}
	}
}
