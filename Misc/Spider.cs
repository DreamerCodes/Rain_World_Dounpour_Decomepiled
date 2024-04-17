using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class Spider : Creature
{
	public struct IndividualVariations
	{
		public float dominance;

		public float size;

		public IndividualVariations(float dominance)
		{
			this.dominance = dominance;
			size = dominance;
		}
	}

	public abstract class SpiderMass
	{
		public List<Spider> spiders;

		public bool lastEu;

		public Room room;

		public Color color = Custom.HSL2RGB(UnityEngine.Random.value, 1f, 0.5f);

		public virtual Spider FirstSpider
		{
			get
			{
				if (spiders.Count == 0)
				{
					return null;
				}
				return spiders[0];
			}
		}

		public SpiderMass(Spider originalSpider, Room room)
		{
			this.room = room;
			spiders = new List<Spider> { originalSpider };
		}

		public virtual void Update(bool eu)
		{
			for (int num = spiders.Count - 1; num >= 0; num--)
			{
				if (spiders[num].dead || spiders[num].room != room)
				{
					AbandonSpider(num);
				}
			}
		}

		public bool ShouldIUpdate(bool eu)
		{
			if (eu == lastEu)
			{
				return false;
			}
			lastEu = eu;
			return true;
		}

		public void AddSpider(Spider spd)
		{
			if (spiders.IndexOf(spd) == -1)
			{
				spiders.Add(spd);
			}
			if (this is Flock)
			{
				spd.flock = this as Flock;
			}
			else if (this is Centipede)
			{
				spd.centipede = this as Centipede;
			}
		}

		public void AbandonSpider(Spider spd)
		{
			for (int i = 0; i < spiders.Count; i++)
			{
				if (spiders[i] == spd)
				{
					AbandonSpider(i);
					break;
				}
			}
		}

		private void AbandonSpider(int i)
		{
			if (this is Flock && spiders[i].flock == this as Flock)
			{
				spiders[i].flock = null;
			}
			else if (this is Centipede && spiders[i].centipede == this as Centipede)
			{
				spiders[i].centipede = null;
			}
			spiders.RemoveAt(i);
		}

		public void Merge(SpiderMass otherFlock)
		{
			if (otherFlock == this)
			{
				return;
			}
			for (int i = 0; i < otherFlock.spiders.Count; i++)
			{
				if (spiders.IndexOf(otherFlock.spiders[i]) == -1)
				{
					spiders.Add(otherFlock.spiders[i]);
					if (this is Flock)
					{
						otherFlock.spiders[i].flock = this as Flock;
					}
					else if (this is Centipede)
					{
						otherFlock.spiders[i].centipede = this as Centipede;
					}
				}
			}
			otherFlock.spiders.Clear();
		}
	}

	public class Flock : SpiderMass
	{
		public Flock(Spider originalSpider, Room room)
			: base(originalSpider, room)
		{
		}

		public override void Update(bool eu)
		{
			if (!ShouldIUpdate(eu))
			{
				return;
			}
			base.Update(eu);
			if (room.abstractRoom.creatures.Count == 0)
			{
				return;
			}
			AbstractCreature abstractCreature = room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)];
			if (abstractCreature.realizedCreature != null && abstractCreature.realizedCreature is Spider && (abstractCreature.realizedCreature as Spider).flock != null && (abstractCreature.realizedCreature as Spider).flock != this && (abstractCreature.realizedCreature as Spider).flock.FirstSpider != null)
			{
				if (spiders.Count >= (abstractCreature.realizedCreature as Spider).flock.spiders.Count)
				{
					Merge((abstractCreature.realizedCreature as Spider).flock);
				}
				else
				{
					(abstractCreature.realizedCreature as Spider).flock.Merge(this);
				}
			}
		}
	}

	public class Centipede : SpiderMass
	{
		public class CentipedePart
		{
			public Centipede centipede;

			public Spider spider;

			public bool inRightPlace;

			public int separatedCounter;

			public int index;

			public CentipedePart master;

			public float BodyFac
			{
				get
				{
					if (centipede.body.Count >= 2)
					{
						return (float)index / (float)(centipede.body.Count - 1);
					}
					return 0f;
				}
			}

			public CentipedePart(Centipede centipede, Spider spider)
			{
				this.centipede = centipede;
				this.spider = spider;
				inRightPlace = false;
			}

			public void Update(int index)
			{
				this.index = index;
				if (index == 0)
				{
					master = null;
				}
				else
				{
					master = centipede.body[index - 1];
				}
				if (!ModManager.MMF || UnityEngine.Random.value <= 0.25f)
				{
					if (index > 0 && Custom.ManhattanDistance(centipede.room.GetTilePosition(spider.mainBodyChunk.pos), centipede.room.GetTilePosition(master.spider.mainBodyChunk.pos)) > 1 && !centipede.room.VisualContact(spider.mainBodyChunk.pos, master.spider.mainBodyChunk.pos))
					{
						separatedCounter++;
					}
					else
					{
						separatedCounter = 0;
					}
				}
				if (index > 0 && !Custom.DistLess(spider.mainBodyChunk.pos, master.spider.mainBodyChunk.pos, inRightPlace ? 50f : 100f))
				{
					separatedCounter = int.MaxValue;
				}
			}
		}

		public List<CentipedePart> body;

		public int maxSize;

		public int walkCycle;

		public int counter;

		public float totalMass;

		public Creature prey;

		public Vector2 preyPos;

		public int preyVisualCounter;

		public float lightAdaption;

		public float hunt;

		public override Spider FirstSpider
		{
			get
			{
				if (body.Count == 0)
				{
					return null;
				}
				return body[0].spider;
			}
		}

		public Centipede(Spider originalSpider, Room room)
			: base(originalSpider, room)
		{
			body = new List<CentipedePart>();
			maxSize = UnityEngine.Random.Range(12, 22);
		}

		public override void Update(bool eu)
		{
			counter++;
			if (counter >= body.Count)
			{
				counter = 0;
				Tighten();
			}
			if (!ShouldIUpdate(eu))
			{
				return;
			}
			base.Update(eu);
			if (lightAdaption < hunt)
			{
				lightAdaption = Mathf.Min(hunt, lightAdaption + 0.00083333335f);
			}
			else if (lightAdaption > hunt)
			{
				lightAdaption = Mathf.Max(hunt, lightAdaption - 0.004166667f);
			}
			if (FirstSpider != null && FirstSpider.moving)
			{
				walkCycle -= 2;
			}
			else
			{
				walkCycle--;
			}
			for (int num = body.Count - 1; num >= 0; num--)
			{
				if (body[num].spider.centipede != this || body[num].separatedCounter > (ModManager.MMF ? 5 : 20))
				{
					if (body[num].spider.centipede == this)
					{
						AbandonSpider(body[num].spider);
					}
					body.RemoveAt(num);
				}
			}
			totalMass = 0f;
			for (int i = 0; i < body.Count; i++)
			{
				body[i].Update(i);
				totalMass += body[i].spider.mainBodyChunk.mass;
				body[i].spider.legsPosition = Mathf.Lerp(-1f, 1f, body[i].BodyFac);
				if (i > 0 && body[i].spider.iVars.size > body[i - 1].spider.iVars.size)
				{
					Spider spider = body[i].spider;
					Spider spider2 = body[i - 1].spider;
					body[i].spider = spider2;
					body[i - 1].spider = spider;
					body[i].inRightPlace = false;
					body[i - 1].inRightPlace = false;
					break;
				}
				if (i >= 3 && body[i - 2].inRightPlace && body[i - 1].inRightPlace && body[i].inRightPlace)
				{
					body[i].spider.mainBodyChunk.vel -= Custom.DirVec(body[i].spider.mainBodyChunk.pos, body[i - 2].spider.mainBodyChunk.pos);
					body[i - 2].spider.mainBodyChunk.vel += Custom.DirVec(body[i].spider.mainBodyChunk.pos, body[i - 2].spider.mainBodyChunk.pos);
				}
			}
			ConsiderCreature();
			if (prey != null && FirstSpider != null)
			{
				if (prey.room != FirstSpider.room || totalMass < prey.TotalMass)
				{
					prey = null;
				}
				else if (FirstSpider.VisualContact(prey.mainBodyChunk.pos))
				{
					preyPos = prey.mainBodyChunk.pos;
					preyVisualCounter = 0;
				}
			}
			if (preyVisualCounter < 100 && prey != null)
			{
				hunt = Mathf.Min(hunt + 0.005f, 1f);
			}
			else
			{
				hunt = Mathf.Max(hunt - 0.005f, 0f);
			}
			preyVisualCounter++;
		}

		public CentipedePart BodyPart(Spider spd)
		{
			for (int num = body.Count - 1; num >= 0; num--)
			{
				if (body[num].spider == spd)
				{
					return body[num];
				}
			}
			CentipedePart centipedePart = new CentipedePart(this, spd);
			body.Add(centipedePart);
			return centipedePart;
		}

		public void Tighten()
		{
			for (int i = 1; i < body.Count; i++)
			{
				if (body[i].inRightPlace)
				{
					Spider spider = body[i].spider;
					Spider spider2 = body[i - 1].spider;
					Vector2 vector = Custom.DirVec(spider.mainBodyChunk.pos, spider2.mainBodyChunk.pos);
					float num = Vector2.Distance(spider.mainBodyChunk.pos, spider2.mainBodyChunk.pos);
					spider.mainBodyChunk.pos += vector * (num - spider2.connectDistance) * 0.5f;
					spider.mainBodyChunk.vel += vector * (num - spider2.connectDistance) * 0.5f;
					spider2.mainBodyChunk.pos -= vector * (num - spider2.connectDistance) * 0.5f;
					spider2.mainBodyChunk.vel -= vector * (num - spider2.connectDistance) * 0.5f;
				}
			}
		}

		private void ConsiderCreature()
		{
			if (room.abstractRoom.creatures.Count != 0 && FirstSpider != null)
			{
				AbstractCreature abstractCreature = room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)];
				if (abstractCreature != null && !abstractCreature.slatedForDeletion && abstractCreature.realizedCreature != null && !abstractCreature.realizedCreature.inShortcut && FirstSpider.ConsiderPrey(abstractCreature.realizedCreature) && totalMass > abstractCreature.realizedCreature.TotalMass && (prey == null || (Custom.DistLess(FirstSpider.mainBodyChunk.pos, abstractCreature.realizedCreature.mainBodyChunk.pos, Vector2.Distance(FirstSpider.mainBodyChunk.pos, preyPos)) && FirstSpider.VisualContact(abstractCreature.realizedCreature.mainBodyChunk.pos))))
				{
					prey = abstractCreature.realizedCreature;
					preyVisualCounter = 0;
				}
			}
		}

		public void SeePrey(Creature creature)
		{
			if (totalMass > creature.TotalMass && (prey == null || Custom.DistLess(FirstSpider.mainBodyChunk.pos, creature.mainBodyChunk.pos, Vector2.Distance(FirstSpider.mainBodyChunk.pos, preyPos))))
			{
				prey = creature;
			}
			if (creature == prey)
			{
				preyVisualCounter = 0;
				preyPos = prey.mainBodyChunk.pos;
			}
		}
	}

	public Vector2 direction;

	public MovementConnection lastFollowingConnection;

	public MovementConnection followingConnection;

	public MovementConnection lastShortCut;

	public bool inAccessibleTerrain;

	public int outsideAccessibleCounter;

	public float lightToMove;

	private List<MovementConnection> path;

	private int pathCount;

	private List<MovementConnection> scratchPath;

	private int scratchPathCount;

	public Flock flock;

	public Centipede centipede;

	public Centipede bannedCentipede;

	public IndividualVariations iVars;

	public int idleCounter;

	public bool idle;

	public bool moving;

	public Vector2 dragPos;

	public int noCentipedeCounter;

	public float connectDistance;

	public float bloodLust;

	public int seenNoPreyCounter;

	public float lightExp;

	public BodyChunk graphicsAttachedToBodyChunk;

	public float legsPosition;

	public float deathSpasms = 1f;

	public WorldCoordinate? denPos;

	public int denMovement;

	public Vector2? moveAwayFromPos;

	public bool WantToFormCentipede
	{
		get
		{
			if (base.Consious && noCentipedeCounter < 1 && Mathf.Pow(UnityEngine.Random.value, 0.3f) < bloodLust && UnityEngine.Random.value > lightExp)
			{
				return denMovement == 0;
			}
			return false;
		}
	}

	private void GenerateIVars()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.abstractCreature.ID.RandomSeed);
		iVars = new IndividualVariations(UnityEngine.Random.value);
		UnityEngine.Random.state = state;
	}

	public Spider(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		GenerateIVars();
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), Mathf.Lerp(2f, 9f, iVars.size), Mathf.Lerp(0.035f, 0.15f, iVars.size));
		bodyChunkConnections = new BodyChunkConnection[0];
		base.GoThroughFloors = true;
		base.airFriction = 0.99f;
		base.gravity = 0.8f;
		bounce = 0f;
		surfaceFriction = 0.87f;
		collisionLayer = 0;
		base.waterFriction = 0.92f;
		base.buoyancy = 0.95f;
		direction = Custom.DegToVec(UnityEngine.Random.value * 360f);
		path = new List<MovementConnection>();
		pathCount = 0;
		scratchPath = new List<MovementConnection>();
		scratchPathCount = 0;
		connectDistance = Mathf.Lerp(6f, 12f, iVars.size);
		ChangeCollisionLayer(0);
		if (abstractCreature.pos.NodeDefined && world.GetNode(abstractCreature.pos).type == AbstractRoomNode.Type.Den)
		{
			denPos = abstractCreature.pos.WashTileData();
		}
		if (world.rainCycle.CycleStartUp < 0.5f)
		{
			denMovement = -1;
		}
		else if (world.rainCycle.TimeUntilRain < 40 * (world.game.IsStorySession ? 60 : 15))
		{
			denMovement = 1;
		}
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new SpiderGraphics(this);
		}
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		ChangeCollisionLayer(0);
		ResetFlock();
	}

	public void ResetFlock()
	{
		flock = null;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		graphicsAttachedToBodyChunk = null;
		dragPos = base.mainBodyChunk.pos + Custom.DirVec(base.mainBodyChunk.pos, dragPos) * connectDistance;
		if (outsideAccessibleCounter > 0)
		{
			outsideAccessibleCounter--;
		}
		if (room == null)
		{
			return;
		}
		if (noCentipedeCounter > 0)
		{
			noCentipedeCounter--;
		}
		if (base.dead)
		{
			deathSpasms = Mathf.Max(0f, deathSpasms - 1f / Mathf.Lerp(200f, 400f, UnityEngine.Random.value));
		}
		IntVector2 tilePosition = room.GetTilePosition(base.mainBodyChunk.pos);
		tilePosition.x = Custom.IntClamp(tilePosition.x, 0, room.TileWidth - 1);
		tilePosition.y = Custom.IntClamp(tilePosition.y, 0, room.TileHeight - 1);
		bool flag = room.aimap.TileAccessibleToCreature(tilePosition, base.Template);
		if (!ModManager.MMF || UnityEngine.Random.value < 0.15f)
		{
			lightExp = room.LightSourceExposure(base.mainBodyChunk.pos);
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		moving = false;
		if (!base.Consious)
		{
			return;
		}
		ConsiderCreature();
		if ((followingConnection == default(MovementConnection) || followingConnection.DestTile != tilePosition) && room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
		{
			base.mainBodyChunk.vel += Custom.IntVector2ToVector2(room.ShorcutEntranceHoleDirection(tilePosition)) * 8f;
		}
		else if (!room.IsPositionInsideBoundries(tilePosition) && flag)
		{
			outsideAccessibleCounter = 5;
			followingConnection = new MovementConnection(MovementConnection.MovementType.Standard, base.abstractCreature.pos, room.GetWorldCoordinate(tilePosition), 1);
		}
		if (room.world.rainCycle.TimeUntilRain < 40 * (room.game.IsStorySession ? 60 : 15) && UnityEngine.Random.value < 0.0125f)
		{
			denMovement = 1;
		}
		if (denMovement != 0)
		{
			if (denMovement == -1 && UnityEngine.Random.value < 1f / Mathf.Lerp(1200f, 400f, room.world.rainCycle.CycleStartUp))
			{
				denMovement = 0;
			}
			if (denMovement == 1 && denPos.HasValue)
			{
				for (int i = 0; i < 4; i++)
				{
					for (int j = 1; j < 3; j++)
					{
						if (room.GetTile(base.abstractCreature.pos.Tile + Custom.fourDirections[i] * j).Terrain == Room.Tile.TerrainType.ShortcutEntrance && room.shortcutData(base.abstractCreature.pos.Tile + Custom.fourDirections[i] * j).destNode == denPos.Value.abstractNode)
						{
							enteringShortCut = base.abstractCreature.pos.Tile + Custom.fourDirections[i] * j;
						}
					}
				}
			}
			if (denPos.HasValue && denPos.Value.room != room.abstractRoom.index)
			{
				denPos = null;
			}
			if (!denPos.HasValue)
			{
				denMovement = 0;
			}
		}
		if (!denPos.HasValue)
		{
			int num = UnityEngine.Random.Range(0, room.abstractRoom.nodes.Length);
			if (room.abstractRoom.nodes[num].type == AbstractRoomNode.Type.Den && room.aimap.CreatureSpecificAImap(base.Template).GetDistanceToExit(base.abstractCreature.pos.x, base.abstractCreature.pos.y, room.abstractRoom.CommonToCreatureSpecificNodeIndex(num, base.Template)) > -1)
			{
				denPos = new WorldCoordinate(room.abstractRoom.index, -1, -1, num);
			}
		}
		if (moveAwayFromPos.HasValue && (UnityEngine.Random.value < 0.0125f || !Custom.DistLess(base.mainBodyChunk.pos, moveAwayFromPos.Value, 150f)))
		{
			moveAwayFromPos = null;
		}
		if (flock == null)
		{
			flock = new Flock(this, room);
		}
		else
		{
			flock.Update(eu);
		}
		inAccessibleTerrain = (followingConnection == default(MovementConnection) || followingConnection.type != MovementConnection.MovementType.DropToFloor) && (outsideAccessibleCounter > 0 || flag);
		if (followingConnection == default(MovementConnection) && !flag)
		{
			for (int k = 0; k < 4; k++)
			{
				if (room.aimap.TileAccessibleToCreature(tilePosition + Custom.fourDirections[k], base.Template))
				{
					base.mainBodyChunk.vel += Custom.fourDirections[k].ToVector2() * 0.5f;
					break;
				}
			}
		}
		bool flag2 = false;
		if (centipede != null)
		{
			bannedCentipede = centipede;
			centipede.Update(eu);
			if (centipede != null && centipede.spiders.Count < 2)
			{
				centipede = null;
			}
			if (centipede != null && base.grasps[0] == null && !TryToAttatch() && centipede.FirstSpider != this)
			{
				Assist();
				flag2 = true;
			}
			if (centipede != null && (UnityEngine.Random.value < 1f / Mathf.Lerp(1f, 3000f, Custom.SCurve(bloodLust, 0.05f)) || denMovement != 0))
			{
				noCentipedeCounter = Math.Max(noCentipedeCounter, 20);
				centipede.AbandonSpider(this);
			}
		}
		else
		{
			legsPosition = 0f;
		}
		if (base.grasps[0] != null)
		{
			Attached();
			return;
		}
		if (base.Consious && WantToFormCentipede && flock.spiders.Count > 0)
		{
			Spider spider = flock.spiders[UnityEngine.Random.Range(0, flock.spiders.Count)];
			if (spider != this && spider.WantToFormCentipede && (spider.centipede != centipede || centipede == null) && (bannedCentipede == null || spider.centipede != bannedCentipede) && Custom.DistLess(base.mainBodyChunk.pos, spider.mainBodyChunk.pos, 30f) && room.VisualContact(base.mainBodyChunk.pos, spider.mainBodyChunk.pos))
			{
				FormCentipede(spider);
			}
		}
		if (flag2)
		{
			return;
		}
		if (base.Consious && inAccessibleTerrain)
		{
			base.mainBodyChunk.vel *= 0.7f;
			base.mainBodyChunk.vel.y += base.gravity;
			if (centipede == null || centipede.FirstSpider == this)
			{
				Crawl();
			}
		}
		else
		{
			followingConnection = default(MovementConnection);
			if (pathCount > 0)
			{
				pathCount = 0;
			}
		}
	}

	private void FormCentipede(Spider otherSpider)
	{
		if (centipede == null)
		{
			if (otherSpider.centipede != null)
			{
				if (otherSpider.centipede.spiders.Count + 1 < otherSpider.centipede.maxSize)
				{
					otherSpider.centipede.AddSpider(this);
				}
			}
			else
			{
				centipede = new Centipede(this, room);
				centipede.AddSpider(otherSpider);
			}
		}
		else if (otherSpider.centipede == null)
		{
			if (centipede.spiders.Count + 1 < centipede.maxSize)
			{
				centipede.AddSpider(otherSpider);
			}
		}
		else if (centipede.spiders.Count + otherSpider.centipede.spiders.Count < centipede.maxSize)
		{
			centipede.Merge(otherSpider.centipede);
		}
	}

	private void Crawl()
	{
		if (!room.IsPositionInsideBoundries(room.GetTilePosition(base.mainBodyChunk.pos)))
		{
			Die();
			return;
		}
		if (lightExp == 0f && (centipede == null || centipede.hunt == 0f) && denMovement == 0 && !moveAwayFromPos.HasValue)
		{
			idleCounter++;
			if (!idle && idleCounter > 10)
			{
				idle = true;
				lightToMove = ((Mathf.Pow(UnityEngine.Random.value, 1f + iVars.size * 2f) < 0.8f) ? 0f : (Mathf.Pow(UnityEngine.Random.value, 0.5f + iVars.size * 2f) * 0.95f));
			}
		}
		else if ((!ModManager.MMF || (double)UnityEngine.Random.value <= 0.15) && (lightExp > lightToMove || (centipede != null && centipede.hunt > 0f) || denMovement != 0 || moveAwayFromPos.HasValue))
		{
			idleCounter = 0;
			idle = false;
		}
		if (idle)
		{
			if (followingConnection != default(MovementConnection))
			{
				Move(followingConnection);
				if (room.GetTilePosition(base.mainBodyChunk.pos) == followingConnection.DestTile)
				{
					followingConnection = default(MovementConnection);
				}
			}
			else
			{
				if (lightExp != 0f || !(UnityEngine.Random.value < 1f / 12f))
				{
					return;
				}
				AItile aItile = room.aimap.getAItile(base.mainBodyChunk.pos);
				if (aItile.outgoingPaths.Count > 0)
				{
					MovementConnection connection = aItile.outgoingPaths[UnityEngine.Random.Range(0, aItile.outgoingPaths.Count)];
					if (connection.type != MovementConnection.MovementType.DropToFloor && room.aimap.IsConnectionAllowedForCreature(connection, base.Template) && room.LightSourceExposure(room.MiddleOfTile(connection.DestTile)) == 0f)
					{
						followingConnection = connection;
					}
				}
			}
			return;
		}
		if (lightExp > 0f || (centipede != null && centipede.hunt > 0f) || denMovement != 0 || moveAwayFromPos.HasValue)
		{
			scratchPathCount = CreateRandomPath(ref scratchPath);
			if (ScoreOfPath(scratchPath, scratchPathCount) > ScoreOfPath(path, pathCount))
			{
				List<MovementConnection> list = path;
				int num = pathCount;
				path = scratchPath;
				pathCount = scratchPathCount;
				scratchPath = list;
				scratchPathCount = num;
			}
		}
		if (followingConnection != default(MovementConnection) && followingConnection.type != 0)
		{
			if (lastFollowingConnection != followingConnection)
			{
				outsideAccessibleCounter = 20;
			}
			if (followingConnection != default(MovementConnection))
			{
				lastFollowingConnection = followingConnection;
			}
			Move(followingConnection);
			if (room.GetTilePosition(base.mainBodyChunk.pos) != followingConnection.DestTile)
			{
				return;
			}
		}
		else if (followingConnection != default(MovementConnection))
		{
			lastFollowingConnection = followingConnection;
		}
		if (pathCount > 0)
		{
			followingConnection = default(MovementConnection);
			for (int num2 = pathCount - 1; num2 >= 0; num2--)
			{
				if (base.abstractCreature.pos.Tile == path[num2].StartTile)
				{
					followingConnection = path[num2];
					break;
				}
			}
			if (followingConnection == default(MovementConnection))
			{
				pathCount = 0;
			}
		}
		if (!(followingConnection != default(MovementConnection)))
		{
			return;
		}
		if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
		{
			enteringShortCut = followingConnection.StartTile;
			if (base.safariControlled)
			{
				bool flag = false;
				List<IntVector2> list2 = new List<IntVector2>();
				ShortcutData[] shortcuts = room.shortcuts;
				for (int i = 0; i < shortcuts.Length; i++)
				{
					ShortcutData shortcutData = shortcuts[i];
					if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != followingConnection.StartTile)
					{
						list2.Add(shortcutData.StartTile);
					}
					if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == followingConnection.StartTile)
					{
						flag = true;
					}
				}
				if (flag)
				{
					if (list2.Count > 0)
					{
						list2.Shuffle();
						NPCTransportationDestination = room.GetWorldCoordinate(list2[0]);
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
			lastShortCut = followingConnection;
			followingConnection = default(MovementConnection);
		}
		else if (followingConnection.type == MovementConnection.MovementType.Standard || followingConnection.type == MovementConnection.MovementType.DropToFloor)
		{
			Move(followingConnection);
		}
	}

	private void Assist()
	{
		Centipede.CentipedePart centipedePart = centipede.BodyPart(this);
		if (centipedePart.master == null)
		{
			return;
		}
		if (centipedePart.inRightPlace && !Custom.DistLess(base.mainBodyChunk.pos, centipedePart.master.spider.mainBodyChunk.pos, 60f))
		{
			centipedePart.inRightPlace = false;
		}
		if (centipedePart.inRightPlace)
		{
			Spider spider = centipedePart.master.spider;
			graphicsAttachedToBodyChunk = spider.mainBodyChunk;
			inAccessibleTerrain = inAccessibleTerrain || spider.inAccessibleTerrain;
			if (inAccessibleTerrain)
			{
				base.mainBodyChunk.vel *= 0.8f;
				base.mainBodyChunk.vel += Vector2.ClampMagnitude(spider.dragPos - base.mainBodyChunk.pos, 10f) / 10f;
			}
			if (spider.moving)
			{
				spider.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, spider.mainBodyChunk.pos) * (inAccessibleTerrain ? 1.5f : 0.75f);
				moving = true;
			}
			float num = (float)centipedePart.index / 6f;
			num = Mathf.Sin(((float)centipede.walkCycle / 40f + num) * (float)Math.PI * 2f);
			base.mainBodyChunk.vel += Custom.PerpendicularVector(Custom.DirVec(base.mainBodyChunk.pos, spider.mainBodyChunk.pos)) * num * 0.9f;
			Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, spider.mainBodyChunk.pos);
			float num2 = Vector2.Distance(base.mainBodyChunk.pos, spider.mainBodyChunk.pos);
			base.mainBodyChunk.pos += vector * (num2 - spider.connectDistance) * 0.5f;
			base.mainBodyChunk.vel += vector * (num2 - spider.connectDistance) * 0.5f;
			spider.mainBodyChunk.pos -= vector * (num2 - spider.connectDistance) * 0.5f;
			spider.mainBodyChunk.vel -= vector * (num2 - spider.connectDistance) * 0.5f;
		}
		else
		{
			if (inAccessibleTerrain)
			{
				base.mainBodyChunk.vel *= 0.8f;
				Move(centipedePart.master.spider.mainBodyChunk.pos);
			}
			if (Custom.DistLess(base.mainBodyChunk.pos, centipedePart.master.spider.mainBodyChunk.pos, 20f))
			{
				centipedePart.inRightPlace = true;
			}
		}
	}

	private bool TryToAttatch()
	{
		if (centipede.prey != null)
		{
			for (int i = 0; i < centipede.prey.bodyChunks.Length; i++)
			{
				if (UnityEngine.Random.value < 1f / 30f && Custom.DistLess(base.mainBodyChunk.pos, centipede.prey.bodyChunks[i].pos, base.mainBodyChunk.rad + centipede.prey.bodyChunks[i].rad))
				{
					return Grab(centipede.prey, 0, i, Grasp.Shareability.NonExclusive, 0f, overrideEquallyDominant: false, pacifying: false);
				}
				if (UnityEngine.Random.value < 0.2f && Custom.DistLess(base.mainBodyChunk.pos, centipede.prey.bodyChunks[i].pos, 60f))
				{
					base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, centipede.prey.bodyChunks[i].pos) * 4f;
					return false;
				}
			}
		}
		return false;
	}

	private void Attached()
	{
		BodyChunk bodyChunk = (graphicsAttachedToBodyChunk = base.grasps[0].grabbed.bodyChunks[base.grasps[0].chunkGrabbed]);
		if (bodyChunk.owner is Creature)
		{
			if (!(bodyChunk.owner as Creature).dead)
			{
				float num = 0f;
				if (bodyChunk.owner is Creature)
				{
					for (int i = 0; i < bodyChunk.owner.grabbedBy.Count; i++)
					{
						if (bodyChunk.owner.grabbedBy[i].grabber is Spider)
						{
							num += bodyChunk.owner.grabbedBy[i].grabber.TotalMass;
						}
					}
				}
				if (num >= bodyChunk.owner.TotalMass)
				{
					(bodyChunk.owner as Creature).Die();
				}
			}
			else if (UnityEngine.Random.value < 0.001f)
			{
				(bodyChunk.owner as Creature).leechedOut = true;
			}
		}
		if (centipede != null)
		{
			centipede.lightAdaption = 1f;
		}
		Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, bodyChunk.pos);
		float num2 = Vector2.Distance(base.mainBodyChunk.pos, bodyChunk.pos);
		float num3 = base.mainBodyChunk.rad + bodyChunk.rad;
		float num4 = base.mainBodyChunk.mass / (base.mainBodyChunk.mass + bodyChunk.mass);
		base.mainBodyChunk.vel += vector * (num2 - num3) * (1f - num4);
		base.mainBodyChunk.pos += vector * (num2 - num3) * (1f - num4);
		bodyChunk.vel -= vector * (num2 - num3) * num4;
		bodyChunk.pos -= vector * (num2 - num3) * num4;
		for (int j = 0; j < base.grasps[0].grabbed.bodyChunks.Length; j++)
		{
			PushOutOfChunk(base.grasps[0].grabbed.bodyChunks[j]);
		}
		for (int k = 0; k < base.grasps[0].grabbed.grabbedBy.Count; k++)
		{
			if (base.grasps[0].grabbed.grabbedBy[k].grabber != this)
			{
				for (int l = 0; l < base.grasps[0].grabbed.grabbedBy[k].grabber.bodyChunks.Length; l++)
				{
					PushOutOfChunk(base.grasps[0].grabbed.grabbedBy[k].grabber.bodyChunks[l]);
				}
			}
		}
		if (((bodyChunk.owner as Creature).dead && UnityEngine.Random.value < 0.01f) || UnityEngine.Random.value < 0.00083333335f || (bodyChunk.owner as Creature).enteringShortCut.HasValue || centipede == null || centipede.totalMass < bodyChunk.owner.TotalMass)
		{
			LoseAllGrasps();
		}
	}

	private void PushOutOfChunk(BodyChunk chunk)
	{
		if (Custom.DistLess(chunk.pos, base.mainBodyChunk.pos, chunk.rad + base.mainBodyChunk.rad))
		{
			Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, chunk.pos);
			float num = Vector2.Distance(base.mainBodyChunk.pos, chunk.pos);
			float num2 = base.mainBodyChunk.mass / (base.mainBodyChunk.mass + chunk.mass);
			base.mainBodyChunk.vel += vector * (num - (chunk.rad + base.mainBodyChunk.rad)) * (1f - num2);
			base.mainBodyChunk.pos += vector * (num - (chunk.rad + base.mainBodyChunk.rad)) * (1f - num2);
			chunk.vel -= vector * (num - (chunk.rad + base.mainBodyChunk.rad)) * num2;
			chunk.pos -= vector * (num - (chunk.rad + base.mainBodyChunk.rad)) * num2;
		}
	}

	private void Move(MovementConnection con)
	{
		Move(room.MiddleOfTile(con.DestTile));
	}

	private void Move(Vector2 dest)
	{
		float num = 1f;
		if (centipede != null)
		{
			num += (float)centipede.spiders.Count * 0.05f;
		}
		base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, dest) * num * 2f + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.8f, 1.2f, iVars.size) * 2f;
		moving = true;
	}

	private bool VisualContact(Vector2 pos)
	{
		if (!Custom.DistLess(base.mainBodyChunk.pos, pos, base.Template.visualRadius))
		{
			return false;
		}
		return room.VisualContact(base.mainBodyChunk.pos, pos);
	}

	private void ConsiderCreature()
	{
		seenNoPreyCounter++;
		if (room.abstractRoom.creatures.Count == 0)
		{
			return;
		}
		AbstractCreature abstractCreature = room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)];
		if (abstractCreature.realizedCreature == null || abstractCreature.slatedForDeletion || abstractCreature.realizedCreature.inShortcut)
		{
			return;
		}
		if (ConsiderPrey(abstractCreature.realizedCreature) && VisualContact(abstractCreature.realizedCreature.mainBodyChunk.pos))
		{
			bloodLust = Mathf.Clamp(bloodLust + base.Template.CreatureRelationship(abstractCreature.realizedCreature).intensity / 10f, 0f, 1f);
			seenNoPreyCounter = 0;
			if (centipede != null)
			{
				centipede.SeePrey(abstractCreature.realizedCreature);
			}
		}
		else
		{
			if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Spider && Custom.DistLess(base.mainBodyChunk.pos, abstractCreature.realizedCreature.mainBodyChunk.pos, 300f) && (Custom.DistLess(base.mainBodyChunk.pos, abstractCreature.realizedCreature.mainBodyChunk.pos, 150f) || VisualContact(abstractCreature.realizedCreature.mainBodyChunk.pos)))
			{
				if ((abstractCreature.realizedCreature as Spider).bloodLust > bloodLust)
				{
					bloodLust = Mathf.Lerp(bloodLust, (abstractCreature.realizedCreature as Spider).bloodLust, 0.3f);
				}
				if ((abstractCreature.realizedCreature as Spider).seenNoPreyCounter < seenNoPreyCounter)
				{
					seenNoPreyCounter = (abstractCreature.realizedCreature as Spider).seenNoPreyCounter;
				}
				if (centipede != null && (abstractCreature.realizedCreature as Spider).centipede != null && (abstractCreature.realizedCreature as Spider).centipede.prey != null && (abstractCreature.realizedCreature as Spider).centipede.preyVisualCounter < centipede.preyVisualCounter)
				{
					centipede.preyPos = (abstractCreature.realizedCreature as Spider).centipede.preyPos;
				}
				if (!moveAwayFromPos.HasValue && (abstractCreature.realizedCreature as Spider).moveAwayFromPos.HasValue && Custom.DistLess((abstractCreature.realizedCreature as Spider).moveAwayFromPos.Value, base.mainBodyChunk.pos, 100f))
				{
					moveAwayFromPos = (abstractCreature.realizedCreature as Spider).moveAwayFromPos.Value;
				}
			}
			if (seenNoPreyCounter > 100 + room.abstractRoom.creatures.Count)
			{
				bloodLust = Mathf.Clamp(bloodLust - 1f / 60f, 0f, 1f);
			}
			else
			{
				bloodLust = Mathf.Clamp(bloodLust - 1f / (60f * (float)room.abstractRoom.creatures.Count), 0f, 1f);
			}
		}
		if (abstractCreature.creatureTemplate.type != CreatureTemplate.Type.Spider && (centipede == null || centipede.prey != abstractCreature.realizedCreature) && Custom.DistLess(base.mainBodyChunk.pos, abstractCreature.realizedCreature.DangerPos, 30f + abstractCreature.realizedCreature.TotalMass * 8f))
		{
			moveAwayFromPos = abstractCreature.realizedCreature.DangerPos;
		}
	}

	private float ScoreOfPath(List<MovementConnection> testPath, int testPathCount)
	{
		if (testPathCount == 0)
		{
			return float.MinValue;
		}
		float num = TileScore(testPath[testPathCount - 1].DestTile);
		for (int i = 0; i < pathCount; i++)
		{
			if (path[i] == lastFollowingConnection)
			{
				num -= 1000f;
			}
		}
		return num;
	}

	public float TileScore(IntVector2 tile)
	{
		float num = 0f;
		if (moveAwayFromPos.HasValue)
		{
			num += Vector2.Distance(room.MiddleOfTile(tile), moveAwayFromPos.Value);
		}
		if (denMovement != 0 && denPos.HasValue && denPos.Value.room == room.abstractRoom.index)
		{
			int distanceToExit = room.aimap.CreatureSpecificAImap(base.Template).GetDistanceToExit(tile.x, tile.y, room.abstractRoom.CommonToCreatureSpecificNodeIndex(denPos.Value.abstractNode, base.Template));
			num = ((distanceToExit != -1) ? (num - (float)distanceToExit * 1f * (float)denMovement) : (num - 100f));
		}
		float num2 = room.LightSourceExposure(room.MiddleOfTile(tile));
		if (lightExp == 0f && num2 == 0f)
		{
			for (int i = 0; i < 5; i++)
			{
				if (room.GetTile(tile + Custom.fourDirectionsAndZero[i]).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					return float.MinValue;
				}
			}
			for (int j = 0; j < flock.spiders.Count; j++)
			{
				if (flock.spiders[j].iVars.dominance > iVars.dominance && flock.spiders[j].abstractCreature.pos.Tile == tile)
				{
					num -= 1f;
				}
			}
			num += (float)room.aimap.getAItile(tile).visibility / 800f;
			if (room.aimap.getAItile(tile).narrowSpace)
			{
				num -= 0.01f;
			}
			num -= (float)room.aimap.getTerrainProximity(tile) * 0.01f;
			if (lastShortCut != default(MovementConnection))
			{
				num -= 10f / lastShortCut.StartTile.FloatDist(tile);
				num -= 10f / lastShortCut.DestTile.FloatDist(tile);
			}
			if ((centipede == null || centipede.hunt == 0f) && bloodLust > 0f && flock != null)
			{
				for (int k = 0; (float)k < 10f * bloodLust; k++)
				{
					if (flock.spiders.Count == 0)
					{
						break;
					}
					Spider spider = flock.spiders[UnityEngine.Random.Range(0, flock.spiders.Count)];
					num = ((spider == this || !Custom.DistLess(base.mainBodyChunk.pos, spider.mainBodyChunk.pos, 200f)) ? (num - 200f * bloodLust) : (num - Vector2.Distance(base.mainBodyChunk.pos, spider.mainBodyChunk.pos) * bloodLust));
				}
			}
		}
		else
		{
			float num3 = 0f;
			if (centipede != null)
			{
				num3 = centipede.lightAdaption;
			}
			num -= Mathf.Max(num2 - num3, 0f) * 10000f;
			if (lastShortCut != default(MovementConnection) && (Custom.ManhattanDistance(tile, lastShortCut.StartTile) < 2 || Custom.ManhattanDistance(tile, lastShortCut.DestTile) < 2))
			{
				num -= 10000f;
			}
		}
		if (centipede != null && centipede.FirstSpider == this && centipede.prey != null)
		{
			num -= Vector2.Distance(room.MiddleOfTile(tile), centipede.preyPos) * centipede.hunt;
		}
		return num;
	}

	private int CreateRandomPath(ref List<MovementConnection> pth)
	{
		WorldCoordinate worldCoordinate = base.abstractCreature.pos;
		if (!room.aimap.TileAccessibleToCreature(worldCoordinate.Tile, base.Template))
		{
			for (int i = 0; i < 4; i++)
			{
				if (room.aimap.TileAccessibleToCreature(worldCoordinate.Tile + Custom.fourDirections[i], base.Template) && room.GetTile(worldCoordinate.Tile + Custom.fourDirections[i]).Terrain != Room.Tile.TerrainType.Slope)
				{
					worldCoordinate.Tile += Custom.fourDirections[i];
					break;
				}
			}
		}
		if (!room.aimap.TileAccessibleToCreature(worldCoordinate.Tile, base.Template))
		{
			return 0;
		}
		WorldCoordinate worldCoordinate2 = base.abstractCreature.pos;
		int num = 0;
		for (int j = 0; j < UnityEngine.Random.Range(5, 16); j++)
		{
			AItile aItile = room.aimap.getAItile(worldCoordinate);
			if (aItile.outgoingPaths.Count == 0)
			{
				continue;
			}
			int index = UnityEngine.Random.Range(0, aItile.outgoingPaths.Count);
			if (!room.aimap.IsConnectionAllowedForCreature(aItile.outgoingPaths[index], base.Template) || !(lastShortCut != aItile.outgoingPaths[index]) || !(worldCoordinate2 != aItile.outgoingPaths[index].destinationCoord))
			{
				continue;
			}
			bool flag = true;
			for (int k = 0; k < num; k++)
			{
				if (pth[k].startCoord == aItile.outgoingPaths[index].destinationCoord || pth[k].destinationCoord == aItile.outgoingPaths[index].destinationCoord)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				worldCoordinate2 = worldCoordinate;
				if (pth.Count <= num)
				{
					pth.Add(aItile.outgoingPaths[index]);
				}
				else
				{
					pth[num] = aItile.outgoingPaths[index];
				}
				num++;
				worldCoordinate = aItile.outgoingPaths[index].destinationCoord;
			}
		}
		return num;
	}

	public bool ConsiderPrey(Creature crit)
	{
		if (crit.TotalMass > 3.3f)
		{
			return false;
		}
		if (base.Template.CreatureRelationship(crit.Template).type != CreatureTemplate.Relationship.Type.Eats)
		{
			return false;
		}
		if (crit.leechedOut)
		{
			return false;
		}
		return true;
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
	}

	public override void Stun(int st)
	{
		if (centipede != null)
		{
			for (int i = 0; i < centipede.body.Count; i++)
			{
				if (centipede.body[i] != null)
				{
					centipede.body[i].separatedCounter = int.MaxValue;
				}
				if (i > 1000)
				{
					break;
				}
			}
		}
		base.LoseAllGrasps();
		base.Stun(st);
	}

	public override void Die()
	{
		surfaceFriction = 0.4f;
		base.Die();
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		shortcutDelay = 20;
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		base.mainBodyChunk.HardSetPosition(newRoom.MiddleOfTile(pos) - vector * 5f);
		base.mainBodyChunk.vel = vector * 5f;
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
		noCentipedeCounter = Math.Max(noCentipedeCounter, 50);
	}
}
