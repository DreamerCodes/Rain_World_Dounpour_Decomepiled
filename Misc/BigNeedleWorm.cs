using System;
using RWCustom;
using UnityEngine;

public class BigNeedleWorm : NeedleWorm, Weapon.INotifyOfFlyingWeapons, SharedPhysics.IProjectileTracer
{
	public float attackReady;

	public float chargingAttack;

	public Vector2? swishDir;

	public int swishCounter;

	public float fangLength = 50f;

	public Vector2? stuckInWallPos;

	public Vector2 stuckDir;

	public float stuckTime;

	public BodyChunk impaleChunk;

	public float[,] impaleDistances;

	public int lameCounter;

	public Vector2? swishAdd;

	private int dodgeDelay;

	public Vector2 controlledCharge;

	public bool attackRefresh;

	private Vector2 FangPos => base.mainBodyChunk.pos + Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos) * fangLength;

	public bool NormalFlyingState
	{
		get
		{
			if (base.Consious && lameCounter < 1 && flying > 0.5f && chargingAttack < 0.5f && !swishDir.HasValue && !stuckInWallPos.HasValue)
			{
				return impaleChunk == null;
			}
			return false;
		}
	}

	public BigNeedleWormAI BigAI => AI as BigNeedleWormAI;

	public BigNeedleWorm(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		impaleDistances = new float[base.bodyChunks.Length, 2];
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null || enteringShortCut.HasValue)
		{
			stuckInWallPos = null;
			return;
		}
		if (dodgeDelay > 0)
		{
			dodgeDelay--;
		}
		if (lameCounter > 0)
		{
			lameCounter--;
		}
		else
		{
			flyingThisFrame = false;
		}
		if (impaleChunk != null)
		{
			StuckInChunk();
		}
		else if (stuckInWallPos.HasValue)
		{
			StuckInWall();
		}
		else if (lameCounter < 1)
		{
			if (swishCounter > 0)
			{
				Swish();
			}
			else if (base.Consious)
			{
				if (base.safariControlled)
				{
					if (!inputWithDiagonals.HasValue || !inputWithDiagonals.Value.pckp)
					{
						attackRefresh = false;
					}
					if (inputWithDiagonals.HasValue)
					{
						if (inputWithDiagonals.Value.pckp && !lastInputWithDiagonals.Value.pckp && attackReady < 0.05f)
						{
							attackReady = 0.05f;
							controlledCharge = Vector2.zero;
						}
						if (inputWithDiagonals.Value.pckp && attackReady > 0f && !attackRefresh)
						{
							attackReady = Custom.LerpAndTick(attackReady, 1f, 0f, 0.0375f);
							if (chargingAttack < 0.1f)
							{
								chargingAttack = 0.1f;
							}
						}
						if (inputWithDiagonals.Value.jmp && !lastInputWithDiagonals.Value.jmp)
						{
							if (inputWithDiagonals.Value.y != 0)
							{
								BigCry();
							}
							else
							{
								SmallCry();
							}
						}
					}
					if (chargingAttack > 0f)
					{
						AttackCharge();
					}
				}
				Act();
			}
			else
			{
				attackReady = Mathf.Max(0f, attackReady - 1f / 60f);
				chargingAttack = 0f;
			}
		}
		if (swishAdd.HasValue)
		{
			for (int i = 0; i < tail.GetLength(0); i++)
			{
				tail[i, 0] += swishAdd.Value * Mathf.Pow(Mathf.InverseLerp(tail.GetLength(0) - 1, 0f, i), 0.4f);
			}
		}
		swishAdd = null;
		AfterUpdate();
		if (impaleChunk != null)
		{
			AttachToChunk(rot: false);
		}
		if (!(attackReady > 0.8f) || !(UnityEngine.Random.value < 1f / 3f) || swishDir.HasValue || stuckInWallPos.HasValue || impaleChunk != null || !(Vector2.Dot(Custom.DirVec(base.mainBodyChunk.lastPos, base.mainBodyChunk.pos), Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos)) > -0.2f))
		{
			return;
		}
		for (int j = 0; j < room.physicalObjects[1].Count; j++)
		{
			if (!(room.physicalObjects[1][j] is Creature))
			{
				continue;
			}
			for (int k = 0; k < room.physicalObjects[1][j].bodyChunks.Length; k++)
			{
				if (Custom.DistLess(room.physicalObjects[1][j].bodyChunks[k].pos, FangPos, room.physicalObjects[1][j].bodyChunks[k].rad))
				{
					(room.physicalObjects[1][j] as Creature).Violence(base.mainBodyChunk, Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos) * 1.6f, room.physicalObjects[1][j].bodyChunks[k], null, DamageType.Stab, 0.05f, 30f);
					base.mainBodyChunk.vel -= Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos) * 1.6f / base.mainBodyChunk.mass;
				}
			}
		}
	}

	public void SmallCry()
	{
		if (!(attackReady > 0.7f) && !(screaming > 0.1f))
		{
			screaming = 0.5f;
			room.PlaySound(SoundID.Big_Needle_Worm_Small_Trumpet, base.mainBodyChunk);
		}
	}

	public void BigCry()
	{
		if (!(attackReady > 0.8f))
		{
			screaming = 1f;
			room.PlaySound(SoundID.Big_Needle_Worm_Big_Trumpet, base.mainBodyChunk);
		}
	}

	public override void Fly(MovementConnection followingConnection)
	{
		base.Fly(followingConnection);
		if (Mathf.Abs(lookDir.x) > 0.65f && lookDir.y < 0.2f && lookDir.x < 0f != base.bodyChunks[0].pos.x < tail[0, 0].x && NormalFlyingState)
		{
			base.bodyChunks[0].vel.x += Mathf.Sign(lookDir.x) * 5f;
			base.bodyChunks[0].pos.x += Mathf.Sign(lookDir.x);
			base.bodyChunks[base.bodyChunks.Length - 1].vel.x -= Mathf.Sign(lookDir.x) * 5f;
			base.bodyChunks[base.bodyChunks.Length - 1].pos.x -= Mathf.Sign(lookDir.x);
			for (int i = 0; i < tail.GetLength(0); i++)
			{
				tail[i, 0].x -= Mathf.Sign(lookDir.x);
				tail[i, 2].x -= Mathf.Sign(lookDir.x) * 6f / (float)(1 + i);
			}
		}
		if (BigAI.attackCounter > 65 && !base.safariControlled)
		{
			attackReady = Custom.LerpAndTick(attackReady, Mathf.InverseLerp(65f, 85f, BigAI.attackCounter), 0f, 1f / 160f);
		}
		else if (!base.safariControlled || !inputWithDiagonals.HasValue || !inputWithDiagonals.Value.pckp)
		{
			attackReady = Mathf.Max(0f, attackReady - 1f / 60f);
		}
		if (!base.safariControlled)
		{
			if (BigAI.behavior == NeedleWormAI.Behavior.Attack && AI.preyTracker.MostAttractivePrey != null && AI.preyTracker.MostAttractivePrey.TicksSinceSeen < 20 && attackReady > 0.5f)
			{
				AttackCharge();
			}
			else
			{
				chargingAttack = 0f;
			}
		}
		if (BigAI.focusCreature == null || !(BigAI.focusCreature.representedCreature.creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm) || !base.Consious || lameCounter >= 1 || !(flying > 0.5f) || !(chargingAttack < 0.9f) || swishDir.HasValue || stuckInWallPos.HasValue || impaleChunk != null || !BigAI.focusCreature.VisualContact || BigAI.focusCreature.representedCreature.realizedCreature == null)
		{
			return;
		}
		BigNeedleWorm bigNeedleWorm = BigAI.focusCreature.representedCreature.realizedCreature as BigNeedleWorm;
		if (bigNeedleWorm.swishDir.HasValue || bigNeedleWorm.chargingAttack > 0.9f)
		{
			Vector2 vector = (bigNeedleWorm.swishDir.HasValue ? bigNeedleWorm.swishDir.Value : Custom.DirVec(bigNeedleWorm.bodyChunks[1].pos, bigNeedleWorm.bodyChunks[0].pos));
			if (Custom.DistLess(base.mainBodyChunk.pos, bigNeedleWorm.mainBodyChunk.pos + vector * 140f, 140f) && Dodge(bigNeedleWorm.mainBodyChunk.pos, vector))
			{
				chargingAttack = 0f;
				attackReady = Mathf.Min(0.75f, attackReady);
				lameCounter = 14;
			}
		}
	}

	public void StuckInChunk()
	{
		if (impaleChunk == null || base.mainBodyChunk.pos.y < -100f)
		{
			impaleChunk = null;
			return;
		}
		AttachToChunk(rot: true);
		AttachToChunk(rot: false);
	}

	private void AttachToChunk(bool rot)
	{
		if (impaleChunk == null)
		{
			return;
		}
		stuckTime += 0.0035714286f;
		float num = Custom.LerpMap(stuckTime, 0.4f, 1f, 1f, 0.5f);
		attackReady = 1f;
		flyingThisFrame = stuckTime > 0.5f && base.Consious;
		for (int i = 0; i < impaleDistances.GetLength(0); i++)
		{
			float num2 = Custom.LerpMap(i, 1f, base.bodyChunks.Length - 1, 1f, 0.1f) * num;
			Vector2 vector = Custom.DirVec(base.bodyChunks[i].pos, impaleChunk.pos) * (Vector2.Distance(base.bodyChunks[i].pos, impaleChunk.pos) - impaleDistances[i, 0]);
			float num3 = impaleChunk.mass / (base.bodyChunks[i].mass + impaleChunk.mass);
			base.bodyChunks[i].vel += vector * num3 * num2;
			base.bodyChunks[i].pos += vector * num3 * num2;
			impaleChunk.vel -= vector * (1f - num3) * num2;
			impaleChunk.pos -= vector * (1f - num3) * num2;
			if (rot && impaleChunk.rotationChunk != null)
			{
				vector = Custom.DirVec(base.bodyChunks[i].pos, impaleChunk.rotationChunk.pos) * (Vector2.Distance(base.bodyChunks[i].pos, impaleChunk.rotationChunk.pos) - impaleDistances[i, 1]);
				num3 = impaleChunk.rotationChunk.mass / (base.bodyChunks[i].mass + impaleChunk.rotationChunk.mass);
				base.bodyChunks[i].vel += vector * num3 * num2;
				base.bodyChunks[i].pos += vector * num3 * num2;
				impaleChunk.rotationChunk.vel -= vector * (1f - num3) * num2;
				impaleChunk.rotationChunk.pos -= vector * (1f - num3) * num2;
			}
		}
		if (base.Consious)
		{
			for (int j = 1; j < base.TotalSegments; j++)
			{
				AddSegmentVel(j, stuckTime * Custom.RNV() * UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value * 18f);
			}
			crawlSin += 0.2f + 0.4f * num;
			SinMovementInBody(2.5f * (small ? 0.6f : 1f) * stuckTime, 2.5f * (small ? 0.6f : 1f) * stuckTime, 0.6f, 0.6f);
		}
		if (stuckTime > 1f || (stuckTime > 0.2f && (grabbedBy.Count > 0 || !(impaleChunk.owner is Creature) || !room.VisualContact(base.mainBodyChunk.pos, impaleChunk.pos))))
		{
			stuckTime = 0f;
			base.mainBodyChunk.vel += Custom.DirVec(impaleChunk.pos, base.mainBodyChunk.pos) * 9f;
			base.bodyChunks[1].vel += Custom.DirVec(impaleChunk.pos, base.mainBodyChunk.pos) * 7f;
			impaleChunk = null;
		}
	}

	public void StuckInWall()
	{
		if (!stuckInWallPos.HasValue)
		{
			return;
		}
		attackReady = 1f;
		flyingThisFrame = stuckTime > 0.5f && base.Consious;
		float num = Custom.LerpMap(stuckTime, 0.3f, 1f, 1f, 0.5f);
		float num2 = 0f;
		for (int i = 0; i < base.TotalSegments / 2; i++)
		{
			float num3 = Mathf.InverseLerp(0f, base.TotalSegments / 2 - 1, i);
			if (i > 0)
			{
				num2 += GetSegmentRadForRopeLength(i - 1) + GetSegmentRadForRopeLength(i);
			}
			SetSegmentVel(i, GetSegmentVel(i) * Mathf.Lerp(1f, num3, num * 0.9f));
			SetSegmentPos(i, Vector2.Lerp(GetSegmentPos(i), stuckInWallPos.Value - stuckDir * num2, (1f - num3) * num * 0.9f));
		}
		if (base.Consious)
		{
			stuckTime += 0.0125f;
			for (int j = 1; j < base.TotalSegments; j++)
			{
				AddSegmentVel(j, stuckDir * Mathf.Lerp(1f, -7f, UnityEngine.Random.value * UnityEngine.Random.value) + Custom.RNV() * UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value * 18f);
			}
			crawlSin += 0.2f + 0.4f * num;
			SinMovementInBody(2.5f * (small ? 0.6f : 1f) * stuckTime, 2.5f * (small ? 0.6f : 1f) * stuckTime, 0.6f, 0.6f);
		}
		else
		{
			stuckTime += UnityEngine.Random.value / 150f;
		}
		stuckAtSamePos = 0f;
		brokenLineOfSight = 0f;
		segmentsStuckOnTerrain = 0f;
		reallyStuckAtSamePos = 0f;
		if (stuckTime > 1f || grabbedBy.Count > 0)
		{
			if (base.Consious)
			{
				base.mainBodyChunk.vel -= stuckDir * 17f * flying;
				base.bodyChunks[1].vel -= stuckDir * 19f * flying;
			}
			stuckTime = 0f;
			stuckInWallPos = null;
			lameCounter = 7;
		}
	}

	public void Swish()
	{
		flyingThisFrame = false;
		flying = 0f;
		lookDir = swishDir.Value;
		dodgeDelay = 30;
		swishCounter--;
		if (swishCounter < 1 || !swishDir.HasValue)
		{
			for (int i = 0; i < base.TotalSegments; i++)
			{
				SetSegmentVel(i, Vector2.ClampMagnitude(GetSegmentVel(i) * 0.75f, 20f));
			}
			swishCounter = 0;
			swishDir = null;
			lameCounter = 7;
			return;
		}
		float num = 90f + 90f * Mathf.Sin(Mathf.InverseLerp(1f, 5f, swishCounter) * (float)Math.PI);
		attackReady = 1f;
		Vector2 value = swishDir.Value;
		Vector2 vector = base.bodyChunks[0].pos + value * fangLength;
		Vector2 vector2 = base.bodyChunks[0].pos + value * (fangLength + num);
		Vector2 vector3 = base.bodyChunks[0].lastPos + value * fangLength;
		Vector2 vector4 = base.bodyChunks[0].pos + value * (fangLength + num);
		FloatRect? floatRect = SharedPhysics.ExactTerrainRayTrace(room, vector3, vector4);
		Vector2 vector5 = default(Vector2);
		if (floatRect.HasValue)
		{
			vector5 = new Vector2(floatRect.Value.left, floatRect.Value.bottom);
		}
		Vector2 pos = vector4;
		SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(this, room, vector3, ref pos, 1f, 1, this, hitAppendages: false);
		if (floatRect.HasValue && collisionResult.chunk != null)
		{
			if (Vector2.Distance(vector3, vector5) < Vector2.Distance(vector3, collisionResult.collisionPoint))
			{
				collisionResult.chunk = null;
			}
			else
			{
				floatRect = null;
			}
		}
		if (floatRect.HasValue)
		{
			vector2 = vector5 - value * fangLength * 0.7f;
			for (int j = 0; j < 6; j++)
			{
				room.AddObject(new WaterDrip(vector5, -value * 8f + Custom.RNV() * 8f * UnityEngine.Random.value, waterColor: false));
			}
			if (Vector2.Dot(value, new Vector2(floatRect.Value.right, floatRect.Value.top)) > 0.73f)
			{
				stuckInWallPos = vector2;
				stuckDir = swishDir.Value;
				swishCounter = 0;
				swishDir = null;
				room.ScreenMovement(vector2, stuckDir * 1.2f, 0.3f);
				Stun(60);
				stuckTime = 0f;
				room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, vector2);
			}
			else
			{
				swishCounter = 0;
				swishDir = null;
				room.ScreenMovement(vector2, stuckDir * 0.75f, 0.25f);
				lameCounter = 30;
				room.PlaySound(SoundID.Big_Needle_Worm_Bounce_Terrain, vector2);
			}
		}
		else if (collisionResult.chunk != null)
		{
			vector2 = collisionResult.collisionPoint - value * fangLength * 0.7f;
			stuckDir = Vector3.Slerp(swishDir.Value, Custom.DirVec(vector2, collisionResult.chunk.pos), 0.4f);
			swishCounter = 0;
			swishDir = null;
			impaleChunk = collisionResult.chunk;
			float num2 = (0f - fangLength) / 4f;
			for (int k = 0; k < impaleDistances.GetLength(0); k++)
			{
				if (k == 1)
				{
					num2 += fangLength / 4f;
				}
				if (k > 0)
				{
					num2 += GetSegmentRadForRopeLength(k - 1) + GetSegmentRadForRopeLength(k) + fangLength / 4f;
				}
				impaleDistances[k, 0] = Vector2.Distance(vector2 - value * num2, impaleChunk.pos);
				if (impaleChunk.rotationChunk != null)
				{
					impaleDistances[k, 1] = Vector2.Distance(vector2 - value * num2, impaleChunk.rotationChunk.pos);
				}
			}
			if (impaleChunk.owner is Creature)
			{
				(impaleChunk.owner as Creature).Violence(base.mainBodyChunk, null, impaleChunk, null, DamageType.Stab, 1.22f, 60f);
			}
			impaleChunk.vel += value * 12f / impaleChunk.mass;
			impaleChunk.pos += value * 7f / impaleChunk.mass;
			for (int l = 0; l < base.TotalSegments; l++)
			{
				SetSegmentVel(l, Vector2.ClampMagnitude(GetSegmentVel(l), 6f));
			}
			room.PlaySound(SoundID.Big_Needle_Worm_Impale_Creature, vector2);
			stuckTime = 0f;
		}
		swishAdd = vector2 - vector;
		for (int m = 0; m < base.TotalSegments; m++)
		{
			float t = Mathf.InverseLerp(0f, base.TotalSegments - 1, m);
			if (m < base.bodyChunks.Length)
			{
				SetSegmentPos(m, GetSegmentPos(m) + (vector2 - vector));
			}
			AddSegmentVel(m, value * Mathf.Lerp(6f, -0.2f, t));
		}
	}

	private void AttackCharge()
	{
		Vector2 p = base.bodyChunks[1].pos;
		Vector2 pos = base.bodyChunks[1].pos;
		if (base.safariControlled)
		{
			if (controlledCharge == Vector2.zero)
			{
				controlledCharge = Custom.RNV() * 80f;
			}
			if (inputWithDiagonals.HasValue && inputWithDiagonals.Value.AnyDirectionalInput)
			{
				controlledCharge = new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 80f;
			}
			Creature creature = null;
			float num = float.MaxValue;
			float current = Custom.VecToDeg(controlledCharge);
			for (int i = 0; i < base.abstractCreature.Room.creatures.Count; i++)
			{
				if (base.abstractCreature != base.abstractCreature.Room.creatures[i] && base.abstractCreature.Room.creatures[i].realizedCreature != null)
				{
					float target = Custom.AimFromOneVectorToAnother(base.bodyChunks[1].pos, base.abstractCreature.Room.creatures[i].realizedCreature.mainBodyChunk.pos);
					float num2 = Custom.Dist(base.bodyChunks[1].pos, base.abstractCreature.Room.creatures[i].realizedCreature.mainBodyChunk.pos);
					if (Mathf.Abs(Mathf.DeltaAngle(current, target)) < 35f && num2 < num)
					{
						num = num2;
						creature = base.abstractCreature.Room.creatures[i].realizedCreature;
					}
				}
			}
			if (creature != null)
			{
				controlledCharge = Custom.DirVec(base.bodyChunks[1].pos, creature.mainBodyChunk.pos) * 80f;
			}
			pos = base.bodyChunks[1].pos + controlledCharge;
		}
		else
		{
			p = BigAI.attackFromPos;
			pos = BigAI.attackTargetPos;
		}
		float num3 = Mathf.InverseLerp(0.5f, 0.95f, attackReady);
		Vector2 vector = Custom.DirVec(p, pos);
		float num4 = Mathf.InverseLerp(0.2f, 0.9f, Vector2.Dot(vector, Custom.DirVec(GetSegmentPos(base.TotalSegments / 2), base.mainBodyChunk.pos)));
		num4 *= Mathf.InverseLerp(20f, 50f, Vector2.Distance(BigAI.attackTargetPos, base.mainBodyChunk.pos));
		if (base.safariControlled)
		{
			if (chargingAttack > 0f && num3 == 1f)
			{
				chargingAttack = Mathf.Min(1f, chargingAttack + 1f / 55f);
			}
			else
			{
				chargingAttack = Mathf.Max(0f, chargingAttack - 1f / 30f);
			}
		}
		else if (chargingAttack > 0f || (Custom.DistLess(base.bodyChunks[1].pos, BigAI.attackFromPos, 40f) && num4 > 0.5f))
		{
			if (num3 == 1f)
			{
				chargingAttack = Mathf.Min(1f, chargingAttack + (0.1f + 0.9f * num4) / 55f);
			}
			else
			{
				chargingAttack = Mathf.Max(0f, chargingAttack - 1f / 30f);
			}
		}
		else
		{
			chargingAttack = Mathf.Max(0f, chargingAttack - 1f / 30f);
		}
		float num5 = 1f;
		float num6 = 1.9f;
		if (base.safariControlled)
		{
			num5 = 0f;
			num6 = 0f;
			num3 = 1f;
			SetSegmentVel(0, vector * 5f);
			for (int j = 1; j < base.TotalSegments; j++)
			{
				SetSegmentVel(j, -vector * 4f);
			}
		}
		Vector2 vector2 = Vector2.ClampMagnitude(BigAI.attackFromPos - vector * chargingAttack * 100f - base.bodyChunks[1].pos, 40f) / 40f * num6 * num3;
		for (int k = 1; k < base.TotalSegments; k++)
		{
			float num7 = Mathf.InverseLerp(0f, base.TotalSegments - 1, k);
			SetSegmentVel(k, GetSegmentVel(k) * Mathf.Lerp(1f, 0.75f, num3 * Mathf.InverseLerp(0.25f, 0.75f, num7)));
			AddSegmentVel(k, vector2 * (1f - num7) + vector * Mathf.Lerp(3f, -6f, num7) * num3 * num5 * Mathf.InverseLerp(0.75f + 0.6f * chargingAttack, 0.5f, num7));
		}
		if (chargingAttack > 0.5f)
		{
			crawlSin += 0.8f * chargingAttack;
			SinMovementInBody(0f, 1.5f * (small ? 0.6f : 1f) * Mathf.InverseLerp(0.5f, 0.75f, chargingAttack), 0.4f, 0.4f);
		}
		if (chargingAttack >= 1f && dodgeDelay < 1 && room.VisualContact(base.mainBodyChunk.pos, FangPos))
		{
			chargingAttack = 0f;
			swishDir = vector;
			swishCounter = 6;
			room.PlaySound(SoundID.Big_Needle_Worm_Attack, base.mainBodyChunk.pos);
			attackRefresh = true;
		}
	}

	public void FlyingWeapon(Weapon weapon)
	{
		if (NormalFlyingState && AI.VisualContact(weapon.firstChunk.pos, 1f))
		{
			Vector2 vector = base.bodyChunks[1].pos - (weapon.firstChunk.pos + weapon.firstChunk.vel.normalized * 200f);
			vector.y *= 2f;
			if (!(vector.magnitude > 200f))
			{
				Dodge(weapon.firstChunk.pos, weapon.firstChunk.vel.normalized);
			}
		}
	}

	public bool Dodge(Vector2 projPos, Vector2 dir)
	{
		if (dodgeDelay > 0 || flying == 0f)
		{
			return false;
		}
		dodgeDelay = 10;
		chargingAttack /= 2f;
		Vector2 vector = Custom.PerpendicularVector(dir) * Mathf.Sign(Custom.DistanceToLine(base.bodyChunks[1].pos, projPos, projPos + dir));
		float dst = float.MinValue;
		int num = -1;
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			if (!Custom.DistLess(projPos, base.bodyChunks[i].pos, dst))
			{
				dst = Vector2.Distance(projPos, base.bodyChunks[i].pos);
				num = i;
			}
		}
		if (num > -1)
		{
			base.bodyChunks[num].vel += vector * 12f * flying;
		}
		for (int j = 0; j < base.TotalSegments; j++)
		{
			float num2 = Mathf.InverseLerp(0f, base.TotalSegments - 1, j);
			AddSegmentVel(j, vector * 12f * (1f - num2) * flying);
		}
		lookDir = Custom.DirVec(base.mainBodyChunk.pos + vector * 20f, projPos);
		return true;
	}

	public bool HitThisObject(PhysicalObject obj)
	{
		if (obj is Creature)
		{
			return (obj as Creature).Template.type != CreatureTemplate.Type.SmallNeedleWorm;
		}
		return false;
	}

	public bool HitThisChunk(BodyChunk chunk)
	{
		return true;
	}
}
