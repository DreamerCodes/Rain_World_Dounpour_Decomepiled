using System;
using RWCustom;
using Smoke;
using UnityEngine;

public class KingTusks
{
	public class Tusk : SharedPhysics.IProjectileTracer
	{
		public class Mode : ExtEnum<Mode>
		{
			public static readonly Mode Attached = new Mode("Attached", register: true);

			public static readonly Mode Charging = new Mode("Charging", register: true);

			public static readonly Mode ShootingOut = new Mode("ShootingOut", register: true);

			public static readonly Mode StuckInCreature = new Mode("StuckInCreature", register: true);

			public static readonly Mode StuckInWall = new Mode("StuckInWall", register: true);

			public static readonly Mode Dangling = new Mode("Dangling", register: true);

			public static readonly Mode Retracting = new Mode("Retracting", register: true);

			public Mode(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		private KingTusks owner;

		public int side;

		public static int TotalSprites = 3;

		private static int tuskSegs = 15;

		public Vector2 lastZRot;

		public Vector2 zRot;

		public Rope rope;

		public Vector2[,] chunkPoints;

		public static float length = 30f;

		public static float maxWireLength = 500f;

		public static float shootRange = 550f;

		public static float minShootRange = 250f;

		public float attached;

		public Vector2[,] wire;

		private float wireLoose;

		private float lastWireLoose;

		public Mode mode;

		private float currWireLength;

		private float wireExtraSlack;

		private float elasticity;

		public int modeCounter;

		public Vector2? stuckInWallPos;

		public Vector2 shootDir;

		private float stuck;

		private float laserAlpha;

		private float lastLaserAlpha;

		private float laserPower;

		private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		public BodyChunk impaleChunk;

		public Color armorColor;

		private Vulture vulture => owner.vulture;

		private Room room => owner.vulture.room;

		public BodyChunk head => owner.vulture.bodyChunks[4];

		public bool FullyAttached => attached == 1f;

		public bool StuckInAnything
		{
			get
			{
				if (!(mode == Mode.StuckInCreature))
				{
					return mode == Mode.StuckInWall;
				}
				return true;
			}
		}

		public bool StuckOrShooting
		{
			get
			{
				if (!StuckInAnything)
				{
					return mode == Mode.ShootingOut;
				}
				return true;
			}
		}

		public bool ReadyToShoot
		{
			get
			{
				if (mode == Mode.Attached)
				{
					return laserPower == 1f;
				}
				return false;
			}
		}

		public int FirstSprite(VultureGraphics vGraphics)
		{
			if (side == 0 == (Mathf.Sign(owner.HeadRotVector.x) == Mathf.Sign(owner.HeadRotVector.y)))
			{
				return vGraphics.FirstKingTuskSpriteFront;
			}
			return vGraphics.FirstKingTuskSpriteBehind;
		}

		public int LaserSprite(VultureGraphics vGraphics)
		{
			return FirstSprite(vGraphics);
		}

		public int TuskSprite(VultureGraphics vGraphics)
		{
			return FirstSprite(vGraphics) + 1;
		}

		public int TuskDetailSprite(VultureGraphics vGraphics)
		{
			return FirstSprite(vGraphics) + 2;
		}

		public float TuskBend(float f)
		{
			return Mathf.Sin(Mathf.Pow(f, 0.85f) * (float)Math.PI * 2f) * Mathf.Pow(1f - f, 2f);
		}

		public float TuskProfBend(float f)
		{
			return (0f - Mathf.Cos(Mathf.Pow(f, 0.85f) * (float)Math.PI * 2.5f)) * Mathf.Pow(1f - f, 3f);
		}

		public float TuskRad(float f, float profileFac)
		{
			return 0.5f + 2f * Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(f, Mathf.Lerp(0.65f, 0.5f, profileFac)) * (float)Math.PI)), 1.2f - 0.3f * profileFac);
		}

		public Vector2 AimDir(float timeStacker)
		{
			Vector2 vector = Custom.DirVec(Vector2.Lerp(vulture.neck.tChunks[vulture.neck.tChunks.Length - 1].lastPos, vulture.neck.tChunks[vulture.neck.tChunks.Length - 1].pos, timeStacker), Vector2.Lerp(head.lastPos, head.pos, timeStacker));
			float num = Mathf.InverseLerp(0f, 25f, (float)modeCounter + timeStacker);
			if (owner.lastEyesHome > 0f || owner.eyesHomeIn > 0f)
			{
				Vector3 vector2 = Custom.DirVec(Vector2.Lerp(head.lastPos, head.pos, timeStacker), Vector2.Lerp(owner.lastPreyPos, owner.preyPos, timeStacker));
				vector = Vector3.Slerp(vector, vector2, Mathf.Lerp(owner.lastEyesHome, owner.eyesHomeIn, timeStacker) * Mathf.Pow(Mathf.InverseLerp(0.2f, 0.85f - 0.2f * num, Vector2.Dot(vector, vector2)), 2f - 1.5f * num));
			}
			if (owner.lastEyesOut > 0f || owner.eyesOut > 0f)
			{
				vector += Custom.PerpendicularVector(vector) * Vector3.Slerp(Custom.DegToVec(owner.lastHeadRot + ((side == 0) ? (-90f) : 90f)), Custom.DegToVec(owner.headRot + ((side == 0) ? (-90f) : 90f)), timeStacker).x * Mathf.Lerp(owner.lastEyesOut, owner.eyesOut, timeStacker) * 0.5f;
			}
			return vector.normalized;
		}

		public Tusk(KingTusks owner, int side)
		{
			this.owner = owner;
			this.side = side;
			chunkPoints = new Vector2[2, 3];
			wire = new Vector2[20, 4];
			Reset(room);
		}

		public void Reset(Room newRoom)
		{
			attached = 1f;
			for (int i = 0; i < chunkPoints.GetLength(0); i++)
			{
				chunkPoints[i, 0] = owner.vulture.bodyChunks[4].pos + Custom.RNV();
				chunkPoints[i, 1] = chunkPoints[i, 0];
				chunkPoints[i, 2] *= 0f;
			}
			if (rope != null && rope.visualizer != null)
			{
				rope.visualizer.ClearSprites();
			}
			rope = null;
			for (int j = 0; j < wire.GetLength(0); j++)
			{
				wire[j, 0] = head.pos + Custom.RNV() * UnityEngine.Random.value;
				wire[j, 1] = wire[j, 0];
				wire[j, 2] *= 0f;
				wire[j, 3] *= 0f;
			}
			mode = Mode.Attached;
			modeCounter = 0;
			wireLoose = 0f;
			lastWireLoose = 0f;
			wireExtraSlack = 0f;
			elasticity = 0.9f;
		}

		public void SwitchMode(Mode newMode)
		{
			if (!(mode == newMode))
			{
				if (newMode != Mode.StuckInCreature)
				{
					impaleChunk = null;
				}
				modeCounter = 0;
				mode = newMode;
			}
		}

		public void Shoot(Vector2 tuskHangPos)
		{
			SwitchMode(Mode.ShootingOut);
			room.PlaySound(SoundID.King_Vulture_Tusk_Shoot, head);
			shootDir = AimDir(1f);
			owner.noShootDelay = 20;
			stuck = 0f;
			attached = 0f;
			currWireLength = maxWireLength;
			head.vel -= shootDir * 25f;
			head.pos -= shootDir * 25f;
			head.lastPos -= shootDir * 25f;
			for (int i = 0; i < vulture.neck.tChunks.Length; i++)
			{
				vulture.neck.tChunks[i].pos -= shootDir * 35f * Mathf.InverseLerp(0f, vulture.neck.tChunks.Length - 1, i);
				vulture.neck.tChunks[i].lastPos -= shootDir * 35f * Mathf.InverseLerp(0f, vulture.neck.tChunks.Length - 1, i);
				vulture.neck.tChunks[i].vel -= shootDir * 35f * Mathf.InverseLerp(0f, vulture.neck.tChunks.Length - 1, i);
			}
			vulture.bodyChunks[0].vel -= shootDir * 5f;
			wireExtraSlack = 1f;
			wireLoose = 1f;
			lastWireLoose = 1f;
			laserPower = 0f;
			laserAlpha = 0f;
			ShootUpdate(60f);
			if (rope != null && rope.visualizer != null)
			{
				rope.visualizer.ClearSprites();
			}
			rope = new Rope(room, head.pos, chunkPoints[1, 0], 1f);
			for (int j = 0; j < wire.GetLength(0); j++)
			{
				float num = Mathf.InverseLerp(0f, wire.GetLength(0), j);
				Vector2 vector = Custom.RNV() * (1f - num);
				wire[j, 1] = Vector2.Lerp(head.pos, chunkPoints[1, 0], num);
				wire[j, 0] = Vector2.Lerp(head.pos, chunkPoints[1, 0], num) + vector * 80f * UnityEngine.Random.value;
				wire[j, 2] = vector * 160f * UnityEngine.Random.value;
			}
			if (vulture.room.BeingViewed && !vulture.room.PointSubmerged(head.pos))
			{
				if (owner.smoke == null)
				{
					owner.smoke = new NewVultureSmoke(room, head.pos, owner.vulture);
					room.AddObject(owner.smoke);
				}
				Vector2 a = Custom.DirVec(head.pos, tuskHangPos);
				for (int k = 0; k < 8; k++)
				{
					float num2 = Mathf.InverseLerp(0f, 8f, k);
					owner.smoke.pos = (head.pos + chunkPoints[0, 0] + chunkPoints[1, 0]) / 3f;
					owner.smoke.EmitSmoke(Vector2.Lerp(a, -shootDir, num2) * Mathf.Lerp(15f, 60f, UnityEngine.Random.value * (1f - num2)) + Custom.RNV() * UnityEngine.Random.value * 60f, 1f);
				}
			}
		}

