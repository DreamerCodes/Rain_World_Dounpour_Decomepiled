using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class GarbageWorm : Creature
{
	public int hole;

	public Vector2 rootPos;

	public Tentacle tentacle;

	public Vector2 lookPoint;

	private float retractSpeed;

	public float extended;

	public bool grabSpears = true;

	public Vector2? chargePos;

	public ChunkDynamicSoundLoop sound;

	private bool lastExtended;

	public InputCircularRegion controlRegion;

	public GarbageWormAI AI => base.abstractCreature.abstractAI.RealAI as GarbageWormAI;

	public float bodySize => State.bodySize;

	public new GarbageWormState State => base.abstractCreature.state as GarbageWormState;

	public GarbageWorm(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 8f, 0.05f);
		base.bodyChunks[1] = new BodyChunk(this, 0, new Vector2(0f, 0f), 8f, 0.05f);
		base.bodyChunks[0].rotationChunk = base.bodyChunks[1];
		base.bodyChunks[1].collideWithTerrain = false;
		bodyChunkConnections = new BodyChunkConnection[0];
		tentacle = new Tentacle(this, base.bodyChunks[1], 400f * bodySize);
		tentacle.tProps = new Tentacle.TentacleProps(stiff: false, rope: false, shorten: true, 0.5f, 0f, 1.4f, 0f, 0f, 1.2f, 10f, 0.25f, 5f, 15, 60, 12, 0);
		tentacle.tChunks = new Tentacle.TentacleChunk[(int)(15f * Mathf.Lerp(bodySize, 1f, 0.5f))];
		for (int i = 0; i < tentacle.tChunks.Length; i++)
		{
			tentacle.tChunks[i] = new Tentacle.TentacleChunk(tentacle, i, (float)(i + 1) / (float)tentacle.tChunks.Length, 2f * Mathf.Lerp(bodySize, 1f, 0.5f));
		}
		tentacle.stretchAndSqueeze = 0.1f;
		base.GoThroughFloors = true;
		base.airFriction = 0.99f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.47f;
		collisionLayer = 1;
		base.waterFriction = 0.92f;
		base.buoyancy = 0.95f;
		extended = 1f;
		retractSpeed = 1f;
		lastExtended = true;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new GarbageWormGraphics(this);
		}
	}

	public override void NewRoom(Room room)
	{
		tentacle.NewRoom(room);
		base.NewRoom(room);
		NewHole(burrowed: false);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		base.abstractCreature.pos.Tile = room.GetTilePosition(rootPos);
		if (room == null || enteringShortCut.HasValue)
		{
			return;
		}
		tentacle.Update();
		tentacle.limp = !base.Consious;
		tentacle.retractFac = 1f - extended;
		if (sound == null)
		{
			sound = new ChunkDynamicSoundLoop(base.mainBodyChunk);
		}
		sound.Update();
		if (AI.attackCounter > 40 && AI.attackCounter < 190)
		{
			sound.sound = SoundID.Garbage_Worm_Swallowing_LOOP;
			sound.Volume = 1f;
			sound.Pitch = 1f;
		}
		else
		{
			sound.sound = (AI.showAsAngry ? SoundID.Garbage_Worm_Upset_LOOP : SoundID.Garbage_Worm_Curious_LOOP);
			sound.Volume = AI.stress * extended;
			sound.Pitch = Mathf.Lerp(0.9f, 1.1f, AI.stress);
		}
		if (extended == 0f && lastExtended)
		{
			room.PlaySound(SoundID.Garbage_Worm_Withdraw, base.mainBodyChunk.pos);
			if (room.water && room.waterObject != null)
			{
				room.waterObject.WaterfallHitSurface(base.mainBodyChunk.pos.x - 15f, base.mainBodyChunk.pos.x + 15f, 40f);
			}
		}
		if (extended > 0f && !lastExtended)
		{
			room.PlaySound(SoundID.Garbage_Worm_Emerge, base.mainBodyChunk.pos);
		}
		lastExtended = extended > 0f;
		base.mainBodyChunk.collideWithTerrain = extended == 1f && tentacle.backtrackFrom == -1;
		canBeHitByWeapons = extended > 0f;
		if (!Custom.DistLess(base.bodyChunks[1].pos, base.bodyChunks[0].pos, tentacle.idealLength * 2f * extended))
		{
			base.bodyChunks[0].pos = base.bodyChunks[1].pos + Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * tentacle.idealLength * 2f * extended;
		}
		if (room.game.devToolsActive && Input.GetKey("b"))
		{
			base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 3f;
		}
		if (base.Consious)
		{
			if (chargePos.HasValue)
			{
				lookPoint = chargePos.Value + Custom.DirVec(base.mainBodyChunk.pos, chargePos.Value) * 100f;
				base.mainBodyChunk.vel += Vector2.ClampMagnitude(chargePos.Value - base.mainBodyChunk.pos, 20f) / 5f;
				chargePos = null;
			}
			extended += retractSpeed;
			extended = Mathf.Clamp(extended, 0f, 1f);
			if (!base.safariControlled)
			{
				base.abstractCreature.abstractAI.RealAI.Update();
			}
			if (retractSpeed < 0f)
			{
				lookPoint = rootPos + new Vector2(0f, 1000f);
			}
			List<IntVector2> path = null;
			if (base.safariControlled)
			{
				controlRegion.drawDebugPos = base.bodyChunks[1].pos;
				AI.searchCounter = 0;
				if (!AI.showAsAngry)
				{
					controlRegion.speed = 5f;
				}
				else
				{
					controlRegion.speed = 10f;
				}
				controlRegion.Update(inputWithDiagonals);
				Vector2 p = base.bodyChunks[0].pos - base.bodyChunks[1].pos;
				float num = Custom.Dist(p, controlRegion.pos);
				if (num > 60f)
				{
					controlRegion.pos.x = Mathf.Lerp(controlRegion.pos.x, p.x, Mathf.Lerp(0f, 0.2f, (num - 60f) / 200f));
					controlRegion.pos.y = Mathf.Lerp(controlRegion.pos.y, p.y, Mathf.Lerp(0f, 0.2f, (num - 60f) / 200f));
				}
				if (inputWithDiagonals.HasValue)
				{
					if (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0)
					{
						tentacle.MoveGrabDest(base.bodyChunks[1].pos + controlRegion.pos, ref path);
					}
					if (inputWithDiagonals.Value.jmp && !lastInputWithDiagonals.Value.jmp)
					{
						AI.showAsAngry = !AI.showAsAngry;
					}
					if (inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw)
					{
						for (int i = 0; i < base.grasps.Length; i++)
						{
							if (base.grasps[i] != null && base.grasps[i].grabbed != null)
							{
								ReleaseGrasp(i);
							}
						}
					}
					bool flag = false;
					for (int j = 0; j < base.grasps.Length; j++)
					{
						if (base.grasps[j] != null && base.grasps[j].grabbed != null)
						{
							flag = true;
							break;
						}
					}
					if (inputWithDiagonals.Value.pckp && !lastInputWithDiagonals.Value.pckp && !AI.showAsAngry && !flag)
					{
						Weapon weapon = null;
						Creature creature = null;
						for (int k = 0; k < base.abstractCreature.Room.creatures.Count; k++)
						{
							if (base.abstractCreature == base.abstractCreature.Room.creatures[k] || base.abstractCreature.Room.creatures[k].realizedCreature == null)
							{
								continue;
							}
							creature = base.abstractCreature.Room.creatures[k].realizedCreature;
							if (creature.grasps == null)
							{
								continue;
							}
							for (int l = 0; l < creature.grasps.Length; l++)
							{
								if (creature.grasps[l] != null && creature.grasps[l].grabbed is Spear && Custom.DistLess(base.mainBodyChunk.pos, creature.grasps[l].grabbed.firstChunk.pos, 48f))
								{
									weapon = creature.grasps[l].grabbed as Weapon;
									break;
								}
							}
							if (weapon != null)
							{
								break;
							}
						}
						if (weapon == null && room != null)
						{
							creature = null;
							for (int m = 0; m < room.physicalObjects.Length; m++)
							{
								for (int n = 0; n < room.physicalObjects[m].Count; n++)
								{
									if (room.physicalObjects[m][n] is Spear && Custom.DistLess(base.mainBodyChunk.pos, room.physicalObjects[m][n].firstChunk.pos, 48f))
									{
										weapon = room.physicalObjects[m][n] as Weapon;
										break;
									}
								}
							}
						}
						if (weapon != null && room != null)
						{
							weapon.AllGraspsLetGoOfThisObject(evenNonExlusive: true);
							Grab(weapon, 0, 0, Grasp.Shareability.NonExclusive, 0.1f, overrideEquallyDominant: true, pacifying: true);
							room.PlaySound(SoundID.Garbage_Worm_Snatch_Spear, base.mainBodyChunk);
							weapon.Forbid();
							if (creature != null)
							{
								Creature creature2 = null;
								int num2 = -1;
								for (int num3 = 0; num3 < weapon.grabbedBy.Count; num3++)
								{
									if (weapon.grabbedBy[num3].grabber == creature)
									{
										creature2 = creature;
										num2 = weapon.grabbedBy[num3].graspUsed;
									}
								}
								if (creature2 == null && weapon.grabbedBy.Count > 0)
								{
									creature2 = weapon.grabbedBy[0].grabber;
									num2 = weapon.grabbedBy[0].graspUsed;
								}
								if (creature2 != null && num2 > -1)
								{
									creature.ReleaseGrasp(num2);
								}
							}
						}
					}
					if (inputWithDiagonals.Value.pckp && AI.showAsAngry && !flag)
					{
						for (int num4 = 0; num4 < base.abstractCreature.Room.creatures.Count; num4++)
						{
							if (base.abstractCreature.Room.creatures[num4].realizedCreature == null)
							{
								continue;
							}
							Creature realizedCreature = base.abstractCreature.Room.creatures[num4].realizedCreature;
							for (int num5 = 0; num5 < realizedCreature.bodyChunks.Length; num5++)
							{
								if (realizedCreature.abstractCreature != base.abstractCreature && Custom.DistLess(base.mainBodyChunk.pos, realizedCreature.bodyChunks[num5].pos, 10f + realizedCreature.bodyChunks[num5].rad))
								{
									Grab(realizedCreature, 0, num5, Grasp.Shareability.NonExclusive, 0.1f, overrideEquallyDominant: false, pacifying: false);
									room.PlaySound(SoundID.Garbage_Worm_Grab_Creature, base.mainBodyChunk);
									break;
								}
							}
						}
					}
				}
			}
			else
			{
				tentacle.MoveGrabDest(lookPoint, ref path);
			}
			float value = Vector2.Distance(tentacle.Tip.pos, lookPoint);
			if (AI.attackCounter == 0)
			{
				tentacle.tProps.goalAttractionSpeedTip = Mathf.Lerp(0.15f, 1.9f, Mathf.InverseLerp(40f, AI.searchingGarbage ? 90f : 290f, value));
				if (tentacle.backtrackFrom == -1 && room.aimap.getTerrainProximity(base.mainBodyChunk.pos) < 2)
				{
					for (int num6 = 0; num6 < 8; num6++)
					{
						if (room.aimap.getTerrainProximity(room.GetTilePosition(base.mainBodyChunk.pos) + Custom.eightDirections[num6]) > 1)
						{
							tentacle.Tip.vel += Custom.eightDirections[num6].ToVector2() * 2f;
							break;
						}
					}
				}
			}
			else if (AI.attackCounter < 20)
			{
				tentacle.tProps.goalAttractionSpeedTip = 0.1f;
			}
			else if (AI.attackCounter < 40)
			{
				tentacle.tProps.goalAttractionSpeedTip = 40f;
				base.mainBodyChunk.vel += Vector2.ClampMagnitude(lookPoint - base.mainBodyChunk.pos, 30f) / 1f;
			}
			else if (AI.attackCounter < 190)
			{
				base.mainBodyChunk.pos = lookPoint;
			}
			else
			{
				lookPoint.y += 20f;
				tentacle.tProps.goalAttractionSpeedTip = 0.01f;
			}
			for (int num7 = 0; num7 < tentacle.tChunks.Length; num7++)
			{
				if (tentacle.backtrackFrom == -1 || num7 < tentacle.backtrackFrom)
				{
					float num8 = ((float)num7 + 0.5f) / (float)tentacle.tChunks.Length;
					tentacle.tChunks[num7].vel *= Mathf.Lerp(0.9f, 0.99f, num8);
					if (AI.attackCounter > 20 || extended < 1f)
					{
						tentacle.tChunks[num7].vel.y += 0.5f;
					}
					else
					{
						tentacle.tChunks[num7].vel.y += (1f - num8) * 0.5f;
					}
					float num9 = Mathf.Sin((float)Math.PI * Mathf.Pow(num8, 2f)) * 0.1f;
					if (AI.CurrentlyLookingAtScaryCreature())
					{
						num9 *= 3f * Mathf.InverseLerp(220f, 20f, value);
					}
					else if (AI.attackCounter > 0 && AI.attackCounter < 20)
					{
						num9 *= 3f;
					}
					tentacle.tChunks[num7].vel += Custom.DirVec(lookPoint, tentacle.tChunks[num7].pos) * num9;
					if (num7 > 1)
					{
						tentacle.tChunks[num7].vel += Custom.DirVec(tentacle.tChunks[num7 - 2].pos, tentacle.tChunks[num7].pos) * 0.2f;
						tentacle.tChunks[num7 - 2].vel -= Custom.DirVec(tentacle.tChunks[num7 - 2].pos, tentacle.tChunks[num7].pos) * 0.2f;
					}
				}
			}
		}
		else
		{
			for (int num10 = 0; num10 < tentacle.tChunks.Length; num10++)
			{
				if (tentacle.backtrackFrom == -1 || num10 < tentacle.backtrackFrom)
				{
					tentacle.tChunks[num10].vel *= 0.95f;
					tentacle.tChunks[num10].vel.y -= 0.5f;
				}
			}
		}
		if (extended == 0f)
		{
			sound.Volume = 0f;
			if (!lastExtended)
			{
				for (int num11 = 0; num11 < 2; num11++)
				{
					base.bodyChunks[num11].collideWithTerrain = false;
					base.bodyChunks[num11].pos = rootPos + new Vector2(0f, -50f);
					base.bodyChunks[num11].vel *= 0f;
				}
			}
			else
			{
				base.bodyChunks[0].collideWithTerrain = false;
				base.bodyChunks[0].pos = rootPos + new Vector2(0f, -50f);
				base.bodyChunks[0].vel *= 0f;
			}
			for (int num12 = base.abstractCreature.stuckObjects.Count - 1; num12 >= 0; num12--)
			{
				if (base.abstractCreature.stuckObjects[num12] is AbstractPhysicalObject.AbstractSpearStick && base.abstractCreature.stuckObjects[num12].A.realizedObject != null && base.abstractCreature.stuckObjects[num12].A.realizedObject is Weapon)
				{
					base.abstractCreature.stuckObjects[num12].A.realizedObject.Destroy();
					Custom.Log("destroy stuck spear");
					base.abstractCreature.stuckObjects[num12].Deactivate();
				}
			}
		}
		else
		{
			base.bodyChunks[1].pos = rootPos;
			base.bodyChunks[1].vel *= 0f;
			for (int num13 = 0; num13 < 2; num13++)
			{
				base.bodyChunks[num13].collideWithTerrain = true;
			}
			float num14 = 0f;
			if (tentacle.backtrackFrom == -1)
			{
				num14 = 0.5f;
			}
			else if (!base.Consious)
			{
				num14 = 0.7f;
			}
			Vector2 vector = Custom.DirVec(base.bodyChunks[0].pos, tentacle.Tip.pos);
			float num15 = Vector2.Distance(base.bodyChunks[0].pos, tentacle.Tip.pos);
			base.bodyChunks[0].pos -= (0f - num15) * vector * (1f - num14);
			base.bodyChunks[0].vel -= (0f - num15) * vector * (1f - num14);
			tentacle.Tip.pos += (0f - num15) * vector * num14;
			tentacle.Tip.vel += (0f - num15) * vector * num14;
			if (base.Consious)
			{
				base.bodyChunks[0].vel.y += base.gravity;
			}
		}
		if (base.grasps[0] != null)
		{
			Carry(eu);
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
	}

	public override void Stun(int st)
	{
		base.Stun(st);
		ReleaseGrasp(0);
	}

	public override void Die()
	{
		base.Die();
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		if (source != null && source.owner is Weapon)
		{
			for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
			{
				if (room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.Slugcat && !State.angryAt.Contains(room.abstractRoom.creatures[i].ID))
				{
					State.angryAt.Add(room.abstractRoom.creatures[i].ID);
				}
			}
		}
		base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
	}

	public void Carry(bool eu)
	{
		Vector2 normalized = (tentacle.tChunks[tentacle.tChunks.Length - 1].pos - tentacle.tChunks[tentacle.tChunks.Length - 2].pos).normalized;
		if (!(base.grasps[0].grabbed is Weapon) && !Custom.DistLess(base.mainBodyChunk.pos, base.grasps[0].grabbedChunk.pos, 100f))
		{
			ReleaseGrasp(0);
			return;
		}
		if (base.grasps[0].grabbed is Weapon)
		{
			if (base.grasps[0].grabbed.grabbedBy.Count > 1)
			{
				ReleaseGrasp(0);
				return;
			}
			(base.grasps[0].grabbed as Weapon).setRotation = Custom.PerpendicularVector(normalized);
			base.grasps[0].grabbedChunk.MoveFromOutsideMyUpdate(eu, base.mainBodyChunk.pos + normalized * 6f);
			base.grasps[0].grabbedChunk.vel *= 0f;
		}
		else
		{
			extended = 1f;
			for (int i = 0; i < base.grasps[0].grabbed.bodyChunks.Length; i++)
			{
				if (room.GetTile(base.grasps[0].grabbed.bodyChunks[i].pos).WaterSurface)
				{
					base.grasps[0].grabbed.bodyChunks[i].vel.y -= 4f;
				}
			}
			SharedPhysics.ConnectChunks(base.mainBodyChunk, base.grasps[0].grabbedChunk, base.grasps[0].grabbedChunk.rad * 0.2f, 1f, 0.45f, 1f);
		}
		if (extended == 0f && UnityEngine.Random.value < 0.025f)
		{
			if (base.grasps[0].grabbed is Weapon)
			{
				base.grasps[0].grabbed.Destroy();
			}
			ReleaseGrasp(0);
		}
	}

	public void Retract()
	{
		retractSpeed = -1f / 30f;
	}

	public void Extend()
	{
		NewHole(burrowed: true);
		grabSpears = true;
		retractSpeed = 0.005f;
	}

	public void NewHole(bool burrowed)
	{
		if (room.garbageHoles == null)
		{
			AI.comeBackOutCounter = 0;
			retractSpeed = -1f / 30f;
			return;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < room.garbageHoles.Length; i++)
		{
			list.Add(i);
		}
		for (int j = 0; j < room.abstractRoom.creatures.Count; j++)
		{
			if (room.abstractRoom.creatures[j] != base.abstractCreature && room.abstractRoom.creatures[j].realizedCreature != null && room.abstractRoom.creatures[j].realizedCreature is GarbageWorm)
			{
				list.Remove((room.abstractRoom.creatures[j].realizedCreature as GarbageWorm).hole);
			}
		}
		if (list.Count == 0)
		{
			AI.comeBackOutCounter = 0;
			retractSpeed = -1f / 30f;
			return;
		}
		hole = list[UnityEngine.Random.Range(0, list.Count)];
		base.abstractCreature.pos.Tile = room.garbageHoles[hole] + new IntVector2(0, 1);
		rootPos = room.MiddleOfTile(base.abstractCreature.pos.Tile) + new Vector2(0f, -10f + base.bodyChunks[1].rad);
		tentacle.Reset(rootPos);
		if (burrowed)
		{
			base.bodyChunks[0].HardSetPosition(rootPos);
		}
		else
		{
			IntVector2 tile = base.abstractCreature.pos.Tile;
			tentacle.segments = new List<IntVector2> { base.abstractCreature.pos.Tile };
			for (int k = base.abstractCreature.pos.Tile.y + 1; (float)k < (float)base.abstractCreature.pos.Tile.y + tentacle.idealLength / 20f; k++)
			{
				if (room.GetTile(tile).Solid)
				{
					break;
				}
				tentacle.segments.Add(tile);
				tile.y = k;
			}
			for (int l = 0; l < tentacle.tChunks.Length; l++)
			{
				tentacle.tChunks[l].pos = room.MiddleOfTile(tentacle.segments[tentacle.tChunks[l].currentSegment]);
				tentacle.tChunks[l].lastPos = tentacle.tChunks[l].pos;
			}
			base.bodyChunks[0].HardSetPosition(room.MiddleOfTile(tile));
			tentacle.retractFac = 0f;
			extended = 1f;
		}
		base.bodyChunks[1].HardSetPosition(rootPos);
		AI.MapFloor(room);
		if (ModManager.MSC)
		{
			float num = 90f;
			controlRegion = new InputCircularRegion(tentacle.idealLength, num - 120f, num + 120f, 5f, Custom.DegToVec(num) * tentacle.idealLength);
		}
	}
}
