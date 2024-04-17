using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class BigEel : Creature
{
	public struct IndividualVariations
	{
		public float patternDisplacement;

		public int finsSeed;

		public HSLColor patternColorA;

		public HSLColor patternColorB;

		public IndividualVariations(float patternDisplacement, int finsSeed, HSLColor patternColorA, HSLColor patternColorB)
		{
			this.patternDisplacement = patternDisplacement;
			this.finsSeed = finsSeed;
			this.patternColorA = patternColorA;
			this.patternColorB = patternColorB;
		}
	}

	private class ClampedObject
	{
		public BodyChunk chunk;

		public float distance;

		public ClampedObject(BodyChunk chunk, float distance)
		{
			this.chunk = chunk;
			this.distance = distance;
		}
	}

	public BigEelAI AI;

	public Vector2 swimDir;

	public float swimMotion;

	public float jawCharge;

	public bool chargeJaw;

	public bool snapFrame;

	public float jawChargeFatigue;

	public Vector2? attackPos;

	public float beakGap;

	public float swimSpeed;

	public bool albino;

	public IndividualVariations iVars;

	private List<ClampedObject> clampedObjects;

	private List<PlacedObject> antiStrandingZones;

	private void GenerateIVars()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.abstractCreature.ID.RandomSeed);
		float num = Custom.WrappedRandomVariation(0.65f, 0.2f, 0.8f);
		iVars = new IndividualVariations(UnityEngine.Random.value, UnityEngine.Random.Range(0, int.MaxValue), new HSLColor(num, Mathf.Lerp(0.5f, 0.95f, UnityEngine.Random.value), Mathf.Lerp(0.12f, 0.18f, UnityEngine.Random.value)), new HSLColor(num + ((UnityEngine.Random.value < 0.5f) ? (-0.15f) : 0.15f), 1f, 0.2f));
		UnityEngine.Random.state = state;
	}

	public BigEel(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		GenerateIVars();
		albino = (world.region != null && world.region.regionParams.albinos) || (ModManager.MMF && world.game.IsArenaSession && UnityEngine.Random.value <= 0.04f);
		if (albino)
		{
			iVars.patternColorB = new HSLColor(0f, 0.6f, 0.75f);
			iVars.patternColorA.hue = 0.5f;
			iVars.patternColorA = HSLColor.Lerp(iVars.patternColorA, new HSLColor(0.97f, 0.8f, 0.75f), 0.9f);
		}
		collisionRange = 1000f;
		base.bodyChunks = new BodyChunk[20];
		bodyChunkConnections = new BodyChunkConnection[base.bodyChunks.Length - 1];
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			float num = (float)i / (float)(base.bodyChunks.Length - 1);
			num = (1f - num) * 0.5f + Mathf.Sin(Mathf.Pow(num, 0.5f) * (float)Math.PI) * 0.5f;
			base.bodyChunks[i] = new BodyChunk(this, i, new Vector2(0f, 0f), Mathf.Lerp(10f, 60f, num), Mathf.Lerp(0.5f, 20f, num));
			base.bodyChunks[i].restrictInRoomRange = 2000f;
			base.bodyChunks[i].defaultRestrictInRoomRange = 2000f;
		}
		for (int j = 0; j < base.bodyChunks.Length - 1; j++)
		{
			bodyChunkConnections[j] = new BodyChunkConnection(base.bodyChunks[j], base.bodyChunks[j + 1], Mathf.Max(base.bodyChunks[j].rad, base.bodyChunks[j + 1].rad), BodyChunkConnection.Type.Normal, 1f, -1f);
		}
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.98f;
		waterRetardationImmunity = 0.1f;
		base.buoyancy = 0.9f;
		base.GoThroughFloors = true;
		antiStrandingZones = new List<PlacedObject>();
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new BigEelGraphics(this);
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		jawCharge = 0f;
		clampedObjects = new List<ClampedObject>();
		impactTreshhold = 500f;
		antiStrandingZones.Clear();
		for (int i = 0; i < newRoom.roomSettings.placedObjects.Count; i++)
		{
			if (newRoom.roomSettings.placedObjects[i].type == PlacedObject.Type.NoLeviathanStrandingZone)
			{
				antiStrandingZones.Add(newRoom.roomSettings.placedObjects[i]);
			}
		}
	}

	public override void Update(bool eu)
	{
		if (impactTreshhold > 1f)
		{
			impactTreshhold -= 1f;
		}
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (room.game.devToolsActive && room.game.cameras[0].room == room)
		{
			if (Input.GetKey("b"))
			{
				base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
				Stun(12);
			}
			if (Input.GetKey("n"))
			{
				for (int i = 0; i < base.bodyChunks.Length; i++)
				{
					base.bodyChunks[i].HardSetPosition((Vector2)Futile.mousePosition + room.game.cameras[0].pos + Custom.RNV() * 3f);
				}
			}
		}
		if (antiStrandingZones.Count > 0)
		{
			for (int j = 0; j < antiStrandingZones.Count; j++)
			{
				if (!Custom.DistLess(base.mainBodyChunk.pos, antiStrandingZones[j].pos, 1000f))
				{
					continue;
				}
				float rad = (antiStrandingZones[j].data as PlacedObject.ResizableObjectData).Rad;
				Vector2 normalized = (antiStrandingZones[j].data as PlacedObject.ResizableObjectData).handlePos.normalized;
				for (int k = 0; k < base.bodyChunks.Length - 2; k++)
				{
					if (Custom.DistLess(base.bodyChunks[k].pos, antiStrandingZones[j].pos, rad))
					{
						base.bodyChunks[k].vel += normalized * 1.4f * Mathf.InverseLerp(rad, rad / 5f, Vector2.Distance(base.bodyChunks[k].pos, antiStrandingZones[j].pos));
					}
				}
			}
		}
		for (int l = 0; l < base.bodyChunks.Length - 2; l++)
		{
			Vector2 vector = Custom.DirVec(base.bodyChunks[l].pos, base.bodyChunks[l + 2].pos);
			float num = base.bodyChunks[l + 2].mass / (base.bodyChunks[l].mass + base.bodyChunks[l + 2].mass);
			base.bodyChunks[l].vel -= vector * 0.15f * num;
			base.bodyChunks[l + 2].vel += vector * 0.15f * (1f - num);
		}
		if (base.Consious)
		{
			Act(eu);
		}
		if (base.grasps[0] != null)
		{
			CarryObject(eu);
		}
	}

	private void Act(bool eu)
	{
		AI.Update();
		float num = jawCharge;
		if (jawCharge == 0f && jawChargeFatigue > 0f)
		{
			jawChargeFatigue = Mathf.Max(jawChargeFatigue - 1f / 120f, 0f);
		}
		else if (jawCharge > 0.3f || (jawChargeFatigue < 1f && AI.WantToChargeJaw()))
		{
			jawCharge += 1f / 120f;
		}
		else
		{
			jawCharge = Mathf.Max(jawCharge - 1f / 120f, 0f);
		}
		if (num == 0f && jawCharge > 0f)
		{
			room.PlaySound(SoundID.Leviathan_Deploy_Jaws, base.mainBodyChunk);
		}
		else if (jawCharge >= 0.25f && num < 0.25f)
		{
			room.PlaySound(SoundID.Leviathan_Jaws_Armed, base.mainBodyChunk);
		}
		snapFrame = false;
		if (jawCharge > 0.3f && num <= 0.3f && !AI.WantToSnapJaw())
		{
			jawCharge = 0.3f;
			jawChargeFatigue += 0.025f;
		}
		if ((double)num <= 0.35 && jawCharge > 0.35f)
		{
			JawsSnap();
			jawChargeFatigue = 1f;
		}
		if (jawCharge >= 1f)
		{
			jawCharge = 0f;
			Swallow();
		}
		Vector2 pos = base.mainBodyChunk.pos;
		Vector2 vector = Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos);
		for (int i = 0; i < clampedObjects.Count; i++)
		{
			Vector2 vector2 = pos + vector * (clampedObjects[i].distance - Mathf.InverseLerp(0.4f, 0.7f, jawCharge) * 60f - Mathf.InverseLerp(0.7f, 1f, jawCharge) * 40f);
			clampedObjects[i].chunk.MoveFromOutsideMyUpdate(eu, vector2);
			if (!(jawCharge > 0.6f))
			{
				continue;
			}
			for (int j = 0; j < clampedObjects[i].chunk.owner.bodyChunks.Length; j++)
			{
				clampedObjects[i].chunk.owner.bodyChunks[j].vel *= 1f - Mathf.InverseLerp(0.6f, 0.8f, jawCharge);
				clampedObjects[i].chunk.owner.bodyChunks[j].MoveFromOutsideMyUpdate(eu, Vector2.Lerp(clampedObjects[i].chunk.owner.bodyChunks[j].pos, vector2, Mathf.InverseLerp(0.6f, 0.8f, jawCharge)));
			}
			if (clampedObjects[i].chunk.owner.graphicsModule != null && clampedObjects[i].chunk.owner.graphicsModule.bodyParts != null)
			{
				for (int k = 0; k < clampedObjects[i].chunk.owner.graphicsModule.bodyParts.Length; k++)
				{
					clampedObjects[i].chunk.owner.graphicsModule.bodyParts[k].vel *= 1f - Mathf.InverseLerp(0.6f, 0.8f, jawCharge);
					clampedObjects[i].chunk.owner.graphicsModule.bodyParts[k].pos = Vector2.Lerp(clampedObjects[i].chunk.owner.graphicsModule.bodyParts[k].pos, vector2, Mathf.InverseLerp(0.6f, 0.8f, jawCharge));
				}
			}
		}
		Swim();
	}

	private void JawsSnap()
	{
		snapFrame = true;
		room.PlaySound(SoundID.Leviathan_Bite, base.mainBodyChunk);
		room.ScreenMovement(base.mainBodyChunk.pos, new Vector2(0f, 0f), 1.3f);
		for (int i = 1; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].vel += (Custom.RNV() + Custom.DirVec(base.bodyChunks[i - 1].pos, base.bodyChunks[i].pos)) * Mathf.Sin(Mathf.InverseLerp(1f, 11f, i) * (float)Math.PI) * 8f;
		}
		Vector2 pos = base.mainBodyChunk.pos;
		Vector2 vector = Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos);
		beakGap = 0f;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int j = 0; j < room.physicalObjects.Length; j++)
		{
			for (int num = room.physicalObjects[j].Count - 1; num >= 0; num--)
			{
				if (!(room.physicalObjects[j][num] is BigEel))
				{
					for (int k = 0; k < room.physicalObjects[j][num].bodyChunks.Length; k++)
					{
						if (!InBiteArea(room.physicalObjects[j][num].bodyChunks[k].pos, room.physicalObjects[j][num].bodyChunks[k].rad / 2f))
						{
							continue;
						}
						Vector2 b = Custom.ClosestPointOnLine(pos, pos + vector, room.physicalObjects[j][num].bodyChunks[k].pos);
						if (ModManager.MSC && room.physicalObjects[j][num] is Overseer && (room.physicalObjects[j][num] as Overseer).SafariOverseer)
						{
							continue;
						}
						if (!ModManager.MSC || (!(room.physicalObjects[j][num] is BigJellyFish) && !(room.physicalObjects[j][num] is EnergyCell)))
						{
							clampedObjects.Add(new ClampedObject(room.physicalObjects[j][num].bodyChunks[k], Vector2.Distance(pos, b)));
							Custom.Log($"Caught: {room.physicalObjects[j][num]}");
						}
						if (ModManager.MSC && room.physicalObjects[j][num] is EnergyCell)
						{
							(room.physicalObjects[j][num] as EnergyCell).Explode();
						}
						if (room.physicalObjects[j][num].bodyChunks[k].rad > beakGap)
						{
							beakGap = room.physicalObjects[j][num].bodyChunks[k].rad;
						}
						if (room.physicalObjects[j][num] is Creature)
						{
							(room.physicalObjects[j][num] as Creature).Die();
							if (room.physicalObjects[j][num] is Player)
							{
								flag3 = true;
							}
							else
							{
								flag = true;
							}
						}
						else
						{
							flag2 = true;
						}
						if (base.graphicsModule != null)
						{
							if (room.physicalObjects[j][num] is IDrawable)
							{
								base.graphicsModule.AddObjectToInternalContainer(room.physicalObjects[j][num] as IDrawable, 0);
							}
							else if (room.physicalObjects[j][num].graphicsModule != null)
							{
								base.graphicsModule.AddObjectToInternalContainer(room.physicalObjects[j][num].graphicsModule, 0);
							}
						}
					}
				}
			}
		}
		if (flag)
		{
			room.PlaySound(SoundID.Leviathan_Crush_NPC, base.mainBodyChunk);
		}
		if (flag2)
		{
			room.PlaySound(SoundID.Leviathan_Crush_Non_Organic_Object, base.mainBodyChunk);
		}
		if (flag3)
		{
			room.PlaySound(SoundID.Leviathan_Crush_Player, base.mainBodyChunk);
		}
		for (float num2 = 20f; num2 < 100f; num2 += 1f)
		{
			if (room.GetTile(pos + vector * num2).Solid)
			{
				room.PlaySound(SoundID.Leviathan_Clamper_Hit_Terrain, base.mainBodyChunk.pos);
				break;
			}
		}
		for (int l = 0; l < clampedObjects.Count; l++)
		{
			clampedObjects[l].chunk.owner.ChangeCollisionLayer(0);
			Crush(clampedObjects[l].chunk.owner);
		}
	}

	private void Crush(PhysicalObject obj)
	{
		for (int i = 0; i < obj.bodyChunkConnections.Length; i++)
		{
			obj.bodyChunkConnections[i].type = BodyChunkConnection.Type.Pull;
		}
	}

	private void Swallow()
	{
		for (int i = 0; i < clampedObjects.Count; i++)
		{
			clampedObjects[i].chunk.owner.Destroy();
			if (clampedObjects[i].chunk.owner is Creature && !(clampedObjects[i].chunk.owner as Creature).Template.smallCreature)
			{
				AI.hungerDelay = Math.Max(AI.hungerDelay, 600);
			}
		}
		clampedObjects.Clear();
	}

	public void AccessSwimSpace(WorldCoordinate start, WorldCoordinate dest)
	{
		room.game.shortcuts.CreatureTakeFlight(this, AbstractRoomNode.Type.SeaExit, start, dest);
	}

	public bool InBiteArea(Vector2 pos, float margin)
	{
		Vector2 vector = Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos);
		if (!Custom.DistLess(base.mainBodyChunk.pos + vector * 60f, pos, 300f + margin))
		{
			return false;
		}
		Vector2 pos2 = base.mainBodyChunk.pos;
		Vector2 vector2 = Custom.PerpendicularVector(vector);
		if (Mathf.Abs(Custom.DistanceToLine(pos, pos2 + 60f * vector - vector2, pos2 + 60f * vector + vector2)) > 60f + margin)
		{
			return false;
		}
		if (Mathf.Abs(Custom.DistanceToLine(pos, pos2 - vector, pos2 + vector)) > 50f + margin)
		{
			return false;
		}
		return room.VisualContact(pos2, pos);
	}

	private void Swim()
	{
		if (AI.behavior == BigEelAI.Behavior.Idle && room.IsPositionInsideBoundries(base.abstractCreature.pos.Tile) && AI.pathFinder.GetDestination.room == room.abstractRoom.index)
		{
			swimSpeed = Mathf.Max(swimSpeed - 1f / 160f, 0.5f);
		}
		else
		{
			swimSpeed = Mathf.Min(swimSpeed + 1f / 160f, 1f);
		}
		swimMotion -= 1f / Mathf.Lerp(220f, 60f, swimSpeed);
		MovementConnection movementConnection = default(MovementConnection);
		if (base.safariControlled)
		{
			if (inputWithDiagonals.HasValue)
			{
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 240f), 2);
					if (!attackPos.HasValue && jawCharge > 0f)
					{
						attackPos = base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 240f;
					}
				}
			}
		}
		else
		{
			movementConnection = (AI.pathFinder as BigEelPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
		}
		if (movementConnection != default(MovementConnection))
		{
			swimDir = Vector3.Slerp(swimDir, Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection.DestTile)), 0.3f);
		}
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			float num = (float)i / (float)(base.bodyChunks.Length - 1);
			base.bodyChunks[i].vel += swimDir * Mathf.Lerp(1f, -1f, Mathf.Pow(num, 0.5f)) * 0.125f * swimSpeed;
			if (i < base.bodyChunks.Length - 1)
			{
				Vector2 vector = Custom.DirVec(base.bodyChunks[i + 1].pos, base.bodyChunks[i].pos);
				base.bodyChunks[i].vel += vector * 0.125f * Mathf.Lerp(0.5f, 1f, swimSpeed);
				base.bodyChunks[i].vel += Custom.PerpendicularVector(vector) * Mathf.Sin((swimMotion + num * 2f) * (float)Math.PI * 2f) * 0.5f * num * Mathf.Lerp(0.5f, 1f, swimSpeed);
			}
			else
			{
				base.bodyChunks[i].vel -= swimDir * swimSpeed;
			}
		}
		base.mainBodyChunk.vel += swimDir * 0.15f;
		base.bodyChunks[1].vel -= swimDir * 0.05f * swimSpeed;
		if (base.mainBodyChunk.ContactPoint.x != 0 || base.mainBodyChunk.ContactPoint.y != 0)
		{
			base.mainBodyChunk.vel += swimDir * 3f * swimSpeed;
			base.bodyChunks[1].vel -= swimDir * 1.5f * swimSpeed;
		}
		if (attackPos.HasValue && jawCharge > 0f)
		{
			float num2 = Mathf.InverseLerp(400f, 150f, Vector2.Distance(base.mainBodyChunk.pos, attackPos.Value)) * (1f - jawChargeFatigue) * Mathf.InverseLerp(-1f, 1f, Vector2.Dot((base.bodyChunks[1].pos - base.mainBodyChunk.pos).normalized, (base.bodyChunks[1].pos - attackPos.Value).normalized));
			base.mainBodyChunk.vel *= Mathf.Lerp(1f, 0.9f, base.mainBodyChunk.submersion * num2);
			base.bodyChunks[1].vel *= Mathf.Lerp(1f, 0.9f, base.bodyChunks[1].submersion * num2);
			Vector2 vector2 = Custom.DirVec(attackPos.Value, base.mainBodyChunk.pos);
			base.mainBodyChunk.vel += Vector2.ClampMagnitude(attackPos.Value + vector2 * 60f - base.mainBodyChunk.pos, 25f) / 15f * num2;
			base.bodyChunks[1].vel += Vector2.ClampMagnitude(attackPos.Value + vector2 * (60f + bodyChunkConnections[0].distance) - base.bodyChunks[1].pos, 10f) / 15f * num2;
			for (int j = 2; j < base.bodyChunks.Length; j++)
			{
				base.bodyChunks[j].vel += vector2 * num2 * base.bodyChunks[j].submersion * 0.5f / Mathf.Lerp(j, 3f, 0.3f);
			}
			attackPos = null;
		}
	}

	public void CarryObject(bool eu)
	{
	}

	public bool AmIHoldingCreature(AbstractCreature crit)
	{
		for (int i = 0; i < clampedObjects.Count; i++)
		{
			if (clampedObjects[i].chunk.owner is Creature && (clampedObjects[i].chunk.owner as Creature).abstractCreature == crit)
			{
				return true;
			}
		}
		return false;
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (speed > 1.5f && firstContact)
		{
			room.PlaySound((speed < 8f) ? SoundID.Leviathan_Light_Terrain_Impact : SoundID.Leviathan_Heavy_Terrain_Impact, base.mainBodyChunk);
		}
	}

	public override void Die()
	{
		base.Die();
		if (ModManager.MMF)
		{
			if (jawCharge >= 1f)
			{
				jawCharge = 0f;
				Swallow();
			}
			jawChargeFatigue = 0f;
			chargeJaw = false;
			snapFrame = false;
		}
	}

	public override Color ShortCutColor()
	{
		return new Color(1f, 0f, 1f);
	}

	public override void Stun(int st)
	{
		if (UnityEngine.Random.value < 0.5f)
		{
			LoseAllGrasps();
		}
		base.Stun(st);
	}
}
