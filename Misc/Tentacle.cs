using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class Tentacle
{
	public struct TentacleProps
	{
		public bool stiff;

		public bool rope;

		public bool shorten;

		public float massDeteriorationPerChunk;

		public float pullAtConnectionChunk;

		public float goalAttractionSpeedTip;

		public float goalAttractionSpeed;

		public float alignToSegmentSpeed;

		public float backtrackSpeed;

		public float chunkVelocityCap;

		public float tileTentacleUpdateSpeed;

		public float tileTentacleSnapWithPathDistance;

		public int segmentPhaseThroughTerrainFrames;

		public int tileTentacleRecordFrames;

		public int maxPullTilesPerTick;

		public int terrainHitsBeforePhase;

		public TentacleProps(bool stiff, bool rope, bool shorten, float massDeteriorationPerChunk, float pullAtConnectionChunk, float goalAttractionSpeedTip, float goalAttractionSpeed, float alignToSegmentSpeed, float backtrackSpeed, float chunkVelocityCap, float tileTentacleUpdateSpeed, float tileTentacleSnapWithPathDistance, int segmentPhaseThroughTerrainFrames, int tileTentacleRecordFrames, int maxPullTilesPerTick, int terrainHitsBeforePhase)
		{
			this.stiff = stiff;
			this.rope = rope;
			this.shorten = shorten;
			this.massDeteriorationPerChunk = massDeteriorationPerChunk;
			this.pullAtConnectionChunk = pullAtConnectionChunk;
			this.goalAttractionSpeedTip = goalAttractionSpeedTip;
			this.goalAttractionSpeed = goalAttractionSpeed;
			this.alignToSegmentSpeed = alignToSegmentSpeed;
			this.backtrackSpeed = backtrackSpeed;
			this.chunkVelocityCap = chunkVelocityCap;
			this.tileTentacleUpdateSpeed = tileTentacleUpdateSpeed;
			this.tileTentacleSnapWithPathDistance = tileTentacleSnapWithPathDistance;
			this.segmentPhaseThroughTerrainFrames = segmentPhaseThroughTerrainFrames;
			this.tileTentacleRecordFrames = tileTentacleRecordFrames;
			this.maxPullTilesPerTick = maxPullTilesPerTick;
			this.terrainHitsBeforePhase = terrainHitsBeforePhase;
		}
	}

	private class PCell
	{
		public IntVector2 pos;

		public int generation;

		public float heuristic;

		public PCell parent;

		public PCell(IntVector2 pos, int generation, float heuristic, PCell parent)
		{
			this.pos = pos;
			this.generation = generation;
			this.heuristic = heuristic;
			this.parent = parent;
		}
	}

	public class TentacleChunk
	{
		public Vector2 pos;

		public Vector2 vel;

		public float rad;

		public Vector2 lastPos;

		public IntVector2 lastContactPoint;

		public IntVector2 contactPoint;

		public Tentacle tentacle;

		public float tPos;

		private DebugSprite[] dbSprites;

		public int tentacleIndex;

		public float lockInPosition;

		public Vector2 phaseFrom;

		public int phasesToSameLocation;

		public IntVector2 afterPhaseStuckPos;

		public float phase;

		public List<IntVector2> currentSegmentTrail;

		public bool collideWithTerrain;

		public int phaseAttempts;

		public float stretchedFac;

		public Rope rope;

		public int currentSegment => Custom.IntClamp((int)(tPos * (float)(tentacle.segments.Count - 1)), 0, tentacle.segments.Count - 1);

		public IntVector2 StuckPos => tentacle.segments[currentSegment];

		public FloatRect StuckRect => FloatRect.MakeFromVector2(tentacle.room.MiddleOfTile(StuckPos) - new Vector2(9f, 9f), tentacle.room.MiddleOfTile(StuckPos) + new Vector2(9f, 9f));

		public TentacleProps tp => tentacle.tProps;

		public float stretchedRad => rad * Mathf.Clamp(Mathf.Pow(stretchedFac, tentacle.stretchAndSqueeze), 0.5f, 1.5f);

		public bool RopeActive
		{
			get
			{
				if (rope != null && phase == -1f && (tentacleIndex == 0 || tentacle.tChunks[tentacleIndex - 1].phase == -1f))
				{
					if (tentacle.backtrackFrom >= tentacleIndex)
					{
						return tentacle.backtrackFrom == -1;
					}
					return true;
				}
				return false;
			}
		}

		public TentacleChunk(Tentacle tentacle, int tentacleIndex, float tPos, float rad)
		{
			this.tentacle = tentacle;
			this.tPos = tPos;
			this.rad = rad;
			this.tentacleIndex = tentacleIndex;
			collideWithTerrain = true;
		}

		public void Reset()
		{
			currentSegmentTrail = new List<IntVector2>();
			for (int i = 0; i < tp.tileTentacleRecordFrames; i++)
			{
				currentSegmentTrail.Add(StuckPos);
			}
			phase = -1f;
			pos = tentacle.room.MiddleOfTile(StuckPos);
			lastPos = pos;
			vel = new Vector2(0f, 0f);
			if (tentacle.tProps.rope)
			{
				rope = new Rope(tentacle.room, tentacle.FloatBase, tentacle.FloatBase + new Vector2(1f, 1f), 1f);
			}
		}

		public void Update()
		{
			lastPos = pos;
			if (rope != null)
			{
				rope.Update((tentacleIndex == 0) ? tentacle.FloatBase : tentacle.tChunks[tentacleIndex - 1].pos, pos);
				if (!RopeActive || phase > -1f || (tentacleIndex > 0 && tentacle.tChunks[tentacleIndex - 1].phase > -1f) || rope.totalLength > tentacle.idealLength / (float)tentacle.tChunks.Length * 5f)
				{
					rope.Reset();
				}
			}
			if (phase == -1f)
			{
				if (phasesToSameLocation > 0 && tentacle.segments[currentSegment] == afterPhaseStuckPos)
				{
					pos = Custom.RestrictInRect(pos, StuckRect);
				}
				if (tentacleIndex == 0)
				{
					Vector2 vector = Custom.DirVec(pos, tentacle.FloatBase);
					float num = Vector2.Distance(pos, tentacle.FloatBase);
					if (RopeActive)
					{
						vector = Custom.DirVec(pos, rope.BConnect);
						num = rope.totalLength;
					}
					float num2 = tentacle.idealLength / (float)tentacle.tChunks.Length * Mathf.Lerp(1f, 0.1f, tentacle.retractFac);
					if (tp.stiff || num > num2)
					{
						pos -= vector * (num2 - num) * (1f - tp.pullAtConnectionChunk);
						vel -= vector * (num2 - num) * (1f - tp.pullAtConnectionChunk);
						if (tentacle.connectedChunk != null && tp.pullAtConnectionChunk > 0f)
						{
							if (RopeActive)
							{
								vector = Custom.DirVec(rope.AConnect, tentacle.connectedChunk.pos);
							}
							tentacle.connectedChunk.pos += vector * (num2 - num) * tp.pullAtConnectionChunk;
							tentacle.connectedChunk.vel += vector * (num2 - num) * tp.pullAtConnectionChunk;
						}
					}
					stretchedFac = tentacle.idealLength / (float)tentacle.tChunks.Length / Mathf.Max(1f, num);
				}
				else
				{
					Vector2 vector2 = Custom.DirVec(pos, tentacle.tChunks[tentacleIndex - 1].pos);
					float num3 = Vector2.Distance(pos, tentacle.tChunks[tentacleIndex - 1].pos);
					if (RopeActive)
					{
						vector2 = Custom.DirVec(pos, rope.BConnect);
						num3 = rope.totalLength;
					}
					float num4 = tentacle.idealLength / (float)tentacle.tChunks.Length * Mathf.Lerp(1f, 0.1f, tentacle.retractFac);
					if (tp.stiff || num3 > num4)
					{
						pos -= vector2 * (num4 - num3) * (1f - tp.massDeteriorationPerChunk);
						vel -= vector2 * (num4 - num3) * (1f - tp.massDeteriorationPerChunk);
						if (RopeActive)
						{
							vector2 = Custom.DirVec(rope.AConnect, tentacle.tChunks[tentacleIndex - 1].pos);
						}
						tentacle.tChunks[tentacleIndex - 1].pos += vector2 * (num4 - num3) * tp.massDeteriorationPerChunk;
						tentacle.tChunks[tentacleIndex - 1].vel += vector2 * (num4 - num3) * tp.massDeteriorationPerChunk;
					}
					stretchedFac = tentacle.idealLength / (float)tentacle.tChunks.Length / Mathf.Max(1f, num3);
				}
				if (StuckPos != currentSegmentTrail[0])
				{
					currentSegmentTrail.Insert(0, StuckPos);
					currentSegmentTrail.RemoveAt(currentSegmentTrail.Count - 1);
				}
				if (tentacle.backtrackFrom == -1 && !tentacle.Visual(tentacle.room.GetTilePosition(pos), tentacle.segments[currentSegment]))
				{
					tentacle.backtrackFrom = tentacleIndex;
				}
				if (tentacle.backtrackFrom != -1 && tentacleIndex >= tentacle.backtrackFrom)
				{
					bool flag = false;
					for (int i = 1; i <= currentSegment; i++)
					{
						if (flag)
						{
							break;
						}
						if (!SharedPhysics.RayTraceTilesForTerrain(tentacle.room, tentacle.room.GetTilePosition(pos), tentacle.segments[i]) && SharedPhysics.RayTraceTilesForTerrain(tentacle.room, tentacle.room.GetTilePosition(pos), tentacle.segments[i - 1]))
						{
							vel += Vector2.ClampMagnitude(tentacle.room.MiddleOfTile(tentacle.segments[i - 1]) - pos, 20f) * tp.backtrackSpeed / 20f;
							flag = true;
						}
					}
					if (!flag)
					{
						Vector2 floatBase = tentacle.FloatBase;
						if (tentacleIndex > 0)
						{
							floatBase = tentacle.tChunks[tentacleIndex - 1].pos;
						}
						vel += Vector2.ClampMagnitude(floatBase - pos, 20f) * tp.backtrackSpeed / 20f;
						if (tentacleIndex > 0)
						{
							tentacle.tChunks[tentacleIndex - 1].vel += Vector2.ClampMagnitude(floatBase - pos, 20f) * tp.backtrackSpeed / 20f;
						}
					}
					if (!flag && (contactPoint.x != 0 || contactPoint.y != 0))
					{
						phaseAttempts++;
						if (phaseAttempts > tentacle.tProps.terrainHitsBeforePhase)
						{
							PhaseToSegment();
							phaseAttempts = 0;
						}
					}
				}
				else
				{
					phaseAttempts = 0;
					if (tentacle.limp)
					{
						for (int num5 = currentSegment; num5 >= 0; num5--)
						{
							if (tentacle.Visual(tentacle.room.GetTilePosition(pos), tentacle.segments[num5]))
							{
								if (tentacle.room.GetTile(tentacle.segments[num5].x, tentacle.segments[num5].y - 1).Solid)
								{
									vel += Vector2.ClampMagnitude(tentacle.room.MiddleOfTile(tentacle.segments[num5].x, tentacle.segments[num5].y) - pos, 20f) * 0.005f;
								}
								else
								{
									vel += Vector2.ClampMagnitude(tentacle.room.MiddleOfTile(tentacle.segments[num5]) - pos, 20f) * 0.005f;
								}
								break;
							}
						}
					}
					else
					{
						if (tentacle.floatGrabDest.HasValue)
						{
							if (tentacleIndex == tentacle.tChunks.Length - 1)
							{
								vel += Vector2.ClampMagnitude(tentacle.floatGrabDest.Value - pos, 20f) * tp.goalAttractionSpeedTip / 20f;
							}
							else
							{
								vel += Vector2.ClampMagnitude(tentacle.floatGrabDest.Value - pos, 20f) * tp.goalAttractionSpeed / 20f;
							}
						}
						vel += Vector2.ClampMagnitude(tentacle.room.MiddleOfTile(tentacle.segments[currentSegment]) - pos, 20f) * tp.alignToSegmentSpeed / 20f;
					}
				}
				vel = Vector2.ClampMagnitude(vel, tp.chunkVelocityCap);
				pos += vel;
				if (collideWithTerrain)
				{
					lastContactPoint = contactPoint;
					SharedPhysics.TerrainCollisionData cd2 = SharedPhysics.VerticalCollision(cd: new SharedPhysics.TerrainCollisionData(pos, pos - vel, vel, rad, new IntVector2(0, 0), goThroughFloors: true), room: tentacle.room);
					cd2 = SharedPhysics.SlopesVertically(tentacle.room, cd2);
					cd2 = SharedPhysics.HorizontalCollision(tentacle.room, cd2);
					contactPoint = cd2.contactPoint;
					pos = cd2.pos;
					vel = cd2.vel;
				}
			}
			else
			{
				phase += 1f / (float)tp.segmentPhaseThroughTerrainFrames;
				if (phase <= 1f)
				{
					pos = Vector2.Lerp(phaseFrom, tentacle.room.MiddleOfTile(StuckPos), Mathf.Min(1f, phase));
				}
				vel *= 0f;
				if (phase >= 1f)
				{
					phase = -1f;
					if (afterPhaseStuckPos == StuckPos)
					{
						phasesToSameLocation++;
					}
					else
					{
						phasesToSameLocation = 0;
					}
					afterPhaseStuckPos = StuckPos;
				}
				if (tentacleIndex > 0 && tentacle.tChunks[tentacleIndex - 1].phase == -1f)
				{
					Vector2 vector3 = Custom.DirVec(pos, tentacle.tChunks[tentacleIndex - 1].pos);
					float num6 = Vector2.Distance(pos, tentacle.tChunks[tentacleIndex - 1].pos);
					float num7 = tentacle.idealLength / (float)tentacle.tChunks.Length * Mathf.Lerp(1f, 0.1f, tentacle.retractFac);
					if (tp.stiff || num6 > num7)
					{
						tentacle.tChunks[tentacleIndex - 1].pos += vector3 * (num7 - num6) * tp.massDeteriorationPerChunk;
						tentacle.tChunks[tentacleIndex - 1].vel += vector3 * (num7 - num6) * tp.massDeteriorationPerChunk;
					}
					stretchedFac = num7 / num6;
				}
			}
			UpdateDebugSprites();
		}

		public void PhaseToSegment()
		{
			phaseFrom = pos;
			phase = 0f;
		}

		public void CreateDebugSprites()
		{
			if (dbSprites != null)
			{
				for (int i = 0; i < 1; i++)
				{
					dbSprites[i].RemoveFromRoom();
				}
			}
			if (tentacle.debugViz)
			{
				dbSprites = new DebugSprite[3];
				dbSprites[0] = new DebugSprite(pos, new FSprite("Circle20"), tentacle.room);
				dbSprites[0].sprite.scale = 0.5f;
				dbSprites[0].sprite.alpha = 1f;
				dbSprites[1] = new DebugSprite(pos, new FSprite("pixel"), tentacle.room);
				dbSprites[1].sprite.color = new Color(1f, 0f, 1f);
				dbSprites[1].sprite.alpha = 0.3f;
				dbSprites[1].sprite.anchorY = 0f;
				dbSprites[2] = new DebugSprite(pos, new FSprite("pixel"), tentacle.room);
				dbSprites[2].sprite.alpha = 1f;
				dbSprites[2].sprite.anchorY = 0f;
				dbSprites[2].sprite.scaleX = 2f;
				for (int j = 0; j < 3; j++)
				{
					tentacle.room.AddObject(dbSprites[j]);
				}
			}
		}

		private void UpdateDebugSprites()
		{
			if (dbSprites != null)
			{
				dbSprites[0].pos.x = pos.x;
				dbSprites[0].pos.y = pos.y;
				dbSprites[1].pos.x = pos.x;
				dbSprites[1].pos.y = pos.y;
				float num = (float)tentacleIndex / (float)tentacle.tChunks.Length * 0.7f;
				num -= Mathf.Floor(num);
				if (tentacle.backtrackFrom > -1 && tentacleIndex >= tentacle.backtrackFrom)
				{
					num = 0f;
				}
				dbSprites[0].sprite.color = Custom.HSL2RGB(num, 1f, 0.5f);
				dbSprites[2].sprite.color = Custom.HSL2RGB(num, 1f, 0.5f);
				dbSprites[2].pos.x = pos.x;
				dbSprites[2].pos.y = pos.y;
				Vector2 floatBase = tentacle.FloatBase;
				if (tentacleIndex > 0)
				{
					floatBase = tentacle.tChunks[tentacleIndex - 1].pos;
				}
				dbSprites[2].sprite.scaleY = Vector2.Distance(pos, floatBase);
				dbSprites[2].sprite.rotation = Custom.AimFromOneVectorToAnother(pos, floatBase);
			}
		}
	}

	public bool debugViz;

	public List<DebugSprite> sprites;

	public List<DebugSprite> grabPathSprites;

	private float updateCounter;

	public int pullCounter;

	public Vector2? floatGrabDest;

	public List<IntVector2> grabPath;

	public List<IntVector2> segments = new List<IntVector2>(10);

	public Room room;

	public float idealLength;

	public TentacleChunk[] tChunks;

	public PhysicalObject owner;

	public int pullsThisTick;

	public BodyChunk connectedChunk;

	public float stretchAndSqueeze = 0.5f;

	public float goForGoalPower;

	public int backtrackFrom;

	private float rf;

	public bool limp;

	private List<IntVector2> scratchPath;

	public TentacleProps tProps;

	public IntVector2? grabDest
	{
		get
		{
			if (floatGrabDest.HasValue)
			{
				return room.GetTilePosition(floatGrabDest.Value);
			}
			return null;
		}
	}

	public TentacleChunk Tip => tChunks[tChunks.Length - 1];

	public IntVector2 BasePos
	{
		get
		{
			CheckIfAnySegmentsLeft();
			return segments[0];
		}
	}

	public float TotalRope
	{
		get
		{
			float num = 0f;
			for (int i = 0; i < tChunks.Length; i++)
			{
				num += tChunks[i].rope.totalLength;
			}
			return num;
		}
	}

	public Vector2 FloatBase
	{
		get
		{
			if (connectedChunk == null)
			{
				return owner.room.MiddleOfTile(BasePos);
			}
			return connectedChunk.pos;
		}
	}

	public float retractFac
	{
		get
		{
			return rf;
		}
		set
		{
			rf = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public Tentacle(PhysicalObject owner, BodyChunk connectedChunk, float length)
	{
		this.owner = owner;
		idealLength = length;
		this.connectedChunk = connectedChunk;
	}

	public virtual void NewRoom(Room room)
	{
		this.room = room;
		segments = new List<IntVector2>();
		for (int i = 0; i < (int)(idealLength / 20f); i++)
		{
			segments.Add(room.GetTilePosition(owner.firstChunk.pos));
		}
		for (int j = 0; j < tChunks.Length; j++)
		{
			tChunks[j].Reset();
		}
		grabPath = new List<IntVector2>();
		if (debugViz)
		{
			for (int k = 0; k < tChunks.Length; k++)
			{
				tChunks[k].CreateDebugSprites();
			}
		}
	}

	public virtual void Update()
	{
		if (connectedChunk != null)
		{
			MoveBase(owner.room.GetTilePosition(connectedChunk.pos), ref scratchPath);
		}
		updateCounter += tProps.tileTentacleUpdateSpeed;
		if (updateCounter >= 1f)
		{
			pullsThisTick = tProps.maxPullTilesPerTick;
			if (limp)
			{
				Gravity();
			}
			else if (grabPath.Count > 0)
			{
				AlignWithGrabPath(ref scratchPath);
			}
			AdjustLength(ref scratchPath);
			updateCounter -= 1f;
		}
		goForGoalPower = 1f;
		backtrackFrom = -1;
		for (int i = 0; i < tChunks.Length; i++)
		{
			tChunks[i].Update();
		}
	}

	private void AlignWithGrabPath(ref List<IntVector2> path)
	{
		bool flag = false;
		for (int num = Math.Min(segments.Count - 2, grabPath.Count - 1); num > 0; num--)
		{
			if (segments[num] != grabPath[num] && Visual(segments[num], grabPath[num]))
			{
				room.RayTraceTilesList(segments[num].x, segments[num].y, grabPath[num].x, grabPath[num].y, ref path);
				if (MoveSegment(num + 1, path[1], allowMovingToOccupiedSpace: false, allowSolidTile: false))
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			return;
		}
		for (int i = 1; i < segments.Count && i < grabPath.Count; i++)
		{
			if (segments[i] != grabPath[i])
			{
				MoveSegment(i, grabPath[i], allowMovingToOccupiedSpace: true, allowSolidTile: false);
				break;
			}
		}
	}

	public void MoveGrabDest(Vector2 newGrabDest, ref List<IntVector2> path)
	{
		if (!grabDest.HasValue || !(grabDest.Value == room.GetTilePosition(newGrabDest)))
		{
			floatGrabDest = newGrabDest;
			FindGrabPath(ref path);
		}
	}

	protected bool Visual(IntVector2 a, IntVector2 b)
	{
		return room.RayTraceTilesForTerrain(a.x, a.y, b.x, b.y);
	}

	private bool IsOccuppiedByTChunk(int segment, int ignoreSegment)
	{
		for (int i = 0; i < tChunks.Length; i++)
		{
			if (tChunks[i].currentSegment == segment)
			{
				return true;
			}
		}
		return false;
	}

	private void FindGrabPath(ref List<IntVector2> path)
	{
		int num = ((room.IsPositionInsideBoundries(grabDest.Value) && room.IsPositionInsideBoundries(BasePos) && !Visual(BasePos, grabDest.Value)) ? FindTentaclePath(BasePos, grabDest.Value, ref grabPath) : room.RayTraceTilesList(BasePos.x, BasePos.y, grabDest.Value.x, grabDest.Value.y, ref grabPath));
		while (grabPath.Count > num)
		{
			grabPath.RemoveAt(grabPath.Count - 1);
		}
		MoveAlignedSegmentsWithPath(ref path);
		UpdateGrabPathDebugSprites();
	}

	private void MoveAlignedSegmentsWithPath(ref List<IntVector2> path)
	{
		int num = Math.Min(segments.Count - 1, grabPath.Count - 1);
		for (int i = 0; i < Math.Min(segments.Count - 1, grabPath.Count - 1) && !(segments[i + 1].FloatDist(grabPath[i + 1]) > tProps.tileTentacleSnapWithPathDistance) && Visual(segments[i + 1], grabPath[i + 1]); i++)
		{
			num = i + 1;
			segments[i] = grabPath[i];
		}
		int num2 = room.RayTraceTilesList(grabPath[num].x, grabPath[num].y, segments[num].x, segments[num].y, ref path);
		for (int num3 = num2 - 2; num3 >= 0; num3--)
		{
			segments.Insert(num, path[num3]);
		}
		pullCounter += num2 - 1;
		PullAtTentacle(num, onlySimplePull: true, ref path);
		UpdateDebugSprites();
	}

	public void MoveBase(IntVector2 newPos, ref List<IntVector2> path)
	{
		if (room == null || room.GetTile(newPos).Solid || newPos == BasePos)
		{
			return;
		}
		if (Custom.AreIntVectorsNeighbors(segments[0], newPos))
		{
			int num = -1;
			for (int i = 1; i < segments.Count - 1; i++)
			{
				if (segments[i] == newPos)
				{
					num = i;
				}
			}
			if (num > 0)
			{
				segments.RemoveRange(0, num);
				Grow(num);
			}
			else
			{
				PrivateMoveBase(newPos - segments[0]);
				pullCounter++;
				if (pullCounter > 4)
				{
					PullAtTentacle(0, onlySimplePull: false, ref path);
				}
			}
		}
		else
		{
			int num2 = ((room.IsPositionInsideBoundries(segments[0]) && room.IsPositionInsideBoundries(newPos) && !Visual(segments[0], newPos)) ? FindTentaclePath(segments[0], newPos, ref path) : room.RayTraceTilesList(segments[0].x, segments[0].y, newPos.x, newPos.y, ref path));
			IntVector2 intVector = segments[0];
			for (int j = 1; j < num2; j++)
			{
				PrivateMoveBase(path[j] - intVector);
				intVector = path[j];
			}
			pullCounter += num2 - 1;
			if (pullCounter > 4)
			{
				PullAtTentacle(0, onlySimplePull: false, ref path);
			}
		}
		if (grabDest.HasValue)
		{
			FindGrabPath(ref path);
			UpdateGrabPathDebugSprites();
		}
		UpdateDebugSprites();
	}

	private void PrivateMoveBase(IntVector2 movement)
	{
		segments.Insert(0, segments[0] + movement);
	}

	private void Detour(int segment, IntVector2 newPos)
	{
		if (Visual(segments[segment], newPos))
		{
			_ = segments.Count;
			int num = segment;
			while (num > 1 && !IsOccuppiedByTChunk(num - 1, segment) && Visual(newPos, segments[num - 1]))
			{
				num--;
			}
			int i;
			for (i = segment; i < segments.Count - 1 && !IsOccuppiedByTChunk(i + 1, segment) && Visual(newPos, segments[i + 1]); i++)
			{
			}
			List<IntVector2> path = new List<IntVector2>();
			List<IntVector2> path2 = new List<IntVector2>();
			room.RayTraceTilesList(segments[num].x, segments[num].y, newPos.x, newPos.y, ref path);
			room.RayTraceTilesList(newPos.x, newPos.y, segments[i].x, segments[i].y, ref path2);
			segments.RemoveRange(num, i - num);
			for (int num2 = path2.Count - 2; num2 > 0; num2--)
			{
				segments.Insert(num, path2[num2]);
			}
			for (int num3 = path.Count - 1; num3 >= 0; num3--)
			{
				segments.Insert(num, path[num3]);
			}
			UpdateDebugSprites();
		}
	}

	private void PullAtTentacle(int startPullingPoint, bool onlySimplePull, ref List<IntVector2> path)
	{
		if (!onlySimplePull)
		{
			while (pullCounter > 0)
			{
				bool flag = false;
				for (int i = startPullingPoint; i < segments.Count - 3; i++)
				{
					if (flag)
					{
						break;
					}
					for (int num = segments.Count - 1; num >= i + 3; num--)
					{
						int num2 = num - i - Custom.ManhattanDistance(segments[i], segments[num]);
						if (num2 > 0 && num2 <= pullCounter && Visual(segments[i], segments[num]))
						{
							int num3 = room.RayTraceTilesList(segments[i].x, segments[i].y, segments[num].x, segments[num].y, ref path);
							bool flag2 = true;
							if (num2 > 1 && num2 <= pullsThisTick)
							{
								for (int j = 1; j < num3 - 1; j++)
								{
									for (int k = i + 1; k < num; k++)
									{
										if (Custom.ManhattanDistance(segments[k], path[j]) > 2 && !Visual(segments[k], path[j]))
										{
											flag2 = false;
											break;
										}
									}
								}
							}
							if (flag2)
							{
								pullsThisTick -= num2;
								pullCounter -= num2;
								Shorten(i, num, path, num3);
								flag = true;
								break;
							}
						}
					}
				}
				if (!flag)
				{
					break;
				}
			}
		}
		while (pullCounter > 0 && pullsThisTick > 0 && segments.Count > 1)
		{
			segments.RemoveAt(segments.Count - 1);
			pullCounter--;
			pullsThisTick--;
		}
		UpdateDebugSprites();
	}

	private void Shorten(int from, int to, List<IntVector2> straightPath, int straightPathCount)
	{
		segments.RemoveRange(from + 1, to - from - 1);
		for (int num = straightPathCount - 2; num > 0; num--)
		{
			segments.Insert(from + 1, straightPath[num]);
		}
		UpdateDebugSprites();
	}

	private bool MoveSegment(int s, IntVector2 dest, bool allowMovingToOccupiedSpace, bool allowSolidTile)
	{
		if (s == 0 || segments[s] == dest || (!Custom.AreIntVectorsNeighbors(segments[s - 1], dest) && segments[s - 1] != dest) || (!allowSolidTile && room.GetTile(dest).Solid))
		{
			return false;
		}
		if (dest == segments[s - 1])
		{
			if (!allowMovingToOccupiedSpace)
			{
				return false;
			}
			segments.Insert(s, dest);
			segments.RemoveAt(segments.Count - 1);
		}
		else if (Math.Abs(segments[s].x - dest.x) > 1 || Math.Abs(segments[s].y - dest.y) > 1)
		{
			if (!allowMovingToOccupiedSpace)
			{
				return false;
			}
			segments.Insert(s, segments[s - 1]);
			segments.Insert(s, dest);
			segments.RemoveAt(segments.Count - 1);
			segments.RemoveAt(segments.Count - 1);
		}
		else
		{
			if (!allowMovingToOccupiedSpace)
			{
				for (int i = 0; i < segments.Count; i++)
				{
					if (segments[i] == dest)
					{
						return false;
					}
				}
			}
			List<IntVector2> list = new List<IntVector2>();
			list.Add(new IntVector2(segments[s].x, dest.y));
			list.Add(new IntVector2(dest.x, segments[s].y));
			IntVector2 intVector = list[0];
			float num = float.MaxValue;
			for (int j = 0; j < list.Count; j++)
			{
				float num2 = 1f;
				if (room.GetTile(list[j]).Solid)
				{
					num2 += 1000f;
				}
				for (int k = 0; k < segments.Count; k++)
				{
					if (list[j] == segments[k])
					{
						num2 += 1f;
					}
				}
				if (num2 < num)
				{
					intVector = list[j];
					num = num2;
				}
			}
			if (!allowMovingToOccupiedSpace)
			{
				for (int l = 0; l < segments.Count; l++)
				{
					if (segments[l] == intVector)
					{
						return false;
					}
				}
			}
			segments.Insert(s, intVector);
			segments.Insert(s, dest);
			segments.RemoveAt(segments.Count - 1);
			segments.RemoveAt(segments.Count - 1);
		}
		UpdateDebugSprites();
		return true;
	}

	private void CheckIfAnySegmentsLeft()
	{
		if (segments.Count <= 0)
		{
			if (connectedChunk != null)
			{
				segments.Add(room.GetTilePosition(connectedChunk.pos));
			}
			else
			{
				segments.Add(room.GetTilePosition(owner.firstChunk.pos));
			}
		}
	}

	private void AdjustLength(ref List<IntVector2> path)
	{
		float num = CurrentLength();
		float num2 = idealLength;
		if (tProps.shorten && grabPath.Count > 0)
		{
			num2 = Mathf.Min(idealLength, (float)grabPath.Count * 20f);
		}
		int num3 = (int)(num2 / 20f - num / 20f);
		if (num3 > 0 && (grabPath.Count == 0 || segments.Count < grabPath.Count))
		{
			Grow(1);
		}
		else if (num3 < 0)
		{
			pullCounter++;
			PullAtTentacle(0, onlySimplePull: false, ref path);
		}
	}

	private void Grow(int add)
	{
		for (int i = 0; i < add; i++)
		{
			float num = float.MaxValue;
			IntVector2 item = segments[segments.Count - 1];
			for (int j = 0; j < 5; j++)
			{
				float num2 = UnityEngine.Random.value;
				IntVector2 intVector = segments[segments.Count - 1] + Custom.fourDirectionsAndZero[j];
				if (room.GetTile(intVector).Solid)
				{
					num2 += 10000f;
				}
				for (int k = 0; k < segments.Count; k++)
				{
					if (segments[k] == intVector)
					{
						num2 += 1f;
					}
				}
				if (num2 < num)
				{
					num = num2;
					item = intVector;
				}
			}
			segments.Add(item);
		}
		UpdateDebugSprites();
	}

	public float CurrentLength()
	{
		float num = (float)segments.Count * 20f;
		for (int i = 0; i < segments.Count - 2; i++)
		{
			if (Custom.AreIntVectorsDiagonalNeighbors(segments[i], segments[i + 2]))
			{
				num -= 5.9000006f;
			}
		}
		return num;
	}

	public void Reset(Vector2 resetPos)
	{
		if (room != null)
		{
			segments = new List<IntVector2> { room.GetTilePosition(resetPos) };
			for (int i = 0; i < tChunks.Length; i++)
			{
				tChunks[i].pos = resetPos;
				tChunks[i].vel *= 0f;
				tChunks[i].Reset();
			}
			UpdateDebugSprites();
		}
	}

	private void Gravity()
	{
		IntVector2 intVector = GravityDirection();
		bool flag = false;
		for (int i = 0; i < segments.Count; i++)
		{
			if (flag)
			{
				break;
			}
			if (MoveSegment(i, segments[i] + intVector, allowMovingToOccupiedSpace: false, allowSolidTile: false))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			for (int j = 0; j < segments.Count && !MoveSegment(j, segments[j] + intVector, allowMovingToOccupiedSpace: true, allowSolidTile: false); j++)
			{
			}
		}
	}

	protected virtual IntVector2 GravityDirection()
	{
		if (!(UnityEngine.Random.value < 0.5f))
		{
			return new IntVector2(0, -1);
		}
		return new IntVector2((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1), -1);
	}

	private int FindTentaclePath(IntVector2 start, IntVector2 goal, ref List<IntVector2> path)
	{
		List<PCell> list = new List<PCell>
		{
			new PCell(goal, 0, 0f, null)
		};
		bool[,] array = new bool[room.TileWidth, room.TileHeight];
		PCell pCell = null;
		while (list.Count > 0 && pCell == null)
		{
			PCell pCell2 = list[0];
			float num = float.MaxValue;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].heuristic < num)
				{
					pCell2 = list[i];
					num = list[i].heuristic;
				}
			}
			list.Remove(pCell2);
			for (int j = 0; j < 4; j++)
			{
				if (pCell2.pos.x + Custom.fourDirections[j].x >= 0 && pCell2.pos.x + Custom.fourDirections[j].x < room.TileWidth && pCell2.pos.y + Custom.fourDirections[j].y >= 0 && pCell2.pos.y + Custom.fourDirections[j].y < room.TileHeight && !array[pCell2.pos.x + Custom.fourDirections[j].x, pCell2.pos.y + Custom.fourDirections[j].y])
				{
					float num2 = Vector2.Distance(IntVector2.ToVector2(pCell2.pos + Custom.fourDirections[j]), IntVector2.ToVector2(start));
					num2 += (room.GetTile(pCell2.pos + Custom.fourDirections[j]).Solid ? 1000f : 0f);
					PCell pCell3 = new PCell(pCell2.pos + Custom.fourDirections[j], pCell2.generation + 1, num2, pCell2);
					array[pCell2.pos.x + Custom.fourDirections[j].x, pCell2.pos.y + Custom.fourDirections[j].y] = true;
					if (pCell3.pos == start)
					{
						pCell = pCell3;
					}
					else
					{
						list.Add(pCell3);
					}
				}
			}
		}
		if (path == null)
		{
			path = new List<IntVector2>();
		}
		int num3 = 0;
		while (pCell.parent != null)
		{
			if (path.Count <= num3)
			{
				path.Add(default(IntVector2));
			}
			path[num3] = pCell.pos;
			num3++;
			pCell = pCell.parent;
		}
		return num3;
	}

	protected void PushChunksApart(int a, int b)
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

	private void UpdateGrabPathDebugSprites()
	{
		if (grabPathSprites != null && !debugViz)
		{
			for (int i = 0; i < grabPathSprites.Count; i++)
			{
				grabPathSprites[i].RemoveFromRoom();
			}
		}
		if (!debugViz)
		{
			return;
		}
		if (grabPathSprites == null)
		{
			grabPathSprites = new List<DebugSprite>();
		}
		if (grabPathSprites.Count > grabPath.Count)
		{
			int num = grabPathSprites.Count - grabPath.Count;
			for (int j = 0; j < num; j++)
			{
				grabPathSprites[grabPathSprites.Count - 1].RemoveFromRoom();
				grabPathSprites.RemoveAt(grabPathSprites.Count - 1);
			}
		}
		if (grabPathSprites.Count < grabPath.Count)
		{
			int num2 = grabPath.Count - grabPathSprites.Count;
			for (int k = 0; k < num2; k++)
			{
				grabPathSprites.Add(new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel"), room));
				room.AddObject(grabPathSprites[grabPathSprites.Count - 1]);
			}
		}
		for (int l = 0; l < grabPathSprites.Count; l++)
		{
			grabPathSprites[l].pos = room.MiddleOfTile(grabPath[l]);
			grabPathSprites[l].sprite.color = new Color(1f, 0f, 0f);
			if (l < grabPathSprites.Count - 1)
			{
				grabPathSprites[l].sprite.scaleX = 1f;
				grabPathSprites[l].sprite.scaleY = 20f;
				grabPathSprites[l].sprite.anchorY = 0f;
				grabPathSprites[l].sprite.rotation = Custom.AimFromOneVectorToAnother(room.MiddleOfTile(grabPath[l]), room.MiddleOfTile(grabPath[l + 1]));
			}
			else
			{
				grabPathSprites[l].sprite.scale = 5f;
			}
		}
	}

	private void CreateDebugSprites()
	{
		ClearDebugSprites();
	}

	private void ClearDebugSprites()
	{
		if (sprites != null)
		{
			for (int i = 0; i < sprites.Count; i++)
			{
				sprites[i].RemoveFromRoom();
			}
		}
	}

	private void UpdateDebugSprites()
	{
		if (debugViz)
		{
			if (sprites == null)
			{
				sprites = new List<DebugSprite>();
			}
			if (sprites.Count > segments.Count)
			{
				int num = sprites.Count - segments.Count;
				for (int i = 0; i < num; i++)
				{
					sprites[sprites.Count - 1].RemoveFromRoom();
					sprites.RemoveAt(sprites.Count - 1);
				}
			}
			if (sprites.Count < segments.Count)
			{
				int num2 = segments.Count - sprites.Count;
				for (int j = 0; j < num2; j++)
				{
					sprites.Add(new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel"), room));
					room.AddObject(sprites[sprites.Count - 1]);
				}
			}
			for (int k = 0; k < segments.Count; k++)
			{
				float num3 = (float)k / (float)segments.Count * 0.7f;
				num3 -= Mathf.Floor(num3);
				sprites[k].sprite.color = Custom.HSL2RGB(num3, 1f, 0.5f);
				if (k < segments.Count - 1)
				{
					sprites[k].sprite.scaleX = 12f;
					sprites[k].sprite.scaleY = 18f;
					sprites[k].sprite.anchorY = 0f;
					sprites[k].sprite.rotation = Custom.AimFromOneVectorToAnother(room.MiddleOfTile(segments[k]), room.MiddleOfTile(segments[k + 1]));
				}
				else
				{
					sprites[k].sprite.scale = 14f;
				}
				sprites[k].pos = room.MiddleOfTile(segments[k]);
				sprites[k].sprite.alpha = ((k < segments.Count - 1) ? 0.2f : 0.5f);
			}
		}
		else
		{
			ClearDebugSprites();
		}
	}
}
