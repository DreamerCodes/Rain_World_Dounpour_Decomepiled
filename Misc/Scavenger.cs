using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Scavenger : AirBreatherCreature, ILookingAtCreatures, Weapon.INotifyOfFlyingWeapons
{
	public class MovementMode : ExtEnum<MovementMode>
	{
		public static readonly MovementMode Run = new MovementMode("Run", register: true);

		public static readonly MovementMode Crawl = new MovementMode("Crawl", register: true);

		public static readonly MovementMode Climb = new MovementMode("Climb", register: true);

		public static readonly MovementMode Swim = new MovementMode("Swim", register: true);

		public static readonly MovementMode StandStill = new MovementMode("StandStill", register: true);

		public MovementMode(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public abstract class ScavengerAnimation
	{
		public class ID : ExtEnum<ID>
		{
			public static readonly ID Rummage = new ID("Rummage", register: true);

			public static readonly ID Throw = new ID("Throw", register: true);

			public static readonly ID ThrowCharge = new ID("ThrowCharge", register: true);

			public static readonly ID Look = new ID("Look", register: true);

			public static readonly ID GeneralPoint = new ID("GeneralPoint", register: true);

			public static readonly ID BackOff = new ID("BackOff", register: true);

			public static readonly ID PlayerMayPass = new ID("PlayerMayPass", register: true);

			public static readonly ID WantToTrade = new ID("WantToTrade", register: true);

			public ID(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Scavenger scavenger;

		public ID id;

		public int age;

		public virtual bool Continue => true;

		public virtual bool Active => true;

		public ScavengerAnimation(Scavenger scavenger, ID id)
		{
			this.id = id;
			this.scavenger = scavenger;
		}

		public virtual void Update()
		{
			age++;
		}
	}

	public class RummageAnimation : ScavengerAnimation
	{
		public WorldCoordinate sitPos;

		public Vector2[] handPositions;

		public Vector2 headLookAt;

		public Vector2 eyesLookAt;

		public Vector2? rummageAtPos;

		public int pause;

		public int holdUpAndLook;

		public int freeze;

		public bool handScraping;

		public float lookAtCloseObj;

		private Vector2? holdUpPos;

		public override bool Continue => RummagePossible(scavenger);

		public override bool Active
		{
			get
			{
				if (pause < 1)
				{
					return rummageAtPos.HasValue;
				}
				return false;
			}
		}

		public RummageAnimation(Scavenger scavenger)
			: base(scavenger, ID.Rummage)
		{
			sitPos = scavenger.room.GetWorldCoordinate(scavenger.occupyTile);
			handPositions = new Vector2[3];
			for (int i = 0; i < handPositions.Length; i++)
			{
				handPositions[i] = scavenger.room.MiddleOfTile(sitPos) + Custom.RNV() * 20f * UnityEngine.Random.value;
			}
			pause = 20;
		}

		public override void Update()
		{
			base.Update();
			if (pause > 0)
			{
				pause--;
			}
			else
			{
				scavenger.bodyChunks[1].vel *= 0.8f;
				scavenger.bodyChunks[1].vel += Vector2.ClampMagnitude(scavenger.room.MiddleOfTile(sitPos) + new Vector2(0f, -6f) - scavenger.bodyChunks[1].pos, 10f) / 5f;
			}
			if (holdUpAndLook > 0)
			{
				holdUpAndLook--;
				if (!holdUpPos.HasValue)
				{
					holdUpPos = scavenger.mainBodyChunk.pos + Custom.DegToVec((30f + UnityEngine.Random.value * 60f) * ((handPositions[2].x < scavenger.mainBodyChunk.pos.x) ? (-1f) : 1f)) * Mathf.Lerp(20f, 60f, UnityEngine.Random.value);
					if (scavenger.room.GetTile(holdUpPos.Value).Solid)
					{
						holdUpPos = null;
					}
				}
				if (holdUpPos.HasValue)
				{
					headLookAt = Vector2.Lerp(headLookAt, holdUpPos.Value, 0.03f);
					eyesLookAt = Vector2.Lerp(eyesLookAt, holdUpPos.Value, 0.03f);
					if (rummageAtPos.HasValue)
					{
						handPositions[0] = rummageAtPos.Value;
					}
					handPositions[1] = Vector2.Lerp(handPositions[1], holdUpPos.Value + Custom.RNV() * 0.5f, 0.3f);
					lookAtCloseObj = Mathf.Lerp(lookAtCloseObj, 1f, 0.08f);
				}
				return;
			}
			lookAtCloseObj = Mathf.Max(lookAtCloseObj - 1f / 30f, 0f);
			headLookAt = Vector2.Lerp(headLookAt, handPositions[2] + new Vector2(0f, 21f), 0.1f);
			eyesLookAt = Vector2.Lerp(eyesLookAt, handPositions[2] + new Vector2(0f, -21f), 0.1f);
			if (freeze > 0)
			{
				freeze--;
				return;
			}
			if (UnityEngine.Random.value < 0.05f)
			{
				handScraping = !handScraping;
			}
			if (rummageAtPos.HasValue)
			{
				for (int i = 0; i < handPositions.Length; i++)
				{
					if (UnityEngine.Random.value < 0.1f)
					{
						handPositions[i] = rummageAtPos.Value + Custom.RNV() * 14f * UnityEngine.Random.value;
					}
					else if (handScraping)
					{
						handPositions[i] += Custom.RNV() * UnityEngine.Random.value * 5f;
						if (!scavenger.room.GetTile(handPositions[i] + new Vector2(0f, -5f)).Solid || !Custom.DistLess(handPositions[i], rummageAtPos.Value, 20f))
						{
							handPositions[i] = rummageAtPos.Value + Custom.RNV() * 14f * UnityEngine.Random.value;
						}
					}
				}
				if (UnityEngine.Random.value < 0.1f)
				{
					FindNewRummageAtPos(rummageAtPos.Value + Custom.RNV() * 10f * UnityEngine.Random.value);
				}
				else if (UnityEngine.Random.value < 1f / Mathf.Lerp(1000f, 140f, scavenger.AI.ActNervous))
				{
					rummageAtPos = null;
					pause = UnityEngine.Random.Range(80, 280);
				}
				if (pause == 0)
				{
					if (UnityEngine.Random.value < 0.0007142857f)
					{
						holdUpAndLook = 60;
						holdUpPos = null;
					}
					else if (UnityEngine.Random.value < 1f / Mathf.Lerp(1000f, 250f, scavenger.AI.ActNervous))
					{
						freeze = UnityEngine.Random.Range(4, 100 - (int)(80f * scavenger.abstractCreature.personality.energy));
					}
				}
			}
			else
			{
				FindNewRummageAtPos(scavenger.mainBodyChunk.pos + Custom.DegToVec(90f + 180f * UnityEngine.Random.value) * 70f);
			}
		}

		private void FindNewRummageAtPos(Vector2 aimAt)
		{
			Vector2? vector = SharedPhysics.ExactTerrainRayTracePos(scavenger.room, scavenger.mainBodyChunk.pos, aimAt);
			if (vector.HasValue && Custom.DistLess(scavenger.mainBodyChunk.pos, vector.Value, 60f) && !Custom.DistLess(scavenger.bodyChunks[1].pos, vector.Value, 30f) && vector.Value.y > scavenger.bodyChunks[1].pos.y - 20f)
			{
				rummageAtPos = vector;
			}
		}

		public static bool RummagePossible(Scavenger scav)
		{
			if (scav.safariControlled)
			{
				if (scav.room.GetTile(scav.occupyTile + new IntVector2(0, -1)).Solid && scav.room.aimap.getAItile(scav.bodyChunks[1].pos).acc == AItile.Accessibility.Floor && scav.inputWithDiagonals.HasValue && scav.inputWithDiagonals.Value.y < 0)
				{
					return scav.inputWithDiagonals.Value.x == 0;
				}
				return false;
			}
			if (scav.movMode != MovementMode.StandStill)
			{
				return false;
			}
			if (scav.AI.behavior != ScavengerAI.Behavior.Idle)
			{
				return false;
			}
			if (scav.AI.discomfortWithOtherCreatures > 0.2f)
			{
				return false;
			}
			if (scav.AI.scared > 0.2f)
			{
				return false;
			}
			if (scav.moving)
			{
				return false;
			}
			if (scav.swingPos.HasValue)
			{
				return false;
			}
			if (!scav.room.GetTile(scav.occupyTile + new IntVector2(0, -1)).Solid)
			{
				return false;
			}
			if (scav.room.aimap.getAItile(scav.bodyChunks[1].pos).acc != AItile.Accessibility.Floor)
			{
				return false;
			}
			return true;
		}
	}

	public class ThrowAnimation : ScavengerAnimation
	{
		public PhysicalObject thrownObject;

		public float flip;

		public override bool Continue => age < 20;

		public ThrowAnimation(Scavenger scavenger, PhysicalObject thrownObject, float flip)
			: base(scavenger, ID.Throw)
		{
			this.thrownObject = thrownObject;
			this.flip = flip;
			scavenger.bodyChunks[2].pos = Vector2.Lerp(scavenger.bodyChunks[2].pos, scavenger.mainBodyChunk.pos + new Vector2(flip * 30f, 0f), 0.8f);
		}

		public override void Update()
		{
			base.Update();
			scavenger.flip = flip;
			if (age < 10)
			{
				scavenger.bodyChunks[2].vel += Custom.DirVec(scavenger.bodyChunks[2].pos, thrownObject.firstChunk.pos) * 3f;
			}
			if (age == 18)
			{
				scavenger.AI.arrangeInventoryCounter = Math.Min(scavenger.AI.arrangeInventoryCounter, 10);
			}
		}
	}

	public class ThrowChargeAnimation : ScavengerAnimation
	{
		public BodyChunk target;

		public int discontinue;

		public float cycle;

		public float shake;

		public Vector2? aimTarget;

		public Vector2 Direction => Custom.DirVec(scavenger.mainBodyChunk.pos, UseTarget);

		public Vector2 UseTarget
		{
			get
			{
				if (target != null)
				{
					return target.pos;
				}
				if (aimTarget.HasValue)
				{
					return aimTarget.Value;
				}
				return Vector2.zero;
			}
		}

		public override bool Continue => discontinue < 60;

		public override bool Active => discontinue < 20;

		public ThrowChargeAnimation(Scavenger scavenger, BodyChunk target)
			: base(scavenger, ID.ThrowCharge)
		{
			this.target = target;
			aimTarget = null;
		}

		public override void Update()
		{
			base.Update();
			discontinue++;
			cycle += 1f;
			shake *= 0.95f;
		}
	}

	public abstract class AttentiveAnimation : ScavengerAnimation
	{
		public Tracker.CreatureRepresentation creatureRep;

		public Vector2 point;

		public bool stop;

		public Vector2 LookPoint
		{
			get
			{
				if (creatureRep != null)
				{
					if (creatureRep.VisualContact && creatureRep.representedCreature.realizedCreature != null)
					{
						return creatureRep.representedCreature.realizedCreature.DangerPos;
					}
					return scavenger.room.MiddleOfTile(creatureRep.BestGuessForPosition());
				}
				return point;
			}
		}

		public AttentiveAnimation(Scavenger scavenger, Tracker.CreatureRepresentation creatureRep, Vector2 point, bool stop, ID id)
			: base(scavenger, id)
		{
			this.point = point;
			this.creatureRep = creatureRep;
			this.stop = stop;
			scavenger.bodyChunks[2].pos = Vector2.Lerp(scavenger.bodyChunks[2].pos, scavenger.bodyChunks[0].pos + Custom.DirVec(scavenger.bodyChunks[0].pos, LookPoint) * scavenger.bodyChunkConnections[1].distance, 0.1f + 0.4f * scavenger.AI.ActNervous);
			scavenger.bodyChunks[2].vel *= 0.5f;
			scavenger.bodyChunks[2].vel += Custom.DirVec(scavenger.bodyChunks[0].pos, LookPoint) * 1.1f;
		}
	}

	public class LookAnimation : AttentiveAnimation
	{
		public float prio;

		public override bool Continue => (float)age < Mathf.Lerp(Mathf.Lerp(160f, 115f, scavenger.AI.ActNervous), 10f, Mathf.Max(scavenger.AI.currentUtility, scavenger.AI.agitation));

		public LookAnimation(Scavenger scavenger, Tracker.CreatureRepresentation creatureRep, Vector2 point, float prio, bool stop)
			: base(scavenger, creatureRep, point, stop, ID.Look)
		{
			this.prio = prio;
		}

		public override void Update()
		{
			prio -= 0.025f;
			base.Update();
		}
	}

	public abstract class PointingAnimation : AttentiveAnimation
	{
		protected int pointingArm;

		public float cycle;

		public int PointingArm
		{
			get
			{
				if (scavenger.grasps[0] != null)
				{
					if (!(scavenger.grasps[0].grabbed is Spear))
					{
						return 1;
					}
					return 0;
				}
				return pointingArm;
			}
		}

		public PointingAnimation(Scavenger scavenger, Tracker.CreatureRepresentation creatureRep, Vector2 point, ID id)
			: base(scavenger, creatureRep, point, stop: true, id)
		{
			pointingArm = ((!(UnityEngine.Random.value < 0.5f)) ? 1 : 0);
		}

		public override void Update()
		{
			cycle += Mathf.Lerp(0.5f, 1.5f, Mathf.Pow(scavenger.abstractCreature.personality.energy * scavenger.AI.agitation, 0.5f));
			base.Update();
		}
	}

	public class GeneralPointAnimation : PointingAnimation
	{
		private List<Tracker.CreatureRepresentation> group;

		private int groupStartNum;

		public override bool Continue
		{
			get
			{
				if (scavenger.IsControlPointing)
				{
					return true;
				}
				return (float)age < Mathf.Lerp(Mathf.Lerp(80f, 20f, scavenger.AI.ActNervous) + 300f * Mathf.Pow(Mathf.InverseLerp(0f, groupStartNum, group.Count), 1.5f - scavenger.abstractCreature.personality.sympathy), Mathf.Lerp(40f, 20f, scavenger.AI.ActNervous), Mathf.Lerp(scavenger.AI.currentUtility, scavenger.AI.agitation, 0.5f));
			}
		}

		public GeneralPointAnimation(Scavenger scavenger, Tracker.CreatureRepresentation creatureRep, Vector2 point, List<Tracker.CreatureRepresentation> group)
			: base(scavenger, creatureRep, point, ID.GeneralPoint)
		{
			this.group = group;
			groupStartNum = group.Count;
		}

		public override void Update()
		{
			base.Update();
			if (group.Count < 1 || creatureRep == null || creatureRep.representedCreature.realizedCreature == null)
			{
				return;
			}
			int index = UnityEngine.Random.Range(0, group.Count);
			if (group[index].representedCreature.realizedCreature != null && group[index].representedCreature.realizedCreature.room == scavenger.room && group[index].representedCreature.realizedCreature is Scavenger)
			{
				if ((group[index].representedCreature.realizedCreature as Scavenger).AI.tracker.RepresentationForObject(creatureRep.representedCreature.realizedCreature, AddIfMissing: false) != null)
				{
					group.RemoveAt(index);
				}
			}
			else
			{
				group.RemoveAt(index);
			}
		}
	}

	public abstract class CommunicationAnimation : AttentiveAnimation
	{
		protected int gestureArm;

		public bool pointWithSpears;

		public int GestureArm
		{
			get
			{
				if (scavenger.grasps[0] != null)
				{
					if (!(scavenger.grasps[0].grabbed is Spear))
					{
						return 1;
					}
					return 0;
				}
				return gestureArm;
			}
		}

		public CommunicationAnimation(Scavenger scavenger, Tracker.CreatureRepresentation creatureRep, Vector2 point, ID id)
			: base(scavenger, creatureRep, point, stop: true, id)
		{
			gestureArm = ((!(UnityEngine.Random.value < 0.5f)) ? 1 : 0);
		}

		public virtual Vector2 GestureArmPos()
		{
			return scavenger.mainBodyChunk.pos + Custom.DirVec(scavenger.mainBodyChunk.pos, base.LookPoint) * 60f;
		}
	}

	public class BackOffAnimation : CommunicationAnimation
	{
		public float cycle;

		public bool discontinue;

		public override bool Continue
		{
			get
			{
				if (age < 120)
				{
					return !discontinue;
				}
				return false;
			}
		}

		public BackOffAnimation(Scavenger scavenger, Tracker.CreatureRepresentation creatureRep, Vector2 point)
			: base(scavenger, creatureRep, point, ID.BackOff)
		{
		}

		public override void Update()
		{
			base.Update();
			cycle += Mathf.Lerp(0.5f, 1.5f, Mathf.Pow(scavenger.abstractCreature.personality.energy * scavenger.AI.agitation, 0.5f));
		}

		public override Vector2 GestureArmPos()
		{
			return scavenger.mainBodyChunk.pos + Custom.DirVec(scavenger.mainBodyChunk.pos, base.LookPoint) * 50f + new Vector2(0f, Mathf.Sin(0.47123894f * cycle) * 20f);
		}
	}

	public class PlayerMayPassAnimation : CommunicationAnimation
	{
		public float cycle;

		public ScavengerOutpost outPost;

		public ScavengerOutpost.PlayerTracker outpostPlayerTracker;

		public override bool Continue
		{
			get
			{
				if (outpostPlayerTracker != null && outpostPlayerTracker.PlayerOnOtherSide)
				{
					Custom.Log("discont. wave through animation");
					return false;
				}
				return age < 120;
			}
		}

		public PlayerMayPassAnimation(Scavenger scavenger, Tracker.CreatureRepresentation creatureRep, Vector2 point, ScavengerOutpost outPost)
			: base(scavenger, creatureRep, point, ID.PlayerMayPass)
		{
			this.outPost = outPost;
			if (outPost == null || creatureRep == null)
			{
				return;
			}
			for (int i = 0; i < outPost.playerTrackers.Count; i++)
			{
				if (outPost.playerTrackers[i].player == creatureRep.representedCreature)
				{
					outpostPlayerTracker = outPost.playerTrackers[i];
					break;
				}
			}
		}

		public override void Update()
		{
			base.Update();
			cycle += Mathf.Lerp(0.5f, 1.5f, Mathf.Pow(scavenger.abstractCreature.personality.energy * scavenger.AI.agitation, 0.5f)) / 20f;
		}

		public override Vector2 GestureArmPos()
		{
			if (creatureRep.representedCreature.realizedCreature == null)
			{
				return scavenger.mainBodyChunk.pos;
			}
			if (creatureRep.representedCreature.realizedCreature.DangerPos.x < outPost.placedObj.pos.x)
			{
				return scavenger.mainBodyChunk.pos + Custom.FlattenVectorAlongAxis(Custom.DegToVec(-90f + (cycle - Mathf.Floor(cycle)) * 180f), 0f, 0.5f) * 60f;
			}
			return scavenger.mainBodyChunk.pos + Custom.FlattenVectorAlongAxis(Custom.DegToVec(90f - (cycle - Mathf.Floor(cycle)) * 180f), 0f, 0.5f) * 60f;
		}
	}

	public class WantToTradeAnimation : CommunicationAnimation
	{
		public PhysicalObject desiredItem;

		private bool pointDown;

		private float downPoint;

		private float cycle;

		public override bool Continue
		{
			get
			{
				if (age < 120 && desiredItem != null)
				{
					return desiredItem.grabbedBy.Count > 0;
				}
				return false;
			}
		}

		public WantToTradeAnimation(Scavenger scavenger, Tracker.CreatureRepresentation creatureRep, Vector2 point, PhysicalObject desiredItem)
			: base(scavenger, creatureRep, point, ID.WantToTrade)
		{
			this.desiredItem = desiredItem;
			pointWithSpears = true;
		}

		public override void Update()
		{
			base.Update();
			cycle += Mathf.Lerp(0.5f, 1.5f, Mathf.Pow(scavenger.abstractCreature.personality.energy * scavenger.AI.agitation, 0.5f));
			if (cycle > 15f)
			{
				pointDown = !pointDown;
				cycle -= 30f;
			}
			downPoint = Custom.LerpAndTick(downPoint, pointDown ? 1f : 0f, 0.05f, 0.1f);
		}

		public override Vector2 GestureArmPos()
		{
			return scavenger.mainBodyChunk.pos + Custom.DirVec(scavenger.mainBodyChunk.pos, desiredItem.firstChunk.pos + new Vector2(0f, -60f * downPoint)) * 60f;
		}
	}

	public class DirectAwayThroneAnimation : CommunicationAnimation
	{
		public bool gestureActive;

		public float cycle;

		public float gestureReach;

		public int gestureCount;

		public Player player;

		public override bool Continue => scavenger.kingWaiting;

		public DirectAwayThroneAnimation(Scavenger scavenger, Tracker.CreatureRepresentation creatureRep, Vector2 point)
			: base(scavenger, creatureRep, point, ID.BackOff)
		{
		}

		public override void Update()
		{
			base.Update();
			gestureArm = 0;
			if (!gestureActive && UnityEngine.Random.value < 0.06f)
			{
				if (scavenger.graphicsModule != null)
				{
					(scavenger.graphicsModule as ScavengerGraphics).ShockReaction(UnityEngine.Random.Range(0.25f, 1f));
				}
				gestureActive = true;
				gestureCount++;
				return;
			}
			if (gestureActive && UnityEngine.Random.value < 0.03f && gestureReach > 0.95f)
			{
				gestureActive = false;
			}
			if (gestureActive)
			{
				gestureReach = Mathf.Lerp(gestureReach, 1f, 0.25f);
			}
			else
			{
				gestureReach = Mathf.Lerp(gestureReach, 0f, 0.1f);
			}
			if (scavenger.room != null)
			{
				for (int i = 0; i < scavenger.room.game.Players.Count; i++)
				{
					if (scavenger.room.game.Players[i].realizedCreature != null)
					{
						player = scavenger.room.game.Players[i].realizedCreature as Player;
						break;
					}
				}
			}
			cycle += 0.5f;
		}

		public override Vector2 GestureArmPos()
		{
			if (!gestureActive)
			{
				return scavenger.mainBodyChunk.pos;
			}
			if (gestureCount % 2 == 0 && player != null)
			{
				return scavenger.mainBodyChunk.pos + Custom.DirVec(scavenger.mainBodyChunk.pos, player.mainBodyChunk.pos) * Custom.LerpElasticEaseOut(10f, 50f, gestureReach);
			}
			return scavenger.mainBodyChunk.pos + new Vector2(Custom.LerpElasticEaseOut(0f, -50f, gestureReach), 0f) + new Vector2(0f, Mathf.Sin(0.47123894f * cycle) * 5f);
		}
	}

	public class JumpFinder
	{
		public class JumpInstruction
		{
			public PathFinder.PathingCell goalCell;

			public int tick;

			public Vector2 startPos;

			public Vector2 initVel;

			public float power;

			public bool grabWhenLanding;

			public JumpInstruction(Vector2 startPos, Vector2 initVel, float power)
			{
				this.startPos = startPos;
				this.initVel = initVel;
				this.power = power;
			}
		}

		private Scavenger owner;

		public bool slatedForDeletion;

		public Room room;

		public IntVector2 startPos;

		public int fade;

		private PathFinder.PathingCell startCell;

		public JumpInstruction bestJump;

		public JumpInstruction currentJump;

		private Vector2 pos;

		private Vector2 lastPos;

		private Vector2 vel;

		private bool hasVenturedAwayFromTerrain;

		public Vector2? landingDirection;

		public Vector2 lastControlledDir;

		private IntVector2[] _cachedRtList = new IntVector2[100];

		public bool BeneficialMovement
		{
			get
			{
				if (bestJump != null && bestJump.goalCell != null && startPos.FloatDist(bestJump.goalCell.worldCoordinate.Tile) > 5f && owner.AI.pathFinder.GetDestination.Tile.FloatDist(bestJump.goalCell.worldCoordinate.Tile) > 3f && !PathWeightComparison(bestJump.goalCell, owner.jumpCell))
				{
					return !PathWeightComparison(bestJump.goalCell, startCell);
				}
				return false;
			}
		}

		public JumpFinder(Room room, Scavenger owner, IntVector2 startPos)
		{
			this.room = room;
			this.owner = owner;
			this.startPos = startPos;
			startCell = owner.AI.pathFinder.PathingCellAtWorldCoordinate(room.GetWorldCoordinate(startPos));
			NewTest();
		}

		public void Update()
		{
			if (owner.InStandardRunMode)
			{
				fade++;
				if (!PathWeightComparison(owner.jumpCell, startCell))
				{
					fade += 10;
				}
				if (fade > 40)
				{
					Destroy();
				}
			}
			if (owner.safariControlled)
			{
				Vector2 vector = Vector2.zero;
				if (owner.inputWithDiagonals.HasValue)
				{
					vector = new Vector2(Mathf.Sign(owner.inputWithDiagonals.Value.x) * (float)((owner.inputWithDiagonals.Value.x == 1) ? 1 : 0), Mathf.Sign(owner.inputWithDiagonals.Value.y) * (float)((owner.inputWithDiagonals.Value.y == 1) ? 1 : 0));
				}
				if (vector != lastControlledDir)
				{
					lastControlledDir = vector;
					NewTest();
				}
			}
			for (int num = Mathf.Max(1, 100 / Mathf.Max(1, owner.jumpFinders.Count)); num >= 0; num--)
			{
				Iterate();
			}
			if (owner.actOnJump != this)
			{
				return;
			}
			PathFinder.PathingCell pathingCell = bestJump.goalCell;
			for (int i = 0; i < 8; i++)
			{
				PathFinder.PathingCell pathingCell2 = owner.AI.pathFinder.PathingCellAtWorldCoordinate(bestJump.goalCell.worldCoordinate + Custom.eightDirections[i]);
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
			for (num = SharedPhysics.RayTracedTilesArray(lastPos, pos, _cachedRtList); num >= _cachedRtList.Length; num = SharedPhysics.RayTracedTilesArray(lastPos, pos, _cachedRtList))
			{
				Custom.LogWarning($"Scavenger JumpFinder ray tracing limit exceeded, extending cache to {_cachedRtList.Length + 100} and trying again!");
				Array.Resize(ref _cachedRtList, _cachedRtList.Length + 100);
			}
			Vector2 vector = Custom.PerpendicularVector(lastPos, pos);
			for (int i = 0; i < num; i++)
			{
				if (room.GetTile(_cachedRtList[i]).Solid || _cachedRtList[i].y < 0 || _cachedRtList[i].y < room.defaultWaterLevel || room.aimap.getAItile(_cachedRtList[i]).narrowSpace)
				{
					NewTest();
					return;
				}
				if (!hasVenturedAwayFromTerrain && room.aimap.getTerrainProximity(_cachedRtList[i]) > 1 && !room.GetTile(_cachedRtList[i]).verticalBeam && !room.GetTile(_cachedRtList[i]).horizontalBeam)
				{
					hasVenturedAwayFromTerrain = true;
				}
				if (hasVenturedAwayFromTerrain && room.aimap.TileAccessibleToCreature(_cachedRtList[i], owner.Template) && (room.aimap.getTerrainProximity(_cachedRtList[i]) == 1 || room.GetTile(_cachedRtList[i]).verticalBeam || room.GetTile(_cachedRtList[i]).horizontalBeam) && startPos.FloatDist(_cachedRtList[i]) > (float)Custom.IntClamp((int)(currentJump.initVel.magnitude / 3f), 5, 20) && owner.AI.pathFinder.GetDestination.Tile.FloatDist(_cachedRtList[i]) > 3f)
				{
					PathFinder.PathingCell pathingCell = owner.AI.pathFinder.PathingCellAtWorldCoordinate(room.GetWorldCoordinate(_cachedRtList[i]));
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
					}
				}
				if ((!room.GetTile(startPos + new IntVector2(0, 1)).Solid && room.GetTile(_cachedRtList[i] + new IntVector2(0, 1)).Solid) || room.GetTile(room.MiddleOfTile(_cachedRtList[i]) + vector * Custom.LerpMap(currentJump.tick, 5f, 20f, 10f, 20f)).Solid || room.GetTile(room.MiddleOfTile(_cachedRtList[i]) - vector * Custom.LerpMap(currentJump.tick, 5f, 20f, 10f, 20f)).Solid)
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
			float num = UnityEngine.Random.value * Mathf.Pow(owner.AI.runSpeedGoal, 0.5f);
			if (room.aimap.getTerrainProximity(startPos) > 1)
			{
				num *= 0.5f;
			}
			float num2 = Mathf.Lerp(14f, 50f, num);
			if (owner.grasps[0] != null)
			{
				num2 *= 0.75f;
			}
			num2 *= Mathf.Lerp(1f, 1f - owner.Injured, 0.5f + 0.5f * UnityEngine.Random.value);
			Vector2 initVel = Custom.DegToVec((45f * Mathf.Pow(UnityEngine.Random.value, 0.75f) + 135f * Mathf.Pow(UnityEngine.Random.value, 2f)) * ((UnityEngine.Random.value >= 0.5f) ? 1f : (-1f))) * num2;
			if (owner.safariControlled && owner.inputWithDiagonals.HasValue && (owner.inputWithDiagonals.Value.x != 0 || owner.inputWithDiagonals.Value.y != 0))
			{
				initVel = Custom.DegToVec(Custom.VecToDeg(new Vector2(owner.inputWithDiagonals.Value.x, owner.inputWithDiagonals.Value.y)) + 22.5f * UnityEngine.Random.value * ((UnityEngine.Random.value >= 0.5f) ? 1f : (-1f))) * num2;
			}
			currentJump = new JumpInstruction(room.MiddleOfTile(startPos), initVel, num);
			pos = room.MiddleOfTile(startPos);
			lastPos = pos;
			vel = currentJump.initVel;
			hasVenturedAwayFromTerrain = false;
			if (bestJump == null)
			{
				bestJump = currentJump;
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

		public void Destroy()
		{
			slatedForDeletion = true;
		}
	}

	public class PrepareToJumpAnimation : ScavengerAnimation
	{
		private bool cancelAnimation;

		public int animationLength;

		public float cycleTime;

		public override bool Continue => !cancelAnimation;

		public PrepareToJumpAnimation(Scavenger scavenger, int animationLength)
			: base(scavenger, MoreSlugcatsEnums.ScavengerAnimationID.PrepareToJump)
		{
			cancelAnimation = false;
			this.animationLength = animationLength;
		}

		public override void Update()
		{
			base.Update();
			if (scavenger.room == null)
			{
				return;
			}
			if (scavenger.actOnJump != null && scavenger.Consious && Custom.DistLess(scavenger.bodyChunks[0].pos, scavenger.room.MiddleOfTile(scavenger.actOnJump.bestJump.startPos), 50f))
			{
				scavenger.bodyChunks[0].vel *= 0.5f;
				scavenger.bodyChunks[0].pos = Vector2.Lerp(scavenger.bodyChunks[0].pos, scavenger.room.MiddleOfTile(scavenger.actOnJump.bestJump.startPos) - scavenger.actOnJump.bestJump.initVel.normalized * age * 0.5f, 0.2f);
				scavenger.bodyChunks[2].vel += scavenger.actOnJump.bestJump.initVel.normalized * 1.5f;
				scavenger.bodyChunks[1].vel -= scavenger.actOnJump.bestJump.initVel.normalized * 2f;
				cycleTime += 1f;
				scavenger.bodyChunks[1].pos += Custom.PerpendicularVector(Custom.DirVec(scavenger.bodyChunks[0].pos, scavenger.bodyChunks[1].pos)) * Mathf.Sin(cycleTime) * ((float)age / (float)animationLength) * 10f;
				if (age > animationLength - 5)
				{
					scavenger.animation = new JumpingAnimation(scavenger);
				}
			}
			else
			{
				cancelAnimation = true;
			}
		}
	}

	public class JumpingAnimation : ScavengerAnimation
	{
		public override bool Continue => scavenger.actOnJump != null;

		public JumpingAnimation(Scavenger scavenger)
			: base(scavenger, MoreSlugcatsEnums.ScavengerAnimationID.Jumping)
		{
			scavenger.Jump();
		}

		public override void Update()
		{
			base.Update();
			scavenger.AI.CheckThrow();
			scavenger.GoThroughFloors = true;
		}
	}

	public ScavengerAI AI;

	public Vector2 lookPoint;

	public Vector2? lastSafariJoinedLookPoint;

	public MovementConnection commitedToMove;

	public int commitToMoveCounter;

	public int commitedMoveFollowChunk;

	public bool drop;

	public int grabbedAttackCounter;

	public bool moving;

	public List<MovementConnection> connections;

	private MovementConnection shortcutComingUp;

	public Vector2? knucklePos;

	public Vector2? swingPos;

	public Vector2? nextSwingPos;

	public float flip;

	public int swingArm;

	public int swingClimbCounter;

	public int swingingForbidden;

	public IntVector2 climbOrientation;

	public IntVector2 occupyTile;

	public List<IntVector2> pastPositions;

	public int stuckCounter;

	public int notFollowingPathToCurrentGoalCounter;

	public int stuckOnShortcutCounter;

	private bool pathingWithExits;

	private int pathWithExitsCounter;

	public float visionFactor;

	public float narrowVision;

	public ScavengerAnimation animation;

	public bool climbingUpComing;

	public int ghostCounter;

	public IntVector2 lastNonSolidTile;

	public MovementMode movMode;

	public MovementMode lastMovMode;

	public int moveModeChangeCounter;

	public float swingProgress;

	public float swingRadius;

	public int footingCounter;

	public CreatureLooker critLooker;

	private List<MovementConnection> _cons0;

	public bool readyToReleaseMask;

	public Vector2? lastInputDir;

	public int timeSinceLastInputDir;

	private float fastReflexBuildUp;

	public List<JumpFinder> jumpFinders;

	public JumpFinder actOnJump;

	public JumpFinder controlledJumpFinder;

	public PathFinder.PathingCell jumpCell;

	public int jumpCounter;

	public int addDelay;

	public int spin;

	public AncientBot myRobot;

	public bool kingWaiting;

	public int armorPieces;

	public int explosionDamageCooldown;

	public static int ArenaScavID;

	private readonly List<WorldCoordinate> _cachedList = new List<WorldCoordinate>(10);

	public int dodgeDelay;

	private float dodgeSkill;

	private float meleeSkill;

	private float midRangeSkill;

	private float blockingSkill;

	private float reactionSkill;

	public BodyChunk immediatelyThrowAtChunk;

	private float reflexBuildUp;

	public Vector2 JoinedLookPoint
	{
		get
		{
			if (!base.Consious)
			{
				return base.mainBodyChunk.pos;
			}
			if (room == null)
			{
				return new Vector2(0f, 0f);
			}
			if (kingWaiting)
			{
				for (int i = 0; i < room.physicalObjects.Length; i++)
				{
					for (int j = 0; j < room.physicalObjects[i].Count; j++)
					{
						if (room.physicalObjects[i][j] is Player)
						{
							Vector2 pos = room.physicalObjects[i][j].firstChunk.pos;
							if (animation != null && animation is DirectAwayThroneAnimation)
							{
								return Vector2.Lerp(pos, (animation as DirectAwayThroneAnimation).GestureArmPos(), (animation as DirectAwayThroneAnimation).gestureReach);
							}
							return pos;
						}
					}
				}
			}
			if (base.safariControlled && lastInputDir.HasValue && timeSinceLastInputDir < 200)
			{
				lastSafariJoinedLookPoint = base.firstChunk.pos + new Vector2(lastInputDir.Value.x, lastInputDir.Value.y) * 256f;
				return lastSafariJoinedLookPoint.Value;
			}
			if (base.safariControlled)
			{
				if (!lastSafariJoinedLookPoint.HasValue)
				{
					return base.mainBodyChunk.pos;
				}
				return lastSafariJoinedLookPoint.Value;
			}
			if (AI.giftForMe != null && AI.giftForMe.realizedObject != null)
			{
				return AI.giftForMe.realizedObject.firstChunk.pos;
			}
			if (critLooker == null || critLooker.lookCreature == null)
			{
				return lookPoint;
			}
			if (critLooker.lookCreature.VisualContact && critLooker.lookCreature.representedCreature.realizedCreature != null)
			{
				return critLooker.lookCreature.representedCreature.realizedCreature.DangerPos;
			}
			return room.MiddleOfTile(critLooker.lookCreature.BestGuessForPosition());
		}
	}

	public Vector2 HeadLookDir => Vector2.ClampMagnitude(HeadLookPoint - base.mainBodyChunk.pos, 300f) / 300f;

	public bool Rummaging
	{
		get
		{
			if (animation != null && animation.Active)
			{
				return animation.id == ScavengerAnimation.ID.Rummage;
			}
			return false;
		}
	}

	public bool Pointing
	{
		get
		{
			if (animation != null && animation is PointingAnimation)
			{
				return animation.Active;
			}
			return false;
		}
	}

	public bool Communicating
	{
		get
		{
			if (animation != null && animation is CommunicationAnimation)
			{
				return animation.Active;
			}
			return false;
		}
	}

	public bool Charging
	{
		get
		{
			if (animation != null && animation is ThrowChargeAnimation)
			{
				return animation.Active;
			}
			return false;
		}
	}

	public bool CommitedToMoveIsDrop
	{
		get
		{
			if (commitedToMove == default(MovementConnection))
			{
				drop = false;
				return false;
			}
			return commitedToMove.IsDrop;
		}
	}

	public float LittleStuck
	{
		get
		{
			if (!base.safariControlled)
			{
				return Mathf.InverseLerp(5f, 20f, stuckCounter);
			}
			return 0f;
		}
	}

	public float ReallyStuck
	{
		get
		{
			if (!base.safariControlled)
			{
				return Mathf.InverseLerp(40f, 200f, stuckCounter);
			}
			return 0f;
		}
	}

	public IntVector2 NextTile
	{
		get
		{
			if (connections.Count > 0)
			{
				return connections[connections.Count - 1].DestTile;
			}
			return occupyTile;
		}
	}

	public override Vector2 VisionPoint => base.bodyChunks[2].pos;

	public float Injured
	{
		get
		{
			if (Elite)
			{
				return Mathf.InverseLerp(0.25f, 0f, (base.State as HealthState).health);
			}
			return Mathf.InverseLerp(0.5f, 0f, (base.State as HealthState).health);
		}
	}

	public float MovementSpeed
	{
		get
		{
			if (ModManager.MSC && animation != null && animation.id == MoreSlugcatsEnums.ScavengerAnimationID.Jumping)
			{
				return Mathf.Abs(base.abstractCreature.personality.energy - 0.5f) * Mathf.Lerp(1f, 0.1f, Injured);
			}
			if (base.safariControlled && AI.runSpeedGoal == 0f)
			{
				return 0f;
			}
			if (animation != null && animation.id == ScavengerAnimation.ID.Throw)
			{
				return 0f;
			}
			return Mathf.Max(LittleStuck, Mathf.Lerp(AI.runSpeedGoal, 0.3f + 0.7f * base.abstractCreature.personality.energy, Mathf.Abs(base.abstractCreature.personality.energy - 0.5f)) * Mathf.Lerp(1f, 0.1f, Injured));
		}
	}

	public Vector2 HeadLookPoint
	{
		get
		{
			if (animation != null && animation.Active && !kingWaiting)
			{
				if (!base.safariControlled)
				{
					if (animation is AttentiveAnimation)
					{
						return (animation as AttentiveAnimation).LookPoint;
					}
					if (animation.id == ScavengerAnimation.ID.Rummage)
					{
						return base.mainBodyChunk.pos + Custom.DirVec(base.mainBodyChunk.pos, (animation as RummageAnimation).headLookAt) * Mathf.Lerp(700f, 40f, (animation as RummageAnimation).lookAtCloseObj);
					}
				}
				if (animation.id == ScavengerAnimation.ID.Throw)
				{
					return (animation as ThrowAnimation).thrownObject.firstChunk.pos;
				}
				if (animation.id == ScavengerAnimation.ID.ThrowCharge)
				{
					return (animation as ThrowChargeAnimation).UseTarget;
				}
			}
			return JoinedLookPoint;
		}
	}

	public Vector2 EyesLookPoint
	{
		get
		{
			if (animation != null && animation.Active && !kingWaiting)
			{
				if (!base.safariControlled)
				{
					if (animation is AttentiveAnimation)
					{
						return (animation as AttentiveAnimation).LookPoint;
					}
					if (animation.id == ScavengerAnimation.ID.Rummage)
					{
						return base.mainBodyChunk.pos + Custom.DirVec(base.mainBodyChunk.pos, (animation as RummageAnimation).eyesLookAt) * 700f;
					}
				}
				if (animation.id == ScavengerAnimation.ID.Throw)
				{
					return (animation as ThrowAnimation).thrownObject.firstChunk.pos;
				}
				if (animation.id == ScavengerAnimation.ID.ThrowCharge)
				{
					return (animation as ThrowChargeAnimation).UseTarget;
				}
			}
			return JoinedLookPoint;
		}
	}

	public bool Elite
	{
		get
		{
			if (ModManager.MSC)
			{
				if (!(base.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite))
				{
					return base.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing;
				}
				return true;
			}
			return false;
		}
	}

	public bool King
	{
		get
		{
			if (ModManager.MSC)
			{
				return base.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing;
			}
			return false;
		}
	}

	public bool AllowIdleMoves
	{
		get
		{
			if (!base.safariControlled || MovementSpeed != 0f)
			{
				if (ModManager.MSC && animation != null)
				{
					return animation.id != MoreSlugcatsEnums.ScavengerAnimationID.Jumping;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsControlPointing
	{
		get
		{
			if (Elite)
			{
				if (base.safariControlled && inputWithDiagonals.HasValue && inputWithDiagonals.Value.jmp)
				{
					return inputWithDiagonals.Value.pckp;
				}
				return false;
			}
			if (base.safariControlled && inputWithDiagonals.HasValue)
			{
				return inputWithDiagonals.Value.jmp;
			}
			return false;
		}
	}

	public Vector2 jumpToPoint => room.MiddleOfTile(actOnJump.bestJump.goalCell.worldCoordinate);

	public bool InStandardRunMode
	{
		get
		{
			if (actOnJump == null)
			{
				if (animation != null)
				{
					if (animation.id != MoreSlugcatsEnums.ScavengerAnimationID.PrepareToJump)
					{
						return animation.id != MoreSlugcatsEnums.ScavengerAnimationID.Jumping;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public bool NoRunBehavior
	{
		get
		{
			if (actOnJump != null && animation != null && animation.id == MoreSlugcatsEnums.ScavengerAnimationID.Jumping)
			{
				return jumpCounter < actOnJump.bestJump.tick;
			}
			return false;
		}
	}

	private float MeleeRange
	{
		get
		{
			if (Elite)
			{
				return 80f + 110f * meleeSkill;
			}
			return 40f + 100f * meleeSkill;
		}
	}

	private float MidRange
	{
		get
		{
			if (Elite)
			{
				return 350f + 200f * midRangeSkill;
			}
			return 200f + 200f * midRangeSkill;
		}
	}

	public Scavenger(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		base.bodyChunks = new BodyChunk[3];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 9.5f, 0.5f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 7f, 0.3f);
		base.bodyChunks[2] = new BodyChunk(this, 2, new Vector2(0f, 0f), 5f, 0.05f);
		bodyChunkConnections = new BodyChunkConnection[2];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 18f, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[2], 22f, BodyChunkConnection.Type.Pull, 0.8f, -1f);
		if (King)
		{
			abstractCreature.ignoreCycle = true;
			kingWaiting = true;
			armorPieces = 3;
		}
		jumpFinders = new List<JumpFinder>();
		readyToReleaseMask = base.dead;
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		connections = new List<MovementConnection>();
		climbOrientation = new IntVector2((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1), (!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
		_cons0 = new List<MovementConnection>(15);
		SetUpCombatSkills();
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new ScavengerGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		pastPositions = new List<IntVector2>();
		commitedToMove = new MovementConnection(MovementConnection.MovementType.Standard, new WorldCoordinate(-1, -1, -1, -1), new WorldCoordinate(-1, -1, -1, -1), 1);
		commitToMoveCounter = 0;
		drop = false;
		lookPoint = new Vector2(UnityEngine.Random.value * newRoom.PixelWidth, UnityEngine.Random.value * newRoom.PixelHeight);
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		if (placeRoom.abstractRoom.scavengerOutpost)
		{
			PlacedObject placedObject = null;
			int num = 0;
			while (placedObject == null && num < placeRoom.roomSettings.placedObjects.Count)
			{
				if (placeRoom.roomSettings.placedObjects[num].type == PlacedObject.Type.ScavengerOutpost)
				{
					placedObject = placeRoom.roomSettings.placedObjects[num];
				}
				num++;
			}
			if (placedObject != null)
			{
				List<IntVector2> list = new List<IntVector2>();
				for (int i = placeRoom.GetTilePosition(placedObject.pos).x - Mathf.RoundToInt((placedObject.data as PlacedObject.ResizableObjectData).Rad / 20f); i <= placeRoom.GetTilePosition(placedObject.pos).x + Mathf.RoundToInt((placedObject.data as PlacedObject.ResizableObjectData).Rad / 20f); i++)
				{
					for (int j = placeRoom.GetTilePosition(placedObject.pos).y - Mathf.RoundToInt((placedObject.data as PlacedObject.ResizableObjectData).Rad / 20f); j <= placeRoom.GetTilePosition(placedObject.pos).x + Mathf.RoundToInt((placedObject.data as PlacedObject.ResizableObjectData).Rad / 20f); j++)
					{
						if (placeRoom.aimap.TileAccessibleToCreature(new IntVector2(i, j), base.Template))
						{
							list.Add(new IntVector2(i, j));
						}
					}
				}
				if (list.Count > 0)
				{
					base.abstractCreature.pos.Tile = list[UnityEngine.Random.Range(0, list.Count)];
				}
			}
		}
		base.PlaceInRoom(placeRoom);
	}

	public override void Update(bool eu)
	{
		if (ModManager.MSC)
		{
			if (grabbedBy.Count > 0 && grabbedBy[0] != null && grabbedBy[0].grabber is Player && (grabbedBy[0].grabber as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				base.bodyChunks[0].mass = 0.05f;
				base.bodyChunks[1].mass = 0.05f;
				base.bodyChunks[2].mass = 0.01f;
			}
			else
			{
				float t = 0.6f;
				base.bodyChunks[0].mass = Mathf.Lerp(base.bodyChunks[0].mass, 0.5f, t);
				base.bodyChunks[1].mass = Mathf.Lerp(base.bodyChunks[1].mass, 0.3f, t);
				base.bodyChunks[2].mass = Mathf.Lerp(base.bodyChunks[2].mass, 0.05f, t);
			}
			if (King && myRobot == null && room != null)
			{
				myRobot = new AncientBot(base.mainBodyChunk.pos, new Color(0.945f, 0.3765f, 0f), this, online: true);
				room.AddObject(myRobot);
			}
			if (King)
			{
				base.abstractCreature.controlled = kingWaiting;
				freezeControls = kingWaiting;
			}
			if (kingWaiting && animation == null)
			{
				animation = new DirectAwayThroneAnimation(this, null, Vector2.zero);
			}
			explosionDamageCooldown--;
		}
		if (critLooker != null)
		{
			critLooker.Update();
		}
		else
		{
			critLooker = new CreatureLooker(this, AI.tracker, this, 2f / 3f, (int)Mathf.Lerp(30f, 140f, base.abstractCreature.personality.nervous));
		}
		timeSinceLastInputDir++;
		if (base.safariControlled && inputWithDiagonals.HasValue && (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0))
		{
			timeSinceLastInputDir = 0;
			lastInputDir = new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y);
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (UnityEngine.Random.value < Injured)
		{
			Stun((int)Mathf.Lerp(0f, 14f * UnityEngine.Random.value, Mathf.Pow(Injured, 0.5f)));
		}
		WeightedPush(2, 0, Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos), 0.3f * (1f - ReallyStuck));
		base.bodyChunks[2].collideWithTerrain = shortcutComingUp == default(MovementConnection) && room.VisualContact(base.mainBodyChunk.pos, base.bodyChunks[2].pos);
		base.Update(eu);
		if (!base.dead)
		{
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].vel += Custom.RNV() * base.Hypothermia;
			}
		}
		if (room == null)
		{
			return;
		}
		if (!room.GetTile(base.mainBodyChunk.pos).Solid)
		{
			lastNonSolidTile = room.GetTilePosition(base.mainBodyChunk.pos);
		}
		else
		{
			base.mainBodyChunk.HardSetPosition(room.MiddleOfTile(lastNonSolidTile));
		}
		if (Elite)
		{
			JumpLogicUpdate();
		}
		if (base.Consious)
		{
			Act();
		}
		else
		{
			kingWaiting = false;
			animation = null;
			shortcutComingUp = default(MovementConnection);
		}
		if (!base.Consious && !base.dead && base.stun < 35 && grabbedBy.Count > 0 && !(grabbedBy[0].grabber is Leech))
		{
			grabbedAttackCounter++;
			if (King && grabbedAttackCounter == 25)
			{
				MeleeGetFree(grabbedBy[0].grabber, eu);
				grabbedAttackCounter = 0;
			}
			else if ((grabbedAttackCounter == 25 || (Elite && grabbedAttackCounter % 75 == 0)) && UnityEngine.Random.value < Mathf.Pow(base.abstractCreature.personality.bravery * (base.State as HealthState).health, 0.4f) && grabbedBy[0].grabber.Template.type != CreatureTemplate.Type.RedLizard && grabbedBy[0].grabber.Template.type != CreatureTemplate.Type.KingVulture && (grabbedBy[0].grabber.Template.type != CreatureTemplate.Type.Vulture || UnityEngine.Random.value < 0.6f))
			{
				MeleeGetFree(grabbedBy[0].grabber, eu);
			}
		}
		else
		{
			grabbedAttackCounter = 0;
		}
		if (swingPos.HasValue)
		{
			float num = Vector2.Distance(base.mainBodyChunk.pos, swingPos.Value);
			if (num > 100f || (num > 70f && Custom.RotateAroundOrigo(base.mainBodyChunk.vel, 0f - Custom.AimFromOneVectorToAnother(base.mainBodyChunk.pos, swingPos.Value)).y < -55f) || (!base.Consious && UnityEngine.Random.value < 1f / 60f))
			{
				swingPos = null;
			}
			else if (num > swingRadius)
			{
				Vector2 vector = Custom.DirVec(swingPos.Value, base.mainBodyChunk.pos);
				base.mainBodyChunk.vel += vector * (swingRadius - num);
				base.mainBodyChunk.pos += vector * (swingRadius - num);
			}
		}
		for (int j = 0; j < base.bodyChunks.Length; j++)
		{
			base.bodyChunks[j].terrainSqueeze = 1f - LittleStuck;
		}
	}

	private void Act()
	{
		if (ModManager.MSC && animation != null && animation.id == MoreSlugcatsEnums.ScavengerAnimationID.PrepareToJump)
		{
			if (!animation.Continue)
			{
				animation = null;
			}
			else
			{
				animation.Update();
			}
			return;
		}
		AI.Update();
		if (animation != null)
		{
			if (!animation.Continue)
			{
				animation = null;
			}
			else
			{
				animation.Update();
			}
		}
		CombatUpdate();
		if (animation == null)
		{
			visionFactor = Mathf.Lerp(visionFactor, moving ? 0.6f : 0.75f, 1f);
		}
		else if (Rummaging)
		{
			visionFactor = Mathf.Lerp(visionFactor, 0.3f, 0.8f);
		}
		else if (animation is AttentiveAnimation)
		{
			visionFactor = Mathf.Lerp(visionFactor, 1f, 0.8f);
		}
		else
		{
			visionFactor = Mathf.Lerp(visionFactor, moving ? 0.6f : 0.75f, 0.8f);
		}
		if (narrowVision < 1f)
		{
			narrowVision = Mathf.Min(1f, narrowVision + 1f / Mathf.Lerp(40f, 10f, base.abstractCreature.personality.energy));
		}
		if (!ModManager.MSC || animation == null || animation.id != MoreSlugcatsEnums.ScavengerAnimationID.Jumping)
		{
			if (drop && base.mainBodyChunk.pos.y <= room.MiddleOfTile(commitedToMove.destinationCoord).y && base.mainBodyChunk.lastPos.y > room.MiddleOfTile(commitedToMove.destinationCoord).y && room.aimap.getAItile(commitedToMove.destinationCoord).acc == AItile.Accessibility.Climb)
			{
				swingPos = room.MiddleOfTile(commitedToMove.destinationCoord);
				swingRadius = 50f;
				movMode = MovementMode.Climb;
				commitToMoveCounter = 0;
				drop = false;
			}
			if (commitToMoveCounter < 1 && (!CommitedToMoveIsDrop || room.GetTilePosition(base.mainBodyChunk.pos).y < commitedToMove.destinationCoord.y) && !swingPos.HasValue && base.mainBodyChunk.vel.y < -5f && base.mainBodyChunk.lastPos.y > base.mainBodyChunk.pos.y)
			{
				for (int num = room.GetTilePosition(base.mainBodyChunk.lastPos).y; num >= room.GetTilePosition(base.mainBodyChunk.pos).y; num--)
				{
					if (room.aimap.getAItile(new IntVector2(room.GetTilePosition(base.mainBodyChunk.pos).x, num)).acc == AItile.Accessibility.Climb)
					{
						swingPos = room.MiddleOfTile(new IntVector2(room.GetTilePosition(base.mainBodyChunk.pos).x, num));
						swingRadius = 50f;
						movMode = MovementMode.Climb;
						drop = false;
						break;
					}
				}
			}
		}
		else
		{
			movMode = MovementMode.Run;
		}
		if (movMode == MovementMode.Run || movMode == MovementMode.StandStill)
		{
			WeightedPush(0, 1, new Vector2(0f, 1f), Custom.LerpMap(Vector2.Dot((base.bodyChunks[0].pos - base.bodyChunks[1].pos).normalized, new Vector2(0f, 1f)), -1f, 1f, 5.5f, 0.3f) * (1f - LittleStuck) * Mathf.Lerp(1f, 0.1f, Mathf.Pow(Injured, 2f)));
		}
		WeightedPush(0, 1, HeadLookDir, 0.05f * (1f - LittleStuck));
		Vector2 vector = base.mainBodyChunk.pos + HeadLookDir * bodyChunkConnections[1].distance * (Pointing ? 0.4f : 1f);
		if (shortcutComingUp != default(MovementConnection))
		{
			vector = room.MiddleOfTile(shortcutComingUp.startCoord);
		}
		float num2 = ((animation != null && animation is AttentiveAnimation) ? 12f : 5f);
		WeightedPush(2, 0, Vector2.ClampMagnitude(vector - base.bodyChunks[2].pos, num2), 0.8f / num2);
		GetUnstuckRoutine();
		MovementConnection movementConnection = default(MovementConnection);
		int num3 = -1;
		occupyTile = new IntVector2(-1, -1);
		bool flag = false;
		shortcutComingUp = default(MovementConnection);
		if (movMode != MovementMode.Climb)
		{
			swingPos = null;
			swingClimbCounter = 0;
		}
		if (commitToMoveCounter > 0)
		{
			commitToMoveCounter--;
			if (!drop)
			{
				bool flag2 = commitToMoveCounter > 0 && room.GetTilePosition(base.bodyChunks[commitedMoveFollowChunk].pos) != commitedToMove.DestTile;
				for (int i = 0; i < connections.Count; i++)
				{
					if (flag2)
					{
						break;
					}
					if (room.GetTilePosition(base.bodyChunks[commitedMoveFollowChunk].pos) != connections[i].DestTile)
					{
						flag2 = false;
					}
				}
				if (flag2)
				{
					occupyTile = commitedToMove.StartTile;
					movementConnection = commitedToMove;
					num3 = commitedMoveFollowChunk;
				}
				else
				{
					commitToMoveCounter = -5;
				}
			}
		}
		else
		{
			if (commitToMoveCounter < 0)
			{
				commitToMoveCounter++;
			}
			if (swingPos.HasValue)
			{
				occupyTile = room.GetTilePosition(swingPos.Value);
				movementConnection = FollowPath(room.GetWorldCoordinate(occupyTile), actuallyFollowingThisPath: true);
				num3 = 0;
			}
			for (int j = 0; j < 2; j++)
			{
				if (num3 >= 0)
				{
					break;
				}
				for (int k = 0; k < 5; k++)
				{
					if (num3 >= 0)
					{
						break;
					}
					if (room.aimap.TileAccessibleToCreature(base.bodyChunks[j].pos + Custom.fourDirectionsAndZero[k].ToVector2() * base.bodyChunks[j].rad, base.Template))
					{
						occupyTile = room.GetTilePosition(base.bodyChunks[j].pos + Custom.fourDirectionsAndZero[k].ToVector2() * base.bodyChunks[j].rad);
						movementConnection = FollowPath(room.GetWorldCoordinate(occupyTile), actuallyFollowingThisPath: true);
						num3 = j;
					}
				}
			}
			if (base.Submersion > 0f && (occupyTile.y < -100 || SwimTile(occupyTile)))
			{
				movMode = MovementMode.Swim;
				moveModeChangeCounter = 0;
			}
			connections.Clear();
		}
		moving = AI.pathFinder.GetDestination.room != base.abstractCreature.pos.room || room.GetTilePosition(base.mainBodyChunk.pos).FloatDist(AI.pathFinder.GetDestination.Tile) >= 3f || occupyTile.FloatDist(AI.pathFinder.GetDestination.Tile) >= 3f;
		if (!moving && !SharedPhysics.RayTraceTilesForTerrain(room, room.GetTilePosition(base.mainBodyChunk.pos), AI.pathFinder.GetDestination.Tile))
		{
			moving = true;
		}
		if (moving && occupyTile.FloatDist(AI.pathFinder.GetEffectualDestination.Tile) < 3f && AI.agitation < 0.5f && !base.safariControlled)
		{
			moving = false;
		}
		if (animation != null && animation is AttentiveAnimation && (animation as AttentiveAnimation).stop && !base.safariControlled)
		{
			moving = false;
		}
		if (ModManager.MSC && animation != null && animation.id == MoreSlugcatsEnums.ScavengerAnimationID.PrepareToJump)
		{
			moving = false;
		}
		if (AI.scared > 0.8f)
		{
			moving = true;
		}
		if (base.safariControlled && (!inputWithDiagonals.HasValue || (inputWithDiagonals.Value.x == 0 && inputWithDiagonals.Value.y == 0)))
		{
			moving = false;
		}
		if (base.safariControlled && inputWithDiagonals.HasValue && inputWithDiagonals.Value.jmp)
		{
			moving = false;
		}
		if (ModManager.MSC && animation != null && animation.id == MoreSlugcatsEnums.ScavengerAnimationID.Jumping)
		{
			moving = true;
		}
		if (drop)
		{
			if (movementConnection != default(MovementConnection))
			{
				footingCounter++;
				if (footingCounter > 10 && commitToMoveCounter == 0)
				{
					drop = false;
				}
			}
			else
			{
				footingCounter = 0;
				if (room.GetTilePosition(base.mainBodyChunk.pos).y < commitedToMove.destinationCoord.y + 2)
				{
					commitToMoveCounter = 0;
				}
			}
			movementConnection = default(MovementConnection);
			num3 = -1;
			occupyTile = new IntVector2(-1, -1);
		}
		if (ReallyStuck > 0f)
		{
			for (int l = 0; l < 3; l++)
			{
				base.bodyChunks[l].vel += Custom.RNV() * UnityEngine.Random.value * 3f * Mathf.Pow(ReallyStuck, 3f);
			}
		}
		if (movementConnection != default(MovementConnection))
		{
			if (commitToMoveCounter < 1)
			{
				MovementConnection movementConnection2 = movementConnection;
				climbingUpComing = false;
				for (int m = 0; m < 5; m++)
				{
					if (!flag && ((room.GetTile(movementConnection2.destinationCoord + new IntVector2(0, 1)).Solid && room.GetTile(movementConnection2.destinationCoord + new IntVector2(0, -1)).Solid) || (room.GetTile(movementConnection2.destinationCoord + new IntVector2(1, 0)).Solid && room.GetTile(movementConnection2.destinationCoord + new IntVector2(-1, 0)).Solid)))
					{
						flag = true;
					}
					if (!climbingUpComing && moving && room.aimap.getAItile(movementConnection2.destinationCoord).acc == AItile.Accessibility.Climb)
					{
						climbingUpComing = true;
					}
					connections.Add(movementConnection2);
					if (shortcutComingUp == default(MovementConnection) && movementConnection2.type == MovementConnection.MovementType.ShortCut)
					{
						shortcutComingUp = movementConnection2;
					}
					movementConnection2 = FollowPath(movementConnection2.destinationCoord, actuallyFollowingThisPath: false);
					for (int n = 0; n < connections.Count; n++)
					{
						if (!(movementConnection2 != default(MovementConnection)))
						{
							break;
						}
						if (connections[n].destinationCoord == movementConnection2.destinationCoord)
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
			if (commitToMoveCounter < 1 && AllowIdleMoves)
			{
				for (int num4 = 0; num4 < 3; num4++)
				{
					base.bodyChunks[num4].vel += Custom.DirVec(base.bodyChunks[num4].pos, room.MiddleOfTile(movementConnection.destinationCoord)) * LittleStuck * (1f - ReallyStuck);
				}
			}
			if (animation != null && animation.id == ScavengerAnimation.ID.ThrowCharge && animation.Active && connections.Count > 0 && (room.MiddleOfTile(connections[connections.Count - 1].destinationCoord).x < base.mainBodyChunk.pos.x != (animation as ThrowChargeAnimation).UseTarget.x < base.mainBodyChunk.pos.x || (movementConnection.destinationCoord.x != movementConnection.startCoord.x && movementConnection.destinationCoord.x < movementConnection.startCoord.x != (animation as ThrowChargeAnimation).UseTarget.x < base.mainBodyChunk.pos.x)))
			{
				moving = false;
			}
			if (!base.safariControlled && moving && AI.behavior == ScavengerAI.Behavior.Idle && AI.discomfortTracker.DiscomfortOfTile(room.GetWorldCoordinate(NextTile)) > AI.discomfortTracker.DiscomfortOfTile(room.GetWorldCoordinate(occupyTile)))
			{
				moving = false;
			}
			bodyChunkConnections[0].type = ((movMode == MovementMode.Crawl || LittleStuck > 0.5f) ? BodyChunkConnection.Type.Pull : BodyChunkConnection.Type.Normal);
			MovementMode movementMode = MovementMode.Run;
			if (flag)
			{
				movementMode = MovementMode.Crawl;
			}
			if (room.aimap.getAItile(occupyTile).acc == AItile.Accessibility.Climb && room.aimap.getAItile(movementConnection.destinationCoord).acc == AItile.Accessibility.Climb && (!ModManager.MSC || animation == null || animation.id != MoreSlugcatsEnums.ScavengerAnimationID.Jumping || base.mainBodyChunk.vel.y < 0f))
			{
				movementMode = MovementMode.Climb;
			}
			if (movementMode == MovementMode.Run && !moving)
			{
				movementMode = MovementMode.StandStill;
			}
			if (movMode == MovementMode.Swim)
			{
				if (!SwimTile(movementConnection.DestTile))
				{
					moveModeChangeCounter = 10;
				}
				else
				{
					for (int num5 = 0; num5 < Math.Min(5, connections.Count); num5++)
					{
						if (Custom.DistLess(base.mainBodyChunk.pos, room.MiddleOfTile(connections[num5].destinationCoord), 50f) && !SwimTile(connections[num5].DestTile))
						{
							moveModeChangeCounter = 10;
							break;
						}
					}
				}
			}
			if (movementMode != movMode)
			{
				moveModeChangeCounter++;
				if (moveModeChangeCounter > 10)
				{
					movMode = movementMode;
				}
			}
			else
			{
				moveModeChangeCounter = 0;
			}
			if (!swingPos.HasValue && movMode != MovementMode.Swim)
			{
				for (int num6 = 0; num6 < 3; num6++)
				{
					base.bodyChunks[num6].vel.y += base.gravity;
				}
				if (!ModManager.MSC || animation == null || animation.id != MoreSlugcatsEnums.ScavengerAnimationID.Jumping)
				{
					base.bodyChunks[0].vel *= 0.9f;
					base.bodyChunks[1].vel *= 0.8f;
					base.bodyChunks[2].vel *= 0.8f;
				}
			}
			else
			{
				base.bodyChunks[2].vel *= 0.8f;
				base.bodyChunks[2].vel.y += base.gravity;
			}
			if ((double)ReallyStuck < 0.5 && (movMode == MovementMode.Run || movMode == MovementMode.StandStill) && room.aimap.getAItile(movementConnection.startCoord).acc == AItile.Accessibility.Floor && (movementConnection.startCoord.y == movementConnection.destinationCoord.y || movMode == MovementMode.StandStill) && (!ModManager.MSC || animation == null || animation.id != MoreSlugcatsEnums.ScavengerAnimationID.Jumping))
			{
				base.bodyChunks[1].vel.y *= 0.5f;
				base.bodyChunks[1].vel.y += (room.MiddleOfTile(occupyTile).y - base.bodyChunks[1].pos.y) * 0.1f;
			}
			if (movementConnection.IsDrop)
			{
				commitedToMove = movementConnection;
				commitToMoveCounter = 20;
				commitedMoveFollowChunk = num3;
				drop = true;
				swingPos = null;
				nextSwingPos = null;
			}
			else
			{
				if (shortcutDelay < 1 && !base.safariControlled && !King && (movementConnection.type == MovementConnection.MovementType.ShortCut || movementConnection.type == MovementConnection.MovementType.NPCTransportation || (movementConnection.type == MovementConnection.MovementType.RegionTransportation && (float)AI.age > 100f * Mathf.InverseLerp(0.1f, 0f, (base.abstractCreature.abstractAI as ScavengerAbstractAI).Shyness))))
				{
					enteringShortCut = movementConnection.StartTile;
					if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
					{
						NPCTransportationDestination = movementConnection.destinationCoord;
					}
					else if (room.shortcutData(movementConnection.StartTile).shortCutType == ShortcutData.Type.RegionTransportation)
					{
						NPCTransportationDestination = AI.pathFinder.BestRegionTransportationGoal();
					}
					shortcutDelay = 100;
					return;
				}
				if (commitToMoveCounter == 0 && movementConnection.type != 0)
				{
					bool flag3 = true;
					if (movementConnection.type == MovementConnection.MovementType.ReachDown && room.GetTile(movementConnection.DestTile + new IntVector2(0, -1)).Solid)
					{
						flag3 = false;
					}
					if (flag3)
					{
						commitedToMove = movementConnection;
						commitToMoveCounter = 20;
						commitedMoveFollowChunk = num3;
					}
				}
			}
			if (movementConnection.type != 0 && room.GetTilePosition(base.bodyChunks[1].pos) != movementConnection.DestTile && room.GetTilePosition(base.bodyChunks[1].pos).FloatDist(movementConnection.DestTile + IntVector2.ClampAtOne(movementConnection.StartTile - movementConnection.DestTile)) < 3f && AllowIdleMoves)
			{
				base.bodyChunks[1].vel += Custom.DirVec(base.bodyChunks[1].pos, room.MiddleOfTile(movementConnection.DestTile + IntVector2.ClampAtOne(movementConnection.StartTile - movementConnection.DestTile)));
			}
			if (movMode == MovementMode.Run)
			{
				base.GoThroughFloors = false;
				if (!ModManager.MSC || animation == null || animation.id != MoreSlugcatsEnums.ScavengerAnimationID.Jumping)
				{
					for (int num7 = 0; num7 < 2; num7++)
					{
						Vector2 vector2 = Custom.DirVec(room.MiddleOfTile(movementConnection.startCoord), room.MiddleOfTile(movementConnection.destinationCoord));
						bool flag4 = false;
						if (num7 == 0)
						{
							int num8 = connections.Count - 1;
							while (num8 >= 0 && !flag4)
							{
								if (room.GetTilePosition(base.bodyChunks[num7].pos).FloatDist(connections[num8].DestTile + new IntVector2(0, 1)) < 5f && room.VisualContact(base.bodyChunks[num7].pos, room.MiddleOfTile(connections[num8].DestTile + new IntVector2(0, 1))))
								{
									vector2 = Vector2.ClampMagnitude(room.MiddleOfTile(connections[num8].DestTile + new IntVector2(0, 1)) - base.bodyChunks[num7].pos, 5f) / 5f;
									flag4 = true;
								}
								num8--;
							}
						}
						int num9 = connections.Count - 1;
						while (num9 >= 0 && !flag4)
						{
							if (room.GetTilePosition(base.bodyChunks[num7].pos).FloatDist(connections[num9].DestTile) < 5f && room.VisualContact(base.bodyChunks[0].pos, room.MiddleOfTile(connections[num9].DestTile)) && room.VisualContact(base.bodyChunks[1].pos, room.MiddleOfTile(connections[num9].DestTile)))
							{
								vector2 = Vector2.ClampMagnitude(room.MiddleOfTile(connections[num9].DestTile) - base.bodyChunks[num7].pos, 5f) / 5f;
								flag4 = true;
							}
							num9--;
						}
						if (!base.GoThroughFloors)
						{
							base.GoThroughFloors = vector2.y < 0f;
						}
						if ((vector2.y < -0.35f && num7 == 0) || (vector2.y > 0.35f && num7 == 1))
						{
							vector2.y *= 0.5f;
						}
						base.bodyChunks[num7].vel += vector2 * 0.8f * MovementSpeed;
					}
					if (knucklePos.HasValue && base.bodyChunks[0].ContactPoint.x == 0 && base.bodyChunks[0].ContactPoint.y == 0 && base.bodyChunks[1].ContactPoint.x == 0 && base.bodyChunks[1].ContactPoint.y > -1)
					{
						float t = Mathf.InverseLerp(40f, 10f, (knucklePos.Value.x - base.mainBodyChunk.pos.x) * Mathf.Sign(flip));
						WeightedPush(1, 0, new Vector2(flip * Mathf.Lerp(-1f, 1f, t), 0f), Custom.LerpMap((base.bodyChunks[0].vel.x + base.bodyChunks[1].vel.x) / 2f * Mathf.Sign(flip), 0f, 5f, 0.5f, 3.5f));
					}
				}
			}
			else if (movMode == MovementMode.Crawl)
			{
				Vector2 vector3 = Custom.DirVec(base.bodyChunks[num3].pos, room.MiddleOfTile(movementConnection.destinationCoord));
				if (LittleStuck < 0.5f)
				{
					for (int num10 = connections.Count - 1; num10 >= 0; num10--)
					{
						if (room.GetTilePosition(base.bodyChunks[num3].pos).FloatDist(connections[num10].DestTile) < 2f && room.VisualContact(base.bodyChunks[num3].pos, room.MiddleOfTile(connections[num10].DestTile)))
						{
							vector3 = Vector2.ClampMagnitude(room.MiddleOfTile(connections[num10].DestTile) - base.bodyChunks[num3].pos, 5f) / 5f;
							break;
						}
					}
				}
				base.bodyChunks[num3].vel += vector3 * 1.5f * MovementSpeed;
				base.GoThroughFloors = vector3.y < 0f;
				knucklePos = room.MiddleOfTile(connections[Math.Min(connections.Count - 1, 3)].destinationCoord);
				flip = Mathf.Lerp(flip, Mathf.Clamp(connections[connections.Count - 1].destinationCoord.x - movementConnection.startCoord.x, -0.5f, 0.5f), 0.1f);
			}
			else if (movMode == MovementMode.Climb)
			{
				if (swingingForbidden < 1 && TileLegalForSwinging(occupyTile) && TileLegalForSwinging(connections[connections.Count - 1].DestTile) && movementConnection.startCoord.y == movementConnection.destinationCoord.y && Math.Abs(occupyTile.y - connections[connections.Count - 1].destinationCoord.y) < 2)
				{
					swingClimbCounter = Math.Min(swingClimbCounter + 1, 40);
				}
				else
				{
					swingClimbCounter = Math.Max(swingClimbCounter - 1, 0);
				}
				if (swingPos.HasValue)
				{
					if (ReallyStuck > 0.1f)
					{
						swingingForbidden = 200;
					}
					if (swingClimbCounter > 10)
					{
						Swing();
					}
					else
					{
						swingRadius = Mathf.Max(swingRadius - 4f, 10f);
						base.mainBodyChunk.vel *= 0.8f;
						if (AllowIdleMoves)
						{
							base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, swingPos.Value) * 1.4f;
						}
						if (Custom.DistLess(base.mainBodyChunk.pos, swingPos.Value, 5f) || room.aimap.getAItile(base.mainBodyChunk.pos).acc == AItile.Accessibility.Climb)
						{
							swingPos = null;
							nextSwingPos = null;
							swingClimbCounter = 0;
						}
					}
				}
				else if (swingClimbCounter < 30 && connections.Count > 0)
				{
					TightClimbing(movementConnection, flag);
				}
				else
				{
					swingPos = room.MiddleOfTile(occupyTile);
					swingRadius = 50f;
					swingClimbCounter = 40;
				}
				if (swingingForbidden > 0)
				{
					swingingForbidden--;
				}
			}
			else if (movMode == MovementMode.StandStill)
			{
				if (occupyTile.FloatDist(AI.pathFinder.GetDestination.Tile) < 5f && AllowIdleMoves)
				{
					if (room.aimap.getAItile(occupyTile).acc == AItile.Accessibility.Climb && room.aimap.getAItile(base.bodyChunks[1].pos).acc != AItile.Accessibility.Floor)
					{
						base.mainBodyChunk.vel += Vector2.ClampMagnitude(room.MiddleOfTile(AI.pathFinder.GetDestination) - base.mainBodyChunk.pos, 10f) / 10f;
					}
					else
					{
						base.bodyChunks[1].vel += Vector2.ClampMagnitude(room.MiddleOfTile(AI.pathFinder.GetDestination) - base.bodyChunks[1].pos, 10f) / 10f;
					}
				}
			}
			else if (movMode == MovementMode.Swim)
			{
				if (moving)
				{
					Vector2 p = room.MiddleOfTile(movementConnection.destinationCoord);
					if (Mathf.Abs(base.mainBodyChunk.vel.x) > 5f)
					{
						base.mainBodyChunk.vel.x *= 0.8f;
					}
					if (SwimTile(movementConnection.DestTile))
					{
						p.y = room.FloatWaterLevel(p.x);
						base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, p) * 0.25f;
					}
					else
					{
						base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, p) * 1.25f;
					}
					if (Mathf.Abs(base.mainBodyChunk.pos.x - p.x) < 5f)
					{
						flip *= 0.9f;
					}
					else if (base.mainBodyChunk.pos.x < p.x)
					{
						flip = Mathf.Min(flip + 0.07f, 1f);
					}
					else
					{
						flip = Mathf.Max(flip - 0.07f, -1f);
					}
					base.bodyChunks[2].vel *= 0.9f;
					base.bodyChunks[2].vel += Vector2.Lerp(Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(connections[connections.Count - 1].destinationCoord)), new Vector2(0f, 1f), base.bodyChunks[2].submersion) * 2f;
				}
				else
				{
					if (AllowIdleMoves)
					{
						base.mainBodyChunk.vel += Vector2.ClampMagnitude(room.MiddleOfTile(AI.pathFinder.GetDestination) - base.mainBodyChunk.pos, 10f) / 60f;
					}
					flip = Mathf.Lerp(flip, Mathf.Clamp(HeadLookDir.x * 1.5f, -1f, 1f), 0.1f);
				}
			}
			if (ReallyStuck < 0.5f && moving && movMode != MovementMode.StandStill && movMode != MovementMode.Swim && moving)
			{
				Vector2 vector4 = Custom.DirVec(room.MiddleOfTile(movementConnection.startCoord), room.MiddleOfTile(movementConnection.destinationCoord + new IntVector2(0, 1)));
				for (int num11 = connections.Count - 1; num11 >= 0; num11--)
				{
					if (connections[num11].DestTile.FloatDist(occupyTile) > 3f && connections[num11].DestTile.FloatDist(occupyTile) < 7f)
					{
						vector4 = Custom.DirVec(base.bodyChunks[2].pos, room.MiddleOfTile(connections[num11].destinationCoord + new IntVector2(0, 1)));
						break;
					}
				}
				base.bodyChunks[2].vel += vector4 * Custom.LerpMap(Vector2.Dot(vector4, Custom.DirVec(base.mainBodyChunk.pos, base.bodyChunks[2].pos)), 1f, -1f, 1f, 4f, 1.5f);
				for (int num12 = 0; num12 < 3; num12++)
				{
					if (movementConnection.startCoord.x != movementConnection.destinationCoord.x && base.bodyChunks[num12].ContactPoint.x == Custom.IntClamp(movementConnection.destinationCoord.x - movementConnection.startCoord.x, -1, 1) && AllowIdleMoves)
					{
						base.bodyChunks[num12].vel.y += Mathf.Clamp(room.MiddleOfTile(movementConnection.startCoord).y - base.bodyChunks[num12].pos.y, -5f, 5f) * 0.1f;
					}
					else if (movementConnection.startCoord.y != movementConnection.destinationCoord.y && base.bodyChunks[num12].ContactPoint.y == Custom.IntClamp(movementConnection.destinationCoord.y - movementConnection.startCoord.y, -1, 1) && AllowIdleMoves)
					{
						base.bodyChunks[num12].vel.x += Mathf.Clamp(room.MiddleOfTile(movementConnection.startCoord).x - base.bodyChunks[num12].pos.x, -5f, 5f) * 0.1f;
					}
				}
			}
		}
		else if (drop)
		{
			if (Mathf.Abs(base.mainBodyChunk.vel.x) < 5f)
			{
				base.mainBodyChunk.vel.x *= 0.8f;
				if (AllowIdleMoves)
				{
					base.mainBodyChunk.vel.x += Mathf.Clamp((room.MiddleOfTile(commitedToMove.destinationCoord).x - base.mainBodyChunk.pos.x) / 3f, -4f, 4f);
				}
			}
			if (room.GetTilePosition(base.bodyChunks[1].pos).y >= commitedToMove.startCoord.y && Math.Abs(room.GetTilePosition(base.bodyChunks[1].pos).x - commitedToMove.startCoord.x) == 1 && AllowIdleMoves)
			{
				base.bodyChunks[1].vel.x += Mathf.Clamp((room.MiddleOfTile(commitedToMove.startCoord).x - base.bodyChunks[1].pos.x) / 3f, -4f, 4f);
			}
		}
		if (movMode == MovementMode.Run)
		{
			if (connections.Count > 2 || (base.safariControlled && connections.Count > 0))
			{
				if (room.MiddleOfTile(connections[connections.Count - 1].destinationCoord).x > base.mainBodyChunk.pos.x)
				{
					flip = Mathf.Min(Mathf.Lerp(flip, 1f, 0.1f) + 0.05f, 1f);
				}
				else
				{
					flip = Mathf.Max(Mathf.Lerp(flip, -1f, 0.1f) - 0.05f, -1f);
				}
			}
			else
			{
				flip *= 0.8f;
			}
			if (!knucklePos.HasValue)
			{
				if (connections.Count > 0)
				{
					int num13 = Math.Min(4, connections.Count - 1);
					while (num13 >= 0 && !knucklePos.HasValue)
					{
						Vector2 vector5 = room.MiddleOfTile(connections[num13].destinationCoord + new IntVector2(0, -1));
						vector5 += Custom.DirVec(base.mainBodyChunk.pos, vector5) * 40f;
						Vector2? testPos = SharedPhysics.ExactTerrainRayTracePos(room, base.mainBodyChunk.pos, vector5);
						if (KnucklePosLegal(testPos))
						{
							if (base.graphicsModule != null && !Custom.DistLess(testPos.Value, (base.graphicsModule as ScavengerGraphics).lastKnuckleSoundPos, 16f) && !Custom.DistLess(testPos.Value, (base.graphicsModule as ScavengerGraphics).lastLastKnuckleSoundPos, 16f))
							{
								if (room.GetTile(testPos.Value + new Vector2(0f, -10f)).Solid)
								{
									room.PlaySound(((base.graphicsModule as ScavengerGraphics).spearSound && base.graphicsModule != null && (base.graphicsModule as ScavengerGraphics).hands[0].spearPosAdd.magnitude() > 1f) ? SoundID.Scavenger_Spear_Blunt_Hit_Ground : SoundID.Scavenger_Knuckle_Hit_Ground, testPos.Value);
								}
								else
								{
									room.PlaySound(SoundID.Scavenger_Grab_Terrain, testPos.Value);
								}
								(base.graphicsModule as ScavengerGraphics).lastLastKnuckleSoundPos = (base.graphicsModule as ScavengerGraphics).lastKnuckleSoundPos;
								(base.graphicsModule as ScavengerGraphics).lastKnuckleSoundPos = testPos.Value;
								(base.graphicsModule as ScavengerGraphics).spearSound = !(base.graphicsModule as ScavengerGraphics).spearSound;
							}
							knucklePos = testPos;
						}
						num13--;
					}
				}
			}
			else if (!KnucklePosLegal(knucklePos))
			{
				knucklePos = null;
			}
		}
		else if (movMode == MovementMode.Climb)
		{
			if (drop)
			{
				swingPos = null;
			}
		}
		else if (movMode == MovementMode.StandStill)
		{
			flip = Mathf.Lerp(flip, Mathf.Clamp(HeadLookDir.x * 10f, -1f, 1f), 0.1f);
		}
		else if (movMode == MovementMode.Swim)
		{
			float num14 = Mathf.Lerp(room.FloatWaterLevel(base.mainBodyChunk.pos.x), room.waterObject.fWaterLevel, 0.5f);
			base.bodyChunks[1].vel.y -= base.gravity * (1f - base.bodyChunks[2].submersion);
			base.bodyChunks[2].vel *= Mathf.Lerp(1f, 0.9f, base.mainBodyChunk.submersion);
			base.mainBodyChunk.vel.y *= Mathf.Lerp(1f, 0.95f, base.mainBodyChunk.submersion);
			base.bodyChunks[1].vel.y *= Mathf.Lerp(1f, 0.95f, base.bodyChunks[1].submersion);
			base.mainBodyChunk.vel.y += Mathf.Clamp((num14 - base.mainBodyChunk.pos.y) / 14f, -0.5f, 0.5f);
			base.bodyChunks[1].vel.y += Mathf.Clamp((num14 - bodyChunkConnections[0].distance - base.bodyChunks[1].pos.y) / 14f, -0.5f, 0.5f);
		}
		if (notFollowingPathToCurrentGoalCounter < 200 && AI.pathFinder.GetEffectualDestination != AI.pathFinder.GetDestination)
		{
			notFollowingPathToCurrentGoalCounter++;
		}
		else if (notFollowingPathToCurrentGoalCounter > 0)
		{
			notFollowingPathToCurrentGoalCounter--;
		}
		int num15 = 0;
		if (moving || notFollowingPathToCurrentGoalCounter > 100)
		{
			pastPositions.Insert(0, base.abstractCreature.pos.Tile);
			if (pastPositions.Count > 60)
			{
				pastPositions.RemoveAt(pastPositions.Count - 1);
			}
			for (int num16 = 30; num16 < pastPositions.Count; num16++)
			{
				if (Custom.DistLess(base.abstractCreature.pos.Tile, pastPositions[num16], 4f))
				{
					num15++;
				}
			}
		}
		if (num15 > 20)
		{
			stuckCounter++;
		}
		else
		{
			stuckCounter -= 4;
		}
		stuckCounter = Custom.IntClamp(stuckCounter, 0, 200);
		if ((base.abstractCreature.abstractAI as ScavengerAbstractAI).Shyness > 0f && (base.abstractCreature.abstractAI as ScavengerAbstractAI).GhostOutOfCurrentRoom && AI.pathFinder.GetDestination.room != room.abstractRoom.index && !room.ViewedByAnyCamera(base.mainBodyChunk.pos, 200f))
		{
			ghostCounter++;
			if (moving && (float)ghostCounter > Mathf.Lerp(600f, 40f, (base.abstractCreature.abstractAI as ScavengerAbstractAI).Shyness) && movementConnection != default(MovementConnection) && movementConnection.type != MovementConnection.MovementType.ShortCut && movementConnection.destinationCoord.TileDefined)
			{
				for (int num17 = 0; num17 < base.bodyChunks.Length; num17++)
				{
					base.bodyChunks[num17].HardSetPosition(room.MiddleOfTile(movementConnection.destinationCoord));
					base.bodyChunks[num17].vel *= 0f;
				}
			}
		}
		else
		{
			ghostCounter = 0;
		}
	}

	private bool SwimTile(IntVector2 tl)
	{
		if (room.GetTile(tl).DeepWater)
		{
			return true;
		}
		if (room.GetTile(tl).AnyWater)
		{
			return !room.GetTile(tl + new IntVector2(0, -1)).Solid;
		}
		return false;
	}

	private MovementConnection FollowPath(WorldCoordinate origin, bool actuallyFollowingThisPath)
	{
		if (pathingWithExits)
		{
			return AI.pathFinder.PathWithExits(origin, avoidForbiddenEntrance: false);
		}
		return (AI.pathFinder as StandardPather).FollowPath(origin, actuallyFollowingThisPath);
	}

	private void Swing()
	{
		if (!nextSwingPos.HasValue)
		{
			for (int num = connections.Count - 1; num >= 1; num--)
			{
				if (TileViableForNextSwingPos(connections[num].DestTile))
				{
					nextSwingPos = room.MiddleOfTile(connections[num].DestTile);
					break;
				}
			}
		}
		else if (!TileViableForNextSwingPos(room.GetTilePosition(nextSwingPos.Value)))
		{
			nextSwingPos = null;
		}
		else
		{
			bool flag = false;
			for (int i = 0; i < connections.Count; i++)
			{
				if (flag)
				{
					break;
				}
				if (room.GetTilePosition(nextSwingPos.Value) == connections[i].DestTile)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				nextSwingPos = null;
			}
		}
		if (swingRadius < 50f)
		{
			swingRadius = Mathf.Min(swingRadius + 0.5f + 4f * Mathf.InverseLerp(0.5f, 1f, swingProgress), 50f);
		}
		else
		{
			swingRadius = Mathf.Max(swingRadius - 0.5f - 4f * Mathf.InverseLerp(0.5f, 1f, swingProgress), 50f);
		}
		if (nextSwingPos.HasValue)
		{
			swingProgress = Mathf.Clamp(swingProgress + 0.0125f, 0f, 1f);
			if (AllowIdleMoves)
			{
				base.mainBodyChunk.vel += Custom.PerpendicularVector(base.mainBodyChunk.pos, swingPos.Value) * 1.2f * Mathf.Sign(nextSwingPos.Value.x - base.mainBodyChunk.pos.x) * Mathf.Sin(Mathf.InverseLerp(0.25f, 1f, swingProgress) * (float)Math.PI);
				base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, nextSwingPos.Value) * 1.2f * Mathf.Pow(Mathf.InverseLerp(0.75f, 1f, swingProgress), 2f);
			}
			if (moving)
			{
				flip = Mathf.Lerp(flip, Mathf.Clamp(nextSwingPos.Value.x - base.mainBodyChunk.pos.x, -1f, 1f), 0.1f);
			}
			if (swingProgress > 0.1f && Custom.DistLess(base.mainBodyChunk.pos, nextSwingPos.Value, 50f))
			{
				swingArm = 1 - swingArm;
				swingRadius = Vector2.Distance(base.mainBodyChunk.pos, nextSwingPos.Value);
				swingPos = nextSwingPos;
				room.PlaySound(SoundID.Scavenger_Grab_Pole, swingPos.Value);
				nextSwingPos = null;
				swingProgress = 0f;
			}
		}
		if (!moving)
		{
			flip = Mathf.Lerp(flip, Mathf.Clamp(HeadLookDir.x * 10f, -1f, 1f), 0.1f);
		}
		if (Mathf.Abs(base.mainBodyChunk.vel.x) < 3f)
		{
			for (int j = 0; j < Math.Min(connections.Count, 3); j++)
			{
				if (connections[j].IsDrop)
				{
					commitedToMove = connections[j];
					commitToMoveCounter = 20;
					drop = true;
					swingPos = null;
					nextSwingPos = null;
					break;
				}
			}
		}
		if (swingPos.HasValue && !Custom.DistLess(base.mainBodyChunk.pos, swingPos.Value, 100f))
		{
			swingPos = null;
		}
	}

	private void TightClimbing(MovementConnection followConnection, bool anyNarrow)
	{
		base.mainBodyChunk.vel *= 0.8f;
		if (connections[connections.Count - 1].destinationCoord.x != followConnection.startCoord.x)
		{
			climbOrientation.x = Custom.IntClamp(connections[connections.Count - 1].destinationCoord.x - followConnection.startCoord.x, -1, 1);
		}
		if (connections[connections.Count - 1].destinationCoord.y != followConnection.startCoord.y)
		{
			climbOrientation.y = Custom.IntClamp(connections[connections.Count - 1].destinationCoord.y - followConnection.startCoord.y, -1, 1);
		}
		Vector2 vector = climbOrientation.ToVector2() * 8f;
		if (followConnection.startCoord.x == followConnection.destinationCoord.x)
		{
			vector.y *= 0f;
		}
		if (followConnection.startCoord.y == followConnection.destinationCoord.y)
		{
			vector.x *= 0f;
		}
		if (!moving)
		{
			base.mainBodyChunk.vel += Vector2.ClampMagnitude(room.MiddleOfTile(AI.pathFinder.GetDestination) - base.mainBodyChunk.pos, 10f) / 10f;
		}
		else
		{
			if (anyNarrow)
			{
				vector *= 0f;
			}
			base.mainBodyChunk.vel += Vector2.ClampMagnitude(room.MiddleOfTile(followConnection.destinationCoord) + vector * 2f - base.mainBodyChunk.pos, 9f) / 10f * MovementSpeed;
			base.bodyChunks[1].vel += Vector2.ClampMagnitude(room.MiddleOfTile(followConnection.startCoord) + vector * 0.8f - base.bodyChunks[1].pos, 9f) / 10f * MovementSpeed;
		}
		for (int i = 0; i < 2; i++)
		{
			if (followConnection.startCoord.x == followConnection.destinationCoord.x && Mathf.Abs(base.bodyChunks[i].vel.x) < 4f && base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y)
			{
				base.bodyChunks[i].vel.x += Mathf.Clamp(room.MiddleOfTile(followConnection.destinationCoord).x + vector.x * ((i == 0) ? 2f : 0.8f) - base.bodyChunks[i].pos.x, -5f, 5f) * 0.05f;
			}
			else if (followConnection.startCoord.y == followConnection.destinationCoord.y && Mathf.Abs(base.bodyChunks[i].vel.y) < 4f)
			{
				base.bodyChunks[i].vel.y += Mathf.Clamp(room.MiddleOfTile(followConnection.destinationCoord).y + vector.y * ((i == 0) ? 2f : 0.8f) - base.bodyChunks[i].pos.y, -5f, 5f) * 0.05f;
			}
		}
		if (followConnection.startCoord.x == followConnection.destinationCoord.x)
		{
			flip = Mathf.Lerp(flip, Custom.LerpMap(room.MiddleOfTile(followConnection.destinationCoord).x - base.bodyChunks[0].pos.x, -20f, 20f, -1f, 1f) * (float)Custom.IntClamp(followConnection.destinationCoord.y - followConnection.startCoord.y, -1, 1), 0.1f);
		}
		if (followConnection.startCoord.y == followConnection.destinationCoord.y)
		{
			flip = Mathf.Lerp(flip, Custom.LerpMap(room.MiddleOfTile(followConnection.destinationCoord).y - base.bodyChunks[0].pos.y, -6f, 6f, -1f, 1f) * (float)Custom.IntClamp(followConnection.startCoord.x - followConnection.destinationCoord.x, -1, 1), 0.1f);
		}
	}

	private bool TileLegalForSwinging(IntVector2 testPos)
	{
		AItile aItile = room.aimap.getAItile(testPos);
		if (aItile.acc != AItile.Accessibility.Climb)
		{
			return false;
		}
		if (room.aimap.getTerrainProximity(testPos) < 3)
		{
			return false;
		}
		if (aItile.smoothedFloorAltitude > -1 && aItile.smoothedFloorAltitude < 4)
		{
			return false;
		}
		if (!room.VisualContact(base.mainBodyChunk.pos, room.MiddleOfTile(testPos)))
		{
			return false;
		}
		return true;
	}

	private bool TileViableForNextSwingPos(IntVector2 testPos)
	{
		if ((float)testPos.y > swingPos.Value.y + 1f)
		{
			return false;
		}
		float num = Vector2.Distance(room.MiddleOfTile(swingPos.Value), room.MiddleOfTile(testPos));
		if (num < 5f || num > 60f)
		{
			return false;
		}
		if (!room.VisualContact(base.mainBodyChunk.pos, room.MiddleOfTile(testPos)))
		{
			return false;
		}
		return true;
	}

	private bool KnucklePosLegal(Vector2? testPos)
	{
		if (!testPos.HasValue)
		{
			return false;
		}
		return Custom.DistLess(base.mainBodyChunk.pos, testPos.Value, Custom.LerpMap((testPos.Value.x - base.mainBodyChunk.pos.x) * flip, 0f, 40f, 20f, 150f));
	}

	public override void NewTile()
	{
		base.NewTile();
		if (!base.Consious)
		{
			return;
		}
		if (!base.safariControlled)
		{
			LookForItemsToPickUp();
		}
		for (int i = 0; i < AI.tracker.CreaturesCount; i++)
		{
			if (AI.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger && AI.tracker.GetRep(i).VisualContact && AI.tracker.GetRep(i).representedCreature.realizedCreature != null && AI.tracker.GetRep(i).representedCreature.realizedCreature.Consious && Custom.DistLess(base.mainBodyChunk.pos, AI.tracker.GetRep(i).representedCreature.realizedCreature.mainBodyChunk.pos, 60f))
			{
				AI.PackMemberEncounter(AI.tracker.GetRep(i).representedCreature.realizedCreature as Scavenger);
				break;
			}
		}
	}

	public void LookForItemsToPickUp()
	{
		bool flag = false;
		for (int i = 0; i < base.grasps.Length; i++)
		{
			if (flag)
			{
				break;
			}
			if (base.grasps[i] == null)
			{
				flag = true;
			}
		}
		if (base.safariControlled)
		{
			bool flag2 = false;
			for (int j = 0; j < room.physicalObjects.Length; j++)
			{
				for (int k = 0; k < room.physicalObjects[j].Count; k++)
				{
					AbstractPhysicalObject abstractPhysicalObject = room.physicalObjects[j][k].abstractPhysicalObject;
					if (abstractPhysicalObject.realizedObject != null && !(abstractPhysicalObject is AbstractCreature) && Custom.DistLess(base.mainBodyChunk.pos, abstractPhysicalObject.realizedObject.firstChunk.pos, 50f) && room.VisualContact(base.mainBodyChunk.pos, abstractPhysicalObject.realizedObject.firstChunk.pos) && abstractPhysicalObject.realizedObject.grabbedBy.Count < 1 && (!(abstractPhysicalObject is AbstractSpear) || !(abstractPhysicalObject as AbstractSpear).stuckInWall) && (!(abstractPhysicalObject.realizedObject is Weapon) || (abstractPhysicalObject.realizedObject as Weapon).mode != Weapon.Mode.Thrown))
					{
						while (abstractPhysicalObject.realizedObject.grabbedBy.Count > 0)
						{
							abstractPhysicalObject.realizedObject.grabbedBy[0].Release();
						}
						abstractPhysicalObject.realizedObject.abstractPhysicalObject.LoseAllStuckObjects();
						PickUpAndPlaceInInventory(abstractPhysicalObject.realizedObject);
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					break;
				}
			}
		}
		for (int l = 0; l < AI.itemTracker.ItemCount; l++)
		{
			if (AI.itemTracker.GetRep(l).representedItem.realizedObject != null && Custom.DistLess(base.mainBodyChunk.pos, AI.itemTracker.GetRep(l).representedItem.realizedObject.firstChunk.pos, 50f) && (AI.itemTracker.GetRep(l).VisualContact || AI.scavengeCandidate == AI.itemTracker.GetRep(l) || room.VisualContact(base.mainBodyChunk.pos, AI.itemTracker.GetRep(l).representedItem.realizedObject.firstChunk.pos)) && AI.CollectScore(AI.itemTracker.GetRep(l), weaponFiltered: true) > 0 && (flag || AI.scavengeCandidate == null || AI.CollectScore(AI.itemTracker.GetRep(l), weaponFiltered: true) >= AI.CollectScore(AI.scavengeCandidate, weaponFiltered: true)) && AI.itemTracker.GetRep(l).representedItem.realizedObject.grabbedBy.Count == 0 && (!(AI.itemTracker.GetRep(l).representedItem is AbstractSpear) || !(AI.itemTracker.GetRep(l).representedItem as AbstractSpear).stuckInWall) && (!ModManager.MMF || !MMF.cfgHunterBackspearProtect.Value || !(AI.itemTracker.GetRep(l).representedItem.realizedObject is Spear) || !(AI.itemTracker.GetRep(l).representedItem.realizedObject as Spear).onPlayerBack) && (!(AI.itemTracker.GetRep(l).representedItem.realizedObject is Weapon) || (AI.itemTracker.GetRep(l).representedItem.realizedObject as Weapon).mode != Weapon.Mode.Thrown))
			{
				while (AI.itemTracker.GetRep(l).representedItem.realizedObject.grabbedBy.Count > 0)
				{
					AI.itemTracker.GetRep(l).representedItem.realizedObject.grabbedBy[0].Release();
				}
				AI.itemTracker.GetRep(l).representedItem.realizedObject.abstractPhysicalObject.LoseAllStuckObjects();
				PickUpAndPlaceInInventory(AI.itemTracker.GetRep(l).representedItem.realizedObject);
				break;
			}
		}
	}

	public void GiveWeaponToOther(Scavenger other)
	{
		if (base.safariControlled)
		{
			return;
		}
		PhysicalObject physicalObject = null;
		for (int num = base.grasps.Length - 1; num >= 0; num--)
		{
			if (base.grasps[num] != null && AI.WeaponScore(base.grasps[num].grabbed, pickupDropInsteadOfWeaponSelection: false) > 2)
			{
				physicalObject = base.grasps[num].grabbed;
				ReleaseGrasp(num);
				break;
			}
		}
		if (physicalObject != null)
		{
			other.PickUpAndPlaceInInventory(physicalObject);
		}
	}

	public void PickUpAndPlaceInInventory(PhysicalObject obj)
	{
		if (obj == null)
		{
			return;
		}
		if (obj.room == null || room == null || obj.room != room)
		{
			AI.itemTracker.RepresentationForObject(obj, AddIfMissing: false)?.Destroy();
			return;
		}
		int num = -1;
		for (int i = 0; i < base.grasps.Length; i++)
		{
			if (base.grasps[i] == null)
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			int num2 = int.MaxValue;
			for (int j = 0; j < base.grasps.Length; j++)
			{
				int num3 = AI.CollectScore(base.grasps[j].grabbed, AI.currentViolenceType == ScavengerAI.ViolenceType.Lethal);
				if (num3 < num2)
				{
					num = j;
					num2 = num3;
				}
			}
			if (num2 >= AI.CollectScore(obj, AI.currentViolenceType == ScavengerAI.ViolenceType.Lethal) && obj.abstractPhysicalObject != AI.giftForMe && !base.safariControlled)
			{
				return;
			}
			Custom.Log($"drop object {base.grasps[num].grabbed.abstractPhysicalObject.type} ({num2}) for object {obj.abstractPhysicalObject.type} ({AI.CollectScore(obj, AI.currentViolenceType == ScavengerAI.ViolenceType.Lethal)})");
			if (num > -1)
			{
				room.socialEventRecognizer.CreaturePutItemOnGround(base.grasps[num].grabbed, this);
				ReleaseGrasp(num);
			}
		}
		if (num > -1)
		{
			AI.arrangeInventoryCounter = Math.Min(AI.arrangeInventoryCounter, UnityEngine.Random.Range(20, 60));
			Custom.Log($"picked up {obj.abstractPhysicalObject.type} {obj.abstractPhysicalObject.ID} {AI.CollectScore(obj, weaponFiltered: true)}");
			MoveItemBetweenGrasps(obj, -1, num);
			PlaceAllGrabbedObjectsInCorrectContainers();
			if (obj.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
			{
				(base.abstractCreature.abstractAI as ScavengerAbstractAI).bringPearlHome = true;
			}
		}
	}

	private void MoveItemBetweenGrasps(PhysicalObject obj, int fromGrasp, int toGrasp)
	{
		if (base.graphicsModule != null)
		{
			PlacedGrabbedObjectInCorrectContainer(obj, toGrasp);
		}
		if (fromGrasp > -1)
		{
			ReleaseGrasp(fromGrasp);
		}
		Grab(obj, toGrasp, 0, Grasp.Shareability.CanNotShare, 0.5f, overrideEquallyDominant: true, pacifying: true);
	}

	public void PlaceAllGrabbedObjectsInCorrectContainers()
	{
		for (int i = 0; i < base.grasps.Length; i++)
		{
			if (base.grasps[i] != null)
			{
				if (base.grasps[i].grabbed == null)
				{
					ReleaseGrasp(i);
				}
				else
				{
					PlacedGrabbedObjectInCorrectContainer(base.grasps[i].grabbed, i);
				}
			}
		}
	}

	private void PlacedGrabbedObjectInCorrectContainer(PhysicalObject obj, int grasp)
	{
		IDrawable drawable = null;
		if (obj is IDrawable)
		{
			drawable = obj as IDrawable;
		}
		else if (obj.graphicsModule != null)
		{
			drawable = obj.graphicsModule;
		}
		if (base.graphicsModule != null && drawable != null)
		{
			base.graphicsModule.ReleaseSpecificInternallyContainedObjectSprites(drawable);
			base.graphicsModule.AddObjectToInternalContainer(drawable, (base.graphicsModule as ScavengerGraphics).ContainerForHeldItem(obj, grasp));
		}
	}

	public bool ArrangeInventory()
	{
		if (animation != null && animation.id == ScavengerAnimation.ID.Throw && animation.id != ScavengerAnimation.ID.ThrowCharge)
		{
			return false;
		}
		PhysicalObject physicalObject = null;
		if (base.grasps[0] != null)
		{
			physicalObject = base.grasps[0].grabbed;
		}
		bool flag = false;
		for (int i = 0; i < base.grasps.Length; i++)
		{
			if (base.grasps[i] != null && AI.CollectScore(base.grasps[i].grabbed, weaponFiltered: true) < 1 && AI.CollectScore(base.grasps[i].grabbed, weaponFiltered: false) < 1)
			{
				room.socialEventRecognizer.CreaturePutItemOnGround(base.grasps[i].grabbed, this);
				ReleaseGrasp(i);
				return true;
			}
		}
		if (AI.HoldWeapon)
		{
			if ((AI.currentViolenceType == ScavengerAI.ViolenceType.ForFun || AI.currentViolenceType == ScavengerAI.ViolenceType.NonLethal) && (physicalObject == null || !(physicalObject is Rock)))
			{
				for (int j = 0; j < base.grasps.Length; j++)
				{
					if (flag)
					{
						break;
					}
					if (base.grasps[j] != null && base.grasps[j].grabbed is Rock)
					{
						SwitchGrasps(0, j);
						if (base.graphicsModule != null)
						{
							(base.graphicsModule as ScavengerGraphics).hands[0].pos = base.grasps[0].grabbed.firstChunk.pos;
							(base.graphicsModule as ScavengerGraphics).hands[0].mode = Limb.Mode.Dangle;
						}
						flag = true;
					}
				}
			}
			if (!flag)
			{
				int num = 0;
				if (physicalObject != null)
				{
					num = AI.WeaponScore(physicalObject, pickupDropInsteadOfWeaponSelection: false);
				}
				int num2 = num;
				int num3 = 0;
				for (int k = 1; k < base.grasps.Length; k++)
				{
					if (base.grasps[k] != null && AI.WeaponScore(base.grasps[k].grabbed, pickupDropInsteadOfWeaponSelection: false) > num2)
					{
						num2 = AI.WeaponScore(base.grasps[k].grabbed, pickupDropInsteadOfWeaponSelection: false);
						num3 = k;
					}
				}
				if (num3 > 0)
				{
					SwitchGrasps(0, num3);
					if (base.graphicsModule != null)
					{
						(base.graphicsModule as ScavengerGraphics).hands[0].pos = base.grasps[0].grabbed.firstChunk.pos;
						(base.graphicsModule as ScavengerGraphics).hands[0].mode = Limb.Mode.Dangle;
					}
					flag = true;
				}
			}
		}
		else if (physicalObject != null)
		{
			for (int l = 1; l < base.grasps.Length; l++)
			{
				if (base.grasps[l] == null)
				{
					SwitchGrasps(0, l);
					if (base.graphicsModule != null)
					{
						(base.graphicsModule as ScavengerGraphics).hands[0].pos = (base.graphicsModule as ScavengerGraphics).ItemPosition(l);
						(base.graphicsModule as ScavengerGraphics).hands[0].mode = Limb.Mode.Dangle;
					}
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			PlaceAllGrabbedObjectsInCorrectContainers();
		}
		return flag;
	}

	public override bool Grab(PhysicalObject obj, int graspUsed, int chunkGrabbed, Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
	{
		if (base.graphicsModule != null)
		{
			(base.graphicsModule as ScavengerGraphics).hands[(graspUsed != 0) ? 1 : 0].pos = obj.bodyChunks[chunkGrabbed].pos;
			(base.graphicsModule as ScavengerGraphics).hands[(graspUsed != 0) ? 1 : 0].mode = Limb.Mode.Dangle;
		}
		room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, base.mainBodyChunk);
		(base.abstractCreature.abstractAI as ScavengerAbstractAI).UpdateMissionAppropriateGear();
		return base.Grab(obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
	}

	public override void RecreateSticksFromAbstract()
	{
		for (int i = 0; i < base.abstractCreature.stuckObjects.Count; i++)
		{
			if (base.abstractCreature.stuckObjects[i] is AbstractPhysicalObject.CreatureGripStick && base.abstractCreature.stuckObjects[i].A == base.abstractCreature && base.abstractCreature.stuckObjects[i].B.realizedObject != null)
			{
				AbstractPhysicalObject.CreatureGripStick creatureGripStick = base.abstractCreature.stuckObjects[i] as AbstractPhysicalObject.CreatureGripStick;
				base.grasps[creatureGripStick.grasp] = new Grasp(this, creatureGripStick.B.realizedObject, creatureGripStick.grasp, UnityEngine.Random.Range(0, creatureGripStick.B.realizedObject.bodyChunks.Length), Grasp.Shareability.CanNotShare, 0.5f, pacifying: true);
				creatureGripStick.B.realizedObject.Grabbed(base.grasps[creatureGripStick.grasp]);
			}
		}
	}

	public override void ReleaseGrasp(int grasp)
	{
		if (base.grasps[grasp] != null && (!(base.grasps[grasp].grabbed is Weapon) || (base.grasps[grasp].grabbed as Weapon).mode != Weapon.Mode.Thrown))
		{
			if (grasp > 0 && base.graphicsModule != null)
			{
				(base.graphicsModule as ScavengerGraphics).hands[1].pos = (base.graphicsModule as ScavengerGraphics).ItemPosition(grasp);
				(base.graphicsModule as ScavengerGraphics).hands[1].mode = Limb.Mode.Dangle;
			}
			base.grasps[grasp].grabbedChunk.vel = base.mainBodyChunk.vel + Custom.DegToVec(Mathf.Lerp(-70f, 70f, UnityEngine.Random.value)) * 8.2f;
		}
		base.ReleaseGrasp(grasp);
	}

	public override void GraphicsModuleUpdated(bool actuallyViewed, bool eu)
	{
		for (int i = 0; i < base.grasps.Length; i++)
		{
			if (base.grasps[i] == null)
			{
				continue;
			}
			PhysicalObject grabbed = base.grasps[i].grabbed;
			if (base.graphicsModule == null)
			{
				grabbed.firstChunk.MoveFromOutsideMyUpdate(eu, base.mainBodyChunk.pos);
				grabbed.firstChunk.vel *= 0f;
				continue;
			}
			Vector2 vector = (base.graphicsModule as ScavengerGraphics).ItemPosition(i);
			if (grabbed is Spear || i == 0)
			{
				grabbed.firstChunk.MoveFromOutsideMyUpdate(eu, vector);
				grabbed.firstChunk.vel *= 0f;
			}
			else
			{
				grabbed.firstChunk.vel += Custom.DirVec(vector, grabbed.firstChunk.pos) * (5f - Vector2.Distance(vector, grabbed.firstChunk.pos));
				grabbed.firstChunk.pos += Custom.DirVec(vector, grabbed.firstChunk.pos) * (5f - Vector2.Distance(vector, grabbed.firstChunk.pos));
			}
			if (grabbed is Weapon)
			{
				(grabbed as Weapon).setRotation = (base.graphicsModule as ScavengerGraphics).ItemDirection(i);
			}
		}
	}

	private void LookForAndMoveToUnstuckTile()
	{
		IntVector2 pos = room.GetTilePosition(base.mainBodyChunk.pos);
		int num = 0;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				if (!room.aimap.TileAccessibleToCreature(room.GetTilePosition(base.bodyChunks[i].pos) + Custom.eightDirections[j], base.Template))
				{
					continue;
				}
				IntVector2 intVector = room.GetTilePosition(base.bodyChunks[i].pos) + Custom.eightDirections[j];
				MovementConnection movementConnection = FollowPath(room.GetWorldCoordinate(intVector), actuallyFollowingThisPath: true);
				_cons0.Clear();
				if (movementConnection != default(MovementConnection))
				{
					for (int k = 0; k < 15; k++)
					{
						_cons0.Add(movementConnection);
						movementConnection = FollowPath(movementConnection.destinationCoord, actuallyFollowingThisPath: false);
						for (int l = 0; l < _cons0.Count; l++)
						{
							if (!(movementConnection != default(MovementConnection)))
							{
								break;
							}
							if (_cons0[l].destinationCoord == movementConnection.destinationCoord)
							{
								movementConnection = default(MovementConnection);
							}
						}
						if (movementConnection == default(MovementConnection))
						{
							break;
						}
					}
				}
				if (_cons0.Count > num)
				{
					num = _cons0.Count;
					pos = intVector;
				}
			}
		}
		if (num > 2)
		{
			for (int m = 0; m < 3; m++)
			{
				base.bodyChunks[m].vel += Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(pos)) * (1f + ReallyStuck * UnityEngine.Random.value);
			}
		}
	}

	private void GetUnstuckRoutine()
	{
		bool flag = false;
		if (ReallyStuck > 0.1f && ReallyStuck < 0.9f && connections.Count < 3)
		{
			float angle = base.bodyChunks[0].vel.GetAngle();
			float angle2 = base.bodyChunks[1].vel.GetAngle();
			float angle3 = base.bodyChunks[2].vel.GetAngle();
			LookForAndMoveToUnstuckTile();
			flag = base.bodyChunks[0].vel.GetAngle() != angle || base.bodyChunks[1].vel.GetAngle() != angle2 || base.bodyChunks[2].vel.GetAngle() != angle3;
		}
		if (pathingWithExits)
		{
			if (pathWithExitsCounter < 0)
			{
				pathWithExitsCounter++;
			}
			if ((pathWithExitsCounter > -1 && stuckCounter == 0) || ReallyStuck >= 1f)
			{
				pathingWithExits = false;
				pathWithExitsCounter = 200;
			}
		}
		else if (ReallyStuck > 0.5f && pathWithExitsCounter < 1)
		{
			pathingWithExits = true;
			pathWithExitsCounter = -50;
			AI.pathFinder.InitiAccessibilityMapping(room.GetWorldCoordinate(occupyTile), null);
		}
		else if (pathWithExitsCounter > 0)
		{
			pathWithExitsCounter--;
		}
		if (ReallyStuck > 0.2f && (!ModManager.MMF || !flag))
		{
			ShortcutData? shortcutData = null;
			for (int i = 0; i < 3; i++)
			{
				if (shortcutData.HasValue)
				{
					break;
				}
				for (int j = 0; j < 9; j++)
				{
					if (shortcutData.HasValue)
					{
						break;
					}
					if (room.GetTile(room.GetTilePosition(base.bodyChunks[i].pos) + Custom.eightDirectionsAndZero[j]).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
					{
						shortcutData = room.shortcutData(room.GetTilePosition(base.bodyChunks[i].pos) + Custom.eightDirectionsAndZero[j]);
					}
				}
			}
			if (shortcutData.HasValue)
			{
				stuckOnShortcutCounter = Math.Min(stuckOnShortcutCounter + 1, 100);
				if (stuckOnShortcutCounter > 25)
				{
					for (int k = 0; k < base.bodyChunks.Length; k++)
					{
						base.bodyChunks[k].vel += room.ShorcutEntranceHoleDirection(shortcutData.Value.StartTile).ToVector2() * 7f + Custom.RNV() * UnityEngine.Random.value * 5f;
					}
				}
			}
		}
		else if (stuckOnShortcutCounter > 0)
		{
			stuckOnShortcutCounter--;
		}
		if (stuckOnShortcutCounter > 25)
		{
			for (int l = 0; l < base.bodyChunks.Length; l++)
			{
				base.bodyChunks[l].vel += Custom.RNV() * 6f * UnityEngine.Random.value;
			}
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!base.Consious)
		{
			return;
		}
		if (Rummaging)
		{
			animation = null;
		}
		if (otherObject is Scavenger)
		{
			if (!(otherObject as Scavenger).moving && !moving)
			{
				return;
			}
			for (int i = 0; i < 3; i++)
			{
				if ((otherObject as Scavenger).abstractCreature.personality.dominance > base.abstractCreature.personality.dominance || ((otherObject as Scavenger).moving && !moving))
				{
					if (base.bodyChunks[i].vel.y > -3f)
					{
						base.bodyChunks[i].vel.y -= (0.5f + (otherObject as Scavenger).LittleStuck) * ((movMode == MovementMode.Climb) ? 0.5f : 1f);
					}
				}
				else if (base.bodyChunks[i].vel.y < 5f)
				{
					base.bodyChunks[i].vel.y += (2.5f + LittleStuck) * ((movMode == MovementMode.Climb) ? 0.5f : 1f);
				}
			}
		}
		else if (otherObject is Creature && !(otherObject is Fly))
		{
			kingWaiting = false;
			AI.tracker.SeeCreature((otherObject as Creature).abstractCreature);
			Tracker.CreatureRepresentation creatureRepresentation = AI.tracker.RepresentationForObject(otherObject, AddIfMissing: false);
			if (creatureRepresentation != null)
			{
				(creatureRepresentation.dynamicRelationship.state as ScavengerAI.ScavengerTrackState).bumpedByThisCreature++;
				CollideWithOtherCreature(otherObject.bodyChunks[otherChunk], creatureRepresentation);
			}
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
	}

	public override Color ShortCutColor()
	{
		return new Color(1f, 1f, 1f);
	}

	public void HeavyStun(int st)
	{
		if (King && st > 80)
		{
			st = 80;
		}
		if (UnityEngine.Random.value < 0.5f && st > 15)
		{
			ReleaseGrasp(0);
		}
		base.Stun(st);
	}

	public override void Stun(int st)
	{
		if (King)
		{
			st = (int)((float)st * 0.5f);
			if (st > 40)
			{
				st = 40;
			}
		}
		if (UnityEngine.Random.value < 0.5f && st > 15)
		{
			ReleaseGrasp(0);
		}
		base.Stun(st);
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 5f;
		}
		base.bodyChunks[0].pos = newRoom.MiddleOfTile(pos) + newRoom.ShorcutEntranceHoleDirection(pos).ToVector2() * 15f;
		base.bodyChunks[1].pos = newRoom.MiddleOfTile(pos) + newRoom.ShorcutEntranceHoleDirection(pos).ToVector2() * 5f;
		base.bodyChunks[2].pos = newRoom.MiddleOfTile(pos) + newRoom.ShorcutEntranceHoleDirection(pos).ToVector2() * 25f;
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
		PlaceAllGrabbedObjectsInCorrectContainers();
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		if (room == null)
		{
			return;
		}
		if (ModManager.MSC && armorPieces > 0)
		{
			if ((type == DamageType.Stab || type == DamageType.Explosion) && explosionDamageCooldown <= 0 && !kingWaiting)
			{
				room.PlaySound(SoundID.Spear_Fragment_Bounce, base.firstChunk.pos);
				armorPieces--;
				Vector2 vector = Vector2.zero;
				if (directionAndMomentum.HasValue)
				{
					vector = directionAndMomentum.Value;
				}
				CentipedeShell obj = new CentipedeShell(hitChunk.pos, vector * Mathf.Lerp(0.7f, 1.6f, UnityEngine.Random.value) + Custom.RNV() * UnityEngine.Random.value * 3f, (base.graphicsModule as ScavengerGraphics).shells[0].hue, (base.graphicsModule as ScavengerGraphics).shells[0].saturation, (base.graphicsModule as ScavengerGraphics).shells[armorPieces].scaleX, (base.graphicsModule as ScavengerGraphics).shells[armorPieces].scaleY);
				room.AddObject(obj);
			}
			else
			{
				room.PlaySound(SoundID.Lizard_Head_Shield_Deflect, base.mainBodyChunk);
				float num = damage / base.Template.baseDamageResistance;
				int num2 = (int)Mathf.Min(Mathf.Max((damage * 30f + stunBonus) / base.Template.baseStunResistance / 2f, (int)(num * 30f)), 25f);
				for (int i = 0; i < num2; i++)
				{
					room.AddObject(new Spark(base.firstChunk.pos + Custom.DegToVec(UnityEngine.Random.value * 360f) * 5f * UnityEngine.Random.value, base.firstChunk.vel * -0.1f + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.2f, 0.4f, UnityEngine.Random.value) * base.firstChunk.vel.magnitude, new Color(1f, 1f, 1f), null, 10, 170));
				}
			}
			explosionDamageCooldown = 40;
			stunBonus = 0f;
			damage = 0f;
			type = DamageType.Blunt;
		}
		kingWaiting = false;
		if (hitChunk.index == 2)
		{
			damage = ((!King) ? (damage * 10f) : (damage * 2f));
		}
		if (Elite)
		{
			stunBonus *= 0.85f;
		}
		base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage * ((hitChunk.index == 2) ? 1.3f : 1f), stunBonus);
		if (hitChunk.index == 2 && damage > 0.5f && (base.State as HealthState).health < 0f)
		{
			Die();
		}
		if (Elite && base.dead && !readyToReleaseMask && room.world != null)
		{
			readyToReleaseMask = true;
			VultureMask.AbstractVultureMask abstractVultureMask = new VultureMask.AbstractVultureMask(room.world, null, room.GetWorldCoordinate(base.firstChunk.pos), room.game.GetNewID(), base.abstractCreature.ID.RandomSeed, king: false, King, (base.graphicsModule as ScavengerGraphics).maskGfx.overrideSprite);
			room.abstractRoom.AddEntity(abstractVultureMask);
			abstractVultureMask.RealizeInRoom();
			(abstractVultureMask.realizedObject as VultureMask).rotVel = new Vector2(20f, 0f);
			if (directionAndMomentum.HasValue)
			{
				(abstractVultureMask.realizedObject as VultureMask).firstChunk.vel = directionAndMomentum.Value.normalized * 20f;
			}
			else
			{
				(abstractVultureMask.realizedObject as VultureMask).firstChunk.vel = Custom.RNV() * 20f;
			}
		}
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		return score;
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		if (AI.agitation > 0.2f)
		{
			return AI.focusCreature;
		}
		return null;
	}

	public void LookAtNothing()
	{
	}

	public override void GrabbedObjectSnatched(PhysicalObject grabbedObject, Creature thief)
	{
		if (grabbedObject != null && thief != null)
		{
			base.GrabbedObjectSnatched(grabbedObject, thief);
			room.socialEventRecognizer.Theft(grabbedObject, thief, this);
			if (Elite)
			{
				AI.agitation = 1f;
			}
		}
	}

	public void ControlCycleInventory()
	{
		if (animation != null && animation.id == ScavengerAnimation.ID.Throw && animation.id != ScavengerAnimation.ID.ThrowCharge)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < base.grasps.Length - 1; i++)
		{
			if (base.grasps[i] != null || base.grasps[i + 1] != null)
			{
				flag = true;
				SwitchGrasps(i, i + 1);
			}
		}
		if (base.grasps[0] == null)
		{
			for (int j = 1; j < base.grasps.Length; j++)
			{
				if (base.grasps[j] != null)
				{
					SwitchGrasps(j, 0);
					break;
				}
			}
		}
		if (flag && base.grasps[0] != null && base.graphicsModule != null)
		{
			(base.graphicsModule as ScavengerGraphics).hands[0].pos = base.grasps[0].grabbed.firstChunk.pos;
			(base.graphicsModule as ScavengerGraphics).hands[0].mode = Limb.Mode.Dangle;
		}
		if (flag)
		{
			PlaceAllGrabbedObjectsInCorrectContainers();
		}
	}

	public void CycleItemIntoPrimaryHand()
	{
		for (int i = 0; i < base.grasps.Length; i++)
		{
			if (base.grasps[0] != null)
			{
				break;
			}
			ControlCycleInventory();
		}
	}

	public bool FastReactionCheck()
	{
		if (!ModManager.MSC)
		{
			ReactionCheck();
		}
		fastReflexBuildUp = Mathf.Min(1f, fastReflexBuildUp + Mathf.Lerp(1f / 120f, 0.5f, Mathf.Pow(reactionSkill, 3.5f)));
		return fastReflexBuildUp >= 1f;
	}

	public bool PlayerHasImmunity(Player player)
	{
		if (player.scavengerImmunity <= 0)
		{
			if (player.room != null && !player.inShortcut && room != null && !base.inShortcut && !King && room.abstractRoom.name == "LC_FINAL")
			{
				return player.mainBodyChunk.pos.x > 1000f;
			}
			return false;
		}
		return true;
	}

	public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos onAppendagePos, Vector2 direction)
	{
		if (armorPieces > 0)
		{
			return false;
		}
		return base.SpearStick(source, dmg, chunk, onAppendagePos, direction);
	}

	private void JumpLogicUpdate()
	{
		if (base.dead)
		{
			return;
		}
		if (!base.Consious && animation != null && (animation.id == MoreSlugcatsEnums.ScavengerAnimationID.PrepareToJump || animation.id == MoreSlugcatsEnums.ScavengerAnimationID.Jumping))
		{
			animation = null;
		}
		for (int num = jumpFinders.Count - 1; num >= 0; num--)
		{
			if (jumpFinders[num].slatedForDeletion)
			{
				jumpFinders.RemoveAt(num);
			}
			else if (base.safariControlled)
			{
				jumpFinders[num].Destroy();
			}
			else
			{
				jumpFinders[num].Update();
			}
		}
		if (base.safariControlled)
		{
			if (controlledJumpFinder != null && controlledJumpFinder.startPos != base.abstractCreature.pos.Tile)
			{
				controlledJumpFinder.Destroy();
				controlledJumpFinder = null;
			}
			if (controlledJumpFinder == null)
			{
				controlledJumpFinder = new JumpFinder(room, this, base.abstractCreature.pos.Tile);
			}
			controlledJumpFinder.Update();
			controlledJumpFinder.fade = 0;
			if (inputWithDiagonals.HasValue && inputWithDiagonals.Value.jmp && !lastInputWithDiagonals.Value.jmp && !inputWithDiagonals.Value.pckp && controlledJumpFinder.bestJump != null)
			{
				InitiateJump(controlledJumpFinder);
			}
		}
		else if (controlledJumpFinder != null)
		{
			controlledJumpFinder.Destroy();
			controlledJumpFinder = null;
		}
		if (animation != null && animation.id == MoreSlugcatsEnums.ScavengerAnimationID.Jumping)
		{
			JumpingUpdate();
		}
		else if (InStandardRunMode)
		{
			RunningUpdate();
		}
		if (actOnJump != null && (animation == null || (animation.id != MoreSlugcatsEnums.ScavengerAnimationID.PrepareToJump && animation.id != MoreSlugcatsEnums.ScavengerAnimationID.Jumping)))
		{
			actOnJump.fade++;
			if (actOnJump.fade > 40)
			{
				actOnJump = null;
			}
		}
	}

	public void JumpingUpdate()
	{
		jumpCounter++;
		bool flag = false;
		if (actOnJump == null)
		{
			flag = true;
		}
		else
		{
			if (jumpCounter > actOnJump.bestJump.tick + 5)
			{
				for (int i = 0; i < base.bodyChunks.Length; i++)
				{
					if (room.aimap.TileAccessibleToCreature(base.bodyChunks[i].pos, base.Template))
					{
						base.bodyChunks[i].vel *= 0.5f;
					}
				}
			}
			else if (jumpCounter >= 5)
			{
				actOnJump = null;
			}
			if (actOnJump != null)
			{
				for (int j = 0; j < base.bodyChunks.Length; j++)
				{
					if (Custom.DistLess(base.bodyChunks[j].pos, jumpToPoint, 40f))
					{
						flag = true;
						break;
					}
				}
			}
		}
		if (!flag && (actOnJump == null || jumpCounter < actOnJump.bestJump.tick + 20))
		{
			return;
		}
		if (actOnJump != null && actOnJump.bestJump.grabWhenLanding)
		{
			for (int k = 0; k < base.bodyChunks.Length; k++)
			{
				base.bodyChunks[k].vel *= 0.5f;
			}
		}
		actOnJump = null;
		animation = null;
	}

	public void RunningUpdate()
	{
		if (!base.safariControlled && (movMode == MovementMode.Run || (movMode == MovementMode.Climb && !swingPos.HasValue)))
		{
			for (int num = jumpFinders.Count - 1; num >= 0; num--)
			{
				if (!jumpFinders[num].slatedForDeletion && base.abstractCreature.pos.Tile == jumpFinders[num].startPos && jumpFinders[num].BeneficialMovement)
				{
					InitiateJump(jumpFinders[num]);
					break;
				}
			}
		}
		WorldCoordinate worldCoordinate = base.abstractCreature.pos;
		_cachedList.Clear();
		bool flag = false;
		for (int i = 0; i < 5; i++)
		{
			MovementConnection movementConnection = FollowPath(worldCoordinate, actuallyFollowingThisPath: false);
			if (movementConnection == default(MovementConnection))
			{
				break;
			}
			worldCoordinate = movementConnection.destinationCoord;
			_cachedList.Add(worldCoordinate);
			for (int j = 0; j < jumpFinders.Count; j++)
			{
				if (jumpFinders[j].startPos == worldCoordinate.Tile)
				{
					jumpFinders[j].fade = 0;
					flag = true;
				}
			}
		}
		if (AI.pathFinder.PathingCellAtWorldCoordinate(base.abstractCreature.pos) != AI.pathFinder.fallbackPathingCell)
		{
			jumpCell = AI.pathFinder.PathingCellAtWorldCoordinate(base.abstractCreature.pos);
		}
		if (addDelay > 0)
		{
			addDelay--;
		}
		else if (jumpFinders.Count < (flag ? 1 : 4) && _cachedList.Count > 1)
		{
			WorldCoordinate pos = _cachedList[UnityEngine.Random.Range(0, _cachedList.Count)];
			if (pos.TileDefined && pos.Tile.FloatDist(base.abstractCreature.pos.Tile) > 2f && !room.aimap.getAItile(pos).narrowSpace && JumpFinder.PathWeightComparison(jumpCell, AI.pathFinder.PathingCellAtWorldCoordinate(pos)))
			{
				jumpFinders.Add(new JumpFinder(room, this, pos.Tile));
			}
		}
	}

	public void InitiateJump(JumpFinder jump)
	{
		if (jump != null && jump.bestJump != null && jump.bestJump.goalCell != null)
		{
			actOnJump = jump;
			int animationLength = 30;
			animation = new PrepareToJumpAnimation(this, animationLength);
			jumpCounter = 0;
			for (int i = 0; i < jumpFinders.Count; i++)
			{
				jumpFinders[i].Destroy();
			}
		}
	}

	public void Jump()
	{
		if (actOnJump != null && actOnJump.bestJump != null)
		{
			movMode = MovementMode.Run;
			moveModeChangeCounter = 0;
			drop = false;
			swingPos = null;
			nextSwingPos = null;
			climbingUpComing = false;
			commitToMoveCounter = 0;
			Vector2 vector = room.MiddleOfTile(actOnJump.startPos);
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].pos = Vector2.Lerp(base.bodyChunks[i].pos, vector + actOnJump.bestJump.initVel.normalized * (1 - i) * 8f, 1f);
			}
			Vector2 vector2 = vector + actOnJump.bestJump.initVel - base.bodyChunks[0].pos;
			for (int j = 0; j < base.bodyChunks.Length; j++)
			{
				base.bodyChunks[j].pos += vector2;
				base.bodyChunks[j].vel = new Vector2(Mathf.Clamp(actOnJump.bestJump.initVel.x, -22f, 22f), Mathf.Min(actOnJump.bestJump.initVel.y, 17f));
			}
			jumpCounter = 0;
			addDelay = 20;
			if (actOnJump.bestJump.tick > 25 && Vector2.Dot((base.bodyChunks[0].pos - base.bodyChunks[2].pos).normalized, actOnJump.bestJump.initVel.normalized) < Mathf.Lerp(-0.9f, -0.1f, Mathf.Pow(1f, 0.5f)))
			{
				spin = (int)Mathf.Sign(actOnJump.bestJump.initVel.x);
			}
			else
			{
				spin = 0;
			}
		}
	}

	public void SetUpCombatSkills()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.abstractCreature.ID.RandomSeed);
		dodgeSkill = Custom.PushFromHalf(Mathf.Lerp((UnityEngine.Random.value < 0.5f) ? base.abstractCreature.personality.nervous : base.abstractCreature.personality.aggression, UnityEngine.Random.value, UnityEngine.Random.value), 1f + UnityEngine.Random.value);
		midRangeSkill = Custom.PushFromHalf(Mathf.Lerp((UnityEngine.Random.value < 0.5f) ? base.abstractCreature.personality.energy : base.abstractCreature.personality.aggression, UnityEngine.Random.value, UnityEngine.Random.value), 1f + UnityEngine.Random.value);
		meleeSkill = Custom.PushFromHalf(UnityEngine.Random.value, 1f + UnityEngine.Random.value);
		blockingSkill = Custom.PushFromHalf(Mathf.InverseLerp(0.35f, 1f, Mathf.Lerp((UnityEngine.Random.value < 0.5f) ? base.abstractCreature.personality.bravery : base.abstractCreature.personality.energy, UnityEngine.Random.value, UnityEngine.Random.value)), 1f + UnityEngine.Random.value);
		reactionSkill = Custom.PushFromHalf(Mathf.Lerp(base.abstractCreature.personality.energy, UnityEngine.Random.value, UnityEngine.Random.value), 1f + UnityEngine.Random.value);
		if (ModManager.MSC)
		{
			if (Elite)
			{
				float num = Mathf.Lerp(base.abstractCreature.personality.dominance, 1f, 0.15f);
				dodgeSkill = Mathf.Lerp(dodgeSkill, 1f, num * 0.15f);
				midRangeSkill = Mathf.Lerp(midRangeSkill, 1f, num * 0.1f);
				blockingSkill = Mathf.Lerp(blockingSkill, 1f, num * 0.1f);
				reactionSkill = Mathf.Lerp(reactionSkill, 1f, num * 0.05f);
			}
			else
			{
				float num2 = 1f - base.abstractCreature.personality.dominance;
				dodgeSkill = Mathf.Lerp(dodgeSkill, 0f, num2 * 0.085f);
				midRangeSkill = Mathf.Lerp(midRangeSkill, 0f, num2 * 0.085f);
				blockingSkill = Mathf.Lerp(blockingSkill, 0f, num2 * 0.05f);
				reactionSkill = Mathf.Lerp(reactionSkill, 0f, num2 * 0.15f);
			}
		}
		UnityEngine.Random.state = state;
	}

	public void CollideWithOtherCreature(BodyChunk creatureChunk, Tracker.CreatureRepresentation rep)
	{
		if (base.grasps[0] != null && base.grasps[0].grabbed is Spear && creatureChunk.pos.x < base.mainBodyChunk.pos.x == base.bodyChunks[2].pos.x < base.mainBodyChunk.pos.x && UnityEngine.Random.value < Mathf.Pow(AI.agitation, 0.2f) * meleeSkill && WantToLethallyAttack(rep))
		{
			immediatelyThrowAtChunk = creatureChunk;
		}
	}

	private bool WantToLethallyAttack(Tracker.CreatureRepresentation rep)
	{
		if (rep == null || rep.representedCreature.state.dead || AI.ViolenceTypeAgainstCreature(rep) != ScavengerAI.ViolenceType.Lethal || (rep.dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Afraid && rep.dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Attacks) || rep.dynamicRelationship.currentRelationship.intensity < Mathf.Pow(AI.agitation * base.abstractCreature.personality.aggression, 0.5f))
		{
			return false;
		}
		if (rep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && room.game.IsStorySession && (room.game.StoryCharacter == SlugcatStats.Name.Yellow || (room.game.StoryCharacter == SlugcatStats.Name.White && rep.dynamicRelationship.currentRelationship.intensity < 0.9f)))
		{
			return false;
		}
		if (!(AI.behavior == ScavengerAI.Behavior.Flee) || rep != AI.threatTracker.mostThreateningCreature || !(UnityEngine.Random.value < AI.scared))
		{
			if (AI.behavior == ScavengerAI.Behavior.Attack && rep == AI.preyTracker.MostAttractivePrey)
			{
				return AI.currentViolenceType == ScavengerAI.ViolenceType.Lethal;
			}
			return false;
		}
		return true;
	}

	public void CombatUpdate()
	{
		if (dodgeDelay > 0)
		{
			dodgeDelay--;
		}
		float num = (room.game.IsStorySession ? (AI.agitation * 0.25f) : Mathf.InverseLerp(0f, 1f - Mathf.Pow(reactionSkill, 3f), AI.agitation));
		if (AI.agitation == 1f && !ModManager.MSC)
		{
			reflexBuildUp = Mathf.Clamp(reflexBuildUp + Mathf.Lerp(1f / 120f, 0.1f, reactionSkill), num, 1f);
		}
		else if (AI.agitation != 1f)
		{
			reflexBuildUp = Mathf.Max(num, reflexBuildUp - Mathf.Lerp(1f / 30f, 0.002f, reactionSkill));
			fastReflexBuildUp = Mathf.Max(num, fastReflexBuildUp - Mathf.Lerp(1f / 30f, 0.002f, reactionSkill));
		}
		if (immediatelyThrowAtChunk != null)
		{
			if (!Custom.DistLess(immediatelyThrowAtChunk.pos, base.mainBodyChunk.pos, immediatelyThrowAtChunk.rad + MeleeRange) || AI.currentViolenceType != ScavengerAI.ViolenceType.Lethal || base.grasps[0] == null || base.grasps[0].grabbed is ScavengerBomb || (ModManager.MSC && base.grasps[0].grabbed is SingularityBomb) || (ModManager.MMF && base.grasps[0].grabbed is ExplosiveSpear))
			{
				immediatelyThrowAtChunk = null;
			}
			else
			{
				TryToMeleeCreature();
			}
		}
		if (UnityEngine.Random.value < midRangeSkill * (0.2f + 0.8f * AI.agitation))
		{
			MidRangeUpdate();
		}
	}

	public bool ReactionCheck()
	{
		reflexBuildUp = Mathf.Min(1f, reflexBuildUp + Mathf.Lerp(1f / 120f, ModManager.MSC ? 0.2f : 0.5f, Mathf.Pow(reactionSkill, 3.5f)));
		return reflexBuildUp >= 1f;
	}

	private void TryToMeleeCreature()
	{
		if (base.safariControlled)
		{
			if (!inputWithDiagonals.HasValue || !inputWithDiagonals.Value.thrw)
			{
				return;
			}
		}
		else if (UnityEngine.Random.value > meleeSkill || !FastReactionCheck())
		{
			return;
		}
		if (ModManager.MMF && Charging)
		{
			(animation as ThrowChargeAnimation).discontinue = 0;
		}
		int num = 5;
		float num2 = 125f;
		if (!Elite)
		{
			if (ModManager.MSC)
			{
				num = (int)Mathf.Lerp(40f, 10f, reactionSkill);
			}
			num2 = 50f;
		}
		if (Custom.DistLess(immediatelyThrowAtChunk.pos, base.mainBodyChunk.pos + new Vector2((base.bodyChunks[2].pos.x < base.mainBodyChunk.pos.x) ? (0f - num2) : num2, 0f), num2))
		{
			if ((Charging && animation.age > num) || (ModManager.MSC && animation != null && animation.id == MoreSlugcatsEnums.ScavengerAnimationID.Jumping))
			{
				Throw(Custom.DirVec(base.mainBodyChunk.pos, immediatelyThrowAtChunk.pos));
				base.mainBodyChunk.vel += Custom.DirVec(immediatelyThrowAtChunk.pos, base.mainBodyChunk.pos) * 3f;
				immediatelyThrowAtChunk = null;
			}
			else if (animation == null)
			{
				animation = new ThrowChargeAnimation(this, immediatelyThrowAtChunk);
			}
		}
		else if ((Charging && animation.age > num) || (ModManager.MSC && animation != null && animation.id == MoreSlugcatsEnums.ScavengerAnimationID.Jumping))
		{
			for (int i = 0; i < immediatelyThrowAtChunk.owner.bodyChunks.Length; i++)
			{
				if (Custom.DistLess(immediatelyThrowAtChunk.owner.bodyChunks[i].pos, base.mainBodyChunk.pos + new Vector2((base.bodyChunks[2].pos.x < base.mainBodyChunk.pos.x) ? (0f - num2) : num2, 0f), num2))
				{
					Throw(Custom.DirVec(base.mainBodyChunk.pos, immediatelyThrowAtChunk.owner.bodyChunks[i].pos));
					base.mainBodyChunk.vel += Custom.DirVec(immediatelyThrowAtChunk.owner.bodyChunks[i].pos, base.mainBodyChunk.pos) * 3f;
					immediatelyThrowAtChunk = null;
					break;
				}
			}
		}
		if (ModManager.MMF && animation == null)
		{
			animation = new ThrowChargeAnimation(this, immediatelyThrowAtChunk);
		}
	}

	private void MidRangeUpdate()
	{
		if (base.grasps[0] == null || !(base.grasps[0].grabbed is Spear))
		{
			return;
		}
		Tracker.CreatureRepresentation focusCreature = AI.focusCreature;
		if (focusCreature == null || !focusCreature.VisualContact || UnityEngine.Random.value > midRangeSkill || !WantToLethallyAttack(focusCreature) || Mathf.Abs(focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos.y - base.mainBodyChunk.pos.y) > Mathf.Lerp(Elite ? 60f : 30f, Elite ? 100f : 60f, midRangeSkill) || !Custom.DistLess(focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos, base.mainBodyChunk.pos, MidRange))
		{
			return;
		}
		BodyChunk bodyChunk = focusCreature.representedCreature.realizedCreature.bodyChunks[UnityEngine.Random.Range(0, focusCreature.representedCreature.realizedCreature.bodyChunks.Length)];
		if (Mathf.Abs(bodyChunk.pos.y - bodyChunk.lastPos.y) > 60f * midRangeSkill || Custom.DirVec(base.mainBodyChunk.pos, bodyChunk.pos).y > 0.2f || !room.VisualContact(base.mainBodyChunk.pos, bodyChunk.pos))
		{
			return;
		}
		Vector2 vector = AimPosForChunk(bodyChunk, 0.45f, 0f);
		if (Custom.DirVec(base.mainBodyChunk.pos, vector).y > 0.25f)
		{
			return;
		}
		if (bodyChunk.owner is Lizard)
		{
			int num = -1;
			float dst = float.MaxValue;
			for (int i = 0; i < bodyChunk.owner.bodyChunks.Length; i++)
			{
				if (Custom.DistLess(base.mainBodyChunk.pos, bodyChunk.owner.bodyChunks[i].pos, dst))
				{
					num = i;
					dst = Vector2.Distance(base.mainBodyChunk.pos, bodyChunk.owner.bodyChunks[i].pos);
				}
			}
			if (num == 0)
			{
				return;
			}
		}
		int num2 = 5;
		if (ModManager.MSC && !Elite)
		{
			num2 = (int)Mathf.Lerp(40f, 10f, reactionSkill);
		}
		if ((Charging && animation.age > num2 && room.VisualContact(base.mainBodyChunk.pos, bodyChunk.pos) && ReactionCheck()) || (ModManager.MSC && animation != null && animation.id == MoreSlugcatsEnums.ScavengerAnimationID.Jumping))
		{
			for (int j = 0; j < AI.tracker.CreaturesCount; j++)
			{
				if (AI.tracker.GetRep(j).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Pack && AI.tracker.GetRep(j).representedCreature.realizedCreature != null && !AI.tracker.GetRep(j).representedCreature.realizedCreature.dead && AI.tracker.GetRep(j).representedCreature.realizedCreature.room == room && Custom.DistLess(AI.tracker.GetRep(j).representedCreature.realizedCreature.mainBodyChunk.pos, Custom.ClosestPointOnLineSegment(base.mainBodyChunk.pos, bodyChunk.pos, AI.tracker.GetRep(j).representedCreature.realizedCreature.mainBodyChunk.pos), 40f))
				{
					Custom.Log("return, friend in the way");
					return;
				}
			}
			Throw(Custom.DirVec(base.mainBodyChunk.pos, vector));
			base.mainBodyChunk.vel += Custom.DirVec(vector, base.mainBodyChunk.pos) * 3f;
		}
		else if (animation == null && (!base.safariControlled || (inputWithDiagonals.HasValue && inputWithDiagonals.Value.thrw)))
		{
			animation = new ThrowChargeAnimation(this, bodyChunk);
		}
	}

	private Vector2 AimPosForChunk(BodyChunk chunk, float projectileGravity, float chunkGravity)
	{
		float num = Mathf.Max(0f, Mathf.Abs(chunk.pos.x - base.mainBodyChunk.pos.x) - 50f) / (Mathf.Max(20f, (base.grasps[0].grabbed as Weapon).exitThrownModeSpeed + 5f) + Mathf.Abs(chunk.pos.x - chunk.lastPos.x));
		Vector2 result = new Vector2(chunk.pos.x + (chunk.pos.x - chunk.lastPos.x) * num, chunk.pos.y);
		result.y += projectileGravity * Mathf.Pow(num, 2f);
		result.y -= chunkGravity * Mathf.Pow(num, 2f);
		result.y += (chunk.pos.y - chunk.lastPos.y) * num;
		return result;
	}

	public void TryThrow(BodyChunk aimChunk, ScavengerAI.ViolenceType violenceType)
	{
		TryThrow(aimChunk, violenceType, null);
	}

	public void TryThrow(BodyChunk aimChunk, ScavengerAI.ViolenceType violenceType, Vector2? aimPosition)
	{
		float num = (Elite ? 0f : 0.75f);
		if (base.grasps[0] == null || (animation != null && animation.id == ScavengerAnimation.ID.Throw) || (aimChunk != null && Mathf.Abs(Custom.DirVec(base.mainBodyChunk.pos, aimChunk.pos).x) < num && !base.safariControlled))
		{
			return;
		}
		bool flag = base.grasps[0].grabbed is Spear;
		bool flag2 = AI.RealWeapon(base.grasps[0].grabbed) && violenceType != ScavengerAI.ViolenceType.Lethal && violenceType != ScavengerAI.ViolenceType.ForFun;
		if (base.safariControlled)
		{
			flag2 = false;
		}
		Vector2 vector = Vector2.zero;
		if (aimChunk != null)
		{
			vector = aimChunk.pos;
			vector += (aimChunk.lastPos - aimChunk.pos) * Vector2.Distance(base.mainBodyChunk.pos, vector) * (0.02f + 0.02f * UnityEngine.Random.value);
		}
		else if (aimPosition.HasValue)
		{
			vector = aimPosition.Value;
		}
		float p = (Elite ? 1f : 1.5f);
		float num2 = (Elite ? 0.35f : 0.05f);
		if (!Custom.DistLess(base.mainBodyChunk.pos, vector, 200f))
		{
			vector.y += Mathf.Pow(Vector2.Distance(base.mainBodyChunk.pos, vector), p) * num2 * (flag ? (0.08f + UnityEngine.Random.value * 0.1f) : (0.15f + UnityEngine.Random.value * 0.2f));
		}
		if (aimChunk != null)
		{
			if (flag2)
			{
				vector.y += 50f * Mathf.Max(1f, Vector2.Distance(base.mainBodyChunk.pos, aimChunk.pos) / 400f);
				for (int i = 0; i < aimChunk.owner.bodyChunks.Length; i++)
				{
					vector.y = Mathf.Max(vector.y, aimChunk.owner.bodyChunks[i].pos.y + aimChunk.owner.bodyChunks[i].rad + 50f);
				}
			}
			else
			{
				Vector2? vector2 = SharedPhysics.ExactTerrainRayTracePos(room, aimChunk.pos, vector);
				if (vector2.HasValue)
				{
					vector = Vector2.Lerp(vector2.Value, vector, 0.1f);
				}
			}
		}
		float num3 = 0.95f;
		if (Elite)
		{
			num3 = 0.95f - Mathf.InverseLerp(150f, 600f, Vector2.Distance(base.mainBodyChunk.pos, vector)) * 0.05f;
		}
		if (base.safariControlled)
		{
			if (Mathf.Abs(Custom.DirVec(base.mainBodyChunk.pos, vector).x) < num3)
			{
				vector.y = base.mainBodyChunk.pos.y;
			}
		}
		else if (Mathf.Abs(Custom.DirVec(base.mainBodyChunk.pos, vector).x) < num3 || !room.VisualContact(base.mainBodyChunk.pos, vector))
		{
			return;
		}
		Vector2 vector3 = Vector2.zero;
		if (aimChunk != null)
		{
			vector3 = aimChunk.lastPos - aimChunk.pos;
		}
		if (violenceType != ScavengerAI.ViolenceType.Warning || !flag)
		{
			vector3.x *= 0.1f;
		}
		float a = 200f;
		float b = 900f;
		float num4 = 40f;
		if (Elite)
		{
			a = 400f;
			b = 1800f;
			num4 = 40f + Mathf.InverseLerp(150f, 600f, Vector2.Distance(base.mainBodyChunk.pos, vector)) * 60f;
		}
		float num5;
		if (flag2)
		{
			num5 = vector3.magnitude * Mathf.Max(20f, Vector2.Distance(base.mainBodyChunk.pos, vector) - Mathf.Lerp(a, b, base.abstractCreature.personality.aggression)) * 0.05f;
		}
		else
		{
			Vector2 b2 = Vector2.zero;
			if (aimChunk != null)
			{
				b2 = aimChunk.pos;
			}
			else if (aimPosition.HasValue)
			{
				b2 = aimPosition.Value;
			}
			num5 = vector3.magnitude * Mathf.Max(10f, Vector2.Distance(base.mainBodyChunk.pos, vector) - Mathf.Lerp(a, b, base.abstractCreature.personality.bravery)) * 0.1f;
			if (Vector2.Distance(base.mainBodyChunk.pos, b2) < (Elite ? 240f : 120f) && Vector2.Distance(base.mainBodyChunk.pos, b2) > 40f && Mathf.Abs(base.mainBodyChunk.pos.y - b2.y) < num4)
			{
				num5 = 0f;
			}
		}
		float num6 = 0f;
		int num7 = 0;
		for (int j = 0; j < base.grasps.Length; j++)
		{
			if (base.grasps[j] != null)
			{
				num6 += (float)AI.WeaponScore(base.grasps[j].grabbed, pickupDropInsteadOfWeaponSelection: false);
				if (AI.RealWeapon(base.grasps[j].grabbed))
				{
					num7++;
				}
			}
		}
		if (base.grasps[0].grabbed is Rock && violenceType != ScavengerAI.ViolenceType.Warning)
		{
			num6 *= 4f;
			num6 += 20f;
		}
		else if (AI.RealWeapon(base.grasps[0].grabbed) && num7 < 2)
		{
			num6 *= 0.2f + 0.8f * base.abstractCreature.personality.aggression;
		}
		num6 *= 1f + AI.agitation;
		num6 *= 1f + base.abstractCreature.personality.aggression * 2f;
		if (base.abstractCreature.world.game.IsArenaSession)
		{
			num6 *= 2f;
			if (violenceType == ScavengerAI.ViolenceType.Lethal && aimChunk != null && aimChunk.owner is Player)
			{
				num6 *= Custom.LerpMap(base.abstractCreature.world.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, -1, (aimChunk.owner as Player).playerState.playerNumber), 0f, -1f, 1f, 4f);
			}
		}
		if (violenceType == ScavengerAI.ViolenceType.Warning)
		{
			num6 *= 1f - 0.5f * base.abstractCreature.personality.dominance;
			num5 = 0f;
			if (animation != null && animation.id == ScavengerAnimation.ID.ThrowCharge)
			{
				(animation as ThrowChargeAnimation).cycle += (animation as ThrowChargeAnimation).shake * 0.3f;
				(animation as ThrowChargeAnimation).shake = Mathf.Lerp((animation as ThrowChargeAnimation).shake, 0.2f + 0.8f * Mathf.InverseLerp(1f, 4f, vector3.magnitude), 0.5f);
			}
		}
		num5 *= Custom.Screen((base.State as HealthState).health, 1f - AI.agitation);
		num6 *= Custom.LerpMap(Custom.Screen((base.State as HealthState).health, 1f - AI.agitation), 0f, 1f, 10f, 1f);
		if (animation == null || (animation.id != ScavengerAnimation.ID.ThrowCharge && (!ModManager.MSC || animation.id != MoreSlugcatsEnums.ScavengerAnimationID.Jumping)))
		{
			if (!(num6 < num5 / 2f) || base.safariControlled)
			{
				animation = new ThrowChargeAnimation(this, aimChunk);
				(animation as ThrowChargeAnimation).aimTarget = aimPosition;
			}
			return;
		}
		if (animation is ThrowChargeAnimation)
		{
			if (aimChunk != null && aimChunk != (animation as ThrowChargeAnimation).target)
			{
				(animation as ThrowChargeAnimation).target = aimChunk;
			}
			if (aimPosition.HasValue)
			{
				(animation as ThrowChargeAnimation).aimTarget = aimPosition;
			}
			if ((animation as ThrowChargeAnimation).discontinue < 30)
			{
				(animation as ThrowChargeAnimation).discontinue = 0;
			}
		}
		if (flag2)
		{
			vector += Custom.DirVec(base.mainBodyChunk.pos, vector) * 400f;
			Vector2? vector4 = SharedPhysics.ExactTerrainRayTracePos(room, base.mainBodyChunk.pos, vector);
			if (vector4.HasValue)
			{
				vector = vector4.Value;
			}
		}
		bool flag3 = (float)animation.age > Mathf.Lerp((base.grasps[0].grabbed is Rock) ? 10 : (Elite ? 20 : 40), 2f, Mathf.Pow(Mathf.Max(base.abstractCreature.personality.aggression, 1f - (base.abstractCreature.state as HealthState).health), 0.93f));
		if (ModManager.MSC && animation.id == MoreSlugcatsEnums.ScavengerAnimationID.Jumping)
		{
			flag3 = true;
		}
		if (!(base.safariControlled && flag3) && (!flag3 || (!(violenceType != ScavengerAI.ViolenceType.Warning) && flag && !(UnityEngine.Random.value < vector3.magnitude / Custom.LerpMap(num6, 2f, 50f, 100f, 0.15f, Mathf.Lerp(0.8f, 0.2f, Mathf.Lerp(AI.backedByPack, base.abstractCreature.personality.aggression, 0.5f))))) || (!(Mathf.Abs(Custom.DirVec(base.mainBodyChunk.pos, vector).x) > 0.975f) && !Elite) || !(num6 > num5) || !AI.IsThrowPathClearFromFriends(vector, flag2 ? 40f : 0f)))
		{
			return;
		}
		float num8 = Vector2.Distance(base.mainBodyChunk.pos, vector);
		if (num8 > 50f && Elite)
		{
			float y = Custom.DirVec(base.mainBodyChunk.pos, vector).y;
			if ((double)y >= 0.0 && (double)y <= 0.05)
			{
				vector.y += 17f * (num8 / 180f);
			}
			else if ((double)y >= -0.1 && y <= 0f)
			{
				vector.y += 10f * (num8 / 200f);
			}
			else if (y <= 0f)
			{
				vector.y += 25f * (num8 / 200f);
			}
			else
			{
				vector.y += 15f * (num8 / 200f);
			}
		}
		Throw(Custom.DirVec(base.mainBodyChunk.pos, vector));
		if (aimChunk != null)
		{
			Tracker.CreatureRepresentation creatureRepresentation = AI.tracker.RepresentationForObject(aimChunk.owner, AddIfMissing: false);
			if (creatureRepresentation != null)
			{
				(creatureRepresentation.dynamicRelationship.state as ScavengerAI.ScavengerTrackState).throwsTowardsThisCreature++;
			}
		}
	}

	public void Throw(Vector2 throwDir)
	{
		animation = new ThrowAnimation(this, base.grasps[0].grabbed, Mathf.Sign(throwDir.x));
		reflexBuildUp = Mathf.Lerp(reflexBuildUp, 1f, 0.5f);
		if (base.grasps[0].grabbed is Weapon)
		{
			(base.grasps[0].grabbed as Weapon).Thrown(this, base.mainBodyChunk.pos + throwDir * 50f, base.mainBodyChunk.pos - throwDir * 5f, new IntVector2((int)Mathf.Sign(throwDir.x), 0), Elite ? 0.75f : (ModManager.MSC ? 0.35f : 1.3f), evenUpdate);
			if (ModManager.MSC && base.grasps[0].grabbed is Spear)
			{
				if (Elite)
				{
					base.grasps[0].grabbed.firstChunk.vel = throwDir * Mathf.Max(20f, (base.grasps[0].grabbed as Weapon).exitThrownModeSpeed + 5f);
				}
				else
				{
					base.grasps[0].grabbed.firstChunk.vel = throwDir * Mathf.Max(20f, (base.grasps[0].grabbed as Weapon).exitThrownModeSpeed);
				}
			}
			base.grasps[0].grabbed.firstChunk.vel = throwDir * Mathf.Max(20f, (base.grasps[0].grabbed as Weapon).exitThrownModeSpeed + 5f);
		}
		else
		{
			if (base.grasps[0].grabbed.bodyChunks.Length == 1)
			{
				base.grasps[0].grabbed.firstChunk.pos = Vector2.Lerp(base.grasps[0].grabbed.firstChunk.pos, base.mainBodyChunk.pos + throwDir * 50f, 0.5f);
			}
			base.grasps[0].grabbed.firstChunk.vel = throwDir * 20f;
			if (base.grasps[0].grabbed is JellyFish)
			{
				(base.grasps[0].grabbed as JellyFish).Tossed(this);
			}
			if (ModManager.MSC && base.grasps[0].grabbed is FireEgg)
			{
				(base.grasps[0].grabbed as FireEgg).Tossed(this);
			}
		}
		base.ReleaseGrasp(0);
	}

	public void MeleeGetFree(Creature target, bool eu)
	{
		int num = -1;
		for (int i = 0; i < base.grasps.Length; i++)
		{
			if (base.grasps[i] != null && base.grasps[i].grabbed is Spear)
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			return;
		}
		Spear spear = base.grasps[num].grabbed as Spear;
		BodyChunk bodyChunk = null;
		float dst = float.MaxValue;
		for (int j = 0; j < target.bodyChunks.Length; j++)
		{
			if (Custom.DistLess(base.mainBodyChunk.pos, target.bodyChunks[j].pos, dst))
			{
				bodyChunk = target.bodyChunks[j];
				dst = Vector2.Distance(base.mainBodyChunk.pos, target.bodyChunks[j].pos);
			}
		}
		if (bodyChunk != null && (!base.safariControlled || (inputWithDiagonals.HasValue && inputWithDiagonals.Value.thrw)))
		{
			spear.Thrown(this, base.mainBodyChunk.pos, base.mainBodyChunk.pos, new IntVector2((base.mainBodyChunk.pos.x < bodyChunk.pos.x) ? 1 : (-1), 0), 1f, eu);
			spear.meleeHitChunk = bodyChunk;
			ReleaseGrasp(num);
		}
	}

	public void FlyingWeapon(Weapon weapon)
	{
		float num = Mathf.Max(blockingSkill, dodgeSkill) * (0.3f + 0.7f * Mathf.Pow(AI.agitation, 0.3f));
		if (base.safariControlled)
		{
			num = 1f;
		}
		if (dodgeDelay > 0 || !base.Consious || (!base.safariControlled && UnityEngine.Random.value > (base.State as HealthState).health) || (UnityEngine.Random.value > num && weapon.thrownBy != null && AI.focusCreature != null && weapon.thrownBy.abstractCreature != AI.focusCreature.representedCreature) || Mathf.Abs(weapon.firstChunk.pos.y - base.mainBodyChunk.pos.y) > 120f || !Custom.DistLess(weapon.firstChunk.pos, base.mainBodyChunk.pos, 400f + 400f * num) || Mathf.Abs(weapon.firstChunk.pos.x - weapon.firstChunk.lastPos.x) < 1f || !weapon.HeavyWeapon || weapon.firstChunk.pos.x < weapon.firstChunk.lastPos.x != base.mainBodyChunk.pos.x < weapon.firstChunk.pos.x || !AI.VisualContact(weapon.firstChunk.pos, Mathf.Pow(num, 0.5f)) || !FastReactionCheck())
		{
			return;
		}
		float num2 = Mathf.Abs(base.mainBodyChunk.pos.x - weapon.firstChunk.pos.x) / Mathf.Abs(weapon.firstChunk.pos.x - weapon.firstChunk.lastPos.x);
		if (Mathf.Abs(num2 - 6f) > 1f + num * 10f + num * AI.agitation * 10f)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			if (BallisticCollision(base.bodyChunks[i].pos, weapon.firstChunk.lastPos, weapon.firstChunk.pos, base.bodyChunks[i].rad + weapon.firstChunk.rad + 5f + 40f * base.abstractCreature.personality.nervous, (weapon is Spear) ? 0.45f : 0.9f))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		for (int j = 0; j < AI.tracker.CreaturesCount; j++)
		{
			if (AI.tracker.GetRep(j).representedCreature.realizedCreature == null || AI.tracker.GetRep(j).representedCreature.realizedCreature.room != room || AI.tracker.GetRep(j).representedCreature.realizedCreature == weapon.thrownBy || !Custom.DistLess(AI.tracker.GetRep(j).representedCreature.realizedCreature.mainBodyChunk.pos, Custom.ClosestPointOnLineSegment(base.mainBodyChunk.pos, weapon.firstChunk.pos, AI.tracker.GetRep(j).representedCreature.realizedCreature.mainBodyChunk.pos), 100f))
			{
				continue;
			}
			for (int k = 0; k < AI.tracker.GetRep(j).representedCreature.realizedCreature.bodyChunks.Length; k++)
			{
				if (Custom.DistLess(AI.tracker.GetRep(j).representedCreature.realizedCreature.bodyChunks[k].pos, Custom.ClosestPointOnLineSegment(base.mainBodyChunk.pos, weapon.firstChunk.pos, AI.tracker.GetRep(j).representedCreature.realizedCreature.bodyChunks[k].pos), AI.tracker.GetRep(j).representedCreature.realizedCreature.bodyChunks[k].rad + 10f))
				{
					Custom.Log("other will take hit");
					return;
				}
			}
		}
		if (!base.safariControlled && ((blockingSkill > dodgeSkill && UnityEngine.Random.value < Mathf.Pow(blockingSkill, 2f) && TakeDownIncomingWeapon(weapon)) || UnityEngine.Random.value > dodgeSkill))
		{
			return;
		}
		float num3 = CrossHeight(base.mainBodyChunk.pos.x, weapon.firstChunk.lastPos, weapon.firstChunk.pos, (weapon is Spear) ? 0.45f : 0.9f);
		Vector2[] array = new Vector2[3]
		{
			base.bodyChunks[0].pos,
			base.bodyChunks[1].pos,
			base.bodyChunks[2].pos
		};
		if (room.aimap.getAItile(base.bodyChunks[1].pos).acc == AItile.Accessibility.Floor && (!base.safariControlled || (inputWithDiagonals.HasValue && inputWithDiagonals.Value.y < 0)))
		{
			float num4 = room.MiddleOfTile(base.bodyChunks[1].pos).y - 10f;
			if (num3 > num4)
			{
				for (int l = 0; l < base.bodyChunks.Length; l++)
				{
					array[l].y = num4 + base.bodyChunks[l].rad;
				}
				if (OutOfDanger(weapon.firstChunk.lastPos, weapon.firstChunk.pos, array, weapon.firstChunk.rad + 5f, (weapon is Spear) ? 0.45f : 0.9f))
				{
					Custom.Log("DUCK!");
					dodgeDelay = (int)Mathf.Lerp(25f, 1f, dodgeSkill);
					for (int m = 0; m < base.bodyChunks.Length; m++)
					{
						base.bodyChunks[m].pos.y = array[m].y;
						base.bodyChunks[m].vel.y -= 4f * Mathf.Max(1f, 0.5f + dodgeSkill);
					}
					base.stun = Mathf.Max(base.stun, (int)(24f * (1f - dodgeSkill)));
					return;
				}
			}
		}
		if (num3 < Mathf.Max(base.bodyChunks[0].pos.y, base.bodyChunks[1].pos.y) && (room.aimap.TileAccessibleToCreature(base.bodyChunks[0].pos + new Vector2(0f, 20f), base.Template) || room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos + new Vector2(0f, 20f), base.Template) || (!room.GetTile(base.firstChunk.pos + new Vector2(0f, 20f)).Solid && room.aimap.TileAccessibleToCreature(base.firstChunk.pos + new Vector2(0f, 40f), base.Template))))
		{
			Vector2 vector = (base.bodyChunks[0].pos * 2f + base.bodyChunks[1].pos * 2f + base.bodyChunks[2].pos) / 5f;
			if (!room.GetTile(vector + new Vector2(0f, 25f)).Solid)
			{
				vector.y += 15f;
				for (int n = 0; n < base.bodyChunks.Length; n++)
				{
					array[n] = base.bodyChunks[n].pos;
					if (array[n].y < vector.y)
					{
						array[n].y = Mathf.Lerp(array[n].y, vector.y, 0.75f);
					}
				}
				if (OutOfDanger(weapon.firstChunk.lastPos, weapon.firstChunk.pos, array, weapon.firstChunk.rad + 5f, (weapon is Spear) ? 0.45f : 0.9f) && (!base.safariControlled || (inputWithDiagonals.HasValue && inputWithDiagonals.Value.y > 0)))
				{
					Custom.Log("UP DODGE!");
					dodgeDelay = (int)Mathf.Lerp(25f, 1f, dodgeSkill);
					for (int num5 = 0; num5 < base.bodyChunks.Length; num5++)
					{
						if (base.bodyChunks[num5].pos.y < vector.y)
						{
							base.bodyChunks[num5].vel.y += 4f * Mathf.Max(1f, 0.5f + dodgeSkill);
						}
						base.bodyChunks[num5].pos.y = array[num5].y;
					}
					base.stun = Mathf.Max(base.stun, (int)(24f * (1f - dodgeSkill)));
					return;
				}
			}
		}
		if (num3 > Mathf.Min(base.bodyChunks[0].pos.y, base.bodyChunks[1].pos.y) && !room.GetTile(base.bodyChunks[1].pos + new Vector2(0f, -20f)).Solid && (!base.safariControlled || (inputWithDiagonals.HasValue && inputWithDiagonals.Value.y > 0)))
		{
			for (int num6 = 0; num6 < base.bodyChunks.Length; num6++)
			{
				array[num6] = base.bodyChunks[num6].pos;
				if (!room.GetTile(array[num6] + new Vector2(0f, -20f)).Solid)
				{
					array[num6].y -= 25f;
				}
			}
			if (OutOfDanger(weapon.firstChunk.lastPos, weapon.firstChunk.pos, array, weapon.firstChunk.rad + 5f, (weapon is Spear) ? 0.45f : 0.9f))
			{
				Custom.Log("DROP DODGE!");
				dodgeDelay = (int)Mathf.Lerp(25f, 1f, dodgeSkill);
				swingPos = null;
				nextSwingPos = null;
				footingCounter = 0;
				drop = true;
				commitToMoveCounter = 20;
				for (int num7 = 0; num7 < base.bodyChunks.Length; num7++)
				{
					base.bodyChunks[num7].pos.y -= 8f * Mathf.Max(1f, 0.5f + dodgeSkill);
					base.bodyChunks[num7].vel.y -= 4f * Mathf.Max(1f, 0.5f + dodgeSkill);
				}
				base.stun = Mathf.Max(base.stun, (int)(24f * (1f - dodgeSkill)));
				return;
			}
		}
		if (BallisticCollision(base.bodyChunks[1].pos, weapon.firstChunk.lastPos, weapon.firstChunk.pos, base.bodyChunks[1].rad + weapon.firstChunk.rad + 5f, (weapon is Spear) ? 0.45f : 0.9f) && num2 < 6f && room.GetTile(base.bodyChunks[1].pos + new Vector2(0f, 0f - base.bodyChunks[1].rad - 10f)).Solid && !room.GetTile(base.mainBodyChunk.pos + new Vector2(0f, 20f)).Solid)
		{
			for (int num8 = 0; num8 < base.bodyChunks.Length; num8++)
			{
				array[num8] = base.bodyChunks[num8].pos + new Vector2(0f, 13f + dodgeSkill * 3f);
			}
			if (OutOfDanger(weapon.firstChunk.lastPos, weapon.firstChunk.pos, array, weapon.firstChunk.rad + 5f, (weapon is Spear) ? 0.45f : 0.9f) && (!base.safariControlled || (inputWithDiagonals.HasValue && inputWithDiagonals.Value.y > 0)))
			{
				Custom.Log("HOP!");
				dodgeDelay = (int)Mathf.Lerp(35f, 3f, dodgeSkill);
				footingCounter = 0;
				for (int num9 = 0; num9 < base.bodyChunks.Length; num9++)
				{
					base.bodyChunks[num9].pos.y += 15f + dodgeSkill * 3f;
				}
				base.bodyChunks[0].vel.y = 2f * Mathf.Max(1f, 0.5f + dodgeSkill);
				base.bodyChunks[1].vel.y = 7f * Mathf.Max(1f, 0.5f + dodgeSkill);
				base.bodyChunks[2].vel.y = 3f * Mathf.Max(1f, 0.5f + dodgeSkill);
				base.stun = Mathf.Max(base.stun, (int)(44f * (1f - dodgeSkill)));
				return;
			}
		}
		if ((UnityEngine.Random.value < AI.agitation * blockingSkill && FastReactionCheck()) || base.safariControlled)
		{
			TakeDownIncomingWeapon(weapon);
		}
	}

	private bool OutOfDanger(Vector2 weaponLast, Vector2 weaponNext, Vector2[] tryPositions, float weaponRad, float gravity)
	{
		for (int i = 0; i < tryPositions.Length; i++)
		{
			if (BallisticCollision(tryPositions[i], weaponLast, weaponNext, base.bodyChunks[i].rad + weaponRad, gravity))
			{
				return false;
			}
		}
		return true;
	}

	private bool BallisticCollision(Vector2 checkPos, Vector2 weaponLast, Vector2 weaponNext, float rad, float gravity)
	{
		float num = CrossHeight(checkPos.x, weaponLast, weaponNext, gravity);
		if (num > checkPos.y - rad)
		{
			return num < checkPos.y + rad;
		}
		return false;
	}

	private float CrossHeight(float xPos, Vector2 weaponLast, Vector2 weaponNext, float gravity)
	{
		if (Mathf.Abs(weaponLast.x - weaponNext.x) < 1f)
		{
			return -1000f;
		}
		float num = Mathf.Abs(xPos - weaponNext.x) / Mathf.Abs(weaponLast.x - weaponNext.x);
		return Custom.VerticalCrossPoint(weaponLast, weaponNext, xPos).y - gravity * num * num;
	}

	private bool TakeDownIncomingWeapon(Weapon weapon)
	{
		if (Custom.DistLess(weapon.firstChunk.pos, base.mainBodyChunk.pos, Mathf.Lerp(200f, 800f, blockingSkill)) && !Custom.DistLess(weapon.firstChunk.pos, base.mainBodyChunk.pos, Mathf.Lerp(160f, 90f, blockingSkill)) && Mathf.Abs(weapon.firstChunk.pos.y - base.mainBodyChunk.pos.y) < Mathf.Lerp(30f, 90f, blockingSkill) && base.grasps[0] != null && base.grasps[0].grabbed is Weapon && (base.grasps[0].grabbed as Weapon).HeavyWeapon && !(base.grasps[0].grabbed is ScavengerBomb) && (!ModManager.MSC || !(base.grasps[0].grabbed is SingularityBomb)) && (!(weapon is Rock) || base.grasps[0].grabbed is Rock) && (!base.safariControlled || (inputWithDiagonals.HasValue && inputWithDiagonals.Value.thrw)))
		{
			Custom.Log("take it down");
			dodgeDelay = (int)Mathf.Lerp(25f, 1f, blockingSkill);
			Vector2 vector = AimPosForChunk(weapon.firstChunk, (base.grasps[0].grabbed is Spear) ? 0.45f : 0.9f, (weapon is Spear) ? 0.45f : 0.9f);
			if (Custom.DirVec(base.mainBodyChunk.pos, vector).y < 0.5f)
			{
				Throw(Custom.DirVec(base.mainBodyChunk.pos, vector));
				base.mainBodyChunk.vel += Custom.DirVec(vector, base.mainBodyChunk.pos) * 3f;
				return true;
			}
		}
		return false;
	}
}
