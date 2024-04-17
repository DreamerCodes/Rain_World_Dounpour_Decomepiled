using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class DeerTentacle : Tentacle
{
	private DebugSprite[] grabGoalSprites;

	public int tentacleNumber;

	public int side;

	public int pair;

	public Vector2 tentacleDir;

	public float maxLength;

	public Vector2 desiredGrabPos;

	public bool attachedAtTip;

	public int framesWithoutReaching;

	public int grabDelay;

	public int setGrabDelay = 15;

	public int sureOfFootingCounter;

	public float smoothedFootingSecurity;

	public bool playStepSound;

	public float stepSoundVol;

	public Deer deer => owner as Deer;

	private DeerTentacle OtherTentacleInPair => deer.legs[(tentacleNumber < 2) ? (1 - tentacleNumber) : (2 + (1 - (tentacleNumber - 1)))];

	public DeerTentacle(Deer deer, BodyChunk chunk, float length, int tentacleNumber)
		: base(deer, chunk, length)
	{
		this.tentacleNumber = tentacleNumber;
		tProps = new TentacleProps(stiff: false, rope: false, shorten: true, 0.5f, 0f, 0.2f, 0.1f, 1.04f, 1.2f, 10f, 1f, 5f, 15, 60, 12, 0);
		tChunks = new TentacleChunk[6];
		for (int i = 0; i < tChunks.Length; i++)
		{
			tChunks[i] = new TentacleChunk(this, i, (float)(i + 1) / (float)tChunks.Length, 4f);
		}
		side = ((tentacleNumber % 2 != 0) ? 1 : 0);
		pair = ((tentacleNumber >= 2) ? 1 : 0);
		tentacleDir = Custom.DegToVec(45f + 90f * (float)tentacleNumber);
		stretchAndSqueeze = 0.1f;
		debugViz = false;
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		if (debugViz)
		{
			if (grabGoalSprites != null)
			{
				grabGoalSprites[0].RemoveFromRoom();
				grabGoalSprites[1].RemoveFromRoom();
			}
			grabGoalSprites = new DebugSprite[2];
			grabGoalSprites[0] = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel"), room);
			grabGoalSprites[0].sprite.scale = 10f;
			grabGoalSprites[0].sprite.color = new Color(1f, 0f, 0f);
			room.AddObject(grabGoalSprites[0]);
			grabGoalSprites[1] = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel"), room);
			grabGoalSprites[1].sprite.scale = 10f;
			grabGoalSprites[1].sprite.color = new Color(0f, 5f, 0f);
			room.AddObject(grabGoalSprites[1]);
		}
		IntVector2 tilePosition = room.GetTilePosition(base.FloatBase + new Vector2(0f, -400f) + tentacleDir * 100f);
		IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, base.BasePos, tilePosition);
		if (intVector.HasValue)
		{
			Vector2 newGrabDest = Custom.RestrictInRect(base.FloatBase, room.TileRect(intVector.Value).Grow(2f));
			SharedPhysics.RayTracedTilesArray(base.FloatBase, room.MiddleOfTile(intVector.Value), segments);
			if (segments.Count > 2)
			{
				segments.RemoveAt(segments.Count - 1);
			}
			List<IntVector2> path = null;
			MoveGrabDest(newGrabDest, ref path);
			for (int i = 0; i < tChunks.Length; i++)
			{
				tChunks[i].Reset();
			}
			attachedAtTip = true;
			smoothedFootingSecurity = 1f;
		}
	}

	public override void Update()
	{
		base.Update();
		smoothedFootingSecurity = Mathf.Lerp(smoothedFootingSecurity, attachedAtTip ? 0f : Mathf.InverseLerp(3f, 30f, sureOfFootingCounter), 0.2f);
		attachedAtTip = false;
		maxLength = 20f * deer.preferredHeight * 1.3333334f;
		idealLength = Mathf.Min(Mathf.Lerp(idealLength, base.grabDest.HasValue ? (Vector2.Distance(base.FloatBase, floatGrabDest.Value) * 1.5f * Mathf.InverseLerp(setGrabDelay, 0f, grabDelay)) : maxLength, 0.03f), maxLength);
		base.retractFac = ((deer.resting == 1f) ? 1f : 0f);
		limp = !deer.Consious;
		if (limp)
		{
			floatGrabDest = null;
			for (int i = 0; i < tChunks.Length; i++)
			{
				tChunks[i].vel *= 0.9f;
				tChunks[i].vel.y -= 0.5f;
			}
		}
		for (int j = 0; j < tChunks.Length; j++)
		{
			tChunks[j].rad = 5f;
			float num = (float)j / (float)(tChunks.Length - 1);
			tChunks[j].vel *= 0.5f;
			if (backtrackFrom == -1 || j < backtrackFrom)
			{
				if (j < tChunks.Length - 1)
				{
					tChunks[j].vel += deer.HeadDir * Mathf.Lerp(-1f, 1f, num) * Mathf.Pow(1f - num, 1.8f);
				}
				tChunks[j].vel += deer.moveDirection * Mathf.Sin(smoothedFootingSecurity * (float)Math.PI) * 1.5f * Mathf.Lerp(1f, 2.5f, Mathf.Sin(num * (float)Math.PI));
			}
			tChunks[j].pos = Vector2.Lerp(tChunks[j].pos, room.MiddleOfTile(segments[tChunks[j].currentSegment]), 0.03f);
			if (room.PointSubmerged(tChunks[j].pos))
			{
				tChunks[j].vel *= 0.5f;
			}
		}
		if (backtrackFrom == -1)
		{
			tChunks[1].vel += deer.moveDirection;
		}
		if (backtrackFrom == -1 && base.grabDest.HasValue)
		{
			base.Tip.vel += Custom.DirVec(base.Tip.pos, floatGrabDest.Value) * Mathf.Lerp(0.2f, 38f, smoothedFootingSecurity);
		}
		if (floatGrabDest.HasValue && Custom.DistLess(base.Tip.pos, floatGrabDest.Value, 40f) && backtrackFrom == -1 && deer.resting < 1f)
		{
			base.Tip.pos = floatGrabDest.Value;
			base.Tip.vel *= 0f;
			attachedAtTip = true;
		}
		base.Tip.collideWithTerrain = !attachedAtTip;
		UpdateDesiredGrabPos();
		for (int k = 0; k < tChunks.Length; k++)
		{
			tChunks[k].vel.y -= 0.1f;
			tChunks[k].vel += connectedChunk.vel * 0.1f;
			if (!attachedAtTip)
			{
				if (floatGrabDest.HasValue)
				{
					tChunks[k].vel += Custom.DirVec(tChunks[k].pos, floatGrabDest.Value) * 0.3f;
				}
				else
				{
					tChunks[k].vel += Custom.DirVec(tChunks[k].pos, desiredGrabPos + Custom.DirVec(base.FloatBase, desiredGrabPos) * 70f) * 0.6f;
				}
			}
		}
		if (attachedAtTip)
		{
			framesWithoutReaching = 0;
			if (SharedPhysics.RayTraceTilesForTerrain(room, base.BasePos, base.grabDest.Value))
			{
				if (!Custom.DistLess(base.Tip.pos, connectedChunk.pos, maxLength))
				{
					ReleaseGrip();
				}
				if (!Custom.DistLess(base.Tip.pos, connectedChunk.pos, maxLength * 0.9f))
				{
					deer.heldBackByLeg = true;
				}
			}
			else
			{
				ReleaseGrip();
			}
			if (playStepSound)
			{
				if (stepSoundVol > 0.5f)
				{
					room.PlaySound(SoundID.Vulture_Tentacle_Grab_Terrain, base.Tip.pos, Mathf.InverseLerp(0.5f, 1f, stepSoundVol) * Mathf.InverseLerp(7f, 45f, Vector2.Distance(base.Tip.pos, base.Tip.lastPos)), 1f);
				}
				playStepSound = false;
			}
			stepSoundVol = 0f;
		}
		else
		{
			stepSoundVol = Mathf.Min(1f, stepSoundVol + 0.025f);
			playStepSound = true;
			FindGrabPos();
			framesWithoutReaching++;
			if ((float)framesWithoutReaching > 60f && !floatGrabDest.HasValue)
			{
				framesWithoutReaching = 0;
			}
		}
		if (debugViz)
		{
			grabGoalSprites[1].pos = desiredGrabPos;
		}
	}

	public void ReleaseGrip()
	{
		if (OtherTentacleInPair.grabDelay < 1 && grabDelay < 1)
		{
			grabDelay = setGrabDelay;
		}
		floatGrabDest = null;
		sureOfFootingCounter = 0;
	}

	private void UpdateDesiredGrabPos()
	{
		if (deer.stayStill)
		{
			desiredGrabPos = connectedChunk.pos + (new Vector2(0f, -1f) + tentacleDir * 0.5f).normalized * Mathf.Min(deer.nextFloorHeight, maxLength * 0.9f);
		}
		else
		{
			desiredGrabPos = connectedChunk.pos + (new Vector2(0f, -1f) + deer.moveDirection * ((pair == 0) ? 0.8f : 0.6f) + tentacleDir * 0.25f).normalized * Mathf.Min(deer.nextFloorHeight, maxLength * 0.9f);
		}
	}

	private void FindGrabPos()
	{
		if (grabDelay > 0)
		{
			grabDelay--;
			return;
		}
		IntVector2? intVector = null;
		for (int i = 0; i < 9; i++)
		{
			if (intVector.HasValue)
			{
				break;
			}
			intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, base.BasePos, room.GetTilePosition(desiredGrabPos) + Custom.eightDirectionsAndZero[i] * 5);
			if (intVector.HasValue && room.GetTile(intVector.Value + new IntVector2(0, 1)).Solid)
			{
				intVector = null;
			}
		}
		if (intVector.HasValue)
		{
			if (!base.grabDest.HasValue || GripPointAttractiveness(intVector.Value) > GripPointAttractiveness(base.grabDest.Value) * (deer.stayStill ? 1f : 3f))
			{
				List<IntVector2> path = null;
				MoveGrabDest(Custom.RestrictInRect(base.FloatBase + tentacleDir * Vector2.Distance(base.FloatBase, room.MiddleOfTile(intVector.Value)) * 0.5f, FloatRect.MakeFromVector2(room.MiddleOfTile(intVector.Value) - new Vector2(11f, 11f), room.MiddleOfTile(intVector.Value) + new Vector2(11f, 11f))), ref path);
				sureOfFootingCounter = 0;
			}
			else
			{
				sureOfFootingCounter++;
			}
		}
	}

	public float Support()
	{
		if (!attachedAtTip)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < 4; i++)
		{
			if (i != tentacleNumber && deer.legs[i].attachedAtTip && deer.legs[i].Tip.pos.x < deer.bodyChunks[2].pos.x != base.Tip.pos.x < deer.bodyChunks[2].pos.x)
			{
				num = Mathf.Max(num, Mathf.Pow(Mathf.Sin(Mathf.InverseLerp(400f, 0f, Mathf.Abs(deer.bodyChunks[2].pos.x - deer.legs[i].Tip.pos.x)) * (float)Math.PI), 0.3f));
			}
		}
		return Mathf.InverseLerp(Mathf.Lerp((ModManager.MMF && MMF.cfgDeerBehavior.Value) ? 0.8f : 0.5f, -1f, num), 1f, Vector2.Dot(new Vector2(0f, -1f), Custom.DirVec(connectedChunk.pos, base.Tip.pos)));
	}

	public float ReleaseScore()
	{
		float num = (base.grabDest.HasValue ? (Vector2.Distance(floatGrabDest.Value, desiredGrabPos) * 2f) : Vector2.Distance(base.Tip.pos, desiredGrabPos));
		if (attachedAtTip)
		{
			num *= 2f;
		}
		if (!OtherTentacleInPair.attachedAtTip)
		{
			num /= 100f;
		}
		num *= 1f + (base.FloatBase.x - base.Tip.pos.x) * Mathf.Sign(deer.moveDirection.x) * 1.4f;
		return num / (1f + Support() * 10f);
	}

	private float GripPointAttractiveness(IntVector2 pos)
	{
		if (!room.GetTile(pos + new IntVector2(0, 1)).Solid)
		{
			return 100f / room.GetTilePosition(desiredGrabPos).FloatDist(pos);
		}
		return 65f / room.GetTilePosition(desiredGrabPos).FloatDist(pos);
	}

	private void PushChunksApartInternal(int a, int b)
	{
		Vector2 vector = Custom.DirVec(tChunks[a].pos, tChunks[b].pos);
		float num = Vector2.Distance(tChunks[a].pos, tChunks[b].pos);
		float num2 = 10f;
		if (num < num2)
		{
			tChunks[a].pos -= vector * (num2 - num) * 0.5f;
			tChunks[a].vel -= vector * (num2 - num) * 0.5f;
			tChunks[b].pos += vector * (num2 - num) * 0.5f;
			tChunks[b].vel += vector * (num2 - num) * 0.5f;
		}
	}

	protected override IntVector2 GravityDirection()
	{
		if (!(UnityEngine.Random.value < 0.5f))
		{
			return new IntVector2(0, -1);
		}
		return new IntVector2((int)Mathf.Sign(tentacleDir.x), -1);
	}
}
