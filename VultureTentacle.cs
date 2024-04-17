using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class VultureTentacle : Tentacle
{
	public class Mode : ExtEnum<Mode>
	{
		public static readonly Mode Climb = new Mode("Climb", register: true);

		public static readonly Mode Fly = new Mode("Fly", register: true);

		public Mode(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	private DebugSprite[] grabGoalSprites;

	public int tentacleNumber;

	public Mode mode;

	public Vector2 desiredGrabPos;

	private bool attachedAtTip;

	public int segmentsGrippingTerrain;

	public int framesWithoutReaching;

	public int otherTentacleIsFlying;

	public int grabDelay;

	public int framesOfHittingTerrain;

	public bool playGrabSound;

	public int stun;

	private float fm;

	public StaticSoundLoop wooshSound;

	private List<IntVector2> scratchPath;

	public Vulture vulture => owner as Vulture;

	public float tentacleDir
	{
		get
		{
			if (tentacleNumber == 0)
			{
				return -1f;
			}
			if (!vulture.IsMiros)
			{
				return 1f;
			}
			if (tentacleNumber == 1)
			{
				return 1f;
			}
			if (tentacleNumber == 2)
			{
				return -4f;
			}
			return 4f;
		}
	}

	public bool hasAnyGrip
	{
		get
		{
			if (!attachedAtTip)
			{
				return segmentsGrippingTerrain > 0;
			}
			return true;
		}
	}

	public float flyingMode
	{
		get
		{
			return fm;
		}
		set
		{
			fm = Mathf.Clamp(value, 0f, 1f);
		}
	}

	private VultureTentacle OtherTentacle
	{
		get
		{
			if (vulture.IsMiros)
			{
				if (tentacleNumber % 2 == 0)
				{
					return vulture.tentacles[tentacleNumber + 1];
				}
				return vulture.tentacles[tentacleNumber - 1];
			}
			return vulture.tentacles[1 - tentacleNumber];
		}
	}

	public float TentacleContour(float x)
	{
		float num = Mathf.Lerp(0.45f, 0.1f, flyingMode);
		float num2 = Mathf.Lerp(0.51f, 0.25f, flyingMode);
		float num3 = Mathf.Lerp(0.85f, 0.4f, flyingMode);
		float num4 = Mathf.Lerp(6.5f, 5.5f, flyingMode);
		float num5 = Mathf.Lerp(0.5f, 0.35f, flyingMode);
		float num6 = Mathf.Lerp(0.85f, 0f, flyingMode);
		float num7 = num6 + (1f - num6) * Mathf.Cos(Mathf.InverseLerp(num2, 1.2f, x) * (float)Math.PI * 0.5f);
		float num8 = (vulture.IsKing ? 1.2f : 1f);
		if (x < num)
		{
			return num4 * num5 * num8;
		}
		if (x < num2)
		{
			return num4 * Mathf.Lerp(num5, 1f, Custom.SCurve(Mathf.InverseLerp(num, num2, x), 0.1f)) * num8;
		}
		if (x < num3)
		{
			return num4 * num7 * num8;
		}
		return num4 * Mathf.Lerp(0.5f, 1f, Mathf.Cos(Mathf.Pow(Mathf.InverseLerp(num3, 1f, x), 4f) * (float)Math.PI * 0.5f)) * num7 * num8;
	}

	public float FeatherContour(float x)
	{
		return FeatherContour(x, flyingMode);
	}

	public static float FeatherContour(float x, float k)
	{
		float num = Mathf.Lerp(0.2f, 1f, Custom.SCurve(Mathf.Pow(x, 1.5f), 0.1f));
		if (Mathf.Pow(x, 1.5f) > 0.5f)
		{
			num *= Mathf.Sqrt(1f - Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, Mathf.Pow(x, 1.5f)), 4.5f));
		}
		float num2 = 1f;
		num2 *= Mathf.Pow(Mathf.Sin(Mathf.Pow(x, 0.5f) * (float)Math.PI), 0.7f);
		if (x < 0.3f)
		{
			num2 *= Mathf.Lerp(0.7f, 1f, Custom.SCurve(Mathf.InverseLerp(0f, 0.3f, x), 0.5f));
		}
		return Mathf.Lerp(num * 0.5f, num2, k);
	}

	public VultureTentacle(Vulture vulture, BodyChunk chunk, float length, int tentacleNumber)
		: base(vulture, chunk, length)
	{
		this.tentacleNumber = tentacleNumber;
		tProps = new TentacleProps(stiff: false, rope: false, shorten: true, 0.5f, 0f, 0.2f, 1.2f, 0.2f, 1.2f, 10f, 0.25f, 5f, 15, 60, 12, 0);
		tChunks = new TentacleChunk[vulture.IsKing ? 10 : 8];
		for (int i = 0; i < tChunks.Length; i++)
		{
			tChunks[i] = new TentacleChunk(this, i, (float)(i + 1) / (float)tChunks.Length, 5f);
		}
		mode = Mode.Climb;
		debugViz = false;
	}

	public static float FeatherWidth(float x)
	{
		return Mathf.Pow(Mathf.Sin(Mathf.InverseLerp(-0.45f, 1f, x) * (float)Math.PI), 2.6f);
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		wooshSound = new StaticSoundLoop(SoundID.Vulture_Wing_Woosh_LOOP, base.Tip.pos, room, 1f, 1f);
		if (debugViz)
		{
			if (grabGoalSprites != null)
			{
				grabGoalSprites[0].RemoveFromRoom();
				grabGoalSprites[1].RemoveFromRoom();
			}
			grabGoalSprites = new DebugSprite[2];
			grabGoalSprites[0] = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel"), room);
			grabGoalSprites[0].sprite.scale = 10f;
			grabGoalSprites[0].sprite.color = new Color(1f, 0f, 0f);
			grabGoalSprites[1] = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel"), room);
			grabGoalSprites[1].sprite.scale = 10f;
			grabGoalSprites[1].sprite.color = new Color(0f, 5f, 0f);
		}
	}

	public override void Update()
	{
		base.Update();
		if (vulture.enteringShortCut.HasValue)
		{
			base.retractFac = Mathf.Min(0f, base.retractFac - 0.1f);
			for (int i = 0; i < tChunks.Length; i++)
			{
				tChunks[i].vel += Vector2.ClampMagnitude(room.MiddleOfTile(vulture.enteringShortCut.Value) - tChunks[i].pos, 50f) / 10f;
			}
			if (segments.Count > 1)
			{
				segments.RemoveAt(segments.Count - 1);
			}
			return;
		}
		attachedAtTip = false;
		idealLength = Mathf.Lerp(20f * (vulture.IsKing ? 9f : 7f), 20f * (vulture.IsKing ? 13.5f : 11f), flyingMode);
		if (stun > 0)
		{
			stun--;
		}
		if (Mathf.Pow(UnityEngine.Random.value, 0.25f) > 2f * (vulture.State as Vulture.VultureState).wingHealth[tentacleNumber])
		{
			stun = Math.Max(stun, (int)Mathf.Lerp(-2f, 12f, Mathf.Pow(UnityEngine.Random.value, 0.5f + 20f * Mathf.Max(0f, (vulture.State as Vulture.VultureState).wingHealth[tentacleNumber]))));
		}
		limp = !vulture.Consious || stun > 0;
		if (limp)
		{
			floatGrabDest = null;
			for (int j = 0; j < tChunks.Length; j++)
			{
				tChunks[j].vel *= 0.9f;
				tChunks[j].vel.y -= 0.5f;
			}
		}
		for (int k = 0; k < tChunks.Length; k++)
		{
			tChunks[k].rad = TentacleContour(tChunks[k].tPos);
			if (backtrackFrom == -1 || k < backtrackFrom)
			{
				if (k > 1 && Custom.DistLess(tChunks[k].pos, tChunks[k - 2].pos, 30f))
				{
					tChunks[k].vel -= Custom.DirVec(tChunks[k].pos, tChunks[k - 2].pos) * (30f - Vector2.Distance(tChunks[k].pos, tChunks[k - 2].pos)) * 0.1f;
				}
				else if (k <= 1)
				{
					tChunks[k].vel = Custom.DirVec(OtherTentacle.connectedChunk.pos, connectedChunk.pos) * ((k == 0) ? 2f : 1.2f);
				}
			}
			if (room.PointSubmerged(tChunks[k].pos))
			{
				tChunks[k].vel *= 0.5f;
			}
			if (tChunks[k].contactPoint.x != 0 && tChunks[k].lastContactPoint.x == 0 && Mathf.Abs(tChunks[k].pos.x - tChunks[k].lastPos.x) > 6f)
			{
				room.PlaySound(SoundID.Vulture_Tentacle_Collide_Terrain, tChunks[k].pos, Mathf.InverseLerp(6f, 16f, Mathf.Abs(tChunks[k].pos.x - tChunks[k].lastPos.x)), 1f);
			}
			else if (tChunks[k].contactPoint.y != 0 && tChunks[k].lastContactPoint.y == 0 && Mathf.Abs(tChunks[k].pos.y - tChunks[k].lastPos.y) > 6f)
			{
				room.PlaySound(SoundID.Vulture_Tentacle_Collide_Terrain, tChunks[k].pos, Mathf.InverseLerp(6f, 16f, Mathf.Abs(tChunks[k].pos.y - tChunks[k].lastPos.y)), 1f);
			}
		}
		if (!limp)
		{
			if (mode == Mode.Climb)
			{
				if (floatGrabDest.HasValue && Custom.DistLess(tChunks[tChunks.Length - 1].pos, floatGrabDest.Value, 40f) && backtrackFrom == -1)
				{
					tChunks[tChunks.Length - 1].pos = floatGrabDest.Value;
					tChunks[tChunks.Length - 1].vel *= 0f;
					attachedAtTip = true;
				}
				flyingMode -= 0.025f;
				base.Tip.collideWithTerrain = !attachedAtTip;
				UpdateDesiredGrabPos();
				bool flag = tentacleNumber == 0;
				if (vulture.IsMiros)
				{
					flag = tentacleNumber % 2 == 0;
				}
				BodyChunk bodyChunk = (flag ? vulture.bodyChunks[3] : vulture.bodyChunks[2]);
				segmentsGrippingTerrain = 0;
				for (int l = 0; l < tChunks.Length; l++)
				{
					tChunks[l].vel *= Mathf.Lerp(0.95f, 0.85f, Support());
					if (attachedAtTip && (backtrackFrom == -1 || l < backtrackFrom) && GripTerrain(l))
					{
						segmentsGrippingTerrain++;
						for (int num = l - 1; num > 0; num--)
						{
							PushChunksApart(l, num);
						}
					}
					else
					{
						tChunks[l].vel.y += 0.1f;
						tChunks[l].vel += connectedChunk.vel * 0.1f;
						if (!hasAnyGrip)
						{
							if (floatGrabDest.HasValue)
							{
								tChunks[l].vel += Custom.DirVec(tChunks[l].pos, floatGrabDest.Value) * 0.3f;
							}
							else
							{
								tChunks[l].vel += Custom.DirVec(tChunks[l].pos, desiredGrabPos + Custom.DirVec(base.FloatBase, desiredGrabPos) * 70f) * 0.6f;
							}
						}
					}
					tChunks[l].vel += Custom.DirVec(bodyChunk.pos, tChunks[l].pos) * 0.5f / ((float)l + 1f);
				}
				if (attachedAtTip)
				{
					framesWithoutReaching = 0;
					if (SharedPhysics.RayTraceTilesForTerrain(room, base.BasePos, base.grabDest.Value))
					{
						if (!Custom.DistLess(base.Tip.pos, connectedChunk.pos, idealLength))
						{
							Vector2 vector = Custom.DirVec(base.Tip.pos, connectedChunk.pos);
							float num2 = Vector2.Distance(base.Tip.pos, connectedChunk.pos);
							float num3 = idealLength * 0.9f;
							connectedChunk.pos += vector * (num3 - num2) * 0.2f;
							connectedChunk.vel += vector * (num3 - num2) * 0.2f;
						}
						if (!Custom.DistLess(base.Tip.pos, connectedChunk.pos, idealLength * 0.9f))
						{
							vulture.hangingInTentacle = true;
						}
					}
					if (playGrabSound)
					{
						room.PlaySound(SoundID.Vulture_Tentacle_Grab_Terrain, base.Tip.pos);
						playGrabSound = false;
					}
				}
				else
				{
					playGrabSound = true;
					FindGrabPos(ref scratchPath);
					framesWithoutReaching++;
					if ((float)framesWithoutReaching > 60f && !floatGrabDest.HasValue)
					{
						framesWithoutReaching = 0;
						SwitchMode(Mode.Fly);
					}
				}
				if (OtherTentacle.mode == Mode.Fly)
				{
					otherTentacleIsFlying++;
					if (!hasAnyGrip && ((otherTentacleIsFlying > 30 && room.aimap.getTerrainProximity(base.BasePos) >= 3) || otherTentacleIsFlying > 100))
					{
						SwitchMode(Mode.Fly);
						otherTentacleIsFlying = 0;
					}
				}
				else
				{
					otherTentacleIsFlying = 0;
				}
			}
			else if (mode == Mode.Fly)
			{
				bool flag2 = false;
				flyingMode += 0.05f;
				for (int m = 0; m < tChunks.Length; m++)
				{
					tChunks[m].vel *= 0.95f;
					tChunks[m].vel.x += tentacleDir * 0.6f;
					bool flag3 = tentacleNumber == 0;
					if (vulture.IsMiros)
					{
						flag3 = tentacleNumber % 2 == 0;
					}
					Vector2 a = connectedChunk.pos - Custom.DirVec(connectedChunk.pos, flag3 ? vulture.bodyChunks[3].pos : vulture.bodyChunks[2].pos) * idealLength * tChunks[m].tPos;
					a = Vector2.Lerp(a, connectedChunk.pos + new Vector2(tentacleDir * idealLength * tChunks[m].tPos, 0f), 0.5f);
					Vector2 vector2 = Custom.PerpendicularVector((connectedChunk.pos - a).normalized) * (flag3 ? (-1f) : 1f);
					a += vector2 * Mathf.Sin((float)Math.PI * 2f * (vulture.wingFlap - tChunks[m].tPos * 0.5f)) * Mathf.Lerp(200f, 600f, vulture.wingFlapAmplitude);
					tChunks[m].vel += Vector2.ClampMagnitude(a - tChunks[m].pos, 30f) / 30f * 5f * Mathf.Lerp(0.2f, 1f, vulture.wingFlapAmplitude);
					if (tChunks[m].contactPoint.x != 0 || tChunks[m].contactPoint.y != 0)
					{
						flag2 = true;
					}
				}
				float num4 = 0.5f;
				if (vulture.IsMiros)
				{
					num4 = 1.4f / (float)vulture.tentacles.Length;
				}
				vulture.bodyChunks[1].vel.y += Mathf.Pow(num4 + num4 * Mathf.Sin((float)Math.PI * 2f * vulture.wingFlap), 2f) * 5.6f * Mathf.Lerp(0.5f, 1f, vulture.wingFlapAmplitude);
				vulture.bodyChunks[1].vel.x += (num4 + num4 * Mathf.Sin((float)Math.PI * 2f * vulture.wingFlap)) * -2.6f * tentacleDir * Mathf.Lerp(0.5f, 1f, vulture.wingFlapAmplitude);
				if (OtherTentacle.stun > 0 && stun < 1)
				{
					for (int n = 0; n < 4; n++)
					{
						vulture.bodyChunks[n].vel += Custom.DirVec(base.Tip.pos, vulture.bodyChunks[n].pos) * Mathf.Pow(num4 + num4 * Mathf.Sin((float)Math.PI * 2f * vulture.wingFlap), 2f) * 0.4f * Mathf.Lerp(0.5f, 1f, vulture.wingFlapAmplitude);
					}
				}
				if (flag2)
				{
					framesOfHittingTerrain++;
				}
				else
				{
					framesOfHittingTerrain--;
				}
				framesOfHittingTerrain = Custom.IntClamp(framesOfHittingTerrain, 0, 30);
				if (framesOfHittingTerrain >= 30)
				{
					framesOfHittingTerrain = 0;
					SwitchMode(Mode.Climb);
				}
				else if (OtherTentacle.mode == Mode.Climb)
				{
					UpdateDesiredGrabPos();
					FindGrabPos(ref scratchPath);
					if (floatGrabDest.HasValue)
					{
						SwitchMode(Mode.Climb);
					}
				}
			}
		}
		wooshSound.volume = Custom.SCurve(Mathf.InverseLerp(0.4f, 18f, Vector2.Distance(base.Tip.pos - connectedChunk.pos, base.Tip.lastPos - connectedChunk.lastPos)), 0.6f) * flyingMode;
		wooshSound.pitch = Mathf.Lerp(0.3f, 1.7f, Mathf.InverseLerp(-20f, 20f, base.Tip.lastPos.y - base.Tip.pos.y - (connectedChunk.lastPos.y - connectedChunk.pos.y)));
		wooshSound.pos = Vector2.Lerp(connectedChunk.pos, base.Tip.pos, 0.7f);
		wooshSound.Update();
		if (debugViz)
		{
			grabGoalSprites[1].pos = desiredGrabPos;
		}
	}

	public void SwitchMode(Mode newMode)
	{
		mode = newMode;
		if (newMode == Mode.Fly)
		{
			if (vulture.IsMiros)
			{
				ReleaseGrip();
			}
			floatGrabDest = null;
		}
	}

	public void ReleaseGrip()
	{
		if (OtherTentacle.grabDelay < 1)
		{
			grabDelay = 10;
		}
		floatGrabDest = null;
	}

	private void UpdateDesiredGrabPos()
	{
		if (vulture.hoverStill)
		{
			desiredGrabPos = vulture.mainBodyChunk.pos + new Vector2(tentacleDir, -0.8f).normalized * idealLength * 0.7f;
		}
		else
		{
			desiredGrabPos = vulture.mainBodyChunk.pos + (Vector2)Vector3.Slerp(vulture.moveDirection, new Vector2(tentacleDir, -0.8f).normalized, 0.3f) * idealLength * 0.7f;
		}
	}

	private void FindGrabPos(ref List<IntVector2> path)
	{
		if (grabDelay > 0)
		{
			grabDelay--;
			return;
		}
		IntVector2? intVector = ClosestSolid(room.GetTilePosition(desiredGrabPos), 8, 8f);
		if (intVector.HasValue)
		{
			IntVector2? intVector2 = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, base.BasePos, intVector.Value);
			if (!base.grabDest.HasValue || GripPointAttractiveness(intVector2.Value) > GripPointAttractiveness(base.grabDest.Value))
			{
				Vector2 newGrabDest = Custom.RestrictInRect(base.FloatBase, FloatRect.MakeFromVector2(room.MiddleOfTile(intVector2.Value) - new Vector2(11f, 11f), room.MiddleOfTile(intVector2.Value) + new Vector2(11f, 11f)));
				MoveGrabDest(newGrabDest, ref path);
			}
		}
		Vector2 pos = desiredGrabPos + Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * idealLength;
		int num = room.RayTraceTilesList(base.BasePos.x, base.BasePos.y, room.GetTilePosition(pos).x, room.GetTilePosition(pos).y, ref path);
		for (int i = 0; i < num && !room.GetTile(path[i]).Solid; i++)
		{
			if ((room.GetTile(path[i]).horizontalBeam || room.GetTile(path[i]).verticalBeam) && (!base.grabDest.HasValue || GripPointAttractiveness(path[i]) > GripPointAttractiveness(base.grabDest.Value)))
			{
				MoveGrabDest(room.MiddleOfTile(path[i]), ref path);
				break;
			}
		}
	}

	public float ReleaseScore()
	{
		if (mode != Mode.Climb)
		{
			return float.MinValue;
		}
		float num = Vector2.Distance(base.Tip.pos, desiredGrabPos);
		if (!floatGrabDest.HasValue)
		{
			num *= 2f;
		}
		return num;
	}

	private bool GripTerrain(int chunk)
	{
		for (int i = 0; i < 4; i++)
		{
			if (room.GetTile(room.GetTilePosition(tChunks[chunk].pos) + Custom.fourDirections[i]).Solid)
			{
				tChunks[chunk].vel *= 0.25f;
				tChunks[chunk].vel += Custom.fourDirections[i].ToVector2() * 0.8f;
				if (tChunks[chunk].contactPoint.x == 0)
				{
					return tChunks[chunk].contactPoint.y != 0;
				}
				return true;
			}
		}
		if (room.GetTile(tChunks[chunk].pos).horizontalBeam)
		{
			tChunks[chunk].vel *= 0.25f;
			tChunks[chunk].vel.y += (room.MiddleOfTile(tChunks[chunk].pos).y - tChunks[chunk].pos.y) * 0.3f;
			return true;
		}
		if (room.GetTile(tChunks[chunk].pos).verticalBeam)
		{
			tChunks[chunk].vel *= 0.25f;
			tChunks[chunk].vel.x += (room.MiddleOfTile(tChunks[chunk].pos).x - tChunks[chunk].pos.x) * 0.3f;
			return true;
		}
		return false;
	}

	private float GripPointAttractiveness(IntVector2 pos)
	{
		if (room.GetTile(pos).Solid)
		{
			return 100f / room.GetTilePosition(desiredGrabPos).FloatDist(pos);
		}
		return 65f / room.GetTilePosition(desiredGrabPos).FloatDist(pos);
	}

	public float Support()
	{
		if (stun > 0)
		{
			return 0f;
		}
		if (mode == Mode.Climb)
		{
			return Mathf.Clamp(((!hasAnyGrip) ? 0f : (vulture.IsMiros ? 4f : 0.5f)) + (float)segmentsGrippingTerrain / (float)tChunks.Length, 0f, 1f);
		}
		if (mode == Mode.Fly)
		{
			if (!vulture.IsMiros)
			{
				return 0.5f;
			}
			return 1.2f;
		}
		return 0f;
	}

	private IntVector2? ClosestSolid(IntVector2 goal, int maxDistance, float maxDistFromBase)
	{
		if (room.GetTile(goal).Solid)
		{
			return goal;
		}
		for (int i = 1; i <= maxDistance; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				if (room.GetTile(goal + Custom.eightDirections[j] * i).Solid && base.BasePos.FloatDist(goal + Custom.eightDirections[j] * i) < maxDistFromBase)
				{
					return goal + Custom.eightDirections[j] * i;
				}
			}
		}
		return null;
	}

	public bool WingSpace()
	{
		for (int i = -1; i <= 1; i++)
		{
			if (!SharedPhysics.RayTraceTilesForTerrain(room, room.GetTilePosition(connectedChunk.pos), room.GetTilePosition(connectedChunk.pos + new Vector2(tentacleDir * idealLength, 50f * (float)i))))
			{
				return false;
			}
		}
		return true;
	}

	protected override IntVector2 GravityDirection()
	{
		if (!(UnityEngine.Random.value < 0.5f))
		{
			return new IntVector2(0, -1);
		}
		return new IntVector2((int)tentacleDir, -1);
	}

	public void Damage(Creature.DamageType type, float damage, float stunBonus)
	{
		damage /= 2.7f;
		stunBonus /= 1.2f;
		stun = Math.Max(stun, (int)(damage * 30f + stunBonus));
		(vulture.State as Vulture.VultureState).wingHealth[tentacleNumber] -= damage;
	}
}
