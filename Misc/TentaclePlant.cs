using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class TentaclePlant : Creature, PhysicalObject.IHaveAppendages
{
	private IntVector2 shortCutPos;

	public Vector2 rootPos;

	public Tentacle tentacle;

	public Vector2 stickOutDir;

	public Vector2 idlePos;

	public float attack;

	public float canGrab;

	public Vector2 attackDir;

	public float rootRad = 8f;

	public float tipRad = 1f;

	public float extended;

	public float forceIntoShortCut;

	public ChunkDynamicSoundLoop soundLoop;

	private List<IntVector2> scratchPath;

	public InputCircularRegion controlRegion;

	private TentaclePlantAI AI => base.abstractCreature.abstractAI.RealAI as TentaclePlantAI;

	public override Vector2 VisionPoint => tentacle.tChunks[UnityEngine.Random.Range(0, tentacle.tChunks.Length)].pos;

	public float Rad(float f)
	{
		f = Mathf.Max(1f - f, Mathf.Sin((float)Math.PI * Mathf.InverseLerp(0.7f, 1f, f)));
		return Mathf.Lerp(tipRad, rootRad, f);
	}

	public TentaclePlant(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), tipRad, 0.2f);
		base.bodyChunks[1] = new BodyChunk(this, 0, new Vector2(0f, 0f), rootRad, 0.2f);
		base.bodyChunks[1].collideWithTerrain = false;
		bodyChunkConnections = new BodyChunkConnection[0];
		tentacle = new Tentacle(this, base.bodyChunks[1], 300f);
		tentacle.tProps = new Tentacle.TentacleProps(stiff: false, rope: true, shorten: true, 0.5f, 0f, 0.5f, 0.05f, 0.05f, 2.2f, 12f, 1f / 3f, 5f, 15, 60, 12, 20);
		tentacle.tChunks = new Tentacle.TentacleChunk[8];
		for (int i = 0; i < tentacle.tChunks.Length; i++)
		{
			tentacle.tChunks[i] = new Tentacle.TentacleChunk(tentacle, i, (float)(i + 1) / (float)tentacle.tChunks.Length, Mathf.Lerp(rootRad, tipRad, (float)i / (float)(tentacle.tChunks.Length - 1)));
		}
		tentacle.stretchAndSqueeze = 0.1f;
		appendages = new List<Appendage>();
		appendages.Add(new Appendage(this, 0, tentacle.tChunks.Length + 1));
		soundLoop = new ChunkDynamicSoundLoop(base.mainBodyChunk);
		base.GoThroughFloors = true;
		base.airFriction = 0.99f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.47f;
		collisionLayer = 1;
		base.waterFriction = 0.92f;
		base.buoyancy = 0.95f;
		extended = 1f;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new TentaclePlantGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public override void NewRoom(Room room)
	{
		tentacle.NewRoom(room);
		base.NewRoom(room);
		shortCutPos = room.LocalCoordinateOfNode(base.abstractCreature.pos.abstractNode).Tile;
		stickOutDir = room.ShorcutEntranceHoleDirection(shortCutPos).ToVector2();
		rootPos = room.MiddleOfTile(room.LocalCoordinateOfNode(base.abstractCreature.pos.abstractNode)) + stickOutDir * 30f;
		idlePos = rootPos + stickOutDir * Mathf.Lerp(200f, 300f, UnityEngine.Random.value);
		if (ModManager.MSC)
		{
			float num = Mathf.Atan2(stickOutDir.y, stickOutDir.x) * (180f / (float)Math.PI);
			controlRegion = new InputCircularRegion(300f, num - 90f, num + 90f, 5f, stickOutDir * 300f);
		}
	}

	public override void Update(bool eu)
	{
		if (attack > 1f)
		{
			soundLoop.sound = SoundID.Tentacle_Plant_Move_LOOP;
			soundLoop.Volume = Mathf.Lerp(soundLoop.Volume, 1f, 0.8f);
			soundLoop.Pitch = Mathf.Lerp(soundLoop.Volume, 1f, 0.8f);
		}
		else if (attack > 0.5f)
		{
			soundLoop.sound = SoundID.Tentacle_Plant_Shake_LOOP;
			soundLoop.Volume = Mathf.Lerp(soundLoop.Volume, Mathf.InverseLerp(0.2f, 1f, attack), 0.5f);
			soundLoop.Pitch = Mathf.Lerp(soundLoop.Volume, 0.5f + Mathf.InverseLerp(0.5f, 1f, attack), 0.5f);
		}
		else
		{
			soundLoop.sound = SoundID.Tentacle_Plant_Move_LOOP;
			soundLoop.Volume = Mathf.Lerp(soundLoop.Volume, Mathf.InverseLerp(0.2f, 7f, base.mainBodyChunk.vel.magnitude), 0.2f);
			soundLoop.Pitch = Custom.LerpMap(base.mainBodyChunk.vel.magnitude, 0.1f, 25f, 0.5f, 1.8f);
		}
		soundLoop.Update();
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		base.abstractCreature.pos.Tile = room.GetTilePosition(rootPos);
		if (room == null || enteringShortCut.HasValue)
		{
			return;
		}
		tentacle.Update();
		tentacle.limp = !base.Consious;
		tentacle.retractFac = 1f - extended;
		base.mainBodyChunk.rad = ((attack > 1f) ? 9f : tipRad);
		base.mainBodyChunk.collideWithTerrain = extended == 1f && tentacle.backtrackFrom == -1;
		if (!Custom.DistLess(base.bodyChunks[1].pos, base.bodyChunks[0].pos, tentacle.idealLength * 2f * extended))
		{
			base.bodyChunks[0].pos = base.bodyChunks[1].pos + Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * tentacle.idealLength * 2f * extended;
		}
		if (room.game.devToolsActive && Input.GetKey("b"))
		{
			base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 3f;
		}
		if (!tentacle.grabDest.HasValue)
		{
			tentacle.MoveGrabDest(idlePos, ref scratchPath);
		}
		if (AI.preyTracker.MostAttractivePrey == null && room.readyForAI)
		{
			if (room.aimap.getTerrainProximity(idlePos) < 8)
			{
				IntVector2 intVector = Custom.eightDirections[UnityEngine.Random.Range(0, 8)];
				if (UnityEngine.Random.value < 0.025f)
				{
					intVector *= 2;
				}
				if (!room.GetTile(idlePos + intVector.ToVector2() * 20f).Solid && (room.aimap.getTerrainProximity(idlePos + intVector.ToVector2() * 20f) > room.aimap.getTerrainProximity(idlePos) || room.aimap.getTerrainProximity(idlePos) < 1))
				{
					idlePos += intVector.ToVector2() * 5f + Custom.RNV() * 3f;
				}
			}
			if (!Custom.DistLess(idlePos, rootPos + stickOutDir * 200f, 150f))
			{
				Vector2 vector = Custom.RNV();
				if (UnityEngine.Random.value < 0.025f)
				{
					vector *= 2f;
				}
				if (!room.GetTile(idlePos + vector * 20f).Solid && Vector2.Distance(idlePos + vector * 20f, rootPos + stickOutDir * 200f) < Vector2.Distance(idlePos, rootPos + stickOutDir * 200f))
				{
					idlePos += vector * 5f + Custom.RNV() * 3f;
				}
			}
			if (UnityEngine.Random.value < 1f / 170f && (!Custom.DistLess(base.mainBodyChunk.pos, idlePos, 50f) || room.aimap.getTerrainProximity(idlePos) < 1 || room.aimap.getAItile(idlePos).narrowSpace))
			{
				idlePos = base.mainBodyChunk.pos;
			}
			tentacle.MoveGrabDest(idlePos, ref scratchPath);
		}
		else
		{
			idlePos = tentacle.floatGrabDest.Value;
		}
		float num = attack;
		if (base.Consious)
		{
			AI.Update();
			if (base.safariControlled)
			{
				controlRegion.drawDebugPos = base.bodyChunks[1].pos;
				controlRegion.Update(inputWithDiagonals);
				Vector2 p = base.bodyChunks[0].pos - base.bodyChunks[1].pos;
				float num2 = Custom.Dist(p, controlRegion.pos);
				if (num2 > 60f)
				{
					controlRegion.pos.x = Mathf.Lerp(controlRegion.pos.x, p.x, Mathf.Lerp(0f, 0.2f, (num2 - 60f) / 200f));
					controlRegion.pos.y = Mathf.Lerp(controlRegion.pos.y, p.y, Mathf.Lerp(0f, 0.2f, (num2 - 60f) / 200f));
				}
				if (inputWithDiagonals.HasValue && lastInputWithDiagonals.HasValue && (inputWithDiagonals.Value.pckp & !lastInputWithDiagonals.Value.pckp) && attack < 1f)
				{
					attack = 1.1f;
					bool flag = false;
					if (AI.preyTracker.MostAttractivePrey != null && AI.preyTracker.MostAttractivePrey.VisualContact)
					{
						Vector2 pos = AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos;
						if (Custom.DistLess(pos, rootPos, tentacle.idealLength))
						{
							flag = true;
							attackDir = Custom.DirVec(base.mainBodyChunk.pos, pos + AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.vel * Vector2.Distance(base.mainBodyChunk.pos, pos) * 0.03f);
						}
					}
					if (!flag)
					{
						attackDir = Custom.DirVec(base.mainBodyChunk.pos, base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f);
					}
				}
				else if (inputWithDiagonals.HasValue && (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0) && (base.grasps[0] == null || base.grasps[0].grabbed == null))
				{
					tentacle.MoveGrabDest(base.bodyChunks[1].pos + controlRegion.pos, ref scratchPath);
				}
			}
			if (base.grasps[0] == null && (!base.safariControlled || attack > 0f))
			{
				if (attack >= 1f)
				{
					attack += 0.1f;
					tentacle.Tip.vel += attackDir * 20f;
					base.mainBodyChunk.vel += attackDir * 20f;
					if (attack > 2f)
					{
						attack = 0f;
					}
				}
				else
				{
					bool flag2 = false;
					if (AI.mostInterestingItem != null && AI.itemInterest > AI.preyInterest)
					{
						tentacle.MoveGrabDest(room.MiddleOfTile(AI.mostInterestingItem.firstChunk.pos), ref scratchPath);
					}
					else if (AI.preyTracker.MostAttractivePrey != null)
					{
						if (AI.preyTracker.MostAttractivePrey.VisualContact)
						{
							Vector2 pos2 = AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos;
							if (Custom.DistLess(pos2, rootPos, tentacle.idealLength))
							{
								flag2 = true;
								attackDir = Custom.DirVec(base.mainBodyChunk.pos, pos2 + AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.vel * Vector2.Distance(base.mainBodyChunk.pos, pos2) * 0.03f);
							}
							tentacle.MoveGrabDest(pos2, ref scratchPath);
						}
						else
						{
							tentacle.MoveGrabDest(room.MiddleOfTile(AI.preyTracker.MostAttractivePrey.BestGuessForPosition()), ref scratchPath);
						}
					}
					if (flag2)
					{
						attack += 1f / 90f;
					}
					else
					{
						attack = Mathf.Max(0f, attack - 1f / 180f);
					}
				}
			}
			else if (!base.safariControlled)
			{
				attack = 0f;
			}
			if (tentacle.floatGrabDest.HasValue)
			{
				base.bodyChunks[0].vel += Vector2.ClampMagnitude(tentacle.floatGrabDest.Value - base.bodyChunks[0].pos, 20f) / 20f;
			}
		}
		else
		{
			attack = 0f;
		}
		if (attack > 1f)
		{
			if (num <= 1f)
			{
				room.PlaySound(SoundID.Tentacle_Plant_Init_Thrust, base.mainBodyChunk);
			}
			canGrab = 1f;
			for (int i = 0; i < room.physicalObjects[0].Count; i++)
			{
				for (int j = 0; j < room.physicalObjects[0][i].bodyChunks.Length; j++)
				{
					if (Custom.DistLess(base.mainBodyChunk.pos, room.physicalObjects[0][i].bodyChunks[j].pos, base.mainBodyChunk.rad + room.physicalObjects[0][i].bodyChunks[j].rad))
					{
						Collide(room.physicalObjects[0][i], 0, j);
					}
				}
			}
		}
		else
		{
			canGrab = Mathf.Max(0f, canGrab - 0.025f);
		}
		for (int k = 0; k < tentacle.tChunks.Length; k++)
		{
			float t = (float)k / (float)(tentacle.tChunks.Length - 1);
			tentacle.tChunks[k].vel *= 0.96f;
			if (extended > 0.5f && base.grasps[0] != null && tentacle.tChunks[k].phase > -1f)
			{
				ReleaseGrasp(0);
			}
			if (tentacle.backtrackFrom != -1 && tentacle.backtrackFrom <= k - 2)
			{
				continue;
			}
			if (attack > 0.5f && attack < 1f)
			{
				tentacle.tChunks[k].vel += Custom.DirVec(tentacle.floatGrabDest.Value, Vector2.Lerp(base.mainBodyChunk.pos, rootPos + stickOutDir * 200f, 0.5f)) * attack * 0.8f;
			}
			Vector2 p2 = rootPos - stickOutDir * 30f;
			if (k == 1)
			{
				p2 = rootPos;
			}
			else if (k > 1)
			{
				p2 = tentacle.tChunks[k - 2].pos;
				tentacle.tChunks[k - 2].vel += Custom.DirVec(tentacle.tChunks[k].pos, tentacle.tChunks[k - 2].pos);
			}
			tentacle.tChunks[k].vel += Custom.DirVec(p2, tentacle.tChunks[k].pos);
			if (ModManager.MSC && base.LickedByPlayer != null)
			{
				Stun(30);
				base.LickedByPlayer.tongue.decreaseRopeLength(3f);
				if ((base.LickedByPlayer.tongue.requestedRopeLength < 4f || (base.LickedByPlayer.firstChunk.vel.magnitude > 15f && base.LickedByPlayer.firstChunk.vel.y > 5f)) && !base.LickedByPlayer.Stunned)
				{
					base.LickedByPlayer.tongue.Release();
					base.LickedByPlayer.firstChunk.vel *= 3f;
				}
			}
			if (base.Consious)
			{
				tentacle.tChunks[k].vel += stickOutDir * Mathf.Lerp(0.3f, 0f, t);
			}
		}
		tentacle.retractFac = 1f - extended;
		if (extended == 0f)
		{
			for (int l = 0; l < 2; l++)
			{
				base.bodyChunks[l].collideWithTerrain = false;
				base.bodyChunks[l].HardSetPosition(rootPos + new Vector2(0f, -50f));
				base.bodyChunks[l].vel *= 0f;
			}
		}
		else
		{
			base.bodyChunks[1].pos = rootPos;
			base.bodyChunks[1].vel *= 0f;
			for (int m = 0; m < 2; m++)
			{
				base.bodyChunks[m].collideWithTerrain = tentacle.backtrackFrom == -1;
			}
			float num3 = 0f;
			if (tentacle.backtrackFrom == -1)
			{
				num3 = 0.5f;
				if (!base.Consious)
				{
					num3 = 0.7f;
				}
			}
			Vector2 vector2 = Custom.DirVec(base.bodyChunks[0].pos, tentacle.Tip.pos);
			float num4 = Vector2.Distance(base.bodyChunks[0].pos, tentacle.Tip.pos);
			base.bodyChunks[0].pos -= (0f - num4) * vector2 * (1f - num3);
			base.bodyChunks[0].vel -= (0f - num4) * vector2 * (1f - num3);
			tentacle.Tip.pos += (0f - num4) * vector2 * num3;
			tentacle.Tip.vel += (0f - num4) * vector2 * num3;
			if (base.Consious)
			{
				base.bodyChunks[0].vel.y += base.gravity;
			}
		}
		if (!base.State.alive && UnityEngine.Random.value < 0.025f)
		{
			Die();
		}
		if (base.dead || Mathf.InverseLerp(0.3f, 0.65f, (base.State as HealthState).health) < 1f - base.abstractCreature.personality.aggression)
		{
			if (ModManager.MMF && base.grasps[0] != null)
			{
				bool flag3 = true;
				if (!base.dead)
				{
					flag3 = ((!(base.grasps[0].grabbed is Player)) ? (UnityEngine.Random.value > 0.98f) : ((base.grasps[0].grabbed as Player).GraspWiggle > UnityEngine.Random.Range(0.1f, 0.2f)));
				}
				if (flag3)
				{
					Custom.Log("Wiggle escaped");
					Stun(70);
					ReleaseGrasp(0);
				}
			}
			extended = Mathf.Max(0f, extended - 1f / 60f);
			if (!ModManager.MMF)
			{
				ReleaseGrasp(0);
			}
			if (extended <= 0f)
			{
				if (base.abstractCreature.pos.abstractNode != -1 && base.abstractCreature.GetNodeType != AbstractRoomNode.Type.Den)
				{
					Destroy();
				}
				else
				{
					enteringShortCut = shortCutPos;
				}
			}
		}
		else if (AI.threatTracker.mostThreateningCreature != null && AI.threatTracker.mostThreateningCreature.BestGuessForPosition().Tile.FloatDist(base.abstractCreature.pos.Tile) < 20f)
		{
			extended = Mathf.Max(0f, extended - 1f / 60f);
			if (extended <= 0f)
			{
				AI.tracker.ForgetCreature(AI.threatTracker.mostThreateningCreature.representedCreature);
				base.abstractCreature.remainInDenCounter = 400;
				enteringShortCut = shortCutPos;
			}
		}
		if (base.grasps[0] != null)
		{
			Carry(eu);
			extended -= 0.0125f;
			base.grasps[0].grabbedChunk.vel += Vector2.ClampMagnitude(rootPos - base.grasps[0].grabbedChunk.pos, Mathf.Lerp(50f, 500f, forceIntoShortCut)) * (1f - extended) / 20f / Mathf.Lerp(base.grasps[0].grabbedChunk.mass, 0.1f, forceIntoShortCut);
			if (base.grasps[0].grabbed is TentaclePlant)
			{
				ReleaseGrasp(0);
				return;
			}
			if (extended < 0f)
			{
				extended = 0f;
				forceIntoShortCut += 1f / 30f;
				if (Custom.DistLess(base.grasps[0].grabbedChunk.pos, rootPos, 20f) || forceIntoShortCut > 1f)
				{
					base.abstractCreature.remainInDenCounter = ((base.grasps[0].grabbed is Creature) ? 1200 : 160);
					enteringShortCut = shortCutPos;
					forceIntoShortCut = 0f;
				}
			}
			else
			{
				forceIntoShortCut = 0f;
			}
			if (base.grasps[0].grabbed is Player && UnityEngine.Random.value < Mathf.Lerp(0f, 1f / 30f, (base.grasps[0].grabbed as Player).GraspWiggle))
			{
				base.mainBodyChunk.vel += Custom.DirVec(base.grasps[0].grabbedChunk.pos, base.mainBodyChunk.pos) * Mathf.Lerp(2f, 4f, UnityEngine.Random.value);
				ReleaseGrasp(0);
				Stun(UnityEngine.Random.Range(4, 16));
			}
			else if (tentacle.TotalRope > 600f || (base.grasps[0].grabbed is Creature && (base.grasps[0].grabbed as Creature).enteringShortCut.HasValue))
			{
				ReleaseGrasp(0);
			}
		}
		else if (!base.dead && AI.threatTracker.mostThreateningCreature == null)
		{
			extended = 1f;
			forceIntoShortCut = 0f;
		}
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		if (source != null && source.owner is Spear)
		{
			stunBonus += 50f;
			Custom.Log("tentacle plant spear extra spear stun");
		}
		base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
		if (ModManager.MMF && base.grasps[0] != null)
		{
			ReleaseGrasp(0);
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!(otherObject is TentaclePlant) && !(otherObject is PoleMimic) && (!ModManager.MSC || !(otherObject is BigJellyFish)) && !(otherObject is GarbageWorm) && (!base.safariControlled || canGrab != 0f) && myChunk <= 0 && base.grasps[0] == null && base.Consious)
		{
			BodyChunk bodyChunk = otherObject.bodyChunks[otherChunk];
			bodyChunk.vel = Vector2.Lerp(bodyChunk.vel, base.mainBodyChunk.vel, 0.2f / bodyChunk.mass);
			if (canGrab > 0f || Custom.DistLess(bodyChunk.vel, base.mainBodyChunk.vel, (otherObject is Player && base.abstractCreature.world.game.StoryCharacter == SlugcatStats.Name.Yellow) ? (0.5f * UnityEngine.Random.value) : 1f))
			{
				Grab(otherObject, 0, otherChunk, Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, overrideEquallyDominant: true, pacifying: false);
				room.PlaySound((bodyChunk.owner is Player) ? SoundID.Tentacle_Plant_Grab_Slugcat : SoundID.Tentacle_Plant_Grab_Other, base.mainBodyChunk);
			}
		}
	}

	public void Carry(bool eu)
	{
		Vector2 vector = Custom.DirVec(base.grasps[0].grabbedChunk.pos, tentacle.Tip.pos);
		float num = Vector2.Distance(base.grasps[0].grabbedChunk.pos, tentacle.Tip.pos);
		float num2 = base.grasps[0].grabbedChunk.mass / (base.grasps[0].grabbedChunk.mass + 0.1f);
		base.grasps[0].grabbedChunk.pos -= (0f - num) * vector * (1f - num2);
		base.grasps[0].grabbedChunk.vel -= (0f - num) * vector * (1f - num2);
		tentacle.Tip.pos += (0f - num) * vector * num2;
		tentacle.Tip.vel += (0f - num) * vector * num2;
	}

	public Vector2 AppendagePosition(int appendage, int segment)
	{
		if (segment == 0)
		{
			return rootPos;
		}
		return tentacle.tChunks[segment - 1].pos;
	}

	public void ApplyForceOnAppendage(Appendage.Pos pos, Vector2 momentum)
	{
		if (pos.prevSegment > 0)
		{
			tentacle.tChunks[pos.prevSegment - 1].pos += momentum / 0.2f * (1f - pos.distanceToNext);
			tentacle.tChunks[pos.prevSegment - 1].vel += momentum / 0.1f * (1f - pos.distanceToNext);
		}
		tentacle.tChunks[pos.prevSegment].pos += momentum / 0.2f * pos.distanceToNext;
		tentacle.tChunks[pos.prevSegment].vel += momentum / 0.1f * pos.distanceToNext;
	}
}
