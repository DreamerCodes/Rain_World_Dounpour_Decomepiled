using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class StowawayBug : InsectoidCreature, PhysicalObject.IHaveAppendages
{
	private class EatObject
	{
		public BodyChunk chunk;

		public float distance;

		public float progression;

		public EatObject(BodyChunk chunk, float distance)
		{
			this.chunk = chunk;
			this.distance = distance;
			progression = 0f;
		}
	}

	public StowawayBugAI AI;

	public Vector2 placedDirection;

	private Vector2 goalDirection;

	private Vector2 currentDirection;

	private Vector2 lastDirection;

	public bool mawOpen;

	public float sleepScale;

	public Vector2[][,] tentacles;

	private float tentaclesWithdrawn;

	public bool anyTentaclePulled;

	public Tentacle[] heads;

	public float[] headCooldown;

	public bool[] headFired;

	private int spitCooldown;

	private float headLength;

	public Vector2 colorPickPos;

	public int teethCount;

	public int teethSeed;

	private List<EatObject> eatObjects;

	public Vector2 originalPos;

	public int huntDelay;

	public override float VisibilityBonus
	{
		get
		{
			if (AI.behavior == StowawayBugAI.Behavior.Attacking)
			{
				return 1f;
			}
			if (AI.behavior == StowawayBugAI.Behavior.Sleeping || AI.behavior == StowawayBugAI.Behavior.Hidden)
			{
				return -1f;
			}
			if (mawOpen && base.graphicsModule != null)
			{
				return -1f + (base.graphicsModule as StowawayBugGraphics).mouthOpen;
			}
			return -1f;
		}
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new StowawayBugGraphics(this);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		Eat(eu);
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			Stun(12);
		}
		spitCooldown--;
		huntDelay--;
		float num = 30f;
		if (base.Consious)
		{
			Act();
			for (int i = 0; i < heads.Length; i++)
			{
				headCooldown[i] -= 0.1f;
				if (headFired[i] && heads[i].retractFac < 1f && spitCooldown <= 0)
				{
					if (base.grasps[i] == null)
					{
						heads[i].retractFac += 0.0055f;
					}
					else
					{
						heads[i].retractFac += 0.0025f;
					}
				}
				if (headFired[i] && heads[i].retractFac == 1f)
				{
					headFired[i] = false;
					headCooldown[i] = Random.Range(12, 20);
				}
			}
		}
		else
		{
			if (!base.dead && AI.behavior == StowawayBugAI.Behavior.Sleeping)
			{
				AI.behavior = StowawayBugAI.Behavior.Idle;
			}
			if (base.dead)
			{
				AI.behavior = StowawayBugAI.Behavior.Idle;
			}
			mawOpen = false;
			for (int j = 0; j < heads.Length; j++)
			{
				headFired[j] = true;
				heads[j].limp = true;
				heads[j].retractFac = 0f;
			}
		}
		bool flag = false;
		bool flag2 = false;
		for (int k = 0; k < heads.Length; k++)
		{
			if (heads[k].retractFac != 1f)
			{
				heads[k].limp = true;
				if (headCooldown[k] > 0f && heads[k].retractFac < 0.3f && base.Consious)
				{
					Vector2 pos = heads[k].Tip.pos;
					int num2 = 0;
					while (base.grasps[k] == null && num2 < room.abstractRoom.creatures.Count)
					{
						if (room.abstractRoom.creatures[num2].realizedCreature != null && room.abstractRoom.creatures[num2].realizedCreature != this && room.abstractRoom.creatures[num2].realizedCreature.room == room)
						{
							int num3 = 0;
							while (base.grasps[k] == null && num3 < room.abstractRoom.creatures[num2].realizedCreature.bodyChunks.Length)
							{
								if (Custom.DistLess(room.abstractRoom.creatures[num2].realizedCreature.bodyChunks[num3].pos, pos, room.abstractRoom.creatures[num2].realizedCreature.bodyChunks[num3].rad + 16f))
								{
									Custom.Log($"stowaway grab creature {room.abstractRoom.creatures[num2]}");
									room.PlaySound(SoundID.Dart_Maggot_Stick_In_Creature, base.firstChunk);
									headCooldown[k] = 0f;
									room.AddObject(new CreatureSpasmer(room.abstractRoom.creatures[num2].realizedCreature, allowDead: false, 100));
									room.abstractRoom.creatures[num2].realizedCreature.Violence(base.firstChunk, null, room.abstractRoom.creatures[num2].realizedCreature.bodyChunks[num3], null, DamageType.Bite, (room.abstractRoom.creatures[num2].realizedCreature is Player) ? 0f : 0.2f, 90f);
									if (AI.WantToEat(room.abstractRoom.creatures[num2].creatureTemplate.type))
									{
										Grab(room.abstractRoom.creatures[num2].realizedCreature, k, num3, Grasp.Shareability.NonExclusive, 0f, overrideEquallyDominant: false, pacifying: false);
									}
								}
								num3++;
							}
						}
						num2++;
					}
				}
			}
			if (base.grasps[k] != null)
			{
				if (base.dead)
				{
					Custom.Log("dropped critter from death");
					room.PlaySound(SoundID.Spear_Dislodged_From_Creature, base.firstChunk);
					heads[k].retractFac = 0f;
					ReleaseGrasp(k);
				}
				else if ((base.grasps[k].grabbed as Creature).enteringShortCut.HasValue)
				{
					enteringShortCut = null;
					Custom.Log("dropped critter from door entry");
					room.PlaySound(SoundID.Spear_Dislodged_From_Creature, base.firstChunk);
					heads[k].retractFac = 0f;
					ReleaseGrasp(k);
				}
				else if (Vector2.Distance(heads[k].Tip.pos, base.bodyChunks[0].pos) > headLength * 6f)
				{
					Custom.Log("dropped critter from extreme distance");
					room.PlaySound(SoundID.Spear_Dislodged_From_Creature, base.firstChunk);
					heads[k].retractFac = 0f;
					ReleaseGrasp(k);
				}
				else if (Vector2.Distance(heads[k].Tip.pos, base.bodyChunks[0].pos) > 96f && heads[k].retractFac >= 0.95f)
				{
					Custom.Log("dropped critter from impossible retraction");
					room.PlaySound(SoundID.Spear_Dislodged_From_Creature, base.firstChunk);
					heads[k].retractFac = 0f;
					ReleaseGrasp(k);
				}
				else
				{
					AI.tracker.SeeCreature((base.grasps[k].grabbed as Creature).abstractCreature);
					anyTentaclePulled = true;
					Carry(eu, k);
					flag = true;
					if (Vector2.Distance(heads[k].Tip.pos, base.bodyChunks[1].pos) < num)
					{
						flag2 = true;
					}
					if (base.grasps[k].grabbed is Player && (base.grasps[k].grabbed as Player).GraspWiggle > 0.25f + Random.value)
					{
						Custom.Log("dropped player from wiggle");
						room.PlaySound(SoundID.Spear_Dislodged_From_Creature, base.firstChunk);
						ReleaseGrasp(k);
					}
				}
			}
			for (int l = 0; l < heads[k].tChunks.Length; l++)
			{
				Tentacle.TentacleChunk tentacleChunk = heads[k].tChunks[l];
				tentacleChunk.vel.y = tentacleChunk.vel.y - base.gravity * (1f - heads[k].retractFac);
				tentacleChunk.vel *= Mathf.Lerp(1f, 0.95f, heads[k].retractFac);
			}
			if (heads[k].retractFac >= 0.98f)
			{
				for (int num4 = heads[k].tChunks.Length - 1; num4 > 1; num4--)
				{
					heads[k].tChunks[num4 - 1].pos = heads[k].tChunks[num4].pos + Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * 3f;
				}
			}
			heads[k].idealLength = headLength * (1f - heads[k].retractFac);
			heads[k].Update();
		}
		if (base.graphicsModule != null && (Random.value < 0.02f || flag2) && flag && ((!flag2) ? (base.graphicsModule as StowawayBugGraphics).Bite() : (base.graphicsModule as StowawayBugGraphics).KillerBite()))
		{
			room.PlaySound(SoundID.Lizard_Jaws_Shut_Miss_Creature, base.firstChunk);
			for (int num5 = Random.Range(1, 5); num5 > 0; num5--)
			{
				room.AddObject(new WaterDrip(base.bodyChunks[1].pos, Custom.DirVec(base.firstChunk.pos, base.bodyChunks[1].pos) * 10f + Custom.RNV(), waterColor: true));
			}
			if (flag2)
			{
				for (int m = 0; m < heads.Length; m++)
				{
					if (base.grasps[m] == null || !(Vector2.Distance(heads[m].Tip.pos, base.bodyChunks[1].pos) < num))
					{
						continue;
					}
					(base.grasps[m].grabbed as Creature).Violence(base.firstChunk, null, base.grasps[m].grabbedChunk, null, DamageType.Bite, 4f, 200f);
					if ((base.grasps[m].grabbed as Creature).dead)
					{
						_ = base.grasps[m].grabbed;
						(base.graphicsModule as StowawayBugGraphics).digestPrey += 0.01f;
						for (int num6 = Random.Range(4, 8); num6 > 0; num6--)
						{
							room.AddObject(new WaterDrip(base.bodyChunks[1].pos, default(Vector2) + Custom.RNV(), waterColor: true));
						}
						eatObjects.Add(new EatObject(base.grasps[m].grabbedChunk, Vector2.Distance(base.firstChunk.pos, base.grasps[m].grabbedChunk.pos)));
						LoseAllGrasps();
						(base.abstractCreature.state as StowawayBugState).StartDigestion(room.world.rainCycle.timer);
						room.PlaySound(SoundID.Bro_Digestion_Init, base.firstChunk);
					}
					else
					{
						heads[m].retractFac = 0f;
					}
					room.PlaySound(SoundID.Lizard_Jaws_Grab_Player, base.firstChunk);
				}
			}
		}
		base.firstChunk.vel *= 0f;
		base.firstChunk.pos = Vector2.Lerp((base.State as StowawayBugState).HomePos, (base.State as StowawayBugState).HomePos - placedDirection * 25f, sleepScale);
		base.bodyChunks[1].collideWithObjects = !mawOpen;
		float num7 = Mathf.Lerp(10f, 1f, tentaclesWithdrawn);
		anyTentaclePulled = false;
		for (int n = 0; n < tentacles.Length; n++)
		{
			for (int num8 = 0; num8 < tentacles[n].GetLength(0); num8++)
			{
				float t = (float)num8 / (float)(tentacles[n].GetLength(0) - 1);
				tentacles[n][num8, 1] = tentacles[n][num8, 0];
				tentacles[n][num8, 0] += tentacles[n][num8, 2];
				if (room.PointSubmerged(tentacles[n][num8, 0]))
				{
					tentacles[n][num8, 2] *= Custom.LerpMap(tentacles[n][num8, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
					tentacles[n][num8, 2] += Custom.RNV() * 0.2f;
					continue;
				}
				tentacles[n][num8, 2] *= 0.999f;
				tentacles[n][num8, 2].y -= room.gravity * 0.6f;
				SharedPhysics.TerrainCollisionData cd2 = SharedPhysics.HorizontalCollision(cd: new SharedPhysics.TerrainCollisionData(tentacles[n][num8, 0], tentacles[n][num8, 1], tentacles[n][num8, 2], 1f, new IntVector2(0, 0), goThroughFloors: false), room: room);
				cd2 = SharedPhysics.VerticalCollision(room, cd2);
				cd2 = SharedPhysics.SlopesVertically(room, cd2);
				tentacles[n][num8, 0] = cd2.pos;
				tentacles[n][num8, 2] = cd2.vel;
			}
			for (int num9 = 0; num9 < tentacles[n].GetLength(0); num9++)
			{
				if (num9 > 0)
				{
					Vector2 normalized = (tentacles[n][num9, 0] - tentacles[n][num9 - 1, 0]).normalized;
					float num10 = Vector2.Distance(tentacles[n][num9, 0], tentacles[n][num9 - 1, 0]);
					tentacles[n][num9, 0] += normalized * (num7 - num10) * 0.5f;
					tentacles[n][num9, 2] += normalized * (num7 - num10) * 0.5f;
					tentacles[n][num9 - 1, 0] -= normalized * (num7 - num10) * 0.5f;
					tentacles[n][num9 - 1, 2] -= normalized * (num7 - num10) * 0.5f;
					if (num9 > 1)
					{
						normalized = (tentacles[n][num9, 0] - tentacles[n][num9 - 2, 0]).normalized;
						tentacles[n][num9, 2] += normalized * 0.2f;
						tentacles[n][num9 - 2, 2] -= normalized * 0.2f;
					}
				}
				else
				{
					tentacles[n][num9, 0] = AttachPos(n, 1f);
					tentacles[n][num9, 2] *= 0f;
				}
			}
			if (!(Random.value < 1f / (float)tentacles.Length))
			{
				continue;
			}
			Vector2 p = tentacles[n][tentacles[n].GetLength(0) - 1, 0];
			for (int num11 = 0; num11 < room.abstractRoom.creatures.Count; num11++)
			{
				if (room.abstractRoom.creatures[num11].realizedCreature == null || room.abstractRoom.creatures[num11].realizedCreature.room != room)
				{
					continue;
				}
				for (int num12 = 0; num12 < room.abstractRoom.creatures[num11].realizedCreature.bodyChunks.Length; num12++)
				{
					if (Custom.DistLess(room.abstractRoom.creatures[num11].realizedCreature.bodyChunks[num12].pos, p, room.abstractRoom.creatures[num11].realizedCreature.bodyChunks[num12].rad + 10f))
					{
						AI.tracker.SeeCreature(room.abstractRoom.creatures[num11]);
						anyTentaclePulled = true;
					}
				}
			}
		}
		float num13 = 0.008f;
		if (AI.activeThisCycle)
		{
			num13 = 0.095f;
		}
		if (mawOpen)
		{
			num13 = 0.2f;
		}
		if (Random.value < num13)
		{
			float num14 = (mawOpen ? 15f : 2f);
			Vector2 pos2 = base.bodyChunks[1].pos + Custom.RNV() * num14;
			room.AddObject(new WaterDrip(pos2, new Vector2(0f, 1f), waterColor: true));
		}
		base.bodyChunks[1].vel += new Vector2(0f, room.gravity * Mathf.InverseLerp(0.01f, 0.25f, placedDirection.y));
		base.bodyChunks[1].vel += Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos + placedDirection * 10f) * 0.33f;
		base.bodyChunks[1].vel *= 0.92f;
	}

	private void Act()
	{
		AI.Update();
		lastDirection = currentDirection;
		Vector2 p = base.bodyChunks[1].pos;
		if (base.graphicsModule != null && (base.graphicsModule as StowawayBugGraphics).digestPrey > 0f)
		{
			AI.behavior = StowawayBugAI.Behavior.Sleeping;
		}
		if (AI.behavior == StowawayBugAI.Behavior.Sleeping || AI.behavior == StowawayBugAI.Behavior.Hidden || AI.behavior == StowawayBugAI.Behavior.EscapeRain)
		{
			p = base.bodyChunks[0].pos + placedDirection * 20f;
			sleepScale += 0.01f;
			if (sleepScale > 1f)
			{
				sleepScale = 1f;
			}
			if (AI.behavior == StowawayBugAI.Behavior.Sleeping)
			{
				tentaclesWithdrawn = 1f;
			}
			else
			{
				tentaclesWithdrawn += 0.09f;
				if (tentaclesWithdrawn > 1f)
				{
					tentaclesWithdrawn = 1f;
				}
			}
			mawOpen = false;
		}
		else if (AI.behavior == StowawayBugAI.Behavior.Idle && placedDirection.y <= 0.3f)
		{
			mawOpen = false;
			p = base.bodyChunks[0].pos + goalDirection * 20f;
			goalDirection = Vector2.Lerp(goalDirection, placedDirection, 0.01f);
			if (base.grasps[0] != null || base.grasps[1] != null || base.grasps[2] != null)
			{
				mawOpen = true;
			}
			if (base.graphicsModule != null && (base.graphicsModule as StowawayBugGraphics).biting > 0f)
			{
				mawOpen = true;
			}
			for (int i = 0; i < heads.Length; i++)
			{
				if (heads[i].retractFac < 0.95f)
				{
					mawOpen = true;
				}
			}
		}
		else if (AI.behavior == StowawayBugAI.Behavior.Attacking || placedDirection.y > 0.3f)
		{
			mawOpen = true;
			Tracker.CreatureRepresentation mostAttractivePrey = AI.preyTracker.MostAttractivePrey;
			if (mostAttractivePrey != null && mostAttractivePrey.representedCreature.realizedCreature != null)
			{
				bool flag = true;
				for (int j = 0; j < heads.Length; j++)
				{
					if (headFired[j] || headCooldown[j] > 0f)
					{
						flag = false;
						break;
					}
				}
				if (mawOpen && (flag || Random.value < 0.4f))
				{
					for (int k = 0; k < heads.Length; k++)
					{
						if (!headFired[k] && spitCooldown < 0 && base.grasps[k] == null)
						{
							headFired[k] = true;
							heads[k].retractFac = 0f;
							headCooldown[k] = Random.Range(20, 40);
							spitCooldown = Random.Range(40, 60);
							heads[k].tChunks[heads[k].tChunks.Length - 1].vel = Custom.DirVec(heads[k].tChunks[heads[k].tChunks.Length - 1].pos, mostAttractivePrey.representedCreature.realizedCreature.DangerPos + mostAttractivePrey.representedCreature.realizedCreature.firstChunk.vel * Random.Range(10f, 45f)) * 45f;
							room.PlaySound(SoundID.Big_Spider_Spit, base.firstChunk);
							room.PlaySound(SoundID.Red_Lizard_Spit_Hit_NPC, base.firstChunk);
							break;
						}
					}
				}
			}
			if (AI.focusCreature != null)
			{
				p = base.bodyChunks[0].pos + Custom.DirVec(base.bodyChunks[0].pos, room.MiddleOfTile(AI.focusCreature.BestGuessForPosition().Tile)) * 20f;
				goalDirection = Custom.DirVec(base.bodyChunks[0].pos, p);
			}
			else
			{
				p = base.bodyChunks[0].pos + goalDirection * 20f;
			}
		}
		if (AI.behavior != StowawayBugAI.Behavior.Sleeping && AI.behavior != StowawayBugAI.Behavior.Hidden && AI.behavior != StowawayBugAI.Behavior.EscapeRain)
		{
			tentaclesWithdrawn -= 0.11f;
			if (tentaclesWithdrawn < 0f)
			{
				tentaclesWithdrawn = 0f;
			}
			sleepScale -= 0.07f;
			if (sleepScale < 0f)
			{
				sleepScale = 0f;
			}
		}
		if (anyTentaclePulled)
		{
			tentaclesWithdrawn += 0.009f;
			if (tentaclesWithdrawn > 1f)
			{
				tentaclesWithdrawn = 1f;
			}
		}
		base.bodyChunks[1].vel += Custom.DirVec(base.bodyChunks[1].pos, p);
		currentDirection = Custom.DirVec(base.bodyChunks[0].pos, base.bodyChunks[1].pos);
		if (Vector2.Distance(currentDirection, goalDirection) < 0.01f)
		{
			Vector2 vector = placedDirection * 10f + Custom.RNV() * 8f;
			goalDirection = Custom.DirVec(base.bodyChunks[0].pos, base.bodyChunks[0].pos + vector);
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
	}

	public override void Die()
	{
		base.Die();
	}

	public override Color ShortCutColor()
	{
		return Color.white;
	}

	public override void Stun(int st)
	{
		if (!AI.activeThisCycle)
		{
			Custom.Log("Stowaway woken by stun...");
			AI.activeThisCycle = true;
			AI.behavior = StowawayBugAI.Behavior.Hidden;
			st /= 3;
			if (st < 20)
			{
				return;
			}
		}
		else
		{
			mawOpen = true;
			if (st < 60)
			{
				st = 60;
			}
		}
		base.Stun(st);
		LoseAllGrasps();
	}

	public StowawayBug(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 7.5f, 0.2f);
		base.bodyChunks[0].collideWithTerrain = false;
		base.bodyChunks[0].collideWithObjects = false;
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 7f, 0.5f);
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 14f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		appendages = new List<Appendage>();
		eatObjects = new List<EatObject>();
		originalPos = (abstractCreature.state as StowawayBugState).HomePos;
		bodySetup();
	}

	public void SetStartDirection(Vector2 direction)
	{
		placedDirection = direction;
		goalDirection = direction;
		currentDirection = direction;
		lastDirection = direction;
	}

	public Vector2 AttachPos(int rag, float timeStacker)
	{
		Vector2 vector = Custom.DirVec(base.bodyChunks[0].lastPos, base.bodyChunks[1].lastPos);
		Vector3 vector2 = Vector3.Slerp(b: Custom.DirVec(base.bodyChunks[0].pos, base.bodyChunks[1].pos), a: vector, t: timeStacker);
		return Vector2.Lerp(base.bodyChunks[1].lastPos, base.bodyChunks[1].pos, timeStacker) + new Vector2(vector2.x, vector2.y) * 17f;
	}

	public void ResetTentacles()
	{
		for (int i = 0; i < tentacles.Length; i++)
		{
			for (int j = 0; j < tentacles[i].GetLength(0); j++)
			{
				tentacles[i][j, 0] = base.firstChunk.pos + Custom.RNV();
				tentacles[i][j, 1] = tentacles[i][j, 0];
				tentacles[i][j, 2] *= 0f;
			}
		}
	}

	public float Rad(float f)
	{
		if (f < 0.5f)
		{
			return Custom.LerpMap(f, 0f, 0.09f, 5.8f, 0.001f);
		}
		return Custom.LerpMap(f, 0.85f, 1f, 0.001f, 0.01f);
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		bodySetup();
		ResetTentacles();
		for (int i = 0; i < heads.Length; i++)
		{
			heads[i].NewRoom(newRoom);
		}
		base.bodyChunks[1].pos = (base.firstChunk.pos += placedDirection * 8f);
	}

	public Vector2 AppendagePosition(int appendage, int segment)
	{
		if (segment <= 0)
		{
			return base.mainBodyChunk.pos;
		}
		return heads[appendage].tChunks[segment].pos;
	}

	public void ApplyForceOnAppendage(Appendage.Pos pos, Vector2 momentum)
	{
		if (pos.prevSegment > 0)
		{
			heads[pos.appendage.appIndex].tChunks[pos.prevSegment - 1].pos += momentum / 0.04f * (1f - pos.distanceToNext);
			heads[pos.appendage.appIndex].tChunks[pos.prevSegment - 1].vel += momentum / 0.04f * (1f - pos.distanceToNext);
		}
		else
		{
			heads[pos.appendage.appIndex].connectedChunk.pos += momentum / heads[pos.appendage.appIndex].connectedChunk.mass * (1f - pos.distanceToNext);
			heads[pos.appendage.appIndex].connectedChunk.vel += momentum / heads[pos.appendage.appIndex].connectedChunk.mass * (1f - pos.distanceToNext);
		}
		heads[pos.appendage.appIndex].tChunks[pos.prevSegment].pos += momentum / 0.04f * pos.distanceToNext;
		heads[pos.appendage.appIndex].tChunks[pos.prevSegment].vel += momentum / 0.04f * pos.distanceToNext;
	}

	public void Carry(bool eu, int head)
	{
		Vector2 vector = Custom.DirVec(base.grasps[head].grabbedChunk.pos, heads[head].Tip.pos);
		float num = Vector2.Distance(base.grasps[head].grabbedChunk.pos, heads[head].Tip.pos);
		float num2 = base.grasps[head].grabbedChunk.mass / (base.grasps[head].grabbedChunk.mass + 0.1f);
		base.grasps[head].grabbedChunk.pos -= (0f - num) * vector * (1f - num2);
		base.grasps[head].grabbedChunk.vel -= (0f - num) * vector * (1f - num2);
		heads[head].Tip.pos += (0f - num) * vector * num2;
		heads[head].Tip.vel += (0f - num) * vector * num2;
		BodyChunk grabbedChunk = base.grasps[head].grabbedChunk;
		grabbedChunk.vel.x = grabbedChunk.vel.x * 0.95f;
		base.grasps[head].grabbedChunk.vel += Custom.DirVec(base.grasps[head].grabbedChunk.pos, base.firstChunk.pos) * (heads[head].retractFac * (base.grasps[head].grabbed.TotalMass * 4f));
		_ = base.grasps[head].grabbedChunk;
	}

	public override bool CanBeGrabbed(Creature grabber)
	{
		return false;
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		if (onAppendagePos != null)
		{
			damage *= 0.05f;
		}
		base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
	}

	public void Eat(bool eu)
	{
		Vector2 pos = base.firstChunk.pos;
		for (int num = eatObjects.Count - 1; num >= 0; num--)
		{
			if (eatObjects[num].progression > 1f)
			{
				if (eatObjects[num].chunk.owner is Creature)
				{
					AI.tracker.ForgetCreature((eatObjects[num].chunk.owner as Creature).abstractCreature);
				}
				eatObjects[num].chunk.owner.Destroy();
				eatObjects.RemoveAt(num);
			}
			else
			{
				if (eatObjects[num].chunk.owner.collisionLayer != 0)
				{
					eatObjects[num].chunk.owner.ChangeCollisionLayer(0);
				}
				float progression = eatObjects[num].progression;
				eatObjects[num].progression += 0.0125f;
				if (progression <= 0.5f && eatObjects[num].progression > 0.5f)
				{
					if (eatObjects[num].chunk.owner is Creature)
					{
						(eatObjects[num].chunk.owner as Creature).Die();
					}
					for (int i = 0; i < eatObjects[num].chunk.owner.bodyChunkConnections.Length; i++)
					{
						eatObjects[num].chunk.owner.bodyChunkConnections[i].type = BodyChunkConnection.Type.Pull;
					}
				}
				float num2 = eatObjects[num].distance * (1f - eatObjects[num].progression);
				Custom.DirVec(pos, eatObjects[num].chunk.pos);
				eatObjects[num].chunk.vel *= 0f;
				eatObjects[num].chunk.MoveFromOutsideMyUpdate(eu, pos + Custom.DirVec(pos, eatObjects[num].chunk.pos) * num2);
				for (int j = 0; j < eatObjects[num].chunk.owner.bodyChunks.Length; j++)
				{
					eatObjects[num].chunk.owner.bodyChunks[j].vel *= 1f - eatObjects[num].progression;
					eatObjects[num].chunk.owner.bodyChunks[j].MoveFromOutsideMyUpdate(eu, Vector2.Lerp(eatObjects[num].chunk.owner.bodyChunks[j].pos, pos + Custom.DirVec(pos, eatObjects[num].chunk.owner.bodyChunks[j].pos) * num2, eatObjects[num].progression));
				}
				if (eatObjects[num].chunk.owner.graphicsModule != null && eatObjects[num].chunk.owner.graphicsModule.bodyParts != null)
				{
					for (int k = 0; k < eatObjects[num].chunk.owner.graphicsModule.bodyParts.Length; k++)
					{
						eatObjects[num].chunk.owner.graphicsModule.bodyParts[k].vel *= 1f - eatObjects[num].progression;
						eatObjects[num].chunk.owner.graphicsModule.bodyParts[k].pos = Vector2.Lerp(eatObjects[num].chunk.owner.graphicsModule.bodyParts[k].pos, pos, eatObjects[num].progression);
					}
				}
			}
		}
	}

	private void bodySetup()
	{
		List<IntVector2> path = new List<IntVector2>();
		base.abstractCreature.Room.realizedRoom.RayTraceTilesList(base.abstractCreature.pos.x, base.abstractCreature.pos.y, base.abstractCreature.pos.x, 0, ref path);
		int num = 0;
		foreach (IntVector2 item in path)
		{
			if (num <= 10)
			{
				colorPickPos = base.abstractCreature.Room.realizedRoom.MiddleOfTile(item);
			}
			if (base.abstractCreature.Room.realizedRoom.GetTile(item).Solid)
			{
				break;
			}
			num++;
		}
		num = ((num >= 5) ? (num + (int)((float)num / 1.75f)) : 5);
		SetStartDirection(Custom.DirVec((base.abstractCreature.state as StowawayBugState).HomePos, (base.abstractCreature.state as StowawayBugState).aimPos));
		Random.State state = Random.state;
		Random.InitState(base.abstractCreature.ID.RandomSeed);
		if (placedDirection.y > -0.3f)
		{
			num = Random.Range(11, 21);
		}
		if (placedDirection.y > 0.25f)
		{
			num = Random.Range(1, 10);
		}
		int num2 = Random.Range(7, 12);
		huntDelay = Random.Range(97, 126);
		tentacles = new Vector2[num2][,];
		for (int i = 0; i < tentacles.Length; i++)
		{
			tentacles[i] = new Vector2[Random.Range((int)((float)num / 2f), num), 3];
		}
		teethCount = Random.Range(3, 7);
		teethSeed = Random.Range(1, 99);
		Random.state = state;
		heads = new Tentacle[base.abstractCreature.creatureTemplate.grasps];
		headFired = new bool[heads.Length];
		headCooldown = new float[heads.Length];
		headLength = Mathf.Clamp(num, 10f, 25f) * 15f;
		List<IntVector2> path2 = null;
		for (int j = 0; j < heads.Length; j++)
		{
			heads[j] = new Tentacle(this, base.mainBodyChunk, headLength);
			heads[j].MoveBase(base.abstractCreature.pos.Tile + new IntVector2(0, 1), ref path2);
			heads[j].retractFac = 1f;
			heads[j].tProps = new Tentacle.TentacleProps(stiff: true, rope: true, shorten: true, 0.5f, 0f, 0.5f, 0.05f, 0.05f, 2.2f, 42f, 1f / 3f, 5f, 15, 60, 12, 20);
			heads[j].tChunks = new Tentacle.TentacleChunk[12];
			for (int k = 0; k < heads[j].tChunks.Length; k++)
			{
				heads[j].tChunks[k] = new Tentacle.TentacleChunk(heads[j], k, (float)(k + 1) / (float)heads[j].tChunks.Length, Mathf.Lerp(3f, 5f, (float)k / (float)(heads[j].tChunks.Length - 1)));
			}
			heads[j].stretchAndSqueeze = 0.8f;
			heads[j].limp = true;
			appendages.Add(new Appendage(this, j, heads[j].tChunks.Length));
		}
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.abstractCreature.pos = new WorldCoordinate(newRoom.abstractRoom.index, pos.x, pos.y, -1);
		(base.abstractCreature.state as StowawayBugState).HomePos = newRoom.MiddleOfTile(pos) + newRoom.ShorcutEntranceHoleDirection(pos).ToVector2() * 40f;
		(base.abstractCreature.state as StowawayBugState).aimPos = (base.abstractCreature.state as StowawayBugState).HomePos + newRoom.ShorcutEntranceHoleDirection(pos).ToVector2() * 20f;
		originalPos = (base.abstractCreature.state as StowawayBugState).HomePos;
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
	}
}
