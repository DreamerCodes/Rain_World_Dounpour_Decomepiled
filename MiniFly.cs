using System;
using RWCustom;
using UnityEngine;

public class MiniFly : CosmeticInsect
{
	private Vector2 dir;

	private Vector2 lastLastPos;

	public BodyChunk buzzAroundCorpse;

	public MiniFly(Room room, Vector2 pos)
		: base(room, pos, Type.StandardFly)
	{
		lastLastPos = pos;
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
		vel *= 0.85f;
		vel += dir * 2.4f;
		dir = Vector2.Lerp(dir, Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Pow(UnityEngine.Random.value, 0.75f), Mathf.Pow(UnityEngine.Random.value, 1.5f));
		if (wantToBurrow)
		{
			dir = Vector2.Lerp(dir, new Vector2(0f, -1f), 0.1f);
		}
		else if (buzzAroundCorpse != null)
		{
			if (!ViableForBuzzaround((buzzAroundCorpse.owner as Creature).abstractCreature))
			{
				buzzAroundCorpse = null;
			}
			else if (!Custom.DistLess(pos, buzzAroundCorpse.pos + new Vector2(0f, buzzAroundCorpse.rad + 30f), buzzAroundCorpse.rad + 20f + Mathf.Pow(UnityEngine.Random.value, 2f) * 100f))
			{
				vel += Custom.DirVec(pos, buzzAroundCorpse.pos + new Vector2(0f, buzzAroundCorpse.rad + 30f)) * 0.4f * UnityEngine.Random.value;
				dir = Vector2.Lerp(dir, Custom.DirVec(pos, buzzAroundCorpse.pos + new Vector2(0f, buzzAroundCorpse.rad)), Mathf.Pow(UnityEngine.Random.value, 2f) * 0.65f);
			}
		}
		else if (base.OutOfBounds)
		{
			dir = Vector2.Lerp(dir, Custom.DirVec(pos, mySwarm.placedObject.pos), Mathf.InverseLerp(mySwarm.insectGroupData.Rad, mySwarm.insectGroupData.Rad + 100f, Vector2.Distance(pos, mySwarm.placedObject.pos)));
		}
		else
		{
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
			if (UnityEngine.Random.value < 1f / 120f && room.abstractRoom.creatures.Count > 0)
			{
				AbstractCreature abstractCreature = room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)];
				if (abstractCreature.realizedCreature != null && Custom.DistLess(pos, abstractCreature.realizedCreature.firstChunk.pos, 250f + 250f * UnityEngine.Random.value) && ViableForBuzzaround(abstractCreature) && room.VisualContact(pos, abstractCreature.realizedCreature.firstChunk.pos))
				{
					buzzAroundCorpse = abstractCreature.realizedCreature.bodyChunks[UnityEngine.Random.Range(0, abstractCreature.realizedCreature.bodyChunks.Length)];
				}
			}
		}
		if (room.PointSubmerged(pos))
		{
			pos.y = room.FloatWaterLevel(pos.x);
		}
		lastLastPos = lastPos;
		base.Update(eu);
		if (!room.BeingViewed)
		{
			Destroy();
		}
	}

	private bool ViableForBuzzaround(AbstractCreature crit)
	{
		if (crit.realizedCreature != null && UnityEngine.Random.value > 0.00083333335f && (crit.state.dead || (crit.realizedCreature is Player && (crit.realizedCreature as Player).Malnourished) || (ModManager.MSC && crit.realizedCreature is DaddyLongLegs && (crit.realizedCreature as DaddyLongLegs).isHD)) && (mySwarm == null || Custom.DistLess(mySwarm.placedObject.pos, mySwarm.placedObject.pos, mySwarm.insectGroupData.Rad * (1f + UnityEngine.Random.value))) && !crit.realizedCreature.slatedForDeletetion && crit.realizedCreature.room == room)
		{
			return !crit.creatureTemplate.smallCreature;
		}
		return false;
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
		sLeaser.sprites[0].color = palette.blackColor;
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
