using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using ScavengerCosmetic;
using Unity.Mathematics;
using UnityEngine;

public class ScavengerGraphics : ComplexGraphicsModule
{
	public struct IndividualVariations
	{
		public float headSize;

		public float eartlerWidth;

		public float neckThickness;

		public float handsHeadColor;

		public float eyeSize;

		public float narrowEyes;

		public float eyesAngle;

		public float fatness;

		public float narrowWaist;

		public float legsSize;

		public float armThickness;

		public float wideTeeth;

		public float pupilSize;

		public float scruffy;

		public bool coloredEartlerTips;

		public bool deepPupils;

		public int coloredPupils;

		public int tailSegs;

		public float generalMelanin;

		public float WaistWidth => fatness * (1f - narrowWaist);

		public IndividualVariations(Scavenger scavenger)
		{
			generalMelanin = Custom.PushFromHalf(UnityEngine.Random.value, 2f);
			headSize = Custom.ClampedRandomVariation(0.5f, 0.5f, 0.1f);
			eartlerWidth = UnityEngine.Random.value;
			eyeSize = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(UnityEngine.Random.value, Mathf.Pow(headSize, 0.5f), UnityEngine.Random.value * 0.4f)), Mathf.Lerp(0.95f, 0.55f, scavenger.abstractCreature.personality.sympathy));
			narrowEyes = ((UnityEngine.Random.value < Mathf.Lerp(0.3f, 0.7f, scavenger.abstractCreature.personality.sympathy)) ? 0f : Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(0.5f, 1.5f, scavenger.abstractCreature.personality.sympathy)));
			if (scavenger.Elite)
			{
				narrowEyes = 1f;
			}
			eyesAngle = Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(2.5f, 0.5f, Mathf.Pow(scavenger.abstractCreature.personality.energy, 0.03f)));
			fatness = Mathf.Lerp(UnityEngine.Random.value, scavenger.abstractCreature.personality.dominance, UnityEngine.Random.value * 0.2f);
			if (ModManager.MSC)
			{
				fatness = Mathf.Lerp(fatness, 0.09f, (float)scavenger.abstractCreature.RemovedKarma / 5f);
			}
			if (scavenger.abstractCreature.personality.energy < 0.5f)
			{
				fatness = Mathf.Lerp(fatness, 1f, UnityEngine.Random.value * Mathf.InverseLerp(0.5f, 0f, scavenger.abstractCreature.personality.energy));
			}
			else
			{
				fatness = Mathf.Lerp(fatness, 0f, UnityEngine.Random.value * Mathf.InverseLerp(0.5f, 1f, scavenger.abstractCreature.personality.energy));
			}
			narrowWaist = Mathf.Lerp(Mathf.Lerp(UnityEngine.Random.value, 1f - fatness, UnityEngine.Random.value), 1f - scavenger.abstractCreature.personality.energy, UnityEngine.Random.value);
			neckThickness = Mathf.Lerp(Mathf.Pow(UnityEngine.Random.value, 1.5f - scavenger.abstractCreature.personality.aggression), 1f - fatness, UnityEngine.Random.value * 0.5f);
			pupilSize = 0f;
			deepPupils = false;
			coloredPupils = 0;
			if (UnityEngine.Random.value < 0.65f && eyeSize > 0.4f && narrowEyes < 0.3f)
			{
				if (UnityEngine.Random.value < Mathf.Pow(scavenger.abstractCreature.personality.sympathy, 1.5f) * 0.8f)
				{
					pupilSize = Mathf.Lerp(0.4f, 0.8f, Mathf.Pow(UnityEngine.Random.value, 0.5f));
					if (UnityEngine.Random.value < 2f / 3f)
					{
						coloredPupils = UnityEngine.Random.Range(1, 4);
					}
				}
				else
				{
					pupilSize = 0.7f;
					deepPupils = true;
				}
			}
			if (scavenger.Elite)
			{
				coloredPupils = UnityEngine.Random.Range(1, 4);
			}
			if (UnityEngine.Random.value < generalMelanin)
			{
				handsHeadColor = ((UnityEngine.Random.value < 0.3f) ? UnityEngine.Random.value : ((UnityEngine.Random.value < 0.6f) ? 1f : 0f));
			}
			else
			{
				handsHeadColor = ((UnityEngine.Random.value < 0.2f) ? UnityEngine.Random.value : ((UnityEngine.Random.value < 0.8f) ? 1f : 0f));
			}
			legsSize = UnityEngine.Random.value;
			armThickness = Mathf.Lerp(UnityEngine.Random.value, Mathf.Lerp(scavenger.abstractCreature.personality.dominance, fatness, 0.5f), UnityEngine.Random.value);
			coloredEartlerTips = scavenger.Elite || UnityEngine.Random.value < 1f / Mathf.Lerp(1.2f, 10f, generalMelanin);
			wideTeeth = UnityEngine.Random.value;
			tailSegs = ((!(UnityEngine.Random.value < 0.5f)) ? UnityEngine.Random.Range(1, 5) : 0);
			scruffy = 0f;
			if (UnityEngine.Random.value < 0.25f)
			{
				scruffy = Mathf.Pow(UnityEngine.Random.value, 0.3f);
			}
			scruffy = 1f;
		}
	}

	public class ScavengerHand : Limb
	{
		public int firstSprite;

		public float armLength = 50f;

		public float2 spearPosAdd;

		public float2 lastSpearPosAdd;

		private float2? grabPos;

		public ScavengerGraphics graphics => owner as ScavengerGraphics;

		public Scavenger scavenger => graphics.scavenger;

		public float MyFlip(float timeStacker)
		{
			return Mathf.Lerp(Custom.LerpMap(Mathf.Lerp(graphics.lastFlip, graphics.flip, timeStacker), -1f + (float)limbNumber, limbNumber, -1f, 1f), Custom.LerpMap(Custom.DistanceToLine(math.lerp(lastPos, pos, timeStacker), math.lerp(graphics.drawPositions[graphics.hipsDrawPos, 1], graphics.drawPositions[graphics.hipsDrawPos, 0], timeStacker), math.lerp(graphics.drawPositions[graphics.chestDrawPos, 1], graphics.drawPositions[graphics.chestDrawPos, 0], timeStacker)), 10f, -10f, -1f, 1f), Mathf.Lerp(graphics.lastSwimArms, graphics.swimArms, timeStacker));
		}

		public ScavengerHand(ScavengerGraphics owner, int num, int firstSprite)
			: base(owner, owner.scavenger.mainBodyChunk, num, 3f, 0.5f, 0.99f, 24f, 0.95f)
		{
			this.firstSprite = firstSprite;
		}

		public override void Update()
		{
			base.Update();
			lastSpearPosAdd = spearPosAdd;
			if (limbNumber == 0 && (scavenger.movMode == Scavenger.MovementMode.Run || scavenger.movMode == Scavenger.MovementMode.StandStill) && scavenger.grasps[0] != null && scavenger.grasps[0].grabbed is Spear)
			{
				spearPosAdd = WeaponDir() * 20f;
			}
			else
			{
				spearPosAdd *= 0f;
			}
			if (!scavenger.Consious)
			{
				base.mode = Mode.Dangle;
			}
			else if (limbNumber == 0 && scavenger.animation != null && scavenger.animation.id == Scavenger.ScavengerAnimation.ID.Throw)
			{
				base.mode = Mode.HuntAbsolutePosition;
				absoluteHuntPos = (scavenger.animation as Scavenger.ThrowAnimation).thrownObject.firstChunk.pos;
				if (scavenger.animation.age < 5)
				{
					pos = absoluteHuntPos;
				}
			}
			else if (limbNumber == 0 && scavenger.animation != null && scavenger.animation.id == Scavenger.ScavengerAnimation.ID.ThrowCharge && scavenger.animation.Active)
			{
				base.mode = Mode.HuntAbsolutePosition;
				float2 v = Vector3Ext.Slerp2(-(scavenger.animation as Scavenger.ThrowChargeAnimation).Direction.ToF2(), Custom.DirVec(graphics.drawPositions[graphics.hipsDrawPos, 1], graphics.drawPositions[graphics.chestDrawPos, 1]), 0.6f);
				float num = Mathf.Sin((float)Math.PI / 20f * (scavenger.animation as Scavenger.ThrowChargeAnimation).cycle);
				v += Custom.DirVec(graphics.drawPositions[graphics.headDrawPos + 1, 1], graphics.drawPositions[graphics.headDrawPos, 1]) * num * (0.1f + 0.9f * (scavenger.animation as Scavenger.ThrowChargeAnimation).shake) * 0.2f;
				absoluteHuntPos = scavenger.mainBodyChunk.pos + v.ToV2() * 35f;
				if (Custom.DistLess(absoluteHuntPos, graphics.drawPositions[graphics.headDrawPos, 1], 10f))
				{
					absoluteHuntPos += Custom.DirVec(graphics.drawPositions[graphics.headDrawPos, 1], absoluteHuntPos) * (10f - math.distance(graphics.drawPositions[graphics.headDrawPos, 1], absoluteHuntPos));
				}
			}
			else if (scavenger.Pointing && limbNumber == (scavenger.animation as Scavenger.PointingAnimation).PointingArm)
			{
				base.mode = Mode.HuntAbsolutePosition;
				absoluteHuntPos = scavenger.mainBodyChunk.pos;
				absoluteHuntPos += Custom.DirVec(scavenger.mainBodyChunk.pos, (scavenger.animation as Scavenger.PointingAnimation).LookPoint) * (50f + 10f * Mathf.Sin((scavenger.animation as Scavenger.PointingAnimation).cycle * 0.3f));
				absoluteHuntPos += Custom.RNV() * UnityEngine.Random.value * 2f * (scavenger.AI.ActNervous + scavenger.AI.agitation);
				spearPosAdd *= 0f;
			}
			else if (scavenger.Communicating && limbNumber == (scavenger.animation as Scavenger.CommunicationAnimation).GestureArm)
			{
				absoluteHuntPos = (scavenger.animation as Scavenger.CommunicationAnimation).GestureArmPos();
				spearPosAdd *= 0f;
			}
			else if (scavenger.movMode == Scavenger.MovementMode.Swim)
			{
				base.mode = Mode.Dangle;
				if (scavenger.moving)
				{
					float t = Mathf.Sin((graphics.cycle * 0.3f + (float)limbNumber * 0.5f) * (float)Math.PI * 2f);
					float num2 = scavenger.mainBodyChunk.pos.x + ((limbNumber == 0) ? (-1f) : 1f) * 5f + scavenger.flip * Mathf.Lerp(10f, 50f, t);
					absoluteHuntPos = new float2(num2, scavenger.room.FloatWaterLevel(num2) - 12f + 20f * Mathf.Cos((graphics.cycle * 0.3f + (float)limbNumber * 0.5f) * (float)Math.PI * 2f) * scavenger.flip);
				}
				else
				{
					float num3 = scavenger.mainBodyChunk.pos.x + ((limbNumber == 0) ? (-1f) : 1f) * 35f + scavenger.flip * 15f;
					absoluteHuntPos = new float2(num3, scavenger.room.FloatWaterLevel(num3) - 4f);
				}
				vel += Vector2.ClampMagnitude(absoluteHuntPos - pos, 4f) / 5f;
				pos = absoluteHuntPos;
			}
			else if (scavenger.movMode == Scavenger.MovementMode.Run && !scavenger.climbingUpComing)
			{
				huntSpeed = defaultHuntSpeed;
				if (grabPos.HasValue)
				{
					base.mode = Mode.HuntAbsolutePosition;
					absoluteHuntPos = grabPos.Value;
					if (scavenger.knucklePos.HasValue && !Custom.DistLess(grabPos.Value, scavenger.knucklePos.Value, 40f))
					{
						float2? @float = CheckForGrabPos();
						if (@float.HasValue && math.distance(scavenger.knucklePos.Value, @float.Value) < math.distance(scavenger.knucklePos.Value, grabPos.Value))
						{
							grabPos = @float;
						}
					}
					if (!GrabPosLegal(grabPos.Value))
					{
						grabPos = null;
					}
				}
				else
				{
					base.mode = Mode.Dangle;
					grabPos = CheckForGrabPos();
				}
			}
			else if (scavenger.Rummaging && (limbNumber == 1 || scavenger.grasps[0] == null) && scavenger.bodyChunks[2].pos.x > scavenger.mainBodyChunk.pos.x == scavenger.flip > 0f)
			{
				base.mode = Mode.HuntAbsolutePosition;
				if (limbNumber == 1 && (scavenger.animation as Scavenger.RummageAnimation).holdUpAndLook > 0)
				{
					base.mode = Mode.Dangle;
					vel *= 0.4f;
					vel += ((scavenger.animation as Scavenger.RummageAnimation).handPositions[limbNumber] - pos) / 5f;
				}
				else if (Custom.DistLess(absoluteHuntPos, (scavenger.animation as Scavenger.RummageAnimation).handPositions[limbNumber], 20f))
				{
					huntSpeed = 4.9f;
					quickness = 0.4f;
				}
				if (UnityEngine.Random.value < 0.25f || !Custom.DistLess(absoluteHuntPos, (scavenger.animation as Scavenger.RummageAnimation).handPositions[limbNumber], 20f))
				{
					absoluteHuntPos = (scavenger.animation as Scavenger.RummageAnimation).handPositions[limbNumber];
				}
			}
			else
			{
				StandardLocomotionProcedure();
			}
			float2 float2 = graphics.drawPositions[graphics.chestDrawPos, 0];
			float2 += Custom.PerpendicularVector((float2 - graphics.drawPositions[graphics.hipsDrawPos, 0]).normalized()) * (1f - Mathf.Abs(graphics.flip)) * 10f * (((float)limbNumber == 0f) ? (-1f) : 1f);
			float2 += Custom.DirVec(graphics.drawPositions[graphics.hipsDrawPos, 0], graphics.drawPositions[graphics.chestDrawPos, 0]) * 5f;
			if (Custom.DistLess(pos, float2, 10f))
			{
				Vector2 vector = Custom.DirVec(float2.ToV2(), pos) * (10f - math.distance(float2, pos));
				vel += vector;
				pos += vector;
			}
			if (base.mode == Mode.Dangle)
			{
				ConnectToPoint(scavenger.mainBodyChunk.pos, armLength, push: false, 0f, new float2(0f, 0f), 0f, 0f);
				if (scavenger.room.PointSubmerged(pos))
				{
					vel *= 0.8f;
				}
				else
				{
					vel.y -= 0.9f;
				}
			}
			else
			{
				ConnectToPoint(scavenger.mainBodyChunk.pos, armLength, push: false, 0f, scavenger.mainBodyChunk.vel, 0.4f, 0.1f);
			}
		}

		private void StandardLocomotionProcedure()
		{
			huntSpeed = defaultHuntSpeed;
			grabPos = null;
			bool flag = true;
			float2 @float = pos;
			Scavenger.MovementMode movementMode = scavenger.movMode;
			if (scavenger.climbingUpComing)
			{
				movementMode = Scavenger.MovementMode.Climb;
			}
			float2 float2 = scavenger.mainBodyChunk.pos.ToF2();
			if (movementMode == Scavenger.MovementMode.StandStill)
			{
				huntSpeed *= 0.2f;
				@float = float2 + Custom.RotateAroundOrigo(new float2(MyFlip(1f) * 30f, 0f), Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos));
				for (int i = 0; i < 3; i++)
				{
					if (!scavenger.room.GetTile(@float + new float2(0f, -20f)).Solid)
					{
						@float = new float2(@float.x, @float.y - 20f);
					}
				}
			}
			else if (movementMode == Scavenger.MovementMode.Crawl)
			{
				huntSpeed *= 0.6f;
				@float = ((!scavenger.knucklePos.HasValue) ? (float2 + Custom.RotateAroundOrigo(new float2(Mathf.Sign(MyFlip(1f)) * 5f + ((limbNumber == 0) ? (-5f) : 5f), 0f), Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos))) : (scavenger.knucklePos.Value.ToF2() + Custom.RotateAroundOrigo(new float2(Mathf.Sign(MyFlip(1f)) * 5f + ((limbNumber == 0) ? (-5f) : 5f), 0f), Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[0].pos, scavenger.knucklePos.Value))));
				if (Custom.DistLess(@float, graphics.hands[1 - limbNumber].absoluteHuntPos, 30f))
				{
					@float = graphics.hands[1 - limbNumber].absoluteHuntPos + Custom.DirVec(graphics.hands[1 - limbNumber].absoluteHuntPos, @float) * 30f;
				}
			}
			else if (movementMode == Scavenger.MovementMode.Climb)
			{
				flag = false;
				if (scavenger.swingPos.HasValue)
				{
					if (limbNumber == scavenger.swingArm)
					{
						base.mode = Mode.HuntAbsolutePosition;
						absoluteHuntPos = scavenger.swingPos.Value;
						pos = scavenger.swingPos.Value;
						@float = scavenger.swingPos.Value;
					}
					else if (scavenger.nextSwingPos.HasValue)
					{
						base.mode = Mode.HuntAbsolutePosition;
						absoluteHuntPos = math.lerp((float2 + pos.ToF2()) / 2f, scavenger.nextSwingPos.Value, 0.3f + 0.7f * Mathf.InverseLerp(100f, 50f, math.distance(float2, scavenger.nextSwingPos.Value)));
					}
					else
					{
						base.mode = Mode.Dangle;
						float2 v = float2 + Custom.RotateAroundOrigo(new float2(math.sign(MyFlip(1f)) * 8f + ((limbNumber == 0) ? (-11f) : 11f), -21f), Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos));
						vel *= 0.4f;
						vel += (v.ToV2() - pos) / 5f;
					}
				}
				else
				{
					if (base.mode == Mode.HuntAbsolutePosition)
					{
						if (!Custom.DistLess(connection.pos, absoluteHuntPos, armLength))
						{
							base.mode = Mode.Dangle;
						}
					}
					else if (scavenger.connections.Count > 0)
					{
						int num = scavenger.connections.Count - 1;
						while (num >= 0 && base.mode != Mode.HuntAbsolutePosition)
						{
							float2 float3 = scavenger.room.MiddleOfTile(scavenger.connections[num].DestTile);
							FindGrip(scavenger.room, connection.pos, float3, armLength, float3, -2, -2, behindWalls: false);
							num--;
						}
					}
					if (base.mode == Mode.Dangle)
					{
						flag = true;
						if (scavenger.connections.Count > 0)
						{
							@float = scavenger.room.MiddleOfTile(scavenger.connections[scavenger.connections.Count - 1].DestTile);
							@float += Custom.RotateAroundOrigo(new float2((limbNumber == 0) ? (-10f) : 10f, 10f), Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos));
						}
					}
				}
			}
			else
			{
				@float = float2 + Custom.RotateAroundOrigo(new float2(Mathf.Sign(MyFlip(1f)) * 30f, 0f), Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos));
			}
			if (flag)
			{
				if (!Custom.DistLess(@float, float2, armLength))
				{
					@float = float2 + Custom.DirVec(float2, @float) * armLength;
				}
				if (scavenger.room.GetTile(@float).Solid)
				{
					float2? float4 = SharedPhysics.ExactTerrainRayTracePos(scavenger.room, float2, @float);
					if (float4.HasValue)
					{
						@float = float4.Value;
					}
				}
				if (base.mode == Mode.HuntAbsolutePosition)
				{
					if (!Custom.DistLess(connection.pos, absoluteHuntPos, armLength))
					{
						base.mode = Mode.Dangle;
					}
					if (scavenger.movMode == Scavenger.MovementMode.StandStill && !Custom.DistLess(@float, absoluteHuntPos, 40f))
					{
						FindGrip(scavenger.room, connection.pos, @float, armLength, @float, -2, -2, behindWalls: false);
					}
				}
				else
				{
					FindGrip(scavenger.room, connection.pos, @float, armLength, @float, -2, -2, behindWalls: false);
				}
			}
			if (base.mode != Mode.HuntAbsolutePosition && scavenger.climbingUpComing && !scavenger.swingPos.HasValue)
			{
				FindGrip(scavenger.room, connection.pos, connection.pos + Custom.RNV() * armLength * UnityEngine.Random.value, armLength, connection.pos + Custom.RNV() * armLength * UnityEngine.Random.value, -2, -2, behindWalls: false);
			}
		}

		public override void GrabbedTerrain()
		{
			base.GrabbedTerrain();
			if (!Custom.DistLess(absoluteHuntPos, graphics.lastKnuckleSoundPos, 16f) && !Custom.DistLess(absoluteHuntPos, graphics.lastLastKnuckleSoundPos, 16f))
			{
				if (scavenger.room.GetTile(absoluteHuntPos).AnyBeam)
				{
					scavenger.room.PlaySound(SoundID.Scavenger_Grab_Pole, absoluteHuntPos);
				}
				else if (scavenger.movMode == Scavenger.MovementMode.Run && scavenger.room.GetTile(absoluteHuntPos + new Vector2(0f, -10f)).Solid)
				{
					scavenger.room.PlaySound((spearPosAdd.magnitude() > 1f) ? SoundID.Scavenger_Spear_Blunt_Hit_Ground : SoundID.Scavenger_Knuckle_Hit_Ground, absoluteHuntPos);
				}
				else if (scavenger.room.aimap.getTerrainProximity(absoluteHuntPos) < 2)
				{
					scavenger.room.PlaySound(SoundID.Scavenger_Grab_Terrain, absoluteHuntPos);
				}
				graphics.lastLastKnuckleSoundPos = graphics.lastKnuckleSoundPos;
				graphics.lastKnuckleSoundPos = absoluteHuntPos;
			}
		}

		private float2? CheckForGrabPos()
		{
			if (scavenger.knucklePos.HasValue)
			{
				float num = Custom.AimFromOneVectorToAnother(scavenger.mainBodyChunk.pos, scavenger.knucklePos.Value);
				for (float num2 = 0f; num2 < 45f; num2 += 5f)
				{
					for (int i = -1; i <= 1; i += 2)
					{
						float2? result = SharedPhysics.ExactTerrainRayTracePos(scavenger.room, scavenger.mainBodyChunk.pos, scavenger.mainBodyChunk.pos + Custom.DegToVec(num + num2 * (float)i + ((limbNumber == 0) ? (-4f) : 4f)) * armLength * 10.5f);
						if (result.HasValue && GrabPosLegal(result.Value))
						{
							return result;
						}
					}
				}
			}
			return null;
		}

		private bool GrabPosLegal(float2 pos)
		{
			float num = 1f;
			if (!Custom.DistLess(scavenger.mainBodyChunk.pos, pos, armLength * 1.5f * num))
			{
				return Custom.DistLess(scavenger.mainBodyChunk.pos + scavenger.mainBodyChunk.vel * 5f, pos, armLength * 1.5f * num);
			}
			return true;
		}

		[Obsolete("Use float2 parameter function instead.")]
		private bool GrabPosLegal(Vector2 pos)
		{
			return GrabPosLegal(new float2(pos.x, pos.y));
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[17]
			{
				new TriangleMesh.Triangle(0, 18, 1),
				new TriangleMesh.Triangle(18, 17, 1),
				new TriangleMesh.Triangle(16, 17, 1),
				new TriangleMesh.Triangle(1, 15, 16),
				new TriangleMesh.Triangle(1, 2, 15),
				new TriangleMesh.Triangle(14, 2, 15),
				new TriangleMesh.Triangle(3, 2, 14),
				new TriangleMesh.Triangle(14, 3, 13),
				new TriangleMesh.Triangle(4, 3, 13),
				new TriangleMesh.Triangle(12, 4, 13),
				new TriangleMesh.Triangle(4, 12, 5),
				new TriangleMesh.Triangle(11, 12, 5),
				new TriangleMesh.Triangle(11, 6, 5),
				new TriangleMesh.Triangle(11, 6, 10),
				new TriangleMesh.Triangle(7, 6, 10),
				new TriangleMesh.Triangle(7, 9, 10),
				new TriangleMesh.Triangle(7, 9, 8)
			};
			sLeaser.sprites[firstSprite] = new TriangleMesh("Futile_White", tris, customColor: true);
			sLeaser.sprites[firstSprite + 1] = new FSprite("pixel");
			sLeaser.sprites[firstSprite + 1].anchorY = 0f;
			sLeaser.sprites[firstSprite + 2] = new FSprite("pixel");
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, float2 camPos)
		{
			float2 @float = math.lerp(graphics.drawPositions[graphics.chestDrawPos, 1], graphics.drawPositions[graphics.chestDrawPos, 0], timeStacker);
			@float += Custom.PerpendicularVector((@float - math.lerp(graphics.drawPositions[graphics.hipsDrawPos, 1], graphics.drawPositions[graphics.hipsDrawPos, 0], timeStacker)).normalized()) * (1f - Mathf.Abs(Mathf.Lerp(graphics.lastFlip, graphics.flip, timeStacker))) * 10f * (((float)limbNumber == 0f) ? (-1f) : 1f);
			@float += Custom.DirVec(math.lerp(graphics.drawPositions[graphics.hipsDrawPos, 1], graphics.drawPositions[graphics.hipsDrawPos, 0], timeStacker), math.lerp(graphics.drawPositions[graphics.chestDrawPos, 1], graphics.drawPositions[graphics.chestDrawPos, 0], timeStacker)) * 5f;
			bool flag = reachedSnapPosition;
			if (scavenger.Pointing && limbNumber == (scavenger.animation as Scavenger.PointingAnimation).PointingArm)
			{
				flag = false;
			}
			else if (scavenger.animation != null && scavenger.animation is Scavenger.CommunicationAnimation && limbNumber == (scavenger.animation as Scavenger.CommunicationAnimation).GestureArm)
			{
				flag = false;
			}
			if (limbNumber == 0 && scavenger.grasps[0] != null)
			{
				flag = true;
			}
			float2 float2 = math.lerp(lastPos, pos, timeStacker);
			float2 += math.lerp(lastSpearPosAdd, spearPosAdd, timeStacker);
			if (scavenger.swingPos.HasValue)
			{
				if (limbNumber == scavenger.swingArm)
				{
					float2 = scavenger.swingPos.Value;
					flag = true;
				}
				else if (scavenger.nextSwingPos.HasValue && limbNumber != scavenger.swingArm)
				{
					flag = Custom.DistLess(float2, scavenger.nextSwingPos.Value, 5f);
				}
			}
			float2 float3 = float2 + Custom.DirVec(float2, @float) * 6f;
			if (Custom.DistLess(float3, @float, 5f))
			{
				float2 += Custom.DirVec(@float, float3) * (5f - math.distance(@float, float3));
				float3 += Custom.DirVec(@float, float3) * (5f - math.distance(@float, float3));
			}
			float2 float4 = Custom.InverseKinematic(@float, float3, 18f, 18f, MyFlip(timeStacker));
			float2 float5 = math.lerp(graphics.drawPositions[graphics.chestDrawPos, 1], graphics.drawPositions[graphics.chestDrawPos, 0], timeStacker);
			float2 float6 = Custom.PerpendicularVector((@float - float5).normalized());
			float num = (0f - (1.75f + Mathf.Lerp(-1f, 1f, graphics.iVars.armThickness))) * Mathf.Sign(MyFlip(timeStacker));
			for (int i = 0; i < 4; i++)
			{
				float2 float7 = @float;
				switch (i)
				{
				case 0:
					float7 = @float;
					break;
				case 1:
					float7 = float4;
					break;
				case 2:
					float7 = float3;
					break;
				case 3:
					float7 = float2;
					break;
				}
				float2 float8 = -(float5 - float7).normalized();
				float2 float9 = Custom.PerpendicularVector(float8);
				float num2 = math.distance(float7, float5);
				switch (i)
				{
				case 0:
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(0, float5 - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(18, float7 - float6 * num * 2f - float8 * num2 * 0.2f - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(1, math.lerp(float7 + (float9 + float6).normalized() * num * 2f - float8, float5, 0.5f) - camPos);
					break;
				case 1:
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(17, float5 - (float9 * 2f + float6).normalized() * num * 2f - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(16, float7 - float9 * num - float8 * num2 * 0.8f - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(15, float7 - float9 * num * 2.5f - float8 * num2 * 0.5f - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(2, float7 + float9 * num * 2.5f - float8 * num2 * 0.5f - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(14, float7 - float9 * num - float8 * num2 * 0.2f - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(3, float7 + float9 * num * 1.6f - float8 * num2 * 0.2f - camPos);
					break;
				case 2:
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(13, float5 - (float9 + float6).normalized() * num * 2f - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(4, float5 + Vector3Ext.Slerp2(float9, float6, 0.7f) * num * 2f - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(12, float7 - float9 * num * 1.3f - float8 * num2 * 0.8f - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(5, float5 + Vector3Ext.Slerp2(float9, float6, 0.3f) * num * 2f - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(11, float7 - float9 * num * 1.5f - float8 * num2 * 0.65f - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(6, float7 + float9 * num * 1.5f - float8 * num2 * 0.65f - camPos);
					break;
				case 3:
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(10, float5 - (float9 + float6).normalized() * num - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(7, float5 + (float9 + float6).normalized() * num - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(9, float7 - float9 * num - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(8, float7 + float9 * num - camPos);
					break;
				}
				float5 = float7;
				float6 = float9;
			}
			sLeaser.sprites[firstSprite + 1].x = float3.x - camPos.x;
			sLeaser.sprites[firstSprite + 1].y = float3.y - camPos.y;
			sLeaser.sprites[firstSprite + 1].element = Futile.atlasManager.GetElementWithName(flag ? "ScavengerHandB" : "ScavengerHandA");
			sLeaser.sprites[firstSprite + 1].rotation = Custom.AimFromOneVectorToAnother(float3, float2);
			sLeaser.sprites[firstSprite + 1].scaleX = (0f - num * 2f) * 3f / 18f;
		}

		[Obsolete("Use float2 parameter function instead.")]
		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			DrawSprites(sLeaser, rCam, timeStacker, new float2(camPos.x, camPos.y));
		}

		public float2 WeaponDir()
		{
			Vector2 vector = Custom.PerpendicularVector(scavenger.mainBodyChunk.pos, pos) * (0f - MyFlip(1f));
			vector += Custom.DirVec(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos) * ((!scavenger.swingPos.HasValue) ? 1f : 0.1f);
			if (scavenger.animation != null && scavenger.animation.id == Scavenger.ScavengerAnimation.ID.ThrowCharge)
			{
				vector += Custom.DirVec(pos, (scavenger.animation as Scavenger.ThrowChargeAnimation).UseTarget) * 2f;
			}
			else if (scavenger.Pointing && limbNumber == (scavenger.animation as Scavenger.PointingAnimation).PointingArm)
			{
				vector += Custom.DirVec(pos, (scavenger.animation as Scavenger.PointingAnimation).LookPoint) * 7f;
			}
			else if (scavenger.animation != null && scavenger.animation is Scavenger.CommunicationAnimation && (scavenger.animation as Scavenger.CommunicationAnimation).pointWithSpears && limbNumber == (scavenger.animation as Scavenger.CommunicationAnimation).GestureArm)
			{
				vector += Custom.DirVec(scavenger.mainBodyChunk.pos, (scavenger.animation as Scavenger.CommunicationAnimation).GestureArmPos()) * 7f;
			}
			return vector.normalized.ToF2();
		}
	}

	public class ScavengerLeg : Limb
	{
		public int firstSprite;

		public float legLength;

		public ScavengerGraphics graphics => owner as ScavengerGraphics;

		public Scavenger scavenger => graphics.scavenger;

		public float2 IdealPos
		{
			get
			{
				if (scavenger.movMode == Scavenger.MovementMode.Run)
				{
					return connection.pos + Vector2.ClampMagnitude(connection.vel * 10f, legLength);
				}
				return connection.pos;
			}
		}

		public ScavengerLeg(ScavengerGraphics owner, int num, int firstSprite)
			: base(owner, owner.owner.bodyChunks[1], num, 1f, 0.5f, 0.99f, 12f, 0.95f)
		{
			this.firstSprite = firstSprite;
			legLength = Mathf.Lerp(20f, 40f, owner.iVars.legsSize);
		}

		public override void Update()
		{
			base.Update();
			ConnectToPoint(connection.pos, legLength, push: false, 0f, connection.vel, 0.8f, 0.5f);
			if (scavenger.Consious)
			{
				if (scavenger.movMode == Scavenger.MovementMode.Swim)
				{
					base.mode = Mode.Dangle;
					float2 vec = new float2(((limbNumber == 0) ? (-10f) : 10f) - scavenger.flip * 7f + Mathf.Sin((graphics.cycle + (float)limbNumber * 0.5f) * (float)Math.PI * 2f) * 12.5f * scavenger.flip, (0f - legLength) * 0.6f + Mathf.Cos((graphics.cycle + (float)limbNumber * 0.5f) * (float)Math.PI * 2f) * 3.5f * scavenger.flip);
					vec = Custom.RotateAroundOrigo(vec, Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos));
					vel += Vector2.ClampMagnitude((graphics.drawPositions[graphics.hipsDrawPos, 0] + vec).ToV2() - pos, 8f) / 2f;
				}
				else if (scavenger.movMode != Scavenger.MovementMode.Climb || !scavenger.swingPos.HasValue || scavenger.swingClimbCounter < 30)
				{
					vel += Custom.DirVec(scavenger.mainBodyChunk.pos, scavenger.bodyChunks[1].pos);
					if (base.mode == Mode.HuntAbsolutePosition)
					{
						if (!Custom.DistLess(IdealPos, absoluteHuntPos, legLength))
						{
							base.mode = Mode.HuntRelativePosition;
						}
					}
					else
					{
						relativeHuntPos = new float2((0f - scavenger.flip) * 10f + ((limbNumber == 0) ? (-4f) : 4f), 10f);
						FindGrip(scavenger.room, IdealPos, IdealPos, legLength, IdealPos + Custom.RotateAroundOrigo(new float2((limbNumber == 0) ? (-1f) : 1f, 0f) * 5f, Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.mainBodyChunk.pos)), -2, -2, behindWalls: false);
					}
				}
				else
				{
					base.mode = Mode.Dangle;
				}
			}
			else
			{
				base.mode = Mode.Dangle;
			}
			if (base.mode == Mode.Dangle)
			{
				vel.y -= 0.9f;
				vel += Custom.RotateAroundOrigo(new float2(0.1f * scavenger.flip + ((limbNumber == 0) ? (-0.15f) : 0.15f), -0.1f), Custom.AimFromOneVectorToAnother(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos)).ToV2();
				if (!Custom.DistLess(pos, connection.pos, legLength / 3f))
				{
					vel += Custom.DirVec(connection.pos, pos) * (legLength / 3f - math.distance(connection.pos, pos)) / 15f;
					pos += Custom.DirVec(connection.pos, pos) * (legLength / 3f - math.distance(connection.pos, pos)) / 15f;
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[10]
			{
				new TriangleMesh.Triangle(0, 1, 2),
				new TriangleMesh.Triangle(1, 2, 3),
				new TriangleMesh.Triangle(3, 4, 2),
				new TriangleMesh.Triangle(4, 2, 5),
				new TriangleMesh.Triangle(2, 5, 6),
				new TriangleMesh.Triangle(5, 6, 7),
				new TriangleMesh.Triangle(5, 7, 8),
				new TriangleMesh.Triangle(8, 7, 9),
				new TriangleMesh.Triangle(8, 9, 10),
				new TriangleMesh.Triangle(8, 9, 11)
			};
			sLeaser.sprites[firstSprite] = new TriangleMesh("Futile_White", tris, customColor: true);
			sLeaser.sprites[firstSprite + 1] = new FSprite("pixel");
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, float2 camPos)
		{
			float num = Custom.LerpMap(Mathf.Lerp(graphics.lastFlip, graphics.flip, timeStacker), 0f - (float)limbNumber, 1f - (float)limbNumber, -1f, 1f);
			float num2 = Mathf.Lerp(1f, 1.4f, graphics.iVars.legsSize);
			float2 @float = math.lerp(graphics.drawPositions[graphics.hipsDrawPos, 1], graphics.drawPositions[graphics.hipsDrawPos, 0], timeStacker);
			float2 float2 = math.lerp(lastPos, pos, timeStacker);
			float2 -= Custom.PerpendicularVector((float2 - @float).normalized()) * num2 * Custom.LerpMap(math.distance(@float, float2) / num2, 2f, 20f, 6f, -1f) * num;
			if (Custom.DistLess(float2, @float, 3f))
			{
				float2 = @float + Custom.DirVec(@float, float2) * 3f;
			}
			float2 float3 = float2 + Custom.DirVec(float2, @float) * 1.5f * num2;
			float2 float4 = Custom.InverseKinematic(@float, float3, 10f * num2, 10f * num2, 0f - num);
			if (Custom.DistLess(float4, (@float + float3) / 2f, 3f * num2))
			{
				float4 = (@float + float3) / 2f + Custom.DirVec((@float + float3) / 2f, float4) * 3f * num2;
			}
			float2 += Custom.DirVec(@float, float4) * 3f * num2;
			float2 += Custom.DirVec(float3, float4) * 2f * num2;
			float2 float5 = math.lerp((@float + float4 + float3) / 3f, (@float + float3) / 2f, 0.35f);
			float4 += Custom.PerpendicularVector((float3 - @float).normalized()) * num2 * Custom.LerpMap(Mathf.Abs(Custom.DistanceToLine(float4, @float, float3)), 0f, 20f, 2f, 0f) * Mathf.Sign(num);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(0, @float - Custom.PerpendicularVector((@float - float4).normalized()) * 2f * num2 * (0f - Mathf.Sign(num)) - camPos);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(1, @float + Custom.PerpendicularVector((@float - float4).normalized()) * 2f * num2 * (0f - Mathf.Sign(num)) - camPos);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(2, float5 - camPos);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(3, float4 - (float3 - @float).normalized() * 3f * num2 - camPos);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(4, float4 + (float3 - @float).normalized() * 3f * num2 - camPos);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(4, float4 + (float3 - @float).normalized() * 3f * num2 - camPos);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(5, float3 - ((float3 - float4).normalized() + (@float - float4).normalized()).normalized() * Custom.LerpMap(math.distance(@float, float3), 10f * num2, 30f * num2, 3f, 1f) * num2 - camPos);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(6, float3 + (@float - float2).normalized() * 0.8f * num2 - camPos);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(7, float3 - (@float - float2).normalized() * 0.8f * num2 - camPos);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(8, float2 + Custom.PerpendicularVector((float3 - float2).normalized()) * 0.8f * num2 * (0f - Mathf.Sign(num)) - camPos);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(9, float2 - Custom.PerpendicularVector((float3 - float2).normalized()) * 0.8f * num2 * (0f - Mathf.Sign(num)) - camPos);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(10, float2 + Custom.PerpendicularVector((float3 - float2).normalized()) * 0.7f * num2 * (0f - Mathf.Sign(num)) + Custom.DirVec(float3, float2) * num2 * 2f - camPos);
			(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(11, float2 - Custom.PerpendicularVector((float3 - float2).normalized()) * 1.2f * num2 * (0f - Mathf.Sign(num)) + Custom.DirVec(float3, float2) * num2 * 2f - camPos);
		}

		[Obsolete("Use float2 parameter function instead.")]
		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			DrawSprites(sLeaser, rCam, timeStacker, new float2(camPos.x, camPos.y));
		}
	}

	public class Eartlers
	{
		public struct Vertex
		{
			public float2 pos;

			public float rad;

			public Vertex(float2 pos, float rad)
			{
				this.pos = pos;
				this.rad = rad;
			}

			[Obsolete("Use float2 parameter constructor instead.")]
			public Vertex(Vector2 pos, float rad)
				: this(new float2(pos.x, pos.y), rad)
			{
			}
		}

		private ScavengerGraphics owner;

		private List<Vertex[]> points;

		public int firstSprite;

		public int TotalSprites => points.Count;

		public Eartlers(int firstSprite, ScavengerGraphics owner)
		{
			this.firstSprite = firstSprite;
			this.owner = owner;
			GenerateSegments();
		}

		private void GenerateSegments()
		{
			bool elite = owner.scavenger.Elite;
			float num = (elite ? 1.75f : 1f);
			float2 @float = new float2(elite ? 45f : 15f, elite ? 90f : 45f);
			float num2 = (elite ? 1.5f : 1f);
			float num3 = (elite ? 2f : 1f);
			float num4 = (elite ? 1f : 1f);
			float num5 = (elite ? 0f : 1f);
			float num6 = (elite ? 2f : 1f);
			float num7 = (elite ? 0f : 1f);
			points = new List<Vertex[]>();
			List<Vertex> list = new List<Vertex>();
			list.Clear();
			list.Add(new Vertex(new float2(0f, 0f), 1f));
			list.Add(new Vertex(Custom.DegToFloat2(Mathf.Lerp(40f, 90f, UnityEngine.Random.value)) * 0.4f * num2, 1f * num3));
			float2 float2 = Custom.DegToFloat2(Mathf.Lerp(@float.x, @float.y, UnityEngine.Random.value) * num);
			float2 pos = float2 - Custom.DegToFloat2(Mathf.Lerp(40f, 90f, UnityEngine.Random.value)) * 0.4f * num2;
			if (pos.x < 0.2f)
			{
				pos = new float2(Mathf.Lerp(pos.x, float2.x, 0.4f), pos.y);
			}
			list.Add(new Vertex(pos, 1.5f * num4));
			list.Add(new Vertex(float2, 2f * num5));
			DefineBranch(list);
			list.Clear();
			list.Add(new Vertex(points[0][1].pos, 1f));
			int num8 = ((!((double)math.distance(points[0][1].pos, points[0][2].pos) > 0.6) || !(UnityEngine.Random.value < 0.5f)) ? 1 : 2);
			float2 float3 = math.lerp(points[0][1].pos, points[0][2].pos, Mathf.Lerp(0f, (num8 == 1) ? 0.7f : 0.25f, UnityEngine.Random.value));
			list.Add(new Vertex(float3, 1.2f));
			list.Add(new Vertex(float3 + points[0][3].pos - points[0][2].pos + Custom.DegToFloat2(UnityEngine.Random.value * 360f) * 0.1f, 1.75f));
			DefineBranch(list);
			if (num8 == 2)
			{
				list.Clear();
				float3 = math.lerp(points[0][1].pos, points[0][2].pos, Mathf.Lerp(0.45f, 0.7f, UnityEngine.Random.value));
				list.Add(new Vertex(float3, 1.2f));
				list.Add(new Vertex(float3 + points[0][3].pos - points[0][2].pos + Custom.DegToFloat2(UnityEngine.Random.value * 360f) * 0.1f, 1.75f));
				DefineBranch(list);
			}
			bool flag = UnityEngine.Random.value < 0.5f && !elite;
			if (flag)
			{
				list.Clear();
				float2 float4 = Custom.DegToFloat2(90f + Mathf.Lerp(-20f, 20f, UnityEngine.Random.value)) * Mathf.Lerp(0.2f, 0.5f, UnityEngine.Random.value);
				if (float4.y > points[0][1].pos.y - 0.1f)
				{
					float4 = new float2(float4.x, float4.y - 0.2f);
				}
				float num9 = Mathf.Lerp(0.8f, 2f, UnityEngine.Random.value);
				if (UnityEngine.Random.value < 0.5f)
				{
					float4 += Custom.DegToFloat2(Mathf.Lerp(120f, 170f, UnityEngine.Random.value)) * Mathf.Lerp(0.1f, 0.3f, UnityEngine.Random.value);
					list.Add(new Vertex(new float2(0f, 0f), num9));
					list.Add(new Vertex(float4, num9));
				}
				else
				{
					list.Add(new Vertex(new float2(0f, 0f), 1f));
					list.Add(new Vertex(float4, (1f + num9) / 2f));
					list.Add(new Vertex(float4 + Custom.DegToFloat2(Mathf.Lerp(95f, 170f, UnityEngine.Random.value)) * Mathf.Lerp(0.1f, 0.2f, UnityEngine.Random.value), num9));
				}
				DefineBranch(list);
			}
			if (UnityEngine.Random.value > 0.25f || !flag || elite)
			{
				list.Clear();
				float num10 = 1f + UnityEngine.Random.value * 1.5f;
				bool flag2 = UnityEngine.Random.value < 0.5f;
				list.Add(new Vertex(new float2(0f, 0f), 1f));
				float num11 = Mathf.Lerp(95f, 135f, UnityEngine.Random.value);
				float num12 = Mathf.Lerp(0.25f, 0.4f, UnityEngine.Random.value) * num6;
				list.Add(new Vertex(Custom.DegToFloat2(num11) * num12, (flag2 ? 0.8f : Mathf.Lerp(1f, num10, 0.3f)) * num6));
				list.Add(new Vertex(Custom.DegToFloat2(num11 + Mathf.Lerp(5f, 35f, UnityEngine.Random.value)) * Mathf.Max(num12 + 0.1f, Mathf.Lerp(0.3f, 0.6f, UnityEngine.Random.value)), flag2 ? 0.8f : Mathf.Lerp(1f, num10, 0.6f)));
				list.Add(new Vertex(list[list.Count - 1].pos.normalized() * (list[list.Count - 1].pos.magnitude() + Mathf.Lerp(0.15f, 0.25f, UnityEngine.Random.value) * num6), num10 * num7));
				DefineBranch(list);
			}
		}

		private void DefineBranch(List<Vertex> vList)
		{
			points.Add(vList.ToArray());
			for (int i = 0; i < vList.Count; i++)
			{
				vList[i] = new Vertex(new float2(0f - vList[i].pos.x, vList[i].pos.y), vList[i].rad);
			}
			points.Add(vList.ToArray());
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			int num = firstSprite;
			for (int i = 0; i < points.Count; i++)
			{
				sLeaser.sprites[num] = TriangleMesh.MakeLongMesh(points[i].Length, pointyTip: false, owner.iVars.coloredEartlerTips);
				num++;
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette, Color headColor, Color decorationColor)
		{
			for (int i = 0; i < points.Count; i++)
			{
				sLeaser.sprites[firstSprite + i].color = headColor;
			}
			if (!owner.iVars.coloredEartlerTips)
			{
				return;
			}
			for (int j = 0; j < points.Count; j++)
			{
				for (int k = 0; k < 2; k++)
				{
					(sLeaser.sprites[firstSprite + j] as TriangleMesh).verticeColors[(points[j].Length - 1) * 4 + 3 - k] = decorationColor;
				}
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, float2 camPos, float2 headPos, float2 headDir, float lookUpFac)
		{
			float rotat = Custom.VecToDeg(-headDir);
			float num = 1f - Mathf.Pow(Mathf.Abs(Custom.RotateAroundOrigo(headDir, owner.BodyAxis(timeStacker)).x), 1.5f) * Mathf.Lerp(1f, 0.5f, lookUpFac);
			float ySqueeze = Mathf.Lerp(1f, -0.25f, lookUpFac);
			int num2 = firstSprite;
			for (int i = 0; i < points.Count; i++)
			{
				float2 @float = RotatedPosOfVertex(i, 0, headPos, rotat, num, ySqueeze);
				float2 a = Custom.PerpendicularVector(points[i][1].pos, points[i][0].pos);
				float num3 = points[i][0].rad;
				for (int j = 0; j < points[i].Length; j++)
				{
					float num4 = (float)j / (float)(points[i].Length - 1);
					float2 float2 = RotatedPosOfVertex(i, j, headPos, rotat, num, ySqueeze);
					float2 float3 = (@float - float2).normalized();
					float2 float4 = Custom.PerpendicularVector(float3);
					float num5 = math.distance(float2, @float) / 10f;
					float num6 = Mathf.Lerp(1f, 2f, owner.iVars.eartlerWidth) * points[i][j].rad * Mathf.Lerp(0.5f, 1f, num);
					(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(j * 4, @float - Vector3Ext.Slerp2(a, float4, 0.5f) * (num6 + num3) * 0.5f - float3 * num5 - camPos);
					(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(j * 4 + 1, @float + Vector3Ext.Slerp2(a, float4, 0.5f) * (num6 + num3) * 0.5f - float3 * num5 - camPos);
					if (num4 == 1f)
					{
						num6 /= 4f;
					}
					(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(j * 4 + 2, float2 - float4 * num6 + float3 * num5 - camPos);
					(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(j * 4 + 3, float2 + float4 * num6 + float3 * num5 - camPos);
					@float = float2;
					a = float4;
					num3 = num6;
				}
				num2++;
			}
		}

		[Obsolete("Use float2 parameter function instead.")]
		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 headPos, Vector2 headDir, float lookUpFac)
		{
			DrawSprites(sLeaser, rCam, timeStacker, new float2(camPos.x, camPos.y), new float2(headPos.x, headPos.y), new float2(headDir.x, headDir.y), lookUpFac);
		}

		private float2 RotatedPosOfVertex(int segment, int vert, float2 headPos, float rotat, float xSqueeze, float ySqueeze)
		{
			return headPos + Custom.RotateAroundOrigo(new float2(points[segment][vert].pos.x * xSqueeze, points[segment][vert].pos.y * ySqueeze), rotat) * Mathf.Lerp(15f, 35f, owner.scavenger.abstractCreature.personality.dominance);
		}

		[Obsolete("Use float2 parameter function instead.")]
		private Vector2 RotatedPosOfVertex(int segment, int vert, Vector2 headPos, float rotat, float xSqueeze, float ySqueeze)
		{
			float2 @float = RotatedPosOfVertex(segment, vert, new float2(headPos.x, headPos.y), rotat, xSqueeze, ySqueeze);
			return new Vector2(@float.x, @float.y);
		}
	}

	public Scavenger scavenger;

	public ScavengerHand[] hands;

	public ScavengerLeg[] legs;

	public float2[] chestPatchShape;

	public float2[,] drawPositions;

	public int headDrawPos;

	public int chestDrawPos = 2;

	public int hipsDrawPos = 4;

	public float[] spineLengths;

	public int totBckCosSprs;

	public int totFrntCosSprs;

	public float cycle;

	public float lastCycle;

	public float flip;

	public float lastFlip;

	public float2 bodyAxis;

	public float2 lastBodyAxis;

	private float riseBody;

	private float lookUp;

	private float lastLookUp;

	private float neutralFace;

	private float lastNeutralFace;

	private float shiftingNeutralFace;

	private float shiftingNeutralFaceGoal;

	public Eartlers eartlers;

	public HSLColor bodyColor;

	public HSLColor headColor;

	public HSLColor decorationColor;

	public HSLColor bellyColor;

	public HSLColor eyeColor;

	public float bodyColorBlack;

	public float headColorBlack;

	public float bellyColorBlack;

	public Color blackColor;

	public float[,] teeth;

	public float2 lastKnuckleSoundPos;

	public float2 lastLastKnuckleSoundPos;

	public bool spearSound;

	public TailSegment[] tail;

	public float swimArms;

	public float lastSwimArms;

	public float eyesOpen;

	public float lastEyesOpen;

	private int blink;

	public float2 lookPoint;

	public float2 lastLookPoint;

	public float shake;

	public float lastShake;

	public float eyesPop;

	public float lastEyesPop;

	public float bristle;

	public float lastBristle;

	public float eyesPopGoBack;

	public VultureMaskGraphics maskGfx;

	public CentipedeShellCosmetic[] shells;

	public float markAlpha;

	public float lastMarkAlpha;

	public IndividualVariations iVars;

	private float SAVFLIP;

	private bool objectsInRightContainers;

	public float darkness;

	public int FirstBehindLimbSprite => 0;

	public int FirstBckCosmeticSprite => 5;

	public int ChestSprite => totBckCosSprs + 5;

	public int HipSprite => totBckCosSprs + 6;

	public int WaistSprite => totBckCosSprs + 7;

	public int ChestPatchSprite => totBckCosSprs + 8;

	public int FirstInFrontLimbSprite => totBckCosSprs + 9;

	public int NeckSprite => totBckCosSprs + 14;

	public int FirstFrntCosmeticSprite => totBckCosSprs + 15;

	public int FirstEartlerSprite => totBckCosSprs + totFrntCosSprs + 15;

	public int HeadSprite => totBckCosSprs + totFrntCosSprs + 15 + eartlers.TotalSprites;

	public int TeethSprite => totBckCosSprs + totFrntCosSprs + 16 + eartlers.TotalSprites;

	public int MaskSprite => totBckCosSprs + totFrntCosSprs + 19 + ((!(iVars.pupilSize <= 0f)) ? 2 : 0) + eartlers.TotalSprites;

	public int ShellSprite => totBckCosSprs + totFrntCosSprs + 19 + ((!(iVars.pupilSize <= 0f)) ? 2 : 0) + eartlers.TotalSprites + ((maskGfx != null) ? maskGfx.TotalSprites : 0);

	public int TotalSprites
	{
		get
		{
			if (!ModManager.MSC)
			{
				return totBckCosSprs + totFrntCosSprs + 19 + ((iVars.pupilSize > 0f) ? 2 : 0) + eartlers.TotalSprites;
			}
			int num = ShellSprite;
			for (int i = 0; i < shells.Length; i++)
			{
				num += shells[i].TotalSprites;
			}
			if (scavenger.King)
			{
				num += 2;
			}
			return num;
		}
	}

	public Color BlendedBodyColor => Color.Lerp(bodyColor.rgb, blackColor, Mathf.Lerp(bodyColorBlack, 1f, darkness));

	public Color BlendedHeadColor => Color.Lerp(headColor.rgb, blackColor, Mathf.Lerp(headColorBlack, 1f, darkness));

	public Color BlendedDecorationColor => Color.Lerp(decorationColor.rgb, blackColor, darkness * 0.9f);

	public Color BlendedEyeColor
	{
		get
		{
			if (darkness < 0.5f)
			{
				return eyeColor.rgb;
			}
			return Custom.Saturate(eyeColor.rgb, Mathf.InverseLerp(0.5f, 1f, darkness));
		}
	}

	public int EyeSprite(int eye, int part)
	{
		return totBckCosSprs + totFrntCosSprs + 17 + eartlers.TotalSprites + eye + part * 2;
	}

	public float Cycle(float timeStacker)
	{
		return Mathf.Sin(Mathf.Lerp(lastCycle, cycle, timeStacker) * (float)Math.PI * 2f);
	}

	public float2 HeadDir(float t)
	{
		return (float2Ext.ClampMagnitude(math.lerp(lastLookPoint, lookPoint, t) - math.lerp(scavenger.mainBodyChunk.lastPos, scavenger.mainBodyChunk.pos, t), 300f) / 300f * 0.2f + float2Ext.ClampMagnitude(math.lerp(drawPositions[headDrawPos, 1], drawPositions[headDrawPos, 0], t) - math.lerp(drawPositions[chestDrawPos, 1], drawPositions[chestDrawPos, 0], t), 100f) / 100f).normalized();
	}

	public float BodyAxis(float timeStacker)
	{
		return 0f - Custom.VecToDeg(Vector3Ext.Slerp2(lastBodyAxis, bodyAxis, timeStacker).normalized());
	}

	public ScavengerGraphics(PhysicalObject ow)
		: base(ow, internalContainers: true)
	{
		scavenger = ow as Scavenger;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(scavenger.abstractCreature.ID.RandomSeed);
		iVars = new IndividualVariations(scavenger);
		GenerateColors();
		tail = new TailSegment[iVars.tailSegs];
		if (iVars.tailSegs > 0)
		{
			for (int i = 0; i < tail.Length; i++)
			{
				tail[i] = new TailSegment(this, Mathf.Lerp(2f, 1f, (float)i / (float)(iVars.tailSegs - 1)), 10f, (i > 0) ? tail[i - 1] : null, 0.5f, 0.9f, 0.5f, pullInPreviousPosition: true);
			}
			AddSubModule(new Tail(this, FirstBckCosmeticSprite + totBckCosSprs));
			totBckCosSprs += subModules[subModules.Count - 1].totalSprites;
		}
		if (UnityEngine.Random.value < 0.1f || scavenger.Elite)
		{
			AddSubModule(new HardBackSpikes(this, FirstBckCosmeticSprite + totBckCosSprs));
			totBckCosSprs += subModules[subModules.Count - 1].totalSprites;
		}
		else
		{
			AddSubModule(new WobblyBackTufts(this, FirstBckCosmeticSprite + totBckCosSprs));
			totBckCosSprs += subModules[subModules.Count - 1].totalSprites;
		}
		hands = new ScavengerHand[2];
		legs = new ScavengerLeg[2];
		int firstBehindLimbSprite = FirstBehindLimbSprite;
		legs[1] = new ScavengerLeg(this, 1, firstBehindLimbSprite);
		firstBehindLimbSprite += 2;
		hands[1] = new ScavengerHand(this, 1, firstBehindLimbSprite);
		firstBehindLimbSprite += 3;
		firstBehindLimbSprite = FirstInFrontLimbSprite;
		legs[0] = new ScavengerLeg(this, 0, firstBehindLimbSprite);
		firstBehindLimbSprite += 2;
		hands[0] = new ScavengerHand(this, 0, firstBehindLimbSprite);
		firstBehindLimbSprite += 3;
		bodyParts = new BodyPart[4 + tail.Length];
		for (int j = 0; j < 2; j++)
		{
			bodyParts[j] = hands[j];
			bodyParts[j + 2] = legs[j];
		}
		for (int k = 0; k < tail.Length; k++)
		{
			bodyParts[4 + k] = tail[k];
		}
		drawPositions = new float2[5, 2];
		spineLengths = new float[2];
		eartlers = new Eartlers(FirstEartlerSprite, this);
		teeth = new float[UnityEngine.Random.Range(2, 5) * 2, 2];
		float num = Mathf.Lerp(0.5f, 1.5f, Mathf.Pow(UnityEngine.Random.value, 1.5f - scavenger.abstractCreature.personality.aggression));
		num = Mathf.Lerp(num, num * Custom.LerpMap(teeth.GetLength(0), 4f, 8f, 1f, 0.5f), 0.3f);
		float a = Mathf.Lerp(num + 0.2f, Mathf.Lerp(0.7f, 1.2f, UnityEngine.Random.value), UnityEngine.Random.value);
		a = Mathf.Lerp(a, Custom.LerpMap(teeth.GetLength(0), 4f, 8f, 1.5f, 0.2f), 0.4f);
		float a2 = 0.3f + 0.7f * UnityEngine.Random.value;
		for (int l = 0; l < teeth.GetLength(0); l++)
		{
			float num2 = (float)l / (float)(teeth.GetLength(0) - 1);
			teeth[l, 0] = Mathf.Lerp(a2, 1f, Mathf.Sin(num2 * (float)Math.PI)) * num;
			if (UnityEngine.Random.value < iVars.scruffy && UnityEngine.Random.value < 0.2f)
			{
				teeth[l, 0] = 0f;
			}
			teeth[l, 1] = Mathf.Lerp(0.5f, 1f, Mathf.Sin(num2 * (float)Math.PI)) * a;
		}
		chestPatchShape = new float2[8];
		float num3 = 0.4f;
		float num4 = 0.95f;
		float num5 = 1f;
		chestPatchShape[0] = new float2((0f - num5) / 4f, num3);
		chestPatchShape[1] = new float2(num5 / 4f, num3);
		chestPatchShape[2] = new float2(0f - num5, Mathf.Lerp(num3, num4, 1f / 3f));
		chestPatchShape[3] = new float2(num5, Mathf.Lerp(num3, num4, 1f / 3f));
		chestPatchShape[4] = new float2(0f - num5, Mathf.Lerp(num3, num4, 2f / 3f));
		chestPatchShape[5] = new float2(num5, Mathf.Lerp(num3, num4, 2f / 3f));
		chestPatchShape[6] = new float2((0f - num5) / 3f, num4);
		chestPatchShape[7] = new float2(num5 / 3f, num4);
		if (scavenger.King)
		{
			maskGfx = new VultureMaskGraphics(scavenger, VultureMask.MaskType.SCAVKING, MaskSprite, "KingMask");
			maskGfx.GenerateColor(scavenger.abstractCreature.ID.RandomSeed);
		}
		else if (scavenger.Elite)
		{
			string overrideSprite = "KrakenMask";
			switch (UnityEngine.Random.Range(0, 4))
			{
			case 1:
				overrideSprite = "SpikeMask";
				break;
			case 2:
				overrideSprite = "HornedMask";
				break;
			case 3:
				overrideSprite = "SadMask";
				break;
			}
			maskGfx = new VultureMaskGraphics(scavenger, VultureMask.MaskType.NORMAL, MaskSprite, overrideSprite);
			maskGfx.GenerateColor(scavenger.abstractCreature.ID.RandomSeed);
		}
		if (scavenger.King)
		{
			shells = new CentipedeShellCosmetic[3];
		}
		else
		{
			shells = new CentipedeShellCosmetic[0];
		}
		int num6 = ShellSprite;
		for (int m = 0; m < shells.Length; m++)
		{
			shells[m] = new CentipedeShellCosmetic(num6, scavenger.firstChunk.pos, 0f, 1f, 1f, 1f);
			num6 += shells[m].TotalSprites;
		}
		UnityEngine.Random.state = state;
	}

	public override void Reset()
	{
		base.Reset();
		drawPositions[0, 0] = scavenger.bodyChunks[2].pos;
		drawPositions[2, 0] = scavenger.bodyChunks[0].pos;
		drawPositions[4, 0] = scavenger.bodyChunks[1].pos;
		drawPositions[1, 0] = math.lerp(drawPositions[0, 0], drawPositions[2, 0], 0.5f);
		drawPositions[3, 0] = math.lerp(drawPositions[2, 0], drawPositions[4, 0], 0.5f);
		for (int i = 0; i < drawPositions.GetLength(0); i++)
		{
			drawPositions[i, 1] = drawPositions[i, 0];
		}
	}

	public override void Update()
	{
		if (!objectsInRightContainers)
		{
			scavenger.PlaceAllGrabbedObjectsInCorrectContainers();
			objectsInRightContainers = true;
		}
		base.Update();
		lastMarkAlpha = markAlpha;
		if (scavenger.Stunned)
		{
			markAlpha = Mathf.Lerp(markAlpha, UnityEngine.Random.Range(0f, 0.5f), 0.25f);
		}
		else if (!scavenger.dead)
		{
			markAlpha = Mathf.Lerp(markAlpha, 1f, 0.2f);
		}
		else
		{
			markAlpha = Mathf.Lerp(markAlpha, 0f, 0.1f);
		}
		if (DEBUGLABELS != null)
		{
			DEBUGLABELS[0].label.text = scavenger.movMode.ToString() + " " + scavenger.AI.behavior.ToString() + " " + ((scavenger.animation != null) ? scavenger.animation.id.ToString() : "none");
			DEBUGLABELS[1].label.text = scavenger.commitedToMove.type.ToString() + " " + scavenger.commitToMoveCounter + " " + scavenger.drop + " idl: " + scavenger.AI.idleCounter;
			DEBUGLABELS[3].label.text = "AL: " + (float.IsNaN(scavenger.AI.filteredLikeB) ? "~" : ((int)(Mathf.Lerp(-1f, 1f, scavenger.AI.filteredLikeB) * 100f)).ToString()) + "  TL: " + (int)(scavenger.AI.tempLikeB * 100f) + "  L: " + (int)(scavenger.AI.likeB * 100f);
			DEBUGLABELS[2].label.text = "agitation: " + (int)(scavenger.AI.agitation * 100f) + " Scared: " + (int)(scavenger.AI.scared * 100f);
		}
		if (scavenger.knucklePos.HasValue && scavenger.movMode == Scavenger.MovementMode.Run)
		{
			float num = Custom.LerpMap(Mathf.Abs(scavenger.mainBodyChunk.pos.x - scavenger.knucklePos.Value.x), 20f, 0f, 0f, 10f);
			if (riseBody < num)
			{
				riseBody = Mathf.Min(num, riseBody + 2f);
			}
			else
			{
				riseBody = Mathf.Lerp(riseBody, num, 0.2f);
			}
		}
		else
		{
			riseBody = Mathf.Max(0f, riseBody - 0.5f);
		}
		if (tail.Length != 0)
		{
			tail[0].connectedPoint = drawPositions[hipsDrawPos, 0];
			float2 @float = drawPositions[hipsDrawPos, 0];
			float2 float2 = drawPositions[hipsDrawPos - 1, 0];
			float num2 = 28f;
			for (int i = 0; i < tail.Length; i++)
			{
				tail[i].Update();
				tail[i].vel *= Mathf.Lerp(1f, 0.5f, base.owner.bodyChunks[1].submersion);
				tail[i].vel.y -= 0.9f * (1f - base.owner.bodyChunks[1].submersion) * base.owner.room.gravity;
				if (!Custom.DistLess(tail[i].pos, base.owner.bodyChunks[1].pos, 9f * (float)(i + 1)))
				{
					tail[i].pos = base.owner.bodyChunks[1].pos + Custom.DirVec(base.owner.bodyChunks[1].pos, tail[i].pos) * 9f * (i + 1);
				}
				tail[i].vel += Custom.DirVec(float2, tail[i].pos) * num2 / math.distance(float2, tail[i].pos);
				num2 *= 0.25f;
				float2 = @float;
				@float = tail[i].pos;
			}
		}
		for (int j = 0; j < 2; j++)
		{
			hands[j].Update();
		}
		for (int k = 0; k < 2; k++)
		{
			legs[k].Update();
		}
		lastFlip = flip;
		flip = scavenger.flip;
		lastCycle = cycle;
		if (scavenger.moving)
		{
			cycle += Mathf.Sign(flip) * 0.1f;
		}
		lastSwimArms = swimArms;
		if (scavenger.movMode == Scavenger.MovementMode.Swim || scavenger.Pointing)
		{
			swimArms = Mathf.Min(swimArms + 0.1f, 1f);
		}
		else
		{
			swimArms = Mathf.Max(swimArms - 0.1f, 0f);
		}
		lastBodyAxis = bodyAxis;
		bodyAxis = Vector3Ext.Slerp2(bodyAxis, (drawPositions[chestDrawPos, 0] - drawPositions[hipsDrawPos, 0] + Custom.PerpendicularVector(drawPositions[chestDrawPos, 0], drawPositions[hipsDrawPos, 0]) * 10f * flip).normalized(), 0.1f);
		if (scavenger.movMode != Scavenger.MovementMode.Crawl && scavenger.movMode != Scavenger.MovementMode.Climb)
		{
			bodyAxis = Vector3Ext.Slerp2(bodyAxis, new float2(0f, 1f), 0.5f);
		}
		lastLookUp = lookUp;
		if (Custom.RotateAroundOrigo(HeadDir(1f), BodyAxis(1f)).y > 0.8f)
		{
			lookUp = Mathf.Min(1f, lookUp + 1f / Mathf.Lerp(5f, 20f, shiftingNeutralFace));
		}
		else
		{
			lookUp = Mathf.Max(0f, lookUp - 1f / Mathf.Lerp(20f, 5f, shiftingNeutralFace));
		}
		lastLookPoint = lookPoint;
		lookPoint = scavenger.EyesLookPoint;
		lastEyesOpen = eyesOpen;
		if (scavenger.Consious)
		{
			blink--;
			if (blink < 0)
			{
				eyesOpen = Mathf.Max(0f, eyesOpen - 0.5f);
				if (blink < -UnityEngine.Random.Range(2, 6))
				{
					blink = UnityEngine.Random.Range(4, (UnityEngine.Random.value < 0.5f) ? 12 : ((int)Mathf.Lerp(800f, 200f, scavenger.abstractCreature.personality.nervous)));
				}
			}
			else
			{
				eyesOpen = Mathf.Min(1f, eyesOpen + 0.05f + 0.95f * scavenger.abstractCreature.personality.energy);
				if (scavenger.Blinded && UnityEngine.Random.value < 0.1f)
				{
					blink = 0;
				}
			}
			lastNeutralFace = neutralFace;
			float num3 = Mathf.Lerp(200f, 600f, Mathf.Pow(shiftingNeutralFace, 1.7f));
			float num4 = Mathf.InverseLerp(num3, 0f, math.distance(scavenger.mainBodyChunk.pos, lookPoint));
			neutralFace = Mathf.Lerp(shiftingNeutralFace, num4, Custom.LerpMap(Mathf.Abs(num4 - num3 * 0.5f), 0f, num3 * 0.5f, 0.05f, 0.55f));
			if (shiftingNeutralFace < shiftingNeutralFaceGoal)
			{
				shiftingNeutralFace = Mathf.Min(shiftingNeutralFace + 0.025f, shiftingNeutralFaceGoal);
			}
			else
			{
				shiftingNeutralFace = Mathf.Max(shiftingNeutralFace - 0.025f, shiftingNeutralFaceGoal);
			}
			shiftingNeutralFace = Mathf.Lerp(shiftingNeutralFace, shiftingNeutralFaceGoal, 0.15f);
			if (UnityEngine.Random.value < 0.0125f)
			{
				shiftingNeutralFaceGoal = UnityEngine.Random.value;
			}
			if (shiftingNeutralFaceGoal > 0.2f && (scavenger.Rummaging || (scavenger.animation != null && scavenger.animation is Scavenger.AttentiveAnimation)))
			{
				shiftingNeutralFaceGoal *= 0.25f;
			}
		}
		else
		{
			eyesOpen = Mathf.Lerp(eyesOpen, 0.5f, 0.1f);
		}
		lastEyesPop = eyesPop;
		if (eyesPop > 0f)
		{
			eyesPop = Mathf.Max(0f, eyesPop - 1f / ((eyesPop > 0.5f || scavenger.animation == null || scavenger.animation.id != Scavenger.ScavengerAnimation.ID.Look) ? eyesPopGoBack : (eyesPopGoBack * 2f)));
			eyesOpen = 1f;
		}
		if (scavenger.AI.scared > 0.8f && scavenger.abstractCreature.personality.bravery < 0.3f)
		{
			eyesPop = Mathf.Lerp(eyesPop, UnityEngine.Random.value * Mathf.InverseLerp(0.8f, 1f, scavenger.AI.scared), 0.1f);
		}
		lastShake = shake;
		if (shake > 0f)
		{
			shake = Mathf.Max(Mathf.InverseLerp(0.6f, 1f, scavenger.AI.scared * (1f - scavenger.abstractCreature.personality.bravery)) * 0.3f, shake - 0.1f);
		}
		lastBristle = bristle;
		if (bristle > 0f)
		{
			bristle = Mathf.Max(0f, bristle - (1f - scavenger.AI.agitation) / 30f);
		}
		if (scavenger.dead)
		{
			eyesPop = 0f;
			bristle = 0f;
			shake = 0f;
		}
		if (scavenger.Blinded)
		{
			eyesPop = 0f;
		}
		for (int l = 0; l < drawPositions.GetLength(0); l++)
		{
			drawPositions[l, 1] = drawPositions[l, 0];
		}
		drawPositions[0, 0] = scavenger.bodyChunks[2].pos;
		drawPositions[2, 0] = scavenger.bodyChunks[0].pos;
		drawPositions[4, 0] = scavenger.bodyChunks[1].pos;
		if (scavenger.Consious)
		{
			float2 float3 = Custom.DirVec(lookPoint, drawPositions[2, 0]) * Custom.LerpMap(math.distance(lookPoint, drawPositions[2, 0]), 50f, 400f, 11f, 0f, 0.5f);
			drawPositions[2, 0] = new float2(drawPositions[2, 0].x + float3.x, drawPositions[2, 0].y + float3.y + riseBody);
		}
		if (scavenger.movMode == Scavenger.MovementMode.Climb && !scavenger.swingPos.HasValue)
		{
			float num5 = math.distance(drawPositions[4, 0], drawPositions[2, 0]);
			float2 float4 = drawPositions[2, 0];
			for (int m = 0; m < 2; m++)
			{
				drawPositions[2, 0] += Custom.DirVec(scavenger.mainBodyChunk.pos, hands[m].pos).ToF2() * Custom.LerpMap(math.distance(scavenger.mainBodyChunk.pos, hands[m].pos), 5f, 40f, 5f, -7.5f);
			}
			drawPositions[2, 0] = math.lerp(drawPositions[2, 0], drawPositions[4, 0] + Custom.DirVec(drawPositions[4, 0], drawPositions[2, 0]) * num5, 0.5f);
			drawPositions[1, 0] += float4 - drawPositions[2, 0];
		}
		if (scavenger.Consious && scavenger.abstractCreature.personality.nervous > 0.8f && UnityEngine.Random.value < 0.5f + 0.5f * scavenger.abstractCreature.personality.nervous)
		{
			drawPositions[2, 0] += Custom.RNVf2() * Mathf.InverseLerp(0.8f, 1f, scavenger.abstractCreature.personality.nervous) * 1.5f * UnityEngine.Random.value;
		}
		drawPositions[3, 0] = (drawPositions[2, 0] + drawPositions[4, 0]) / 2f;
		drawPositions[3, 0] -= Custom.PerpendicularVector(drawPositions[4, 0], drawPositions[2, 0]) * Mathf.Lerp(5f, 15f, iVars.narrowWaist) * scavenger.flip;
		drawPositions[3, 0] = math.lerp(drawPositions[3, 0], drawPositions[4, 0], 0.25f);
		drawPositions[1, 0] = scavenger.bodyChunks[0].pos.ToF2() + Custom.DirVec(drawPositions[3, 0], drawPositions[chestDrawPos, 0]) * 8f;
		for (int n = 0; n < 2; n++)
		{
			drawPositions[4, 0] += Custom.DirVec(drawPositions[4, 0], legs[n].pos.ToF2());
		}
		if (shake > 0f)
		{
			for (int num6 = 0; num6 < drawPositions.GetLength(0); num6++)
			{
				drawPositions[num6, 0] += Custom.RNVf2() * UnityEngine.Random.value * shake * 7f * Custom.LerpMap(math.distance(scavenger.mainBodyChunk.lastPos, scavenger.mainBodyChunk.pos), 2f, 8f, 1f, 4f);
			}
		}
		spineLengths[1] = spineLengths[0];
		spineLengths[0] = 0f;
		for (int num7 = 0; num7 < drawPositions.GetLength(0) - 1; num7++)
		{
			spineLengths[0] = Mathf.Clamp(spineLengths[0] + math.distance(drawPositions[num7, 0], drawPositions[num7 + 1, 0]), 0f, 200f);
		}
		if (ModManager.MSC)
		{
			if (maskGfx != null)
			{
				float2 f = HeadDir(0f);
				float num8 = BodyAxis(0f);
				float num9 = Mathf.Lerp(lastLookUp, lookUp, 0f);
				float num10 = Mathf.Lerp(lastNeutralFace, neutralFace, 0f);
				num9 *= 1f - num10;
				float2 f2 = math.lerp(f.normalized(), -Custom.DegToFloat2(0f - num8), Mathf.Lerp(0.5f, 1f, Mathf.Max(Mathf.Pow(num9, 1.1f), num10))).normalized();
				maskGfx.rotationA = Vector3Ext.Slerp2(maskGfx.rotationA, -f2.normalized(), 0.5f);
				maskGfx.rotationB = new float2(0f, 1f);
				maskGfx.Update();
			}
			for (int num11 = 0; num11 < shells.Length; num11++)
			{
				shells[num11].Update();
			}
		}
	}

	public int ContainerForHeldItem(PhysicalObject item, int grasp)
	{
		if (grasp == 0)
		{
			return 2;
		}
		if (!(item is Spear))
		{
			return 1;
		}
		return 0;
	}

	public void ShockReaction(float intensity)
	{
		if (!scavenger.dead && scavenger.stun <= 20)
		{
			eyesPop = Mathf.Max(eyesPop, Mathf.Pow(intensity, 0.5f));
			eyesPopGoBack = Mathf.Max(eyesPopGoBack, Mathf.Lerp(2f, 20f, Mathf.Pow(intensity, Mathf.Lerp(0.8f, 2f, scavenger.abstractCreature.personality.bravery))));
			shake = Mathf.Max(shake, Mathf.Max(0f, intensity - Custom.LerpMap(scavenger.abstractCreature.personality.bravery, 0.75f, 1f, 0f, 0.5f)));
			blink = UnityEngine.Random.Range(10, 40);
			bristle = Mathf.Max(bristle, scavenger.AI.agitation, intensity);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[ChestSprite] = new FSprite("Circle20");
		sLeaser.sprites[ChestSprite].scaleX = base.owner.bodyChunks[0].rad * Mathf.Lerp(0.7f, 1.3f, iVars.fatness) / 10f;
		sLeaser.sprites[ChestSprite].scaleY = (base.owner.bodyChunks[0].rad + Mathf.Lerp(2f, 1.5f, iVars.narrowWaist)) / 10f;
		sLeaser.sprites[ChestSprite].anchorY = 0.4f + 0.05f * iVars.narrowWaist;
		sLeaser.sprites[HipSprite] = new FSprite("Circle20");
		sLeaser.sprites[HipSprite].scale = base.owner.bodyChunks[1].rad / 15f;
		sLeaser.sprites[HeadSprite] = new FSprite("Circle20");
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < ((!(iVars.pupilSize > 0f)) ? 1 : 2); j++)
			{
				sLeaser.sprites[EyeSprite(i, j)] = new FSprite("Circle20");
			}
		}
		TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[6];
		for (int k = 0; k < 6; k++)
		{
			array[k] = new TriangleMesh.Triangle(k, k + 1, k + 2);
		}
		sLeaser.sprites[WaistSprite] = new TriangleMesh("Futile_White", array, customColor: false);
		sLeaser.sprites[NeckSprite] = TriangleMesh.MakeLongMesh(4, pointyTip: false, customColor: true);
		for (int l = 0; l < 2; l++)
		{
			hands[l].InitiateSprites(sLeaser, rCam);
		}
		for (int m = 0; m < 2; m++)
		{
			legs[m].InitiateSprites(sLeaser, rCam);
		}
		eartlers.InitiateSprites(sLeaser, rCam);
		array = new TriangleMesh.Triangle[teeth.GetLength(0) * 3];
		for (int n = 0; n < teeth.GetLength(0); n++)
		{
			array[n * 3] = new TriangleMesh.Triangle(n * 5, n * 5 + 1, n * 5 + 2);
			array[n * 3 + 1] = new TriangleMesh.Triangle(n * 5 + 1, n * 5 + 2, n * 5 + 3);
			array[n * 3 + 2] = new TriangleMesh.Triangle(n * 5 + 2, n * 5 + 3, n * 5 + 4);
		}
		sLeaser.sprites[TeethSprite] = new TriangleMesh("Futile_White", array, customColor: false);
		array = new TriangleMesh.Triangle[6];
		for (int num = 0; num < 6; num++)
		{
			array[num] = new TriangleMesh.Triangle(num, num + 1, num + 2);
		}
		sLeaser.sprites[ChestPatchSprite] = new TriangleMesh("Futile_White", array, customColor: false);
		if (ModManager.MSC)
		{
			if (maskGfx != null)
			{
				maskGfx.InitiateSprites(sLeaser, rCam);
			}
			for (int num2 = 0; num2 < shells.Length; num2++)
			{
				shells[num2].InitiateSprites(sLeaser, rCam);
			}
			if (scavenger.King)
			{
				sLeaser.sprites[TotalSprites - 1] = new FSprite("pixel");
				sLeaser.sprites[TotalSprites - 1].scale = 5f;
				sLeaser.sprites[TotalSprites - 2] = new FSprite("Futile_White");
				sLeaser.sprites[TotalSprites - 2].shader = rCam.game.rainWorld.Shaders["FlatLight"];
			}
		}
		sLeaser.containers = new FContainer[3];
		for (int num3 = 0; num3 < sLeaser.containers.Length; num3++)
		{
			sLeaser.containers[num3] = new FContainer();
		}
		base.InitiateSprites(sLeaser, rCam);
		AddToContainer(sLeaser, rCam, null);
		scavenger.PlaceAllGrabbedObjectsInCorrectContainers();
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		newContatiner.AddChild(sLeaser.containers[0]);
		for (int i = 0; i < FirstInFrontLimbSprite; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
		if (ModManager.MSC)
		{
			if (scavenger.King)
			{
				FContainer fContainer = rCam.ReturnFContainer("Foreground");
				for (int j = ShellSprite; j < TotalSprites - 2; j++)
				{
					newContatiner.AddChild(sLeaser.sprites[j]);
				}
				for (int k = TotalSprites - 2; k < TotalSprites; k++)
				{
					fContainer.AddChild(sLeaser.sprites[k]);
				}
			}
			else
			{
				for (int l = ShellSprite; l < TotalSprites; l++)
				{
					newContatiner.AddChild(sLeaser.sprites[l]);
				}
			}
		}
		for (int m = FirstInFrontLimbSprite; m < FirstInFrontLimbSprite + 2; m++)
		{
			newContatiner.AddChild(sLeaser.sprites[m]);
		}
		newContatiner.AddChild(sLeaser.containers[1]);
		for (int n = FirstInFrontLimbSprite + 2; n < (ModManager.MSC ? ShellSprite : TotalSprites); n++)
		{
			newContatiner.AddChild(sLeaser.sprites[n]);
		}
		newContatiner.AddChild(sLeaser.containers[2]);
		if (ModManager.MSC && maskGfx != null)
		{
			maskGfx.AddToContainer(sLeaser, rCam, null);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPosV2)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPosV2);
		float2 @float = camPosV2.ToF2();
		float num = rCam.PaletteDarkness();
		if (num > 0.5f)
		{
			num -= rCam.room.LightSourceExposure(scavenger.mainBodyChunk.pos) * Custom.LerpMap(num, 0.5f, 1f, 0f, 0.5f);
		}
		if (num != darkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		float num2 = Mathf.Lerp(lastFlip, flip, timeStacker);
		float2 float2 = math.lerp(drawPositions[headDrawPos, 1], drawPositions[headDrawPos, 0], timeStacker);
		float2 float3 = math.lerp(drawPositions[chestDrawPos, 1], drawPositions[chestDrawPos, 0], timeStacker);
		float2 float4 = math.lerp(drawPositions[hipsDrawPos, 1], drawPositions[hipsDrawPos, 0], timeStacker);
		float2 float5 = (float3 + float4) / 2f;
		float5 -= Custom.PerpendicularVector((float4 - float3).normalized()) * Mathf.Lerp(5f, 15f, iVars.narrowWaist) * num2;
		sLeaser.sprites[ChestSprite].x = float3.x - @float.x;
		sLeaser.sprites[ChestSprite].y = float3.y - @float.y;
		sLeaser.sprites[ChestSprite].rotation = Custom.AimFromOneVectorToAnother(float3, float5);
		sLeaser.sprites[HipSprite].x = float4.x - @float.x;
		sLeaser.sprites[HipSprite].y = float4.y - @float.y;
		float2 float6 = Custom.DirVec(float4, float3);
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(0, float4 + Custom.PerpendicularVector((float4 - float5).normalized()) * scavenger.bodyChunks[1].rad * Mathf.Lerp(0.65f, 0.9f, iVars.WaistWidth) - @float);
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(1, float4 - Custom.PerpendicularVector((float4 - float5).normalized()) * scavenger.bodyChunks[1].rad * Mathf.Lerp(0.65f, 0.9f, iVars.WaistWidth) - @float);
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(2, math.lerp(float5, (float3 + float4) / 2f, 0.4f) - float6 * 4f + Custom.PerpendicularVector(-float6) * scavenger.mainBodyChunk.rad * Mathf.Lerp(0.35f, 0.9f, Mathf.Pow(iVars.WaistWidth, 1.3f)) - @float);
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(3, math.lerp(float5, (float3 + float4) / 2f, 0.4f) - float6 * 4f - Custom.PerpendicularVector(-float6) * scavenger.mainBodyChunk.rad * Mathf.Lerp(0.35f, 0.9f, Mathf.Pow(iVars.WaistWidth, 1.3f)) - @float);
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(4, math.lerp(float5, (float3 + float4) / 2f, 0.4f) + float6 * 4f + Custom.PerpendicularVector(-float6) * scavenger.mainBodyChunk.rad * Mathf.Lerp(0.25f, 0.8f, Mathf.Pow(iVars.WaistWidth, 1.3f)) - @float);
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(5, math.lerp(float5, (float3 + float4) / 2f, 0.4f) + float6 * 4f - Custom.PerpendicularVector(-float6) * scavenger.mainBodyChunk.rad * Mathf.Lerp(0.25f, 0.8f, Mathf.Pow(iVars.WaistWidth, 1.3f)) - @float);
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(6, float3 + Custom.PerpendicularVector((float5 - float3).normalized()) * scavenger.mainBodyChunk.rad * Mathf.Lerp(0.7f, 1.3f, iVars.fatness) - @float);
		(sLeaser.sprites[WaistSprite] as TriangleMesh).MoveVertice(7, float3 - Custom.PerpendicularVector((float5 - float3).normalized()) * scavenger.mainBodyChunk.rad * Mathf.Lerp(0.7f, 1.3f, iVars.fatness) - @float);
		float2 float7 = float3 + Custom.DirVec(float5, float3) * 5f;
		float2 float8 = float7;
		float num3 = scavenger.mainBodyChunk.rad;
		float num4 = Mathf.InverseLerp(0f, 10f, Custom.DistanceToLine(float2, float4, float3) * (0f - num2));
		for (int i = 0; i < 4; i++)
		{
			float num5 = (float)i / 3f;
			float2 float9 = math.lerp(float7, float2, num5);
			float9 += Custom.PerpendicularVector((float7 - float2).normalized()) * Mathf.Sin(num5 * (float)Math.PI) * 3f * num4 * num2;
			float2 float10 = (float8 - float9).normalized();
			float2 float11 = Custom.PerpendicularVector(float10);
			float num6 = math.distance(float9, float8) / 4f;
			float num7 = Mathf.Lerp(7f, 3f, num5) - 2f * Mathf.Sin(num5 * (float)Math.PI) * Mathf.Lerp(0.5f, 1.5f, iVars.neckThickness);
			(sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(i * 4, float8 - float11 * (num3 + num7) * 0.5f - float10 * num6 - @float);
			(sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(i * 4 + 1, float8 + float11 * (num3 + num7) * 0.5f - float10 * num6 - @float);
			(sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(i * 4 + 2, float9 - float11 * num7 + float10 * num6 - @float);
			(sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(i * 4 + 3, float9 + float11 * num7 + float10 * num6 - @float);
			float8 = float9;
			num3 = num7;
		}
		sLeaser.sprites[HeadSprite].x = float2.x - @float.x;
		sLeaser.sprites[HeadSprite].y = float2.y - @float.y;
		if (scavenger.King)
		{
			sLeaser.sprites[TotalSprites - 1].x = float2.x - @float.x;
			sLeaser.sprites[TotalSprites - 1].y = float2.y - @float.y + 32f;
			sLeaser.sprites[TotalSprites - 1].alpha = Mathf.Lerp(lastMarkAlpha, markAlpha, timeStacker);
			sLeaser.sprites[TotalSprites - 2].x = float2.x - @float.x;
			sLeaser.sprites[TotalSprites - 2].y = float2.y - @float.y + 32f;
			sLeaser.sprites[TotalSprites - 2].alpha = 0.2f * Mathf.Lerp(lastMarkAlpha, markAlpha, timeStacker);
			sLeaser.sprites[TotalSprites - 2].scale = 1f + Mathf.Lerp(lastMarkAlpha, markAlpha, timeStacker);
		}
		float2 f = HeadDir(timeStacker);
		float num8 = BodyAxis(timeStacker);
		float num9 = Mathf.Lerp(lastLookUp, lookUp, timeStacker);
		float num10 = Mathf.Lerp(lastNeutralFace, neutralFace, timeStacker);
		num9 *= 1f - num10;
		float2 f2 = math.lerp(f.normalized(), -Custom.DegToFloat2(0f - num8), Mathf.Lerp(0.5f, 1f, Mathf.Max(Mathf.Pow(num9, 1.1f), num10))).normalized();
		float2 float12 = Custom.RotateAroundOrigo(f2.normalized(), num8);
		f = f.normalized();
		f2 = f2.normalized();
		float num11 = Mathf.Lerp(0.6f, 1.1f, iVars.headSize);
		sLeaser.sprites[HeadSprite].rotation = Custom.VecToDeg(f2.normalized());
		if (ModManager.MSC)
		{
			if (maskGfx != null && !scavenger.readyToReleaseMask)
			{
				float num12 = Custom.VecToDeg(f);
				if (num12 < 30f && num12 > -30f)
				{
					maskGfx.overrideAnchorVector = -f.normalized();
					maskGfx.overrideDrawVector = new float2(float2.x, float2.y + 4f);
				}
				else if (num12 <= 90f && num12 >= -90f)
				{
					maskGfx.overrideAnchorVector = -f2.normalized();
					maskGfx.overrideDrawVector = new float2(float2.x, float2.y + 1f);
				}
				else
				{
					maskGfx.overrideAnchorVector = -f2.normalized();
					maskGfx.overrideDrawVector = float2;
				}
				maskGfx.DrawSprites(sLeaser, rCam, timeStacker, @float);
			}
			if (scavenger.readyToReleaseMask && maskGfx != null)
			{
				maskGfx.SetVisible(sLeaser, visible: false);
			}
		}
		float2 float13 = Vector3Ext.Slerp2(f2.normalized(), -f.normalized(), num9);
		float2 float14 = float2 + float13 * (4f - Mathf.Max(10f * Mathf.Pow(num9, 0.5f), 3f * num10)) * num11;
		float2 float15 = float14 + float13 * Mathf.Lerp(-5f, 60f, iVars.eyesAngle) * Mathf.Lerp(lastEyesOpen, eyesOpen, timeStacker);
		float num13 = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastEyesPop, eyesPop, timeStacker)), 0.25f);
		float num14 = Mathf.Clamp(iVars.eyeSize + Mathf.Lerp(0.5f, 0.25f, num9) * num13, 0f, 1f);
		for (int j = 0; j < 2; j++)
		{
			float value = ((j == 0) ? (-1f) : 1f) * 0.5f + float12.x * (1f - num10);
			value = Mathf.Clamp(value, -1f, 1f);
			float2 float16 = float14 + Custom.PerpendicularVector(float13) * 8f * num11 * value;
			sLeaser.sprites[EyeSprite(j, 0)].x = float16.x - @float.x;
			sLeaser.sprites[EyeSprite(j, 0)].y = float16.y - @float.y;
			float num15 = Custom.AimFromOneVectorToAnother(float16, float15);
			float num16 = Mathf.Lerp(1.5f, 2f, Mathf.Pow(num9, 1.5f)) * Mathf.Lerp(0.3f, 1.5f, Mathf.Pow(num14, 0.7f)) * (1f + 0.2f * num13 * (1f - num9)) * Mathf.InverseLerp(1f, 0.7f, Mathf.Abs(value)) * Mathf.Lerp(1f, Mathf.Lerp(0.5f, 0.25f, num14), iVars.narrowEyes * Mathf.Lerp(1f, 0.5f, num13)) * Mathf.Lerp(lastEyesOpen, eyesOpen, timeStacker);
			float num17 = Mathf.Lerp(2.5f, 1.5f, Mathf.Pow(num9, 0.5f)) * Mathf.Lerp(0.3f, 1.5f, Mathf.Pow(num14, 0.7f)) * (1f + 0.2f * num13) * Mathf.InverseLerp(0f, 0.75f, Mathf.Lerp(lastEyesOpen, eyesOpen, timeStacker));
			if (iVars.pupilSize > 0f)
			{
				float2 vec = ((!iVars.deepPupils) ? (Custom.DirVec(float16, lookPoint) * Mathf.InverseLerp(0f, 30f, math.distance(float16, lookPoint)) * Mathf.InverseLerp(0.3f, 0.7f, scavenger.abstractCreature.personality.sympathy)) : (-f));
				vec = Custom.RotateAroundOrigo(vec, num15);
				vec = new float2(vec.x * num16 * (1f - iVars.pupilSize), vec.y * num17 * (1f - iVars.pupilSize));
				vec = Custom.RotateAroundOrigo(vec, 0f - num15);
				sLeaser.sprites[EyeSprite(j, 1)].x = float16.x + vec.x - @float.x;
				sLeaser.sprites[EyeSprite(j, 1)].y = float16.y + vec.y - @float.y;
				sLeaser.sprites[EyeSprite(j, 1)].scaleX = num16 * 0.1f * iVars.pupilSize * (1f - 0.5f * num13);
				sLeaser.sprites[EyeSprite(j, 1)].scaleY = num17 * 0.1f * iVars.pupilSize * (1f - 0.5f * num13);
				sLeaser.sprites[EyeSprite(j, 1)].rotation = num15;
			}
			sLeaser.sprites[EyeSprite(j, 0)].scaleX = num16 * 0.1f;
			sLeaser.sprites[EyeSprite(j, 0)].scaleY = num17 * 0.1f;
			sLeaser.sprites[EyeSprite(j, 0)].rotation = num15;
		}
		if (scavenger.blind >= 10)
		{
			if (scavenger.blind == 10)
			{
				ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			}
			else
			{
				for (int k = 0; k < 2; k++)
				{
					for (int l = 0; l < ((!(iVars.pupilSize > 0f)) ? 1 : 2); l++)
					{
						sLeaser.sprites[EyeSprite(k, l)].color = new Color(0.3f, 0.3f, 0.3f);
					}
				}
			}
		}
		sLeaser.sprites[HeadSprite].scaleX = Mathf.Lerp(8f, 9f, Mathf.Pow(num9, 0.5f)) * num11 / 10f;
		sLeaser.sprites[HeadSprite].scaleY = Mathf.Lerp(11f, 8f, Mathf.Pow(num9, 0.5f)) * num11 / 10f;
		for (int m = 0; m < teeth.GetLength(0); m++)
		{
			float num18 = (float)m / (float)(teeth.GetLength(0) - 1);
			float2 float17 = float14 + float13 * 4f + Custom.PerpendicularVector(float13) * Mathf.Lerp(-3f, 3f, num18) * float12.x;
			float2 float18 = float14 + float13 * Mathf.Lerp(8f, 10f, Mathf.Sin(num18 * (float)Math.PI)) * teeth[m, 0] + Custom.PerpendicularVector(float13) * Mathf.Lerp(Mathf.Lerp(-9f, 9f, num18) * Mathf.Lerp(0.5f, 1.2f, iVars.wideTeeth), -2f * Mathf.Sign(float12.x), 1f - Mathf.Abs(float12.y)) * teeth[m, 0];
			float2 float19 = float14 + float13 * Mathf.Lerp(12f, 15f, Mathf.Sin(num18 * (float)Math.PI)) * teeth[m, 0] + Custom.PerpendicularVector(float13) * Mathf.Lerp(Mathf.Lerp(-9f, 9f, num18) * Mathf.Lerp(0.5f, 1.2f, iVars.wideTeeth), -15f * Mathf.Sign(float12.x), 1f - Mathf.Abs(float12.y)) * teeth[m, 0];
			(sLeaser.sprites[TeethSprite] as TriangleMesh).MoveVertice(m * 5, float17 - Custom.PerpendicularVector(float17, float18) * 1f - @float);
			(sLeaser.sprites[TeethSprite] as TriangleMesh).MoveVertice(m * 5 + 1, float17 + Custom.PerpendicularVector(float17, float18) * 1f - @float);
			(sLeaser.sprites[TeethSprite] as TriangleMesh).MoveVertice(m * 5 + 2, float18 - Custom.PerpendicularVector(float18, float19) * teeth[m, 1] - @float);
			(sLeaser.sprites[TeethSprite] as TriangleMesh).MoveVertice(m * 5 + 3, float18 + Custom.PerpendicularVector(float18, float19) * teeth[m, 1] - @float);
			(sLeaser.sprites[TeethSprite] as TriangleMesh).MoveVertice(m * 5 + 4, float19 - @float);
		}
		for (int n = 0; n < chestPatchShape.Length; n++)
		{
			(sLeaser.sprites[ChestPatchSprite] as TriangleMesh).MoveVertice(n, OnBellySurfacePos(chestPatchShape[n], timeStacker) - @float);
		}
		for (int num19 = 0; num19 < 2; num19++)
		{
			hands[num19].DrawSprites(sLeaser, rCam, timeStacker, @float);
		}
		for (int num20 = 0; num20 < 2; num20++)
		{
			legs[num20].DrawSprites(sLeaser, rCam, timeStacker, @float);
		}
		eartlers.DrawSprites(sLeaser, rCam, timeStacker, @float, float2, Vector3Ext.Slerp2(f2, -f, num9), num9);
		if (!ModManager.MSC)
		{
			return;
		}
		for (int num21 = 0; num21 < Mathf.Min(scavenger.armorPieces, shells.Length); num21++)
		{
			switch (num21)
			{
			case 2:
				shells[num21].pos = float3;
				shells[num21].scaleX = 1.25f;
				shells[num21].scaleY = 1f;
				shells[num21].rotation = 0f;
				shells[num21].zRotation = 90f;
				break;
			case 1:
				shells[num21].pos = float5;
				shells[num21].scaleX = 0.75f;
				shells[num21].scaleY = 0.75f;
				shells[num21].rotation = 0f;
				shells[num21].zRotation = 90f;
				break;
			case 0:
				shells[num21].pos = float4;
				shells[num21].scaleX = 1f;
				shells[num21].scaleY = 0.75f;
				shells[num21].rotation = 0f;
				shells[num21].zRotation = 90f;
				break;
			}
			shells[num21].DrawSprites(sLeaser, rCam, timeStacker, @float);
		}
		for (int num22 = Mathf.Min(scavenger.armorPieces, shells.Length); num22 < shells.Length; num22++)
		{
			shells[num22].visible = false;
			shells[num22].DrawSprites(sLeaser, rCam, timeStacker, @float);
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		darkness = rCam.PaletteDarkness();
		if (darkness > 0.5f)
		{
			darkness -= rCam.room.LightSourceExposure(scavenger.mainBodyChunk.pos) * Custom.LerpMap(darkness, 0.5f, 1f, 0f, 0.5f);
		}
		blackColor = palette.blackColor;
		if (ModManager.MSC)
		{
			if (maskGfx != null)
			{
				maskGfx.ApplyPalette(sLeaser, rCam, palette);
			}
			for (int i = 0; i < shells.Length; i++)
			{
				shells[i].ApplyPalette(sLeaser, rCam, palette);
			}
		}
		Color blendedBodyColor = BlendedBodyColor;
		Color blendedHeadColor = BlendedHeadColor;
		sLeaser.sprites[ChestSprite].color = blendedBodyColor;
		sLeaser.sprites[HipSprite].color = blendedBodyColor;
		sLeaser.sprites[WaistSprite].color = blendedBodyColor;
		for (int j = 0; j < 5; j++)
		{
			sLeaser.sprites[FirstBehindLimbSprite + j].color = blendedBodyColor;
			sLeaser.sprites[FirstInFrontLimbSprite + j].color = blendedBodyColor;
		}
		sLeaser.sprites[NeckSprite].color = blendedBodyColor;
		for (int k = 0; k < 4; k++)
		{
			(sLeaser.sprites[NeckSprite] as TriangleMesh).verticeColors[k * 4] = Color.Lerp(blendedBodyColor, blendedHeadColor, ((float)k - 0.5f) / 3f);
			(sLeaser.sprites[NeckSprite] as TriangleMesh).verticeColors[k * 4 + 1] = Color.Lerp(blendedBodyColor, blendedHeadColor, ((float)k - 0.5f) / 3f);
			(sLeaser.sprites[NeckSprite] as TriangleMesh).verticeColors[k * 4 + 2] = Color.Lerp(blendedBodyColor, blendedHeadColor, (float)k / 3f);
			(sLeaser.sprites[NeckSprite] as TriangleMesh).verticeColors[k * 4 + 3] = Color.Lerp(blendedBodyColor, blendedHeadColor, (float)k / 3f);
		}
		if (iVars.handsHeadColor > 0f)
		{
			for (int l = 0; l < 2; l++)
			{
				for (int m = 0; m < 4; m++)
				{
					for (int n = 7; n < 11; n++)
					{
						(sLeaser.sprites[hands[l].firstSprite] as TriangleMesh).verticeColors[n] = Color.Lerp(blendedBodyColor, blendedHeadColor, iVars.handsHeadColor);
					}
					(sLeaser.sprites[hands[l].firstSprite] as TriangleMesh).verticeColors[6] = Color.Lerp(blendedBodyColor, blendedHeadColor, 0.5f * iVars.handsHeadColor);
					(sLeaser.sprites[hands[l].firstSprite] as TriangleMesh).verticeColors[11] = Color.Lerp(blendedBodyColor, blendedHeadColor, 0.5f * iVars.handsHeadColor);
					(sLeaser.sprites[hands[l].firstSprite] as TriangleMesh).verticeColors[5] = Color.Lerp(blendedBodyColor, blendedHeadColor, 0.2f * iVars.handsHeadColor);
					(sLeaser.sprites[hands[l].firstSprite] as TriangleMesh).verticeColors[12] = Color.Lerp(blendedBodyColor, blendedHeadColor, 0.2f * iVars.handsHeadColor);
				}
				sLeaser.sprites[hands[l].firstSprite + 1].color = Color.Lerp(blendedBodyColor, blendedHeadColor, iVars.handsHeadColor);
			}
		}
		sLeaser.sprites[HeadSprite].color = blendedHeadColor;
		eartlers.ApplyPalette(sLeaser, rCam, palette, blendedHeadColor, BlendedDecorationColor);
		for (int num = 0; num < 2; num++)
		{
			sLeaser.sprites[EyeSprite(num, 0)].color = BlendedEyeColor;
			if (!(iVars.pupilSize > 0f))
			{
				continue;
			}
			if (iVars.deepPupils)
			{
				if (headColor.lightness * (1f - headColorBlack) > 0.1f)
				{
					sLeaser.sprites[EyeSprite(num, 1)].color = Color.Lerp(BlendedEyeColor, Color.Lerp(blendedBodyColor, blendedHeadColor, 0.5f), 0.5f);
					sLeaser.sprites[EyeSprite(num, 0)].color = Color.Lerp(BlendedEyeColor, new Color(1f, 1f, 1f), 0.5f);
				}
				else
				{
					sLeaser.sprites[EyeSprite(num, 0)].color = Color.Lerp(BlendedEyeColor, Color.Lerp(blendedBodyColor, blendedHeadColor, 0.5f), 0.5f);
					sLeaser.sprites[EyeSprite(num, 1)].color = Color.Lerp(BlendedEyeColor, new Color(1f, 1f, 1f), 0.5f);
				}
				continue;
			}
			switch ((!(iVars.generalMelanin < 0.5f)) ? iVars.coloredPupils : 0)
			{
			case 1:
				sLeaser.sprites[EyeSprite(num, 1)].color = Custom.HSL2RGB(bodyColor.hue, 1f, 0.35f);
				continue;
			case 2:
				sLeaser.sprites[EyeSprite(num, 1)].color = Custom.HSL2RGB(headColor.hue, 1f, 0.35f);
				continue;
			case 3:
				sLeaser.sprites[EyeSprite(num, 1)].color = Custom.HSL2RGB(decorationColor.hue, 1f, 0.35f);
				continue;
			}
			if (headColor.lightness * (1f - headColorBlack) > 0.1f)
			{
				sLeaser.sprites[EyeSprite(num, 1)].color = Color.Lerp(Custom.HSL2RGB(headColor.hue, headColor.saturation, 0.15f), blackColor, headColorBlack);
			}
			else
			{
				sLeaser.sprites[EyeSprite(num, 1)].color = blendedHeadColor;
			}
		}
		sLeaser.sprites[TeethSprite].color = blendedHeadColor;
		sLeaser.sprites[ChestPatchSprite].color = Color.Lerp(bellyColor.rgb, palette.blackColor, Mathf.Lerp(bellyColorBlack, 1f, darkness));
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public float2 OnSpinePos(float f, float timeStacker)
	{
		float num = OnSpineIndex(f, timeStacker);
		return math.lerp(SpineSegmentPos(Custom.IntClamp(Mathf.FloorToInt(num), 0, drawPositions.GetLength(0) - 1), timeStacker), SpineSegmentPos(Custom.IntClamp(Mathf.FloorToInt(num) + 1, 0, drawPositions.GetLength(0) - 1), timeStacker), num - Mathf.Floor(num));
	}

	private float2 SpineSegmentPos(int seg, float timeStacker)
	{
		if (seg == 0)
		{
			float2 @float = math.lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker);
			float num = Mathf.InverseLerp(0f, 10f, Custom.DistanceToLine(@float, SpineSegmentPos(hipsDrawPos, timeStacker), SpineSegmentPos(chestDrawPos, timeStacker)) * (0f - math.lerp(lastFlip, flip, timeStacker)));
			return @float + Custom.PerpendicularVector((math.lerp(drawPositions[1, 1], drawPositions[1, 0], timeStacker) - @float).normalized()) * 5f * num * math.lerp(lastFlip, flip, timeStacker);
		}
		return math.lerp(drawPositions[seg, 1], drawPositions[seg, 0], timeStacker);
	}

	public float2 OnBackSurfacePos(float2 relPos, float timeStacker)
	{
		float x = relPos.x;
		x = math.clamp(x - math.lerp(lastFlip, flip, timeStacker), -1f, 1f);
		return OnSpinePos(relPos.y, timeStacker) + OnSpinePerp(relPos.y, timeStacker) * OnSpineWidth(relPos.y, timeStacker) * 0.5f * x;
	}

	[Obsolete("Use float2 parameter function instead.")]
	public Vector2 OnBackSurfacePos(Vector2 relPos, float timeStacker)
	{
		float2 @float = OnBackSurfacePos(new float2(relPos.x, relPos.y), timeStacker);
		return new Vector2(@float.x, @float.y);
	}

	public float2 OnBellySurfacePos(float2 relPos, float timeStacker)
	{
		float x = relPos.x;
		x = Mathf.Clamp(x + Mathf.Lerp(lastFlip, flip, timeStacker), -1f, 1f);
		float y = relPos.y;
		y = Mathf.Lerp(y, 1f, Custom.LerpMap(math.dot((SpineSegmentPos(1, timeStacker) - SpineSegmentPos(0, timeStacker)).normalized(), (SpineSegmentPos(2, timeStacker) - SpineSegmentPos(1, timeStacker)).normalized()), 1f, -1f, 0f, 0.5f));
		return OnSpinePos(y, timeStacker) + OnSpinePerp(y, timeStacker) * OnSpineWidth(y, timeStacker) * 0.5f * x;
	}

	[Obsolete("Use float2 parameter function instead.")]
	public Vector2 OnBellySurfacePos(Vector2 relPos, float timeStacker)
	{
		float2 @float = OnBellySurfacePos(new float2(relPos.x, relPos.y), timeStacker);
		return new Vector2(@float.x, @float.y);
	}

	public float2 OnSpineDir(float f, float timeStacker)
	{
		float num = OnSpineIndex(f, timeStacker);
		return Vector3Ext.Slerp2(SpineSegmentDir(Mathf.FloorToInt(num), timeStacker), SpineSegmentDir(Mathf.FloorToInt(num) + 1, timeStacker), Mathf.InverseLerp(0.5f, 1f, num - Mathf.Floor(num)));
	}

	private float2 SpineSegmentDir(int seg, float timeStacker)
	{
		if (seg > drawPositions.GetLength(0) - 2)
		{
			return Custom.DirVec(SpineSegmentPos(drawPositions.GetLength(0) - 2, timeStacker), SpineSegmentPos(drawPositions.GetLength(0) - 1, timeStacker));
		}
		return Custom.DirVec(SpineSegmentPos(seg, timeStacker), SpineSegmentPos(seg + 1, timeStacker));
	}

	public float2 OnSpinePerp(float f, float timeStacker)
	{
		float num = OnSpineIndex(f, timeStacker);
		return Vector3Ext.Slerp2(SpineSegmentPerp(Mathf.FloorToInt(num), timeStacker), SpineSegmentPerp(Mathf.FloorToInt(num) + 1, timeStacker), Mathf.InverseLerp(0.5f, 1f, num - Mathf.Floor(num)));
	}

	private float2 SpineSegmentPerp(int seg, float timeStacker)
	{
		if (seg == 0)
		{
			float f = math.dot((SpineSegmentPos(1, timeStacker) - SpineSegmentPos(0, timeStacker)).normalized(), (SpineSegmentPos(2, timeStacker) - SpineSegmentPos(1, timeStacker)).normalized());
			return Vector3Ext.Slerp2(Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(seg, timeStacker), SpineSegmentPos(seg + 1, timeStacker))) * Mathf.Sign(f), SpineSegmentPerp(1, timeStacker), Mathf.InverseLerp(0.5f, 1f, 1f - Mathf.Abs(f)));
		}
		if (seg > drawPositions.GetLength(0) - 2)
		{
			return Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(drawPositions.GetLength(0) - 2, timeStacker), SpineSegmentPos(drawPositions.GetLength(0) - 1, timeStacker)));
		}
		return Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(seg, timeStacker), SpineSegmentPos(seg + 1, timeStacker)));
	}

	public float2 OnSpineOutwardsDir(float2 ps, float timeStacker)
	{
		float num = OnSpineIndex(ps.y, timeStacker);
		float dt = math.dot((SpineSegmentPos(1, timeStacker) - SpineSegmentPos(0, timeStacker)).normalized(), (SpineSegmentPos(2, timeStacker) - SpineSegmentPos(1, timeStacker)).normalized());
		float num2 = Mathf.Clamp(ps.x - Mathf.Lerp(lastFlip, flip, timeStacker), -1f, 1f);
		return Vector3Ext.Slerp2(Vector3Ext.Slerp2(SpineSegmentOutwardsDir(Mathf.FloorToInt(num), timeStacker, dt, num2), SpineSegmentOutwardsDir(Mathf.FloorToInt(num) + 1, timeStacker, dt, num2), Mathf.InverseLerp(0f, 1f, num - Mathf.Floor(num))) * math.sign(num2), new float2(ps.y, timeStacker), Mathf.Pow(1f - Mathf.Abs(num2), 3f));
	}

	[Obsolete("Use float2 parameter function instead.")]
	public Vector2 OnSpineOutwardsDir(Vector2 ps, float timeStacker)
	{
		float2 @float = OnSpineOutwardsDir(new float2(ps.x, ps.y), timeStacker);
		return new Vector2(@float.x, @float.y);
	}

	private float2 SpineSegmentOutwardsDir(int seg, float timeStacker, float dt, float psX)
	{
		if (seg == 0)
		{
			float2 @float = Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(seg, timeStacker), SpineSegmentPos(seg + 1, timeStacker)));
			return Vector3Ext.Slerp2(Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(seg + 1, timeStacker), SpineSegmentPos(seg + 2, timeStacker))), @float * math.sign(dt), math.pow(math.abs(dt), 3f));
		}
		if (seg > drawPositions.GetLength(0) - 2)
		{
			return Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(drawPositions.GetLength(0) - 2, timeStacker), SpineSegmentPos(drawPositions.GetLength(0) - 1, timeStacker)));
		}
		return Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(seg, timeStacker), SpineSegmentPos(seg + 1, timeStacker)));
	}

	public float2 OnSpineUpDir(float f, float timeStacker)
	{
		float num = OnSpineIndex(f, timeStacker);
		float num2 = Mathf.Lerp(lastFlip, flip, timeStacker);
		float dt = math.dot((SpineSegmentPos(1, timeStacker) - SpineSegmentPos(0, timeStacker)).normalized(), (SpineSegmentPos(2, timeStacker) - SpineSegmentPos(1, timeStacker)).normalized());
		return Vector3Ext.Slerp2(Vector3Ext.Slerp2(SpineSegmentUpDir(Mathf.FloorToInt(num), timeStacker, dt, num2), SpineSegmentUpDir(Mathf.FloorToInt(num) + 1, timeStacker, dt, num2), Mathf.InverseLerp(0f, 1f, num - Mathf.Floor(num))), -OnSpineDirForUps(f, timeStacker, dt), 1f - Mathf.Abs(num2));
	}

	private float2 SpineSegmentUpDir(int seg, float timeStacker, float dt, float uFlp)
	{
		if (seg == 0)
		{
			float2 @float = Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(seg, timeStacker), SpineSegmentPos(seg + 1, timeStacker))) * (0f - Mathf.Sign(uFlp));
			float f = Custom.DistanceToLine(SpineSegmentPos(0, timeStacker), SpineSegmentPos(1, timeStacker), SpineSegmentPos(2, timeStacker));
			return Vector3Ext.Slerp2(@float * Mathf.Sign(f) * Mathf.Sign(uFlp), -SpineSegmentDir(1, timeStacker), Mathf.Abs(dt));
		}
		if (seg > drawPositions.GetLength(0) - 2)
		{
			return Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(drawPositions.GetLength(0) - 2, timeStacker), SpineSegmentPos(drawPositions.GetLength(0) - 1, timeStacker))) * (0f - Mathf.Sign(uFlp));
		}
		return Custom.PerpendicularVector(Custom.DirVec(SpineSegmentPos(seg, timeStacker), SpineSegmentPos(seg + 1, timeStacker))) * (0f - Mathf.Sign(uFlp));
	}

	private float2 OnSpineDirForUps(float f, float timeStacker, float dt)
	{
		float num = OnSpineIndex(f, timeStacker);
		return Vector3Ext.Slerp2(SpineSegmentDirForUps(Mathf.FloorToInt(num), timeStacker, dt), SpineSegmentDirForUps(Mathf.FloorToInt(num) + 1, timeStacker, dt), Mathf.InverseLerp(0.5f, 1f, num - Mathf.Floor(num)));
	}

	private float2 SpineSegmentDirForUps(int seg, float timeStacker, float dt)
	{
		if (seg == 0)
		{
			return SpineSegmentDir(1, timeStacker);
		}
		if (seg > drawPositions.GetLength(0) - 2)
		{
			return Custom.DirVec(SpineSegmentPos(drawPositions.GetLength(0) - 2, timeStacker), SpineSegmentPos(drawPositions.GetLength(0) - 1, timeStacker));
		}
		return Custom.DirVec(SpineSegmentPos(seg, timeStacker), SpineSegmentPos(seg + 1, timeStacker));
	}

	public float OnSpineWidth(float f, float timeStacker)
	{
		float num = OnSpineIndex(f, timeStacker);
		int num2 = Mathf.FloorToInt(num);
		return math.lerp(SpineSegmentWidth(num2, timeStacker), SpineSegmentWidth(num2 + 1, timeStacker), Mathf.InverseLerp(0.5f, 1f, num - Mathf.Floor(num)));
	}

	private float SpineSegmentWidth(int seg, float timeStacker)
	{
		return seg switch
		{
			0 => 4f * math.lerp(0.5f, 1.5f, iVars.neckThickness) * math.abs(math.sign(math.dot((SpineSegmentPos(1, timeStacker) - SpineSegmentPos(0, timeStacker)).normalized(), (SpineSegmentPos(2, timeStacker) - SpineSegmentPos(1, timeStacker)).normalized()))), 
			1 => math.lerp(8f, 18f, iVars.fatness), 
			2 => math.lerp(10f, 20f, iVars.fatness), 
			3 => math.lerp(4f, 8f, iVars.WaistWidth), 
			4 => 10f, 
			_ => 0f, 
		};
	}

	private float OnSpineIndex(float f, float timeStacker)
	{
		float num = f * math.lerp(spineLengths[1], spineLengths[0], timeStacker);
		float a = 0f;
		float num2 = math.distance(math.lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker), math.lerp(drawPositions[1, 1], drawPositions[1, 0], timeStacker));
		int num3;
		for (num3 = 0; num2 < num; num2 += math.distance(math.lerp(drawPositions[num3, 1], drawPositions[num3, 1], timeStacker), math.lerp(drawPositions[num3 + 1, 1], drawPositions[num3 + 1, 1], timeStacker)))
		{
			a = num2;
			num3++;
			if (num3 >= drawPositions.GetLength(0) - 1)
			{
				break;
			}
		}
		return (float)num3 + Mathf.InverseLerp(a, num2, num);
	}

	public float2 ItemPosition(int grasp)
	{
		if (grasp == 0)
		{
			return hands[0].pos.ToF2() + hands[0].spearPosAdd;
		}
		if (scavenger.grasps[grasp].grabbed is Spear)
		{
			return OnBackSurfacePos(new float2((0f - flip) * 0.9f, 0.4f + (float)(grasp - 1) * 0.1f), 1f);
		}
		if (scavenger.grasps[grasp].grabbed is Lantern)
		{
			return hands[1].pos;
		}
		return math.lerp(scavenger.bodyChunks[1].pos, scavenger.bodyChunks[0].pos, 0.4f - 0.1f * (float)(grasp - 1));
	}

	public float2 ItemDirection(int grasp)
	{
		if (grasp == 0)
		{
			return hands[0].WeaponDir();
		}
		if (scavenger.grasps[grasp].grabbed is Spear)
		{
			return Custom.RotateAroundOrigo(Custom.DegToFloat2(45f - flip * (float)(grasp - 1) * 10f + Custom.LerpMap(flip, -1f, 1f, -30f, 0f)), Custom.VecToDeg(Vector3Ext.Slerp2(bodyAxis, Custom.DirVec(drawPositions[hipsDrawPos, 1], drawPositions[chestDrawPos, 1]), 0.75f)));
		}
		return Custom.DirVec(scavenger.grasps[grasp].grabbed.firstChunk.pos, ItemPosition(grasp));
	}

	private void GenerateColors()
	{
		float num = UnityEngine.Random.value * 0.1f;
		if (UnityEngine.Random.value < 0.025f)
		{
			num = Mathf.Pow(UnityEngine.Random.value, 0.4f);
		}
		if (scavenger.Elite)
		{
			num = Mathf.Pow(UnityEngine.Random.value, 5f);
		}
		float num2 = num + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.3f * Mathf.Pow(UnityEngine.Random.value, 2f);
		if (num2 > 1f)
		{
			num2 -= 1f;
		}
		else if (num2 < 0f)
		{
			num2 += 1f;
		}
		bodyColor = new HSLColor(num, Mathf.Lerp(0.05f, 1f, Mathf.Pow(UnityEngine.Random.value, 0.85f)), Mathf.Lerp(0.05f, 0.8f, UnityEngine.Random.value));
		bodyColor.saturation *= 1f - iVars.generalMelanin;
		bodyColor.lightness = Mathf.Lerp(bodyColor.lightness, 0.5f + 0.5f * Mathf.Pow(UnityEngine.Random.value, 0.8f), 1f - iVars.generalMelanin);
		bodyColorBlack = Custom.LerpMap((bodyColor.rgb.r + bodyColor.rgb.g + bodyColor.rgb.b) / 3f, 0.04f, 0.8f, 0.3f, 0.95f, 0.5f);
		bodyColorBlack = Mathf.Lerp(bodyColorBlack, Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value), UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value);
		bodyColorBlack *= iVars.generalMelanin;
		float2 @float = new float2(bodyColor.saturation, Mathf.Lerp(-1f, 1f, bodyColor.lightness * (1f - bodyColorBlack)));
		if (@float.magnitude() < 0.5f)
		{
			@float = math.lerp(@float, @float.normalized(), Mathf.InverseLerp(0.5f, 0.3f, @float.magnitude()));
			bodyColor = new HSLColor(bodyColor.hue, Mathf.InverseLerp(-1f, 1f, @float.x), Mathf.InverseLerp(-1f, 1f, @float.y));
			bodyColorBlack = Custom.LerpMap((bodyColor.rgb.r + bodyColor.rgb.g + bodyColor.rgb.b) / 3f, 0.04f, 0.8f, 0.3f, 0.95f, 0.5f);
			bodyColorBlack = Mathf.Lerp(bodyColorBlack, Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value), UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value);
			bodyColorBlack *= iVars.generalMelanin;
		}
		float a;
		if (UnityEngine.Random.value < Custom.LerpMap(bodyColorBlack, 0.5f, 0.8f, 0.9f, 0.3f))
		{
			a = num2 + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.1f * Mathf.Pow(UnityEngine.Random.value, 1.5f);
			a = Mathf.Lerp(a, 0.15f, UnityEngine.Random.value);
			if (a > 1f)
			{
				a -= 1f;
			}
			else if (a < 0f)
			{
				a += 1f;
			}
		}
		else
		{
			a = ((UnityEngine.Random.value < 0.5f) ? Custom.Decimal(num + 0.5f) : Custom.Decimal(num2 + 0.5f)) + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.25f * Mathf.Pow(UnityEngine.Random.value, 2f);
			if (UnityEngine.Random.value < Mathf.Lerp(0.8f, 0.2f, scavenger.abstractCreature.personality.energy))
			{
				a = Mathf.Lerp(a, 0.15f, UnityEngine.Random.value);
			}
			if (a > 1f)
			{
				a -= 1f;
			}
			else if (a < 0f)
			{
				a += 1f;
			}
		}
		headColor = new HSLColor((UnityEngine.Random.value < 0.75f) ? num2 : a, 1f, 0.05f + 0.15f * UnityEngine.Random.value);
		headColor.saturation *= Mathf.Pow(1f - iVars.generalMelanin, 2f);
		headColor.lightness = Mathf.Lerp(headColor.lightness, 0.5f + 0.5f * Mathf.Pow(UnityEngine.Random.value, 0.8f), 1f - iVars.generalMelanin);
		headColor.saturation *= 0.1f + 0.9f * Mathf.InverseLerp(0.1f, 0f, Custom.DistanceBetweenZeroToOneFloats(bodyColor.hue, headColor.hue) * Custom.LerpMap(Mathf.Abs(0.5f - headColor.lightness), 0f, 0.5f, 1f, 0.3f));
		if (headColor.lightness < 0.5f)
		{
			headColor.lightness *= 0.5f + 0.5f * Mathf.InverseLerp(0.2f, 0.05f, Custom.DistanceBetweenZeroToOneFloats(bodyColor.hue, headColor.hue));
		}
		headColorBlack = Custom.LerpMap((headColor.rgb.r + headColor.rgb.g + headColor.rgb.b) / 3f, 0.035f, 0.26f, 0.7f, 0.95f, 0.25f);
		headColorBlack = Mathf.Lerp(headColorBlack, Mathf.Lerp(0.8f, 1f, UnityEngine.Random.value), UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value);
		headColorBlack *= 0.2f + 0.7f * iVars.generalMelanin;
		headColorBlack = Mathf.Max(headColorBlack, bodyColorBlack);
		headColor.saturation = Custom.LerpMap(headColor.lightness * (1f - headColorBlack), 0f, 0.15f, 1f, headColor.saturation);
		if (headColor.lightness > bodyColor.lightness)
		{
			headColor = bodyColor;
		}
		if (headColor.saturation < bodyColor.saturation * 0.75f)
		{
			if (UnityEngine.Random.value < 0.5f)
			{
				headColor.hue = bodyColor.hue;
			}
			else
			{
				headColor.lightness *= 0.25f;
			}
			headColor.saturation = bodyColor.saturation * 0.75f;
		}
		decorationColor = new HSLColor((UnityEngine.Random.value < 0.65f) ? num : ((UnityEngine.Random.value < 0.5f) ? num2 : a), UnityEngine.Random.value, 0.5f + 0.5f * Mathf.Pow(UnityEngine.Random.value, 0.5f));
		decorationColor.lightness *= Mathf.Lerp(iVars.generalMelanin, UnityEngine.Random.value, 0.5f);
		eyeColor = new HSLColor(scavenger.Elite ? 0f : a, 1f, (UnityEngine.Random.value < 0.2f) ? (0.5f + UnityEngine.Random.value * 0.5f) : 0.5f);
		if (iVars.coloredPupils > 0)
		{
			eyeColor.lightness = Mathf.Lerp(eyeColor.lightness, 1f, 0.3f);
		}
		if (headColor.lightness * (1f - headColorBlack) > eyeColor.lightness / 2f && (iVars.pupilSize == 0f || iVars.deepPupils))
		{
			eyeColor.lightness *= 0.2f;
		}
		float value = UnityEngine.Random.value;
		float value2 = UnityEngine.Random.value;
		bellyColor = new HSLColor(Mathf.Lerp(bodyColor.hue, decorationColor.hue, value * 0.7f), bodyColor.saturation * Mathf.Lerp(1f, 0.5f, value), bodyColor.lightness + 0.05f + 0.3f * value2);
		bellyColorBlack = Mathf.Lerp(bodyColorBlack, 1f, 0.3f * Mathf.Pow(value2, 1.4f));
		if (UnityEngine.Random.value < 1f / 30f)
		{
			headColor.lightness = Mathf.Lerp(0.2f, 0.35f, UnityEngine.Random.value);
			headColorBlack *= Mathf.Lerp(1f, 0.8f, UnityEngine.Random.value);
			bellyColor.hue = Mathf.Lerp(bellyColor.hue, headColor.hue, Mathf.Pow(UnityEngine.Random.value, 0.5f));
		}
	}
}
