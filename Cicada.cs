using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Cicada : InsectoidCreature
{
	public struct IndividualVariations
	{
		public float fatness;

		public float wingSoundPitch;

		public float defaultWingDeployment;

		public float tentacleLength;

		public float tentacleThickness;

		public float wingThickness;

		public float wingLength;

		public int bustedWing;

		public HSLColor color;

		public IndividualVariations(float fatness, float wingSoundPitch, float defaultWingDeployment, float tentacleLength, float tentacleThickness, float wingThickness, float wingLength, int bustedWing, HSLColor color)
		{
			this.fatness = fatness;
			this.wingSoundPitch = wingSoundPitch;
			this.defaultWingDeployment = defaultWingDeployment;
			this.tentacleLength = tentacleLength;
			this.tentacleThickness = tentacleThickness;
			this.wingThickness = wingThickness;
			this.wingLength = wingLength;
			this.bustedWing = bustedWing;
			this.color = color;
		}
	}

	public CicadaAI AI;

	public float sinCounter;

	public bool flying;

	public int waitToFlyCounter;

	public float flyingPower;

	private int flipH;

	public int chargeCounter;

	public Vector2 chargeDir;

	public BodyChunk stickyCling;

	public int noStickyCounter;

	public int cantPickUpCounter;

	public Player cantPickUpPlayer;

	public bool gender;

	public IndividualVariations iVars;

	public IntVector2 sitDirection;

	public float stamina = 1f;

	public float struggleAgainstPlayer;

	public bool currentlyLiftingPlayer;

	public float playerJumpBoost;

	private bool WantToSitDownAtDestination
	{
		get
		{
			if (AI.behavior == CicadaAI.Behavior.Idle && AI.pathFinder.GetDestination.room == room.abstractRoom.index)
			{
				return Climbable(AI.pathFinder.GetDestination.Tile);
			}
			return false;
		}
	}

	public bool AtSitDestination
	{
		get
		{
			if (WantToSitDownAtDestination && Custom.ManhattanDistance(base.abstractCreature.pos, AI.pathFinder.GetDestination) < 2)
			{
				return Climbable(AI.pathFinder.GetDestination.Tile);
			}
			return false;
		}
	}

	public bool Charging => chargeCounter > 21;

	public float LiftPlayerPower => Custom.SCurve(stamina, 0.15f) * (0.4f + playerJumpBoost * 0.6f);

	private void GenerateIVars()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.abstractCreature.ID.RandomSeed);
		HSLColor color = new HSLColor(Custom.ClampedRandomVariation(0.55f, 0.1f, 0.5f), 1f, 0.5f);
		float num = Custom.ClampedRandomVariation(gender ? 0.6f : 0.4f, 0.1f, 0.5f) * 2f;
		int bustedWing = -1;
		if (UnityEngine.Random.value < 0.125f)
		{
			bustedWing = UnityEngine.Random.Range(0, 4);
		}
		iVars = new IndividualVariations(num, 1f / num + Mathf.Lerp(-0.1f, 0.1f, UnityEngine.Random.value), UnityEngine.Random.value, Mathf.Lerp(0.6f, 1.4f, UnityEngine.Random.value), Mathf.Lerp(0.6f, 1.4f, UnityEngine.Random.value), Mathf.Lerp(1f, 0.4f, UnityEngine.Random.value * UnityEngine.Random.value), Custom.ClampedRandomVariation(0.66667f, 0.3f, 0.2f) * 1.5f, bustedWing, color);
		UnityEngine.Random.state = state;
	}

	public Cicada(AbstractCreature abstractCreature, World world, bool gender)
		: base(abstractCreature, world)
	{
		this.gender = gender;
		GenerateIVars();
		float num = (gender ? 0.65f : 0.55f);
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 7.5f, num / 2f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 7f, num / 2f);
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 14f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		sinCounter = UnityEngine.Random.value;
		flipH = ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
		flying = true;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new CicadaGraphics(this);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (noStickyCounter > 0)
		{
			noStickyCounter--;
		}
		if (chargeCounter == 0 && cantPickUpCounter > 0)
		{
			cantPickUpCounter--;
		}
		if ((base.State as HealthState).health < 0.5f && UnityEngine.Random.value > (base.State as HealthState).health && UnityEngine.Random.value < 1f / 3f)
		{
			Stun(4);
			if ((base.State as HealthState).health <= 0f && UnityEngine.Random.value < 0.25f)
			{
				Die();
			}
		}
		if (base.Consious)
		{
			bool flag = false;
			for (int i = 0; i < grabbedBy.Count; i++)
			{
				if (grabbedBy[i].grabber is Player)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				GrabbedByPlayer();
			}
			else if (base.Submersion > 0.5f)
			{
				Swim();
			}
			else
			{
				Act();
			}
		}
		else
		{
			stickyCling = null;
			cantPickUpCounter = 0;
			stamina = 0f;
		}
		if (!flying)
		{
			cantPickUpCounter = 0;
		}
		if (base.grasps[0] != null)
		{
			CarryObject();
		}
	}

	private void Swim()
	{
		base.mainBodyChunk.vel.y += 0.5f;
	}

	private void Act()
	{
		AI.Update();
		if (grabbedBy.Count == 0 && stickyCling == null)
		{
			stamina = Mathf.Min(stamina + 1f / 70f, 1f);
		}
		MovementConnection movementConnection = default(MovementConnection);
		if ((flying || !AtSitDestination) && chargeCounter == 0 && !AI.swooshToPos.HasValue)
		{
			movementConnection = (AI.pathFinder as CicadaPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
		}
		if (base.safariControlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
		{
			movementConnection = default(MovementConnection);
			if (inputWithDiagonals.HasValue)
			{
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				if ((inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0) && chargeCounter == 0)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
				}
				if (inputWithDiagonals.Value.jmp && !lastInputWithDiagonals.Value.jmp && chargeCounter == 0)
				{
					if (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0)
					{
						Charge(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f);
					}
					else
					{
						Charge(base.mainBodyChunk.pos + (base.graphicsModule as CicadaGraphics).lookDir * 40f);
					}
				}
				if (inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw)
				{
					for (int i = 0; i < base.grasps.Length; i++)
					{
						ReleaseGrasp(i);
					}
				}
				if (inputWithDiagonals.Value.pckp && room != null && (base.grasps.Length == 0 || base.grasps[0] == null || base.grasps[0].grabbed == null))
				{
					for (int j = 0; j < room.physicalObjects.Length; j++)
					{
						for (int k = 0; k < room.physicalObjects[j].Count; k++)
						{
							if ((room.physicalObjects[j][k] is Fly || room.physicalObjects[j][k] is Leech) && Custom.DistLess(base.mainBodyChunk.pos, (room.physicalObjects[j][k] as Creature).mainBodyChunk.pos, 50f))
							{
								TryToGrabPrey(room.physicalObjects[j][k]);
							}
						}
					}
				}
				if (inputWithDiagonals.Value.y < 0)
				{
					base.GoThroughFloors = true;
				}
				else
				{
					base.GoThroughFloors = false;
				}
			}
		}
		if (flying)
		{
			sinCounter += 1f / Mathf.Lerp(45f, 85f, UnityEngine.Random.value);
			if (sinCounter > 1f)
			{
				sinCounter -= 1f;
			}
			base.bodyChunks[0].vel.y += Mathf.Sin(sinCounter * (float)Math.PI * 2f) * 0.05f * flyingPower * stamina;
			base.bodyChunks[1].vel.y += Mathf.Sin(sinCounter * (float)Math.PI * 2f) * 0.05f * flyingPower * stamina;
			base.bodyChunks[0].vel *= Mathf.Lerp(1f, 0.98f, flyingPower * stamina);
			base.bodyChunks[1].vel *= Mathf.Lerp(1f, 0.94f, flyingPower * stamina);
			if (!base.safariControlled)
			{
				base.bodyChunks[0].vel.y += 0.8f * flyingPower * stamina;
				base.bodyChunks[1].vel.y += (AtSitDestination ? 0.5f : 1.2f) * flyingPower * stamina;
			}
			else
			{
				base.bodyChunks[0].vel.y += 0.8f * flyingPower * stamina;
				if (!inputWithDiagonals.HasValue || (inputWithDiagonals.Value.x == 0 && inputWithDiagonals.Value.y == 0))
				{
					base.bodyChunks[1].vel.y += (AtSitDestination ? 0.5f : 1f) * flyingPower * stamina;
				}
				else
				{
					base.bodyChunks[1].vel.y += (AtSitDestination ? 0.5f : 1.2f) * flyingPower * stamina;
				}
			}
			bool flag = false;
			if (movementConnection == default(MovementConnection) || Climbable(movementConnection.DestTile) || Climbable(room.GetTilePosition(base.mainBodyChunk.pos)))
			{
				if (room.aimap.getAItile(base.bodyChunks[0].pos).narrowSpace)
				{
					flag = true;
				}
				else if (room.aimap.getTerrainProximity(base.bodyChunks[0].pos) == 1 && room.aimap.getTerrainProximity(base.bodyChunks[1].pos) == 1 && (movementConnection == default(MovementConnection) || room.aimap.getTerrainProximity(movementConnection.destinationCoord) == 1))
				{
					flag = true;
				}
				else if (AtSitDestination)
				{
					flag = true;
				}
			}
			if (base.safariControlled && (!inputWithDiagonals.HasValue || !inputWithDiagonals.Value.pckp))
			{
				flag = false;
			}
			bool flag2 = true;
			if (flag)
			{
				int num = UnityEngine.Random.Range(0, 4);
				if (room.GetTile(base.abstractCreature.pos.Tile + Custom.fourDirections[num]).Solid)
				{
					base.mainBodyChunk.vel += Custom.fourDirections[num].ToVector2() * 3f;
					base.bodyChunks[1].vel += Custom.fourDirections[num].ToVector2() * 3f;
					Land();
				}
				else if (room.GetTile(base.mainBodyChunk.pos).verticalBeam)
				{
					Land();
				}
				else if (movementConnection != default(MovementConnection) && movementConnection.destinationCoord.y < base.abstractCreature.pos.y)
				{
					flag2 = false;
				}
			}
			else
			{
				for (int l = 0; l < 2; l++)
				{
					if (base.bodyChunks[l].ContactPoint.x != 0 || base.bodyChunks[l].ContactPoint.y != 0)
					{
						base.bodyChunks[l].vel -= base.bodyChunks[l].ContactPoint.ToVector2() * 8f * flyingPower * stamina * UnityEngine.Random.value;
					}
				}
			}
			if (stickyCling != null)
			{
				base.bodyChunks[1].vel += chargeDir * 0.4f;
				Vector2 vector = Custom.DegToVec(UnityEngine.Random.value * 360f) * 1.5f;
				base.bodyChunks[0].vel += vector;
				stickyCling.vel += vector;
				flag2 = true;
				if (Custom.DistLess(base.mainBodyChunk.pos, stickyCling.pos, base.mainBodyChunk.rad + stickyCling.rad + 35f) && !(stickyCling.owner as Creature).enteringShortCut.HasValue && AI.behavior == CicadaAI.Behavior.Antagonize && stickyCling.owner.room == room && stickyCling.pos.y > -20f && flying && UnityEngine.Random.value > 0.004761905f && (grabbedBy.Count == 0 || grabbedBy[0].grabber != stickyCling.owner))
				{
					float num2 = Vector2.Distance(base.mainBodyChunk.pos, stickyCling.pos);
					Vector2 vector2 = Custom.DirVec(base.mainBodyChunk.pos, stickyCling.pos);
					float num3 = base.mainBodyChunk.rad + stickyCling.rad + 15f;
					float num4 = 0.65f;
					float num5 = stickyCling.mass / (stickyCling.mass + base.mainBodyChunk.mass);
					base.mainBodyChunk.pos -= (num3 - num2) * vector2 * num5 * num4;
					base.mainBodyChunk.vel -= (num3 - num2) * vector2 * num5 * num4;
					stickyCling.pos += (num3 - num2) * vector2 * (1f - num5) * num4;
					stickyCling.vel += (num3 - num2) * vector2 * (1f - num5) * num4;
					stamina = Mathf.Clamp(stamina - 1f / 120f, 0f, 1f);
					if (stamina < 0.2f)
					{
						stickyCling = null;
					}
				}
				else
				{
					if (Custom.DistLess(base.mainBodyChunk.pos, stickyCling.pos, base.mainBodyChunk.rad + stickyCling.rad + 45f))
					{
						for (int m = 0; m < 2; m++)
						{
							base.bodyChunks[m].vel += Custom.DirVec(stickyCling.pos, base.bodyChunks[m].pos) * 4f;
						}
					}
					stickyCling = null;
				}
				if (stickyCling == null)
				{
					room.PlaySound(SoundID.Cicada_Tentacles_Detatch, base.mainBodyChunk);
				}
			}
			else
			{
				flyingPower = Mathf.Lerp(flyingPower, flag2 ? 1f : 0f, 0.1f);
			}
		}
		else
		{
			flyingPower = Mathf.Lerp(flyingPower, 0f, 0.05f);
			if (Climbable(room.GetTilePosition(base.mainBodyChunk.pos)))
			{
				base.bodyChunks[0].vel *= 0.8f;
				base.bodyChunks[1].vel *= 0.8f;
				base.bodyChunks[0].vel.y += base.gravity;
				base.bodyChunks[1].vel.y += base.gravity;
			}
			else
			{
				flying = true;
			}
		}
		if (AtSitDestination)
		{
			base.bodyChunks[1].vel += Vector2.ClampMagnitude(BodySitPosOffset(FindBodySitPos(AI.pathFinder.GetDestination.Tile)) - base.bodyChunks[1].pos, 10f) / 10f * 0.5f;
			base.mainBodyChunk.vel += Vector2.ClampMagnitude(BodySitPosOffset(AI.pathFinder.GetDestination.Tile) - base.mainBodyChunk.pos, 10f) / 10f * 0.5f;
		}
		if (movementConnection != default(MovementConnection))
		{
			if (movementConnection.destinationCoord.x < movementConnection.startCoord.x)
			{
				flipH = -1;
			}
			else if (movementConnection.destinationCoord.x > movementConnection.startCoord.x)
			{
				flipH = 1;
			}
			base.GoThroughFloors = movementConnection.destinationCoord.y < movementConnection.startCoord.y;
			if (movementConnection.type == MovementConnection.MovementType.ShortCut || movementConnection.type == MovementConnection.MovementType.NPCTransportation)
			{
				enteringShortCut = movementConnection.StartTile;
				if (base.safariControlled)
				{
					bool flag3 = false;
					List<IntVector2> list = new List<IntVector2>();
					ShortcutData[] shortcuts = room.shortcuts;
					for (int n = 0; n < shortcuts.Length; n++)
					{
						ShortcutData shortcutData = shortcuts[n];
						if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != movementConnection.StartTile)
						{
							list.Add(shortcutData.StartTile);
						}
						if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == movementConnection.StartTile)
						{
							flag3 = true;
						}
					}
					if (flag3)
					{
						if (list.Count > 0)
						{
							list.Shuffle();
							NPCTransportationDestination = room.GetWorldCoordinate(list[0]);
						}
						else
						{
							NPCTransportationDestination = movementConnection.destinationCoord;
						}
					}
				}
				else if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
				{
					NPCTransportationDestination = movementConnection.destinationCoord;
				}
			}
			else if (flying)
			{
				Vector2 vector3 = room.MiddleOfTile(movementConnection.destinationCoord);
				MovementConnection movementConnection2 = movementConnection;
				int num6 = 1;
				for (int num7 = 0; num7 < 3; num7++)
				{
					movementConnection2 = (AI.pathFinder as CicadaPather).FollowPath(movementConnection.destinationCoord, actuallyFollowingThisPath: false);
					if (!(movementConnection2 != default(MovementConnection)))
					{
						break;
					}
					vector3 += room.MiddleOfTile(movementConnection2.destinationCoord);
					num6++;
				}
				vector3 /= (float)num6;
				float a = (float)room.aimap.getTerrainProximity(base.mainBodyChunk.pos) / Mathf.Max(room.aimap.getTerrainProximity(base.mainBodyChunk.pos + Custom.DirVec(base.mainBodyChunk.pos, vector3) * Mathf.Clamp(base.mainBodyChunk.vel.magnitude * 5f, 5f, 15f)), 1f);
				a = Mathf.Min(a, 1f);
				a = Mathf.Pow(a, 3f);
				if (WantToSitDownAtDestination && AI.pathFinder.GetDestination.room == room.abstractRoom.index && Custom.DistLess(room.MiddleOfTile(AI.pathFinder.GetDestination.Tile), base.mainBodyChunk.pos, 200f) && AI.VisualContact(room.MiddleOfTile(AI.pathFinder.GetDestination.Tile), 0f))
				{
					a *= Mathf.Lerp(0.2f, 1f, Mathf.InverseLerp(0f, 300f, Vector2.Distance(room.MiddleOfTile(AI.pathFinder.GetDestination.Tile), base.mainBodyChunk.pos)));
				}
				base.mainBodyChunk.vel += Vector2.ClampMagnitude(vector3 - base.mainBodyChunk.pos, 40f) / 40f * 1.1f * a * flyingPower * stamina;
				base.bodyChunks[1].vel += Vector2.ClampMagnitude(vector3 - base.mainBodyChunk.pos, 40f) / 40f * 0.65f * a * flyingPower * stamina;
			}
			else
			{
				if (!movementConnection.destinationCoord.TileDefined)
				{
					return;
				}
				if (room.GetTile(movementConnection.DestTile).Terrain == Room.Tile.TerrainType.Slope)
				{
					TakeOff(Custom.DegToVec(UnityEngine.Random.value * 360f));
				}
				if (Climbable(movementConnection.DestTile))
				{
					base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord)) * Mathf.Lerp(0.4f, 1.8f, AI.stuckTracker.Utility());
					return;
				}
				waitToFlyCounter++;
				if (waitToFlyCounter > 30)
				{
					TakeOff(Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord)));
				}
			}
		}
		else if (chargeCounter > 0)
		{
			chargeCounter++;
			if (chargeCounter < 21)
			{
				base.bodyChunks[0].vel *= 0.8f;
				base.bodyChunks[1].vel *= 0.8f;
				base.bodyChunks[1].vel -= chargeDir * 0.8f;
			}
			else if (chargeCounter == 21)
			{
				room.PlaySound(SoundID.Cicada_Wings_Start_Bump_Attack, base.mainBodyChunk.pos);
			}
			else if (chargeCounter > 38)
			{
				chargeCounter = 0;
				base.bodyChunks[0].vel *= 0.5f;
				base.bodyChunks[1].vel *= 0.5f;
				room.PlaySound(SoundID.Cicada_Wings_Exit_Bump_Attack, base.mainBodyChunk.pos);
			}
			else
			{
				base.bodyChunks[0].vel += chargeDir * 4f;
				if (base.mainBodyChunk.vel.magnitude > 15f)
				{
					base.bodyChunks[1].vel *= 0.8f;
				}
				else
				{
					base.bodyChunks[1].vel *= 0.98f;
				}
			}
			if (room.aimap.getAItile(base.mainBodyChunk.pos).narrowSpace)
			{
				chargeCounter = 0;
			}
			flying = true;
		}
		else if (AI.swooshToPos.HasValue)
		{
			base.mainBodyChunk.vel += Vector2.ClampMagnitude(AI.swooshToPos.Value - base.mainBodyChunk.pos, 20f) / 20f * 1.8f * flyingPower * stamina;
			base.bodyChunks[1].vel += Vector2.ClampMagnitude(AI.swooshToPos.Value - base.mainBodyChunk.pos, 20f) / 20f * 0.8f * flyingPower * stamina;
			base.bodyChunks[1].vel *= 0.9f;
			base.bodyChunks[1].vel.y -= 0.2f;
			flying = true;
		}
	}

	private void GrabbedByPlayer()
	{
		flying = stamina > 1f / 3f;
		stickyCling = null;
		if (currentlyLiftingPlayer)
		{
			stamina -= 1f / (gender ? 190f : 120f);
		}
		stamina = Mathf.Clamp(stamina, 0f, 1f);
		base.bodyChunks[0].vel *= Mathf.Lerp(1f, 0.98f, stamina);
		base.bodyChunks[1].vel *= Mathf.Lerp(1f, 0.94f, stamina);
		base.bodyChunks[0].vel.y += 1.2f * stamina;
		base.bodyChunks[1].vel.y += playerJumpBoost + 1.8f * stamina;
		Player player = null;
		for (int i = 0; i < grabbedBy.Count; i++)
		{
			if (grabbedBy[i].grabber is Player)
			{
				player = grabbedBy[i].grabber as Player;
				break;
			}
		}
		if (ModManager.MMF && (room.aimap.getAItile(player.firstChunk.pos).narrowSpace || room.aimap.getAItile(base.firstChunk.pos).narrowSpace || room.aimap.getAItile(base.bodyChunks[1].pos).narrowSpace))
		{
			base.bodyChunks[0].vel.y = base.bodyChunks[0].vel.y - stamina * 1.3f;
		}
		SocialMemory.Relationship orInitiateRelationship = base.abstractCreature.state.socialMemory.GetOrInitiateRelationship(player.abstractCreature.ID);
		if (orInitiateRelationship.like < 0.9f)
		{
			orInitiateRelationship.like = Mathf.Lerp(orInitiateRelationship.like, -1f, 0.00025f);
		}
		if (!currentlyLiftingPlayer)
		{
			if (stamina < 1f / 3f)
			{
				stamina += 1f / ((player.input[0].x == 0 && player.input[0].y == 0) ? 600f : 900f);
			}
			else if (stamina < 2f / 3f)
			{
				stamina += 0.004f;
			}
			else
			{
				stamina += 1f / 30f;
			}
		}
		stamina = Mathf.Clamp(stamina, 0f, 1f);
		if (player.input[0].x == 0 && player.input[0].y == 0 && stamina == 1f)
		{
			struggleAgainstPlayer = Mathf.Min(1f, struggleAgainstPlayer + UnityEngine.Random.value / 30f);
			base.bodyChunks[0].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * struggleAgainstPlayer * 6f;
		}
		else
		{
			struggleAgainstPlayer = 0f;
		}
		if (player.input[0].jmp)
		{
			if (currentlyLiftingPlayer)
			{
				playerJumpBoost = Mathf.Max(0f, playerJumpBoost * 0.9f - 1f / 30f);
			}
			else if (!player.input[1].jmp)
			{
				playerJumpBoost = 1f;
			}
		}
		else
		{
			playerJumpBoost = 0f;
		}
		if (currentlyLiftingPlayer)
		{
			base.bodyChunks[0].vel.x += (float)player.input[0].x * stamina * 0.3f;
			base.bodyChunks[1].vel.x += (float)player.input[0].x * stamina * 0.4f;
		}
		noStickyCounter = 200;
		AI.panicFleeCrit = player;
	}

	public void CarryObject()
	{
		if (base.grasps[0].grabbed is Creature && UnityEngine.Random.value < 0.025f && AI.StaticRelationship((base.grasps[0].grabbed as Creature).abstractCreature).type != CreatureTemplate.Relationship.Type.Eats)
		{
			LoseAllGrasps();
			return;
		}
		float num = Vector2.Distance(base.mainBodyChunk.pos, base.grasps[0].grabbedChunk.pos);
		if (num > 50f)
		{
			LoseAllGrasps();
			return;
		}
		Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, base.grasps[0].grabbedChunk.pos);
		float num2 = base.mainBodyChunk.rad + base.grasps[0].grabbedChunk.rad;
		float num3 = 0.95f;
		float num4 = 0f;
		if (base.grasps[0].grabbed.TotalMass > base.TotalMass / 3f)
		{
			num4 = base.grasps[0].grabbedChunk.mass / (base.grasps[0].grabbedChunk.mass + base.mainBodyChunk.mass);
		}
		base.mainBodyChunk.pos -= (num2 - num) * vector * num4 * num3;
		base.mainBodyChunk.vel -= (num2 - num) * vector * num4 * num3;
		base.grasps[0].grabbedChunk.pos += (num2 - num) * vector * (1f - num4) * num3;
		base.grasps[0].grabbedChunk.vel += (num2 - num) * vector * (1f - num4) * num3;
	}

	public void Charge(Vector2 pos)
	{
		stickyCling = null;
		noStickyCounter = 140;
		if (chargeCounter <= 0)
		{
			chargeDir = Custom.DirVec(base.mainBodyChunk.pos, pos);
			chargeCounter = 1;
			room.PlaySound(SoundID.Cicada_Wings_Prepare_Bump_Attack, base.mainBodyChunk.pos);
		}
	}

	public bool Climbable(IntVector2 tile)
	{
		if (base.safariControlled)
		{
			if (!inputWithDiagonals.HasValue || !inputWithDiagonals.Value.pckp || room.aimap.getTerrainProximity(tile) != 1)
			{
				return room.aimap.getAItile(tile).acc == AItile.Accessibility.Climb;
			}
			return true;
		}
		if (room.aimap.getTerrainProximity(tile) != 1)
		{
			return room.aimap.getAItile(tile).acc == AItile.Accessibility.Climb;
		}
		return true;
	}

	public bool TryToGrabPrey(PhysicalObject prey)
	{
		BodyChunk bodyChunk = null;
		float a = float.MaxValue;
		for (int i = 0; i < prey.bodyChunks.Length; i++)
		{
			if (Custom.DistLess(base.mainBodyChunk.pos, prey.bodyChunks[i].pos, Mathf.Max(a, prey.bodyChunks[i].rad + base.mainBodyChunk.rad + 3f)))
			{
				a = Vector2.Distance(base.mainBodyChunk.pos, prey.bodyChunks[i].pos);
				bodyChunk = prey.bodyChunks[i];
			}
		}
		if (bodyChunk == null)
		{
			return false;
		}
		for (int j = 0; j < 2; j++)
		{
			base.bodyChunks[j].vel *= 0.75f;
			base.bodyChunks[j].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * 6f;
		}
		return Grab(prey, 0, bodyChunk.index, Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, overrideEquallyDominant: true, prey.TotalMass < base.TotalMass);
	}

	private void TakeOff(Vector2 dir)
	{
		waitToFlyCounter = 0;
		flying = true;
		int num = 0;
		Vector2 b = new Vector2(0f, 0f);
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				int terrainProximity = room.aimap.getTerrainProximity(base.abstractCreature.pos.Tile + Custom.eightDirections[i] * j);
				num += terrainProximity;
				b = Custom.eightDirections[i].ToVector2() * terrainProximity;
			}
		}
		b /= (float)num;
		float value = UnityEngine.Random.value;
		base.mainBodyChunk.vel += Vector2.Lerp(dir, b, 0.5f).normalized * 9f * value;
		base.bodyChunks[1].vel += Vector2.Lerp(dir, b, 0.8f).normalized * 7f * value;
		flyingPower = 0.5f;
		room.PlaySound(SoundID.Cicada_Wings_TakeOff, base.mainBodyChunk, loop: false, 1f, iVars.wingSoundPitch);
	}

	private void Land()
	{
		waitToFlyCounter = 0;
		flying = false;
		room.PlaySound(SoundID.Cicada_Landing, base.mainBodyChunk);
	}

	private IntVector2 FindBodySitPos(IntVector2 headPos)
	{
		if (Climbable(headPos + new IntVector2(0, -1)))
		{
			return headPos + new IntVector2(0, -1);
		}
		if (Climbable(headPos + new IntVector2(flipH, 0)))
		{
			return headPos + new IntVector2(flipH, 0);
		}
		if (Climbable(headPos + new IntVector2(-flipH, 0)))
		{
			return headPos + new IntVector2(-flipH, 0);
		}
		return headPos + new IntVector2(0, 1);
	}

	private Vector2 BodySitPosOffset(IntVector2 pos)
	{
		if (room.GetTile(pos + new IntVector2(flipH, 0)).Solid)
		{
			sitDirection = new IntVector2(flipH, 0);
			return room.MiddleOfTile(pos) + new Vector2((float)flipH * -2f, 0f);
		}
		if (room.GetTile(pos + new IntVector2(-flipH, 0)).Solid)
		{
			sitDirection = new IntVector2(-flipH, 0);
			return room.MiddleOfTile(pos) + new Vector2((float)(-flipH) * -2f, 0f);
		}
		if (room.GetTile(pos + new IntVector2(0, 1)).Solid)
		{
			sitDirection = new IntVector2(0, 1);
			return room.MiddleOfTile(pos) + new Vector2(0f, -2f);
		}
		if (room.GetTile(pos + new IntVector2(0, -1)).Solid)
		{
			sitDirection = new IntVector2(0, -1);
			return room.MiddleOfTile(pos) + new Vector2(0f, 2f);
		}
		if (room.GetTile(pos).verticalBeam)
		{
			if (!room.GetTile(pos + new IntVector2(flipH, 0)).Solid)
			{
				sitDirection = new IntVector2(-flipH, 0);
				return room.MiddleOfTile(pos) + new Vector2((float)flipH * 7f, 0f);
			}
			if (!room.GetTile(pos + new IntVector2(-flipH, 0)).Solid)
			{
				sitDirection = new IntVector2(flipH, 0);
				return room.MiddleOfTile(pos) + new Vector2((float)(-flipH) * 7f, 0f);
			}
		}
		return room.MiddleOfTile(pos);
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!base.Consious)
		{
			return;
		}
		if (Charging)
		{
			Vector2 vector = Vector2.Lerp(chargeDir * 6f, base.bodyChunks[myChunk].vel * 0.5f, 0.5f);
			if (vector.y < 0f)
			{
				vector.y *= 0.5f;
			}
			if (ModManager.MSC && otherObject is Player && (otherObject as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				otherObject.bodyChunks[otherChunk].vel += vector / otherObject.bodyChunks[otherChunk].mass * 2f;
			}
			else
			{
				otherObject.bodyChunks[otherChunk].vel += vector / otherObject.bodyChunks[otherChunk].mass;
			}
			if (otherObject is Cicada)
			{
				chargeCounter = 25;
				chargeDir = Custom.DirVec(otherObject.bodyChunks[otherChunk].pos, base.bodyChunks[myChunk].pos);
				if (!(otherObject as Cicada).Charging)
				{
					(otherObject as Creature).Stun(20);
				}
			}
			else
			{
				chargeCounter = 0;
				Stun(10);
				if (otherObject is Creature)
				{
					if (ModManager.MSC && otherObject is Player && (otherObject as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
					{
						(otherObject as Player).SaintStagger(220);
					}
					else
					{
						(otherObject as Creature).Stun(4);
					}
				}
			}
			room.PlaySound((otherObject is Player) ? SoundID.Cicada_Bump_Attack_Hit_Player : SoundID.Cicada_Bump_Attack_Hit_NPC, base.mainBodyChunk);
		}
		else if (myChunk == 0 && noStickyCounter == 0 && UnityEngine.Random.value < 0.5f && stickyCling == null && otherObject is Creature && AI.behavior == CicadaAI.Behavior.Antagonize && AI.preyTracker.MostAttractivePrey.representedCreature == (otherObject as Creature).abstractCreature)
		{
			stickyCling = otherObject.bodyChunks[otherChunk];
			chargeDir = Custom.DegToVec(Mathf.Lerp(-70f, 70f, UnityEngine.Random.value));
			room.PlaySound((stickyCling.owner is Player) ? SoundID.Cicada_Tentacles_Grab_Player : SoundID.Cicada_Tentacles_Grab_NPC, base.mainBodyChunk);
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (Charging)
		{
			if (firstContact)
			{
				room.PlaySound(SoundID.Cicada_Bump_Attack_Hit_Terrain, base.mainBodyChunk);
			}
			if (speed < 20f)
			{
				if (direction.y < 0)
				{
					base.mainBodyChunk.vel.y = Mathf.Abs(base.mainBodyChunk.vel.y);
				}
				else if (direction.y > 0)
				{
					base.mainBodyChunk.vel.y = 0f - Mathf.Abs(base.mainBodyChunk.vel.y);
				}
				if (direction.x < 0)
				{
					base.mainBodyChunk.vel.x = Mathf.Abs(base.mainBodyChunk.vel.x);
				}
				else if (direction.x > 0)
				{
					base.mainBodyChunk.vel.x = 0f - Mathf.Abs(base.mainBodyChunk.vel.x);
				}
				if (firstContact)
				{
					chargeCounter = 25;
					room.ScreenMovement(base.mainBodyChunk.pos, Vector2.ClampMagnitude(direction.ToVector2() * (speed / 25f), 1.5f), Mathf.Min(speed * 0.01f, 0.3f));
					base.mainBodyChunk.vel -= direction.ToVector2();
				}
				chargeDir = base.mainBodyChunk.vel.normalized;
			}
			else
			{
				Stun(20);
				if (firstContact)
				{
					room.ScreenMovement(base.mainBodyChunk.pos, Vector2.ClampMagnitude(direction.ToVector2() * (speed / 25f), 2.5f), Mathf.Min(speed * 0.02f, 0.7f));
				}
			}
		}
		else if (speed > 1.5f && firstContact)
		{
			room.PlaySound((speed < 8f) ? SoundID.Cicada_Light_Terrain_Impact : SoundID.Cicada_Heavy_Terrain_Impact, base.mainBodyChunk);
		}
	}

	public override void Die()
	{
		base.Die();
	}

	public override Color ShortCutColor()
	{
		return iVars.color.rgb;
	}

	public override void Stun(int st)
	{
		flying = false;
		chargeCounter = 0;
		if (UnityEngine.Random.value < 0.5f)
		{
			LoseAllGrasps();
		}
		base.Stun(st);
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = newRoom.ShorcutEntranceHoleDirection(pos).ToVector2();
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - vector * (-1.5f + (float)i) * 15f;
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 8f;
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public override void RecreateSticksFromAbstract()
	{
		for (int i = 0; i < base.abstractCreature.stuckObjects.Count; i++)
		{
			if (base.abstractCreature.stuckObjects[i] is AbstractPhysicalObject.CreatureGripStick && base.abstractCreature.stuckObjects[i].A == base.abstractCreature && base.abstractCreature.stuckObjects[i].B.realizedObject != null)
			{
				AbstractPhysicalObject.CreatureGripStick creatureGripStick = base.abstractCreature.stuckObjects[i] as AbstractPhysicalObject.CreatureGripStick;
				base.grasps[creatureGripStick.grasp] = new Grasp(this, creatureGripStick.B.realizedObject, creatureGripStick.grasp, UnityEngine.Random.Range(0, creatureGripStick.B.realizedObject.bodyChunks.Length), Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, creatureGripStick.B.realizedObject.TotalMass < base.TotalMass);
				creatureGripStick.B.realizedObject.Grabbed(base.grasps[creatureGripStick.grasp]);
			}
		}
	}
}
