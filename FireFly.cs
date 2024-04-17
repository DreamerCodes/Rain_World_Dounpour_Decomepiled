using System;
using RWCustom;
using UnityEngine;

public class FireFly : CosmeticInsect
{
	private Vector2 dir;

	private Vector2 lastLastPos;

	private LightSource light;

	public Color col;

	public float sin;

	public FireFly(Room room, Vector2 pos)
		: base(room, pos, Type.FireFly)
	{
		lastLastPos = pos;
		col = new Color(1f, Mathf.Lerp(0.6f, 0.8f, UnityEngine.Random.value), 0f);
		sin = UnityEngine.Random.value;
	}

	public override void Reset(Vector2 resetPos)
	{
		base.Reset(resetPos);
		lastLastPos = resetPos;
	}

	public override void EmergeFromGround(Vector2 emergePos)
	{
		base.EmergeFromGround(emergePos);
		dir = new Vector2(0f, 1f);
	}

	public override void Update(bool eu)
	{
		vel *= 0.95f;
		vel.x += dir.x * 0.3f;
		vel.y += dir.y * 0.2f;
		dir = Vector2.Lerp(dir, Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Pow(UnityEngine.Random.value, 0.75f), 0.4f).normalized;
		if (wantToBurrow)
		{
			dir = Vector2.Lerp(dir, new Vector2(0f, -1f), 0.1f);
		}
		else if (base.OutOfBounds)
		{
			dir = Vector2.Lerp(dir, Custom.DirVec(pos, mySwarm.placedObject.pos), Mathf.InverseLerp(mySwarm.insectGroupData.Rad, mySwarm.insectGroupData.Rad + 100f, Vector2.Distance(pos, mySwarm.placedObject.pos)));
		}
		float num = TileScore(room.GetTilePosition(pos));
		IntVector2 intVector = new IntVector2(0, 0);
		for (int i = 0; i < 4; i++)
		{
			if (!room.GetTile(room.GetTilePosition(pos) + Custom.fourDirections[i]).Solid && TileScore(room.GetTilePosition(pos) + Custom.fourDirections[i] * 3) > num)
			{
				num = TileScore(room.GetTilePosition(pos) + Custom.fourDirections[i] * 3);
				intVector = Custom.fourDirections[i];
			}
		}
		vel += intVector.ToVector2() * 0.4f;
		if (room.PointSubmerged(pos))
		{
			pos.y = room.FloatWaterLevel(pos.x);
		}
		sin += 1f / Mathf.Lerp(20f, 80f, UnityEngine.Random.value);
		if (room.Darkness(pos) > 0f)
		{
			if (light == null)
			{
				light = new LightSource(pos, environmentalLight: false, col, this);
				light.noGameplayImpact = ModManager.MMF;
				room.AddObject(light);
			}
			light.setPos = pos;
			light.setAlpha = 0.15f - 0.1f * Mathf.Sin(sin * (float)Math.PI * 2f);
			light.setRad = 60f + 20f * Mathf.Sin(sin * (float)Math.PI * 2f);
		}
		else if (light != null)
		{
			light.Destroy();
			light = null;
		}
		lastLastPos = lastPos;
		base.Update(eu);
		if (!room.BeingViewed)
		{
			Destroy();
		}
	}

	private float TileScore(IntVector2 tile)
	{
		if (!room.readyForAI || !room.IsPositionInsideBoundries(tile))
		{
			return 0f;
		}
		return UnityEngine.Random.value / (float)Math.Abs(room.aimap.getAItile(tile).floorAltitude - 4) / (float)Math.Abs(room.aimap.getTerrainProximity(tile) - 4);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("pixel");
		sLeaser.sprites[0].scaleX = 2f;
		sLeaser.sprites[0].anchorY = 0f;
		sLeaser.sprites[0].color = col;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(lastLastPos, lastPos, timeStacker), Vector2.Lerp(lastPos, pos, timeStacker));
		sLeaser.sprites[0].scaleY = Mathf.Max(2f, 2f + 1.1f * Vector2.Distance(Vector2.Lerp(lastLastPos, lastPos, timeStacker), Vector2.Lerp(lastPos, pos, timeStacker)));
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