		public void ShootUpdate(float speed)
		{
			for (int i = 0; i < chunkPoints.GetLength(0); i++)
			{
				chunkPoints[i, 1] = chunkPoints[i, 0];
				chunkPoints[i, 2] *= 0f;
			}
			if (owner.smoke != null && modeCounter < 3)
			{
				owner.smoke.pos = chunkPoints[1, 0];
				owner.smoke.EmitSmoke(Custom.RNV() * UnityEngine.Random.value * 3f - shootDir * UnityEngine.Random.value * 3f, 1f);
			}
			float num = 20f;
			Vector2 vector = chunkPoints[0, 0] + shootDir * num;
			Vector2 vector2 = chunkPoints[0, 0] + shootDir * (num + speed);
			FloatRect? floatRect = SharedPhysics.ExactTerrainRayTrace(room, vector, vector2);
			Vector2 vector3 = default(Vector2);
			if (floatRect.HasValue)
			{
				vector3 = new Vector2(floatRect.Value.left, floatRect.Value.bottom);
			}
			Vector2 pos = vector2;
			SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(this, room, vector, ref pos, 5f, 1, owner.vulture, hitAppendages: false);
			if (floatRect.HasValue && collisionResult.chunk != null)
			{
				if (Vector2.Distance(vector, vector3) < Vector2.Distance(vector, collisionResult.collisionPoint))
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
				vector2 = vector3 - shootDir * num * 0.7f;
				if (room.BeingViewed)
				{
					for (int j = 0; j < 6; j++)
					{
						if (UnityEngine.Random.value < Mathf.InverseLerp(0f, 0.5f, room.roomSettings.CeilingDrips))
						{
							room.AddObject(new WaterDrip(vector3, -shootDir * 8f + Custom.RNV() * 8f * UnityEngine.Random.value, waterColor: false));
						}
					}
				}
				if (modeCounter > 0 && Vector2.Dot(shootDir, new Vector2(floatRect.Value.right, floatRect.Value.top)) > Custom.LerpMap(modeCounter, 1f, 8f, 0.65f, 0.95f))
				{
					stuckInWallPos = vector2;
					room.ScreenMovement(vector2, shootDir * 1.2f, 0.3f);
					SwitchMode(Mode.StuckInWall);
					room.PlaySound(SoundID.King_Vulture_Tusk_Stick_In_Terrain, vector2);
					stuck = 1f;
				}
				else
				{
					room.ScreenMovement(vector2, shootDir * 0.75f, 0.25f);
					if (floatRect.Value.right != 0f)
					{
						chunkPoints[0, 2].x = (Mathf.Abs(chunkPoints[0, 2].x) + 15f) * Mathf.Sign(floatRect.Value.right) * -1.5f;
					}
					if (floatRect.Value.top != 0f)
					{
						chunkPoints[0, 2].y = (Mathf.Abs(chunkPoints[0, 2].y) + 15f) * Mathf.Sign(floatRect.Value.top) * -1.5f;
					}
					Vector2 vector4 = Custom.RNV();
					chunkPoints[0, 2] += vector4 * 10f;
					chunkPoints[1, 2] -= vector4 * 10f;
					SwitchMode(Mode.Dangling);
					room.PlaySound(SoundID.King_Vulture_Tusk_Bounce_Off_Terrain, vector2);
				}
			}
			else if (collisionResult.chunk != null)
			{
				vector2 = collisionResult.collisionPoint - shootDir * num * 0.7f;
				chunkPoints[0, 0] = vector2 - shootDir * num;
				chunkPoints[1, 0] = vector2 - shootDir * (num + length);
				if (room.BeingViewed)
				{
					for (int k = 0; k < 6; k++)
					{
						if (UnityEngine.Random.value < Mathf.InverseLerp(0f, 0.5f, room.roomSettings.CeilingDrips))
						{
							room.AddObject(new WaterDrip(collisionResult.collisionPoint, -shootDir * Mathf.Lerp(5f, 15f, UnityEngine.Random.value) + Custom.RNV() * UnityEngine.Random.value * 10f, waterColor: false));
						}
					}
				}
				SwitchMode(Mode.StuckInCreature);
				room.PlaySound(SoundID.King_Vulture_Tusk_Impale_Creature, vector2);
				impaleChunk = collisionResult.chunk;
				impaleChunk.vel += shootDir * 12f / impaleChunk.mass;
				impaleChunk.vel = Vector2.ClampMagnitude(impaleChunk.vel, 50f);
				if (impaleChunk.owner is Creature)
				{
					(impaleChunk.owner as Creature).Violence(null, null, impaleChunk, null, Creature.DamageType.Stab, 1.5f, 0f);
				}
				shootDir = Vector3.Slerp(shootDir, Custom.DirVec(vector2, impaleChunk.pos), 0.4f);
				if (impaleChunk.rotationChunk != null)
				{
					shootDir = Custom.RotateAroundOrigo(shootDir, 0f - Custom.AimFromOneVectorToAnother(impaleChunk.pos, impaleChunk.rotationChunk.pos));
				}
				if (impaleChunk.owner.graphicsModule != null)
				{
					impaleChunk.owner.graphicsModule.BringSpritesToFront();
				}
				return;
			}
			chunkPoints[0, 0] = vector2 - shootDir * num;
			chunkPoints[1, 0] = vector2 - shootDir * (num + length);
			if (room.PointSubmerged(chunkPoints[0, 0]))
			{
				for (int l = 0; l < 8; l++)
				{
					Vector2 pos2 = Vector2.Lerp(vector, vector2, UnityEngine.Random.value);
					if (room.PointSubmerged(pos2))
					{
						room.AddObject(new Bubble(pos2, shootDir * UnityEngine.Random.value * 30f + Custom.RNV() * UnityEngine.Random.value * 15f, bottomBubble: false, fakeWaterBubble: false));
					}
				}
			}
			elasticity = 0.2f;
			if (!(mode == Mode.ShootingOut))
			{
				return;
			}
			float num2 = ((rope != null) ? rope.totalLength : Vector2.Distance(head.pos, chunkPoints[1, 0]));
			wireExtraSlack = Mathf.InverseLerp(shootRange * 0.8f, shootRange * 0.5f, num2);
			if (wireExtraSlack < 1f)
			{
				for (int m = 0; m < wire.GetLength(0); m++)
				{
					float num3 = Mathf.InverseLerp(0f, wire.GetLength(0), m);
					wire[m, 2] += (Vector2.Lerp(head.pos, chunkPoints[1, 0], num3) - wire[m, 0]) * Mathf.Pow(1f - wireExtraSlack, 3f) / 5f;
					wire[m, 0] += (Vector2.Lerp(head.pos, chunkPoints[1, 0], num3) - wire[m, 0]) * Mathf.Pow(1f - wireExtraSlack, 3f);
					if (num3 > 0.6f)
					{
						wire[m, 2] = Vector2.Lerp(wire[m, 2], Custom.DirVec(wire[m, 0], head.pos) * 10f, Mathf.InverseLerp(0.6f, 1f, num3) * (1f - wireExtraSlack));
					}
				}
			}
			if (num2 > shootRange)
			{
				SwitchMode(Mode.Dangling);
				room.PlaySound(SoundID.King_Vulture_Tusk_Wire_End, vector2, Custom.LerpMap(num2, shootRange, shootRange + 30f, 0.5f, 1f), 1f);
				head.pos += Custom.DirVec(head.pos, chunkPoints[1, 0]) * 10f;
				head.vel += Custom.DirVec(head.pos, chunkPoints[1, 0]) * 10f;
				chunkPoints[0, 2] = shootDir * speed * 0.4f;
				chunkPoints[1, 2] = shootDir * speed * 0.6f;
				Vector2 vector5 = Custom.RNV();
				chunkPoints[0, 0] += vector5 * 4f;
				chunkPoints[0, 2] += vector5 * 6f;
				chunkPoints[1, 0] -= vector5 * 4f;
				chunkPoints[1, 2] -= vector5 * 6f;
			}
		}

