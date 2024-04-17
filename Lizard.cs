using System;
using System.Collections.Generic;
using System.Globalization;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Lizard : AirBreatherCreature
{
	public class Animation : ExtEnum<Animation>
	{
		public static readonly Animation Standard = new Animation("Standard", register: true);

		public static readonly Animation HearSound = new Animation("HearSound", register: true);

		public static readonly Animation PreyReSpotted = new Animation("PreyReSpotted", register: true);

		public static readonly Animation PreySpotted = new Animation("PreySpotted", register: true);

		public static readonly Animation FightingStance = new Animation("FightingStance", register: true);

		public static readonly Animation ThreatSpotted = new Animation("ThreatSpotted", register: true);

		public static readonly Animation ThreatReSpotted = new Animation("ThreatReSpotted", register: true);

		public static readonly Animation ShootTongue = new Animation("ShootTongue", register: true);

		public static readonly Animation Spit = new Animation("Spit", register: true);

		public static readonly Animation PrepareToJump = new Animation("PrepareToJump", register: true);

		public static readonly Animation Jumping = new Animation("Jumping", register: true);

		public static readonly Animation PrepareToLounge = new Animation("PrepareToLounge", register: true);

		public static readonly Animation Lounge = new Animation("Lounge", register: true);

		public static readonly Animation ShakePrey = new Animation("ShakePrey", register: true);

		public Animation(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class MovementAnimation
	{
		private struct BodyPartDestination
		{
			public int bodyPart;

			public Vector2 destination;

			public float finishedDist;

			public float maxDistance;

			public BodyPartDestination(BodyChunk bodyChunk, Vector2 destination, float finishedDist)
			{
				bodyPart = bodyChunk.index;
				this.destination = destination;
				this.finishedDist = finishedDist;
				maxDistance = Vector2.Distance(bodyChunk.pos, destination) + 10f;
			}
		}

		private Lizard parent;

		public MovementConnection connection;

		private int step;

		private int totalSteps;

		private BodyPartDestination[] destinations;

		private bool finished;

		private float friction;

		private float speed;

		public override string ToString()
		{
			return "movAn. " + connection.type;
		}

		public MovementAnimation(Lizard parent, MovementConnection connection, int step, float friction, float speed)
		{
			this.parent = parent;
			this.connection = connection;
			this.step = step;
			this.friction = friction;
			this.speed = speed;
			finished = false;
			if (connection.type == MovementConnection.MovementType.Standard)
			{
				totalSteps = 1;
				destinations = new BodyPartDestination[2];
				destinations[0] = new BodyPartDestination(parent.bodyChunks[0], parent.room.MiddleOfTile(connection.DestTile), 5f);
				destinations[1] = new BodyPartDestination(parent.bodyChunks[2], parent.room.MiddleOfTile(connection.StartTile), 5f);
			}
			else if (connection.type == MovementConnection.MovementType.ReachOverGap || connection.type == MovementConnection.MovementType.ReachUp || connection.type == MovementConnection.MovementType.ReachDown)
			{
				totalSteps = 2;
				if (step == 1)
				{
					destinations = new BodyPartDestination[1];
					destinations[0] = new BodyPartDestination(parent.bodyChunks[0], (parent.room.MiddleOfTile(connection.StartTile) + parent.room.MiddleOfTile(connection.DestTile)) * 0.5f, 10f);
					return;
				}
				destinations = new BodyPartDestination[3];
				destinations[0] = new BodyPartDestination(parent.bodyChunks[0], parent.room.MiddleOfTile(connection.DestTile), 5f);
				destinations[1] = new BodyPartDestination(parent.bodyChunks[1], (parent.room.MiddleOfTile(connection.StartTile) + parent.room.MiddleOfTile(connection.DestTile)) * 0.5f, 100f);
				destinations[2] = new BodyPartDestination(parent.bodyChunks[2], parent.room.MiddleOfTile(connection.StartTile), 12f);
			}
			else if (connection.type == MovementConnection.MovementType.SemiDiagonalReach)
			{
				totalSteps = 1;
				destinations = new BodyPartDestination[2];
				destinations[0] = new BodyPartDestination(parent.bodyChunks[0], parent.room.MiddleOfTile(connection.DestTile), 5f);
				destinations[1] = new BodyPartDestination(parent.bodyChunks[2], parent.room.MiddleOfTile(connection.StartTile), 15f);
			}
			else if (connection.type == MovementConnection.MovementType.DoubleReachUp)
			{
				totalSteps = 1;
				destinations = new BodyPartDestination[2];
				destinations[0] = new BodyPartDestination(parent.bodyChunks[0], parent.room.MiddleOfTile(connection.DestTile), 5f);
				destinations[1] = new BodyPartDestination(parent.bodyChunks[2], Vector2.Lerp(parent.room.MiddleOfTile(connection.DestTile), parent.room.MiddleOfTile(connection.StartTile), 0.5f), 15f);
			}
			else if (connection.type == MovementConnection.MovementType.LizardTurn)
			{
				totalSteps = 2;
				destinations = new BodyPartDestination[3];
				if (step == 1)
				{
					destinations[0] = new BodyPartDestination(parent.bodyChunks[0], parent.room.MiddleOfTile(connection.StartTile), 8f);
					destinations[1] = new BodyPartDestination(parent.bodyChunks[1], parent.room.MiddleOfTile(connection.StartTile + new IntVector2(0, 1)), 9f);
					destinations[2] = new BodyPartDestination(parent.bodyChunks[2], parent.room.MiddleOfTile(connection.DestTile + new IntVector2(0, 1)), 10f);
				}
				else
				{
					destinations[0] = new BodyPartDestination(parent.bodyChunks[0], parent.room.MiddleOfTile(connection.DestTile), 5f);
					destinations[1] = new BodyPartDestination(parent.bodyChunks[1], parent.room.MiddleOfTile(connection.StartTile), 5f);
					destinations[2] = new BodyPartDestination(parent.bodyChunks[2], parent.room.MiddleOfTile(connection.StartTile + new IntVector2(0, 1)), 8f);
				}
			}
			else if (connection.type == MovementConnection.MovementType.DropToClimb)
			{
				totalSteps = 1;
				destinations = new BodyPartDestination[2];
				destinations[0] = new BodyPartDestination(parent.bodyChunks[0], parent.room.MiddleOfTile(connection.DestTile) + new Vector2(0f, 5f), 5f);
				destinations[1] = new BodyPartDestination(parent.bodyChunks[1], parent.room.MiddleOfTile(connection.StartTile), 12f);
			}
		}

		public void Update(float speedFac)
		{
			finished = true;
			bool flag = false;
			BodyPartDestination[] array = destinations;
			for (int i = 0; i < array.Length; i++)
			{
				BodyPartDestination bodyPartDestination = array[i];
				if (finished && !Custom.DistLess(parent.bodyChunks[bodyPartDestination.bodyPart].pos, bodyPartDestination.destination, bodyPartDestination.finishedDist))
				{
					finished = false;
				}
				parent.bodyChunks[bodyPartDestination.bodyPart].vel *= friction;
				float num = speed * ((bodyPartDestination.bodyPart == 0) ? parent.BodyForce : 1f) * Mathf.Pow(parent.AI.runSpeed, 0.2f);
				if (Custom.DistLess(parent.bodyChunks[bodyPartDestination.bodyPart].pos, bodyPartDestination.destination, num))
				{
					num = Vector2.Distance(parent.bodyChunks[bodyPartDestination.bodyPart].pos, bodyPartDestination.destination);
				}
				num *= parent.BodyDesperation * parent.lizardParams.maxMusclePower * 2f * UnityEngine.Random.value + 1f;
				parent.bodyChunks[bodyPartDestination.bodyPart].vel += Custom.DirVec(parent.bodyChunks[bodyPartDestination.bodyPart].pos, bodyPartDestination.destination) * num * speedFac;
				parent.bodyChunks[bodyPartDestination.bodyPart].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * parent.BodyDesperation * parent.lizardParams.maxMusclePower * 2f * UnityEngine.Random.value * speed;
				if (!Custom.DistLess(parent.bodyChunks[bodyPartDestination.bodyPart].pos, bodyPartDestination.destination, bodyPartDestination.maxDistance))
				{
					finished = true;
					flag = true;
					break;
				}
			}
			if (finished)
			{
				if (step < totalSteps && !flag)
				{
					parent.movementAnimation = new MovementAnimation(parent, connection, step + 1, friction, speed);
				}
				else
				{
					parent.MovementAnimationEnded(connection, !flag);
				}
			}
		}
	}

	public DebugSprite debugsprite1;

	public DebugSprite debugsprite2;

	public LizardBreedParams lizardParams;

	public LizardTongue tongue;

	private int biteDelay;

	public bool salamanderLurk;

	public float swim;

	public float spawnDataEvil;

	public int grabbedAttackCounter;

	private int turnedByRockDirection;

	private int turnedByRockCounter;

	private float jawOpen;

	public float lastJawOpen;

	public float jawForcedShut;

	public LizardAI AI;

	public LizardVoice voice;

	public Vector2 loungeDir;

	public int bubble;

	public float bubbleIntensity;

	public int loungeDelay;

	public int postLoungeStun;

	public int timeInAnimation;

	public int timeToRemainInAnimation;

	public Animation animation;

	public LizardJumpModule jumpModule;

	public int jumpHeldTime;

	public bool biteControlReset;

	public static float zeroGravityMovementThreshold = 0.3f;

	public List<MovementConnection> upcomingConnections;

	public MovementConnection followingConnection;

	public MovementConnection lastFollowingConnection;

	public MovementConnection commitedToDropConnection;

	public MovementAnimation movementAnimation;

	public bool applyGravity;

	public int inAllowedTerrainCounter;

	public Vector2? gripPoint;

	public int timeSpentTryingThisMove;

	public float desperationSmoother;

	private int snakeTicker;

	public Vector2 limbsAimFor;

	public float straightenOutNeeded;

	private bool narrowUpcoming;

	private bool climbUpcoming;

	private bool shortcutUpcoming;

	private float bodyWiggle;

	private int bwc;

	public Color effectColor { get; set; }

	public override Vector2 DangerPos
	{
		get
		{
			if (tongue != null && tongue.state == LizardTongue.State.LashingOut)
			{
				return tongue.pos;
			}
			return base.mainBodyChunk.pos;
		}
	}

	public override float VisibilityBonus
	{
		get
		{
			if (base.Template.type != CreatureTemplate.Type.WhiteLizard)
			{
				return 0f;
			}
			if (base.graphicsModule != null)
			{
				return 0f - (base.graphicsModule as LizardGraphics).Camouflaged;
			}
			return 0f;
		}
	}

	public float JawOpen
	{
		get
		{
			if (base.grasps[0] != null)
			{
				jawOpen = 0f;
			}
			if (lizardParams.tongue && tongue.Out)
			{
				jawOpen = Mathf.Max(jawOpen, 0.5f);
			}
			if (jawForcedShut > 0f)
			{
				jawOpen = Mathf.Min(Mathf.Pow(1f - jawForcedShut, 4f), jawOpen);
			}
			return jawOpen;
		}
		set
		{
			if (base.grasps[0] == null)
			{
				jawOpen = Mathf.Clamp(value, 0f, 1f);
			}
			else
			{
				jawOpen = 0f;
			}
		}
	}

	public bool JawReadyForBite
	{
		get
		{
			if (JawOpen > 0.9f)
			{
				return biteDelay == 0;
			}
			return false;
		}
	}

	public LizardState LizardState => base.abstractCreature.state as LizardState;

	public bool IsWallClimber
	{
		get
		{
			if (!ModManager.MMF || room == null || !(room.gravity <= zeroGravityMovementThreshold))
			{
				return lizardParams.WallClimber;
			}
			return true;
		}
	}

	public int BodyDirection => (Mathf.RoundToInt((0f - Custom.AimFromOneVectorToAnother(base.bodyChunks[1].pos, base.bodyChunks[0].pos) + 180f) / 90f) + 1) % 4;

	private float BodyForce => Mathf.Clamp(desperationSmoother * 0.025f, 1f, lizardParams.maxMusclePower);

	private float BodyDesperation => Mathf.InverseLerp(120f, 400f, desperationSmoother);

	public int LegsGripping
	{
		get
		{
			if (base.graphicsModule != null)
			{
				return (base.graphicsModule as LizardGraphics).legsGrabbing;
			}
			return 2;
		}
	}

	public int NoGripCounter
	{
		get
		{
			if (base.graphicsModule != null)
			{
				return (base.graphicsModule as LizardGraphics).noGripCounter;
			}
			return 0;
		}
	}

	private float BodyWiggleFac => Mathf.Clamp(((float)bodyWiggleCounter - (float)lizardParams.wiggleDelay) / (50f + (float)lizardParams.wiggleDelay), 0f, 1f);

	public int bodyWiggleCounter
	{
		get
		{
			return bwc;
		}
		set
		{
			bwc = Custom.IntClamp(value, 0, 100);
		}
	}

	public float SnakeCoil
	{
		get
		{
			if (!ModManager.MSC || base.Template.type != MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
			{
				return 1f;
			}
			float num = Mathf.Abs(Mathf.Sin((float)snakeTicker / 60f * (float)Math.PI));
			if ((double)num < 0.6)
			{
				return Custom.LerpElasticEaseIn(0f, 1f, num / 0.6f);
			}
			return Custom.LerpElasticEaseOut(0f, 1f, (num - 0.6f) / 0.4f);
		}
	}

	public void EnterAnimation(Animation anim, bool forceAnimationChange)
	{
		if ((!forceAnimationChange && (int)anim < (int)animation) || animation == anim)
		{
			return;
		}
		timeInAnimation = 0;
		animation = anim;
		if (animation == Animation.Standard)
		{
			timeToRemainInAnimation = -1;
		}
		else if (animation == Animation.HearSound)
		{
			timeToRemainInAnimation = 40;
			bubble = 40;
			bubbleIntensity = 0.1f;
			if (base.abstractCreature.creatureTemplate.type != CreatureTemplate.Type.BlackLizard)
			{
				voice.MakeSound(LizardVoice.Emotion.Curious);
			}
		}
		else if (animation == Animation.PreySpotted)
		{
			bubble = 30;
			bubbleIntensity = 1f;
			timeToRemainInAnimation = 30;
			voice.MakeSound(LizardVoice.Emotion.SpottedPreyFirstTime);
		}
		else if (animation == Animation.PreyReSpotted)
		{
			bubble = 10;
			bubbleIntensity = 0.4f;
			timeToRemainInAnimation = 10;
			if (base.safariControlled)
			{
				return;
			}
			if (base.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.YellowLizard)
			{
				if (AI.yellowAI.pack != null && AI.yellowAI.pack.PackLeader == base.abstractCreature)
				{
					voice.MakeSound(LizardVoice.Emotion.SpottedPreyFirstTime);
				}
				else
				{
					voice.MakeSound(LizardVoice.Emotion.ReSpottedPrey, UnityEngine.Random.Range(0.1f, 0.25f));
				}
			}
			else if (base.abstractCreature.creatureTemplate.type != CreatureTemplate.Type.BlackLizard)
			{
				voice.MakeSound(LizardVoice.Emotion.ReSpottedPrey);
			}
		}
		else if (animation == Animation.ThreatSpotted)
		{
			bubbleIntensity = 1f;
			timeToRemainInAnimation = UnityEngine.Random.Range(10, 50);
			if (!base.safariControlled)
			{
				voice.MakeSound(LizardVoice.Emotion.Fear);
			}
		}
		else if (animation == Animation.ThreatReSpotted)
		{
			bubbleIntensity = 1f;
			timeToRemainInAnimation = 10;
		}
		else if (animation == Animation.ShootTongue)
		{
			timeToRemainInAnimation = lizardParams.tongueWarmUp;
			room.PlaySound(SoundID.Lizard_Prepare_To_Shoot_Tongue, base.mainBodyChunk);
		}
		else if (animation == Animation.Spit)
		{
			timeToRemainInAnimation = 200;
		}
		else if (animation == Animation.PrepareToJump)
		{
			bubble = 3;
			bubbleIntensity = 0.1f;
		}
		else if (animation == Animation.Jumping)
		{
			timeToRemainInAnimation = -1;
			jumpModule.Jump();
			base.GoThroughFloors = true;
		}
		else if (animation == Animation.PrepareToLounge)
		{
			bubble = 3;
			bubbleIntensity = 0.1f;
			timeToRemainInAnimation = lizardParams.preLoungeCrouch + 5;
			if (AI.focusCreature != null && AI.focusCreature.representedCreature != null && AI.focusCreature.representedCreature.realizedCreature != null)
			{
				loungeDir = Custom.DirVec(base.mainBodyChunk.pos, AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
			}
			else if (base.safariControlled)
			{
				if (base.Template.type != MoreSlugcatsEnums.CreatureTemplateType.SpitLizard || !inputWithDiagonals.HasValue || (inputWithDiagonals.Value.x == 0 && inputWithDiagonals.Value.y == 0))
				{
					loungeDir = Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
				}
				else if (inputWithDiagonals.HasValue)
				{
					loungeDir = new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y).normalized;
				}
			}
			room.PlaySound(SoundID.Lizard_Prepare_Lunge_Attack_Init, base.mainBodyChunk);
		}
		else if (animation == Animation.Lounge)
		{
			loungeDelay = lizardParams.loungeDelay * ((!(UnityEngine.Random.value < lizardParams.riskOfDoubleLoungeDelay)) ? 1 : 2) + lizardParams.loungeMaximumFrames;
			if (AI.focusCreature != null && AI.focusCreature.representedCreature != null && AI.focusCreature.representedCreature.realizedCreature != null)
			{
				loungeDir = Vector3.Slerp(loungeDir, Custom.DirVec(base.mainBodyChunk.pos, AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos), lizardParams.findLoungeDirection);
			}
			if (Vector2.Dot(Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos), loungeDir) < 0f)
			{
				if ((lizardParams.canExitLounge || lizardParams.canExitLoungeWarmUp) && !base.safariControlled)
				{
					EnterAnimation(Animation.Standard, forceAnimationChange: true);
				}
				else
				{
					loungeDir = Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
				}
			}
			if (loungeDir.y > 0f)
			{
				loungeDir.y += lizardParams.loungeJumpyness;
			}
			loungeDir.y *= 0.5f;
			timeToRemainInAnimation = lizardParams.loungeMaximumFrames + 5;
			room.PlaySound(SoundID.Lizard_Lunge_Attack_Init, base.mainBodyChunk);
		}
		else if (animation == Animation.ShakePrey)
		{
			bubbleIntensity = 0f;
			timeToRemainInAnimation = UnityEngine.Random.Range(lizardParams.shakePrey, lizardParams.shakePrey * 2);
			timeToRemainInAnimation = (int)((float)timeToRemainInAnimation * AI.DynamicRelationship((base.grasps[0].grabbed as Creature).abstractCreature).intensity);
		}
		else if (animation == Animation.FightingStance)
		{
			bubble = 10;
			bubbleIntensity = 1f;
			timeToRemainInAnimation = UnityEngine.Random.Range(20, 50);
		}
	}

	public Lizard(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		lizardParams = abstractCreature.creatureTemplate.breedParameters as LizardBreedParams;
		base.bodyChunks = new BodyChunk[3];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(200f, 500f), 8f * lizardParams.bodySizeFac * lizardParams.bodyRadFac, lizardParams.bodyMass / 3f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(200f, 500f), 8f * lizardParams.bodySizeFac * lizardParams.bodyRadFac, lizardParams.bodyMass / 3f);
		base.bodyChunks[2] = new BodyChunk(this, 2, new Vector2(200f, 500f), 8f * lizardParams.bodySizeFac * lizardParams.bodyRadFac, lizardParams.bodyMass / 3f);
		bodyChunkConnections = new BodyChunkConnection[3];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 17f * lizardParams.bodyLengthFac * ((lizardParams.bodySizeFac + 1f) / 2f), BodyChunkConnection.Type.Normal, 0.95f, 0.5f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[2], 17f * lizardParams.bodyLengthFac * ((lizardParams.bodySizeFac + 1f) / 2f), BodyChunkConnection.Type.Normal, 0.95f, 0.5f);
		bodyChunkConnections[2] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[2], 17f * lizardParams.bodyLengthFac * ((lizardParams.bodySizeFac + 1f) / 2f) * (1f + lizardParams.bodyStiffnes), BodyChunkConnection.Type.Push, 1f - Mathf.Lerp(0.9f, 0.5f, lizardParams.bodyStiffnes), 0.5f);
		animation = Animation.Standard;
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.5f;
		collisionLayer = 1;
		base.waterFriction = 0.92f;
		base.buoyancy = ((base.Template.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)) ? 0.92f : 1.06f);
		base.GoThroughFloors = true;
		if (base.Template.type == CreatureTemplate.Type.CyanLizard && world.game.IsArenaSession)
		{
			spawnDataEvil = 0.5f;
		}
		if (abstractCreature.spawnData != null && abstractCreature.spawnData[0] == '{')
		{
			string[] array = abstractCreature.spawnData.Substring(1, abstractCreature.spawnData.Length - 2).Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Length > 0)
				{
					string[] array2 = array[i].Split(':');
					string text = array2[0];
					if (text != null && text == "Mean" && array2.Length > 1)
					{
						spawnDataEvil = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
				}
			}
		}
		if (lizardParams.tongue)
		{
			tongue = new LizardTongue(this);
		}
		voice = new LizardVoice(this);
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractCreature.ID.RandomSeed);
		effectColor = lizardParams.standardColor;
		if (base.Template.type == CreatureTemplate.Type.PinkLizard)
		{
			effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.87f, 0.1f, 0.6f), 1f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
		}
		else if (base.Template.type == CreatureTemplate.Type.GreenLizard)
		{
			effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.32f, 0.1f, 0.6f), 1f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
		}
		else if (base.Template.type == CreatureTemplate.Type.BlueLizard)
		{
			effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.57f, 0.08f, 0.6f), 1f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
		}
		else if (base.Template.type == CreatureTemplate.Type.YellowLizard)
		{
			effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.1f, 0.05f, 0.6f), 1f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
		}
		else if (base.Template.type == CreatureTemplate.Type.WhiteLizard)
		{
			effectColor = lizardParams.standardColor;
		}
		else if (base.Template.type == CreatureTemplate.Type.Salamander)
		{
			effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.9f, 0.15f, 0.6f), 1f, Custom.ClampedRandomVariation(0.4f, 0.15f, 0.2f));
		}
		else if (base.Template.type == CreatureTemplate.Type.RedLizard)
		{
			effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.0025f, 0.02f, 0.6f), 1f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
		}
		else if (base.Template.type == CreatureTemplate.Type.CyanLizard)
		{
			effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.49f, 0.04f, 0.6f), 1f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
			jumpModule = new LizardJumpModule(this);
		}
		else if (ModManager.MSC && base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
		{
			effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.1f, 0.03f, 0.2f), 0.55f, Custom.ClampedRandomVariation(0.55f, 0.36f, 0.2f));
		}
		if (base.abstractCreature.IsVoided())
		{
			effectColor = RainWorld.SaturatedGold;
		}
		UnityEngine.Random.state = state;
		StartUp();
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new LizardGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public override void Update(bool eu)
	{
		if (base.abstractCreature.InDen)
		{
			return;
		}
		if (base.Template.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard))
		{
			lungs = 1f;
			if (ModManager.MMF)
			{
				base.buoyancy = 0.92f;
			}
		}
		if (inputWithDiagonals.HasValue && inputWithDiagonals.Value.jmp)
		{
			jumpHeldTime++;
		}
		else
		{
			jumpHeldTime = 0;
		}
		lastJawOpen = JawOpen;
		voice.Update();
		if (JawOpen > 0.9f)
		{
			if (biteDelay > 0)
			{
				biteDelay--;
			}
		}
		else if (biteDelay < lizardParams.biteDelay)
		{
			biteDelay++;
		}
		jawForcedShut = Mathf.Max(0f, jawForcedShut - 1f / 60f);
		if (!base.dead && UnityEngine.Random.value * 0.7f > LizardState.health && UnityEngine.Random.value < 0.125f)
		{
			Stun(UnityEngine.Random.Range(1, UnityEngine.Random.Range(1, 27 - Custom.IntClamp((int)(20f * LizardState.health), 0, 10))));
		}
		if (!base.dead && UnityEngine.Random.value * 0.2f > LizardState.health && UnityEngine.Random.value < 0.025f && base.graphicsModule != null)
		{
			(base.graphicsModule as LizardGraphics).WhiteFlicker(UnityEngine.Random.Range(1, 5));
		}
		if (!base.dead && UnityEngine.Random.value * 0.5f > LizardState.health && UnityEngine.Random.value < 0.05f)
		{
			JawOpen = UnityEngine.Random.value;
		}
		bodyChunkConnections[2].active = BodyStiff();
		if (turnedByRockCounter > 0)
		{
			turnedByRockCounter--;
			WeightedPush(0, 2, new Vector2(turnedByRockDirection, 0f), 6f);
			if (Mathf.Abs(base.bodyChunks[0].pos.y - base.bodyChunks[2].pos.y) < 5f)
			{
				WeightedPush(0, 2, new Vector2(0f, 1f), 2f);
			}
		}
		if (bubble > 0)
		{
			bubble--;
		}
		loungeDelay--;
		if (tongue != null)
		{
			tongue.Update();
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (base.Consious)
		{
			Act();
		}
		else
		{
			applyGravity = true;
		}
		if (!base.Consious && !base.dead && base.stun < 35 && grabbedBy.Count > 0 && !(grabbedBy[0].grabber is Leech) && grabbedAttackCounter < 22 && !base.safariControlled)
		{
			grabbedAttackCounter++;
			jawForcedShut = Mathf.Max(0f, jawForcedShut - 1f / 24f);
			JawOpen = JawOpen + 0.1f + 0.1f * LizardState.health;
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].vel += Custom.RNV() * UnityEngine.Random.value * 6f * LizardState.health;
			}
			if (grabbedAttackCounter == 22 && JawReadyForBite && ((UnityEngine.Random.value < lizardParams.getFreeBiteChance * Custom.LerpMap(grabbedBy[0].grabber.TotalMass, base.TotalMass, base.TotalMass * 3f, 1f, 0.1f) * LizardState.health && grabbedBy[0].grabber.Template.type != CreatureTemplate.Type.RedLizard && (grabbedBy[0].grabber.Template.type != CreatureTemplate.Type.Vulture || UnityEngine.Random.value < 0.5f) && grabbedBy[0].grabber.Template.type != CreatureTemplate.Type.KingVulture) || (base.Template.type == CreatureTemplate.Type.RedLizard && UnityEngine.Random.value < LizardState.health)))
			{
				DamageAttackClosestChunk(grabbedBy[0].grabber);
			}
		}
		else if (grabbedBy.Count == 0)
		{
			grabbedAttackCounter = 0;
		}
		for (int j = 0; j < 2; j++)
		{
			bodyChunkConnections[j].type = ((movementAnimation != null && movementAnimation.connection.type == MovementConnection.MovementType.Standard) ? BodyChunkConnection.Type.Pull : BodyChunkConnection.Type.Normal);
		}
		if (tongue != null && (animation == Animation.ShootTongue || (base.Template.type != CreatureTemplate.Type.BlueLizard && tongue.Out)))
		{
			jawOpen += 0.1f;
		}
		if (ModManager.MSC && base.graphicsModule != null && !room.aimap.getAItile(base.abstractCreature.pos).narrowSpace && base.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard && animation == Animation.PrepareToLounge)
		{
			(base.graphicsModule as LizardGraphics).showDominance = 1f;
			Vector2 p = new Vector2(base.bodyChunks[2].pos.x + loungeDir.x * 10f, base.bodyChunks[2].pos.y + 15f);
			base.bodyChunks[1].vel += Custom.DirVec(base.bodyChunks[1].pos, p);
			p = new Vector2(base.bodyChunks[2].pos.x + loungeDir.x * -10f, base.bodyChunks[2].pos.y + 20f);
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, base.bodyChunks[1].pos + loungeDir * 10f);
			(base.graphicsModule as LizardGraphics).head.pos = Vector2.Lerp((base.graphicsModule as LizardGraphics).head.pos, base.bodyChunks[0].pos + loungeDir * 20f, 0.25f);
			(base.graphicsModule as LizardGraphics).head.vel = loungeDir * 20f;
			p = new Vector2(base.bodyChunks[2].pos.x + loungeDir.x * 10f, base.bodyChunks[2].pos.y - 15f);
			(base.graphicsModule as LizardGraphics).limbs[0].mode = Limb.Mode.HuntAbsolutePosition;
			(base.graphicsModule as LizardGraphics).limbs[0].absoluteHuntPos = p;
			(base.graphicsModule as LizardGraphics).limbs[1].mode = Limb.Mode.HuntAbsolutePosition;
			(base.graphicsModule as LizardGraphics).limbs[1].absoluteHuntPos = p;
			p = new Vector2(base.bodyChunks[2].pos.x + loungeDir.x * 20f, base.bodyChunks[2].pos.y - 15f);
			(base.graphicsModule as LizardGraphics).limbs[2].mode = Limb.Mode.HuntAbsolutePosition;
			(base.graphicsModule as LizardGraphics).limbs[2].absoluteHuntPos = p;
			(base.graphicsModule as LizardGraphics).limbs[3].mode = Limb.Mode.HuntAbsolutePosition;
			(base.graphicsModule as LizardGraphics).limbs[3].absoluteHuntPos = p;
		}
		if (animation == Animation.Lounge || postLoungeStun > 0 || (jumpModule != null && jumpModule.NoRunBehavior))
		{
			base.airFriction = 0.999f;
			base.gravity = 0.9f;
			bounce = 0.1f;
			surfaceFriction = ((animation == Animation.Lounge) ? 0.5f : 0.4f);
			base.GoThroughFloors = loungeDir.y < 0f;
			postLoungeStun--;
		}
		else if (applyGravity)
		{
			base.airFriction = 0.999f;
			base.gravity = 0.9f;
			bounce = 0.1f;
			surfaceFriction = 0.3f;
			base.GoThroughFloors = swim > 0f;
		}
		else
		{
			base.airFriction = 0.8f;
			base.gravity = 0f;
			bounce = 0.1f;
			surfaceFriction = 0.5f;
			base.GoThroughFloors = true;
		}
		if (base.grasps[0] != null)
		{
			CarryObject(eu);
		}
		if (base.Consious && AI.pathFinder.forbiddenEntranceCounter > 0)
		{
			for (int k = 0; k < base.bodyChunks.Length; k++)
			{
				if (room.GetTile(base.bodyChunks[k].pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					base.bodyChunks[k].vel += room.ShorcutEntranceHoleDirection(room.GetTilePosition(base.bodyChunks[k].pos)).ToVector2() * 4f;
				}
			}
		}
		if (lizardParams.pullDownFac != 1f && base.Submersion == 0f && !room.aimap.getAItile(base.abstractCreature.pos).narrowSpace && (room.GetTile(base.abstractCreature.pos.Tile + new IntVector2(0, -1)).Solid || room.GetTile(base.abstractCreature.pos.Tile + new IntVector2(0, -2)).Solid))
		{
			for (int l = 1; l < base.bodyChunks.Length; l++)
			{
				base.bodyChunks[l].vel.y -= lizardParams.pullDownFac - 1f;
			}
		}
		if (jumpModule != null)
		{
			jumpModule.Update();
		}
		base.Update(eu);
	}

	public void AttemptBite(Creature creature)
	{
		if (base.grasps[0] != null || !base.Consious)
		{
			return;
		}
		if (!JawReadyForBite)
		{
			if (base.safariControlled)
			{
				biteDelay = 0;
				JawOpen += 0.2f;
			}
			else
			{
				JawOpen += 0.05f;
			}
			return;
		}
		bool flag = false;
		if (UnityEngine.Random.value < lizardParams.biteChance && creature != null)
		{
			BodyChunk[] array = creature.bodyChunks;
			foreach (BodyChunk bodyChunk in array)
			{
				if (Custom.DistLess(base.mainBodyChunk.pos + Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos) * lizardParams.biteInFront, bodyChunk.pos, (ModManager.MMF ? Mathf.Max(8f, bodyChunk.rad) : bodyChunk.rad) + lizardParams.biteRadBonus))
				{
					if (tongue != null && tongue.Out)
					{
						if (tongue.state != LizardTongue.State.StuckInTerrain)
						{
							tongue.Retract();
						}
						return;
					}
					flag = true;
					Bite(bodyChunk);
					break;
				}
			}
		}
		else if (creature == null)
		{
			Bite(null);
		}
		if (LegsGripping <= 0)
		{
			return;
		}
		if (flag)
		{
			for (int j = 0; j < 3; j++)
			{
				base.bodyChunks[j].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * 7f;
			}
		}
		else if (creature != null && (tongue == null || !tongue.Out))
		{
			base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, creature.mainBodyChunk.pos) * 3f * lizardParams.biteHomingSpeed;
			base.bodyChunks[1].vel -= Custom.DirVec(base.mainBodyChunk.pos, creature.mainBodyChunk.pos) * lizardParams.biteHomingSpeed;
			base.bodyChunks[2].vel -= Custom.DirVec(base.mainBodyChunk.pos, creature.mainBodyChunk.pos) * lizardParams.biteHomingSpeed;
		}
	}

	public void GrabInanimate(BodyChunk chunk)
	{
		if (base.grasps[0] != null || !base.Consious)
		{
			return;
		}
		if (ModManager.MMF && MMF.cfgAlphaRedLizards.Value && (base.Template.type == CreatureTemplate.Type.RedLizard || (ModManager.MSC && base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)) && Vector2.Distance(chunk.pos, base.firstChunk.pos) < 20f && (chunk.owner is Spear || chunk.owner is ExplosiveSpear) && base.Consious)
		{
			Custom.Log("LIZARD BROKE SPEAR");
			if (chunk.owner is ExplosiveSpear)
			{
				(chunk.owner as ExplosiveSpear).Explode();
			}
			else
			{
				if (chunk.owner is ElectricSpear)
				{
					(chunk.owner as ElectricSpear).Zap();
					(chunk.owner as ElectricSpear).Electrocute(this);
				}
				Spear spear = chunk.owner as Spear;
				for (int i = 0; i < 2; i++)
				{
					room.AddObject(new ExplosiveSpear.SpearFragment(spear.firstChunk.pos, Custom.RNV() * Mathf.Lerp(5f, 10f, UnityEngine.Random.value)));
				}
				room.AddObject(new PuffBallSkin(spear.firstChunk.pos + spear.rotation * 10f, Custom.RNV() * Mathf.Lerp(10f, 30f, UnityEngine.Random.value), Color.red, Color.Lerp(Color.red, new Color(0f, 0f, 0f), 0.3f)));
				room.PlaySound(SoundID.Spear_Fragment_Bounce, spear.firstChunk.pos);
				spear.Destroy();
			}
		}
		if (tongue != null && tongue.Out)
		{
			tongue.Retract();
		}
		Bite(chunk);
	}

	private void Bite(BodyChunk chunk)
	{
		if (ModManager.MSC && base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard && room != null)
		{
			for (int i = 0; i < 16; i++)
			{
				Vector2 vector = Custom.RNV();
				room.AddObject(new Spark(base.firstChunk.pos + vector * 40f, vector * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.white, null, 8, 24));
			}
		}
		if ((base.grasps[0] != null && grabbedBy.Count == 0) || (chunk != null && chunk.owner is Creature && (chunk.owner as Creature).newToRoomInvinsibility > 0))
		{
			return;
		}
		biteControlReset = false;
		JawOpen = 0f;
		lastJawOpen = 0f;
		if (chunk == null)
		{
			room.PlaySound(SoundID.Lizard_Jaws_Shut_Miss_Creature, base.mainBodyChunk);
			return;
		}
		chunk.vel += base.mainBodyChunk.vel * Mathf.Lerp(base.mainBodyChunk.mass, 1.1f, 0.5f) / Mathf.Max(1f, chunk.mass);
		bool flag = false;
		if ((chunk.owner is Creature && AI.DynamicRelationship((chunk.owner as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats) || (UnityEngine.Random.value < 0.5f && chunk.owner.TotalMass < base.TotalMass * 1.2f) || (!(chunk.owner is Creature) && chunk.owner.TotalMass < base.TotalMass * 1.2f))
		{
			flag = Grab(chunk.owner, 0, chunk.index, Grasp.Shareability.CanOnlyShareWithNonExclusive, lizardParams.biteDominance * UnityEngine.Random.value, overrideEquallyDominant: true, pacifying: true);
		}
		if (flag)
		{
			if (chunk.owner is Creature)
			{
				if (base.Template.type == CreatureTemplate.Type.RedLizard)
				{
					(chunk.owner as Creature).LoseAllGrasps();
				}
				AI.BitCreature(chunk);
				if (ModManager.MMF)
				{
					if (chunk.owner is Player)
					{
						if (AI.friendTracker.friend != null && AI.friendTracker.friend == chunk.owner)
						{
							(chunk.owner as Player).Violence(base.mainBodyChunk, Custom.DirVec(base.mainBodyChunk.pos, chunk.pos) * 0.1f, chunk, null, DamageType.Bite, 0.1f, 0f);
						}
						else if (lizardParams.biteDamageChance > 0f && (lizardParams.biteDamageChance >= 1f || UnityEngine.Random.value < lizardParams.biteDamageChance * (chunk.owner as Player).DeathByBiteMultiplier()))
						{
							(chunk.owner as Player).Violence(base.mainBodyChunk, Custom.DirVec(base.mainBodyChunk.pos, chunk.pos) * 0.1f, chunk, null, DamageType.Bite, 1.5f, 0f);
						}
					}
					else if (UnityEngine.Random.value < lizardParams.biteDamageChance)
					{
						(chunk.owner as Creature).Violence(base.mainBodyChunk, Custom.DirVec(base.mainBodyChunk.pos, chunk.pos) * 0.1f, chunk, null, DamageType.Bite, lizardParams.biteDamage * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value), 0f);
					}
				}
				else if (UnityEngine.Random.value < lizardParams.biteDamageChance)
				{
					if (chunk.owner is Player)
					{
						(chunk.owner as Player).Violence(base.mainBodyChunk, Custom.DirVec(base.mainBodyChunk.pos, chunk.pos) * 0.1f, chunk, null, DamageType.Bite, 1.5f, 0f);
					}
					else
					{
						(chunk.owner as Creature).Violence(base.mainBodyChunk, Custom.DirVec(base.mainBodyChunk.pos, chunk.pos) * 0.1f, chunk, null, DamageType.Bite, lizardParams.biteDamage * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value), 0f);
					}
				}
			}
			if (base.graphicsModule != null)
			{
				if (chunk.owner is IDrawable)
				{
					base.graphicsModule.AddObjectToInternalContainer(chunk.owner as IDrawable, 0);
				}
				else if (chunk.owner.graphicsModule != null)
				{
					base.graphicsModule.AddObjectToInternalContainer(chunk.owner.graphicsModule, 0);
				}
			}
			room.PlaySound((chunk.owner is Player) ? SoundID.Lizard_Jaws_Grab_Player : SoundID.Lizard_Jaws_Grab_NPC, base.mainBodyChunk);
			return;
		}
		if (UnityEngine.Random.value < 0.5f && chunk.owner is Creature && AI.DynamicRelationship((chunk.owner as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Attacks)
		{
			DamageAttack(chunk, 0.5f);
			return;
		}
		room.PlaySound(SoundID.Lizard_Jaws_Shut_Miss_Creature, base.mainBodyChunk);
		for (int num = chunk.owner.grabbedBy.Count - 1; num >= 0; num--)
		{
			if (chunk.owner.grabbedBy[num].grabber is Lizard)
			{
				chunk.owner.grabbedBy[num].Release();
			}
		}
	}

	private void DamageAttackClosestChunk(Creature target)
	{
		BodyChunk bodyChunk = null;
		float dst = float.MaxValue;
		for (int i = 0; i < target.bodyChunks.Length; i++)
		{
			if (Custom.DistLess(base.mainBodyChunk.pos, target.bodyChunks[i].pos, dst))
			{
				bodyChunk = target.bodyChunks[i];
				dst = Vector2.Distance(base.mainBodyChunk.pos, target.bodyChunks[i].pos);
			}
		}
		if (bodyChunk != null)
		{
			DamageAttack(bodyChunk, 1f);
		}
	}

	private void DamageAttack(BodyChunk chunk, float dmgFac)
	{
		if (chunk == null || !(chunk.owner is Creature))
		{
			return;
		}
		if (AI.DynamicRelationship((chunk.owner as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.AgressiveRival)
		{
			dmgFac = 0f;
		}
		(chunk.owner as Creature).Violence(base.mainBodyChunk, Custom.DirVec(base.mainBodyChunk.pos, chunk.pos) * 11f, chunk, null, DamageType.Bite, lizardParams.biteDamage * UnityEngine.Random.value * dmgFac, 30f);
		room.PlaySound((chunk.owner is Player) ? SoundID.Lizard_Jaws_Grab_Player : SoundID.Lizard_Jaws_Grab_NPC, base.mainBodyChunk);
		Custom.Log("lizard damage attack!", biteDelay.ToString());
		JawOpen = 0f;
		lastJawOpen = 0f;
		base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, chunk.pos) * 8f;
		base.bodyChunks[1].vel -= Custom.DirVec(base.mainBodyChunk.pos, chunk.pos) * 6f;
		base.bodyChunks[2].vel -= Custom.DirVec(base.mainBodyChunk.pos, chunk.pos) * 6f;
		biteDelay = lizardParams.biteDelay;
		base.stun = Math.Max(base.stun, (int)(16f * dmgFac));
		if (room.BeingViewed)
		{
			for (int i = 0; i < (int)(3f + 6f * dmgFac); i++)
			{
				room.AddObject(new WaterDrip(Vector2.Lerp(base.mainBodyChunk.pos, chunk.pos, UnityEngine.Random.value), Custom.RNV() * UnityEngine.Random.value * (6f + lizardParams.biteDamage * 2.5f * dmgFac), waterColor: false));
			}
		}
		LoseAllGrasps();
	}

	public void JawsSnapShut(Vector2 pos)
	{
		room.PlaySound(SoundID.Lizard_Jaws_Shut_Miss_Creature, base.mainBodyChunk);
		JawOpen = 0f;
		lastJawOpen = 0f;
		base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, pos) * 8f;
		base.bodyChunks[1].vel -= Custom.DirVec(base.mainBodyChunk.pos, pos) * 6f;
		base.bodyChunks[2].vel -= Custom.DirVec(base.mainBodyChunk.pos, pos) * 6f;
		biteDelay = lizardParams.biteDelay;
		jawForcedShut = 1f;
	}

	public override void LoseAllGrasps()
	{
		if (base.graphicsModule != null)
		{
			base.graphicsModule.ReleaseAllInternallyContainedSprites();
		}
		base.LoseAllGrasps();
	}

	public override void ReleaseGrasp(int grasp)
	{
		if (base.graphicsModule != null)
		{
			base.graphicsModule.ReleaseAllInternallyContainedSprites();
		}
		base.ReleaseGrasp(grasp);
	}

	private void CarryObject(bool eu)
	{
		if (UnityEngine.Random.value < 0.025f && (!(base.grasps[0].grabbed is Creature) || AI.DynamicRelationship((base.grasps[0].grabbed as Creature).abstractCreature).type != CreatureTemplate.Relationship.Type.Eats))
		{
			LoseAllGrasps();
			return;
		}
		Vector2 vector = base.mainBodyChunk.pos + Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos) * 25f * lizardParams.headSize;
		PhysicalObject grabbed = base.grasps[0].grabbed;
		Vector2 vector2 = grabbed.bodyChunks[base.grasps[0].chunkGrabbed].vel - base.mainBodyChunk.vel;
		float mass = grabbed.bodyChunks[base.grasps[0].chunkGrabbed].mass;
		if (mass <= base.mainBodyChunk.mass / 2f)
		{
			mass /= 2f;
		}
		else if (mass <= base.mainBodyChunk.mass / 10f)
		{
			mass = 0f;
		}
		grabbed.bodyChunks[base.grasps[0].chunkGrabbed].vel = base.mainBodyChunk.vel;
		if (grabbed is Weapon && base.graphicsModule != null)
		{
			(grabbed as Weapon).setRotation = Custom.PerpendicularVector(base.mainBodyChunk.pos, (base.graphicsModule as LizardGraphics).head.pos);
		}
		if (!enteringShortCut.HasValue && (vector2.magnitude * grabbed.bodyChunks[base.grasps[0].chunkGrabbed].mass > 30f || !Custom.DistLess(vector, grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos, 70f + grabbed.bodyChunks[base.grasps[0].chunkGrabbed].rad)))
		{
			LoseAllGrasps();
		}
		else
		{
			grabbed.bodyChunks[base.grasps[0].chunkGrabbed].MoveFromOutsideMyUpdate(eu, vector);
		}
		if (base.grasps[0] != null)
		{
			for (int i = 0; i < 2; i++)
			{
				base.grasps[0].grabbed.PushOutOf(base.bodyChunks[i].pos, base.bodyChunks[i].rad, base.grasps[0].chunkGrabbed);
			}
		}
	}

	private bool BodyStiff()
	{
		if (!base.Consious)
		{
			return true;
		}
		if (movementAnimation != null)
		{
			return false;
		}
		return true;
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		if (otherObject is Creature)
		{
			AI.tracker.SeeCreature((otherObject as Creature).abstractCreature);
			if (AI.DynamicRelationship((otherObject as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.AgressiveRival && AI.submittedTo != (otherObject as Creature).abstractCreature)
			{
				AI.agressionTracker.IncrementAnger(AI.tracker.RepresentationForObject(otherObject, AddIfMissing: false), 0.05f * AI.DynamicRelationship((otherObject as Creature).abstractCreature).intensity);
			}
			if (otherObject is Player && AI.LikeOfPlayer(AI.tracker.RepresentationForCreature((otherObject as Player).abstractCreature, addIfMissing: false)) < 0.5f && LizardState.socialMemory.GetTempLike((otherObject as Player).abstractCreature.ID) > -0.5f)
			{
				LizardState.socialMemory.GetOrInitiateRelationship((otherObject as Player).abstractCreature.ID).InfluenceTempLike(-0.05f);
			}
		}
		if (base.Consious && base.grasps[0] == null && otherObject is Creature && AI.DynamicRelationship((otherObject as Creature).abstractCreature).GoForKill)
		{
			if (animation == Animation.Lounge && myChunk == 0)
			{
				Bite(otherObject.bodyChunks[otherChunk]);
			}
			else if (!base.safariControlled)
			{
				AttemptBite(otherObject as Creature);
			}
		}
		base.Collide(otherObject, myChunk, otherChunk);
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		if (firstContact && base.graphicsModule != null && speed > 10f)
		{
			(base.graphicsModule as LizardGraphics).TerrainImpact(chunk, direction, speed);
		}
		if (speed > 1.5f && firstContact)
		{
			float num = Mathf.InverseLerp(6f, 14f, speed);
			if (num < 1f)
			{
				room.PlaySound(SoundID.Lizard_Light_Terrain_Impact, base.mainBodyChunk, loop: false, 1f - num, Mathf.Lerp(1f / lizardParams.bodySizeFac, 1f, 0.8f));
			}
			if (num > 0f)
			{
				room.PlaySound(SoundID.Lizard_Heavy_Terrain_Impact, base.mainBodyChunk, loop: false, num, Mathf.Lerp(1f / lizardParams.bodySizeFac, 1f, 0.8f));
			}
		}
		base.TerrainImpact(chunk, direction, speed, firstContact);
	}

	public override Color ShortCutColor()
	{
		return effectColor;
	}

	public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos onAppendagePos, Vector2 direction)
	{
		if (chunk == null)
		{
			return false;
		}
		float num = Custom.Angle(direction, -chunk.Rotation) * ((chunk.index == 2) ? (-1f) : 1f);
		if (chunk.index == 0 && HitHeadShield(direction))
		{
			return false;
		}
		bool flag = chunk.index == 0 && HitInMouth(direction);
		if (source != null && source is Spear && base.graphicsModule != null && UnityEngine.Random.value < 1f / (flag ? 2f : 7f))
		{
			BodyPart bodyPart = null;
			if (chunk.index == 0 && (UnityEngine.Random.value < 1f / 3f || flag))
			{
				bodyPart = (base.graphicsModule as LizardGraphics).head;
			}
			else if (chunk.index != 1 || UnityEngine.Random.value < 0.5f)
			{
				int num2 = 0;
				num2 = ((chunk.index != 2) ? ((num < 0f) ? 1 : 0) : ((num < 0f) ? 2 : 3));
				bodyPart = (base.graphicsModule as LizardGraphics).limbs[num2];
				LizardState.limbHealth[num2] = 0f;
			}
			if (bodyPart != null)
			{
				(source as Spear).ProvideRotationBodyPart(chunk, bodyPart);
			}
		}
		if (flag)
		{
			Stun(100);
		}
		if (source != null && base.Template.type == CreatureTemplate.Type.CyanLizard && !base.dead && !flag && jumpModule.gasLeakPower > 0f && jumpModule.gasLeakSpear == null && source is Spear && chunk.index < 2 && (animation == Animation.Jumping || animation == Animation.PrepareToJump || UnityEngine.Random.value < ((chunk.index == 1) ? 0.5f : 0.25f)))
		{
			jumpModule.gasLeakSpear = source as Spear;
		}
		return true;
	}

	private bool HitHeadShield(Vector2 direction)
	{
		float num = Vector2.Angle(direction, -base.bodyChunks[0].Rotation);
		if (HitInMouth(direction))
		{
			return false;
		}
		if (num < lizardParams.headShieldAngle + 20f * JawOpen)
		{
			if (room != null)
			{
				room.PlaySound(SoundID.Lizard_Head_Shield_Deflect, base.mainBodyChunk);
			}
			return true;
		}
		return false;
	}

	private bool HitInMouth(Vector2 direction)
	{
		if (direction.y > 0f)
		{
			return false;
		}
		direction = Vector3.Slerp(direction, new Vector2(0f, 1f), 0.1f);
		return Mathf.Abs(Vector2.Angle(direction, -base.bodyChunks[0].Rotation)) < Mathf.Lerp(-15f, 11f, JawOpen);
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		float num = damage / base.Template.baseDamageResistance;
		float num2 = (damage * 30f + stunBonus) / base.Template.baseStunResistance;
		if (type.Index != -1)
		{
			if (base.Template.damageRestistances[type.Index, 0] > 0f)
			{
				num /= base.Template.damageRestistances[type.Index, 0];
			}
			if (base.Template.damageRestistances[type.Index, 1] > 0f)
			{
				num2 /= base.Template.damageRestistances[type.Index, 1];
			}
		}
		voice.MakeSound(LizardVoice.Emotion.PainImpact, Mathf.Max(num * 2f, num2 / 40f));
		if (hitChunk != null && hitChunk.index == 0 && directionAndMomentum.HasValue)
		{
			if (HitHeadShield(directionAndMomentum.Value))
			{
				if (type == DamageType.Bite || type == DamageType.Stab)
				{
					type = DamageType.Blunt;
				}
				if (base.graphicsModule != null && source != null)
				{
					int num3 = (int)Math.Min(Math.Max(num2 / 2f, (int)(num * 30f)), 25f);
					for (int i = 0; i < num3; i++)
					{
						room.AddObject(new Spark(source.pos + Custom.DegToVec(UnityEngine.Random.value * 360f) * 5f * UnityEngine.Random.value, source.vel * -0.1f + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.2f, 0.4f, UnityEngine.Random.value) * source.vel.magnitude, new Color(1f, 1f, 1f), base.graphicsModule as LizardGraphics, 10, 170));
					}
					(base.graphicsModule as LizardGraphics).WhiteFlicker(num3);
					room.AddObject(new StationaryEffect(source.pos, new Color(1f, 1f, 1f), base.graphicsModule as LizardGraphics, StationaryEffect.EffectType.FlashingOrb));
					if (directionAndMomentum.HasValue)
					{
						(base.graphicsModule as LizardGraphics).head.vel += directionAndMomentum.Value * 2f;
					}
				}
				num *= 0.1f;
				num2 = (damage * 0.5f * 30f + stunBonus * (2f / 3f)) / base.Template.baseStunResistance;
				if (base.Template.type == CreatureTemplate.Type.CyanLizard)
				{
					num2 *= 1.2f;
				}
				directionAndMomentum = directionAndMomentum.Value / 3f;
			}
			else if (HitInMouth(directionAndMomentum.Value))
			{
				num *= 1.5f;
				num2 = Mathf.Min(num2 * 2f, 120f);
				LizardState.throatHealth -= num;
			}
		}
		if (directionAndMomentum.HasValue)
		{
			hitChunk.vel += directionAndMomentum.Value / hitChunk.mass;
		}
		if (source != null && source.owner is Rock && base.Template.type != CreatureTemplate.Type.RedLizard)
		{
			turnedByRockDirection = (int)Mathf.Sign(source.pos.x - source.lastPos.x);
			turnedByRockCounter = 20;
		}
		num2 *= 1f + Mathf.InverseLerp(0.75f, 0f, LizardState.health) * UnityEngine.Random.value;
		Stun((int)num2);
		LizardState.health -= num;
	}

	public override void Stun(int st)
	{
		if (LizardState.health < 0.5f)
		{
			st = (int)((float)st * (1f + Mathf.InverseLerp(0.75f, 0f, LizardState.health) * 1.5f));
		}
		if (st > 5 && !base.Stunned && base.graphicsModule != null)
		{
			(base.graphicsModule as LizardGraphics).Stun(st);
		}
		base.Stun(st);
		if (base.Stunned)
		{
			gripPoint = null;
			movementAnimation = null;
			LoseAllGrasps();
		}
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
		if (UnityEngine.Random.value < 0.5f && !base.safariControlled)
		{
			voice.MakeSound(LizardVoice.Emotion.OutOfShortcut);
		}
		if (base.graphicsModule == null)
		{
			return;
		}
		base.graphicsModule.Reset();
		if (base.grasps[0] != null)
		{
			if (base.grasps[0].grabbed is IDrawable)
			{
				base.graphicsModule.AddObjectToInternalContainer(base.grasps[0].grabbed as IDrawable, 0);
			}
			else if (base.grasps[0].grabbed.graphicsModule != null)
			{
				base.graphicsModule.AddObjectToInternalContainer(base.grasps[0].grabbed.graphicsModule, 0);
			}
		}
	}

	public void MovementAnimationEnded(MovementConnection connection, bool success)
	{
		movementAnimation = null;
	}

	private void StartUp()
	{
		upcomingConnections = new List<MovementConnection>();
	}

	private void Act()
	{
		AI.Update();
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			if (room.aimap.TileAccessibleToCreature(base.bodyChunks[i].pos, base.abstractCreature.creatureTemplate))
			{
				flag = flag || base.bodyChunks[i].index == 0;
				flag2 = flag2 || base.bodyChunks[i].index > 0;
				num++;
			}
		}
		if ((flag || flag2 || movementAnimation != null) && inAllowedTerrainCounter < 100)
		{
			inAllowedTerrainCounter++;
		}
		else
		{
			bool flag3 = false;
			int num2 = 0;
			while (!flag3 && num2 < 3)
			{
				int num3 = 0;
				while (!flag3 && num3 < 4)
				{
					if (room.aimap.TileAccessibleToCreature(room.GetTilePosition(base.bodyChunks[num2].pos) + Custom.fourDirections[num3], base.abstractCreature.creatureTemplate) && !room.GetTile(room.GetTilePosition(base.bodyChunks[num2].pos) + Custom.fourDirections[num3]).AnyWater)
					{
						flag3 = true;
					}
					num3++;
				}
				num2++;
			}
			if (flag3)
			{
				inAllowedTerrainCounter = Math.Max(0, inAllowedTerrainCounter - 10);
			}
			else
			{
				inAllowedTerrainCounter = 0;
			}
		}
		applyGravity = inAllowedTerrainCounter < lizardParams.regainFootingCounter || NoGripCounter > 10 || commitedToDropConnection != default(MovementConnection);
		swim = Mathf.Clamp(swim - 0.05f, 0f, 1f);
		AI.stuckTracker.satisfiedWithThisPosition = swim > 0.5f;
		float num4 = ActAnimation();
		if (ModManager.MSC && base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard && base.Submersion == 0f)
		{
			bool flag4 = true;
			if (room.aimap.getAItile(base.mainBodyChunk.pos).narrowSpace || room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				flag4 = false;
			}
			if (base.Submersion > 0f)
			{
				flag4 = false;
			}
			if (flag4)
			{
				snakeTicker++;
				num4 *= Mathf.Lerp(0.25f, 1f, SnakeCoil);
			}
		}
		WorldCoordinate worldCoordinate = room.GetWorldCoordinate(base.mainBodyChunk.pos);
		if (movementAnimation == null && worldCoordinate.TileDefined && !AI.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
		{
			for (int j = 0; j < 8; j++)
			{
				if (AI.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate + Custom.eightDirectionsDiagonalsLast[j]))
				{
					worldCoordinate += Custom.eightDirectionsDiagonalsLast[j];
					break;
				}
			}
		}
		followingConnection = (AI.pathFinder as LizardPather).FollowPath(worldCoordinate, BodyDirection, actuallyFollowingThisPath: true);
		if (commitedToDropConnection != default(MovementConnection))
		{
			followingConnection = commitedToDropConnection;
			commitedToDropConnection = default(MovementConnection);
		}
		if (base.safariControlled && (followingConnection == default(MovementConnection) || !AllowableControlledAIOverride(followingConnection.type)))
		{
			if (inputWithoutDiagonals.HasValue)
			{
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (followingConnection != default(MovementConnection) && followingConnection.type != MovementConnection.MovementType.LizardTurn && followingConnection.type != MovementConnection.MovementType.DropToFloor && followingConnection.type != MovementConnection.MovementType.DropToClimb && followingConnection.type != MovementConnection.MovementType.DropToWater)
				{
					type = followingConnection.type;
				}
				MovementConnection movementConnection = (AI.pathFinder as LizardPather).ConnectionAtCoordinate(outGoing: true, room.GetWorldCoordinate(base.mainBodyChunk.pos), 0);
				if (movementConnection != default(MovementConnection) && movementConnection.type != MovementConnection.MovementType.BigCreatureShortCutSqueeze && movementConnection.type != MovementConnection.MovementType.ShortCut && movementConnection.type != MovementConnection.MovementType.NPCTransportation && movementConnection.type != MovementConnection.MovementType.OffScreenMovement && movementConnection.type != MovementConnection.MovementType.OutsideRoom && movementConnection.type != MovementConnection.MovementType.BetweenRooms)
				{
					type = movementConnection.type;
				}
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				followingConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithoutDiagonals.Value.x, inputWithoutDiagonals.Value.y) * 40f), 2);
				if (inputWithoutDiagonals.Value.thrw && !lastInputWithoutDiagonals.Value.thrw && base.abstractCreature.creatureTemplate.type != CreatureTemplate.Type.CyanLizard)
				{
					LoseAllGrasps();
					if (tongue != null && tongue.StuckToSomething)
					{
						tongue.Retract();
					}
					if (inputWithoutDiagonals.Value.y == 0 && inputWithoutDiagonals.Value.x == 0)
					{
						voice.MakeSound(LizardVoice.Emotion.GeneralSmallNoise);
						bubble = 5;
						bubbleIntensity = UnityEngine.Random.value * 0.5f;
					}
					else if (Mathf.Abs(inputWithoutDiagonals.Value.y) > Mathf.Abs(inputWithoutDiagonals.Value.x))
					{
						voice.MakeSound(LizardVoice.Emotion.Dominance);
						bubble = 30;
						bubbleIntensity = UnityEngine.Random.value * 0.5f + 0.5f;
					}
					else
					{
						voice.MakeSound(LizardVoice.Emotion.Frustration);
						bubble = 20;
						bubbleIntensity = UnityEngine.Random.value * 0.5f;
					}
				}
			}
			else
			{
				followingConnection = new MovementConnection(MovementConnection.MovementType.Standard, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos), 0);
			}
		}
		if (base.safariControlled)
		{
			bodyChunkConnections[0].type = BodyChunkConnection.Type.Normal;
			bodyChunkConnections[1].type = BodyChunkConnection.Type.Pull;
		}
		else if ((followingConnection == default(MovementConnection) || Vector2.Distance(base.mainBodyChunk.pos, room.MiddleOfTile(followingConnection.DestTile)) > Vector2.Distance(base.bodyChunks[1].pos, room.MiddleOfTile(followingConnection.DestTile))) && timeSpentTryingThisMove > 10)
		{
			bodyChunkConnections[0].type = BodyChunkConnection.Type.Pull;
			bodyChunkConnections[1].type = BodyChunkConnection.Type.Pull;
		}
		else
		{
			bodyChunkConnections[0].type = BodyChunkConnection.Type.Normal;
			bodyChunkConnections[1].type = BodyChunkConnection.Type.Pull;
		}
		if (base.safariControlled)
		{
			if (room.aimap.getAItile(followingConnection.DestTile).narrowSpace || room.aimap.getAItile(base.abstractCreature.pos).narrowSpace)
			{
				for (int k = 0; k < base.bodyChunks.Length; k++)
				{
					base.bodyChunks[k].terrainSqueeze = Mathf.InverseLerp(10f, 100f, base.bodyChunks[k].rad);
					base.bodyChunks[k].collideWithSlopes = false;
				}
			}
			else
			{
				for (int l = 0; l < base.bodyChunks.Length; l++)
				{
					base.bodyChunks[l].terrainSqueeze = Mathf.Lerp(base.bodyChunks[l].terrainSqueeze, 0f, 0.1f);
					base.bodyChunks[l].collideWithSlopes = true;
				}
			}
		}
		else
		{
			for (int m = 0; m < base.bodyChunks.Length; m++)
			{
				base.bodyChunks[m].terrainSqueeze = Custom.LerpMap(timeSpentTryingThisMove, 10f, 30f, 1f, 0.05f);
				base.bodyChunks[m].collideWithSlopes = timeSpentTryingThisMove < 30;
			}
		}
		if (num4 > 0.1f && (followingConnection == default(MovementConnection) || AI.stuckTracker.moveBacklog.IsMoveInLog(followingConnection, (int)Mathf.Lerp(1f, 20f, num4))))
		{
			timeSpentTryingThisMove = Custom.IntClamp(timeSpentTryingThisMove + 1, 0, 600);
		}
		else
		{
			timeSpentTryingThisMove = 0;
		}
		desperationSmoother = Custom.LerpAndTick(desperationSmoother, Mathf.Max((float)timeSpentTryingThisMove + Mathf.Lerp(-300f, 0f, num4), AI.stuckTracker.Utility() * 100f), 0.05f, 0.5f);
		if (desperationSmoother > Mathf.Lerp(40f, 10f, num4))
		{
			bodyWiggleCounter += 2;
		}
		else
		{
			bodyWiggleCounter--;
		}
		AI.stuckTracker.moveBacklog.ReportNewMove(followingConnection);
		upcomingConnections.Clear();
		if (base.safariControlled && !followingConnection.destinationCoord.NodeDefined && !room.aimap.TileAccessibleToCreature(followingConnection.DestTile.x, followingConnection.DestTile.y, base.abstractCreature.creatureTemplate) && !room.aimap.TileAccessibleToCreature(room.GetTile(base.bodyChunks[1].pos).X, room.GetTile(base.bodyChunks[0].pos).Y, base.abstractCreature.creatureTemplate) && !room.aimap.TileAccessibleToCreature(room.GetTile(base.bodyChunks[1].pos).X, room.GetTile(base.bodyChunks[1].pos).Y, base.abstractCreature.creatureTemplate) && !room.aimap.TileAccessibleToCreature(room.GetTile(base.bodyChunks[2].pos).X, room.GetTile(base.bodyChunks[2].pos).Y, base.abstractCreature.creatureTemplate))
		{
			followingConnection = default(MovementConnection);
		}
		if (followingConnection != default(MovementConnection))
		{
			limbsAimFor = room.MiddleOfTile(followingConnection.destinationCoord);
			if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
			{
				enteringShortCut = followingConnection.StartTile;
				if (base.abstractCreature.controlled)
				{
					bool flag5 = false;
					List<IntVector2> list = new List<IntVector2>();
					ShortcutData[] shortcuts = room.shortcuts;
					for (int n = 0; n < shortcuts.Length; n++)
					{
						ShortcutData shortcutData = shortcuts[n];
						if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != followingConnection.StartTile)
						{
							list.Add(shortcutData.StartTile);
						}
						if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == followingConnection.StartTile)
						{
							flag5 = true;
						}
					}
					if (flag5)
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
			else if (followingConnection.type == MovementConnection.MovementType.ReachUp || followingConnection.type == MovementConnection.MovementType.ReachOverGap || followingConnection.type == MovementConnection.MovementType.ReachDown || followingConnection.type == MovementConnection.MovementType.LizardTurn || followingConnection.type == MovementConnection.MovementType.SemiDiagonalReach || followingConnection.type == MovementConnection.MovementType.DoubleReachUp)
			{
				movementAnimation = new MovementAnimation(this, followingConnection, 1, 0.99f, 0.75f);
			}
			MovementConnection movementConnection2 = followingConnection;
			narrowUpcoming = false;
			climbUpcoming = false;
			shortcutUpcoming = false;
			if (base.safariControlled)
			{
				upcomingConnections.Clear();
			}
			else
			{
				for (int num5 = 0; num5 < 5; num5++)
				{
					if (!narrowUpcoming && ((room.GetTile(movementConnection2.destinationCoord + new IntVector2(0, 1)).Solid && room.GetTile(movementConnection2.destinationCoord + new IntVector2(0, -1)).Solid) || (room.GetTile(movementConnection2.destinationCoord + new IntVector2(1, 0)).Solid && room.GetTile(movementConnection2.destinationCoord + new IntVector2(-1, 0)).Solid)))
					{
						narrowUpcoming = true;
					}
					if (!climbUpcoming && room.aimap.getAItile(movementConnection2.destinationCoord).acc == AItile.Accessibility.Climb && (!ModManager.MMF || base.Submersion < 0.8f))
					{
						climbUpcoming = true;
					}
					upcomingConnections.Add(movementConnection2);
					if (!shortcutUpcoming && movementConnection2.type == MovementConnection.MovementType.ShortCut)
					{
						shortcutUpcoming = true;
					}
					movementConnection2 = (AI.pathFinder as LizardPather).FollowPath(movementConnection2.destinationCoord, null, actuallyFollowingThisPath: false);
					for (int num6 = 0; num6 < upcomingConnections.Count; num6++)
					{
						if (!(movementConnection2 != default(MovementConnection)))
						{
							break;
						}
						if (upcomingConnections[num6].destinationCoord == movementConnection2.destinationCoord)
						{
							movementConnection2 = default(MovementConnection);
						}
					}
					if (movementConnection2 == default(MovementConnection))
					{
						break;
					}
				}
			}
		}
		timeInAnimation++;
		if (timeToRemainInAnimation > -1 && timeInAnimation > timeToRemainInAnimation)
		{
			EnterAnimation(Animation.Standard, forceAnimationChange: true);
		}
		if (bubble == 0 && UnityEngine.Random.value < 0.05f && UnityEngine.Random.value < AI.excitement && UnityEngine.Random.value < AI.excitement)
		{
			bubbleIntensity = AI.excitement * AI.excitement * UnityEngine.Random.value;
			bubble = 1;
		}
		if (jumpModule != null && !gripPoint.HasValue && jumpModule.NoRunBehavior)
		{
			return;
		}
		if (base.safariControlled)
		{
			movementAnimation = null;
		}
		if (gripPoint.HasValue)
		{
			GripPointBehavior();
		}
		else if (movementAnimation != null)
		{
			movementAnimation.Update(num4 * lizardParams.baseSpeed * 0.5f);
		}
		else if (followingConnection != default(MovementConnection) && !room.aimap.getAItile(base.abstractCreature.pos).narrowSpace && !room.GetTile(followingConnection.DestTile).AnyWater && base.Submersion > 0.5f)
		{
			SwimBehavior();
		}
		else if ((followingConnection != default(MovementConnection) && followingConnection.destinationCoord.TileDefined && room.GetTile(followingConnection.DestTile).AnyWater) || (followingConnection == default(MovementConnection) && base.Submersion > 0.5f && base.Submersion > 0.2f))
		{
			SwimBehavior();
		}
		else if (followingConnection != default(MovementConnection))
		{
			FollowConnection(num4);
		}
		if (followingConnection == default(MovementConnection) && movementAnimation == null)
		{
			if (!applyGravity && !room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.abstractCreature.creatureTemplate))
			{
				IntVector2 intVector = room.GetTilePosition(base.mainBodyChunk.pos) + Custom.fourDirections[UnityEngine.Random.Range(0, 4)];
				if (room.aimap.TileAccessibleToCreature(intVector, base.abstractCreature.creatureTemplate))
				{
					followingConnection = new MovementConnection(MovementConnection.MovementType.Standard, room.ToWorldCoordinate(base.mainBodyChunk.pos), room.ToWorldCoordinate(intVector), 1);
				}
			}
			else if (flag && !flag2 && timeSpentTryingThisMove > 20)
			{
				for (int num7 = 1; num7 < 2; num7++)
				{
					for (int num8 = 0; num8 < 4; num8++)
					{
						if (room.aimap.TileAccessibleToCreature(room.GetTilePosition(base.bodyChunks[num7].pos) + Custom.fourDirections[num8], base.abstractCreature.creatureTemplate))
						{
							base.bodyChunks[num7].vel += Custom.DirVec(base.bodyChunks[num7].pos, room.MiddleOfTile(room.GetTilePosition(base.bodyChunks[num7].pos) + Custom.fourDirections[num8]));
							break;
						}
					}
				}
			}
		}
		if (BodyDesperation > 0f && swim == 0f)
		{
			Vector2 vector = Custom.DegToVec(UnityEngine.Random.value * 360f) * BodyDesperation * lizardParams.maxMusclePower * 2f * UnityEngine.Random.value;
			base.bodyChunks[0].vel += vector * 0.5f;
			base.bodyChunks[1].vel -= vector;
			base.bodyChunks[2].vel += vector * 0.5f;
		}
		bodyChunkConnections[2].elasticity = 0.7f + 0.3f / BodyForce;
		if (animation == Animation.ShakePrey)
		{
			bodyWiggleCounter = Custom.IntClamp(50 - timeInAnimation, 0, 100);
			Vector2 vector2 = Custom.DegToVec(UnityEngine.Random.value * 360f) * 12f * UnityEngine.Random.value;
			if (base.grasps[0] == null)
			{
				vector2 *= 0.5f;
			}
			else
			{
				float value = Mathf.Abs(base.grasps[0].grabbed.bodyChunks[base.grasps[0].chunkGrabbed].mass - base.mainBodyChunk.mass * 0.5f);
				value = Mathf.InverseLerp(base.mainBodyChunk.mass * 0.5f, 0f, value);
				value = Mathf.Pow(value, 1.5f);
				vector2 *= value;
				bodyWiggleCounter = Custom.IntClamp((int)((float)bodyWiggleCounter * Mathf.Lerp(value, 1f, 0.5f)), 0, 100);
				if (value == 0f && UnityEngine.Random.value < 0.025f)
				{
					bodyWiggleCounter = 0;
					EnterAnimation(Animation.Standard, forceAnimationChange: true);
				}
			}
			base.bodyChunks[0].vel -= vector2 * 0.5f;
			base.bodyChunks[1].vel += vector2;
			base.bodyChunks[2].vel -= vector2 * 0.5f;
		}
		if (BodyWiggleFac > 0f && swim < 0.5f)
		{
			bodyWiggle += Mathf.Lerp(0.05f, 0.15f, BodyWiggleFac) * Mathf.Lerp(0.5f, 0.8f, lizardParams.wiggleSpeed);
			float num9 = Mathf.Sin(bodyWiggle * 2f * (float)Math.PI);
			Vector2 v = Vector3.Slerp(Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos), Custom.DirVec(base.bodyChunks[2].pos, base.bodyChunks[1].pos), 0.5f);
			v = Custom.PerpendicularVector(v);
			float num10 = (BodyWiggleFac + 2f) / (Mathf.Pow(1f - lizardParams.wiggleSpeed, 2f) + 2f);
			base.bodyChunks[0].vel += v * num9 * num10;
			base.bodyChunks[1].vel -= v * num9 * num10 * 2f;
			base.bodyChunks[2].vel += v * num9 * num10;
		}
		if (followingConnection != default(MovementConnection))
		{
			lastFollowingConnection = followingConnection;
		}
		straightenOutNeeded = Custom.LerpAndTick(straightenOutNeeded, 0f, 0.1f, 0.05f);
	}

	private void FollowConnection(float runSpeed)
	{
		float frameSpeed = GetFrameSpeed(runSpeed);
		if (timeSpentTryingThisMove < 20 && narrowUpcoming && frameSpeed > 0.2f && upcomingConnections.Count > 1 && room.aimap.getAItile(upcomingConnections[1].DestTile).narrowSpace)
		{
			IntVector2 intVector = IntVector2.ClampAtOne(followingConnection.DestTile - followingConnection.StartTile);
			IntVector2 intVector2 = IntVector2.ClampAtOne(upcomingConnections[1].DestTile - upcomingConnections[1].StartTile);
			if (intVector.x != intVector2.x && intVector.y != intVector2.y)
			{
				base.bodyChunks[0].vel *= 0.75f;
				base.bodyChunks[0].vel += intVector2.ToVector2() * 2f;
				runSpeed = Mathf.Min(runSpeed, 0.2f);
				base.bodyChunks[1].vel *= 0.75f;
				base.bodyChunks[2].vel *= 0.5f;
			}
		}
		if (followingConnection.type == MovementConnection.MovementType.Standard || followingConnection.type == MovementConnection.MovementType.Slope || followingConnection.type == MovementConnection.MovementType.CeilingSlope || followingConnection.type == MovementConnection.MovementType.OpenDiagonal)
		{
			Vector2 vector = new Vector2(0f, 0f);
			if (room.aimap.getAItile(followingConnection.DestTile).acc == AItile.Accessibility.Climb)
			{
				IntVector2 intVector3 = ((upcomingConnections.Count <= 0) ? IntVector2.ClampAtOne(followingConnection.DestTile - followingConnection.StartTile) : IntVector2.ClampAtOne(upcomingConnections[Math.Min(2, upcomingConnections.Count - 1)].DestTile - followingConnection.StartTile));
				if (followingConnection.destinationCoord.y == followingConnection.startCoord.y)
				{
					vector.y = (float)intVector3.y * 8f;
				}
				else if (followingConnection.destinationCoord.x == followingConnection.startCoord.x)
				{
					if (room.aimap.getAItile(followingConnection.StartTile).acc != AItile.Accessibility.Climb)
					{
						intVector3.x = ((!(base.bodyChunks[2].pos.x < room.MiddleOfTile(base.mainBodyChunk.pos).x)) ? 1 : (-1));
					}
					vector.x = (float)intVector3.x * 8f;
				}
				if (IsTileSolid(0, intVector3.x, 0))
				{
					intVector3.x *= -1;
					vector.x = 0f;
				}
				if (IsTileSolid(0, 0, intVector3.y))
				{
					intVector3.y *= -1;
					vector.y = 0f;
				}
			}
			else if (NoGripCounter == 0 && followingConnection.destinationCoord.y == followingConnection.startCoord.y && !IsTileSolid(0, 0, 1) && !IsTileSolid(0, ((followingConnection.destinationCoord.x < base.abstractCreature.pos.x) ? (-1) : 0) + ((followingConnection.destinationCoord.x > base.abstractCreature.pos.x) ? 1 : 0), 1))
			{
				vector.y = lizardParams.floorLeverage;
			}
			if (followingConnection.startCoord.x != followingConnection.destinationCoord.x && followingConnection.startCoord.y == followingConnection.destinationCoord.y)
			{
				if ((IsTileSolid(0, 0, -1) && base.mainBodyChunk.pos.y < room.MiddleOfTile(base.mainBodyChunk.pos).y) || (IsTileSolid(0, 0, 1) && base.mainBodyChunk.pos.y > room.MiddleOfTile(base.mainBodyChunk.pos).y))
				{
					base.mainBodyChunk.vel.y -= (base.mainBodyChunk.pos.y - room.MiddleOfTile(base.mainBodyChunk.pos).y) * 0.5f;
				}
			}
			else if (followingConnection.startCoord.y != followingConnection.destinationCoord.y && followingConnection.startCoord.x == followingConnection.destinationCoord.x && ((IsTileSolid(0, -1, 0) && base.mainBodyChunk.pos.x < room.MiddleOfTile(base.mainBodyChunk.pos).x) || (IsTileSolid(0, 1, 0) && base.mainBodyChunk.pos.x > room.MiddleOfTile(base.mainBodyChunk.pos).x)))
			{
				base.mainBodyChunk.vel.x -= (base.mainBodyChunk.pos.x - room.MiddleOfTile(base.mainBodyChunk.pos).x) * 0.5f;
			}
			IntVector2 intVector4 = followingConnection.DestTile - followingConnection.StartTile;
			if (intVector4.y != 0)
			{
				for (int i = 0; i < 3; i++)
				{
					if (base.bodyChunks[i].ContactPoint.y != 0 && !IsTileSolid(i, 0, base.bodyChunks[i].ContactPoint.y))
					{
						base.bodyChunks[i].vel.x -= (base.bodyChunks[i].pos.x - room.MiddleOfTile(base.bodyChunks[i].pos).x) * 0.25f;
						vector.x *= 0f;
					}
				}
			}
			else if (intVector4.x != 0)
			{
				for (int j = 0; j < 3; j++)
				{
					if (base.bodyChunks[j].ContactPoint.x != 0 && !IsTileSolid(j, base.bodyChunks[j].ContactPoint.x, 0))
					{
						base.bodyChunks[j].vel.y -= (base.bodyChunks[j].pos.y - room.MiddleOfTile(base.bodyChunks[j].pos).y) * 0.25f;
						vector.y *= 0f;
					}
				}
			}
			Vector2 vector2 = room.MiddleOfTile(followingConnection.DestTile);
			if (upcomingConnections.Count > 0 && room.aimap.getAItile(upcomingConnections[upcomingConnections.Count - 1].destinationCoord).acc < AItile.Accessibility.Climb)
			{
				for (int num = upcomingConnections.Count - 1; num >= 0; num--)
				{
					if ((room.GetTilePosition(base.mainBodyChunk.pos).FloatDist(upcomingConnections[num].DestTile) < 2f && room.VisualContact(base.mainBodyChunk.pos, room.MiddleOfTile(upcomingConnections[num].DestTile))) || AI.pathFinder.RayTraceInAccessibleTiles(room.GetTilePosition(base.mainBodyChunk.pos), upcomingConnections[num].DestTile))
					{
						vector2 = room.MiddleOfTile(upcomingConnections[num].DestTile);
						break;
					}
				}
			}
			vector2 += vector;
			Vector2 p = vector2 + Custom.DirVec(vector2, base.bodyChunks[0].pos) * bodyChunkConnections[0].distance;
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, vector2) * frameSpeed * BodyForce;
			base.bodyChunks[1].vel += Custom.DirVec(base.bodyChunks[1].pos, p) * frameSpeed * BodyForce * Custom.LerpMap(Vector2.Dot(Custom.DirVec(base.bodyChunks[1].pos, vector2), Custom.DirVec(base.bodyChunks[1].pos, p)), -1f, 1f, 0.5f, 1f);
			if (animation == Animation.Standard && AI.runSpeed > 0.1f)
			{
				float num2 = Mathf.InverseLerp(0f, -1f, Vector2.Dot(Custom.DirVec(base.bodyChunks[0].pos, vector2), Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos)));
				if (num2 > 0.5f)
				{
					straightenOutNeeded = Custom.LerpAndTick(straightenOutNeeded, 1f, 0.1f, 0.1f);
				}
				num2 = Mathf.Lerp(num2, 1f, Mathf.InverseLerp(20f, 40f, timeSpentTryingThisMove));
				num2 *= Mathf.Max((narrowUpcoming || shortcutUpcoming) ? 1f : 0.2f, climbUpcoming ? 0.5f : 0.2f, Mathf.InverseLerp(5f, 20f, timeSpentTryingThisMove), straightenOutNeeded);
				base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[1].pos, vector2) * (num2 * 2f * BodyForce + BodyDesperation * 2f) * frameSpeed;
				base.bodyChunks[1].vel -= Custom.DirVec(base.bodyChunks[1].pos, vector2) * (num2 * 2f * BodyForce + BodyDesperation * 2f) * 0.5f * Custom.LerpMap(timeSpentTryingThisMove, 20f, 60f, 1f, 0.5f) * frameSpeed;
				base.bodyChunks[2].vel -= Custom.DirVec(base.bodyChunks[1].pos, vector2) * (num2 * 2f * BodyForce + BodyDesperation * 2f) * 0.5f * Custom.LerpMap(timeSpentTryingThisMove, 20f, 60f, 1f, 0.5f) * frameSpeed;
			}
		}
		else
		{
			if (followingConnection.type != MovementConnection.MovementType.DropToFloor && followingConnection.type != MovementConnection.MovementType.DropToClimb)
			{
				return;
			}
			bool flag = true;
			if (Math.Abs(base.abstractCreature.pos.x - followingConnection.startCoord.x) > 4 && Math.Abs(base.abstractCreature.pos.x - followingConnection.destinationCoord.x) > 4)
			{
				flag = false;
			}
			for (int k = 0; k < 3; k++)
			{
				if (base.bodyChunks[k].ContactPoint.y < 0)
				{
					flag = false;
				}
			}
			if (flag)
			{
				commitedToDropConnection = followingConnection;
			}
			base.bodyChunks[0].vel.y -= 0.01f;
			base.bodyChunks[2].vel.y += 0.01f;
			if (Mathf.Abs(base.mainBodyChunk.pos.x - room.MiddleOfTile(followingConnection.DestTile).x) < 5f)
			{
				if (Mathf.Abs(base.mainBodyChunk.vel.x) < 5f)
				{
					base.mainBodyChunk.vel.x *= 0.8f;
				}
			}
			else if (base.mainBodyChunk.pos.x < room.MiddleOfTile(followingConnection.DestTile).x)
			{
				base.mainBodyChunk.vel.x += 0.5f;
			}
			else
			{
				base.mainBodyChunk.vel.x -= 0.5f;
			}
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			BodyChunk[] array = base.bodyChunks;
			foreach (BodyChunk bodyChunk in array)
			{
				if (bodyChunk.ContactPoint.y < 0)
				{
					flag2 = true;
				}
				if (room.GetTilePosition(bodyChunk.pos).y >= followingConnection.StartTile.y)
				{
					flag4 = true;
					if (room.GetTilePosition(bodyChunk.pos).x == followingConnection.StartTile.x)
					{
						bodyChunk.vel += Custom.DirVec(bodyChunk.pos, room.MiddleOfTile(followingConnection.DestTile)) * frameSpeed * ((bodyChunk.index == 0) ? BodyForce : 1f);
						flag3 = true;
					}
					else
					{
						bodyChunk.vel += Custom.DirVec(bodyChunk.pos, room.MiddleOfTile(followingConnection.StartTile)) * frameSpeed * ((bodyChunk.index == 0) ? BodyForce : 1f);
					}
				}
				if (room.GetTilePosition(bodyChunk.pos).x == followingConnection.StartTile.x)
				{
					bodyChunk.vel.y -= 1f;
				}
			}
			if (flag3)
			{
				return;
			}
			if (upcomingConnections.Count > 0)
			{
				base.bodyChunks[0].vel.x -= 0.3f * (float)(followingConnection.destinationCoord.x - upcomingConnections[upcomingConnections.Count - 1].destinationCoord.x);
				base.bodyChunks[2].vel.x += 0.3f * (float)(followingConnection.destinationCoord.x - upcomingConnections[upcomingConnections.Count - 1].destinationCoord.x);
			}
			if (base.mainBodyChunk.pos.y <= base.mainBodyChunk.lastPos.y)
			{
				if (followingConnection.type == MovementConnection.MovementType.DropToClimb)
				{
					int num3 = room.GetTilePosition(base.mainBodyChunk.lastPos).y + 1;
					int num4 = room.GetTilePosition(base.mainBodyChunk.pos).y - 1;
					for (int num5 = num3; num5 >= num4; num5--)
					{
						if (num5 <= followingConnection.destinationCoord.y && room.aimap.getAItile(room.GetTilePosition(base.mainBodyChunk.pos).x, num5).acc == AItile.Accessibility.Climb)
						{
							room.PlaySound(SoundID.Lizard_Grab_Pole, base.mainBodyChunk);
							gripPoint = room.MiddleOfTile(new IntVector2(room.GetTilePosition(base.mainBodyChunk.pos).x, num5));
							base.mainBodyChunk.vel.y = 0f;
							break;
						}
					}
				}
				else if (followingConnection.destinationCoord.x != followingConnection.startCoord.x && Math.Abs(room.GetTilePosition(base.mainBodyChunk.pos).x - followingConnection.destinationCoord.x) < 2)
				{
					int num6 = room.GetTilePosition(base.mainBodyChunk.lastPos).y + 1;
					int num7 = room.GetTilePosition(base.mainBodyChunk.pos).y - 1;
					for (int num8 = num6; num8 >= num7; num8--)
					{
						if (num8 <= followingConnection.destinationCoord.y && room.aimap.getAItile(followingConnection.destinationCoord.x, num8).acc == AItile.Accessibility.Floor)
						{
							gripPoint = room.MiddleOfTile(new IntVector2(followingConnection.destinationCoord.x, num8));
							base.mainBodyChunk.vel.y = 0f;
							break;
						}
					}
				}
			}
			if (flag2 && !flag4 && !gripPoint.HasValue)
			{
				followingConnection = default(MovementConnection);
			}
		}
	}

	private float ActAnimation()
	{
		float num = 0f;
		if (animation == Animation.Standard)
		{
			num = AI.runSpeed;
		}
		else if (animation == Animation.HearSound)
		{
			num = 0f;
			JawOpen = 0.2f;
		}
		else if (animation == Animation.PreySpotted)
		{
			num = 0f;
			JawOpen = 1f;
		}
		else if (animation == Animation.PreyReSpotted)
		{
			num = AI.runSpeed * 0.3f;
			JawOpen = Mathf.Lerp(JawOpen, 1f, UnityEngine.Random.value * UnityEngine.Random.value);
		}
		else if (animation == Animation.ThreatSpotted)
		{
			num = 0.5f;
			JawOpen = UnityEngine.Random.value;
			if (AI.focusCreature != null && base.Template.CreatureRelationship(AI.focusCreature.representedCreature.creatureTemplate).type == CreatureTemplate.Relationship.Type.Afraid && AI.focusCreature.VisualContact && !base.safariControlled)
			{
				base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, AI.focusCreature.representedCreature.realizedCreature.DangerPos) * 5f * LegsGripping;
				base.bodyChunks[1].vel -= Custom.DirVec(base.mainBodyChunk.pos, AI.focusCreature.representedCreature.realizedCreature.DangerPos) * 6f * LegsGripping;
			}
		}
		else if (animation == Animation.FightingStance)
		{
			JawOpen = 1f;
			num = 0f;
			if (AI.focusCreature != null && !AI.UnpleasantFallRisk(room.GetTilePosition(base.mainBodyChunk.pos)) && AI.focusCreature.VisualContact && !base.safariControlled)
			{
				base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, AI.focusCreature.representedCreature.realizedCreature.DangerPos) * 4f * LegsGripping;
				base.bodyChunks[1].vel -= Custom.DirVec(base.mainBodyChunk.pos, AI.focusCreature.representedCreature.realizedCreature.DangerPos) * 2.2f * LegsGripping;
				base.bodyChunks[2].vel -= Custom.DirVec(base.mainBodyChunk.pos, AI.focusCreature.representedCreature.realizedCreature.DangerPos) * (IsWallClimber ? 2.2f : 2.6f) * LegsGripping;
			}
		}
		else if (animation == Animation.ThreatReSpotted)
		{
			num = AI.runSpeed * 0.8f;
			JawOpen = Mathf.Lerp(JawOpen, UnityEngine.Random.value, UnityEngine.Random.value);
		}
		else if (animation == Animation.ShootTongue)
		{
			bodyWiggleCounter = 0;
			num = 0.2f;
			if (AI.behavior != LizardAI.Behavior.Hunt || !AI.focusCreature.VisualContact || Vector2.Dot(Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos), Custom.DirVec(base.bodyChunks[0].pos, AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos)) < 0.3f)
			{
				if (base.safariControlled)
				{
					JawOpen = 1f;
					if (!inputWithDiagonals.HasValue || (inputWithDiagonals.Value.x == 0 && inputWithDiagonals.Value.y == 0))
					{
						tongue.LashOut(base.mainBodyChunk.pos + Custom.DegToVec((base.graphicsModule as LizardGraphics).HeadRotation(0f)) * lizardParams.tongueAttackRange);
					}
					else
					{
						tongue.LashOut(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * lizardParams.tongueAttackRange);
					}
				}
				EnterAnimation(Animation.Standard, forceAnimationChange: true);
			}
			if (timeInAnimation == timeToRemainInAnimation / 2 && AI.focusCreature != null && AI.focusCreature.representedCreature.realizedCreature != null)
			{
				JawOpen = 1f;
				tongue.LashOut(AI.focusCreature.representedCreature.realizedCreature.bodyChunks[UnityEngine.Random.Range(0, AI.focusCreature.representedCreature.realizedCreature.bodyChunks.Length)].pos);
			}
		}
		else if (animation == Animation.Spit)
		{
			num = 0f;
			bodyWiggleCounter = 0;
			JawOpen = Mathf.Clamp(JawOpen + 0.2f, 0f, 1f);
			if (!AI.redSpitAI.spitting && !base.safariControlled)
			{
				EnterAnimation(Animation.Standard, forceAnimationChange: true);
			}
			else
			{
				Vector2? vector = AI.redSpitAI.AimPos();
				if (vector.HasValue)
				{
					if (AI.redSpitAI.AtSpitPos)
					{
						Vector2 vector2 = room.MiddleOfTile(AI.redSpitAI.spitFromPos);
						base.mainBodyChunk.vel += Vector2.ClampMagnitude(vector2 - Custom.DirVec(vector2, vector.Value) * bodyChunkConnections[0].distance - base.mainBodyChunk.pos, 10f) / 5f;
						base.bodyChunks[1].vel += Vector2.ClampMagnitude(vector2 - base.bodyChunks[1].pos, 10f) / 5f;
					}
					if (!AI.UnpleasantFallRisk(room.GetTilePosition(base.mainBodyChunk.pos)))
					{
						base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, vector.Value) * 4f * LegsGripping;
						base.bodyChunks[1].vel -= Custom.DirVec(base.mainBodyChunk.pos, vector.Value) * 2f * LegsGripping;
						base.bodyChunks[2].vel -= Custom.DirVec(base.mainBodyChunk.pos, vector.Value) * 2f * LegsGripping;
					}
					if (AI.redSpitAI.delay < 1)
					{
						Vector2 vector3 = base.bodyChunks[0].pos + Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * 10f;
						Vector2 vector4 = Custom.DirVec(vector3, vector.Value);
						if (Vector2.Dot(vector4, Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos)) > 0.3f || base.safariControlled)
						{
							if (base.safariControlled)
							{
								EnterAnimation(Animation.Standard, forceAnimationChange: true);
								LoseAllGrasps();
							}
							room.PlaySound(SoundID.Red_Lizard_Spit, vector3);
							room.AddObject(new LizardSpit(vector3, vector4 * 40f, this));
							AI.redSpitAI.delay = 12;
							base.bodyChunks[2].pos -= vector4 * 8f;
							base.bodyChunks[1].pos -= vector4 * 4f;
							base.bodyChunks[2].vel -= vector4 * 2f;
							base.bodyChunks[1].vel -= vector4 * 1f;
							JawOpen = 1f;
						}
					}
				}
			}
		}
		else if (animation == Animation.PrepareToJump)
		{
			if (jumpModule.actOnJump != null && base.Consious && Custom.DistLess(base.bodyChunks[1].pos, room.MiddleOfTile(jumpModule.actOnJump.bestJump.startPos), 50f))
			{
				base.bodyChunks[1].vel *= 0.5f;
				base.bodyChunks[1].pos = Vector2.Lerp(base.bodyChunks[1].pos, room.MiddleOfTile(jumpModule.actOnJump.bestJump.startPos) - jumpModule.actOnJump.bestJump.initVel.normalized * timeInAnimation * 0.5f, 0.2f);
				num = 0f;
				base.bodyChunks[0].vel += jumpModule.actOnJump.bestJump.initVel.normalized * 1.5f;
				base.bodyChunks[2].vel -= jumpModule.actOnJump.bestJump.initVel.normalized * 2f;
				if (base.graphicsModule != null)
				{
					(base.graphicsModule as LizardGraphics).head.vel += Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(jumpModule.actOnJump.bestJump.goalCell.worldCoordinate)) * 10f;
					Vector2 vector5 = Custom.PerpendicularVector(base.bodyChunks[0].pos, base.bodyChunks[2].pos);
					for (int i = 0; i < (base.graphicsModule as LizardGraphics).tail.Length; i++)
					{
						(base.graphicsModule as LizardGraphics).tail[i].vel -= jumpModule.actOnJump.bestJump.initVel.normalized * i + vector5 * ((timeInAnimation % 6 < 3) ? (-5f) : 5f);
					}
				}
				if (timeInAnimation > timeToRemainInAnimation - 5)
				{
					EnterAnimation(Animation.Jumping, forceAnimationChange: false);
				}
				inAllowedTerrainCounter = lizardParams.regainFootingCounter + 10;
			}
			else
			{
				EnterAnimation(Animation.Standard, forceAnimationChange: true);
			}
		}
		else if (!(animation == Animation.Jumping))
		{
			if (animation == Animation.PrepareToLounge)
			{
				bodyWiggleCounter = 0;
				if (!lizardParams.canExitLoungeWarmUp || (AI.behavior == LizardAI.Behavior.Hunt && AI.focusCreature != null && AI.focusCreature.VisualContact) || base.safariControlled)
				{
					num = 0f;
					if ((AI.focusCreature != null && AI.focusCreature.representedCreature.realizedCreature != null) || base.safariControlled)
					{
						if (LegsGripping > 0)
						{
							BodyChunk[] array = base.bodyChunks;
							foreach (BodyChunk bodyChunk in array)
							{
								Vector2 vector6 = Vector2.zero;
								if (AI.focusCreature != null && AI.focusCreature.representedCreature != null && AI.focusCreature.representedCreature.realizedCreature != null)
								{
									vector6 = Custom.DirVec(bodyChunk.pos, AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
								}
								else if (base.safariControlled)
								{
									if (base.Template.type != MoreSlugcatsEnums.CreatureTemplateType.SpitLizard || !inputWithDiagonals.HasValue || (inputWithDiagonals.Value.x == 0 && inputWithDiagonals.Value.y == 0))
									{
										vector6 = Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
									}
									else if (inputWithDiagonals.HasValue)
									{
										vector6 = new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y).normalized;
									}
								}
								bodyChunk.vel += vector6 * lizardParams.preLoungeCrouchMovement;
							}
						}
						if (timeInAnimation >= lizardParams.preLoungeCrouch)
						{
							EnterAnimation(Animation.Lounge, forceAnimationChange: false);
						}
					}
					else
					{
						EnterAnimation(Animation.Standard, forceAnimationChange: true);
					}
				}
				else
				{
					EnterAnimation(Animation.Standard, forceAnimationChange: true);
				}
			}
			else if (animation == Animation.Lounge)
			{
				if (timeInAnimation < lizardParams.loungeMaximumFrames && (!lizardParams.canExitLounge || (AI.behavior == LizardAI.Behavior.Hunt && AI.focusCreature != null && AI.focusCreature.VisualContact)))
				{
					num = 0f;
					JawOpen += 0.1f;
					if (timeInAnimation < lizardParams.loungePropulsionFrames && LegsGripping > 0)
					{
						for (int k = 0; k < 3; k++)
						{
							base.bodyChunks[k].vel += loungeDir * lizardParams.loungeSpeed / (k + 1);
						}
					}
				}
				else
				{
					JawsSnapShut(base.mainBodyChunk.pos + Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos) * 10f);
					EnterAnimation(Animation.Standard, forceAnimationChange: true);
					postLoungeStun = 40;
					inAllowedTerrainCounter = 0;
				}
			}
			else if (animation == Animation.ShakePrey)
			{
				num = 0.75f;
				if (base.grasps[0] == null && UnityEngine.Random.value < 0.025f)
				{
					EnterAnimation(Animation.Standard, forceAnimationChange: true);
				}
			}
		}
		if (base.safariControlled && (animation == Animation.Standard || animation == Animation.HearSound || animation == Animation.PreyReSpotted || animation == Animation.PreySpotted || animation == Animation.FightingStance || animation == Animation.ThreatSpotted || animation == Animation.ThreatReSpotted))
		{
			num = (inputWithoutDiagonals.HasValue ? Mathf.Min(1f, new Vector2(inputWithoutDiagonals.Value.x, inputWithoutDiagonals.Value.y).magnitude) : 0f);
		}
		if (tongue != null && tongue.StuckToSomething)
		{
			if (base.Template.type == CreatureTemplate.Type.WhiteLizard)
			{
				bodyWiggleCounter += 2;
				num = 0f;
				if (!applyGravity)
				{
					float num2 = 1f - (float)LegsGripping / 4f;
					for (int l = 0; l < 3; l++)
					{
						base.bodyChunks[l].vel *= num2;
					}
					if (LegsGripping > 0)
					{
						base.bodyChunks[2].vel += Custom.DirVec(tongue.pos, base.bodyChunks[2].pos) * 1.2f;
					}
				}
			}
			else
			{
				num *= 0.6f;
			}
		}
		if (postLoungeStun > 0)
		{
			num *= 0.1f;
		}
		if (base.Template.type == CreatureTemplate.Type.RedLizard && AI.runSpeed > 0.1f && animation != Animation.Spit && !base.safariControlled)
		{
			num = Mathf.Lerp(num, 1f, Mathf.Lerp(0.2f, 0.7f, AI.hunger));
		}
		return num;
	}

	private float GetFrameSpeed(float runSpeed)
	{
		float num = (float)LegsGripping / 4f;
		num = num * (1f - lizardParams.noGripSpeed) + lizardParams.noGripSpeed;
		if (NoGripCounter < 1)
		{
			num = Mathf.Lerp(num, 1f + BodyDesperation, BodyForce - 1f);
		}
		if (num == 0f && BodyWiggleFac > 0f)
		{
			for (int i = 0; i < 3; i++)
			{
				if (base.bodyChunks[i].ContactPoint.x != 0 || base.bodyChunks[i].ContactPoint.y != 0)
				{
					num = BodyWiggleFac * (0.5f + 0.5f * Mathf.Sin(bodyWiggle * 2f * (float)Math.PI)) * 0.2f;
					break;
				}
			}
		}
		if (num < 0.1f || BodyForce > 1.2f)
		{
			bodyWiggleCounter++;
		}
		else
		{
			bodyWiggleCounter--;
		}
		float num2 = lizardParams.baseSpeed * num * Mathf.Lerp(runSpeed, 1f, BodyForce - 1f);
		num2 *= Mathf.Lerp(UnityEngine.Random.value, 1f, (LizardState.health - 0.5f) * 2f);
		LizardBreedParams.SpeedMultiplier speedMultiplier = lizardParams.TerrainSpeed(room.aimap.getAItile(base.bodyChunks[0].pos).acc) + lizardParams.TerrainSpeed(room.aimap.getAItile(base.bodyChunks[1].pos).acc) + lizardParams.TerrainSpeed(room.aimap.getAItile(base.bodyChunks[2].pos).acc);
		if (ModManager.MSC && base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
		{
			if (room.aimap.getAItile(base.mainBodyChunk.pos).narrowSpace || room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				num2 *= 1.75f;
			}
			if (base.safariControlled)
			{
				num2 *= 0.75f;
			}
		}
		if (ModManager.MMF && room.gravity <= zeroGravityMovementThreshold)
		{
			speedMultiplier.speed = 3.2f;
			speedMultiplier.up = 1f;
			speedMultiplier.down = 1f;
			speedMultiplier.horizontal = 1f;
		}
		speedMultiplier /= 3f;
		num2 *= speedMultiplier.speed;
		IntVector2 intVector = followingConnection.DestTile - followingConnection.StartTile;
		if (intVector.x != 0)
		{
			return num2 * speedMultiplier.horizontal;
		}
		if (intVector.y > 0)
		{
			return num2 * speedMultiplier.up;
		}
		return num2 * speedMultiplier.down;
	}

	private void GripPointBehavior()
	{
		if (ModManager.MMF && gripPoint.HasValue && base.Submersion >= 0.8f)
		{
			gripPoint = null;
			followingConnection = default(MovementConnection);
		}
		else if (base.mainBodyChunk.vel.magnitude > 10f && inAllowedTerrainCounter > 5)
		{
			gripPoint = null;
			followingConnection = default(MovementConnection);
		}
		else if (!applyGravity)
		{
			MovementConnection movementConnection = (base.abstractCreature.abstractAI.RealAI.pathFinder as LizardPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), 0, actuallyFollowingThisPath: true);
			movementAnimation = null;
			if (followingConnection != default(MovementConnection) && followingConnection.type == MovementConnection.MovementType.DropToClimb && movementConnection != default(MovementConnection) && movementConnection.DestTile.y - movementConnection.StartTile.y == 0 && Math.Abs(movementConnection.DestTile.x - movementConnection.StartTile.x) == 1)
			{
				movementConnection.type = MovementConnection.MovementType.DropToClimb;
				movementAnimation = new MovementAnimation(this, movementConnection, 1, 0.99f, 0.75f);
			}
			gripPoint = null;
			followingConnection = default(MovementConnection);
		}
		else
		{
			if (base.bodyChunks[1].pos.y < base.bodyChunks[0].pos.y)
			{
				base.bodyChunks[1].vel.y += UnityEngine.Random.value * 2f;
			}
			if (upcomingConnections.Count > 0)
			{
				base.bodyChunks[1].vel.x += 3f * (float)IntVector2.ClampAtOne(room.GetTilePosition(base.mainBodyChunk.pos) - upcomingConnections[upcomingConnections.Count - 1].DestTile).x;
				base.bodyChunks[2].vel.x += UnityEngine.Random.value * 3f * (float)IntVector2.ClampAtOne(room.GetTilePosition(base.mainBodyChunk.pos) - upcomingConnections[upcomingConnections.Count - 1].DestTile).x;
			}
			else
			{
				base.bodyChunks[1].vel.x += -2f + UnityEngine.Random.value * 4f;
			}
		}
		base.mainBodyChunk.vel *= 0f;
		if (gripPoint.HasValue)
		{
			base.mainBodyChunk.vel -= base.mainBodyChunk.pos - gripPoint.Value;
		}
	}

	private void SwimBehavior()
	{
		swim = Mathf.Clamp(swim + 1f / 15f, 0f, 1f);
		desperationSmoother = Mathf.Lerp(desperationSmoother, 30f, 0.1f);
		bool flag = !ModManager.MMF || (base.Template.type != CreatureTemplate.Type.Salamander && (!ModManager.MSC || base.Template.type != MoreSlugcatsEnums.CreatureTemplateType.EelLizard));
		if (base.bodyChunks[0].submersion > 0f && base.bodyChunks[0].submersion < 1f && flag && (followingConnection == default(MovementConnection) || followingConnection.DestTile.y == room.defaultWaterLevel))
		{
			base.bodyChunks[0].vel.y *= 0.8f;
			base.bodyChunks[0].vel.y += Mathf.Clamp(room.FloatWaterLevel(base.bodyChunks[0].pos.x) - base.bodyChunks[0].pos.y, -10f, 10f) * 0.1f;
		}
		WorldCoordinate worldCoordinate = room.GetWorldCoordinate(base.mainBodyChunk.pos);
		if (base.Template.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard))
		{
			if (ModManager.MMF)
			{
				applyGravity = true;
			}
			if (!base.safariControlled && AI.behavior == LizardAI.Behavior.Lurk && Custom.ManhattanDistance(AI.pathFinder.GetDestination, room.GetWorldCoordinate(base.bodyChunks[2].pos)) < 3)
			{
				base.bodyChunks[2].vel += Vector2.ClampMagnitude(room.MiddleOfTile(AI.pathFinder.GetDestination) - base.bodyChunks[2].pos, 10f) / 7f;
				base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[2].pos, room.MiddleOfTile(AI.lurkTracker.lookPosition)) * 0.2f;
				salamanderLurk = true;
			}
			else
			{
				salamanderLurk = false;
				Vector2 vector = new Vector2(0f, 0f);
				if (!AI.pathFinder.GetDestination.NodeDefined && AI.pathFinder.GetDestination.room == room.abstractRoom.index && room.GetTile(AI.pathFinder.GetDestination).AnyWater && room.VisualContact(base.mainBodyChunk.pos, room.MiddleOfTile(AI.pathFinder.GetDestination)))
				{
					vector = Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(AI.pathFinder.GetDestination));
					if (base.safariControlled)
					{
						vector = ((inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.AnyDirectionalInput) ? new Vector2(inputWithoutDiagonals.Value.x, inputWithoutDiagonals.Value.y).normalized : Vector2.zero);
					}
				}
				else
				{
					if (followingConnection == default(MovementConnection))
					{
						followingConnection = (AI.pathFinder as LizardPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), null, actuallyFollowingThisPath: true);
					}
					if (base.safariControlled)
					{
						followingConnection = default(MovementConnection);
						if (inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.AnyDirectionalInput)
						{
							followingConnection = new MovementConnection(MovementConnection.MovementType.Standard, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithoutDiagonals.Value.x, inputWithoutDiagonals.Value.y) * 40f), 2);
						}
					}
					if (followingConnection != default(MovementConnection))
					{
						vector = Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(followingConnection.destinationCoord));
						if (ModManager.MMF && !room.GetTile(AI.pathFinder.GetDestination).AnyWater && base.Submersion >= 1f)
						{
							vector = Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(base.mainBodyChunk.pos));
							if (AI.pathFinder.DestInRoom)
							{
								vector += Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(AI.pathFinder.GetDestination));
								vector += new Vector2(0f, -1f);
							}
						}
					}
				}
				float num = 1f;
				if (ModManager.MSC && base.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
				{
					num = ((!base.abstractCreature.controlled) ? 1.4f : 2.6f);
				}
				vector *= num;
				base.mainBodyChunk.vel += vector * 1.3f * Mathf.Lerp(0.5f, 1f, AI.runSpeed) * (room.GetTile(base.mainBodyChunk.pos).WaterSurface ? 0.7f : 1f);
				base.bodyChunks[1].vel -= vector * 0.3f * Mathf.Lerp(0.5f, 1f, AI.runSpeed) * (room.GetTile(base.mainBodyChunk.pos).WaterSurface ? 0.7f : 1f);
			}
		}
		else
		{
			worldCoordinate.y = Custom.IntClamp(worldCoordinate.y, room.defaultWaterLevel, room.TileHeight);
			if (followingConnection == default(MovementConnection))
			{
				followingConnection = (base.abstractCreature.abstractAI.RealAI.pathFinder as LizardPather).FollowPath(worldCoordinate, null, actuallyFollowingThisPath: true);
				timeSpentTryingThisMove = 0;
			}
			if (swim > 0.5f)
			{
				inAllowedTerrainCounter = 0;
			}
			if (followingConnection != default(MovementConnection))
			{
				base.mainBodyChunk.vel.x += Mathf.Clamp(room.MiddleOfTile(followingConnection.destinationCoord).x - base.mainBodyChunk.pos.x, -1f, 1f) * lizardParams.swimSpeed * BodyForce;
				if (room.GetTile(followingConnection.DestTile).AnyWater && room.GetTilePosition(base.mainBodyChunk.pos).x == followingConnection.DestTile.x && room.GetTilePosition(base.mainBodyChunk.pos).y == followingConnection.DestTile.y)
				{
					followingConnection = default(MovementConnection);
				}
			}
		}
		if (followingConnection != default(MovementConnection))
		{
			if (worldCoordinate.x != followingConnection.StartTile.x && worldCoordinate.x != followingConnection.DestTile.x)
			{
				followingConnection = default(MovementConnection);
			}
			else if (worldCoordinate.Tile == followingConnection.DestTile)
			{
				followingConnection = default(MovementConnection);
			}
		}
		if (followingConnection != default(MovementConnection) && followingConnection.destinationCoord.TileDefined && !room.GetTile(followingConnection.DestTile).AnyWater)
		{
			inAllowedTerrainCounter = lizardParams.regainFootingCounter + 2;
			swim = 0f;
			if (followingConnection.destinationCoord.TileDefined && room.aimap.TileAccessibleToCreature(new IntVector2(followingConnection.DestTile.x, followingConnection.DestTile.y + 1), base.Template))
			{
				movementAnimation = new MovementAnimation(this, new MovementConnection(MovementConnection.MovementType.ReachUp, followingConnection.startCoord, new WorldCoordinate(followingConnection.destinationCoord.room, followingConnection.DestTile.x, followingConnection.DestTile.y + 1, -1), 2), 1, 0.99f, 0.75f);
			}
		}
	}
}
