using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class LanternMouse : AirBreatherCreature, IProvideWarmth
{
	public struct IndividualVariations
	{
		public float dominance;

		public HSLColor color;

		public IndividualVariations(float dominance, HSLColor color)
		{
			this.dominance = dominance;
			this.color = color;
		}
	}

	public MouseAI AI;

	private int footingCounter;

	public int specialMoveCounter;

	public IntVector2 specialMoveDestination;

	private MovementConnection lastFollowedConnection;

	public float runSpeed;

	public Vector2? ropeAttatchedPos;

	public Vector2 lastRopeClearPos;

	public float ropeLength;

	private int ropeInvincible;

	public float runCycle;

	public bool currentlyClimbingCorridor;

	public List<Vector2> ropeBends;

	public bool shrinkingRope;

	public float swingFac;

	public float swingFacGetTo;

	public float profileFac;

	public float ropeStretch;

	public bool sitting;

	public float fallAsleep;

	public float wakeUp;

	public bool carried;

	private int struggleCountdownA;

	private int struggleCountdownB;

	public int voiceCounter;

	public bool controlSitting;

	public IndividualVariations iVars;

	private IntVector2[] _cachedRay = new IntVector2[100];

	private IntVector2[] _cachedRay2 = new IntVector2[100];

	public new MouseState State => base.abstractCreature.state as MouseState;

	public bool Footing => footingCounter > 20;

	public bool Sleeping
	{
		get
		{
			if (ropeAttatchedPos.HasValue)
			{
				return fallAsleep >= 1f;
			}
			return false;
		}
	}

	float IProvideWarmth.warmth => RainWorldGame.DefaultHeatSourceWarmth * 1.5f * Mathf.InverseLerp(0f, 3500f, State.battery);

	Room IProvideWarmth.loadedRoom => room;

	float IProvideWarmth.range => 190f;

	private void GenerateIVars()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.abstractCreature.ID.RandomSeed);
		float hue = ((UnityEngine.Random.value < 0.01f) ? UnityEngine.Random.value : ((!(UnityEngine.Random.value < 0.5f)) ? Mathf.Lerp(0.5f, 0.65f, UnityEngine.Random.value) : Mathf.Lerp(0f, 0.1f, UnityEngine.Random.value)));
		HSLColor color = new HSLColor(hue, 1f, 0.8f);
		float value = UnityEngine.Random.value;
		iVars = new IndividualVariations(value, color);
		UnityEngine.Random.state = state;
	}

	public LanternMouse(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		GenerateIVars();
		float num = 0.4f;
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5.5f, num / 2f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 6f, num / 2f);
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 12f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new MouseGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		room = placeRoom;
		if (placeRoom.game.IsStorySession)
		{
			if (!AI.DangleTile(base.abstractCreature.pos.Tile, noAccessMap: true).HasValue)
			{
				for (int i = 0; i < Custom.IntClamp(base.abstractCreature.timeSpentHere - 20, 0, 300); i++)
				{
					IntVector2 tile = room.RandomTile();
					if (AI.DangleTile(tile, noAccessMap: true).HasValue)
					{
						base.abstractCreature.pos.Tile = tile;
						break;
					}
				}
			}
			if (!AI.DangleTile(base.abstractCreature.pos.Tile, noAccessMap: true).HasValue)
			{
				for (int num = base.abstractCreature.pos.Tile.y; num >= 0; num--)
				{
					if (room.aimap.TileAccessibleToCreature(new IntVector2(base.abstractCreature.pos.x, num), base.Template))
					{
						base.abstractCreature.pos.y = num;
						break;
					}
				}
			}
		}
		base.PlaceInRoom(placeRoom);
		if (placeRoom.game.IsStorySession && AI.DangleTile(base.abstractCreature.pos.Tile, noAccessMap: true).HasValue)
		{
			if (base.graphicsModule != null)
			{
				(base.graphicsModule as MouseGraphics).SpawnAsHanging();
			}
			AI.dangle = AI.DangleTile(base.abstractCreature.pos.Tile, noAccessMap: true);
			AttatchRope(AI.dangle.Value.attachedPos.Tile);
			ropeLength = AI.dangle.Value.attachedPos.Tile.FloatDist(AI.dangle.Value.bodyPos.Tile) * 20f;
		}
	}

	public override void Update(bool eu)
	{
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (!base.dead && State.health < 0f && UnityEngine.Random.value < 0f - State.health && UnityEngine.Random.value < 0.025f)
		{
			Die();
		}
		if (!base.dead && UnityEngine.Random.value * 0.7f > State.health && UnityEngine.Random.value < 0.125f)
		{
			Stun(UnityEngine.Random.Range(1, UnityEngine.Random.Range(1, 27 - Custom.IntClamp((int)(20f * State.health), 0, 10))));
		}
		if (base.dead)
		{
			State.battery--;
		}
		carried = grabbedBy.Count > 0;
		if (carried)
		{
			Carried();
		}
		base.Update(eu);
		ropeStretch = 0f;
		if (ropeInvincible > 0)
		{
			ropeInvincible--;
		}
		if (room == null)
		{
			return;
		}
		sitting = false;
		shrinkingRope = false;
		currentlyClimbingCorridor = false;
		if (ropeAttatchedPos.HasValue)
		{
			Hang();
		}
		else
		{
			if (base.Consious)
			{
				footingCounter++;
				Act();
			}
			else
			{
				footingCounter = 0;
			}
			if (Footing)
			{
				for (int i = 0; i < 2; i++)
				{
					base.bodyChunks[i].vel *= 0.8f;
					base.bodyChunks[i].vel.y += base.gravity;
				}
			}
		}
		if (base.Consious && !Footing && AI.behavior == MouseAI.Behavior.Flee && !base.safariControlled)
		{
			for (int j = 0; j < 2; j++)
			{
				if (room.aimap.TileAccessibleToCreature(room.GetTilePosition(base.bodyChunks[j].pos), base.Template))
				{
					base.bodyChunks[j].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 5f;
				}
			}
		}
		if (Sleeping)
		{
			wakeUp -= 0.0045454544f;
		}
		else if (fallAsleep < 0.8f || !ropeAttatchedPos.HasValue)
		{
			wakeUp += 1f / Mathf.Lerp(100f, 400f, UnityEngine.Random.value);
		}
		wakeUp += AI.fear / Mathf.Lerp(3f, 10f, UnityEngine.Random.value);
		wakeUp = Mathf.Clamp(wakeUp, 0f, 1f);
	}

	private void Swim()
	{
		base.mainBodyChunk.vel.y += 1.5f;
	}

	private void Act()
	{
		AI.Update();
		if (specialMoveCounter > 0)
		{
			specialMoveCounter--;
			MoveTowards(room.MiddleOfTile(specialMoveDestination));
			if (Custom.DistLess(base.mainBodyChunk.pos, room.MiddleOfTile(specialMoveDestination), 5f))
			{
				specialMoveCounter = 0;
			}
		}
		else
		{
			if (!room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template) && !room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos, base.Template))
			{
				footingCounter = 0;
			}
			if ((!base.safariControlled && room.GetWorldCoordinate(base.mainBodyChunk.pos) == AI.pathFinder.GetDestination) || (!base.safariControlled && room.GetWorldCoordinate(base.bodyChunks[1].pos) == AI.pathFinder.GetDestination && AI.threatTracker.Utility() < 0.5f))
			{
				Sit();
				base.GoThroughFloors = false;
			}
			else
			{
				MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
				if (movementConnection == default(MovementConnection))
				{
					movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[1].pos), actuallyFollowingThisPath: true);
				}
				if (base.abstractCreature.controlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
				{
					movementConnection = default(MovementConnection);
					if (inputWithDiagonals.HasValue)
					{
						MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
						if (movementConnection != default(MovementConnection))
						{
							type = movementConnection.type;
						}
						if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
						{
							type = MovementConnection.MovementType.ShortCut;
						}
						if (inputWithDiagonals.Value.AnyDirectionalInput)
						{
							movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
							controlSitting = false;
						}
						if (inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw && !Sleeping)
						{
							Squeak(UnityEngine.Random.value);
						}
						if (inputWithDiagonals.Value.jmp && !lastInputWithDiagonals.Value.jmp && !inputWithDiagonals.Value.AnyDirectionalInput)
						{
							controlSitting = true;
						}
						if (room != null && !controlSitting && inputWithDiagonals.Value.pckp && !lastInputWithDiagonals.Value.pckp)
						{
							for (int i = 0; i < 4; i++)
							{
								if (room.GetTile(new IntVector2(base.abstractCreature.pos.Tile.x, base.abstractCreature.pos.Tile.y + i)).horizontalBeam)
								{
									AttatchRope(new IntVector2(base.abstractCreature.pos.Tile.x, base.abstractCreature.pos.Tile.y + i));
									break;
								}
								if (room.GetTile(new IntVector2(base.abstractCreature.pos.Tile.x, base.abstractCreature.pos.Tile.y + i)).Solid)
								{
									if (i != 0)
									{
										AttatchRope(new IntVector2(base.abstractCreature.pos.Tile.x, base.abstractCreature.pos.Tile.y + (i - 1)));
									}
									break;
								}
							}
						}
						if (controlSitting)
						{
							Sit();
							base.GoThroughFloors = false;
						}
						else if (inputWithDiagonals.Value.y < 0)
						{
							base.GoThroughFloors = true;
						}
						else
						{
							base.GoThroughFloors = false;
						}
					}
				}
				if (movementConnection != default(MovementConnection))
				{
					Run(movementConnection);
				}
				else
				{
					base.GoThroughFloors = false;
				}
			}
		}
		if (base.Consious)
		{
			profileFac *= 0.97f;
		}
		if (!Custom.DistLess(base.mainBodyChunk.pos, base.mainBodyChunk.lastPos, 5f))
		{
			runCycle += runSpeed / 40f;
		}
		if (voiceCounter > 0)
		{
			voiceCounter--;
		}
		else if (!base.safariControlled && !Sleeping && UnityEngine.Random.value < (ropeAttatchedPos.HasValue ? 0.1f : 1f) / ((AI.behavior == MouseAI.Behavior.Flee) ? Mathf.Lerp(80f, 20f, AI.threatTracker.Utility()) : 100f))
		{
			Squeak(Mathf.InverseLerp(0.5f, 1f, AI.threatTracker.Utility()));
		}
	}

	protected void Squeak(float stress)
	{
		if (base.dead || voiceCounter > 0)
		{
			return;
		}
		room.PlaySound(SoundID.Mouse_Squeak, base.mainBodyChunk, loop: false, Mathf.Lerp(0.5f, 1f, stress), Mathf.Lerp(1f, 1.3f, stress));
		voiceCounter = UnityEngine.Random.Range(5, 12);
		if (base.graphicsModule != null)
		{
			(base.graphicsModule as MouseGraphics).head.pos += Custom.RNV() * 4f * UnityEngine.Random.value;
			if (UnityEngine.Random.value > Mathf.InverseLerp(0.5f, 1f, stress))
			{
				(base.graphicsModule as MouseGraphics).ouchEyes = Math.Max((base.graphicsModule as MouseGraphics).ouchEyes, voiceCounter);
			}
		}
	}

	public override void Grabbed(Grasp grasp)
	{
		base.Grabbed(grasp);
		controlSitting = false;
		Squeak(AI.threatTracker.Utility() * 0.5f + UnityEngine.Random.value * 0.5f);
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
		controlSitting = false;
		Squeak(1f);
	}

	private void Run(MovementConnection followingConnection)
	{
		float num = runCycle;
		runCycle += runSpeed / Mathf.Lerp(4f, 12f, UnityEngine.Random.value);
		if (num < Mathf.Floor(runCycle))
		{
			room.PlaySound(SoundID.Mouse_Scurry, base.mainBodyChunk);
		}
		if (followingConnection.destinationCoord.x != followingConnection.startCoord.x)
		{
			if (followingConnection.destinationCoord.x > followingConnection.startCoord.x)
			{
				profileFac = Mathf.Min(profileFac + 1f / 7f, 1f);
			}
			else
			{
				profileFac = Mathf.Max(profileFac - 1f / 7f, -1f);
			}
		}
		else
		{
			profileFac = Mathf.Lerp(profileFac, 0f - Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos).x, 0.6f);
		}
		if (followingConnection.destinationCoord.y > followingConnection.startCoord.y && room.aimap.getAItile(followingConnection.destinationCoord).acc != AItile.Accessibility.Climb)
		{
			currentlyClimbingCorridor = true;
		}
		if (followingConnection.type == MovementConnection.MovementType.ReachUp)
		{
			(AI.pathFinder as StandardPather).pastConnections.Clear();
		}
		if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
		{
			enteringShortCut = followingConnection.StartTile;
			if (base.safariControlled)
			{
				bool flag = false;
				List<IntVector2> list = new List<IntVector2>();
				ShortcutData[] shortcuts = room.shortcuts;
				for (int i = 0; i < shortcuts.Length; i++)
				{
					ShortcutData shortcutData = shortcuts[i];
					if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != followingConnection.StartTile)
					{
						list.Add(shortcutData.StartTile);
					}
					if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == followingConnection.StartTile)
					{
						flag = true;
					}
				}
				if (flag)
				{
					if (list.Count > 0)
					{
						list.Shuffle();
						NPCTransportationDestination = room.GetWorldCoordinate(list[0]);
					}
					else
					{
						NPCTransportationDestination = followingConnection.destinationCoord;
					}
				}
			}
			else if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
			{
				NPCTransportationDestination = followingConnection.destinationCoord;
			}
		}
		else if (followingConnection.type == MovementConnection.MovementType.OpenDiagonal || followingConnection.type == MovementConnection.MovementType.ReachOverGap || followingConnection.type == MovementConnection.MovementType.ReachUp || followingConnection.type == MovementConnection.MovementType.ReachDown || followingConnection.type == MovementConnection.MovementType.SemiDiagonalReach)
		{
			specialMoveCounter = 30;
			specialMoveDestination = followingConnection.DestTile;
		}
		else
		{
			Vector2 vector = room.MiddleOfTile(followingConnection.DestTile);
			if (lastFollowedConnection != default(MovementConnection) && lastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
			{
				base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, vector) * 4f;
			}
			if (Footing)
			{
				for (int j = 0; j < 2; j++)
				{
					if (followingConnection.startCoord.x == followingConnection.destinationCoord.x)
					{
						base.bodyChunks[j].vel.x += Mathf.Min((vector.x - base.bodyChunks[j].pos.x) / 8f, 1.2f);
					}
					else if (followingConnection.startCoord.y == followingConnection.destinationCoord.y)
					{
						base.bodyChunks[j].vel.y += Mathf.Min((vector.y - base.bodyChunks[j].pos.y) / 8f, 1.2f);
					}
				}
			}
			if (lastFollowedConnection != default(MovementConnection) && (Footing || room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template)) && ((followingConnection.startCoord.x != followingConnection.destinationCoord.x && lastFollowedConnection.startCoord.x == lastFollowedConnection.destinationCoord.x) || (followingConnection.startCoord.y != followingConnection.destinationCoord.y && lastFollowedConnection.startCoord.y == lastFollowedConnection.destinationCoord.y)))
			{
				base.mainBodyChunk.vel *= 0.7f;
				base.bodyChunks[1].vel *= 0.5f;
			}
			if (followingConnection.type == MovementConnection.MovementType.DropToFloor)
			{
				footingCounter = 0;
			}
			MoveTowards(vector);
		}
		lastFollowedConnection = followingConnection;
	}

	private void MoveTowards(Vector2 moveTo)
	{
		Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, moveTo);
		if (room.aimap.getAItile(base.bodyChunks[1].pos).acc >= AItile.Accessibility.Climb)
		{
			vector *= 0.5f;
		}
		if (!Footing)
		{
			vector *= 0.3f;
		}
		if (IsTileSolid(1, 0, -1) && (((double)vector.x < -0.5 && base.mainBodyChunk.pos.x > base.bodyChunks[1].pos.x + 5f) || ((double)vector.x > 0.5 && base.mainBodyChunk.pos.x < base.bodyChunks[1].pos.x - 5f)))
		{
			base.mainBodyChunk.vel.x -= ((vector.x < 0f) ? (-1f) : 1f) * 1.3f;
			base.bodyChunks[1].vel.x += ((vector.x < 0f) ? (-1f) : 1f) * 0.5f;
			if (!IsTileSolid(0, 0, 1))
			{
				base.mainBodyChunk.vel.y += 3.2f;
			}
		}
		base.mainBodyChunk.vel += vector * 4.2f * runSpeed;
		base.bodyChunks[1].vel -= vector * 1f * runSpeed;
		base.GoThroughFloors = moveTo.y < base.mainBodyChunk.pos.y - 5f;
	}

	private void Sit()
	{
		if (room.aimap.getAItile(base.bodyChunks[1].pos).acc == AItile.Accessibility.Floor && !IsTileSolid(0, 0, 1) && !IsTileSolid(1, 0, 1))
		{
			base.mainBodyChunk.vel.y += 2f;
			base.bodyChunks[1].vel.y -= 4f;
			profileFac *= 0.6f;
			sitting = true;
		}
	}

	private void Hang()
	{
		profileFac *= 0.8f;
		if (base.Consious)
		{
			AI.Update();
			if (!ropeAttatchedPos.HasValue)
			{
				return;
			}
			if (base.safariControlled && inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp && !lastInputWithDiagonals.Value.pckp)
			{
				DetatchRope();
				return;
			}
			if (AI.dangle.HasValue)
			{
				float num = Vector2.Distance(ropeAttatchedPos.Value, room.MiddleOfTile(AI.dangle.Value.bodyPos));
				if (wakeUp == 1f)
				{
					num = Mathf.Max(num * (1f - Mathf.Clamp(AI.pullUp, 0f, 1f)), 20f);
				}
				if (ropeLength < num)
				{
					ropeLength = Mathf.Min(Mathf.Lerp(ropeLength, num, 0.002f) + 0.6f, num);
					if (ropeLength < num - 4f)
					{
						fallAsleep -= 1f / 60f;
					}
				}
				else if (ropeLength > num)
				{
					if (ropeLength > num + 1.5f)
					{
						shrinkingRope = true;
						runCycle += 0.05f;
						fallAsleep -= 1f / 60f;
					}
					ropeLength = Mathf.Max(Mathf.Lerp(ropeLength, num, 0.002f) - 0.6f, num);
				}
			}
			if (UnityEngine.Random.value < 0.025f)
			{
				swingFacGetTo = Mathf.Pow(UnityEngine.Random.value, 1.5f);
			}
			swingFac = Mathf.Lerp(swingFac, swingFacGetTo, 0.05f);
			if (base.mainBodyChunk.vel.x == 0f)
			{
				base.mainBodyChunk.vel.x = Mathf.Lerp(-0.5f, 0.5f, UnityEngine.Random.value);
			}
			else
			{
				base.mainBodyChunk.vel.x += Mathf.Sign(base.mainBodyChunk.vel.x) * 0.02f * swingFac * (1f - AI.fear);
			}
			controlSitting = false;
			if (base.safariControlled)
			{
				if (inputWithDiagonals.HasValue)
				{
					if (inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw && !Sleeping)
					{
						Squeak(UnityEngine.Random.value);
					}
					if (inputWithDiagonals.Value.x != 0 && !Sleeping)
					{
						BodyChunk bodyChunk = base.mainBodyChunk;
						bodyChunk.vel.x = bodyChunk.vel.x + (float)inputWithDiagonals.Value.x * 0.1f;
					}
					if (inputWithDiagonals.Value.jmp)
					{
						fallAsleep += 1f / Mathf.Lerp(120f, 700f, UnityEngine.Random.value);
					}
				}
				if (!inputWithDiagonals.HasValue || !inputWithDiagonals.Value.jmp)
				{
					fallAsleep -= 1f / 30f;
				}
			}
			if (voiceCounter > 0)
			{
				voiceCounter--;
			}
			if (AI.fear > 0f)
			{
				base.bodyChunks[1].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * AI.fear;
			}
			if (!base.safariControlled)
			{
				if (AI.wantToSleep)
				{
					fallAsleep += 1f / Mathf.Lerp(120f, 700f, UnityEngine.Random.value);
				}
				else
				{
					fallAsleep -= 1f / 30f;
				}
			}
			fallAsleep = Mathf.Clamp(fallAsleep, 0f, 1f);
		}
		UpdateRope();
		footingCounter = 0;
		if (!base.safariControlled)
		{
			ropeLength += 0.1f;
		}
		else if (inputWithDiagonals.HasValue && inputWithDiagonals.Value.y < 0 && !Sleeping)
		{
			ropeLength += 0.35f;
		}
		float num2 = ropeLength;
		for (int i = 1; i < ropeBends.Count; i++)
		{
			num2 -= Vector2.Distance(ropeBends[i - 1], ropeBends[i]);
		}
		if (num2 <= 0f || ropeBends.Count > 3)
		{
			DetatchRope();
			return;
		}
		if (!Custom.DistLess(base.bodyChunks[1].pos, ropeBends[ropeBends.Count - 1], num2))
		{
			Vector2 vector = Custom.DirVec(base.bodyChunks[1].pos, ropeBends[ropeBends.Count - 1]);
			float num3 = Vector2.Distance(base.bodyChunks[1].pos, ropeBends[ropeBends.Count - 1]);
			ropeStretch = num3 / num2 - 1f;
			base.bodyChunks[1].pos += (num3 - num2) * vector * 0.15f;
			base.bodyChunks[1].vel += (num3 - num2) * vector * 0.15f;
		}
		for (int j = 0; j < 2; j++)
		{
			if (base.bodyChunks[j].ContactPoint.y >= 0)
			{
				continue;
			}
			for (int k = -1; k < 2; k += 2)
			{
				if (!IsTileSolid(j, k, -1))
				{
					base.bodyChunks[0].vel.y -= 1f;
					base.bodyChunks[1].vel.y -= 1f;
					base.bodyChunks[0].vel.x += k;
					base.bodyChunks[1].vel.x += k;
					break;
				}
			}
		}
	}

	private void Carried()
	{
		if (base.dead)
		{
			return;
		}
		bool flag = room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template) || room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos, base.Template);
		if (grabbedBy[0].grabber is Player && ((grabbedBy[0].grabber as Player).input[0].x != 0 || (grabbedBy[0].grabber as Player).input[0].y != 0))
		{
			flag = false;
		}
		if (flag)
		{
			struggleCountdownA--;
			if (struggleCountdownA < 0)
			{
				if (UnityEngine.Random.value < 1f / 120f)
				{
					struggleCountdownA = UnityEngine.Random.Range(20, 400);
				}
				for (int i = 0; i < 2; i++)
				{
					base.bodyChunks[i].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * 6f * UnityEngine.Random.value;
				}
			}
		}
		struggleCountdownB--;
		if (struggleCountdownB < 0 && UnityEngine.Random.value < 1f / 120f)
		{
			struggleCountdownB = UnityEngine.Random.Range(10, 100);
		}
		if (!base.dead && base.graphicsModule != null && (struggleCountdownA < 0 || struggleCountdownB < 0))
		{
			if (UnityEngine.Random.value < 0.025f)
			{
				(base.graphicsModule as MouseGraphics).ResetUnconsiousProfile();
			}
			for (int j = 0; j < base.graphicsModule.bodyParts.Length; j++)
			{
				base.graphicsModule.bodyParts[j].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * 3f * UnityEngine.Random.value;
				base.graphicsModule.bodyParts[j].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * 6f * UnityEngine.Random.value;
			}
		}
	}

	public void AttatchRope(IntVector2 attatchPos)
	{
		ropeInvincible = 20;
		base.GoThroughFloors = true;
		ropeAttatchedPos = room.MiddleOfTile(attatchPos);
		if (room.GetTile(ropeAttatchedPos.Value + new Vector2(0f, 20f)).Solid)
		{
			ropeAttatchedPos = new Vector2(ropeAttatchedPos.Value.x, ropeAttatchedPos.Value.y + 9f);
		}
		ropeLength = 20f;
		ropeBends = new List<Vector2> { ropeAttatchedPos.Value };
		lastRopeClearPos = base.bodyChunks[1].pos;
		room.PlaySound(SoundID.Mouse_Rope_Attach, base.mainBodyChunk);
	}

	public void DetatchRope()
	{
		if (ropeInvincible <= 0)
		{
			ropeAttatchedPos = null;
			ropeBends = null;
			room.PlaySound(SoundID.Mouse_Rope_Detach, base.mainBodyChunk);
		}
	}

	public void CarryObject()
	{
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		fallAsleep = Mathf.Max(0f, fallAsleep - 0.25f);
		if (otherObject is LanternMouse)
		{
			AI.CollideWithMouse(otherObject as LanternMouse);
			if (base.bodyChunks[myChunk].pos.y > otherObject.bodyChunks[otherChunk].pos.y)
			{
				base.bodyChunks[myChunk].vel.y += 2f;
				otherObject.bodyChunks[otherChunk].vel.y -= 2f;
			}
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		if (base.graphicsModule != null && speed > 12f)
		{
			(base.graphicsModule as MouseGraphics).ouchEyes = Custom.IntClamp((int)(speed * 0.5f), 0, 30);
			(base.graphicsModule as MouseGraphics).TerrainImpact(speed);
		}
		base.TerrainImpact(chunk, direction, speed, firstContact);
	}

	public override void Die()
	{
		base.Die();
	}

	public override Color ShortCutColor()
	{
		return iVars.color.rgb;
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - vector * (-1.5f + (float)i) * 15f;
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 2f;
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	private void UpdateRope()
	{
		if (ropeBends.Count > 1)
		{
			int num;
			for (num = SharedPhysics.RayTracedTilesArray(base.bodyChunks[1].pos, ropeBends[ropeBends.Count - 2], _cachedRay2); num >= _cachedRay2.Length; num = SharedPhysics.RayTracedTilesArray(base.bodyChunks[1].pos, ropeBends[ropeBends.Count - 2], _cachedRay2))
			{
				Custom.LogWarning($"LanternMouse UpdateRope ray tracing limit exceeded, extending cache to {_cachedRay2.Length + 100} and trying again!");
				Array.Resize(ref _cachedRay2, _cachedRay2.Length + 100);
			}
			bool flag = true;
			for (int i = 0; i < num - 1; i++)
			{
				if (room.GetTile(_cachedRay2[i]).Solid)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				ropeBends.RemoveAt(ropeBends.Count - 1);
				lastRopeClearPos = base.bodyChunks[1].pos;
				return;
			}
		}
		Vector2 vector = ropeBends[ropeBends.Count - 1];
		IntVector2 tilePosition = room.GetTilePosition(vector);
		IntVector2 intVector = tilePosition;
		int num2;
		for (num2 = SharedPhysics.RayTracedTilesArray(base.bodyChunks[1].pos, vector, _cachedRay); num2 >= _cachedRay.Length; num2 = SharedPhysics.RayTracedTilesArray(base.bodyChunks[1].pos, vector, _cachedRay))
		{
			Custom.LogWarning($"LanternMouse UpdateRope ray tracing limit exceeded, extending cache to {_cachedRay.Length + 100} and trying again!");
			Array.Resize(ref _cachedRay, _cachedRay.Length + 100);
		}
		bool flag2 = true;
		for (int j = 0; j < num2 && !(_cachedRay[j] == tilePosition); j++)
		{
			if (!room.GetTile(_cachedRay[j]).Solid)
			{
				continue;
			}
			flag2 = false;
			FloatRect floatRect = Custom.RectCollision(ropeAttatchedPos.Value, base.bodyChunks[1].pos, room.TileRect(_cachedRay[j]));
			Vector2 b = new Vector2(floatRect.left, floatRect.bottom);
			intVector = _cachedRay[j];
			float dst = Vector2.Distance(base.bodyChunks[1].pos, b);
			for (int k = 0; k < 8; k++)
			{
				if (room.GetTile(_cachedRay[j] + Custom.eightDirections[k]).Solid)
				{
					FloatRect floatRect2 = Custom.RectCollision(ropeAttatchedPos.Value, base.bodyChunks[1].pos, room.TileRect(_cachedRay[j] + Custom.eightDirections[k]));
					if (Custom.DistLess(base.bodyChunks[1].pos, new Vector2(floatRect2.left, floatRect2.bottom), dst))
					{
						b = new Vector2(floatRect2.left, floatRect2.bottom);
						intVector = _cachedRay[j] + Custom.eightDirections[k];
						dst = Vector2.Distance(base.bodyChunks[1].pos, b);
					}
				}
			}
			break;
		}
		if (flag2)
		{
			lastRopeClearPos = base.bodyChunks[1].pos;
			return;
		}
		Vector2 vector2 = ropeAttatchedPos.Value;
		float num3 = float.MaxValue;
		for (int l = 0; l < 100; l++)
		{
			IntVector2 intVector2 = intVector;
			for (int m = 0; m < 9; m++)
			{
				IntVector2 intVector3 = intVector + Custom.eightDirectionsAndZero[m];
				if (!room.GetTile(intVector3).Solid)
				{
					continue;
				}
				bool flag3 = false;
				for (int n = 0; n < 4; n++)
				{
					if (room.IsCornerFree(intVector3.x, intVector3.y, n) && Mathf.Abs(Custom.DistanceToLine(room.TileRect(intVector3).GetCorner(n), ropeAttatchedPos.Value, lastRopeClearPos)) < num3)
					{
						vector2 = room.TileRect(intVector3).GetCorner(n);
						num3 = Mathf.Abs(Custom.DistanceToLine(room.TileRect(intVector3).GetCorner(n), ropeAttatchedPos.Value, lastRopeClearPos));
						vector2 += Custom.DirVec(room.MiddleOfTile(intVector3), vector2) * 0.4f;
						intVector2 = intVector3;
						flag3 = true;
					}
				}
				if (!flag3 && Mathf.Abs(Custom.DistanceToLine(room.MiddleOfTile(intVector3), ropeAttatchedPos.Value, lastRopeClearPos)) < num3)
				{
					num3 = Mathf.Abs(Custom.DistanceToLine(room.MiddleOfTile(intVector3), ropeAttatchedPos.Value, lastRopeClearPos));
					intVector2 = intVector3;
				}
			}
			if (intVector2 == intVector)
			{
				break;
			}
			intVector = intVector2;
		}
		if (vector2 != ropeBends[ropeBends.Count - 1])
		{
			ropeBends.Add(vector2);
		}
	}

	Vector2 IProvideWarmth.Position()
	{
		return base.firstChunk.pos;
	}
}