		public void Update()
		{
			lastZRot = zRot;
			lastWireLoose = wireLoose;
			lastLaserAlpha = laserAlpha;
			zRot = Vector3.Slerp(zRot, Custom.DegToVec(owner.headRot + ((side == 0) ? (-90f) : 90f)), 0.9f * attached);
			Vector2 vector = Custom.DirVec(vulture.neck.tChunks[vulture.neck.tChunks.Length - 1].pos, vulture.bodyChunks[4].pos);
			Vector2 vector2 = Custom.PerpendicularVector(vector);
			Vector2 vector3 = vulture.bodyChunks[4].pos + vector * -5f;
			vector3 += vector2 * zRot.x * 15f;
			vector3 += vector2 * zRot.y * ((side == 0) ? (-1f) : 1f) * 7f;
			laserPower = Custom.LerpAndTick(laserPower, attached, 0.01f, 1f / 120f);
			if (owner.tusks[1 - side].mode == Mode.Charging || !vulture.Consious)
			{
				laserAlpha = Mathf.Max(laserAlpha - 0.1f, 0f);
			}
			else if (UnityEngine.Random.value < 0.25f)
			{
				laserAlpha = ((UnityEngine.Random.value < laserPower) ? Mathf.Lerp(laserAlpha, Mathf.Pow(laserPower, 0.25f), Mathf.Pow(UnityEngine.Random.value, 0.5f)) : (laserAlpha * UnityEngine.Random.value * UnityEngine.Random.value));
			}
			modeCounter++;
			if (mode != Mode.ShootingOut)
			{
				wireExtraSlack = Mathf.Max(0f, wireExtraSlack - 1f / 30f);
				elasticity = Mathf.Min(0.9f, elasticity + 0.025f);
			}
			if (mode == Mode.Attached)
			{
				attached = 1f;
			}
			else if (mode == Mode.Charging)
			{
				attached = Custom.LerpMap(modeCounter, 0f, 25f, 0.2f, 1f);
				if (modeCounter > (owner.CloseQuarters ? 10 : 25))
				{
					if (vulture.Consious && (owner.targetRep != null || vulture.safariControlled) && owner.noShootDelay < 1 && (vulture.safariControlled || owner.GoodShootAngle(side, checkMinDistance: false) > (owner.CloseQuarters ? 0.6f : (owner.tusks[1 - side].ReadyToShoot ? 0.2f : 0.4f))) && (owner.VisualOnAnyTargetChunk() || vulture.safariControlled))
					{
						Shoot(vector3);
					}
					else
					{
						room.PlaySound(SoundID.King_Vulture_Tusk_Cancel_Shot, chunkPoints[0, 0]);
						SwitchMode(Mode.Attached);
						owner.noShootDelay = Mathf.Max(owner.noShootDelay, 10);
						Custom.Log("cancel shot");
					}
				}
				if (modeCounter % 6 == 0)
				{
					room.PlaySound(SoundID.King_Vulture_Tusk_Aim_Beep, chunkPoints[0, 0]);
				}
			}
			else if (mode == Mode.ShootingOut)
			{
				attached = 0f;
				currWireLength = maxWireLength;
				if (modeCounter > (room.PointSubmerged(chunkPoints[0, 0]) ? 6 : 10))
				{
					SwitchMode(Mode.Dangling);
					room.PlaySound(SoundID.King_Vulture_Tusk_Wire_End, chunkPoints[0, 0], 0.4f, 1f);
				}
			}
			else if (mode == Mode.Dangling)
			{
				attached = 0f;
				if (modeCounter > 80)
				{
					SwitchMode(Mode.Retracting);
				}
			}
			else if (mode == Mode.Retracting)
			{
				if (currWireLength > 0f)
				{
					currWireLength = Mathf.Max(0f, currWireLength - maxWireLength / 90f);
					attached = 0f;
				}
				else
				{
					float num = attached;
					if (attached < 1f)
					{
						attached = Mathf.Min(1f, attached + 0.05f);
					}
					else
					{
						SwitchMode(Mode.Attached);
					}
					if (num < 0.5f && attached >= 0.5f)
					{
						room.PlaySound(SoundID.King_Vulture_Tusk_Reattach, chunkPoints[0, 0]);
					}
				}
			}
			else if (mode == Mode.StuckInCreature)
			{
				attached = 0f;
				if (modeCounter > 80)
				{
					currWireLength = Mathf.Max(100f, currWireLength - maxWireLength / 180f);
				}
				if (impaleChunk == null)
				{
					SwitchMode(Mode.Dangling);
				}
			}
			else if (mode == Mode.StuckInWall)
			{
				attached = 0f;
				if (modeCounter > 240)
				{
					currWireLength = Mathf.Max(100f, currWireLength - maxWireLength / 180f);
				}
				if (!stuckInWallPos.HasValue || stuck <= 0f)
				{
					SwitchMode(Mode.Dangling);
				}
				else
				{
					for (int i = 0; i < chunkPoints.GetLength(0); i++)
					{
						chunkPoints[i, 1] = chunkPoints[i, 0];
						chunkPoints[i, 2] *= 0f;
					}
					chunkPoints[0, 0] = stuckInWallPos.Value;
					chunkPoints[1, 0] = stuckInWallPos.Value - shootDir * length;
					if (rope != null && rope.totalLength >= currWireLength)
					{
						chunkPoints[1, 0] += Custom.DirVec(chunkPoints[1, 0], head.pos) * UnityEngine.Random.value * 10f * (1f - stuck);
					}
				}
			}
			Vector2 vector4 = vector;
			if (mode == Mode.Charging)
			{
				vector4 = Vector3.Slerp(vector, AimDir(1f), Mathf.InverseLerp(0f, 25f, modeCounter));
			}
			if (!StuckOrShooting)
			{
				Vector2 vector6;
				for (int j = 0; j < chunkPoints.GetLength(0); j++)
				{
					chunkPoints[j, 1] = chunkPoints[j, 0];
					chunkPoints[j, 0] += chunkPoints[j, 2];
					if (room.PointSubmerged(chunkPoints[j, 0]))
					{
						chunkPoints[j, 2] *= 0.95f;
						chunkPoints[j, 2].y += 0.1f;
					}
					else
					{
						chunkPoints[j, 2] *= 0.98f;
						chunkPoints[j, 2].y -= 0.9f;
					}
					if (!FullyAttached && Custom.DistLess(chunkPoints[j, 0], chunkPoints[j, 1], 200f))
					{
						SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(chunkPoints[j, 0], chunkPoints[j, 1], chunkPoints[j, 2], 2f, new IntVector2(0, 0), goThroughFloors: true);
						cd = SharedPhysics.VerticalCollision(room, cd);
						cd = SharedPhysics.HorizontalCollision(room, cd);
						chunkPoints[j, 0] = cd.pos;
						chunkPoints[j, 2] = cd.vel;
						if ((float)cd.contactPoint.y != 0f)
						{
							chunkPoints[j, 2].x *= 0.5f;
						}
						if ((float)cd.contactPoint.x != 0f)
						{
							chunkPoints[j, 2].y *= 0.5f;
						}
					}
					if (attached > 0f)
					{
						Vector2 vector5 = vector3 + vector4 * length * ((j == 0) ? 0.5f : (-0.5f));
						float num2 = Mathf.Lerp(6f, 1f, attached);
						if (!Custom.DistLess(chunkPoints[j, 0], vector5, num2))
						{
							vector6 = Custom.DirVec(chunkPoints[j, 0], vector5) * (Vector2.Distance(chunkPoints[j, 0], vector5) - num2);
							chunkPoints[j, 0] += vector6;
							chunkPoints[j, 2] += vector6;
						}
					}
				}
				vector6 = Custom.DirVec(chunkPoints[0, 0], chunkPoints[1, 0]) * (Vector2.Distance(chunkPoints[0, 0], chunkPoints[1, 0]) - length);
				chunkPoints[0, 0] += vector6 / 2f;
				chunkPoints[0, 2] += vector6 / 2f;
				chunkPoints[1, 0] -= vector6 / 2f;
				chunkPoints[1, 2] -= vector6 / 2f;
			}
			wireLoose = Custom.LerpAndTick(wireLoose, (attached > 0f) ? 0f : 1f, 0.07f, 1f / 30f);
			if (lastWireLoose == 0f && wireLoose == 0f)
			{
				for (int k = 0; k < wire.GetLength(0); k++)
				{
					wire[k, 0] = head.pos + Custom.RNV();
					wire[k, 1] = wire[k, 0];
					wire[k, 0] *= 0f;
				}
			}
			else
			{
				float num3 = 1f;
				if (rope != null)
				{
					num3 = rope.totalLength / (float)wire.GetLength(0) * 0.5f;
				}
				num3 *= wireLoose;
				num3 += 10f * wireExtraSlack;
				float num4 = Mathf.InverseLerp(currWireLength * 0.75f, currWireLength, (rope != null) ? rope.totalLength : Vector2.Distance(head.pos, chunkPoints[1, 0]));
				num4 *= 1f - wireExtraSlack;
				for (int l = 0; l < wire.GetLength(0); l++)
				{
					wire[l, 1] = wire[l, 0];
					wire[l, 0] += wire[l, 2];
					if (room.PointSubmerged(wire[l, 0]))
					{
						wire[l, 2] *= 0.7f;
						wire[l, 2].y += 0.2f;
					}
					else
					{
						wire[l, 2] *= Mathf.Lerp(0.98f, 1f, wireExtraSlack);
						wire[l, 2].y -= 0.9f * (1f - wireExtraSlack);
					}
					if (rope != null)
					{
						Vector2 vector7 = OnRopePos(Mathf.InverseLerp(0f, wire.GetLength(0) - 1, l));
						wire[l, 2] += (vector7 - wire[l, 0]) * (1f - wireExtraSlack) / Mathf.Lerp(60f, 2f, num4);
						wire[l, 0] += (vector7 - wire[l, 0]) * (1f - wireExtraSlack) / Mathf.Lerp(60f, 2f, num4);
						wire[l, 0] = Vector2.Lerp(vector7, wire[l, 0], wireLoose);
						if (wire[l, 3].x == 0f && wireLoose == 1f && Custom.DistLess(wire[l, 0], wire[l, 1], 500f))
						{
							SharedPhysics.TerrainCollisionData cd2 = scratchTerrainCollisionData.Set(wire[l, 0], wire[l, 1], wire[l, 2], 3f, new IntVector2(0, 0), goThroughFloors: true);
							cd2 = SharedPhysics.VerticalCollision(room, cd2);
							cd2 = SharedPhysics.HorizontalCollision(room, cd2);
							wire[l, 0] = cd2.pos;
							wire[l, 2] = cd2.vel;
						}
					}
					wire[l, 3].x = 0f;
				}
				for (int m = 1; m < wire.GetLength(0); m++)
				{
					if (!Custom.DistLess(wire[m, 0], wire[m - 1, 0], num3))
					{
						Vector2 vector6 = Custom.DirVec(wire[m, 0], wire[m - 1, 0]) * (Vector2.Distance(wire[m, 0], wire[m - 1, 0]) - num3);
						wire[m, 0] += vector6 / 2f;
						wire[m, 2] += vector6 / 2f;
						wire[m - 1, 0] -= vector6 / 2f;
						wire[m - 1, 2] -= vector6 / 2f;
					}
				}
				if (rope != null && wireLoose == 1f)
				{
					AlignWireToRopeSim();
				}
				Vector2 pos = owner.vulture.neck.tChunks[owner.vulture.neck.tChunks.Length - 1].pos;
				pos += vector2 * zRot.x * 15f;
				pos += vector2 * zRot.y * ((side == 0) ? (-1f) : 1f) * 7f;
				if (!Custom.DistLess(wire[0, 0], pos, num3))
				{
					Vector2 vector6 = Custom.DirVec(wire[0, 0], pos) * (Vector2.Distance(wire[0, 0], pos) - num3);
					wire[0, 0] += vector6;
					wire[0, 2] += vector6;
				}
				pos = WireAttachPos(1f);
				if (!Custom.DistLess(wire[wire.GetLength(0) - 1, 0], pos, num3))
				{
					Vector2 vector6 = Custom.DirVec(wire[wire.GetLength(0) - 1, 0], pos) * (Vector2.Distance(wire[wire.GetLength(0) - 1, 0], pos) - num3);
					wire[wire.GetLength(0) - 1, 0] += vector6;
					wire[wire.GetLength(0) - 1, 2] += vector6;
				}
			}
			if (mode == Mode.ShootingOut)
			{
				ShootUpdate(Custom.LerpMap(modeCounter, 0f, 8f, 50f, 30f, 3f));
			}
			if (impaleChunk != null)
			{
				if (!(impaleChunk.owner is Creature) || mode != Mode.StuckInCreature || (impaleChunk.owner as Creature).enteringShortCut.HasValue || (impaleChunk.owner as Creature).room != room)
				{
					impaleChunk = null;
				}
				else if (vulture.Consious && modeCounter > 20 && UnityEngine.Random.value < Custom.LerpMap(modeCounter, 20f, 80f, 0.0016666667f, 1f / 30f) && !owner.DoIWantToHoldCreature(impaleChunk.owner as Creature))
				{
					if (vulture.grasps[0] != null && vulture.grasps[0].grabbed == impaleChunk.owner)
					{
						currWireLength = 0f;
						SwitchMode(Mode.Retracting);
					}
					else
					{
						SwitchMode(Mode.Dangling);
					}
					impaleChunk = null;
				}
				else
				{
					for (int n = 0; n < 2; n++)
					{
						chunkPoints[n, 1] = chunkPoints[n, 0];
						chunkPoints[n, 2] *= 0f;
					}
					Vector2 vec = shootDir;
					vec = ((impaleChunk.rotationChunk != null) ? Custom.RotateAroundOrigo(vec, Custom.AimFromOneVectorToAnother(impaleChunk.pos, impaleChunk.rotationChunk.pos)) : ((rope == null) ? Custom.DirVec(impaleChunk.pos, head.pos) : Custom.DirVec(impaleChunk.pos, rope.BConnect)));
					chunkPoints[0, 0] = impaleChunk.pos - vec * impaleChunk.rad;
					chunkPoints[1, 0] = impaleChunk.pos - vec * (impaleChunk.rad + length);
					if (vulture.AI.behavior == VultureAI.Behavior.Hunt && vulture.grasps[0] == null && vulture.AI.focusCreature != null && impaleChunk.owner is Creature && (impaleChunk.owner as Creature).abstractCreature == vulture.AI.focusCreature.representedCreature)
					{
						for (int num5 = 0; num5 < impaleChunk.owner.bodyChunks.Length; num5++)
						{
							if (Custom.DistLess(impaleChunk.owner.bodyChunks[num5].pos, vulture.bodyChunks[4].pos, impaleChunk.owner.bodyChunks[num5].rad + vulture.bodyChunks[4].rad))
							{
								Custom.Log("grab impaled");
								vulture.Grab(impaleChunk.owner, 0, num5, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, overrideEquallyDominant: true, pacifying: true);
								room.PlaySound(SoundID.Vulture_Grab_NPC, vulture.bodyChunks[4]);
								break;
							}
						}
					}
					if (UnityEngine.Random.value < 0.05f && impaleChunk.owner.grabbedBy.Count > 0)
					{
						for (int num6 = 0; num6 < impaleChunk.owner.grabbedBy.Count; num6++)
						{
							if (impaleChunk.owner.grabbedBy[num6].shareability != Creature.Grasp.Shareability.NonExclusive)
							{
								SwitchMode(Mode.Dangling);
								impaleChunk = null;
								break;
							}
						}
					}
				}
			}
			if (attached == 0f)
			{
				Vector2 vector8 = ((impaleChunk != null) ? impaleChunk.pos : chunkPoints[1, 0]);
				if (rope == null && room.VisualContact(head.pos, vector8))
				{
					rope = new Rope(room, head.pos, vector8, 1f);
				}
				if (rope != null)
				{
					rope.Update(head.pos, vector8);
					if (rope.totalLength > currWireLength)
					{
						wireExtraSlack = Mathf.Max(0f, wireExtraSlack - 0.1f);
						float num7 = stuck;
						stuck -= Mathf.InverseLerp(30f, 90f, modeCounter) * UnityEngine.Random.value / Custom.LerpMap(rope.totalLength / currWireLength, 1f, 1.3f, 120f, 10f, 0.7f);
						if (vulture.grasps[0] != null)
						{
							stuck -= 0.1f;
						}
						if (mode == Mode.StuckInWall && stuck <= 0f && num7 > 0f)
						{
							room.PlaySound(SoundID.King_Vulture_Tusk_Out_Of_Terrain, chunkPoints[0, 0], 1f, 1f);
						}
						float num8 = head.mass / (0.1f + head.mass);
						float num9 = rope.totalLength - currWireLength;
						if (mode == Mode.StuckInWall)
						{
							Vector2 vector6 = Custom.DirVec(head.pos, rope.AConnect) * num9;
							head.pos += vector6 * elasticity;
							head.vel += vector6 * elasticity;
						}
						else if (mode == Mode.StuckInCreature && impaleChunk != null)
						{
							num8 = head.mass / (impaleChunk.mass + head.mass);
							Vector2 vector6 = Custom.DirVec(head.pos, rope.AConnect) * num9;
							head.pos += vector6 * (1f - num8) * elasticity;
							head.vel += vector6 * (1f - num8) * elasticity;
							vector6 = Custom.DirVec(impaleChunk.pos, rope.BConnect) * num9;
							impaleChunk.pos += vector6 * num8 * elasticity;
							impaleChunk.vel += vector6 * num8 * elasticity;
						}
						else
						{
							Vector2 vector6 = Custom.DirVec(head.pos, rope.AConnect) * num9;
							head.pos += vector6 * (1f - num8) * elasticity;
							head.vel += vector6 * (1f - num8) * elasticity;
							vector6 = Custom.DirVec(chunkPoints[1, 0], rope.BConnect) * num9;
							chunkPoints[1, 0] += vector6 * num8 * elasticity;
							chunkPoints[1, 2] += vector6 * num8 * elasticity;
						}
					}
				}
				if (StuckInAnything && !Custom.DistLess(head.pos, vulture.bodyChunks[0].pos, vulture.neck.idealLength * 0.75f))
				{
					Vector2 vector6 = Custom.DirVec(head.pos, vulture.bodyChunks[0].pos) * (Vector2.Distance(head.pos, vulture.bodyChunks[0].pos) - vulture.neck.idealLength * 0.75f);
					float num10 = head.mass / (vulture.bodyChunks[0].mass + head.mass);
					head.pos += vector6 * (1f - num10);
					head.vel += vector6 * (1f - num10);
					vulture.bodyChunks[0].pos -= vector6 * num10;
					vulture.bodyChunks[0].vel -= vector6 * num10;
				}
				return;
			}
			if (rope != null)
			{
				if (rope.visualizer != null)
				{
					rope.visualizer.ClearSprites();
				}
				rope = null;
			}
			for (int num11 = 0; num11 < wire.GetLength(0); num11++)
			{
				wire[num11, 0] = head.pos + Custom.RNV();
			}
		}

