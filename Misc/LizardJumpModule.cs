using System;
using System.Collections.Generic;
using RWCustom;
using Smoke;
using UnityEngine;

public class LizardJumpModule
{
	public class JumpFinder
	{
		public struct JumpInstruction : IEquatable<JumpInstruction>
		{
			public PathFinder.PathingCell goalCell;

			public int tick;

			public Vector2 startPos;

			public Vector2 initVel;

			public float power;

			public bool grabWhenLanding;

			public bool Equals(JumpInstruction other)
			{
				if (object.Equals(goalCell, other.goalCell) && tick == other.tick && startPos.Equals(other.startPos) && initVel.Equals(other.initVel) && power.Equals(other.power))
				{
					return grabWhenLanding == other.grabWhenLanding;
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is JumpInstruction other)
				{
					return Equals(other);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (((((((((((goalCell != null) ? goalCell.GetHashCode() : 0) * 397) ^ tick) * 397) ^ startPos.GetHashCode()) * 397) ^ initVel.GetHashCode()) * 397) ^ power.GetHashCode()) * 397) ^ grabWhenLanding.GetHashCode();
			}

			public static bool operator ==(JumpInstruction left, JumpInstruction right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(JumpInstruction left, JumpInstruction right)
			{
				return !left.Equals(right);
			}

			public JumpInstruction(Vector2 startPos, Vector2 initVel, float power)
			{
				this.startPos = startPos;
				this.initVel = initVel;
				this.power = power;
				goalCell = null;
				tick = 0;
				grabWhenLanding = false;
			}
		}

		private LizardJumpModule owner;

		public bool slatedForDeletion;

		public Room room;

		public IntVector2 startPos;

		public int fade;

		private bool visualize;

		public DebugSprite[] dbsprts;

		private PathFinder.PathingCell startCell;

		public JumpInstruction bestJump;

		public JumpInstruction currentJump;

		private Vector2 pos;

		private Vector2 lastPos;

		private Vector2 vel;

		private bool hasVenturedAwayFromTerrain;

		public Vector2? landingDirection;

		public Vector2 lastControlledDir;

		public bool chainJump;

		private IntVector2[] _cachedTls = new IntVector2[100];

		public bool BeneficialMovement
		{
			get
			{
				if (bestJump != default(JumpInstruction) && bestJump.goalCell != null && startPos.FloatDist(bestJump.goalCell.worldCoordinate.Tile) > 5f && owner.lizard.AI.pathFinder.GetDestination.Tile.FloatDist(bestJump.goalCell.worldCoordinate.Tile) > 3f && !PathWeightComparison(bestJump.goalCell, owner.lizardCell))
				{
					return !PathWeightComparison(bestJump.goalCell, startCell);
				}
				return false;
			}
		}

		public JumpFinder(Room room, LizardJumpModule owner, IntVector2 startPos, bool chainJump)
		{
			this.room = room;
			this.owner = owner;
			this.startPos = startPos;
			this.chainJump = chainJump;
			startCell = owner.lizard.AI.pathFinder.PathingCellAtWorldCoordinate(room.GetWorldCoordinate(startPos));
			if (visualize)
			{
				dbsprts = new DebugSprite[4];
				dbsprts[0] = new DebugSprite(default(Vector2), new FSprite("pixel"), room);
				dbsprts[0].sprite.scale = 10f;
				dbsprts[0].sprite.alpha = 0.5f;
				dbsprts[0].pos = room.MiddleOfTile(startPos);
				dbsprts[1] = new DebugSprite(default(Vector2), new FSprite("pixel"), room);
				dbsprts[1].sprite.scale = 10f;
				dbsprts[1].sprite.alpha = 0.5f;
				dbsprts[2] = new DebugSprite(default(Vector2), new FSprite("pixel"), room);
				dbsprts[2].sprite.scaleX = 2f;
				dbsprts[2].sprite.anchorY = 0f;
				dbsprts[3] = new DebugSprite(default(Vector2), new FSprite("pixel"), room);
				dbsprts[3].sprite.scale = 5f;
				for (int i = 0; i < dbsprts.Length; i++)
				{
					room.AddObject(dbsprts[i]);
				}
			}
			NewTest();
		}

		public void Update()
		{
			if (owner.InStandardRunMode)
			{
				fade++;
				if (!PathWeightComparison(owner.lizardCell, startCell))
				{
					fade += 10;
				}
				if (fade > 40)
				{
					Destroy();
				}
			}
			if (owner.lizard.safariControlled)
			{
				Vector2 vector = Vector2.zero;
				if (owner.lizard.inputWithDiagonals.HasValue)
				{
					vector = new Vector2(Mathf.Sign(owner.lizard.inputWithDiagonals.Value.x) * (float)((owner.lizard.inputWithDiagonals.Value.x == 1) ? 1 : 0), Mathf.Sign(owner.lizard.inputWithDiagonals.Value.y) * (float)((owner.lizard.inputWithDiagonals.Value.y == 1) ? 1 : 0));
				}
				if (vector != lastControlledDir)
				{
					lastControlledDir = vector;
					NewTest();
				}
			}
			for (int num = Math.Max(1, 100 / Math.Max(1, owner.jumpFinders.Count)); num >= 0; num--)
			{
				Iterate();
			}
			if (visualize)
			{
				if (BeneficialMovement)
				{
					dbsprts[0].sprite.color = new Color(0f, 1f, 0f);
					dbsprts[2].sprite.color = new Color(0f, 1f, 0f);
				}
				else
				{
					dbsprts[0].sprite.color = new Color(1f, 0f, 0f);
					dbsprts[2].sprite.color = new Color(1f, 0f, 0f);
				}
				dbsprts[3].pos = pos;
			}
			if (owner.actOnJump != this)
			{
				return;
			}
			PathFinder.PathingCell pathingCell = bestJump.goalCell;
			for (int i = 0; i < 8; i++)
			{
				PathFinder.PathingCell pathingCell2 = owner.lizard.AI.pathFinder.PathingCellAtWorldCoordinate(bestJump.goalCell.worldCoordinate + Custom.eightDirections[i]);
				if (PathWeightComparison(pathingCell, pathingCell2))
				{
					pathingCell = pathingCell2;
				}
			}
			if (pathingCell != bestJump.goalCell)
			{
				landingDirection = Custom.DirVec(room.MiddleOfTile(bestJump.goalCell.worldCoordinate), room.MiddleOfTile(pathingCell.worldCoordinate));
			}
		}

		private void Iterate()
		{
			lastPos = pos;
			pos += vel;
			vel *= 0.999f;
			vel.y -= 0.9f;
			int num;
			for (num = SharedPhysics.RayTracedTilesArray(lastPos, pos, _cachedTls); num >= _cachedTls.Length; num = SharedPhysics.RayTracedTilesArray(lastPos, pos, _cachedTls))
			{
				Custom.LogWarning($"Lizard JumpFinder ray tracing limit exceeded, extending cache to {_cachedTls.Length + 100} and trying again!");
				Array.Resize(ref _cachedTls, _cachedTls.Length + 100);
			}
			Vector2 vector = Custom.PerpendicularVector(lastPos, pos);
			for (int i = 0; i < num; i++)
			{
				if (room.GetTile(_cachedTls[i]).Solid || _cachedTls[i].y < 0 || _cachedTls[i].y < room.defaultWaterLevel || room.aimap.getAItile(_cachedTls[i]).narrowSpace)
				{
					NewTest();
					return;
				}
				if (!hasVenturedAwayFromTerrain && room.aimap.getTerrainProximity(_cachedTls[i]) > 1 && !room.GetTile(_cachedTls[i]).verticalBeam && !room.GetTile(_cachedTls[i]).horizontalBeam)
				{
					hasVenturedAwayFromTerrain = true;
				}
				if (hasVenturedAwayFromTerrain && room.aimap.TileAccessibleToCreature(_cachedTls[i], owner.lizard.Template) && (room.aimap.getTerrainProximity(_cachedTls[i]) == 1 || room.GetTile(_cachedTls[i]).verticalBeam || room.GetTile(_cachedTls[i]).horizontalBeam) && startPos.FloatDist(_cachedTls[i]) > (float)Custom.IntClamp((int)(currentJump.initVel.magnitude / 3f), 5, 20) && owner.lizard.AI.pathFinder.GetDestination.Tile.FloatDist(_cachedTls[i]) > 3f)
				{
					PathFinder.PathingCell pathingCell = owner.lizard.AI.pathFinder.PathingCellAtWorldCoordinate(room.GetWorldCoordinate(_cachedTls[i]));
					if (PathWeightComparison(bestJump.goalCell, pathingCell))
					{
						bestJump = currentJump;
						bestJump.goalCell = pathingCell;
						Vector2 vector2 = room.MiddleOfTile(pathingCell.worldCoordinate);
						Vector2 vector3 = Custom.DirVec(lastPos, pos);
						bestJump.grabWhenLanding = false;
						for (int j = -1; j < 2; j++)
						{
							if (!room.GetTile(vector2 + Custom.PerpendicularVector(vector3) * 15f + vector3 * 20f).Solid)
							{
								bestJump.grabWhenLanding = true;
								break;
							}
						}
						if (visualize)
						{
							dbsprts[1].pos = room.MiddleOfTile(pathingCell.worldCoordinate);
							dbsprts[2].pos = room.MiddleOfTile(startPos);
							dbsprts[2].sprite.rotation = Custom.VecToDeg(bestJump.initVel.normalized);
							dbsprts[2].sprite.scaleY = bestJump.initVel.magnitude;
							dbsprts[1].sprite.color = (bestJump.grabWhenLanding ? new Color(1f, 1f, 1f) : new Color(0f, 0f, 1f));
						}
					}
				}
				if ((!room.GetTile(startPos + new IntVector2(0, 1)).Solid && room.GetTile(_cachedTls[i] + new IntVector2(0, 1)).Solid) || room.GetTile(room.MiddleOfTile(_cachedTls[i]) + vector * Custom.LerpMap(currentJump.tick, 5f, 20f, 10f, 20f)).Solid || room.GetTile(room.MiddleOfTile(_cachedTls[i]) - vector * Custom.LerpMap(currentJump.tick, 5f, 20f, 10f, 20f)).Solid)
				{
					NewTest();
					return;
				}
			}
			currentJump.tick++;
			if (currentJump.tick > 700)
			{
				NewTest();
			}
		}

		private void NewTest()
		{
			float num = UnityEngine.Random.value * Mathf.Pow(owner.lizard.AI.runSpeed, 0.5f);
			if (room.aimap.getTerrainProximity(startPos) > 1)
			{
				num *= 0.5f;
			}
			if (chainJump)
			{
				num *= 0.5f;
			}
			float num2 = Mathf.Lerp(14f, 50f, num);
			if (owner.lizard.grasps[0] != null)
			{
				num2 *= 0.75f;
			}
			num2 *= Mathf.Lerp(1f, owner.lizard.LizardState.ClampedHealth, 0.5f + 0.5f * UnityEngine.Random.value);
			Vector2 initVel = Custom.DegToVec((45f * Mathf.Pow(UnityEngine.Random.value, 0.75f) + 135f * Mathf.Pow(UnityEngine.Random.value, 2f)) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f)) * num2;
			if (owner.lizard.safariControlled && owner.lizard.inputWithDiagonals.HasValue && (owner.lizard.inputWithDiagonals.Value.x != 0 || owner.lizard.inputWithDiagonals.Value.y != 0))
			{
				initVel = Custom.DegToVec(Custom.VecToDeg(new Vector2(owner.lizard.inputWithDiagonals.Value.x, owner.lizard.inputWithDiagonals.Value.y)) + 22.5f * UnityEngine.Random.value * ((UnityEngine.Random.value >= 0.5f) ? 1f : (-1f))) * num2;
			}
			currentJump = new JumpInstruction(room.MiddleOfTile(startPos), initVel, num);
			pos = room.MiddleOfTile(startPos);
			lastPos = pos;
			vel = currentJump.initVel;
			hasVenturedAwayFromTerrain = false;
			if (bestJump == default(JumpInstruction))
			{
				bestJump = currentJump;
			}
		}

