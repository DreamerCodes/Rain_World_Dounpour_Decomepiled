using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class BigSpider : InsectoidCreature, Weapon.INotifyOfFlyingWeapons
{
	public BigSpiderAI AI;

	private int footingCounter;

	public int outOfWaterFooting;

	public int specialMoveCounter;

	public IntVector2 specialMoveDestination;

	private MovementConnection lastFollowedConnection;

	public float runSpeed;

	public float runCycle;

	public bool currentlyClimbingCorridor;

	public bool sitting;

	public Vector2 travelDir;

	public float charging;

	public float mandiblesCharged;

	public float jumpStamina = 1f;

	public Vector2 jumpAtPos;

	private float carryObjectMass;

	public BodyChunk[,] grabChunks;

	public float deathConvulsions;

	public ChunkSoundEmitter warningSound;

	public Color yellowCol;

	public int canBite;

	public bool offGround;

	public int canCling;

	public float stuckShake;

	public bool spitter;

	public Vector2? spitPos;

	private Vector2 spitDir;

	public int grabbedCounter;

	private float bounceSoundVol = 1f;

	public BigSpider revivingBuddy;

	public int borrowedTime = -1;

	public bool mother;

	private bool spewBabies;

	public float selfDestruct;

	public Vector2 selfDestructOrigin;

	public bool jumping;

	public new HealthState State => base.abstractCreature.state as HealthState;

	public bool Footing
	{
		get
		{
			if (footingCounter <= 20)
			{
				return outOfWaterFooting > 0;
			}
			return true;
		}
	}

	public bool CanJump
	{
		get
		{
			if (jumpStamina >= 0.2f)
			{
				return base.grasps[0] == null;
			}
			return false;
		}
	}

	public bool CarryBackwards
	{
		get
		{
			if (carryObjectMass > 0f)
			{
				return AI.stuckTracker.Utility() == 0f;
			}
			return false;
		}
	}

	public bool LegsGrabby
	{
		get
		{
			if ((spitter || canCling > 0) && base.grasps[0] == null)
			{
				if (base.safariControlled)
				{
					if (inputWithDiagonals.HasValue)
					{
						return inputWithDiagonals.Value.pckp;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public bool CanIBeRevived
	{
		get
		{
			if (room != null && State.health > -8f && State.meatLeft > 0 && poison < 0.2f && !base.slatedForDeletetion && lungs > 0f && grabbedBy.Count < 1)
			{
				return !mother;
			}
			return false;
		}
	}

	public bool CanSpit(bool initiate)
	{
		if (base.safariControlled)
		{
			if (base.Consious && base.grasps[0] == null)
			{
				return AI.spitModule.CanSpit();
			}
			return false;
		}
		if (base.Consious && Footing && base.grasps[0] == null && AI.spitModule.CanSpit())
		{
			return Vector2.Dot(Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos), AI.spitModule.aimDir) > (initiate ? 0.6f : 0.3f);
		}
		return false;
	}

	public BigSpider(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		spitter = abstractCreature.creatureTemplate.type == CreatureTemplate.Type.SpitterSpider;
		spewBabies = false;
		mother = ModManager.MSC && abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider;
		float num = (spitter ? 1.2f : 0.8f);
		if (mother)
		{
			num = 2f;
		}
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, num * (1f / 3f));
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 9f, num * (2f / 3f));
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], spitter ? 25f : 15f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		grabChunks = new BodyChunk[2, 4];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractCreature.ID.RandomSeed);
		if (spitter)
		{
			yellowCol = Color.Lerp(new Color(1f, 0f, 0f), Custom.HSL2RGB(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), UnityEngine.Random.value * 0.2f);
		}
		else if (mother)
		{
			yellowCol = Color.Lerp(new Color(0f, 1f, 0f), Custom.HSL2RGB(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), UnityEngine.Random.value * 0.2f);
		}
		else
		{
			yellowCol = Color.Lerp(new Color(1f, 0.8f, 0.3f), Custom.HSL2RGB(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), UnityEngine.Random.value * 0.2f);
		}
		if (abstractCreature.IsVoided())
		{
			yellowCol = Color.Lerp(RainWorld.SaturatedGold, Custom.HSL2RGB(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), UnityEngine.Random.value * 0.2f);
		}
		UnityEngine.Random.state = state;
		deathConvulsions = (State.alive ? 1f : 0f);
	}

	public override Color ShortCutColor()
	{
		return yellowCol * 0.5f;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new BigSpiderGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public void Revive()
	{
		base.dead = false;
		base.abstractCreature.abstractAI.SetDestination(base.abstractCreature.pos);
		for (int num = base.abstractCreature.stuckObjects.Count - 1; num >= 0; num--)
		{
			if (base.abstractCreature.stuckObjects[num] is AbstractPhysicalObject.AbstractSpearStick && base.abstractCreature.stuckObjects[num].A.type == AbstractPhysicalObject.AbstractObjectType.Spear && base.abstractCreature.stuckObjects[num].A.realizedObject != null)
			{
				(base.abstractCreature.stuckObjects[num].A.realizedObject as Spear).ChangeMode(Weapon.Mode.Free);
			}
		}
		if (ModManager.MMF)
		{
			base.abstractCreature.LoseAllStuckObjects();
		}
		deathConvulsions = 1f;
		killTag = null;
		killTagCounter = 0;
	}

	public override void Update(bool eu)
	{
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (!base.dead && State.health < 1f)
		{
			if (State.health < 0f && UnityEngine.Random.value < 0f - State.health && UnityEngine.Random.value < 0.025f)
			{
				Die();
			}
			if (UnityEngine.Random.value * 0.7f > State.health && UnityEngine.Random.value < 0.125f)
			{
				Stun(UnityEngine.Random.Range(1, UnityEngine.Random.Range(1, 27 - Custom.IntClamp((int)(20f * State.health), 0, 10))));
			}
		}
		if (outOfWaterFooting > 0)
		{
			outOfWaterFooting--;
		}
		if (canBite > 0)
		{
			bool flag = false;
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				if (base.bodyChunks[i].ContactPoint.x != 0 || base.bodyChunks[i].ContactPoint.y != 0)
				{
					flag = true;
				}
			}
			if (flag)
			{
				if (offGround)
				{
					canBite = Math.Min(canBite, 10);
				}
				offGround = false;
			}
			else
			{
				offGround = true;
			}
			canBite--;
		}
		bounceSoundVol = Mathf.Min(1f, bounceSoundVol + 1f / 120f);
		if (State.health > 0f)
		{
			State.health = Mathf.Min(1f, State.health + 0.0014705883f);
		}
		base.Update(eu);
		if (!base.dead && base.stun < 35 && grabbedBy.Count > 0 && !(grabbedBy[0].grabber is Vulture) && !(grabbedBy[0].grabber is Leech))
		{
			grabbedCounter++;
			if (grabbedCounter == 25 && UnityEngine.Random.value < 0.65f * State.health)
			{
				BodyChunk bodyChunk = grabbedBy[0].grabber.mainBodyChunk;
				float dst = float.MaxValue;
				for (int j = 0; j < grabbedBy[0].grabber.bodyChunks.Length; j++)
				{
					if (Custom.DistLess(base.mainBodyChunk.pos, grabbedBy[0].grabber.bodyChunks[j].pos, dst))
					{
						dst = Vector2.Distance(base.mainBodyChunk.pos, grabbedBy[0].grabber.bodyChunks[j].pos);
						bodyChunk = grabbedBy[0].grabber.bodyChunks[j];
					}
				}
				for (int k = 0; k < AI.relationshipTracker.relationships.Count; k++)
				{
					if (AI.relationshipTracker.relationships[k].trackerRep.representedCreature == grabbedBy[0].grabber.abstractCreature)
					{
						(AI.relationshipTracker.relationships[k].state as BigSpiderAI.SpiderTrackState).accustomed = 0;
						break;
					}
				}
				if (spitter)
				{
					AI.spitModule.spitAtCrit = AI.tracker.RepresentationForCreature(grabbedBy[0].grabber.abstractCreature, addIfMissing: false);
					AI.spitModule.randomCritSpitDelay = 140;
				}
				grabbedBy[0].grabber.Violence(base.mainBodyChunk, Custom.DirVec(base.mainBodyChunk.pos, bodyChunk.pos) * 8f, bodyChunk, null, DamageType.Bite, spitter ? 0.6f : 0.4f, 20f);
				base.mainBodyChunk.vel += Custom.DirVec(bodyChunk.pos, base.mainBodyChunk.pos) * 8f;
				AI.stayAway = true;
				room.PlaySound(SoundID.Big_Spider_Slash_Creature, base.mainBodyChunk);
				if (grabbedBy.Count == 0)
				{
					base.stun = 0;
				}
			}
			else if (grabbedCounter < 25)
			{
				base.bodyChunks[0].pos += Custom.RNV() * UnityEngine.Random.value * 6f;
				base.bodyChunks[1].pos += Custom.RNV() * UnityEngine.Random.value * 6f;
				base.bodyChunks[0].vel += Custom.RNV() * UnityEngine.Random.value * 6f;
				base.bodyChunks[1].vel += Custom.RNV() * UnityEngine.Random.value * 6f;
			}
		}
		else
		{
			grabbedCounter = 0;
		}
		if (base.graphicsModule != null && room != null && base.Consious && !room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template) && !room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos, base.Template))
		{
			for (int l = 0; l < 2; l++)
			{
				for (int m = 0; m < 4; m++)
				{
					if ((base.graphicsModule as BigSpiderGraphics).legs[l, m].reachedSnapPosition && !Custom.DistLess(base.mainBodyChunk.pos, (base.graphicsModule as BigSpiderGraphics).legs[l, m].absoluteHuntPos, (base.graphicsModule as BigSpiderGraphics).legLength) && Custom.DistLess(base.mainBodyChunk.pos, (base.graphicsModule as BigSpiderGraphics).legs[l, m].absoluteHuntPos, (base.graphicsModule as BigSpiderGraphics).legLength + 15f))
					{
						Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, (base.graphicsModule as BigSpiderGraphics).legs[l, m].absoluteHuntPos) * (Vector2.Distance(base.mainBodyChunk.pos, (base.graphicsModule as BigSpiderGraphics).legs[l, m].absoluteHuntPos) - (base.graphicsModule as BigSpiderGraphics).legLength);
						base.mainBodyChunk.pos += vector * 0.8f;
						base.mainBodyChunk.vel += vector * 0.8f;
					}
				}
			}
		}
		if (room == null)
		{
			return;
		}
		sitting = false;
		currentlyClimbingCorridor = false;
		if (warningSound != null && (charging == 0f || charging == 1f || jumping))
		{
			if (warningSound.slatedForDeletetion)
			{
				warningSound = null;
			}
			else
			{
				warningSound.volume = Mathf.Max(0f, warningSound.volume - 0.3f);
				if (warningSound.volume <= 0f)
				{
					warningSound.Destroy();
					warningSound = null;
				}
			}
		}
		if (borrowedTime > 0)
		{
			borrowedTime--;
		}
		else if (borrowedTime == 0 && !base.dead)
		{
			State.health -= 1f / 140f;
		}
		if (base.Consious)
		{
			if (room.aimap.TileAccessibleToCreature(base.bodyChunks[0].pos, base.Template) || room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos, base.Template))
			{
				footingCounter++;
			}
			Act();
			if (charging == 0f && canBite == 0)
			{
				mandiblesCharged = Mathf.Max(0f, mandiblesCharged - 0.1f);
				if (!jumping)
				{
					jumpStamina = Mathf.Min(1f, jumpStamina + 0.0033333334f);
					if (jumpStamina == 1f && !spitter)
					{
						AI.stayAway = false;
					}
				}
			}
			else
			{
				mandiblesCharged = Custom.LerpAndTick(mandiblesCharged, (charging > 0f || canBite > 0) ? 1f : 0f, 0.01f, 1f / 44f);
			}
			if (AI.utilityComparer.GetUtilityTracker(AI.threatTracker).SmoothedUtility() < 0.9f)
			{
				canCling = 40;
			}
			else if (canCling > 0)
			{
				canCling--;
			}
			if (revivingBuddy != null)
			{
				if (!Custom.DistLess(revivingBuddy.mainBodyChunk.pos, base.mainBodyChunk.pos, 50f) || !revivingBuddy.dead || revivingBuddy.enteringShortCut.HasValue || !revivingBuddy.CanIBeRevived)
				{
					revivingBuddy = null;
					if (UnityEngine.Random.value < 0.5f)
					{
						ReleaseAllGrabChunks();
					}
				}
				else
				{
					Vector2 vector2 = Custom.DirVec(revivingBuddy.mainBodyChunk.pos, base.mainBodyChunk.pos) * (Vector2.Distance(revivingBuddy.mainBodyChunk.pos, base.mainBodyChunk.pos) - 20f);
					revivingBuddy.mainBodyChunk.vel += vector2 * 0.5f;
					revivingBuddy.mainBodyChunk.pos += vector2 * 0.5f;
					base.mainBodyChunk.vel -= vector2 * 0.5f;
					base.mainBodyChunk.pos -= vector2 * 0.5f;
					base.bodyChunks[1].vel += Custom.RNV() * 4f;
					base.bodyChunks[1].pos += Custom.RNV() * 4f;
					if (UnityEngine.Random.value < 1f / 30f)
					{
						room.PlaySound(SoundID.Big_Spider_Revive, base.mainBodyChunk);
					}
					if (room.BeingViewed)
					{
						room.AddObject(new WaterDrip(Vector2.Lerp(base.mainBodyChunk.pos, revivingBuddy.mainBodyChunk.pos, UnityEngine.Random.value), base.mainBodyChunk.vel * UnityEngine.Random.value + Custom.RNV() * UnityEngine.Random.value * 7f, waterColor: false));
					}
					WeightedPush(1, 0, Custom.DirVec(revivingBuddy.mainBodyChunk.pos, base.mainBodyChunk.pos), 3f);
					revivingBuddy.borrowedTime += 5;
					revivingBuddy.State.health = Mathf.Min(1f, revivingBuddy.State.health + (revivingBuddy.spitter ? 0.5f : 1f) / ((revivingBuddy.State.health < -1f) ? 160f : 60f));
					if (revivingBuddy.borrowedTime > 300 && revivingBuddy.State.health == 1f)
					{
						revivingBuddy.Revive();
					}
				}
			}
		}
		else
		{
			spitPos = null;
			footingCounter = 0;
			charging = 0f;
			jumping = false;
			mandiblesCharged *= 0.8f;
			revivingBuddy = null;
			if (deathConvulsions > 0f)
			{
				if (base.dead)
				{
					deathConvulsions = Mathf.Max(0f, deathConvulsions - UnityEngine.Random.value / 80f);
				}
				if (base.mainBodyChunk.ContactPoint.x != 0 || base.mainBodyChunk.ContactPoint.y != 0)
				{
					base.mainBodyChunk.vel += Custom.RNV() * UnityEngine.Random.value * 8f * Mathf.Pow(deathConvulsions, 0.5f);
				}
				if (base.bodyChunks[1].ContactPoint.x != 0 || base.bodyChunks[1].ContactPoint.y != 0)
				{
					base.bodyChunks[1].vel += Custom.RNV() * UnityEngine.Random.value * 4f * Mathf.Pow(deathConvulsions, 0.5f);
				}
				if (UnityEngine.Random.value < 0.05f)
				{
					room.PlaySound(SoundID.Big_Spider_Death_Rustle, base.mainBodyChunk, loop: false, 0.5f + UnityEngine.Random.value * 0.5f * deathConvulsions, 0.9f + 0.3f * deathConvulsions);
				}
				if (UnityEngine.Random.value < 0.025f)
				{
					room.PlaySound(SoundID.Big_Spider_Take_Damage, base.mainBodyChunk, loop: false, UnityEngine.Random.value * 0.5f, 1f);
				}
			}
			LoseAllGrasps();
		}
		if (Footing)
		{
			for (int n = 0; n < 2; n++)
			{
				base.bodyChunks[n].vel *= 0.8f;
				base.bodyChunks[n].vel.y += base.gravity;
			}
		}
		int num = 0;
		for (int num2 = 0; num2 < grabChunks.GetLength(0); num2++)
		{
			for (int num3 = 0; num3 < grabChunks.GetLength(1); num3++)
			{
				if (grabChunks[num2, num3] == null)
				{
					continue;
				}
				if (!Custom.DistLess(grabChunks[num2, num3].pos, base.mainBodyChunk.pos, 60f + grabChunks[num2, num3].rad) || UnityEngine.Random.value < 1f / (((grabChunks[num2, num3].owner is Player) ? Mathf.Lerp(180f, 30f, (grabChunks[num2, num3].owner as Player).GraspWiggle) : 160f) * (LegsGrabby ? 1f : 0.2f)) || base.grasps[0] != null || charging > 0f || canBite > 0 || (grabChunks[num2, num3].owner is Creature && (grabChunks[num2, num3].owner as Creature).enteringShortCut.HasValue) || !room.VisualContact(base.mainBodyChunk.pos, grabChunks[num2, num3].pos))
				{
					grabChunks[num2, num3] = null;
				}
				else
				{
					if (Custom.DistLess(grabChunks[num2, num3].pos, base.mainBodyChunk.pos, 25f + grabChunks[num2, num3].rad))
					{
						continue;
					}
					Vector2 vector3 = Custom.DirVec(grabChunks[num2, num3].pos, base.mainBodyChunk.pos) * (Vector2.Distance(grabChunks[num2, num3].pos, base.mainBodyChunk.pos) - (25f + grabChunks[num2, num3].rad));
					float num4 = base.mainBodyChunk.mass / (base.mainBodyChunk.mass + grabChunks[num2, num3].mass);
					grabChunks[num2, num3].vel += vector3 * num4 * 0.65f;
					grabChunks[num2, num3].pos += vector3 * num4 * 0.65f;
					base.mainBodyChunk.vel -= vector3 * (1f - num4) * 0.65f;
					base.mainBodyChunk.pos -= vector3 * (1f - num4) * 0.65f;
					num++;
					num4 = base.mainBodyChunk.mass / (base.mainBodyChunk.mass + base.bodyChunks[1].mass);
					base.bodyChunks[1].vel += Custom.DirVec(grabChunks[num2, num3].pos, base.mainBodyChunk.pos) * num4 * 0.4f;
					base.mainBodyChunk.vel -= Custom.DirVec(grabChunks[num2, num3].pos, base.mainBodyChunk.pos) * (1f - num4) * 0.4f;
					if ((UnityEngine.Random.value < 1f / 3f && LegsGrabby) || revivingBuddy != null)
					{
						IntVector2 intVector = new IntVector2(UnityEngine.Random.Range(0, 2), UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, 4)));
						BodyChunk bodyChunk2 = grabChunks[num2, num3].owner.bodyChunks[UnityEngine.Random.Range(0, grabChunks[num2, num3].owner.bodyChunks.Length)];
						if (grabChunks[intVector.x, intVector.y] == null && Custom.DistLess(bodyChunk2.pos, base.mainBodyChunk.pos, 50f + bodyChunk2.rad))
						{
							grabChunks[intVector.x, intVector.y] = bodyChunk2;
						}
					}
				}
			}
		}
		if (num > 3 && base.Submersion == 0f)
		{
			footingCounter = 0;
		}
		if (spitter && num > 3 && base.grasps[0] == null && !base.safariControlled)
		{
			base.bodyChunks[1].vel += Custom.RNV() * UnityEngine.Random.value * 5f;
			for (int num5 = 0; num5 < 2; num5++)
			{
				if (base.grasps[0] != null)
				{
					break;
				}
				for (int num6 = 0; num6 < 4; num6++)
				{
					IntVector2 intVector2 = new IntVector2(num5, num6);
					if (grabChunks[intVector2.x, intVector2.y] == null || !(grabChunks[intVector2.x, intVector2.y].owner is Creature) || !(UnityEngine.Random.value < 1f / ((grabChunks[intVector2.x, intVector2.y].owner as Creature).Consious ? 120f : 8f)) || !(AI.DynamicRelationship((grabChunks[intVector2.x, intVector2.y].owner as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats))
					{
						continue;
					}
					if (UnityEngine.Random.value < 0.125f || !(grabChunks[intVector2.x, intVector2.y].owner as Creature).Consious)
					{
						if (Grab(grabChunks[intVector2.x, intVector2.y].owner, 0, grabChunks[intVector2.x, intVector2.y].index, Grasp.Shareability.CanNotShare, 0.5f, overrideEquallyDominant: true, pacifying: true))
						{
							room.PlaySound(SoundID.Big_Spider_Grab_Creature, base.mainBodyChunk);
						}
					}
					else
					{
						(grabChunks[intVector2.x, intVector2.y].owner as Creature).Stun(20);
					}
					base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, grabChunks[intVector2.x, intVector2.y].pos) * 11f;
					base.firstChunk.pos += Custom.DirVec(base.firstChunk.pos, grabChunks[intVector2.x, intVector2.y].pos) * 6f;
					break;
				}
			}
		}
		travelDir *= (sitting ? 0.5f : 0.9995f);
		if (base.Consious && !Footing && AI.behavior == BigSpiderAI.Behavior.Flee)
		{
			for (int num7 = 0; num7 < 2; num7++)
			{
				if (room.aimap.TileAccessibleToCreature(room.GetTilePosition(base.bodyChunks[num7].pos), base.Template))
				{
					base.bodyChunks[num7].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 5f;
				}
			}
		}
		if (base.grasps[0] != null)
		{
			CarryObject(eu);
		}
		else
		{
			carryObjectMass = 0f;
		}
	}

	private void CarryObject(bool eu)
	{
		if (!base.safariControlled && UnityEngine.Random.value < 0.025f && (!(base.grasps[0].grabbed is Creature) || AI.DynamicRelationship((base.grasps[0].grabbed as Creature).abstractCreature).type != CreatureTemplate.Relationship.Type.Eats))
		{
			LoseAllGrasps();
			return;
		}
		PhysicalObject grabbed = base.grasps[0].grabbed;
		carryObjectMass = grabbed.bodyChunks[base.grasps[0].chunkGrabbed].owner.TotalMass;
		if (carryObjectMass <= base.TotalMass * 1.4f)
		{
			carryObjectMass /= 2f;
		}
		else if (carryObjectMass <= base.TotalMass / 5f)
		{
			carryObjectMass = 0f;
		}
		float num = base.mainBodyChunk.rad + base.grasps[0].grabbed.bodyChunks[base.grasps[0].chunkGrabbed].rad;
		Vector2 vector = -Custom.DirVec(base.mainBodyChunk.pos, grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos) * (num - Vector2.Distance(base.mainBodyChunk.pos, grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos));
		float num2 = grabbed.bodyChunks[base.grasps[0].chunkGrabbed].mass / (base.mainBodyChunk.mass + grabbed.bodyChunks[base.grasps[0].chunkGrabbed].mass);
		num2 *= 0.2f * (1f - AI.stuckTracker.Utility());
		base.mainBodyChunk.pos += vector * num2;
		base.mainBodyChunk.vel += vector * num2;
		grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos -= vector * (1f - num2);
		grabbed.bodyChunks[base.grasps[0].chunkGrabbed].vel -= vector * (1f - num2);
		Vector2 vector2 = base.mainBodyChunk.pos + Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos) * num;
		Vector2 vector3 = grabbed.bodyChunks[base.grasps[0].chunkGrabbed].vel - base.mainBodyChunk.vel;
		grabbed.bodyChunks[base.grasps[0].chunkGrabbed].vel = base.mainBodyChunk.vel;
		if (!enteringShortCut.HasValue && (vector3.magnitude * grabbed.bodyChunks[base.grasps[0].chunkGrabbed].mass > 30f || !Custom.DistLess(vector2, grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos, 70f + grabbed.bodyChunks[base.grasps[0].chunkGrabbed].rad)))
		{
			LoseAllGrasps();
		}
		else
		{
			grabbed.bodyChunks[base.grasps[0].chunkGrabbed].MoveFromOutsideMyUpdate(eu, vector2);
		}
		if (base.grasps[0] != null)
		{
			for (int i = 0; i < 2; i++)
			{
				base.grasps[0].grabbed.PushOutOf(base.bodyChunks[i].pos, base.bodyChunks[i].rad, base.grasps[0].chunkGrabbed);
			}
		}
	}

	private void Swim()
	{
		if (ModManager.MMF)
		{
			charging = 0f;
		}
		selfDestruct = 0f;
		selfDestructOrigin = base.mainBodyChunk.pos;
		base.bodyChunks[0].vel *= 1f - 0.1f * base.bodyChunks[0].submersion;
		base.bodyChunks[1].vel *= 1f - 0.2f * base.bodyChunks[1].submersion;
		base.GoThroughFloors = true;
		MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[0].pos), actuallyFollowingThisPath: true);
		if (movementConnection == default(MovementConnection))
		{
			movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[1].pos), actuallyFollowingThisPath: true);
		}
		if (movementConnection == default(MovementConnection) && Math.Abs(base.abstractCreature.pos.y - room.defaultWaterLevel) < 4)
		{
			movementConnection = (AI.pathFinder as StandardPather).FollowPath(new WorldCoordinate(base.abstractCreature.pos.room, base.abstractCreature.pos.x, room.defaultWaterLevel, base.abstractCreature.pos.abstractNode), actuallyFollowingThisPath: true);
		}
		if (base.safariControlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
		{
			movementConnection = default(MovementConnection);
			footingCounter = 0;
			if (inputWithDiagonals.HasValue)
			{
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				if (inputWithDiagonals.Value.AnyDirectionalInput)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
				}
			}
		}
		if (movementConnection != default(MovementConnection))
		{
			if (base.graphicsModule != null)
			{
				(base.graphicsModule as BigSpiderGraphics).flip = Mathf.Lerp((base.graphicsModule as BigSpiderGraphics).flip, Mathf.Sign(room.MiddleOfTile(movementConnection.StartTile).x - room.MiddleOfTile(movementConnection.DestTile).x), 0.25f);
			}
			if (movementConnection.StartTile.y == movementConnection.DestTile.y && movementConnection.DestTile.y == room.defaultWaterLevel)
			{
				base.mainBodyChunk.vel.x -= Mathf.Sign(room.MiddleOfTile(movementConnection.StartTile).x - room.MiddleOfTile(movementConnection.DestTile).x) * 1.2f * base.bodyChunks[0].submersion;
				base.bodyChunks[1].vel.x += Mathf.Sign(room.MiddleOfTile(movementConnection.StartTile).x - room.MiddleOfTile(movementConnection.DestTile).x) * 0.5f * base.bodyChunks[1].submersion;
				footingCounter = 0;
				return;
			}
			base.bodyChunks[0].vel *= 0.8f;
			base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord)) * 1.4f;
			if (!base.safariControlled || base.Submersion < 0.5f)
			{
				footingCounter = Math.Max(footingCounter, 25);
				Run(movementConnection);
				outOfWaterFooting = 20;
			}
			else
			{
				base.mainBodyChunk.vel *= 0.65f;
				footingCounter = 0;
				Run(movementConnection);
				outOfWaterFooting = 0;
			}
		}
		else
		{
			base.mainBodyChunk.vel.y += 0.5f;
		}
	}

	private void Act()
	{
		if (base.Submersion > 0.3f)
		{
			Swim();
			AI.Update();
			spitPos = null;
			return;
		}
		if (spitter)
		{
			if (spitPos.HasValue)
			{
				if (AI.spitModule.AbandonSitAndSpit())
				{
					spitPos = null;
					AI.spitModule.noSitDelay = 60;
				}
				else
				{
					if (!Custom.DistLess(spitDir, AI.spitModule.aimDir, 0.3f))
					{
						spitDir = (spitDir + AI.spitModule.aimDir * 0.2f).normalized;
					}
					base.bodyChunks[0].vel *= 0.5f;
					base.bodyChunks[1].vel -= spitDir;
					Vector2 vector = spitPos.Value + spitDir * Mathf.Lerp(5f, -5f, charging);
					base.bodyChunks[1].pos = Vector2.Lerp(base.bodyChunks[1].pos, vector + -spitDir * bodyChunkConnections[0].distance * 0.5f, 0.2f);
					base.bodyChunks[0].vel *= 0.5f;
					base.bodyChunks[0].vel += spitDir;
					base.bodyChunks[0].pos = Vector2.Lerp(base.bodyChunks[0].pos, vector + spitDir * bodyChunkConnections[0].distance * 0.5f, 0.2f);
					footingCounter = 30;
				}
				AI.Update();
			}
			else if (AI.spitModule.SitAndSpit())
			{
				spitPos = base.bodyChunks[0].pos;
				spitDir = Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
			}
			if (charging > 0f)
			{
				if (room.aimap.getAItile(base.mainBodyChunk.pos).fallRiskTile.y > base.abstractCreature.pos.Tile.y - 20)
				{
					base.bodyChunks[1].vel -= AI.spitModule.aimDir * charging * 2f;
					base.bodyChunks[0].vel += AI.spitModule.aimDir * charging;
				}
				spitDir = (spitDir + AI.spitModule.aimDir * 0.2f).normalized;
				if (CanSpit(initiate: false))
				{
					charging += 0.05f;
					if (charging > 1f)
					{
						Spit();
					}
				}
				else
				{
					charging = 0f;
				}
			}
			if (spitPos.HasValue || charging > 0f)
			{
				return;
			}
		}
		if (jumping)
		{
			bool flag = false;
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				if ((base.bodyChunks[i].ContactPoint.x != 0 || base.bodyChunks[i].ContactPoint.y != 0) && room.aimap.TileAccessibleToCreature(base.bodyChunks[i].pos, base.Template))
				{
					flag = true;
				}
			}
			if (flag)
			{
				footingCounter++;
			}
			else
			{
				footingCounter = 0;
			}
			if (AI.preyTracker.MostAttractivePrey != null && AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature != null && AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.room == room && AI.preyTracker.MostAttractivePrey.VisualContact)
			{
				Vector2 pos = AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos;
				base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, pos) * 1f;
				base.bodyChunks[1].vel -= Custom.DirVec(base.bodyChunks[0].pos, pos) * 0.5f;
				if (base.graphicsModule != null)
				{
					for (int j = 0; j < 2; j++)
					{
						for (int k = 0; k < 2; k++)
						{
							(base.graphicsModule as BigSpiderGraphics).legs[j, k].mode = Limb.Mode.Dangle;
							(base.graphicsModule as BigSpiderGraphics).legFlips[j, k, 0] = ((j == 0) ? (-1f) : 1f);
							(base.graphicsModule as BigSpiderGraphics).legs[j, k].vel += (Vector2)Vector3.Slerp(Custom.DirVec(base.mainBodyChunk.pos, pos), Custom.PerpendicularVector(base.mainBodyChunk.pos, pos) * ((j == 0) ? (-1f) : 1f), (k == 0) ? 0.1f : 0.5f) * 3f;
						}
					}
				}
			}
			if (Footing)
			{
				jumping = false;
			}
			return;
		}
		if (AI.stuckTracker.Utility() > 0.9f)
		{
			stuckShake = Custom.LerpAndTick(stuckShake, 1f, 0.07f, 1f / 70f);
		}
		else if (AI.stuckTracker.Utility() < 0.2f)
		{
			stuckShake = Custom.LerpAndTick(stuckShake, 0f, 0.07f, 0.05f);
		}
		if (stuckShake > 0f && (!base.safariControlled || (inputWithDiagonals.HasValue && inputWithDiagonals.Value.AnyDirectionalInput)))
		{
			for (int l = 0; l < base.bodyChunks.Length; l++)
			{
				base.bodyChunks[l].vel += Custom.RNV() * UnityEngine.Random.value * 5f * stuckShake;
				base.bodyChunks[l].pos += Custom.RNV() * UnityEngine.Random.value * 5f * stuckShake;
			}
		}
		if (ModManager.MSC && selfDestruct > 0f)
		{
			for (int m = 0; m < base.bodyChunks.Length; m++)
			{
				base.bodyChunks[m].vel += Custom.RNV() * (UnityEngine.Random.value * 10f * selfDestruct + 3f);
			}
			base.mainBodyChunk.pos.x = selfDestructOrigin.x;
			if (base.mainBodyChunk.pos.y > selfDestructOrigin.y)
			{
				base.mainBodyChunk.pos.y = selfDestructOrigin.y;
			}
		}
		if (specialMoveCounter > 0)
		{
			specialMoveCounter--;
			footingCounter = Mathf.Max(footingCounter, 30);
			MoveTowards(room.MiddleOfTile(specialMoveDestination));
			travelDir = Vector2.Lerp(travelDir, Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(specialMoveDestination)), 0.4f);
			if (Custom.DistLess(base.mainBodyChunk.pos, room.MiddleOfTile(specialMoveDestination), 5f))
			{
				specialMoveCounter = 0;
			}
		}
		else
		{
			if (!room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template) && !room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos, base.Template))
			{
				footingCounter = Custom.IntClamp(footingCounter - 3, 0, 35);
			}
			if (Footing && charging > 0f)
			{
				sitting = true;
				base.GoThroughFloors = false;
				charging += 0.05f;
				Vector2 vector2 = Custom.DirVec(base.mainBodyChunk.pos, jumpAtPos);
				base.bodyChunks[0].vel += vector2 * Mathf.Pow(charging, 2f);
				base.bodyChunks[1].vel -= vector2 * Mathf.Lerp(0.7f, 2f, charging);
				if (charging >= 1f)
				{
					Attack();
				}
			}
			else if ((room.GetWorldCoordinate(base.mainBodyChunk.pos) == AI.pathFinder.GetDestination || room.GetWorldCoordinate(base.bodyChunks[1].pos) == AI.pathFinder.GetDestination) && AI.threatTracker.Utility() < 0.5f && !base.safariControlled)
			{
				sitting = true;
				base.GoThroughFloors = false;
			}
			else
			{
				MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[CarryBackwards ? 1 : 0].pos), actuallyFollowingThisPath: true);
				if (movementConnection == default(MovementConnection))
				{
					movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[(!CarryBackwards) ? 1u : 0u].pos), actuallyFollowingThisPath: true);
				}
				if (base.safariControlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
				{
					movementConnection = default(MovementConnection);
					if (inputWithDiagonals.HasValue && charging == 0f)
					{
						MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
						if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
						{
							type = MovementConnection.MovementType.ShortCut;
						}
						if (inputWithDiagonals.Value.AnyDirectionalInput)
						{
							movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
						}
						if (inputWithDiagonals.Value.jmp && !lastInputWithDiagonals.Value.jmp)
						{
							if (spitter)
							{
								TryInitiateSpit();
							}
							else if (inputWithDiagonals.Value.AnyDirectionalInput)
							{
								InitiateJump(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f);
							}
							else
							{
								InitiateJump(base.mainBodyChunk.pos + travelDir * 40f);
							}
						}
						if (inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw)
						{
							ReleaseAllGrabChunks();
							LoseAllGrasps();
							revivingBuddy = null;
						}
						if (inputWithDiagonals.Value.y < 0)
						{
							base.GoThroughFloors = true;
						}
						else
						{
							base.GoThroughFloors = false;
						}
						if (inputWithDiagonals.Value.thrw && mother)
						{
							if (selfDestruct == 0f)
							{
								selfDestructOrigin = base.mainBodyChunk.pos;
							}
							selfDestruct += 0.02f;
							movementConnection = default(MovementConnection);
							if (selfDestruct >= 1f)
							{
								Die();
							}
						}
						else
						{
							selfDestruct = 0f;
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
		AI.Update();
		if (base.Consious && !Custom.DistLess(base.mainBodyChunk.pos, base.mainBodyChunk.lastPos, 2f))
		{
			runCycle += 0.0625f;
		}
	}

	public override void Stun(int st)
	{
		base.Stun(st);
		if (st > 4 && UnityEngine.Random.value < 0.5f)
		{
			LoseAllGrasps();
		}
		revivingBuddy = null;
		ReleaseAllGrabChunks();
	}

	public void ReleaseAllGrabChunks()
	{
		for (int i = 0; i < grabChunks.GetLength(0); i++)
		{
			for (int j = 0; j < grabChunks.GetLength(1); j++)
			{
				grabChunks[i, j] = null;
			}
		}
	}

	public void InitiateJump(Vector2 target)
	{
		if (!(charging > 0f) && !jumping && CanJump)
		{
			charging = 0.01f;
			jumpAtPos = target;
			warningSound = room.PlaySound(SoundID.Big_Spider_Jump_Warning_Rustle, base.mainBodyChunk);
		}
	}

	public void TryInitiateSpit()
	{
		if (!(charging > 0f) && CanSpit(initiate: true))
		{
			charging = 0.01f;
			warningSound = room.PlaySound(SoundID.Big_Spider_Spit_Warning_Rustle, base.mainBodyChunk);
		}
	}

	public void Spit()
	{
		Vector2 vector = AI.spitModule.aimDir;
		if (base.safariControlled)
		{
			vector = ((!inputWithDiagonals.HasValue || !inputWithDiagonals.Value.AnyDirectionalInput) ? travelDir.normalized : new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y).normalized);
			Creature creature = null;
			float num = float.MaxValue;
			float current = Custom.VecToDeg(vector);
			for (int i = 0; i < base.abstractCreature.Room.creatures.Count; i++)
			{
				if (base.abstractCreature != base.abstractCreature.Room.creatures[i] && base.abstractCreature.Room.creatures[i].realizedCreature != null)
				{
					float target = Custom.AimFromOneVectorToAnother(base.mainBodyChunk.pos, base.abstractCreature.Room.creatures[i].realizedCreature.mainBodyChunk.pos);
					float num2 = Custom.Dist(base.mainBodyChunk.pos, base.abstractCreature.Room.creatures[i].realizedCreature.mainBodyChunk.pos);
					if (Mathf.Abs(Mathf.DeltaAngle(current, target)) < 22.5f && num2 < num)
					{
						num = num2;
						creature = base.abstractCreature.Room.creatures[i].realizedCreature;
					}
				}
			}
			if (creature != null)
			{
				vector = Custom.DirVec(base.mainBodyChunk.pos, creature.mainBodyChunk.pos);
			}
		}
		charging = 0f;
		base.mainBodyChunk.pos += vector * 12f;
		base.mainBodyChunk.vel += vector * 2f;
		AbstractPhysicalObject obj = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.DartMaggot, null, base.abstractCreature.pos, room.game.GetNewID());
		obj.RealizeInRoom();
		(obj.realizedObject as DartMaggot).Shoot(base.mainBodyChunk.pos, vector, this);
		room.PlaySound(SoundID.Big_Spider_Spit, base.mainBodyChunk);
		AI.spitModule.SpiderHasSpit();
	}

	private void Attack()
	{
		if (!base.safariControlled && (AI.preyTracker.MostAttractivePrey == null || !CanJump || !AI.preyTracker.MostAttractivePrey.VisualContact || !room.VisualContact(base.mainBodyChunk.pos, jumpAtPos) || AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature == null || AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.room != room))
		{
			charging = 0f;
			return;
		}
		if (base.safariControlled && !CanJump)
		{
			charging = 0f;
			return;
		}
		Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, jumpAtPos);
		if (base.safariControlled && inputWithDiagonals.HasValue && inputWithDiagonals.Value.AnyDirectionalInput)
		{
			vector = Custom.DirVec(base.mainBodyChunk.pos, base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f);
		}
		if (!base.safariControlled)
		{
			Vector2 pos = AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos;
			pos += new Vector2(0f, Mathf.InverseLerp(40f, 300f, Vector2.Distance(base.mainBodyChunk.pos, pos)) * 40f);
			if (!Custom.DistLess(base.mainBodyChunk.pos, pos, Custom.LerpMap(Vector2.Dot(vector, Custom.DirVec(base.mainBodyChunk.pos, pos)), -1f, 1f, 0f, 500f)))
			{
				charging = 0f;
				return;
			}
		}
		jumpStamina = Mathf.Max(0f, jumpStamina - 0.35f);
		if (jumpStamina < 0.2f && UnityEngine.Random.value < 0.5f && !spitter)
		{
			AI.stayAway = true;
		}
		if (!room.GetTile(base.mainBodyChunk.pos + new Vector2(0f, 20f)).Solid && !room.GetTile(base.bodyChunks[1].pos + new Vector2(0f, 20f)).Solid)
		{
			vector = Vector3.Slerp(vector, new Vector2(0f, 1f), Custom.LerpMap(Vector2.Distance(base.mainBodyChunk.pos, jumpAtPos), 40f, 400f, 0.2f, 0.5f));
		}
		Jump(vector, 1f);
		canBite = 40;
	}

	private void Jump(Vector2 jumpDir, float soundVol)
	{
		float num = Custom.LerpMap(jumpDir.y, -1f, 1f, 0.7f, 1.2f, 1.1f);
		footingCounter = 0;
		base.mainBodyChunk.vel *= 0.5f;
		base.bodyChunks[1].vel *= 0.5f;
		base.mainBodyChunk.vel += jumpDir * 16f * num;
		base.bodyChunks[1].vel += jumpDir * 11f * num;
		charging = 0f;
		jumping = true;
		if (base.graphicsModule != null)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					(base.graphicsModule as BigSpiderGraphics).legs[i, j].mode = Limb.Mode.Dangle;
					(base.graphicsModule as BigSpiderGraphics).legs[i, j].vel += jumpDir * 30f * ((j < 2) ? 1f : (-1f));
				}
			}
		}
		room.PlaySound(SoundID.Big_Spider_Jump, base.mainBodyChunk, loop: false, soundVol, 1f);
	}

	private void Run(MovementConnection followingConnection)
	{
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
			MovementConnection movementConnection = followingConnection;
			if (AI.stuckTracker.Utility() == 0f)
			{
				MovementConnection movementConnection2 = (AI.pathFinder as StandardPather).FollowPath(movementConnection.destinationCoord, actuallyFollowingThisPath: false);
				if (movementConnection2 != default(MovementConnection))
				{
					if (movementConnection2.destinationCoord == followingConnection.startCoord)
					{
						sitting = true;
						return;
					}
					if (movementConnection2.destinationCoord.TileDefined && room.aimap.getAItile(movementConnection2.DestTile).acc < AItile.Accessibility.Ceiling)
					{
						bool flag2 = false;
						for (int j = Math.Min(followingConnection.StartTile.x, movementConnection2.DestTile.x); j < Math.Max(followingConnection.StartTile.x, movementConnection2.DestTile.x); j++)
						{
							if (flag2)
							{
								break;
							}
							for (int k = Math.Min(followingConnection.StartTile.y, movementConnection2.DestTile.y); k < Math.Max(followingConnection.StartTile.y, movementConnection2.DestTile.y); k++)
							{
								if (!room.aimap.TileAccessibleToCreature(j, k, base.Template))
								{
									flag2 = true;
									break;
								}
							}
						}
						if (!flag2)
						{
							movementConnection = movementConnection2;
						}
					}
				}
			}
			Vector2 vector = room.MiddleOfTile(movementConnection.DestTile);
			travelDir = Vector2.Lerp(travelDir, Custom.DirVec(base.bodyChunks[CarryBackwards ? 1 : 0].pos, vector), 0.4f);
			if (lastFollowedConnection != default(MovementConnection) && lastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
			{
				base.bodyChunks[CarryBackwards ? 1 : 0].vel += Custom.DirVec(base.bodyChunks[CarryBackwards ? 1 : 0].pos, vector) * 4f;
			}
			if (Footing)
			{
				for (int l = 0; l < 2; l++)
				{
					if (followingConnection.startCoord.x == followingConnection.destinationCoord.x)
					{
						base.bodyChunks[l].vel.x += Mathf.Min((vector.x - base.bodyChunks[l].pos.x) / 8f, 1.2f);
					}
					else if (followingConnection.startCoord.y == followingConnection.destinationCoord.y)
					{
						base.bodyChunks[l].vel.y += Mathf.Min((vector.y - base.bodyChunks[l].pos.y) / 8f, 1.2f);
					}
				}
			}
			if (lastFollowedConnection != default(MovementConnection) && (Footing || room.aimap.TileAccessibleToCreature(base.bodyChunks[CarryBackwards ? 1 : 0].pos, base.Template)) && ((followingConnection.startCoord.x != followingConnection.destinationCoord.x && lastFollowedConnection.startCoord.x == lastFollowedConnection.destinationCoord.x) || (followingConnection.startCoord.y != followingConnection.destinationCoord.y && lastFollowedConnection.startCoord.y == lastFollowedConnection.destinationCoord.y)))
			{
				base.bodyChunks[CarryBackwards ? 1 : 0].vel *= 0.7f;
				base.bodyChunks[(!CarryBackwards) ? 1u : 0u].vel *= 0.5f;
			}
			if (followingConnection.type == MovementConnection.MovementType.DropToFloor && (!ModManager.MMF || base.mainBodyChunk.pos.y > 150f))
			{
				footingCounter = 0;
			}
			MoveTowards(vector);
		}
		lastFollowedConnection = followingConnection;
	}

	private void MoveTowards(Vector2 moveTo)
	{
		if (UnityEngine.Random.value > State.health)
		{
			return;
		}
		Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, moveTo);
		if (!Footing)
		{
			vector *= 0.3f;
		}
		if (!CarryBackwards && IsTileSolid(1, 0, -1) && (((double)vector.x < -0.5 && base.mainBodyChunk.pos.x > base.bodyChunks[1].pos.x + 5f) || ((double)vector.x > 0.5 && base.mainBodyChunk.pos.x < base.bodyChunks[1].pos.x - 5f)))
		{
			base.mainBodyChunk.vel.x -= ((vector.x < 0f) ? (-1f) : 1f) * 1.3f;
			base.bodyChunks[1].vel.x += ((vector.x < 0f) ? (-1f) : 1f) * 0.5f;
			if (!IsTileSolid(0, 0, 1))
			{
				base.mainBodyChunk.vel.y += 3.2f;
			}
		}
		float num = Custom.LerpMap(carryObjectMass, 0f, spitter ? 24f : 14f, 1f, 0.2f + 0.8f * stuckShake) * Mathf.Lerp(0.5f, 1f, runSpeed) * Mathf.Lerp((spitter && !CarryBackwards) ? 0.7f : 1.25f, 1.5f, stuckShake);
		if (CarryBackwards)
		{
			base.bodyChunks[1].vel += vector * 3.1f * num;
			base.mainBodyChunk.vel -= vector * 1.9f * num;
			base.GoThroughFloors = moveTo.y < base.bodyChunks[1].pos.y - 5f;
		}
		else
		{
			base.mainBodyChunk.vel += vector * 4.1f * num;
			base.bodyChunks[1].vel -= vector * 0.9f * num;
			base.GoThroughFloors = moveTo.y < base.mainBodyChunk.pos.y - 5f;
		}
	}

	public void BabyPuff()
	{
		if (base.inShortcut || base.slatedForDeletetion || room == null || room.world == null || room.game.cameras[0].room != room || !mother || spewBabies || base.dead)
		{
			return;
		}
		spewBabies = true;
		InsectCoordinator smallInsects = null;
		for (int i = 0; i < room.updateList.Count; i++)
		{
			if (room.updateList[i] is InsectCoordinator)
			{
				smallInsects = room.updateList[i] as InsectCoordinator;
				break;
			}
		}
		for (int j = 0; j < 70; j++)
		{
			SporeCloud sporeCloud = new SporeCloud(base.firstChunk.pos, Custom.RNV() * UnityEngine.Random.value * 10f, new Color(0.1f, 0.25f, 0.1f, 0.8f), 1f, null, j % 20, smallInsects);
			sporeCloud.nonToxic = true;
			room.AddObject(sporeCloud);
		}
		SporePuffVisionObscurer sporePuffVisionObscurer = new SporePuffVisionObscurer(base.firstChunk.pos);
		sporePuffVisionObscurer.doNotCallDeer = true;
		room.AddObject(sporePuffVisionObscurer);
		for (int k = 0; k < 7; k++)
		{
			room.AddObject(new PuffBallSkin(base.firstChunk.pos, Custom.RNV() * UnityEngine.Random.value * 16f, new Color(0.1f, 0.3f, 0.1f), new Color(0.1f, 0.1f, 0.3f)));
		}
		room.PlaySound(SoundID.Puffball_Eplode, base.firstChunk.pos);
		for (int l = 0; l < 25; l++)
		{
			Vector2 pos = base.mainBodyChunk.pos;
			AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Spider), null, room.GetWorldCoordinate(pos), room.world.game.GetNewID());
			room.abstractRoom.AddEntity(abstractCreature);
			abstractCreature.RealizeInRoom();
			(abstractCreature.realizedCreature as Spider).bloodLust = 1f;
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!base.Consious)
		{
			return;
		}
		if (otherObject is BigSpider && (!base.safariControlled || (inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp)))
		{
			AI.CollideWithKin(otherObject as BigSpider);
			if (!spitter && (otherObject as BigSpider).dead && (otherObject as BigSpider).CanIBeRevived && AI.behavior == BigSpiderAI.Behavior.ReviveBuddy && AI.reviveBuddy != null && (otherObject as Creature).abstractCreature == AI.reviveBuddy.representedCreature)
			{
				revivingBuddy = otherObject as BigSpider;
				IntVector2 intVector = new IntVector2(UnityEngine.Random.Range(0, 2), UnityEngine.Random.Range(0, UnityEngine.Random.Range(2, 4)));
				grabChunks[intVector.x, intVector.y] = otherObject.bodyChunks[otherChunk];
				room.PlaySound(SoundID.Big_Spider_Revive, base.mainBodyChunk);
			}
			else if (base.bodyChunks[myChunk].pos.y > otherObject.bodyChunks[otherChunk].pos.y)
			{
				base.bodyChunks[myChunk].vel.y += 2f;
				otherObject.bodyChunks[otherChunk].vel.y -= 2f;
			}
		}
		else
		{
			if (!(otherObject is Creature))
			{
				return;
			}
			AI.tracker.SeeCreature((otherObject as Creature).abstractCreature);
			for (int i = 0; i < AI.relationshipTracker.relationships.Count; i++)
			{
				if (AI.relationshipTracker.relationships[i].trackerRep.representedCreature == (otherObject as Creature).abstractCreature)
				{
					(AI.relationshipTracker.relationships[i].state as BigSpiderAI.SpiderTrackState).accustomed += 10;
					break;
				}
			}
			bool flag = false;
			bool consious = (otherObject as Creature).Consious;
			if (myChunk == 0 && base.grasps[0] == null)
			{
				bool flag2 = ((!spitter && mandiblesCharged > 0.8f && canBite > 0) || (spitter && UnityEngine.Random.value < 0.25f && !consious)) && Vector2.Dot(Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos), Custom.DirVec(base.mainBodyChunk.pos, otherObject.bodyChunks[otherChunk].pos)) > 0f && AI.DynamicRelationship((otherObject as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats;
				if (ModManager.MMF)
				{
					flag2 = flag2 && AI.preyTracker.TotalTrackedPrey > 0 && AI.preyTracker.Utility() > 0f && AI.preyTracker.MostAttractivePrey.representedCreature == (otherObject as Creature).abstractCreature;
				}
				if (base.safariControlled)
				{
					flag2 = inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp && Vector2.Dot(Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos), Custom.DirVec(base.mainBodyChunk.pos, otherObject.bodyChunks[otherChunk].pos)) > 0f && AI.DynamicRelationship((otherObject as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats;
				}
				if (flag2)
				{
					for (int j = 0; j < 4; j++)
					{
						room.AddObject(new WaterDrip(Vector2.Lerp(base.mainBodyChunk.pos, otherObject.bodyChunks[otherChunk].pos, UnityEngine.Random.value), Custom.RNV() * UnityEngine.Random.value * 14f, waterColor: false));
					}
					if (base.safariControlled || UnityEngine.Random.value < Custom.LerpMap(otherObject.TotalMass, 0.84f, spitter ? 5.5f : 3f, 0.5f, 0.15f, 0.12f) || !consious)
					{
						if (Grab(otherObject, 0, otherChunk, Grasp.Shareability.CanNotShare, 0.5f, overrideEquallyDominant: false, pacifying: true))
						{
							flag = true;
							room.PlaySound(SoundID.Big_Spider_Grab_Creature, base.mainBodyChunk);
						}
						else
						{
							room.PlaySound(SoundID.Big_Spider_Slash_Creature, base.mainBodyChunk);
						}
					}
					else
					{
						room.PlaySound(SoundID.Big_Spider_Slash_Creature, base.mainBodyChunk);
					}
					canBite = 0;
					(otherObject as Creature).Violence(base.mainBodyChunk, Custom.DirVec(base.mainBodyChunk.pos, otherObject.bodyChunks[otherChunk].pos) * (spitter ? 8f : 6f), otherObject.bodyChunks[otherChunk], null, DamageType.Bite, spitter ? 0.6f : ((UnityEngine.Random.value < 0.5f) ? 1.2f : 0.4f), spitter ? 20f : 60f);
					ReleaseAllGrabChunks();
				}
				else if (AI.StaticRelationship((otherObject as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats && ((otherObject as Creature).Template.CreatureRelationship(base.Template).type != CreatureTemplate.Relationship.Type.Eats || UnityEngine.Random.value < 0.1f) && LegsGrabby)
				{
					IntVector2 intVector2 = new IntVector2(UnityEngine.Random.Range(0, 2), UnityEngine.Random.Range(0, UnityEngine.Random.Range(2, 4)));
					grabChunks[intVector2.x, intVector2.y] = otherObject.bodyChunks[otherChunk];
					flag = true;
				}
			}
			if (!flag && revivingBuddy != null && jumpStamina > 0.2f && (AI.DynamicRelationship((otherObject as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats || AI.DynamicRelationship((otherObject as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Afraid))
			{
				Vector2 vector = (base.bodyChunks[0].pos + base.bodyChunks[1].pos) / 2f;
				Vector2 vector2 = Custom.DirVec(vector, otherObject.bodyChunks[otherChunk].pos);
				base.bodyChunks[0].pos = vector + vector2 * bodyChunkConnections[0].distance * 0.5f;
				base.bodyChunks[1].pos = vector - vector2 * bodyChunkConnections[0].distance * 0.5f;
				Jump(vector2, 0.8f);
				canBite = 40;
				mandiblesCharged = 1f;
				ReleaseAllGrabChunks();
				revivingBuddy = null;
				jumpStamina = Mathf.Max(0f, jumpStamina - 0.5f);
				flag = true;
				Custom.Log("revive fend off");
			}
			if (!(!flag && !base.safariControlled && (spitter || !(mandiblesCharged > 0.8f) || canBite <= 0) && (spitter || jumpStamina > 0.15f) && base.grasps[0] == null && consious) || !((otherObject as Creature).Template.CreatureRelationship(base.Template).intensity > 0f) || !((otherObject as Creature).TotalMass > base.TotalMass * 0.2f))
			{
				return;
			}
			for (int k = 0; k < grabChunks.GetLength(0); k++)
			{
				for (int l = 0; l < grabChunks.GetLength(1); l++)
				{
					if (grabChunks[k, l] != null && grabChunks[k, l].owner == otherObject)
					{
						return;
					}
				}
			}
			if (spitter)
			{
				base.mainBodyChunk.vel += Custom.DirVec(otherObject.bodyChunks[otherChunk].pos, base.mainBodyChunk.pos) * 8f;
				base.bodyChunks[1].vel += Custom.DirVec(otherObject.bodyChunks[otherChunk].pos, base.mainBodyChunk.pos) * 8f;
				otherObject.bodyChunks[otherChunk].vel -= Vector2.ClampMagnitude(Custom.DirVec(otherObject.bodyChunks[otherChunk].pos, base.mainBodyChunk.pos) * 8f * base.TotalMass / otherObject.bodyChunks[otherChunk].mass, 15f);
				room.PlaySound(SoundID.Big_Spider_Jump, base.mainBodyChunk, loop: false, 0.1f + 0.4f * bounceSoundVol, 1f);
				bounceSoundVol = Mathf.Max(0f, bounceSoundVol - 0.2f);
			}
			else
			{
				Jump(Custom.DirVec(otherObject.bodyChunks[otherChunk].pos, base.mainBodyChunk.pos + new Vector2(0f, 10f)), 0.1f + 0.4f * bounceSoundVol);
				bounceSoundVol = Mathf.Max(0f, bounceSoundVol - 0.2f);
				otherObject.bodyChunks[otherChunk].vel -= Vector2.ClampMagnitude(Custom.DirVec(otherObject.bodyChunks[otherChunk].pos, base.mainBodyChunk.pos + new Vector2(0f, 10f)) * 8f * base.TotalMass / otherObject.bodyChunks[otherChunk].mass, 15f);
				base.bodyChunks[0].pos.y += 20f;
				base.bodyChunks[1].pos.y += 10f;
				jumpStamina = Mathf.Max(0f, jumpStamina - 0.15f);
			}
			if ((otherObject as Creature).Consious && (otherObject as Creature).Template.CreatureRelationship(base.Template).type == CreatureTemplate.Relationship.Type.Eats)
			{
				AI.stayAway = true;
			}
		}
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		if (!base.dead && (damage > 0.1f || stunBonus > 20f))
		{
			if (UnityEngine.Random.value < Custom.LerpMap(damage, 0.3f, 3f, 0.4f, 0.9f))
			{
				room.PlaySound(SoundID.Big_Spider_Take_Damage, base.mainBodyChunk);
			}
			room.PlaySound(SoundID.Big_Spider_Death_Rustle, base.mainBodyChunk);
		}
		base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
	}

	public override void Die()
	{
		if (ModManager.MSC)
		{
			BabyPuff();
		}
		base.Die();
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

	public void FlyingWeapon(Weapon weapon)
	{
		if (base.Consious && !base.safariControlled && !spitter && !(jumpStamina < 0.3f) && !room.GetTile(room.GetTilePosition(base.mainBodyChunk.pos) + new IntVector2(0, 1)).Solid && !Custom.DistLess(base.mainBodyChunk.pos, weapon.thrownPos, 60f) && (room.GetTile(room.GetTilePosition(base.mainBodyChunk.pos) + new IntVector2(0, -1)).Solid || room.GetTile(room.GetTilePosition(base.mainBodyChunk.pos) + new IntVector2(0, -1)).Terrain == Room.Tile.TerrainType.Floor || room.GetTile(room.GetTilePosition(base.mainBodyChunk.pos)).AnyBeam) && base.grasps[0] == null && !(Vector2.Dot((base.bodyChunks[1].pos - base.bodyChunks[0].pos).normalized, (base.bodyChunks[0].pos - weapon.firstChunk.pos).normalized) < -0.2f) && AI.VisualContact(weapon.firstChunk.pos, 0.3f) && Custom.DistLess(weapon.firstChunk.pos + weapon.firstChunk.vel.normalized * 140f, base.mainBodyChunk.pos, 140f) && (Mathf.Abs(Custom.DistanceToLine(base.bodyChunks[0].pos, weapon.firstChunk.pos, weapon.firstChunk.pos + weapon.firstChunk.vel)) < 7f || Mathf.Abs(Custom.DistanceToLine(base.bodyChunks[1].pos, weapon.firstChunk.pos, weapon.firstChunk.pos + weapon.firstChunk.vel)) < 7f))
		{
			Jump(Custom.DirVec(base.mainBodyChunk.pos, weapon.thrownPos + new Vector2(0f, 400f)), 1f);
			base.bodyChunks[0].pos.y += 20f;
			base.bodyChunks[1].pos.y += 10f;
			jumpStamina = Mathf.Max(0f, jumpStamina - 0.15f);
		}
	}
}
