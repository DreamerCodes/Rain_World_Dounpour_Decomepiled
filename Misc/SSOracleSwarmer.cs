using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CoralBrain;
using RWCustom;
using Unity.Mathematics;
using UnityEngine;

public class SSOracleSwarmer : OracleSwarmer, IOwnProjectedCircles
{
	public class MovementMode : ExtEnum<MovementMode>
	{
		public static readonly MovementMode Swarm = new MovementMode("Swarm", register: true);

		public static readonly MovementMode SuckleMycelia = new MovementMode("SuckleMycelia", register: true);

		public static readonly MovementMode FollowDijkstra = new MovementMode("FollowDijkstra", register: true);

		public MovementMode(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public struct Behavior : IEquatable<Behavior>
	{
		private float dom;

		public float idealDistance;

		public float aimInFront;

		public float torque;

		public float randomVibrations;

		public float revolveSpeed;

		public float life;

		public float deathSpeed;

		public SSOracleSwarmer leader;

		public Vector2 color;

		public bool suckle;

		public bool Equals(Behavior other)
		{
			if (dom.Equals(other.dom) && idealDistance.Equals(other.idealDistance) && aimInFront.Equals(other.aimInFront) && torque.Equals(other.torque) && randomVibrations.Equals(other.randomVibrations) && revolveSpeed.Equals(other.revolveSpeed) && life.Equals(other.life) && deathSpeed.Equals(other.deathSpeed) && object.Equals(leader, other.leader) && color.Equals(other.color))
			{
				return suckle == other.suckle;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Behavior other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((((((((((((((dom.GetHashCode() * 397) ^ idealDistance.GetHashCode()) * 397) ^ aimInFront.GetHashCode()) * 397) ^ torque.GetHashCode()) * 397) ^ randomVibrations.GetHashCode()) * 397) ^ revolveSpeed.GetHashCode()) * 397) ^ life.GetHashCode()) * 397) ^ deathSpeed.GetHashCode()) * 397) ^ ((leader != null) ? leader.GetHashCode() : 0)) * 397) ^ color.GetHashCode()) * 397) ^ suckle.GetHashCode();
		}

		public static bool operator ==(Behavior left, Behavior right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Behavior left, Behavior right)
		{
			return !left.Equals(right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsDead()
		{
			if (!(life <= 0f) && !leader.slatedForDeletetion)
			{
				return leader.currentBehavior != this;
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float Dominance()
		{
			if (!IsDead())
			{
				return dom * math.pow(life, 0.25f);
			}
			return -1f;
		}

		public Behavior(SSOracleSwarmer leader)
		{
			this.leader = leader;
			dom = UnityEngine.Random.value;
			idealDistance = Mathf.Lerp(10f, 300f, UnityEngine.Random.value * UnityEngine.Random.value);
			life = 1f;
			deathSpeed = 1f / Mathf.Lerp(40f, 220f, UnityEngine.Random.value);
			color = new Vector2((float)UnityEngine.Random.Range(0, 3) / 2f, (UnityEngine.Random.value < 0.75f) ? 0f : 1f);
			aimInFront = Mathf.Lerp(40f, 300f, UnityEngine.Random.value);
			torque = ((UnityEngine.Random.value < 0.5f) ? 0f : Mathf.Lerp(-1f, 1f, UnityEngine.Random.value));
			randomVibrations = UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value;
			revolveSpeed = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) / Mathf.Lerp(15f, 65f, UnityEngine.Random.value);
			suckle = UnityEngine.Random.value < 1f / 6f;
		}
	}

	public CoralNeuronSystem system;

	public Vector2 travelDirection;

	public Behavior currentBehavior;

	private float torque;

	private int listBreakPoint;

	public Vector2 color;

	public List<Vector2> stuckList;

	public int stuckListCounter;

	public MovementMode mode = MovementMode.Swarm;

	public Mycelium suckleMyc;

	public bool attachedToMyc;

	public int onlySwarm;

	public int dijkstra;

	public bool dark;

	public override bool Edible
	{
		get
		{
			if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && (grabbedBy[0].grabber as Player).FoodInStomach == (grabbedBy[0].grabber as Player).MaxFoodInStomach)
			{
				return false;
			}
			return true;
		}
	}

	public SSOracleSwarmer(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		travelDirection = Custom.RNV();
		currentBehavior = new Behavior(this);
		color = currentBehavior.color;
		stuckList = new List<Vector2>();
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		if (newRoom.game.SeededRandom(abstractPhysicalObject.ID.RandomSeed) < 1f / 170f && newRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SuperStructureProjector) > 0f)
		{
			newRoom.AddObject(new ProjectedCircle(newRoom, this, 0, 0f));
		}
		system = null;
		for (int i = 0; i < newRoom.updateList.Count; i++)
		{
			if (system != null)
			{
				break;
			}
			if (newRoom.updateList[i] is CoralNeuronSystem)
			{
				system = newRoom.updateList[i] as CoralNeuronSystem;
			}
		}
		stuckList.Clear();
		stuckListCounter = 10;
	}

	public override void Update(bool eu)
	{
		if (system != null && system.Frozen)
		{
			return;
		}
		base.Update(eu);
		if (!room.readyForAI || room.gravity * affectedByGravity > 0.5f)
		{
			return;
		}
		direction = travelDirection;
		if (mode == MovementMode.Swarm)
		{
			SwarmBehavior();
			if (onlySwarm > 0)
			{
				onlySwarm--;
			}
			else if (currentBehavior.suckle && UnityEngine.Random.value < 0.1f && system != null && system.mycelia.Count > 0)
			{
				Mycelium mycelium = system.mycelia[UnityEngine.Random.Range(0, system.mycelia.Count)];
				if (Custom.DistLess(base.firstChunk.pos, mycelium.Tip, 400f) && room.VisualContact(base.firstChunk.pos, mycelium.Tip))
				{
					bool flag = false;
					for (int i = 0; i < otherSwarmers.Count; i++)
					{
						if (flag)
						{
							break;
						}
						if ((otherSwarmers[i] as SSOracleSwarmer).suckleMyc == mycelium)
						{
							flag = true;
						}
					}
					if (!flag)
					{
						mode = MovementMode.SuckleMycelia;
						suckleMyc = mycelium;
						attachedToMyc = false;
					}
				}
			}
			else if (room.aimap.getTerrainProximity(base.firstChunk.pos) < 7)
			{
				if (stuckListCounter > 0)
				{
					stuckListCounter--;
				}
				else
				{
					stuckList.Insert(0, base.firstChunk.pos);
					if (stuckList.Count > 10)
					{
						stuckList.RemoveAt(stuckList.Count - 1);
					}
					stuckListCounter = 80;
				}
				if (UnityEngine.Random.value < 0.025f && stuckList.Count > 1 && Custom.DistLess(base.firstChunk.pos, stuckList[stuckList.Count - 1], 200f))
				{
					List<int> list = new List<int>();
					for (int j = 0; j < room.abstractRoom.connections.Length; j++)
					{
						if (room.aimap.ExitDistanceForCreature(room.GetTilePosition(base.firstChunk.pos), j, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly)) > 0)
						{
							list.Add(j);
						}
					}
					if (list.Count > 0)
					{
						mode = MovementMode.FollowDijkstra;
						dijkstra = list[UnityEngine.Random.Range(0, list.Count)];
					}
				}
			}
		}
		else if (mode == MovementMode.SuckleMycelia)
		{
			if (suckleMyc == null)
			{
				mode = MovementMode.Swarm;
			}
			else if (attachedToMyc)
			{
				direction = Custom.DirVec(base.firstChunk.pos, suckleMyc.Tip);
				float num = Vector2.Distance(base.firstChunk.pos, suckleMyc.Tip);
				base.firstChunk.vel -= (2f - num) * direction * 0.15f;
				base.firstChunk.pos -= (2f - num) * direction * 0.15f;
				suckleMyc.points[suckleMyc.points.GetLength(0) - 1, 0] += (2f - num) * direction * 0.35f;
				suckleMyc.points[suckleMyc.points.GetLength(0) - 1, 2] += (2f - num) * direction * 0.35f;
				travelDirection = new Vector2(0f, 0f);
				if (UnityEngine.Random.value < 0.05f)
				{
					room.AddObject(new NeuronSpark((base.firstChunk.pos + suckleMyc.Tip) / 2f));
				}
				if (UnityEngine.Random.value < 0.0125f)
				{
					suckleMyc = null;
					onlySwarm = UnityEngine.Random.Range(40, 400);
				}
			}
			else
			{
				travelDirection = Custom.DirVec(base.firstChunk.pos, suckleMyc.Tip);
				if (Custom.DistLess(base.firstChunk.pos, suckleMyc.Tip, 5f))
				{
					attachedToMyc = true;
				}
				else if (UnityEngine.Random.value < 0.05f && !room.VisualContact(base.firstChunk.pos, suckleMyc.Tip))
				{
					suckleMyc = null;
				}
			}
			color = Vector2.Lerp(color, currentBehavior.color, 0.05f);
		}
		else if (mode == MovementMode.FollowDijkstra)
		{
			IntVector2 tilePosition = room.GetTilePosition(base.firstChunk.pos);
			int num2 = -1;
			int num3 = int.MaxValue;
			for (int k = 0; k < 4; k++)
			{
				if (!room.GetTile(tilePosition + Custom.fourDirections[k]).Solid)
				{
					int num4 = room.aimap.ExitDistanceForCreature(tilePosition + Custom.fourDirections[k], dijkstra, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
					if (num4 > 0 && num4 < num3)
					{
						num2 = k;
						num3 = num4;
					}
				}
			}
			if (num2 > -1)
			{
				travelDirection += Custom.fourDirections[num2].ToVector2().normalized * 1.4f + Custom.RNV() * UnityEngine.Random.value * 0.5f;
			}
			else
			{
				mode = MovementMode.Swarm;
			}
			travelDirection.Normalize();
			int num5 = room.aimap.ExitDistanceForCreature(tilePosition, dijkstra, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
			if ((UnityEngine.Random.value < 0.025f && num5 < 34) || num5 < 12 || dijkstra < 0 || UnityEngine.Random.value < 0.0025f || (room.aimap.getTerrainProximity(base.firstChunk.pos) >= 7 && UnityEngine.Random.value < 1f / 60f))
			{
				mode = MovementMode.Swarm;
			}
		}
		base.firstChunk.vel += travelDirection * 0.8f * (1f - room.gravity * affectedByGravity);
		base.firstChunk.vel *= Custom.LerpMap(base.firstChunk.vel.magnitude, 0.2f, 3f, 1f, 0.9f);
		if (currentBehavior.IsDead())
		{
			Vector2 vector = currentBehavior.color;
			currentBehavior = new Behavior(this);
			if (UnityEngine.Random.value > 0.25f)
			{
				currentBehavior.color = vector;
			}
		}
		else if (currentBehavior.leader == this)
		{
			currentBehavior.life -= currentBehavior.deathSpeed;
		}
		if (abstractPhysicalObject.destroyOnAbstraction && grabbedBy.Count > 0)
		{
			abstractPhysicalObject.destroyOnAbstraction = false;
		}
	}

	private void SwarmBehavior()
	{
		Vector2 vector = default(Vector2);
		float num = 0f;
		float num2 = currentBehavior.torque;
		Vector2 vector2 = new Vector2(0f, 0f);
		float num3 = 0f;
		float num4 = currentBehavior.revolveSpeed;
		float num5 = 0f;
		int num6 = 0;
		int num7 = -1;
		for (int i = listBreakPoint; i < otherSwarmers.Count; i++)
		{
			if (otherSwarmers[i].slatedForDeletetion)
			{
				otherSwarmers.RemoveAt(i);
				num7 = i;
				break;
			}
			Vector2 pos = base.firstChunk.pos;
			Vector2 pos2 = otherSwarmers[i].firstChunk.pos;
			SSOracleSwarmer sSOracleSwarmer = otherSwarmers[i] as SSOracleSwarmer;
			if (Custom.DistLess(pos, pos2, 400f) && sSOracleSwarmer.mode != MovementMode.SuckleMycelia)
			{
				float num8 = Mathf.InverseLerp(400f, 0f, Vector2.Distance(pos, pos2));
				vector += pos2 * num8;
				num2 += sSOracleSwarmer.torque * num8;
				num4 += sSOracleSwarmer.revolveSpeed * num8;
				num5 += (otherSwarmers[i].rotation - Mathf.Floor(otherSwarmers[i].rotation)) * num8;
				num += num8;
				vector2 += sSOracleSwarmer.color * Mathf.InverseLerp(0.9f, 1f, num8);
				num3 += Mathf.InverseLerp(0.9f, 1f, num8);
				travelDirection += (pos2 + sSOracleSwarmer.travelDirection * currentBehavior.aimInFront * num8 - pos).normalized * num8 * 0.01f;
				travelDirection += (pos - pos2).normalized * Mathf.InverseLerp(currentBehavior.idealDistance, 0f, Vector2.Distance(pos, pos2)) * 0.1f;
				if (currentBehavior.Dominance() < sSOracleSwarmer.currentBehavior.Dominance() * math.pow(num8, 4f))
				{
					currentBehavior = sSOracleSwarmer.currentBehavior;
				}
				num6++;
				if (num6 > 30)
				{
					num7 = i;
					break;
				}
			}
		}
		listBreakPoint = num7 + 1;
		travelDirection += Custom.RNV() * 0.5f * currentBehavior.randomVibrations;
		if (num > 0f)
		{
			travelDirection += Custom.PerpendicularVector(base.firstChunk.pos, vector / num) * torque;
			num5 /= num;
			num5 += Mathf.Floor(rotation);
			if (Mathf.Abs(rotation - num5) < 0.4f)
			{
				rotation = Mathf.Lerp(rotation, num5, 0.05f);
			}
		}
		torque = Mathf.Lerp(torque, num2 / (1f + num), 0.1f);
		revolveSpeed = Mathf.Lerp(revolveSpeed, num4 / (1f + num), 0.2f);
		if (num3 > 0f)
		{
			color = Vector2.Lerp(color, vector2 / num3, 0.4f);
		}
		color = Vector2.Lerp(color, currentBehavior.color, 0.05f);
		if (room.aimap.getTerrainProximity(base.firstChunk.pos) < 5)
		{
			IntVector2 tilePosition = room.GetTilePosition(base.firstChunk.pos);
			Vector2 vector3 = new Vector2(0f, 0f);
			for (int j = 0; j < 4; j++)
			{
				if (!room.GetTile(tilePosition + Custom.fourDirections[j]).Solid && !room.aimap.getAItile(tilePosition + Custom.fourDirections[j]).narrowSpace)
				{
					float num9 = 0f;
					for (int k = 0; k < 4; k++)
					{
						num9 += (float)room.aimap.getTerrainProximity(tilePosition + Custom.fourDirections[j] + Custom.fourDirections[k]);
					}
					vector3 += Custom.fourDirections[j].ToVector2() * num9;
				}
			}
			travelDirection = Vector2.Lerp(travelDirection, vector3.normalized * 2f, 0.5f * Mathf.Pow(Mathf.InverseLerp(5f, 1f, room.aimap.getTerrainProximity(base.firstChunk.pos)), 0.25f));
		}
		travelDirection.Normalize();
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Color color;
		if (!dark)
		{
			color = Custom.HSL2RGB((this.color.x < 0.5f) ? Custom.LerpMap(this.color.x, 0f, 0.5f, 4f / 9f, 2f / 3f) : Custom.LerpMap(this.color.x, 0.5f, 1f, 2f / 3f, 0.99722224f), 1f, 0.5f + 0.5f * this.color.y);
			sLeaser.sprites[4].color = Custom.HSL2RGB((this.color.x < 0.5f) ? Custom.LerpMap(this.color.x, 0f, 0.5f, 4f / 9f, 2f / 3f) : Custom.LerpMap(this.color.x, 0.5f, 1f, 2f / 3f, 0.99722224f), 1f - this.color.y, Mathf.Lerp(0.8f + 0.2f * Mathf.InverseLerp(0.4f, 0.1f, this.color.x), 0.35f, Mathf.Pow(this.color.y, 2f)));
		}
		else
		{
			color = Custom.HSL2RGB((this.color.x <= 0.5f) ? (2f / 3f) : Custom.LerpMap(this.color.x, 0.5f, 1f, 2f / 3f, 0.99722224f), 1f, Mathf.Lerp(0.1f, 0.5f, this.color.y));
			sLeaser.sprites[4].color = Custom.HSL2RGB((this.color.x <= 0.5f) ? (2f / 3f) : Custom.LerpMap(this.color.x, 0.5f, 1f, 2f / 3f, 0.99722224f), 1f, Mathf.Lerp(0.75f, 0.9f, this.color.y));
			sLeaser.sprites[0].isVisible = false;
		}
		for (int i = 0; i < 4; i++)
		{
			sLeaser.sprites[i].color = color;
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public Room HostingCircleFromRoom()
	{
		return room;
	}

	public bool CanHostCircle()
	{
		return !base.slatedForDeletetion;
	}

	public Vector2 CircleCenter(int index, float timeStacker)
	{
		return Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
	}
}