		public void Destroy()
		{
			if (visualize)
			{
				for (int i = 0; i < dbsprts.Length; i++)
				{
					dbsprts[i].Destroy();
				}
			}
			slatedForDeletion = true;
		}
	}

	public class JumpLight : CosmeticSprite
	{
		public float life;

		public float lastLife;

		public float lifeTime;

		public float intensity;

		public LizardGraphics lizard;

		public JumpLight(Vector2 pos, LizardGraphics lizard, float intensity)
		{
			life = 1f;
			lastLife = 1f;
			base.pos = pos;
			lastPos = pos;
			this.lizard = lizard;
			lifeTime = Mathf.Lerp(4f, 22f, Mathf.Pow(intensity, 2f));
		}

		public override void Update(bool eu)
		{
			lastLife = life;
			life -= 1f / lifeTime;
			if (lastLife <= 0f)
			{
				Destroy();
			}
			base.Update(eu);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[3];
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i] = new FSprite("Futile_White");
			}
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["LightSource"];
			sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["FlatLight"];
			sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["FlatLight"];
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			float num = Mathf.Lerp(lastLife, life, timeStacker);
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].x = vector.x - camPos.x;
				sLeaser.sprites[i].y = vector.y - camPos.y;
			}
			float num2 = Mathf.Lerp(20f + 10f * intensity, 40f + 30f * intensity, num) + Mathf.Lerp(50f, 90f, intensity) * Mathf.Sin(Mathf.Pow(num, 2f) * (float)Math.PI);
			sLeaser.sprites[0].color = lizard.effectColor;
			sLeaser.sprites[0].scale = num2 * 2f / 8f;
			sLeaser.sprites[0].alpha = Mathf.Pow(Mathf.InverseLerp(0f, 0.5f, num), 0.5f);
			sLeaser.sprites[1].color = lizard.effectColor;
			sLeaser.sprites[1].scale = num2 / 8f;
			sLeaser.sprites[1].alpha = Mathf.Pow(num, 2f) * (0.4f + 0.4f * intensity);
			sLeaser.sprites[2].scale = num2 * Mathf.Lerp(0.4f, 0.8f, UnityEngine.Random.value) / 8f;
			sLeaser.sprites[2].alpha = Mathf.Pow(Mathf.InverseLerp(0.25f, 1f, num), 3f) * intensity;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Bloom");
			}
			FSprite[] sprites = sLeaser.sprites;
			foreach (FSprite fSprite in sprites)
			{
				fSprite.RemoveFromContainer();
				newContatiner.AddChild(fSprite);
			}
		}
	}

	private Lizard lizard;

	public List<JumpFinder> jumpFinders;

	public JumpFinder actOnJump;

	public PathFinder.PathingCell lizardCell;

	private int addDelay;

	public int jumpCounter;

	public int spin;

	public CyanLizardSmoke smoke;

	public Spear gasLeakSpear;

	public float gasLeakPower = 1f;

	public float gasLeakTime;

	public ChunkSoundEmitter gasLeakSound;

	public ChunkSoundEmitter chargeSound;

	public JumpFinder controlledJumpFinder;

	private List<WorldCoordinate> futurePath = new List<WorldCoordinate>(100);

	private Room room => lizard.room;

	public Vector2 jumpToPoint => room.MiddleOfTile(actOnJump.bestJump.goalCell.worldCoordinate);

	public bool InStandardRunMode
	{
		get
		{
			if (actOnJump == null && lizard.animation != Lizard.Animation.PrepareToJump)
			{
				return lizard.animation != Lizard.Animation.Jumping;
			}
			return false;
		}
	}

	public bool canChainJump
	{
		get
		{
			if (lizard.Template.type == CreatureTemplate.Type.CyanLizard)
			{
				return lizard.grasps[0] == null;
			}
			return false;
		}
	}

	public bool NoRunBehavior
	{
		get
		{
			if (actOnJump != null && lizard.animation == Lizard.Animation.Jumping)
			{
				return jumpCounter < actOnJump.bestJump.tick;
			}
			return false;
		}
	}

	public LizardJumpModule(Lizard lizard)
	{
		this.lizard = lizard;
		jumpFinders = new List<JumpFinder>();
		gasLeakTime = Mathf.Lerp(100f, 600f, UnityEngine.Random.value);
	}

	public void Update()
	{
		if (room == null || lizard.dead)
		{
			return;
		}
		if (chargeSound != null)
		{
			chargeSound.alive = true;
			if (lizard.Consious && (lizard.animation == Lizard.Animation.PrepareToJump || lizard.animation == Lizard.Animation.Jumping))
			{
				chargeSound.volume = Mathf.Min(1f, chargeSound.volume + 1f / 9f);
				chargeSound.pitch = Mathf.Min(1f, chargeSound.pitch + 1f / 9f);
			}
			else
			{
				chargeSound.volume = Mathf.Max(0f, chargeSound.volume - 1f / 9f);
				chargeSound.pitch = Mathf.Max(0.5f, chargeSound.pitch - 1f / 9f);
				if (chargeSound.volume <= 0f)
				{
					chargeSound.alive = false;
					chargeSound = null;
				}
			}
		}
		if (!lizard.Consious && (lizard.animation == Lizard.Animation.PrepareToJump || lizard.animation == Lizard.Animation.Jumping))
		{
			lizard.EnterAnimation(Lizard.Animation.Standard, forceAnimationChange: true);
		}
		if (gasLeakSpear != null)
		{
			if (gasLeakSound == null)
			{
				gasLeakSound = room.PlaySound(SoundID.Cyan_Lizard_Gas_Leak_LOOP, gasLeakSpear.firstChunk, loop: true, 1f, 1f);
				gasLeakSound.requireActiveUpkeep = true;
			}
			else
			{
				gasLeakSound.alive = true;
				gasLeakSound.volume = 1f;
				gasLeakSound.pitch = 0.5f + 0.75f * Mathf.Clamp01(gasLeakPower);
			}
			if (gasLeakSpear.stuckInObject == null || gasLeakSpear.stuckInChunk.owner != lizard || gasLeakSpear.mode != Weapon.Mode.StuckInCreature || gasLeakPower <= 0f || UnityEngine.Random.value < 0.005f)
			{
				if (gasLeakPower > 0.2f)
				{
					gasLeakSpear.ChangeMode(Weapon.Mode.Free);
					gasLeakSpear.firstChunk.vel += -gasLeakSpear.rotation * Mathf.Lerp(21f, 31f, UnityEngine.Random.value);
					if (lizard.graphicsModule != null && smoke != null)
					{
						smoke.EmitSmoke(gasLeakSpear.firstChunk.pos, gasLeakSpear.rotation * -20f * UnityEngine.Random.value + Custom.RNV() * UnityEngine.Random.value * 14f, lizard.graphicsModule as LizardGraphics, big: true, 60f);
					}
				}
				gasLeakSpear = null;
				gasLeakTime = Mathf.Lerp(100f, 600f, UnityEngine.Random.value);
			}
			else
			{
				gasLeakPower -= 1f / gasLeakTime;
				float num = Mathf.Pow(Mathf.Clamp01(gasLeakPower), 0.5f) * Mathf.Pow(UnityEngine.Random.value, 0.5f);
				if (UnityEngine.Random.value < num)
				{
					lizard.inAllowedTerrainCounter = 0;
				}
				if (UnityEngine.Random.value < num * Mathf.InverseLerp(1f, 0.9f, gasLeakPower) / 15f)
				{
					Vector2 vector = lizard.bodyChunks[UnityEngine.Random.Range(0, 2)].pos + Custom.DegToVec(Mathf.Pow(UnityEngine.Random.value, 0.25f) * 180f * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f)) * 16f * UnityEngine.Random.value;
					room.PlaySound(SoundID.Cyan_Lizard_Small_Jump, lizard.mainBodyChunk);
					room.AddObject(new JumpLight(vector, lizard.graphicsModule as LizardGraphics, 0.2f));
					room.AddObject(new ShockWave(vector, 50f, 0.07f, 7));
					if (lizard.graphicsModule != null && smoke != null)
					{
						for (int i = 0; i < 4; i++)
						{
							smoke.EmitSmoke(gasLeakSpear.firstChunk.pos, gasLeakSpear.rotation * Mathf.Lerp(-7f, -20f, num) + Custom.RNV() * UnityEngine.Random.value * 14f, lizard.graphicsModule as LizardGraphics, big: true, 60f);
						}
					}
					for (int j = 0; j < lizard.bodyChunks.Length; j++)
					{
						if (!room.GetTile(lizard.bodyChunks[j].pos + Custom.DirVec(vector, lizard.bodyChunks[j].pos) * 25f).Solid)
						{
							lizard.bodyChunks[j].vel += Custom.DirVec(vector, lizard.bodyChunks[j].pos) * Mathf.Lerp(19f, 25f, num);
						}
					}
					lizard.Stun(25);
				}
				gasLeakSpear.stuckInChunk.vel += 9f * gasLeakSpear.rotation * num;
				if (lizard.graphicsModule != null)
				{
					if (smoke == null)
					{
						smoke = new CyanLizardSmoke(lizard.room);
						lizard.room.AddObject(smoke);
					}
					smoke.EmitSmoke(gasLeakSpear.firstChunk.pos, gasLeakSpear.rotation * Mathf.Lerp(-20f, -60f, num) + Custom.RNV() * UnityEngine.Random.value * 4f, lizard.graphicsModule as LizardGraphics, UnityEngine.Random.value < num / 20f, 50f);
				}
			}
		}
		else if (gasLeakSound != null)
		{
			gasLeakSound.alive = false;
			gasLeakSound = null;
		}
		for (int num2 = jumpFinders.Count - 1; num2 >= 0; num2--)
		{
			if (jumpFinders[num2].slatedForDeletion)
			{
				jumpFinders.RemoveAt(num2);
			}
			else if (lizard.safariControlled)
			{
				jumpFinders[num2].Destroy();
			}
			else
			{
				jumpFinders[num2].Update();
			}
		}
		if (lizard.safariControlled)
		{
			if (controlledJumpFinder != null && controlledJumpFinder.startPos != lizard.abstractCreature.pos.Tile)
			{
				controlledJumpFinder.Destroy();
				controlledJumpFinder = null;
			}
			if (controlledJumpFinder == null)
			{
				controlledJumpFinder = new JumpFinder(room, this, lizard.abstractCreature.pos.Tile, chainJump: false);
			}
			controlledJumpFinder.Update();
			controlledJumpFinder.fade = 0;
			if (lizard.inputWithDiagonals.HasValue && lizard.inputWithDiagonals.Value.thrw && !lizard.lastInputWithDiagonals.Value.thrw && controlledJumpFinder.bestJump != default(JumpFinder.JumpInstruction))
			{
				InitiateJump(controlledJumpFinder, chainJump: false);
			}
		}
		else if (controlledJumpFinder != null)
		{
			controlledJumpFinder.Destroy();
			controlledJumpFinder = null;
		}
		if (lizard.animation == Lizard.Animation.Jumping)
		{
			JumpingUpdate();
		}
		else if (InStandardRunMode)
		{
			RunningUpdate();
		}
		if (actOnJump != null && lizard.animation != Lizard.Animation.PrepareToJump && lizard.animation != Lizard.Animation.Jumping)
		{
			actOnJump.fade++;
			if (actOnJump.fade > 40)
			{
				actOnJump = null;
			}
		}
		if (smoke != null && (smoke.slatedForDeletetion || smoke.room != lizard.room))
		{
			smoke = null;
		}
	}

	private void JumpingUpdate()
	{
		jumpCounter++;
		bool flag = false;
		if (actOnJump == null)
		{
			flag = true;
		}
		else
		{
			if (smoke != null && (float)jumpCounter < 70f * actOnJump.bestJump.power)
			{
				smoke.EmitSmoke(lizard.bodyChunks[1].pos, lizard.bodyChunks[1].vel + Custom.DirVec(lizard.bodyChunks[1].pos, lizard.bodyChunks[2].pos) * 16f, lizard.graphicsModule as LizardGraphics, big: false, (70f * actOnJump.bestJump.power - (float)jumpCounter) * 1.5f);
			}
			if (jumpCounter > actOnJump.bestJump.tick + 5)
			{
				Custom.Log("missed goal, grab any terrain");
				for (int i = 0; i < lizard.bodyChunks.Length; i++)
				{
					if (room.aimap.TileAccessibleToCreature(lizard.bodyChunks[i].pos, lizard.Template))
					{
						lizard.bodyChunks[0].vel *= 0.5f;
						lizard.inAllowedTerrainCounter++;
					}
				}
			}
			else
			{
				lizard.inAllowedTerrainCounter = 0;
				if (jumpCounter > actOnJump.bestJump.tick - 30 && actOnJump.landingDirection.HasValue)
				{
					lizard.bodyChunks[0].vel += actOnJump.landingDirection.Value * 3f;
					lizard.bodyChunks[2].vel -= actOnJump.landingDirection.Value * 3f;
				}
				else if (spin == 0)
				{
					lizard.bodyChunks[0].vel += Custom.DirVec(lizard.bodyChunks[1].pos, jumpToPoint) * Custom.LerpMap(jumpCounter, 10f, 60f, 2f, 0f);
					lizard.bodyChunks[2].vel -= Custom.DirVec(lizard.bodyChunks[1].pos, jumpToPoint) * Custom.LerpMap(jumpCounter, 10f, 60f, 2f, 0f);
				}
				else if (room.aimap.getTerrainProximity(lizard.bodyChunks[1].pos) > 2)
				{
					Vector2 normalized = (Custom.PerpendicularVector(lizard.bodyChunks[2].pos, lizard.bodyChunks[1].pos) + Custom.PerpendicularVector(lizard.bodyChunks[1].pos, lizard.bodyChunks[0].pos)).normalized;
					lizard.bodyChunks[0].vel += normalized * spin * Custom.LerpMap(jumpCounter, 20f, 60f, 3f, 0f);
					lizard.bodyChunks[2].vel -= normalized * spin * Custom.LerpMap(jumpCounter, 20f, 60f, 3f, 0f);
					if (lizard.graphicsModule != null)
					{
						(lizard.graphicsModule as LizardGraphics).head.vel += normalized * spin * 50f;
						for (int j = 0; j < (lizard.graphicsModule as LizardGraphics).tail.Length; j++)
						{
							(lizard.graphicsModule as LizardGraphics).tail[j].vel += normalized * spin * Mathf.Sin(Mathf.InverseLerp(0f, (lizard.graphicsModule as LizardGraphics).tail.Length - 1, j) * (float)Math.PI) * 15f;
						}
					}
				}
			}
			for (int k = 0; k < lizard.bodyChunks.Length; k++)
			{
				if (!Custom.DistLess(lizard.bodyChunks[k].pos, jumpToPoint, 40f))
				{
					continue;
				}
				if (TryChainJump(jumpToPoint))
				{
					return;
				}
				if (actOnJump.bestJump.grabWhenLanding)
				{
					lizard.gripPoint = jumpToPoint;
				}
				flag = true;
				break;
			}
		}
		if (!flag && jumpCounter < actOnJump.bestJump.tick + 20)
		{
			return;
		}
		lizard.inAllowedTerrainCounter = lizard.lizardParams.regainFootingCounter + 5;
		if (actOnJump.bestJump.grabWhenLanding)
		{
			for (int l = 0; l < lizard.bodyChunks.Length; l++)
			{
				lizard.bodyChunks[l].vel *= 0.5f;
			}
		}
		actOnJump = null;
		lizard.EnterAnimation(Lizard.Animation.Standard, forceAnimationChange: true);
	}

	private void RunningUpdate()
	{
		if (!lizard.safariControlled)
		{
			for (int num = jumpFinders.Count - 1; num >= 0; num--)
			{
				if (!jumpFinders[num].slatedForDeletion && lizard.abstractCreature.pos.Tile == jumpFinders[num].startPos && jumpFinders[num].BeneficialMovement)
				{
					InitiateJump(jumpFinders[num], chainJump: false);
				}
			}
		}
		WorldCoordinate worldCoordinate = lizard.abstractCreature.pos;
		futurePath.Clear();
		bool flag = false;
		for (int i = 0; i < 10; i++)
		{
			MovementConnection movementConnection = (lizard.AI.pathFinder as LizardPather).FollowPath(worldCoordinate, null, actuallyFollowingThisPath: false);
			if (movementConnection == default(MovementConnection))
			{
				break;
			}
			worldCoordinate = movementConnection.destinationCoord;
			futurePath.Add(worldCoordinate);
			for (int j = 0; j < jumpFinders.Count; j++)
			{
				if (jumpFinders[j].startPos == worldCoordinate.Tile)
				{
					jumpFinders[j].fade = 0;
					flag = true;
				}
			}
		}
		if (lizard.AI.pathFinder.PathingCellAtWorldCoordinate(lizard.abstractCreature.pos) != lizard.AI.pathFinder.fallbackPathingCell)
		{
			lizardCell = lizard.AI.pathFinder.PathingCellAtWorldCoordinate(lizard.abstractCreature.pos);
		}
		if (addDelay > 0)
		{
			addDelay--;
		}
		else if (jumpFinders.Count < (flag ? 1 : 4) && futurePath.Count > 1)
		{
			WorldCoordinate worldCoordinate2 = futurePath[UnityEngine.Random.Range(0, futurePath.Count)];
			if (worldCoordinate2.TileDefined && worldCoordinate2.Tile.FloatDist(lizard.abstractCreature.pos.Tile) > 2f && !room.aimap.getAItile(worldCoordinate2).narrowSpace && PathWeightComparison(lizardCell, lizard.AI.pathFinder.PathingCellAtWorldCoordinate(worldCoordinate2)))
			{
				jumpFinders.Add(new JumpFinder(room, this, worldCoordinate2.Tile, chainJump: false));
			}
		}
	}

	public void InitiateJump(JumpFinder jump, bool chainJump)
	{
		if (jump != null && !(jump.bestJump == default(JumpFinder.JumpInstruction)) && jump.bestJump.goalCell != null)
		{
			actOnJump = jump;
			lizard.EnterAnimation(Lizard.Animation.PrepareToJump, forceAnimationChange: true);
			if (chainJump)
			{
				lizard.timeToRemainInAnimation = 10;
			}
			else if (jump.bestJump.power > 0.75f)
			{
				lizard.timeToRemainInAnimation = 45;
				chargeSound = room.PlaySound(SoundID.Cyan_Lizard_Prepare_Powerful_Jump, lizard.mainBodyChunk);
				chargeSound.requireActiveUpkeep = true;
			}
			else if (jump.bestJump.power > 0.25f)
			{
				lizard.timeToRemainInAnimation = 30;
				chargeSound = room.PlaySound(SoundID.Cyan_Lizard_Prepare_Medium_Jump, lizard.mainBodyChunk);
				chargeSound.requireActiveUpkeep = true;
			}
			else
			{
				lizard.timeToRemainInAnimation = 15;
				chargeSound = room.PlaySound(SoundID.Cyan_Lizard_Prepare_Small_Jump, lizard.mainBodyChunk);
				chargeSound.requireActiveUpkeep = true;
			}
			lizard.loungeDir = Custom.DirVec(lizard.mainBodyChunk.pos, room.MiddleOfTile(jump.bestJump.goalCell.worldCoordinate));
			jumpCounter = 0;
			for (int i = 0; i < jumpFinders.Count; i++)
			{
				jumpFinders[i].Destroy();
			}
			if (canChainJump && (actOnJump.chainJump || actOnJump.bestJump.power > 0.25f))
			{
				jumpFinders.Add(new JumpFinder(room, this, jump.bestJump.goalCell.worldCoordinate.Tile, chainJump: true));
			}
		}
	}

	public bool TryChainJump(Vector2 pos)
	{
		if (!canChainJump)
		{
			return false;
		}
		for (int i = 0; i < jumpFinders.Count; i++)
		{
			if (!jumpFinders[i].chainJump || !jumpFinders[i].BeneficialMovement)
			{
				continue;
			}
			for (int j = 0; j < lizard.bodyChunks.Length; j++)
			{
				if (Custom.DistLess(room.MiddleOfTile(jumpFinders[i].startPos), lizard.bodyChunks[j].pos, 40f))
				{
					for (int k = 0; k < lizard.bodyChunks.Length; k++)
					{
						lizard.bodyChunks[k].vel *= 0f;
					}
					lizard.inAllowedTerrainCounter = 1000;
					lizard.bodyChunks[1].pos = room.MiddleOfTile(jumpFinders[i].startPos);
					InitiateJump(jumpFinders[i], chainJump: true);
					return true;
				}
			}
		}
		return false;
	}

	public void Jump()
	{
		if (actOnJump == null || actOnJump.bestJump == default(JumpFinder.JumpInstruction))
		{
			return;
		}
		if (actOnJump.bestJump.power > 0.75f)
		{
			room.PlaySound(SoundID.Cyan_Lizard_Powerful_Jump, lizard.mainBodyChunk);
		}
		else if (actOnJump.bestJump.power > 0.25f)
		{
			room.PlaySound(SoundID.Cyan_Lizard_Medium_Jump, lizard.mainBodyChunk);
		}
		else
		{
			room.PlaySound(SoundID.Cyan_Lizard_Small_Jump, lizard.mainBodyChunk);
		}
		Vector2 vector = room.MiddleOfTile(actOnJump.startPos);
		for (int i = 0; i < lizard.bodyChunks.Length; i++)
		{
			lizard.bodyChunks[i].pos = Vector2.Lerp(lizard.bodyChunks[i].pos, vector + actOnJump.bestJump.initVel.normalized * (1 - i) * 8f, 1f);
		}
		Vector2 vector2 = vector + actOnJump.bestJump.initVel - lizard.bodyChunks[1].pos;
		for (int j = 0; j < lizard.bodyChunks.Length; j++)
		{
			lizard.bodyChunks[j].pos += vector2;
			lizard.bodyChunks[j].vel = actOnJump.bestJump.initVel;
		}
		lizard.movementAnimation = null;
		lizard.inAllowedTerrainCounter = 0;
		jumpCounter = 0;
		addDelay = 20;
		lizard.gripPoint = null;
		if (actOnJump.bestJump.tick > 25 && Vector2.Dot((lizard.bodyChunks[1].pos - lizard.bodyChunks[0].pos).normalized, actOnJump.bestJump.initVel.normalized) < Mathf.Lerp(-0.9f, -0.1f, Mathf.Pow((lizard.AI.excitement + lizard.AI.runSpeed) / 2f, 0.5f)))
		{
			spin = (int)Mathf.Sign(actOnJump.bestJump.initVel.x);
		}
		else
		{
			spin = 0;
		}
		if (lizard.graphicsModule != null)
		{
			if (smoke == null)
			{
				smoke = new CyanLizardSmoke(room);
				room.AddObject(smoke);
			}
			for (int k = 0; k < 7; k++)
			{
				smoke.EmitSmoke(vector, -actOnJump.bestJump.initVel * UnityEngine.Random.value * 0.5f + Custom.RNV() * Mathf.Lerp(6f, 23f, actOnJump.bestJump.power), lizard.graphicsModule as LizardGraphics, big: true, Mathf.Lerp(30f, 140f, actOnJump.bestJump.power));
			}
			for (int num = (int)(actOnJump.bestJump.power * 10f * UnityEngine.Random.value); num >= 0; num--)
			{
				room.AddObject(new LizardBubble(lizard.graphicsModule as LizardGraphics, 1f, 0f, actOnJump.bestJump.power * 10f));
			}
			room.AddObject(new JumpLight(vector - actOnJump.bestJump.initVel.normalized * 10f, lizard.graphicsModule as LizardGraphics, actOnJump.bestJump.power));
			room.AddObject(new ShockWave(vector - actOnJump.bestJump.initVel.normalized * 15f, Mathf.Lerp(40f, 120f, actOnJump.bestJump.power), 0.07f, 6 + (int)(actOnJump.bestJump.power * 4f)));
		}
	}

	public static bool PathWeightComparison(PathFinder.PathingCell A, PathFinder.PathingCell B)
	{
		if (A == null)
		{
			return B != null;
		}
		if (B == null)
		{
			return false;
		}
		if (B.costToGoal.legality != 0)
		{
			return false;
		}
		if (B.generation == A.generation)
		{
			return B.costToGoal.resistance < A.costToGoal.resistance;
		}
		return B.generation > A.generation;
	}
}
