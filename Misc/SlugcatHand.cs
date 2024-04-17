using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class SlugcatHand : Limb
{
	public int retractCounter;

	public bool reachingForObject;

	public SlugcatHand(GraphicsModule ow, BodyChunk con, int num, float rd, float sfFric, float aFric)
		: base(ow, con, num, rd, sfFric, aFric, 7f, 0.5f)
	{
		reachingForObject = false;
	}

	public override void Update()
	{
		base.Update();
		ConnectToPoint(connection.pos, 20f, push: false, 0f, connection.vel, 0f, 0f);
		if (ModManager.MSC || ModManager.CoopAvailable)
		{
			if ((owner.owner as Player).Consious && (owner.owner as Player).grabbedBy.Count > 0 && (owner.owner as Player).grabbedBy[0] != null && (owner.owner as Player).grabbedBy[0].grabber is Player && Mathf.Sign((owner.owner as Player).firstChunk.pos.x - (owner.owner as Player).grabbedBy[0].grabber.firstChunk.pos.x) == (float)((limbNumber != 0) ? 1 : (-1)))
			{
				base.mode = Mode.HuntAbsolutePosition;
				absoluteHuntPos = Vector2.Lerp((owner.owner as Player).firstChunk.pos, (owner.owner as Player).grabbedBy[0].grabber.firstChunk.pos, 0.5f);
				huntSpeed = 20f;
				quickness = 1f;
				return;
			}
			if ((owner.owner as Player).grasps[limbNumber] != null && (owner.owner as Player).grasps[limbNumber].grabbed is Player && ((owner.owner as Player).grasps[limbNumber].grabbed as Player).Consious)
			{
				base.mode = Mode.HuntAbsolutePosition;
				absoluteHuntPos = Vector2.Lerp((owner.owner as Player).firstChunk.pos, (owner.owner as Player).grasps[limbNumber].grabbed.firstChunk.pos, 0.5f);
				huntSpeed = 20f;
				quickness = 1f;
				return;
			}
		}
		bool flag = true;
		if (reachingForObject)
		{
			base.mode = Mode.HuntAbsolutePosition;
			flag = false;
			reachingForObject = false;
		}
		else
		{
			flag = EngageInMovement();
		}
		if (ModManager.MMF)
		{
			if ((owner.owner as Player).grasps[limbNumber] != null && (owner.owner as Player).HeavyCarry((owner.owner as Player).grasps[limbNumber].grabbed))
			{
				flag = true;
			}
		}
		else if (limbNumber == 0 && (owner.owner as Player).grasps[0] != null && (owner.owner as Player).HeavyCarry((owner.owner as Player).grasps[0].grabbed))
		{
			flag = true;
		}
		if (flag)
		{
			if (((owner.owner as Player).grasps[0] != null && (owner.owner as Player).HeavyCarry((owner.owner as Player).grasps[0].grabbed)) || (ModManager.MMF && (owner.owner as Player).grasps[1] != null && (owner.owner as Player).HeavyCarry((owner.owner as Player).grasps[1].grabbed)))
			{
				base.mode = Mode.HuntAbsolutePosition;
				BodyChunk bodyChunk = ((!ModManager.MMF) ? (owner.owner as Player).grasps[0].grabbedChunk : (((owner.owner as Player).grasps[0] != null && (owner.owner as Player).HeavyCarry((owner.owner as Player).grasps[0].grabbed)) ? (owner.owner as Player).grasps[0].grabbedChunk : (owner.owner as Player).grasps[1].grabbedChunk));
				absoluteHuntPos = bodyChunk.pos + Custom.PerpendicularVector((connection.pos - bodyChunk.pos).normalized) * bodyChunk.rad * 0.8f * ((limbNumber == 0) ? (-1f) : 1f);
				huntSpeed = 20f;
				quickness = 1f;
				flag = false;
			}
			else if ((owner.owner as Player).grasps[limbNumber] != null)
			{
				base.mode = Mode.HuntRelativePosition;
				if (ModManager.MSC && (owner.owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
				{
					relativeHuntPos.x = (owner.owner as Player).ThrowDirection * 3;
				}
				else
				{
					relativeHuntPos.x = -20f + 40f * (float)limbNumber;
				}
				relativeHuntPos.y = -12f;
				if ((owner.owner as Player).eatCounter < 40)
				{
					int num = -1;
					int num2 = 0;
					while (num < 0 && num2 < 2)
					{
						if ((owner.owner as Player).grasps[num2] != null && (owner.owner as Player).grasps[num2].grabbed is IPlayerEdible && ((owner.owner as Player).grasps[num2].grabbed as IPlayerEdible).Edible)
						{
							num = num2;
						}
						num2++;
					}
					if (num == limbNumber)
					{
						relativeHuntPos *= Custom.LerpMap((owner.owner as Player).eatCounter, 40f, 20f, 0.9f, 0.7f);
						relativeHuntPos.y += Custom.LerpMap((owner.owner as Player).eatCounter, 40f, 20f, 2f, 4f);
						relativeHuntPos.x *= Custom.LerpMap((owner.owner as Player).eatCounter, 40f, 20f, 1f, 1.2f);
					}
				}
				if (((owner.owner as Player).swallowAndRegurgitateCounter > 10 && (owner.owner as Player).objectInStomach == null) || (owner.owner as Player).craftingObject)
				{
					int num3 = -1;
					int num4 = 0;
					while (num3 < 0 && num4 < 2)
					{
						if ((owner.owner as Player).grasps[num4] != null && (owner.owner as Player).CanBeSwallowed((owner.owner as Player).grasps[num4].grabbed))
						{
							num3 = num4;
						}
						num4++;
					}
					if (num3 == limbNumber || (owner.owner as Player).craftingObject)
					{
						float num5 = Mathf.InverseLerp(10f, 90f, (owner.owner as Player).swallowAndRegurgitateCounter);
						if (num5 < 0.5f)
						{
							relativeHuntPos *= Mathf.Lerp(0.9f, 0.7f, num5 * 2f);
							relativeHuntPos.y += Mathf.Lerp(2f, 4f, num5 * 2f);
							relativeHuntPos.x *= Mathf.Lerp(1f, 1.2f, num5 * 2f);
						}
						else
						{
							(owner as PlayerGraphics).blink = 5;
							relativeHuntPos = new Vector2(0f, -4f) + Custom.RNV() * 2f * UnityEngine.Random.value * Mathf.InverseLerp(0.5f, 1f, num5);
							(owner as PlayerGraphics).head.vel += Custom.RNV() * 2f * UnityEngine.Random.value * Mathf.InverseLerp(0.5f, 1f, num5);
							owner.owner.bodyChunks[0].vel += Custom.RNV() * 0.2f * UnityEngine.Random.value * Mathf.InverseLerp(0.5f, 1f, num5);
						}
					}
				}
				relativeHuntPos.x *= 1f - Mathf.Sin((owner.owner as Player).switchHandsProcess * (float)Math.PI);
				if ((owner as PlayerGraphics).spearDir != 0f && (owner.owner as Player).bodyMode == Player.BodyModeIndex.Stand)
				{
					Vector2 b = Custom.DegToVec(180f + ((limbNumber == 0) ? (-1f) : 1f) * 8f + (float)(owner.owner as Player).input[0].x * 4f) * 12f;
					b.y += Mathf.Sin((float)(owner.owner as Player).animationFrame / 6f * 2f * (float)Math.PI) * 2f;
					b.x -= Mathf.Cos((float)((owner.owner as Player).animationFrame + ((!(owner.owner as Player).leftFoot) ? 6 : 0)) / 12f * 2f * (float)Math.PI) * 4f * (float)(owner.owner as Player).input[0].x;
					b.x += (float)(owner.owner as Player).input[0].x * 2f;
					relativeHuntPos = Vector2.Lerp(relativeHuntPos, b, Mathf.Abs((owner as PlayerGraphics).spearDir));
					if ((owner.owner as Player).grasps[limbNumber].grabbed is Weapon)
					{
						((owner.owner as Player).grasps[limbNumber].grabbed as Weapon).ChangeOverlap(((owner as PlayerGraphics).spearDir > -0.4f && limbNumber == 0) || ((owner as PlayerGraphics).spearDir < 0.4f && limbNumber == 1));
					}
				}
				flag = false;
				if ((owner.owner as Creature).grasps[limbNumber].grabbed is Fly && !((owner.owner as Creature).grasps[limbNumber].grabbed as Fly).dead)
				{
					huntSpeed = UnityEngine.Random.value * 5f;
					quickness = UnityEngine.Random.value * 0.3f;
					vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * UnityEngine.Random.value * (Custom.DistLess(absoluteHuntPos, pos, 7f) ? 4f : 1.5f);
					pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 4f;
					(owner as PlayerGraphics).NudgeDrawPosition(0, Custom.DirVec((owner.owner as Creature).mainBodyChunk.pos, pos) * 3f * UnityEngine.Random.value);
					(owner as PlayerGraphics).head.vel += Custom.DirVec((owner.owner as Creature).mainBodyChunk.pos, pos) * 2f * UnityEngine.Random.value;
				}
				else if ((owner.owner as Creature).grasps[limbNumber].grabbed is VultureMask)
				{
					relativeHuntPos *= 1f - ((owner.owner as Creature).grasps[limbNumber].grabbed as VultureMask).donned;
				}
			}
		}
		if (flag && base.mode != Mode.Retracted)
		{
			retractCounter++;
			if ((float)retractCounter > 5f)
			{
				base.mode = Mode.HuntAbsolutePosition;
				pos = Vector2.Lerp(pos, owner.owner.bodyChunks[0].pos, Mathf.Clamp(((float)retractCounter - 5f) * 0.05f, 0f, 1f));
				if (Custom.DistLess(pos, owner.owner.bodyChunks[0].pos, 2f) && reachedSnapPosition)
				{
					base.mode = Mode.Retracted;
				}
				absoluteHuntPos = owner.owner.bodyChunks[0].pos;
				huntSpeed = 1f + (float)retractCounter * 0.2f;
				quickness = 1f;
			}
		}
		else
		{
			retractCounter -= 10;
			if (retractCounter < 0)
			{
				retractCounter = 0;
			}
		}
	}

	public bool EngageInMovement()
	{
		bool flag = true;
		if ((owner.owner as Player).bodyMode == Player.BodyModeIndex.ClimbingOnBeam)
		{
			flag = false;
			if ((owner.owner as Player).animation == Player.AnimationIndex.ClimbOnBeam)
			{
				float f = (float)(owner.owner as Player).animationFrame / 20f * (float)Math.PI * 2f;
				f = ((limbNumber == 1 != ((owner.owner as Player).flipDirection == 1)) ? Mathf.Cos(f) : Mathf.Sin(f));
				base.mode = Mode.HuntAbsolutePosition;
				absoluteHuntPos = new Vector2(owner.owner.room.MiddleOfTile(owner.owner.bodyChunks[0].pos).x, owner.owner.bodyChunks[0].pos.y);
				absoluteHuntPos.y += ((limbNumber == 1 == ((owner.owner as Player).flipDirection == 1)) ? (-3f) : 3f) + 6f * f;
				absoluteHuntPos.x += ((limbNumber == 1 == ((owner.owner as Player).flipDirection == 1)) ? (-(owner.owner as Player).flipDirection) : (owner.owner as Player).flipDirection);
				retractCounter = 40;
			}
			else if ((owner.owner as Player).animation == Player.AnimationIndex.HangFromBeam || (owner.owner as Player).animation == Player.AnimationIndex.GetUpOnBeam)
			{
				base.mode = Mode.HuntAbsolutePosition;
				absoluteHuntPos = new Vector2(owner.owner.bodyChunks[0].pos.x, owner.owner.room.MiddleOfTile(owner.owner.bodyChunks[0].pos).y);
				absoluteHuntPos.y -= 1f;
				absoluteHuntPos.x += ((limbNumber == 0) ? (-1f) : 1f) * (10f + 3f * Mathf.Sin((float)Math.PI * 2f * (float)(owner.owner as Player).animationFrame / 20f));
				if (owner.owner.room.GetTile(absoluteHuntPos).Terrain == Room.Tile.TerrainType.Solid || !owner.owner.room.GetTile(absoluteHuntPos).horizontalBeam)
				{
					absoluteHuntPos.x = owner.owner.room.MiddleOfTile(owner.owner.bodyChunks[0].pos).x + ((limbNumber == 0) ? (-10f) : 10f);
				}
			}
			else if ((owner.owner as Player).animation == Player.AnimationIndex.BeamTip || (owner.owner as Player).animation == Player.AnimationIndex.StandOnBeam)
			{
				base.mode = Mode.HuntRelativePosition;
				float num = Mathf.Sin((float)Math.PI * 2f * (owner as PlayerGraphics).balanceCounter / 300f);
				relativeHuntPos.x = -20f + 40f * (float)limbNumber;
				relativeHuntPos.y = -4f - 6f * num * ((limbNumber == 0) ? (-1f) : 1f);
				FindGrip(owner.owner.room, connection.pos, connection.pos, 100f, connection.pos + new Vector2(-10f + 20f * (float)limbNumber, (limbNumber == 0 == ((owner.owner as Player).flipDirection == -1)) ? 0f : (-5f)), (limbNumber == 0) ? 1 : (-1), -1, behindWalls: false);
				if (base.mode == Mode.HuntAbsolutePosition)
				{
					if (pos.y > owner.owner.bodyChunks[0].pos.y + 5f)
					{
						(owner as PlayerGraphics).head.vel.x += 2f - 4f * (float)limbNumber;
					}
					if (Mathf.Abs(owner.owner.bodyChunks[0].pos.x - owner.owner.bodyChunks[1].pos.x) < 10f)
					{
						(owner as PlayerGraphics).disbalanceAmount -= 8f;
					}
				}
				else if ((owner as PlayerGraphics).disbalanceAmount < 40f)
				{
					float num2 = (40f - (owner as PlayerGraphics).disbalanceAmount) / 40f;
					relativeHuntPos.y *= 1f - num2;
					relativeHuntPos.y -= num2 * 15f;
					relativeHuntPos.x *= 1f - num2;
					if (num2 >= 1f)
					{
						flag = true;
					}
				}
				huntSpeed = 5f;
				quickness = 0.2f;
			}
			else if ((owner.owner as Player).animation == Player.AnimationIndex.HangUnderVerticalBeam)
			{
				base.mode = Mode.HuntAbsolutePosition;
				absoluteHuntPos = owner.owner.room.MiddleOfTile(owner.owner.firstChunk.pos) + new Vector2((limbNumber == 0) ? (-0.5f) : 0.5f, (limbNumber == 0) ? 20f : 25f);
				huntSpeed = 10f;
				quickness = 1f;
			}
		}
		else if ((owner.owner as Player).bodyMode == Player.BodyModeIndex.Crawl)
		{
			flag = false;
			base.mode = Mode.HuntAbsolutePosition;
			huntSpeed = 12f;
			quickness = 0.7f;
			if ((limbNumber == 0 || (Mathf.Abs((owner as PlayerGraphics).hands[0].pos.x - owner.owner.bodyChunks[0].pos.x) < 10f && (owner as PlayerGraphics).hands[0].reachedSnapPosition)) && !Custom.DistLess(owner.owner.bodyChunks[0].pos, absoluteHuntPos, 29f))
			{
				Vector2 vector = absoluteHuntPos;
				FindGrip(owner.owner.room, connection.pos + new Vector2((float)(owner.owner as Player).flipDirection * 20f, 0f), connection.pos + new Vector2((float)(owner.owner as Player).flipDirection * 20f, 0f), 100f, new Vector2(owner.owner.bodyChunks[0].pos.x + (float)(owner.owner as Player).flipDirection * 28f, owner.owner.room.MiddleOfTile(owner.owner.bodyChunks[0].pos).y - 10f), 2, 1, behindWalls: false);
				if (!(absoluteHuntPos != vector))
				{
				}
			}
		}
		else if ((owner.owner as Player).bodyMode == Player.BodyModeIndex.CorridorClimb)
		{
			flag = false;
			base.mode = Mode.HuntAbsolutePosition;
			if (!Custom.DistLess(pos, connection.pos, 20f))
			{
				Vector2 vector2 = Custom.DirVec(owner.owner.bodyChunks[1].pos, owner.owner.bodyChunks[0].pos);
				FindGrip(owner.owner.room, connection.pos, connection.pos, 100f, connection.pos + (vector2 + new Vector2((owner.owner as Player).input[0].x, (owner.owner as Player).input[0].y).normalized * 1.5f).normalized * 20f + Custom.PerpendicularVector(vector2) * (6f - 12f * (float)limbNumber), 2, 2, behindWalls: false);
			}
		}
		else if ((owner.owner as Player).bodyMode == Player.BodyModeIndex.WallClimb)
		{
			flag = false;
			base.mode = Mode.HuntAbsolutePosition;
			absoluteHuntPos.x = owner.owner.room.MiddleOfTile(owner.owner.bodyChunks[0].pos).x + (float)(owner.owner as Player).flipDirection * 10f;
			if (limbNumber == 0 == ((owner.owner as Player).flipDirection == -1))
			{
				absoluteHuntPos.y = owner.owner.bodyChunks[0].pos.y - 7f;
			}
			else
			{
				absoluteHuntPos.y = owner.owner.bodyChunks[0].pos.y + 3f;
			}
		}
		else if ((owner.owner as Player).bodyMode == Player.BodyModeIndex.Swimming)
		{
			flag = false;
			float num3 = (((owner.owner as Player).swimCycle < 3f) ? ((owner.owner as Player).swimCycle / 3f) : (1f - ((owner.owner as Player).swimCycle - 3f)));
			if ((owner.owner as Player).animation == Player.AnimationIndex.DeepSwim)
			{
				base.mode = Mode.HuntRelativePosition;
				float num4 = Mathf.Pow(1f - Mathf.InverseLerp(0.5f, 1f, num3), 1.5f);
				relativeHuntPos = Custom.DegToVec((20f + num4 * 140f) * ((limbNumber == 0) ? (-1f) : 1f)) * 20f;
				if ((owner.owner as Player).swimCycle < 3f)
				{
					relativeHuntPos.x *= 0.5f;
				}
			}
			else
			{
				base.mode = Mode.HuntAbsolutePosition;
				absoluteHuntPos.x = owner.owner.bodyChunks[0].pos.x + Mathf.Lerp(((limbNumber == 0) ? (-1f) : 1f) * 30f, (float)(owner.owner as Player).input[0].x * 10f * (0f - Mathf.Sin(num3 * 2f * (float)Math.PI)), 0.5f);
				absoluteHuntPos.y = owner.owner.room.FloatWaterLevel(absoluteHuntPos.x) - 7f;
				absoluteHuntPos += Custom.DegToVec(360f * num3 * (((owner.owner as Player).input[0].x == 0) ? 1f : (0f - (float)(owner.owner as Player).input[0].x))) * ((limbNumber == 0) ? (-1f) : 1f) * (((owner.owner as Player).input[0].x == 0) ? 5f : 10f);
			}
			huntSpeed = 5f;
			quickness = 0.5f;
		}
		else if ((owner.owner as Player).bodyMode == Player.BodyModeIndex.ZeroG)
		{
			flag = false;
			if ((owner.owner as Player).animation == Player.AnimationIndex.ZeroGPoleGrab)
			{
				float f2 = (float)(owner.owner as Player).animationFrame / 20f * (float)Math.PI * 2f;
				bool flag2 = ((owner.owner as Player).standing && limbNumber == 1 == ((owner.owner as Player).zeroGPoleGrabDir.x == 1)) || (!(owner.owner as Player).standing && limbNumber == 1 == ((owner.owner as Player).zeroGPoleGrabDir.x == 1));
				f2 = ((!flag2) ? Mathf.Cos(f2) : Mathf.Sin(f2));
				base.mode = Mode.HuntAbsolutePosition;
				if ((owner.owner as Player).standing)
				{
					absoluteHuntPos = new Vector2(owner.owner.room.MiddleOfTile(owner.owner.bodyChunks[0].pos).x, owner.owner.bodyChunks[0].pos.y);
					absoluteHuntPos.y += ((flag2 ? (-3f) : 3f) + 6f * f2) * Mathf.Sign(owner.owner.bodyChunks[1].pos.y - owner.owner.bodyChunks[0].pos.y);
					absoluteHuntPos.x += (flag2 ? (-1f) : 1f) * (float)(owner.owner as Player).zeroGPoleGrabDir.x;
				}
				else
				{
					absoluteHuntPos = new Vector2(owner.owner.bodyChunks[0].pos.x, owner.owner.room.MiddleOfTile(owner.owner.bodyChunks[0].pos).y);
					absoluteHuntPos.x += ((flag2 ? (-3f) : 3f) + 6f * f2) * Mathf.Sign(owner.owner.bodyChunks[1].pos.x - owner.owner.bodyChunks[0].pos.x);
					absoluteHuntPos.y += (flag2 ? (-1f) : 1f) * (float)(owner.owner as Player).zeroGPoleGrabDir.y;
				}
				retractCounter = 40;
			}
			else if ((owner.owner as Player).animation == Player.AnimationIndex.ZeroGSwim)
			{
				flag = true;
				if ((owner.owner as Player).swimBits[limbNumber] != null && Custom.DistLess(owner.owner.firstChunk.pos, (owner.owner as Player).swimBits[limbNumber].pos, 30f))
				{
					flag = false;
					base.mode = Mode.HuntAbsolutePosition;
					absoluteHuntPos = (owner.owner as Player).swimBits[limbNumber].pos;
				}
				if (flag && owner.owner.firstChunk.vel.magnitude < 5f)
				{
					base.mode = Mode.Dangle;
					FindGrip(owner.owner.room, connection.pos, connection.pos, 100f, connection.pos + new Vector2(-10f + 20f * (float)limbNumber, (limbNumber == 0 == ((owner.owner as Player).flipDirection == -1)) ? 0f : (-5f)), (limbNumber == 0) ? 1 : (-1), -2, behindWalls: false);
					flag = base.mode != Mode.HuntAbsolutePosition;
				}
				if (flag && owner.owner.firstChunk.vel.magnitude < 6f && Vector2.Distance(owner.owner.firstChunk.vel, owner.owner.bodyChunks[1].vel) > 2f && (owner as PlayerGraphics).flail > 0.1f && ((owner.owner as Player).input[0].x != 0 || (owner.owner as Player).input[0].y != 0))
				{
					flag = false;
					float num5 = 0.5f + 0.5f * Mathf.Sin((owner.owner as Player).swimCycle / 4f * (float)Math.PI * 2f);
					base.mode = Mode.HuntRelativePosition;
					relativeHuntPos = Custom.DegToVec((20f + num5 * 140f) * ((limbNumber == 0) ? (-1f) : 1f)) * 20f * Mathf.InverseLerp(0.1f, 0.3f, (owner as PlayerGraphics).flail);
				}
				if (flag && (owner.owner.firstChunk.vel.magnitude < 4f || Vector2.Dot(owner.owner.firstChunk.vel.normalized, Custom.DirVec(owner.owner.bodyChunks[1].pos, owner.owner.firstChunk.pos)) < 0.6f))
				{
					flag = false;
					float num6 = (owner.owner as Player).swimCycle / 4f;
					float num7 = Mathf.Sin((owner as PlayerGraphics).balanceCounter / 300f * (float)Math.PI * 2f);
					num7 *= Mathf.InverseLerp(0f, 50f, (owner as PlayerGraphics).disbalanceAmount);
					base.mode = Mode.HuntRelativePosition;
					relativeHuntPos = new Vector2(((limbNumber == 0) ? (-1f) : 1f) * 17f + 5f * num7, -5f * num7) + Custom.DegToVec(num6 * ((limbNumber == 0) ? (-1f) : 1f) * 360f) * 5f;
				}
			}
		}
		else if ((owner.owner as Player).bodyMode == Player.BodyModeIndex.Default)
		{
			if ((owner.owner as Player).animation == Player.AnimationIndex.AntlerClimb)
			{
				vel = Vector2.Lerp(vel, (owner.owner as Player).playerInAntlers.antlerChunk.vel, 0.7f);
				flag = false;
				base.mode = Mode.HuntAbsolutePosition;
				absoluteHuntPos = (owner.owner as Player).playerInAntlers.handGrabPoints[limbNumber];
				if ((owner.owner as Player).playerInAntlers.dangle)
				{
					absoluteHuntPos += Custom.PerpendicularVector(Custom.DirVec(owner.owner.firstChunk.pos, (owner.owner as Player).playerInAntlers.handGrabPoints[limbNumber])) * ((limbNumber == 0) ? 4f : (-4f));
					huntSpeed = 50f;
					quickness = 1f;
				}
				else
				{
					huntSpeed = 6f;
					quickness = 0.6f;
				}
			}
			else if ((owner as PlayerGraphics).airborneCounter > 180f && !Custom.DistLess(new Vector2(0f, 0f), owner.owner.bodyChunks[0].vel, 4f))
			{
				flag = false;
				retractCounter = 0;
				Vector2 vector3 = Custom.DegToVec(UnityEngine.Random.value * 360f) * 40f * UnityEngine.Random.value;
				if (owner.owner.room.GetTile(owner.owner.bodyChunks[0].pos + owner.owner.bodyChunks[0].vel * 4f + vector3).Terrain != 0)
				{
					base.mode = Mode.HuntAbsolutePosition;
					absoluteHuntPos = owner.owner.bodyChunks[0].pos + owner.owner.bodyChunks[0].vel * 4f + vector3;
					huntSpeed = 18f;
					quickness = 1f;
				}
				else
				{
					base.mode = Mode.HuntRelativePosition;
					relativeHuntPos.x = (Mathf.Abs(owner.owner.bodyChunks[0].vel.x * ((owner.owner.bodyChunks[0].pos.y > owner.owner.bodyChunks[1].pos.y + 5f) ? 2.1f : 1f)) + 4f) * (-1f + 2f * (float)limbNumber);
					relativeHuntPos.y = owner.owner.bodyChunks[0].vel.y * ((owner.owner.bodyChunks[0].pos.y > owner.owner.bodyChunks[1].pos.y + 5f) ? (-3f) : (-0.9f)) + Mathf.Abs(owner.owner.bodyChunks[0].vel.x * 0.6f) + 1f;
					if (owner.owner.bodyChunks[0].vel.magnitude > 10f)
					{
						relativeHuntPos += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * UnityEngine.Random.value * owner.owner.bodyChunks[0].vel.magnitude;
					}
					huntSpeed = 8f;
					quickness = 0.6f;
				}
			}
		}
		if ((owner.owner as Player).animation == Player.AnimationIndex.DownOnFours || (owner.owner as Player).animation == Player.AnimationIndex.CrawlTurn)
		{
			flag = false;
			base.mode = Mode.HuntAbsolutePosition;
			absoluteHuntPos = connection.pos;
			absoluteHuntPos.x += -6f + (float)(12 * limbNumber) + connection.vel.normalized.x * 20f;
			absoluteHuntPos.y += Mathf.Abs(connection.vel.normalized.y) * -20f;
		}
		else if ((owner.owner as Player).animation == Player.AnimationIndex.LedgeGrab)
		{
			flag = false;
			absoluteHuntPos = owner.owner.room.MiddleOfTile(owner.owner.bodyChunks[0].pos);
			if (limbNumber == 0 == ((owner.owner as Player).flipDirection == -1))
			{
				absoluteHuntPos.x += (float)(owner.owner as Player).flipDirection * 10f;
				absoluteHuntPos.y -= 10f;
			}
			else
			{
				absoluteHuntPos.x += (float)(owner.owner as Player).flipDirection * 15f;
				absoluteHuntPos.y += 10f;
			}
		}
		else if ((owner.owner as Player).animation == Player.AnimationIndex.VineGrab && owner.owner.room.climbableVines != null && (owner.owner as Player).vinePos != null)
		{
			flag = false;
			float num8 = 20f * ((limbNumber == 0) ? (-1f) : 1f) * Mathf.Sin((float)(owner.owner as Player).animationFrame / 30f * 2f * (float)Math.PI);
			num8 /= owner.owner.room.climbableVines.TotalLength((owner.owner as Player).vinePos.vine);
			Vector2 vector4 = owner.owner.room.climbableVines.OnVinePos(new ClimbableVinesSystem.VinePosition((owner.owner as Player).vinePos.vine, (owner.owner as Player).vinePos.floatPos + num8));
			vector4 += Custom.PerpendicularVector(owner.owner.room.climbableVines.VineDir((owner.owner as Player).vinePos)) * owner.owner.room.climbableVines.VineRad((owner.owner as Player).vinePos) * ((limbNumber == 0) ? (-1f) : 1f);
			base.mode = Mode.HuntAbsolutePosition;
			absoluteHuntPos = vector4;
		}
		return flag;
	}
}
