using System;
using Noise;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class ElectricSpear : Spear
{
	private int segments;

	private float[] fluxSpeeds;

	private float[] fluxTimers;

	public Vector2 sparkPoint;

	public bool exploded;

	public int destroyCounter;

	public Color electricColor;

	private Color blackColor;

	private float zapPitch;

	public bool didZapCoilCheck;

	public ElectricSpear(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		segments = 4;
		sparkPoint = Vector2.zero;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		electricColor = Custom.HSL2RGB(UnityEngine.Random.Range(0.55f, 0.7f), UnityEngine.Random.Range(0.8f, 1f), UnityEngine.Random.Range(0.3f, 0.6f));
		UnityEngine.Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		fluxSpeeds = new float[segments];
		fluxTimers = new float[segments];
		for (int i = 0; i < fluxSpeeds.Length; i++)
		{
			ResetFluxSpeed(i);
		}
		UnityEngine.Random.state = state;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < fluxSpeeds.Length; i++)
		{
			fluxTimers[i] += fluxSpeeds[i];
			if (fluxTimers[i] > (float)Math.PI * 2f)
			{
				ResetFluxSpeed(i);
			}
		}
		if (UnityEngine.Random.value < 0.025f)
		{
			Spark();
		}
		if (exploded)
		{
			destroyCounter++;
			for (int j = 0; j < 2; j++)
			{
				room.AddObject(new ExplosiveSpear.SpearFragment(base.firstChunk.pos, Custom.RNV() * Mathf.Lerp(20f, 40f, UnityEngine.Random.value)));
			}
			if (destroyCounter > 4)
			{
				room.PlaySound(SoundID.Zapper_Zap, sparkPoint, 1f, 0.4f + 0.25f * UnityEngine.Random.value);
				Destroy();
			}
		}
		if (!stuckInWall.HasValue)
		{
			didZapCoilCheck = false;
		}
		if (!stuckInWall.HasValue || didZapCoilCheck)
		{
			return;
		}
		for (int k = 0; k < room.zapCoils.Count; k++)
		{
			ZapCoil zapCoil = room.zapCoils[k];
			if (zapCoil.turnedOn > 0.5f && zapCoil.GetFloatRect.Vector2Inside(base.firstChunk.pos + rotation * 30f))
			{
				if (base.abstractSpear.electricCharge == 0)
				{
					Recharge();
				}
				else
				{
					ExplosiveShortCircuit();
				}
				didZapCoilCheck = true;
				break;
			}
		}
	}

	public void Spark()
	{
		if (base.abstractSpear.electricCharge != 0)
		{
			for (int i = 0; i < 10; i++)
			{
				Vector2 vector = Custom.RNV();
				room.AddObject(new Spark(sparkPoint + vector * UnityEngine.Random.value * 20f, vector * Mathf.Lerp(4f, 10f, UnityEngine.Random.value), Color.white, null, 4, 18));
			}
		}
	}

	public void Zap()
	{
		if (base.abstractSpear.electricCharge != 0)
		{
			room.AddObject(new ZapCoil.ZapFlash(sparkPoint, 10f));
			room.PlaySound(SoundID.Zapper_Zap, sparkPoint, 1f, (zapPitch == 0f) ? (1.5f + UnityEngine.Random.value * 1.5f) : zapPitch);
			if (base.Submersion > 0.5f)
			{
				room.AddObject(new UnderwaterShock(room, null, base.firstChunk.pos, 10, 800f, 2f, thrownBy, new Color(0.8f, 0.8f, 1f)));
			}
		}
	}

	public void Electrocute(PhysicalObject otherObject)
	{
		if (!(otherObject is Creature))
		{
			return;
		}
		bool flag = CheckElectricCreature(otherObject as Creature);
		if (flag && base.abstractSpear.electricCharge == 0)
		{
			Recharge();
		}
		else
		{
			if (base.abstractSpear.electricCharge == 0)
			{
				return;
			}
			if (!(otherObject is BigEel) && !flag)
			{
				(otherObject as Creature).Violence(base.firstChunk, Custom.DirVec(base.firstChunk.pos, otherObject.firstChunk.pos) * 5f, otherObject.firstChunk, null, Creature.DamageType.Electric, 0.1f, (!(otherObject is Player)) ? (320f * Mathf.Lerp((otherObject as Creature).Template.baseStunResistance, 1f, 0.5f)) : 140f);
				room.AddObject(new CreatureSpasmer(otherObject as Creature, allowDead: false, (otherObject as Creature).stun));
			}
			bool flag2 = false;
			if (base.Submersion <= 0.5f && otherObject.Submersion > 0.5f)
			{
				room.AddObject(new UnderwaterShock(room, null, otherObject.firstChunk.pos, 10, 800f, 2f, thrownBy, new Color(0.8f, 0.8f, 1f)));
				flag2 = true;
			}
			room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, base.firstChunk.pos);
			room.AddObject(new Explosion.ExplosionLight(base.firstChunk.pos, 200f, 1f, 4, new Color(0.7f, 1f, 1f)));
			for (int i = 0; i < 15; i++)
			{
				Vector2 vector = Custom.DegToVec(360f * UnityEngine.Random.value);
				room.AddObject(new MouseSpark(base.firstChunk.pos + vector * 9f, base.firstChunk.vel + vector * 36f * UnityEngine.Random.value, 20f, new Color(0.7f, 1f, 1f)));
			}
			if (UnityEngine.Random.value < (flag ? 0.4f : ((base.abstractSpear.electricCharge == 1) ? 0.2f : 0f)) || flag2)
			{
				if (flag)
				{
					ExplosiveShortCircuit();
					return;
				}
				ShortCircuit();
			}
			if (base.abstractSpear.electricCharge > 1)
			{
				base.abstractSpear.electricCharge--;
			}
		}
	}

	public override void WeaponDeflect(Vector2 inbetweenPos, Vector2 deflectDir, float bounceSpeed)
	{
		base.WeaponDeflect(inbetweenPos, deflectDir, bounceSpeed);
		Zap();
	}

	public override void ChangeMode(Mode newMode)
	{
		if (newMode == Mode.StuckInCreature || newMode == Mode.StuckInWall)
		{
			if (newMode == Mode.StuckInCreature && stuckInObject != null && stuckInObject is Creature && CheckElectricCreature(stuckInObject as Creature))
			{
				zapPitch = 4f + UnityEngine.Random.value * 3f;
			}
			if (newMode == Mode.StuckInWall)
			{
				for (int i = 0; i < room.zapCoils.Count; i++)
				{
					ZapCoil zapCoil = room.zapCoils[i];
					if (zapCoil.turnedOn > 0.5f && zapCoil.GetFloatRect.Vector2Inside(base.firstChunk.pos + rotation * 30f))
					{
						if (base.abstractSpear.electricCharge == 0)
						{
							Recharge();
						}
						else
						{
							ExplosiveShortCircuit();
						}
						didZapCoilCheck = true;
						break;
					}
				}
			}
			if (!didZapCoilCheck)
			{
				Zap();
			}
			zapPitch = 0f;
		}
		base.ChangeMode(newMode);
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
		Spark();
	}

	public override void HitSomethingWithoutStopping(PhysicalObject obj, BodyChunk chunk, Appendage appendage)
	{
		base.HitSomethingWithoutStopping(obj, chunk, appendage);
		Spark();
	}

	public Vector2 PointAlongSpear(RoomCamera.SpriteLeaser sLeaser, float percent)
	{
		float height = sLeaser.sprites[0].element.sourceRect.height;
		return new Vector2(base.firstChunk.pos.x, base.firstChunk.pos.y) - Custom.DegToVec(sLeaser.sprites[0].rotation) * height * sLeaser.sprites[0].anchorY + Custom.DegToVec(sLeaser.sprites[0].rotation) * height * percent;
	}

	private void ResetFluxSpeed(int ind)
	{
		fluxSpeeds[ind] = UnityEngine.Random.value * 0.2f + 0.025f;
		while (fluxTimers[ind] > (float)Math.PI * 2f)
		{
			fluxTimers[ind] -= (float)Math.PI * 2f;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1 + segments];
		sLeaser.sprites[0] = new FSprite("SmallSpear");
		for (int i = 0; i < segments; i++)
		{
			if (i == 0)
			{
				sLeaser.sprites[1 + i] = new FSprite("ShortcutArrow");
			}
			if (i == segments - 1)
			{
				if (UnityEngine.Random.value < 0.5f)
				{
					sLeaser.sprites[1 + i] = new FSprite("Pebble10");
				}
				else
				{
					sLeaser.sprites[1 + i] = new FSprite("Pebble9");
				}
			}
			else
			{
				sLeaser.sprites[1 + i] = new FSprite("Pebble" + UnityEngine.Random.Range(1, 12));
			}
			sLeaser.sprites[1 + i].scale = Mathf.Lerp(0.4f, 1f, (float)i / (float)segments);
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (blink > 0)
		{
			if (blink > 1 && UnityEngine.Random.value < 0.5f)
			{
				sLeaser.sprites[0].color = new Color(1f, 1f, 1f);
			}
			else
			{
				sLeaser.sprites[0].color = color;
			}
		}
		for (int i = 0; i < segments; i++)
		{
			Vector2 vector = ZapperAttachPos(timeStacker, i);
			sLeaser.sprites[1 + i].x = vector.x - camPos.x;
			sLeaser.sprites[1 + i].y = vector.y - camPos.y;
			sLeaser.sprites[1 + i].rotation = sLeaser.sprites[0].rotation;
			if (base.abstractSpear.electricCharge == 0)
			{
				sLeaser.sprites[1 + i].color = blackColor;
			}
			else
			{
				sLeaser.sprites[1 + i].color = Color.Lerp(electricColor, Color.white, Mathf.Abs(Mathf.Sin(fluxTimers[i])));
			}
		}
		sparkPoint = PointAlongSpear(sLeaser, 0.9f);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		sLeaser.sprites[0].color = color;
		blackColor = palette.blackColor;
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		sLeaser.sprites[0].RemoveFromContainer();
		newContatiner.AddChild(sLeaser.sprites[0]);
		for (int num = sLeaser.sprites.Length - 1; num >= 1; num--)
		{
			sLeaser.sprites[num].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[num]);
		}
	}

	public void ShortCircuit()
	{
		if (base.abstractSpear.electricCharge != 0)
		{
			Vector2 pos = base.firstChunk.pos;
			room.AddObject(new Explosion.ExplosionLight(pos, 40f, 1f, 2, electricColor));
			for (int i = 0; i < 8; i++)
			{
				Vector2 vector = Custom.RNV();
				room.AddObject(new Spark(pos + vector * UnityEngine.Random.value * 10f, vector * Mathf.Lerp(6f, 18f, UnityEngine.Random.value), electricColor, null, 4, 18));
			}
			room.AddObject(new ShockWave(pos, 30f, 0.035f, 2));
			room.PlaySound(SoundID.Fire_Spear_Pop, pos);
			room.PlaySound(SoundID.Firecracker_Bang, pos);
			room.InGameNoise(new InGameNoise(pos, 800f, this, 1f));
			vibrate = Math.Max(vibrate, 6);
			base.abstractSpear.electricCharge = 0;
		}
	}

	public Vector2 ZapperAttachPos(float timeStacker, int node)
	{
		Vector3 vector = Vector3.Slerp(lastRotation, rotation, timeStacker);
		Vector3 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker) * node * -4f;
		return Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker) + new Vector2(vector.x, vector.y) * 30f + new Vector2(vector2.x, vector2.y);
	}

	public void ExplosiveShortCircuit()
	{
		if (base.abstractSpear.electricCharge != 0)
		{
			Vector2 pos = base.firstChunk.pos;
			exploded = true;
			room.AddObject(new Explosion.ExplosionLight(pos, 40f, 1f, 2, electricColor));
			for (int i = 0; i < 8; i++)
			{
				Vector2 vector = Custom.RNV();
				room.AddObject(new Spark(pos + vector * UnityEngine.Random.value * 10f, vector * Mathf.Lerp(6f, 18f, UnityEngine.Random.value), electricColor, null, 4, 18));
			}
			room.AddObject(new ShockWave(pos, 30f, 0.035f, 2));
			room.PlaySound(SoundID.Fire_Spear_Pop, pos);
			room.PlaySound(SoundID.Firecracker_Bang, pos);
			room.InGameNoise(new InGameNoise(pos, 800f, this, 1f));
			vibrate = Math.Max(vibrate, 6);
		}
	}

	public void Recharge()
	{
		if (base.abstractSpear.electricCharge == 0)
		{
			base.abstractSpear.electricCharge = 3;
			room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, base.firstChunk.pos);
			room.AddObject(new Explosion.ExplosionLight(base.firstChunk.pos, 200f, 1f, 4, new Color(0.7f, 1f, 1f)));
			Spark();
			Zap();
			room.AddObject(new ZapCoil.ZapFlash(sparkPoint, 25f));
		}
	}

	public bool CheckElectricCreature(Creature otherObject)
	{
		if (!(otherObject is Centipede) && !(otherObject is BigJellyFish))
		{
			return otherObject is Inspector;
		}
		return true;
	}
}
