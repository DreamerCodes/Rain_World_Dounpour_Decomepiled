using RWCustom;
using UnityEngine;

public class RedSwarmer : CosmeticInsect
{
	public Vector2 hoverPos;

	public int hoverTimer;

	public Vector2 dir;

	public Vector2 lastDir;

	private float hue;

	public RedSwarmer(Room room, Vector2 pos)
		: base(room, pos, Type.RedSwarmer)
	{
		hoverPos = pos;
		hoverTimer = Random.Range(20, 120);
		creatureAvoider = new CreatureAvoider(this, 10, 200f, 0.2f);
		hue = Random.value;
	}

	public override void Update(bool eu)
	{
		lastDir = dir;
		base.Update(eu);
		vel.y -= 0.8f;
		vel *= 0.87f;
		dir -= vel * 0.3f;
		dir.Normalize();
	}

	public override void Act()
	{
		base.Act();
		vel.y += 0.8f;
		vel += Vector2.ClampMagnitude(hoverPos - pos, 20f) / 20f * 0.5f;
		vel += Custom.RNV() * Random.value * 0.8f;
		dir = Vector3.Slerp(dir, new Vector2(0f, -1f), 0.2f);
		if (wantToBurrow)
		{
			hoverPos -= Custom.DegToVec(Mathf.Lerp(-20f, 20f, Random.value)) * Random.value * 30f;
			return;
		}
		hoverTimer--;
		if (hoverTimer < 0)
		{
			Vector2 vector = pos + Custom.RNV() * Mathf.Lerp(20f, 120f, Random.value);
			if (!room.GetTile(vector).Solid && room.IsPositionInsideBoundries(room.GetTilePosition(vector)) && !room.PointSubmerged(vector + new Vector2(0f, -60f)) && SharedPhysics.RayTraceTilesForTerrain(room, pos, vector))
			{
				hoverPos = vector;
				hoverTimer = Random.Range(0, 6);
			}
		}
		IntVector2 intVector = Custom.eightDirections[Random.Range(0, 8)];
		if (TileScore(room.GetTilePosition(hoverPos) + intVector) < TileScore(room.GetTilePosition(hoverPos)))
		{
			hoverPos += intVector.ToVector2().normalized * Random.value * 20f;
		}
		if (creatureAvoider.currentWorstCrit != null)
		{
			hoverPos = pos + Custom.DirVec(creatureAvoider.currentWorstCrit.DangerPos, pos) * creatureAvoider.FleeSpeed * 200f;
		}
		if (base.OutOfBounds)
		{
			hoverPos += Custom.DirVec(hoverPos, mySwarm.placedObject.pos);
		}
		if (submerged || room.PointSubmerged(pos + new Vector2(0f, -40f)))
		{
			vel.y += 0.5f;
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
		if (room.water && (float)testTile.y < (float)room.defaultWaterLevel + 5f)
		{
			return float.MaxValue;
		}
		if (!room.readyForAI)
		{
			return Random.value;
		}
		return 0f + Mathf.Abs(3f - (float)room.aimap.getTerrainProximity(testTile)) + Mathf.Abs(3f - (float)room.aimap.getAItile(testTile).floorAltitude) * 2f;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[5];
		sLeaser.sprites[0] = new FSprite("Circle20");
		sLeaser.sprites[0].anchorY = 0.3f;
		sLeaser.sprites[1] = new FSprite("pixel");
		sLeaser.sprites[1].anchorY = 0f;
		sLeaser.sprites[2] = new FSprite("tinyStar");
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[3 + i] = new FSprite("pixel");
			sLeaser.sprites[3 + i].anchorY = -1.2f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		float num = Mathf.Lerp(lastInGround, inGround, timeStacker);
		Vector2 v = Vector3.Slerp(lastDir, dir, timeStacker);
		vector.y -= 5f * num;
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[0].scaleY = 6f * (1f - num) / 20f;
		sLeaser.sprites[0].scaleX = 3f * (1f - num) / 20f;
		sLeaser.sprites[0].rotation = Custom.VecToDeg(v);
		sLeaser.sprites[1].x = vector.x - camPos.x;
		sLeaser.sprites[1].y = vector.y - camPos.y;
		sLeaser.sprites[1].rotation = Custom.VecToDeg(v);
		sLeaser.sprites[1].scaleY = 11f * (1f - num);
		sLeaser.sprites[2].x = vector.x + v.x * 2f - camPos.x - 0.5f;
		sLeaser.sprites[2].y = vector.y + v.x * 2f - camPos.y + 0.5f;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[3 + i].x = vector.x - camPos.x;
			sLeaser.sprites[3 + i].y = vector.y - camPos.y;
			sLeaser.sprites[3 + i].scaleY = 4f * (1f - num);
			sLeaser.sprites[3 + i].scaleX = 1f - num;
			sLeaser.sprites[3 + i].rotation = Mathf.Pow(Random.value, 0.5f) * 40f * ((Random.value < 0.5f) ? (-1f) : 1f) + ((i == 0) ? (-90f) : 90f);
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		sLeaser.sprites[0].color = Color.Lerp(Custom.HSL2RGB(Mathf.Lerp(0.005f, 0.03f, hue), 1f, 0.5f), palette.blackColor, 0.1f + 0.6f * palette.darkness);
		sLeaser.sprites[1].color = Color.Lerp(Custom.HSL2RGB(Mathf.Lerp(0.005f, 0.03f, hue), 1f, 0.5f), palette.blackColor, 0.1f + 0.6f * palette.darkness);
		sLeaser.sprites[2].color = Color.Lerp(Custom.HSL2RGB(Mathf.Lerp(0.005f, 0.03f, hue), 1f, 0.6f), palette.blackColor, 0.4f * palette.darkness);
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[3 + i].color = Color.Lerp(Color.Lerp(Custom.HSL2RGB(Mathf.Lerp(0.01f, 0.03f, hue) + 0.02f, 1f, 0.6f), palette.fogColor, 0.4f), palette.blackColor, 0.5f * palette.darkness);
		}
	}
}