		private Vector2 WireAttachPos(float timeStacker)
		{
			return Vector2.Lerp(chunkPoints[0, 1], chunkPoints[0, 0], timeStacker);
		}

		private void AlignWireToRopeSim()
		{
			if (rope.TotalPositions < 3)
			{
				return;
			}
			float totalLength = rope.totalLength;
			float num = 0f;
			for (int i = 0; i < rope.TotalPositions; i++)
			{
				if (i > 0)
				{
					num += Vector2.Distance(RopePos(i - 1), RopePos(i));
				}
				int num2 = Custom.IntClamp((int)(num / totalLength * (float)wire.GetLength(0)), 0, wire.GetLength(0) - 1);
				wire[num2, 1] = wire[num2, 0];
				wire[num2, 0] = RopePos(i);
				wire[num2, 2] *= 0f;
				wire[num2, 3].x = 1f;
			}
		}

		private Vector2 RopePos(int i)
		{
			if (i == rope.TotalPositions - 1)
			{
				return WireAttachPos(1f);
			}
			return rope.GetPosition(i);
		}

		private float RopeFloatAtSegment(int segment)
		{
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < rope.TotalPositions - 1; i++)
			{
				if (i < segment)
				{
					num2 += Vector2.Distance(RopePos(i), RopePos(i + 1));
				}
				num += Vector2.Distance(RopePos(i), RopePos(i + 1));
			}
			return num2 / num;
		}

		public int RopePrevSegAtFloat(float fPos)
		{
			fPos *= rope.totalLength;
			float num = 0f;
			for (int i = 0; i < rope.TotalPositions - 1; i++)
			{
				num += Vector2.Distance(RopePos(i), RopePos(i + 1));
				if (num > fPos)
				{
					return i;
				}
			}
			return rope.TotalPositions - 1;
		}

		public Vector2 OnRopePos(float fPos)
		{
			if (rope == null)
			{
				return head.pos;
			}
			int num = RopePrevSegAtFloat(fPos);
			int num2 = Custom.IntClamp(num + 1, 0, rope.TotalPositions - 1);
			float t = Mathf.InverseLerp(RopeFloatAtSegment(num), RopeFloatAtSegment(num2), fPos);
			return Vector2.Lerp(RopePos(num), RopePos(num2), t);
		}

		public void UpdateTuskColors(RoomCamera.SpriteLeaser sLeaser)
		{
			VultureGraphics vultureGraphics = vulture.graphicsModule as VultureGraphics;
			for (int i = 0; i < (sLeaser.sprites[TuskDetailSprite(vultureGraphics)] as TriangleMesh).verticeColors.Length; i++)
			{
				float num = Mathf.InverseLerp(0f, (sLeaser.sprites[TuskSprite(vultureGraphics)] as TriangleMesh).verticeColors.Length - 1, i);
				(sLeaser.sprites[TuskSprite(vultureGraphics)] as TriangleMesh).verticeColors[i] = Color.Lerp(Color.Lerp(armorColor, Color.white, Mathf.Pow(num, 2f)), vultureGraphics.palette.blackColor, vultureGraphics.darkness);
				(sLeaser.sprites[TuskDetailSprite(vultureGraphics)] as TriangleMesh).verticeColors[i] = Color.Lerp(Color.Lerp(Color.Lerp(HSLColor.Lerp(vultureGraphics.ColorA, vultureGraphics.ColorB, num).rgb, vultureGraphics.palette.blackColor, 0.65f - 0.4f * num), armorColor, Mathf.Pow(num, 2f)), vultureGraphics.palette.blackColor, vultureGraphics.darkness);
			}
		}

		public void InitiateSprites(VultureGraphics vGraphics, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[LaserSprite(vGraphics)] = new CustomFSprite("Futile_White");
			sLeaser.sprites[LaserSprite(vGraphics)].shader = rCam.game.rainWorld.Shaders["HologramBehindTerrain"];
			sLeaser.sprites[vGraphics.NeckLumpSprite(side)] = new FSprite("Circle20");
			sLeaser.sprites[vGraphics.NeckLumpSprite(side)].anchorY = 0f;
			sLeaser.sprites[TuskSprite(vGraphics)] = TriangleMesh.MakeLongMesh(tuskSegs, pointyTip: true, customColor: true);
			sLeaser.sprites[TuskDetailSprite(vGraphics)] = TriangleMesh.MakeLongMesh(tuskSegs, pointyTip: true, customColor: true);
			sLeaser.sprites[TuskDetailSprite(vGraphics)].shader = rCam.game.rainWorld.Shaders["KingTusk"];
			sLeaser.sprites[vGraphics.TuskWireSprite(side)] = TriangleMesh.MakeLongMesh(wire.GetLength(0), pointyTip: false, customColor: true);
		}

		public void DrawSprites(VultureGraphics vGraphics, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (vGraphics.shadowMode)
			{
				camPos.y -= rCam.room.PixelHeight + 300f;
			}
			if (ModManager.MMF)
			{
				UpdateTuskColors(sLeaser);
			}
			Vector2 vector = Vector2.Lerp(vulture.bodyChunks[4].lastPos, vulture.bodyChunks[4].pos, timeStacker);
			Vector2 vector2 = Custom.DirVec(Vector2.Lerp(vulture.neck.tChunks[vulture.neck.tChunks.Length - 1].lastPos, vulture.neck.tChunks[vulture.neck.tChunks.Length - 1].pos, timeStacker), Vector2.Lerp(vulture.bodyChunks[4].lastPos, vulture.bodyChunks[4].pos, timeStacker));
			Vector2 vector3 = Custom.PerpendicularVector(vector2);
			Vector2 vector4 = Vector3.Slerp(lastZRot, zRot, timeStacker);
			float num = Mathf.Lerp(lastLaserAlpha, laserAlpha, timeStacker);
			Color color = Custom.HSL2RGB(vGraphics.ColorB.hue, 1f, 0.5f);
			if (mode == Mode.Charging)
			{
				num = ((modeCounter % 6 < 3) ? 1f : 0f);
				if (modeCounter % 2 == 0)
				{
					color = Color.Lerp(color, Color.white, UnityEngine.Random.value);
				}
			}
			Vector2 vector5 = vector + vector2 * 15f + vector3 * Vector3.Slerp(Custom.DegToVec(owner.lastHeadRot + ((side == 0) ? (-90f) : 90f)), Custom.DegToVec(owner.headRot + ((side == 0) ? (-90f) : 90f)), timeStacker).x * 7f;
			Vector2 vector6 = AimDir(timeStacker);
			if (num <= 0f)
			{
				sLeaser.sprites[LaserSprite(vGraphics)].isVisible = false;
			}
			else
			{
				sLeaser.sprites[LaserSprite(vGraphics)].isVisible = true;
				sLeaser.sprites[LaserSprite(vGraphics)].alpha = num;
				Vector2 corner = Custom.RectCollision(vector5, vector5 + vector6 * 100000f, rCam.room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
				IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(rCam.room, vector5, corner);
				if (intVector.HasValue)
				{
					corner = Custom.RectCollision(corner, vector5, rCam.room.TileRect(intVector.Value)).GetCorner(FloatRect.CornerLabel.D);
				}
				(sLeaser.sprites[LaserSprite(vGraphics)] as CustomFSprite).verticeColors[0] = Custom.RGB2RGBA(color, num);
				(sLeaser.sprites[LaserSprite(vGraphics)] as CustomFSprite).verticeColors[1] = Custom.RGB2RGBA(color, num);
				(sLeaser.sprites[LaserSprite(vGraphics)] as CustomFSprite).verticeColors[2] = Custom.RGB2RGBA(color, Mathf.Pow(num, 2f) * ((mode == Mode.Charging) ? 1f : 0.5f));
				(sLeaser.sprites[LaserSprite(vGraphics)] as CustomFSprite).verticeColors[3] = Custom.RGB2RGBA(color, Mathf.Pow(num, 2f) * ((mode == Mode.Charging) ? 1f : 0.5f));
				(sLeaser.sprites[LaserSprite(vGraphics)] as CustomFSprite).MoveVertice(0, vector5 + vector6 * 2f + Custom.PerpendicularVector(vector6) * 0.5f - camPos);
				(sLeaser.sprites[LaserSprite(vGraphics)] as CustomFSprite).MoveVertice(1, vector5 + vector6 * 2f - Custom.PerpendicularVector(vector6) * 0.5f - camPos);
				(sLeaser.sprites[LaserSprite(vGraphics)] as CustomFSprite).MoveVertice(2, corner - Custom.PerpendicularVector(vector6) * 0.5f - camPos);
				(sLeaser.sprites[LaserSprite(vGraphics)] as CustomFSprite).MoveVertice(3, corner + Custom.PerpendicularVector(vector6) * 0.5f - camPos);
			}
			Vector2 vector7 = (Vector2.Lerp(chunkPoints[0, 1], chunkPoints[0, 0], timeStacker) + Vector2.Lerp(chunkPoints[1, 1], chunkPoints[1, 0], timeStacker)) / 2f;
			Vector2 vector8 = Custom.DirVec(Vector2.Lerp(chunkPoints[1, 1], chunkPoints[1, 0], timeStacker), Vector2.Lerp(chunkPoints[0, 1], chunkPoints[0, 0], timeStacker));
			Vector2 vector9 = Custom.PerpendicularVector(vector8);
			if (mode == Mode.Charging)
			{
				vector7 += vector8 * Mathf.Lerp(-6f, 6f, UnityEngine.Random.value);
			}
			Vector2 vector10 = vector - vector8 * 10f;
			Vector2 vector11 = Vector2.Lerp(vector, vector7, Mathf.InverseLerp(0f, 0.25f, attached));
			sLeaser.sprites[vGraphics.NeckLumpSprite(side)].x = vector10.x - camPos.x;
			sLeaser.sprites[vGraphics.NeckLumpSprite(side)].y = vector10.y - camPos.y;
			sLeaser.sprites[vGraphics.NeckLumpSprite(side)].scaleY = (Vector2.Distance(vector10, vector11) + 4f) / 20f;
			sLeaser.sprites[vGraphics.NeckLumpSprite(side)].rotation = Custom.AimFromOneVectorToAnother(vector10, vector11);
			sLeaser.sprites[vGraphics.NeckLumpSprite(side)].scaleX = 0.6f;
			Vector2 vector12 = vector7 + vector8 * -35f + vector9 * vector4.y * ((side == 0) ? (-1f) : 1f) * -15f;
			float num2 = 0f;
			for (int i = 0; i < tuskSegs; i++)
			{
				float num3 = Mathf.InverseLerp(0f, tuskSegs - 1, i);
				Vector2 vector13 = vector7 + vector8 * Mathf.Lerp(-30f, 60f, num3) + TuskBend(num3) * vector9 * 20f * vector4.x + TuskProfBend(num3) * vector9 * vector4.y * ((side == 0) ? (-1f) : 1f) * 10f;
				Vector2 normalized = (vector13 - vector12).normalized;
				Vector2 vector14 = Custom.PerpendicularVector(normalized);
				float num4 = Vector2.Distance(vector13, vector12) / 5f;
				float num5 = TuskRad(num3, Mathf.Abs(vector4.y));
				(sLeaser.sprites[TuskSprite(vGraphics)] as TriangleMesh).MoveVertice(i * 4, vector12 - vector14 * (num5 + num2) * 0.5f + normalized * num4 - camPos);
				(sLeaser.sprites[TuskSprite(vGraphics)] as TriangleMesh).MoveVertice(i * 4 + 1, vector12 + vector14 * (num5 + num2) * 0.5f + normalized * num4 - camPos);
				if (i == tuskSegs - 1)
				{
					(sLeaser.sprites[TuskSprite(vGraphics)] as TriangleMesh).MoveVertice(i * 4 + 2, vector13 + normalized * num4 - camPos);
				}
				else
				{
					(sLeaser.sprites[TuskSprite(vGraphics)] as TriangleMesh).MoveVertice(i * 4 + 2, vector13 - vector14 * num5 - normalized * num4 - camPos);
					(sLeaser.sprites[TuskSprite(vGraphics)] as TriangleMesh).MoveVertice(i * 4 + 3, vector13 + vector14 * num5 - normalized * num4 - camPos);
				}
				num2 = num5;
				vector12 = vector13;
			}
			for (int j = 0; j < (sLeaser.sprites[TuskSprite(vGraphics)] as TriangleMesh).vertices.Length; j++)
			{
				(sLeaser.sprites[TuskDetailSprite(vGraphics)] as TriangleMesh).MoveVertice(j, (sLeaser.sprites[TuskSprite(vGraphics)] as TriangleMesh).vertices[j]);
			}
			if (lastWireLoose > 0f || wireLoose > 0f)
			{
				sLeaser.sprites[vGraphics.TuskWireSprite(side)].isVisible = true;
				float num6 = Mathf.Lerp(lastWireLoose, wireLoose, timeStacker);
				vector12 = vector - vector2 * 14f;
				for (int k = 0; k < wire.GetLength(0); k++)
				{
					Vector2 vector15 = Vector2.Lerp(wire[k, 1], wire[k, 0], timeStacker);
					if (num6 < 1f)
					{
						vector15 = Vector2.Lerp(Vector2.Lerp(vector - vector2 * 14f, vector7 + vector8 * 6f, Mathf.InverseLerp(0f, wire.GetLength(0) - 1, k)), vector15, num6);
					}
					if (k == wire.GetLength(0) - 1)
					{
						vector15 = WireAttachPos(timeStacker);
					}
					Vector2 normalized2 = (vector15 - vector12).normalized;
					Vector2 vector16 = Custom.PerpendicularVector(normalized2);
					float num7 = Vector2.Distance(vector15, vector12) / 5f;
					if (k == wire.GetLength(0) - 1)
					{
						num7 = 0f;
					}
					(sLeaser.sprites[vGraphics.TuskWireSprite(side)] as TriangleMesh).MoveVertice(k * 4, vector12 - vector16 + normalized2 * num7 - camPos);
					(sLeaser.sprites[vGraphics.TuskWireSprite(side)] as TriangleMesh).MoveVertice(k * 4 + 1, vector12 + vector16 + normalized2 * num7 - camPos);
					(sLeaser.sprites[vGraphics.TuskWireSprite(side)] as TriangleMesh).MoveVertice(k * 4 + 2, vector15 - vector16 - normalized2 * num7 - camPos);
					(sLeaser.sprites[vGraphics.TuskWireSprite(side)] as TriangleMesh).MoveVertice(k * 4 + 3, vector15 + vector16 - normalized2 * num7 - camPos);
					vector12 = vector15;
				}
			}
			else
			{
				sLeaser.sprites[vGraphics.TuskWireSprite(side)].isVisible = false;
			}
		}

		public void AddToContainer(VultureGraphics vGraphics, int spr, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (spr == LaserSprite(vGraphics))
			{
				rCam.ReturnFContainer(ModManager.MMF ? "Midground" : "Foreground").AddChild(sLeaser.sprites[spr]);
			}
			else if (spr == vGraphics.TuskWireSprite(side) || spr == TuskSprite(vGraphics) || spr == TuskDetailSprite(vGraphics))
			{
				rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[spr]);
			}
		}

		public void ApplyPalette(VultureGraphics vGraphics, RoomPalette palette, Color armorColor, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			this.armorColor = armorColor;
			for (int i = 0; i < (sLeaser.sprites[TuskDetailSprite(vGraphics)] as TriangleMesh).verticeColors.Length; i++)
			{
				float num = Mathf.InverseLerp(0f, (sLeaser.sprites[TuskSprite(vGraphics)] as TriangleMesh).verticeColors.Length - 1, i);
				(sLeaser.sprites[TuskSprite(vGraphics)] as TriangleMesh).verticeColors[i] = Color.Lerp(armorColor, Color.white, Mathf.Pow(num, 2f));
				(sLeaser.sprites[TuskDetailSprite(vGraphics)] as TriangleMesh).verticeColors[i] = Color.Lerp(Color.Lerp(HSLColor.Lerp(vGraphics.ColorA, vGraphics.ColorB, num).rgb, palette.blackColor, 0.65f - 0.4f * num), armorColor, Mathf.Pow(num, 2f));
			}
			(sLeaser.sprites[TuskDetailSprite(vGraphics)] as TriangleMesh).alpha = owner.patternDisplace;
			for (int j = 0; j < (sLeaser.sprites[vGraphics.TuskWireSprite(side)] as TriangleMesh).verticeColors.Length; j++)
			{
				(sLeaser.sprites[vGraphics.TuskWireSprite(side)] as TriangleMesh).verticeColors[j] = Color.Lerp(palette.blackColor, palette.fogColor, 0.33f * Mathf.Sin(Mathf.InverseLerp(0f, (sLeaser.sprites[vGraphics.TuskWireSprite(side)] as TriangleMesh).verticeColors.Length - 1, j) * (float)Math.PI));
			}
		}

		public bool HitThisObject(PhysicalObject obj)
		{
			if (obj != vulture)
			{
				return obj is Creature;
			}
			return false;
		}

		public bool HitThisChunk(BodyChunk chunk)
		{
			return true;
		}
	}

	private Vulture vulture;

	public Tusk[] tusks;

	private float headRot;

	private float lastHeadRot;

	public float eyesOut;

	public float lastEyesOut;

	public float eyesOutCycle;

	public float eyesHomeIn;

	public float lastEyesHome;

	private Vector2 preyPos;

	private Vector2 lastPreyPos;

	private Vector2 preyVelEstimate;

	private NewVultureSmoke smoke;

	public int noShootDelay;

	public float patternDisplace;

	public Vector2[] targetTrail;

	public Tracker.CreatureRepresentation targetRep;

	private Vector2 HeadRotVector => Custom.DegToVec(headRot);

	public bool ReadyToShoot
	{
		get
		{
			if (!tusks[0].ReadyToShoot)
			{
				return tusks[1].ReadyToShoot;
			}
			return true;
		}
	}

	public bool AnyCreatureImpaled
	{
		get
		{
			if (tusks[0].impaleChunk == null)
			{
				return tusks[1].impaleChunk != null;
			}
			return true;
		}
	}

	public bool CloseQuarters
	{
		get
		{
			if (vulture.room.aimap.getTerrainProximity(preyPos) != 1 || vulture.room.aimap.getTerrainProximity(vulture.bodyChunks[4].pos) >= 2)
			{
				return vulture.room.aimap.getAItile(vulture.bodyChunks[4].pos).narrowSpace;
			}
			return true;
		}
	}

	public bool ThisCreatureImpaled(AbstractCreature crit)
	{
		for (int i = 0; i < tusks.Length; i++)
		{
			if (tusks[i].impaleChunk != null && tusks[i].impaleChunk.owner is Creature && (tusks[i].impaleChunk.owner as Creature).abstractCreature == crit)
			{
				return true;
			}
		}
		return false;
	}

	public KingTusks(Vulture vulture)
	{
		this.vulture = vulture;
		tusks = new Tusk[2];
		for (int i = 0; i < tusks.Length; i++)
		{
			tusks[i] = new Tusk(this, i);
		}
		targetTrail = new Vector2[15];
	}

	public void NewRoom(Room newRoom)
	{
		for (int i = 0; i < tusks.Length; i++)
		{
			tusks[i].Reset(newRoom);
		}
		lastPreyPos = vulture.mainBodyChunk.pos;
		preyPos = vulture.mainBodyChunk.pos;
		smoke = null;
		for (int j = 0; j < targetTrail.Length; j++)
		{
			targetTrail[j] = vulture.mainBodyChunk.pos;
		}
		preyVelEstimate *= 0f;
		noShootDelay = 220;
	}

	public void Update()
	{
		if (vulture.room == null)
		{
			return;
		}
		lastEyesOut = eyesOut;
		eyesOutCycle += 1f / 15f;
		eyesOut = (0.5f + 0.5f * Mathf.Sin(eyesOutCycle)) * (1f - eyesHomeIn);
		lastEyesHome = eyesHomeIn;
		lastPreyPos = preyPos;
		if (noShootDelay > 0)
		{
			noShootDelay--;
		}
		lastHeadRot = headRot;
		headRot = Custom.AimFromOneVectorToAnother(vulture.neck.tChunks[vulture.neck.tChunks.Length - 1].pos, vulture.bodyChunks[4].pos);
		headRot -= Custom.AimFromOneVectorToAnother(vulture.bodyChunks[0].pos, vulture.bodyChunks[1].pos);
		for (int i = 0; i < tusks.Length; i++)
		{
			tusks[i].Update();
		}
		if (smoke != null)
		{
			smoke.WindDrag(vulture.mainBodyChunk.pos, vulture.mainBodyChunk.vel, 30f);
			if (smoke.Dead || vulture.room != smoke.room)
			{
				smoke = null;
			}
		}
		if (vulture.AI.behavior == VultureAI.Behavior.Hunt && vulture.Consious)
		{
			if (vulture.AI.preyTracker.MostAttractivePrey != null && (vulture.AI.preyTracker.MostAttractivePrey.VisualContact || vulture.room.VisualContact(preyPos, vulture.bodyChunks[4].pos)))
			{
				if (targetRep == vulture.AI.preyTracker.MostAttractivePrey)
				{
					eyesHomeIn = Mathf.Min(1f, eyesHomeIn + Mathf.InverseLerp(0.25f, 0.9f, Vector2.Dot((vulture.neck.tChunks[vulture.neck.tChunks.Length - 1].pos - vulture.bodyChunks[4].pos).normalized, (vulture.bodyChunks[4].pos - preyPos).normalized)) / 40f);
				}
				else
				{
					eyesHomeIn = Mathf.Max(0f, eyesHomeIn - 0.2f);
					if (eyesHomeIn == 0f && lastEyesHome == 0f)
					{
						targetRep = vulture.AI.preyTracker.MostAttractivePrey;
					}
				}
			}
			else
			{
				eyesHomeIn = Mathf.Max(0f, eyesHomeIn - Custom.LerpMap((vulture.AI.preyTracker.MostAttractivePrey != null) ? vulture.AI.preyTracker.MostAttractivePrey.TicksSinceSeen : 120, 60f, 120f, 0f, 0.01f));
			}
		}
		else
		{
			eyesHomeIn = Custom.LerpAndTick(eyesHomeIn, 0f, 0.07f, 0.025f);
		}
		if (targetRep != null && targetRep.BestGuessForPosition().room == vulture.room.abstractRoom.index)
		{
			Vector2 vector;
			if (targetRep.VisualContact)
			{
				vector = targetRep.representedCreature.realizedCreature.mainBodyChunk.pos;
				preyVelEstimate = Custom.MoveTowards(preyVelEstimate, targetRep.representedCreature.realizedCreature.mainBodyChunk.pos - targetRep.representedCreature.realizedCreature.mainBodyChunk.lastPos, 0.1f);
			}
			else
			{
				vector = vulture.room.MiddleOfTile(targetRep.BestGuessForPosition());
				preyVelEstimate *= 0.99f;
			}
			for (int num = targetTrail.Length - 1; num > 0; num--)
			{
				targetTrail[num] = targetTrail[num - 1];
			}
			targetTrail[0] = vector;
			preyPos = targetTrail[targetTrail.Length - 1] + preyVelEstimate * targetTrail.Length * Custom.LerpMap(Vector2.Distance(targetTrail[targetTrail.Length - 1], vulture.bodyChunks[4].pos), 100f, 500f, 1f, 1.5f);
			if (eyesHomeIn == 1f && !vulture.safariControlled && noShootDelay < 1 && vulture.Consious && Custom.DistLess(preyPos, vulture.bodyChunks[4].pos, Tusk.shootRange) && !vulture.ChargingSnap && tusks[0].mode != Tusk.Mode.Charging && tusks[1].mode != Tusk.Mode.Charging && WantToShoot(checkVisualOnAnyTargetChunk: true, !CloseQuarters) && GoodShootAngle(-1, !CloseQuarters) > (CloseQuarters ? 0.3f : 0.7f))
			{
				TryToShoot();
			}
		}
		else
		{
			targetRep = null;
		}
		if (vulture.safariControlled && vulture.inputWithDiagonals.HasValue && vulture.inputWithDiagonals.Value.thrw && !vulture.lastInputWithDiagonals.Value.thrw && vulture.grasps[0] == null)
		{
			TryToShoot();
		}
	}

	public void InitiateSprites(VultureGraphics vGraphics, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int i = 0; i < tusks.Length; i++)
		{
			tusks[i].InitiateSprites(vGraphics, sLeaser, rCam);
		}
	}

	public void DrawSprites(VultureGraphics vGraphics, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < tusks.Length; i++)
		{
			tusks[i].DrawSprites(vGraphics, sLeaser, rCam, timeStacker, camPos);
		}
	}

	public void ApplyPalette(VultureGraphics vGraphics, RoomPalette palette, Color armorColor, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int i = 0; i < tusks.Length; i++)
		{
			tusks[i].ApplyPalette(vGraphics, palette, armorColor, sLeaser, rCam);
		}
	}

	public void AddToContainer(VultureGraphics vGraphics, int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		for (int i = 0; i < tusks.Length; i++)
		{
			tusks[i].AddToContainer(vGraphics, sprite, sLeaser, rCam, newContatiner);
		}
	}

	public bool DoIWantToHoldCreature(Creature creature)
	{
		if (vulture.grasps[0] != null)
		{
			return false;
		}
		if (vulture.AI.behavior != VultureAI.Behavior.Hunt && vulture.AI.behavior != VultureAI.Behavior.Idle)
		{
			return false;
		}
		return vulture.AI.DynamicRelationship(creature.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats;
	}

	public bool VisualOnAnyTargetChunk()
	{
		if (targetRep == null || targetRep.representedCreature.realizedCreature == null)
		{
			return false;
		}
		if (targetRep.VisualContact)
		{
			return true;
		}
		for (int i = 0; i < targetRep.representedCreature.realizedCreature.bodyChunks.Length; i++)
		{
			if (vulture.room.VisualContact(vulture.bodyChunks[4].pos, targetRep.representedCreature.realizedCreature.bodyChunks[i].pos))
			{
				return true;
			}
		}
		return false;
	}

	public bool HitBySpear(Vector2 directionAndMomentum)
	{
		int num = UnityEngine.Random.Range(0, 2);
		if (tusks[num].mode != Tusk.Mode.Attached && tusks[num].mode != Tusk.Mode.Charging)
		{
			return false;
		}
		tusks[num].SwitchMode(Tusk.Mode.Dangling);
		tusks[num].chunkPoints[UnityEngine.Random.Range(0, 2), 2] += directionAndMomentum * 3.3f;
		return true;
	}

	public bool WantToShoot(bool checkVisualOnAnyTargetChunk, bool checkMinDistance)
	{
		if (vulture.grasps[0] != null)
		{
			return false;
		}
		if (vulture.AI.behavior != VultureAI.Behavior.Hunt && vulture.AI.behavior != VultureAI.Behavior.Idle)
		{
			return false;
		}
		if (targetRep == null || targetRep.representedCreature.realizedCreature == null)
		{
			return false;
		}
		for (int i = 0; i < tusks.Length; i++)
		{
			if (tusks[i].impaleChunk != null && tusks[i].impaleChunk.owner == targetRep.representedCreature.realizedCreature)
			{
				return false;
			}
		}
		if (checkMinDistance && (Custom.DistLess(preyPos, vulture.bodyChunks[4].pos, Tusk.minShootRange) || Custom.DistLess(preyPos, vulture.bodyChunks[0].pos, Tusk.minShootRange)))
		{
			for (int j = 0; j < 9; j++)
			{
				if (vulture.AI.pathFinder.CoordinateReachable(targetRep.BestGuessForPosition() + Custom.eightDirectionsAndZero[j]))
				{
					return false;
				}
			}
		}
		if (checkVisualOnAnyTargetChunk && !VisualOnAnyTargetChunk())
		{
			return false;
		}
		return vulture.AI.DynamicRelationship(targetRep.representedCreature).type == CreatureTemplate.Relationship.Type.Eats;
	}

	public void TryToShoot()
	{
		int num = UnityEngine.Random.Range(0, 2);
		if (!tusks[num].ReadyToShoot)
		{
			num = 1 - num;
		}
		if (tusks[num].ReadyToShoot)
		{
			tusks[num].SwitchMode(Tusk.Mode.Charging);
			vulture.room.PlaySound(SoundID.King_Vulture_Tusk_Aim, vulture.bodyChunks[4]);
			if (!Custom.DistLess(vulture.bodyChunks[1].lastPos, vulture.bodyChunks[1].pos, 5f))
			{
				vulture.AirBrake(15);
			}
		}
	}

	public float GoodShootAngle(int tusk, bool checkMinDistance)
	{
		if (targetRep == null || targetRep.TicksSinceSeen > 20)
		{
			return 0f;
		}
		Vector2 lhs = ((tusk != -1) ? tusks[tusk].AimDir(1f) : ((Vector2)Vector3.Slerp(tusks[0].AimDir(1f), tusks[1].AimDir(1f), 0.5f)));
		float num = Mathf.InverseLerp(0.8f, 1f, Vector2.Dot(lhs, Custom.DirVec(vulture.bodyChunks[4].pos, preyPos)));
		num *= Mathf.InverseLerp(Tusk.shootRange, Tusk.shootRange * 0.8f, Vector2.Distance(vulture.bodyChunks[4].pos, preyPos));
		if (num == 0f)
		{
			return 0f;
		}
		if (checkMinDistance)
		{
			for (int i = 0; i < 9; i++)
			{
				if (vulture.AI.pathFinder.CoordinateReachable(targetRep.BestGuessForPosition() + Custom.eightDirectionsAndZero[i]))
				{
					num *= Mathf.InverseLerp(Tusk.minShootRange, Mathf.Lerp(Tusk.minShootRange, Tusk.shootRange, 0.3f), Vector2.Distance(vulture.bodyChunks[4].pos, preyPos));
					break;
				}
			}
		}
		return num;
	}
}
