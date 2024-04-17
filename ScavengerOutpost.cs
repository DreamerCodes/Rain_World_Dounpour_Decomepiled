using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

public class ScavengerOutpost : UpdatableAndDeletable, IDrawable, INotifyWhenRoomIsReady
{
	public class GuardOutpostModule : AIModule
	{
		public ScavengerOutpost outpost;

		public ScavengerAI scavAI => AI as ScavengerAI;

		private float ScavengerInOutpostZone => Custom.LerpMap(Vector2.Distance(outpost.placedObj.pos, scavAI.scavenger.mainBodyChunk.pos), outpost.Rad, outpost.Rad + 1000f, 1f, 0f);

		public float FearDebuff
		{
			get
			{
				if (outpost == null)
				{
					return 0f;
				}
				return Custom.LerpMap(outpost.team.Count, 1f, 4f, 0.5f, 1f) * ScavengerInOutpostZone;
			}
		}

		public float AngerBuff
		{
			get
			{
				if (outpost != null)
				{
					for (int i = 0; i < outpost.playerTrackers.Count; i++)
					{
						if (outpost.playerTrackers[i].killOrder)
						{
							return 1f;
						}
					}
				}
				return 0f;
			}
		}

		public GuardOutpostModule(ArtificialIntelligence AI)
			: base(AI)
		{
		}

		public override float Utility()
		{
			if (outpost == null)
			{
				return 0f;
			}
			if (scavAI.behavior == ScavengerAI.Behavior.Travel && scavAI.creature.abstractAI.destination.room != outpost.room.abstractRoom.index)
			{
				return 0f;
			}
			return outpost.GuardDuty(scavAI.scavenger) * (1f - AngerBuff) * (1f - ScavengerInOutpostZone) * (1f - scavAI.creature.world.game.session.creatureCommunities.scavengerShyness) * ((scavAI.behavior == ScavengerAI.Behavior.GuardOutpost || scavAI.behavior == ScavengerAI.Behavior.Idle) ? 1f : 0.75f);
		}

		public float LikeModifier(AbstractCreature player)
		{
			if (outpost != null)
			{
				for (int i = 0; i < outpost.playerTrackers.Count; i++)
				{
					if (outpost.playerTrackers[i].player == player)
					{
						if (outpost.playerTrackers[i].killOrder)
						{
							return -0.5f;
						}
						if (outpost.playerTrackers[i].AllowedToPass)
						{
							return 0.65f;
						}
						return 0.35f;
					}
				}
			}
			return 0f;
		}
	}

	public class PlayerTracker
	{
		private ScavengerOutpost outpost;

		public AbstractCreature player;

		public int playerSide;

		public bool hasCommitedTransgression;

		public bool killOrder;

		public bool inRoom;

		public bool lastInRoom;

		public bool AllowedToPass
		{
			get
			{
				if (outpost.worldOutpost != null && outpost.worldOutpost.feePayed > 9)
				{
					return !killOrder;
				}
				return false;
			}
		}

		public bool PlayerOnOtherSide => playerSide == -(int)Mathf.Sign(player.realizedCreature.mainBodyChunk.pos.x - outpost.placedObj.pos.x);

		public PlayerTracker(ScavengerOutpost outpost, AbstractCreature player)
		{
			this.player = player;
			this.outpost = outpost;
		}

		public void Update()
		{
			lastInRoom = inRoom;
			if (player.realizedCreature == null || player.Room.index != outpost.room.abstractRoom.index)
			{
				playerSide = 0;
				hasCommitedTransgression = false;
				inRoom = false;
				return;
			}
			inRoom = true;
			if (!lastInRoom)
			{
				for (int i = 0; i < player.realizedCreature.grasps.Length; i++)
				{
					if (player.realizedCreature.grasps[0] != null)
					{
						outpost.AddPlayerEnterWithItem(player.realizedCreature.grasps[0].grabbed.abstractPhysicalObject);
					}
				}
				if ((player.realizedCreature as Player).objectInStomach != null)
				{
					outpost.AddPlayerEnterWithItem((player.realizedCreature as Player).objectInStomach);
				}
			}
			if (playerSide == 0)
			{
				bool flag = false;
				int num = 0;
				while (!flag && num < outpost.team.Count)
				{
					Tracker.CreatureRepresentation creatureRepresentation = outpost.team[num].AI.tracker.RepresentationForCreature(player, addIfMissing: false);
					if (creatureRepresentation != null && creatureRepresentation.TicksSinceSeen < 20)
					{
						flag = true;
					}
					num++;
				}
				if (flag)
				{
					playerSide = (int)Mathf.Sign(player.realizedCreature.mainBodyChunk.pos.x - outpost.placedObj.pos.x);
				}
			}
			if (hasCommitedTransgression)
			{
				return;
			}
			if (!AllowedToPass && PlayerOnOtherSide)
			{
				TransGression();
			}
			for (int j = 0; j < player.realizedCreature.grasps.Length; j++)
			{
				if (player.realizedCreature.grasps[j] == null)
				{
					continue;
				}
				for (int k = 0; k < outpost.outPostProperty.Count; k++)
				{
					if (!(player.realizedCreature.grasps[j].grabbed.abstractPhysicalObject.ID == outpost.outPostProperty[k].ID))
					{
						continue;
					}
					bool flag2 = false;
					if (outpost.outPostProperty[k].type == AbstractPhysicalObject.AbstractObjectType.Spear)
					{
						int num2 = 0;
						while (!flag2 && num2 < outpost.team.Count)
						{
							if (outpost.team[num2].AI.threatTracker.mostThreateningCreature != null && outpost.team[num2].AI.threatTracker.mostThreateningCreature.representedCreature != player && (Custom.DistLess(outpost.room.MiddleOfTile(outpost.team[num2].AI.threatTracker.mostThreateningCreature.BestGuessForPosition()), outpost.team[num2].mainBodyChunk.pos, 400f) || Custom.DistLess(outpost.room.MiddleOfTile(outpost.team[num2].AI.threatTracker.mostThreateningCreature.BestGuessForPosition()), player.realizedCreature.mainBodyChunk.pos, 400f)))
							{
								flag2 = true;
							}
							num2++;
						}
					}
					if (!flag2)
					{
						outpost.room.socialEventRecognizer.AddStolenProperty(outpost.outPostProperty[k].ID);
						TransGression();
					}
					break;
				}
			}
		}

		private void TransGression()
		{
			if (outpost.worldOutpost == null)
			{
				return;
			}
			Custom.Log("outpost transgression!");
			bool flag = false;
			int num = 0;
			while (!flag && num < outpost.team.Count)
			{
				Tracker.CreatureRepresentation creatureRepresentation = outpost.team[num].AI.tracker.RepresentationForCreature(player, addIfMissing: false);
				if (creatureRepresentation != null && creatureRepresentation.TicksSinceSeen < 20)
				{
					flag = true;
				}
				num++;
			}
			if (!flag)
			{
				return;
			}
			Custom.Log("transgression seen");
			hasCommitedTransgression = true;
			float num2 = outpost.room.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, outpost.room.world.RegionNumber, (player.state as PlayerState).playerNumber);
			if (num2 < 0.5f)
			{
				killOrder = true;
				outpost.worldOutpost.killSquads[(player.state as PlayerState).playerNumber]++;
			}
			if (num2 < 0.9f)
			{
				outpost.room.game.session.creatureCommunities.InfluenceLikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, outpost.room.world.RegionNumber, (player.state as PlayerState).playerNumber, Custom.LerpMap(num2, 0.9f, 0.2f, 0f, -0.3f), 0.75f, 0f);
				for (int i = 0; i < outpost.team.Count; i++)
				{
					SocialMemory.Relationship orInitiateRelationship = outpost.team[i].abstractCreature.state.socialMemory.GetOrInitiateRelationship(player.ID);
					orInitiateRelationship.tempLike = Mathf.Max(0f, orInitiateRelationship.tempLike - Custom.LerpMap(orInitiateRelationship.like, 1f, 0f, 0.1f, 2f));
				}
			}
		}
	}

	public class PearlString : UpdatableAndDeletable, IDrawable
	{
		public ScavengerOutpost outpost;

		public List<AbstractConsumable> pearls;

		public List<float> connectionRads;

		public List<bool> activeConnections;

		public int[] onAntlerPos;

		private Vector2 AttachedPos => outpost.antlers.TransformToHeadRotat(outpost.antlers.parts[onAntlerPos[0]].GetTransoformedPos(onAntlerPos[1], onAntlerPos[2]), outpost.antlerPos, Custom.VecToDeg((outpost.placedObj.data as PlacedObject.ResizableObjectData).handlePos.normalized), onAntlerPos[2], outpost.antlerFlip);

		public PearlString(Room room, ScavengerOutpost outpost, float length)
		{
			this.outpost = outpost;
			onAntlerPos = new int[3];
			onAntlerPos[0] = Random.Range(0, outpost.antlers.parts.Length);
			onAntlerPos[1] = outpost.antlers.parts[onAntlerPos[0]].positions.Length - 1;
			onAntlerPos[2] = ((!(Random.value < 0.5f)) ? 1 : (-1));
			Vector2 attachedPos = AttachedPos;
			pearls = new List<AbstractConsumable>();
			connectionRads = new List<float>();
			activeConnections = new List<bool>();
			float num = Mathf.Lerp(40f, 70f, Random.value);
			float num2 = 0f;
			for (int i = 0; (float)i < length; i++)
			{
				if (Random.value < 0.5f || i == 1)
				{
					num = Mathf.Lerp(5f, 40f, Random.value);
				}
				num2 += num;
				attachedPos.y -= num;
				DataPearl.AbstractDataPearl abstractDataPearl = new DataPearl.AbstractDataPearl(room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, room.GetWorldCoordinate(attachedPos), room.game.GetNewID(), -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Misc);
				outpost.outPostProperty.Add(abstractDataPearl);
				abstractDataPearl.destroyOnAbstraction = true;
				room.abstractRoom.entities.Add(abstractDataPearl);
				pearls.Add(abstractDataPearl);
				connectionRads.Add(num);
				activeConnections.Add(item: true);
				if (num2 > length)
				{
					break;
				}
			}
		}

		public void Initiate()
		{
			for (int i = 0; i < pearls.Count; i++)
			{
				if (pearls[i].realizedObject != null)
				{
					pearls[i].realizedObject.ChangeCollisionLayer(0);
				}
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (!room.shortCutsReady)
			{
				return;
			}
			for (int i = 0; i < pearls.Count; i++)
			{
				if (pearls[i].realizedObject == null || pearls[i].realizedObject.grabbedBy.Count > 0)
				{
					activeConnections[i] = false;
				}
			}
			for (int j = 1; j < pearls.Count; j++)
			{
				if (activeConnections[j] && activeConnections[j - 1])
				{
					float num = Vector2.Distance(PearlPos(j), PearlPos(j - 1));
					if (num > connectionRads[j])
					{
						Vector2 normalized = (PearlPos(j) - PearlPos(j - 1)).normalized;
						pearls[j].realizedObject.firstChunk.pos += normalized * (connectionRads[j] - num) * 0.5f;
						pearls[j].realizedObject.firstChunk.vel += normalized * (connectionRads[j] - num) * 0.5f;
						pearls[j - 1].realizedObject.firstChunk.pos -= normalized * (connectionRads[j] - num) * 0.5f;
						pearls[j - 1].realizedObject.firstChunk.vel -= normalized * (connectionRads[j] - num) * 0.5f;
					}
				}
			}
			Attach();
			for (int num2 = pearls.Count - 2; num2 >= 0; num2--)
			{
				if (activeConnections[num2] && activeConnections[num2 + 1])
				{
					float num = Vector2.Distance(PearlPos(num2), PearlPos(num2 + 1));
					if (num > connectionRads[num2])
					{
						Vector2 normalized = (PearlPos(num2) - PearlPos(num2 + 1)).normalized;
						pearls[num2].realizedObject.firstChunk.pos += normalized * (connectionRads[num2] - num) * 0.5f;
						pearls[num2].realizedObject.firstChunk.vel += normalized * (connectionRads[num2] - num) * 0.5f;
						pearls[num2 + 1].realizedObject.firstChunk.pos -= normalized * (connectionRads[num2] - num) * 0.5f;
						pearls[num2 + 1].realizedObject.firstChunk.vel -= normalized * (connectionRads[num2] - num) * 0.5f;
					}
				}
			}
			Attach();
		}

		private Vector2 PearlPos(int i)
		{
			return pearls[i].realizedObject.firstChunk.pos;
		}

		private void Attach()
		{
			if (activeConnections[0])
			{
				Vector2 normalized = (pearls[0].realizedObject.firstChunk.pos - AttachedPos).normalized;
				float num = Vector2.Distance(pearls[0].realizedObject.firstChunk.pos, AttachedPos);
				pearls[0].realizedObject.firstChunk.pos += normalized * (connectionRads[0] - num);
				pearls[0].realizedObject.firstChunk.vel += normalized * (connectionRads[0] - num);
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[pearls.Count];
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i] = new FSprite("pixel");
				sLeaser.sprites[i].anchorY = 0f;
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = AttachedPos;
			bool isVisible = true;
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				if (!activeConnections[i] || pearls[i].realizedObject == null || pearls[i].realizedObject.room != room)
				{
					sLeaser.sprites[i].isVisible = false;
					isVisible = false;
					continue;
				}
				sLeaser.sprites[i].isVisible = isVisible;
				isVisible = true;
				Vector2 vector2 = Vector2.Lerp(pearls[i].realizedObject.firstChunk.lastPos, pearls[i].realizedObject.firstChunk.pos, timeStacker);
				sLeaser.sprites[i].x = vector2.x - camPos.x;
				sLeaser.sprites[i].y = vector2.y - camPos.y;
				sLeaser.sprites[i].scaleY = Vector2.Distance(vector2, vector);
				sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(vector2, vector);
				vector = vector2;
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].color = palette.blackColor;
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Items");
			}
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public List<Scavenger> team;

	public PlacedObject placedObj;

	public List<IntVector2> scavAccessibleTiles;

	public List<AbstractPhysicalObject> outPostProperty;

	public List<PlayerTracker> playerTrackers;

	public ScavengersWorldAI.Outpost worldOutpost;

	public DeerGraphics.Antlers antlers;

	public Vector2 antlerPos;

	public float antlerFlip;

	private bool paintJob;

	public List<PearlString> pearlStrings;

	public List<EntityID> receivedItems = new List<EntityID>();

	public List<AbstractPhysicalObject> playerEnteredRoomWithItems = new List<AbstractPhysicalObject>();

	private bool initiated;

	public Color blackColor;

	public Color boneColor;

	public Color redColor;

	public float Rad => (placedObj.data as PlacedObject.ResizableObjectData).Rad;

	public WorldCoordinate GoToPos
	{
		get
		{
			if (scavAccessibleTiles.Count < 1)
			{
				return room.GetWorldCoordinate(placedObj.pos);
			}
			return room.GetWorldCoordinate(scavAccessibleTiles[Random.Range(0, scavAccessibleTiles.Count)]);
		}
	}

	public int PoleSprite => 0;

	private int FirstAntlerSprite => 1;

	private int LastAntlerSprite => FirstAntlerSprite + antlers.SpritesClaimed - 1;

	private int FirstAntlerDetailSprite => LastAntlerSprite + 1;

	private int LastAntlerDetailSprite => FirstAntlerDetailSprite + antlers.SpritesClaimed - 1;

	private int SkullSprite(int part)
	{
		return LastAntlerDetailSprite + 1 + part;
	}

	public ScavengerOutpost(PlacedObject placedObj, Room room)
	{
		this.placedObj = placedObj;
		base.room = room;
		if (room.world.scavengersWorldAI == null)
		{
			room.world.AddWorldProcess(new ScavengersWorldAI(room.world));
		}
		for (int i = 0; i < room.world.scavengersWorldAI.outPosts.Count; i++)
		{
			if (room.world.scavengersWorldAI.outPosts[i].room == room.abstractRoom.index)
			{
				worldOutpost = room.world.scavengersWorldAI.outPosts[i];
			}
		}
		team = new List<Scavenger>();
		scavAccessibleTiles = new List<IntVector2>();
		for (int j = room.GetTilePosition(placedObj.pos).x - Mathf.RoundToInt((placedObj.data as PlacedObject.ResizableObjectData).Rad / 20f); j <= room.GetTilePosition(placedObj.pos).x + Mathf.RoundToInt((placedObj.data as PlacedObject.ResizableObjectData).Rad / 20f); j++)
		{
			for (int k = room.GetTilePosition(placedObj.pos).y - Mathf.RoundToInt((placedObj.data as PlacedObject.ResizableObjectData).Rad / 20f); k <= room.GetTilePosition(placedObj.pos).x + Mathf.RoundToInt((placedObj.data as PlacedObject.ResizableObjectData).Rad / 20f); k++)
			{
				if (!room.GetTile(j, k).Solid && room.GetTile(j, k - 1).Solid)
				{
					scavAccessibleTiles.Add(new IntVector2(j, k));
				}
			}
		}
		outPostProperty = new List<AbstractPhysicalObject>();
		if (room.abstractRoom.firstTimeRealized && scavAccessibleTiles.Count > 0)
		{
			for (int num = Random.Range(12, 24); num >= 0; num--)
			{
				IntVector2 intVector = scavAccessibleTiles[Random.Range(0, scavAccessibleTiles.Count)];
				AbstractPhysicalObject abstractPhysicalObject = new AbstractSpear(room.world, null, new WorldCoordinate(room.abstractRoom.index, intVector.x, intVector.y, -1), room.game.GetNewID(), explosive: false);
				outPostProperty.Add(abstractPhysicalObject);
				room.abstractRoom.AddEntity(abstractPhysicalObject);
			}
		}
		playerTrackers = new List<PlayerTracker>();
		for (int l = 0; l < room.game.Players.Count; l++)
		{
			playerTrackers.Add(new PlayerTracker(this, room.game.Players[l]));
		}
		antlerFlip = Mathf.Lerp(-0.8f, 0.8f, (placedObj.data as PlacedObject.ScavengerOutpostData).direction);
		Random.State state = Random.state;
		Random.InitState((placedObj.data as PlacedObject.ScavengerOutpostData).skullSeed);
		antlers = new DeerGraphics.Antlers(70f, 0.7f);
		antlerPos = placedObj.pos + (placedObj.data as PlacedObject.ResizableObjectData).handlePos + (placedObj.data as PlacedObject.ResizableObjectData).handlePos.normalized * antlers.rad;
		paintJob = Random.value < 0.5f;
		Random.InitState((placedObj.data as PlacedObject.ScavengerOutpostData).pearlsSeed);
		pearlStrings = new List<PearlString>();
		int num2 = Random.Range(5, 15);
		for (int m = 0; m < num2; m++)
		{
			PearlString pearlString = new PearlString(room, this, 20f + Mathf.Lerp(20f, 150f, Random.value) * Custom.LerpMap(num2, 5f, 15f, 1f, 0.1f));
			room.AddObject(pearlString);
			pearlStrings.Add(pearlString);
		}
		Random.state = state;
		if (!Futile.atlasManager.DoesContainAtlas("outpostSkulls"))
		{
			Futile.atlasManager.LoadAtlas("Atlases/outPostSkulls");
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!initiated && room.fullyLoaded)
		{
			team.Clear();
			for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
			{
				if (room.abstractRoom.creatures[i].realizedCreature != null && room.abstractRoom.creatures[i].realizedCreature is Scavenger && ScavToBeTracked(room.abstractRoom.creatures[i].realizedCreature as Scavenger))
				{
					(room.abstractRoom.creatures[i].realizedCreature as Scavenger).AI.outpostModule.outpost = this;
					team.Add(room.abstractRoom.creatures[i].realizedCreature as Scavenger);
					SortTeam();
				}
			}
			for (int j = 0; j < pearlStrings.Count; j++)
			{
				pearlStrings[j].Initiate();
			}
			initiated = true;
		}
		AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
		for (int num = playerEnteredRoomWithItems.Count - 1; num >= 0; num--)
		{
			if (playerEnteredRoomWithItems[num].realizedObject != null)
			{
				if (playerEnteredRoomWithItems[num].realizedObject.grabbedBy.Count > 0 && playerEnteredRoomWithItems[num].realizedObject.grabbedBy[0].grabber is Scavenger && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
				{
					FeeRecieved(firstAlivePlayer.realizedCreature as Player, playerEnteredRoomWithItems[num], (playerEnteredRoomWithItems[num].realizedObject.grabbedBy[0].grabber as Scavenger).AI.CollectScore(playerEnteredRoomWithItems[num].realizedObject, weaponFiltered: false));
					playerEnteredRoomWithItems.RemoveAt(num);
				}
				else if (playerEnteredRoomWithItems[num].realizedObject.room != null && playerEnteredRoomWithItems[num].realizedObject.room != room)
				{
					playerEnteredRoomWithItems.RemoveAt(num);
				}
			}
		}
		if (room.abstractRoom.creatures.Count > 0)
		{
			AbstractCreature abstractCreature = room.abstractRoom.creatures[Random.Range(0, room.abstractRoom.creatures.Count)];
			if (abstractCreature.realizedCreature != null && abstractCreature.realizedCreature is Scavenger && ScavToBeTracked(abstractCreature.realizedCreature as Scavenger) && !team.Contains(abstractCreature.realizedCreature as Scavenger))
			{
				(abstractCreature.realizedCreature as Scavenger).AI.outpostModule.outpost = this;
				team.Add(abstractCreature.realizedCreature as Scavenger);
				SortTeam();
			}
		}
		for (int num2 = team.Count - 1; num2 >= 0; num2--)
		{
			if (!ScavToBeTracked(team[num2]))
			{
				if (team[num2].AI.outpostModule.outpost == this)
				{
					team[num2].AI.outpostModule.outpost = null;
				}
				team.RemoveAt(num2);
				SortTeam();
			}
		}
		if (room.abstractRoom.entities.Count > 0)
		{
			AbstractWorldEntity abstractWorldEntity = room.abstractRoom.entities[Random.Range(0, room.abstractRoom.entities.Count)];
			if (abstractWorldEntity is AbstractPhysicalObject && (abstractWorldEntity as AbstractPhysicalObject).type == AbstractPhysicalObject.AbstractObjectType.Spear && (abstractWorldEntity as AbstractPhysicalObject).realizedObject != null && (abstractWorldEntity as AbstractPhysicalObject).stuckObjects.Count == 0 && Custom.DistLess((abstractWorldEntity as AbstractPhysicalObject).realizedObject.firstChunk.pos, placedObj.pos, Rad) && !outPostProperty.Contains(abstractWorldEntity as AbstractPhysicalObject))
			{
				outPostProperty.Add(abstractWorldEntity as AbstractPhysicalObject);
			}
			if (outPostProperty.Count > 0)
			{
				AbstractPhysicalObject abstractPhysicalObject = outPostProperty[Random.Range(0, outPostProperty.Count)];
				if (abstractPhysicalObject.stuckObjects.Count > 0 || (abstractPhysicalObject.realizedObject != null && !Custom.DistLess(abstractPhysicalObject.realizedObject.firstChunk.pos, placedObj.pos, Rad + 200f)))
				{
					outPostProperty.Remove(abstractPhysicalObject);
				}
			}
		}
		for (int k = 0; k < playerTrackers.Count; k++)
		{
			playerTrackers[k].Update();
		}
		if (room.game.session is StoryGameSession && !(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ScavTollMessage && room.ViewedByAnyCamera(antlerPos, 20f))
		{
			(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ScavTollMessage = true;
			room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Scavenger Toll"), 0, 120, darken: true, hideHud: true);
		}
	}

	private void SortTeam()
	{
		team = team.OrderBy((Scavenger o) => 1f - o.abstractCreature.personality.dominance).ToList();
	}

	private bool ScavToBeTracked(Scavenger scav)
	{
		if (scav.room != room)
		{
			return false;
		}
		if (scav.dead)
		{
			return false;
		}
		if (scav.King)
		{
			return false;
		}
		if ((scav.abstractCreature.abstractAI as ScavengerAbstractAI).squad != null && (scav.abstractCreature.abstractAI as ScavengerAbstractAI).squad.guardOutpost != null && (scav.abstractCreature.abstractAI as ScavengerAbstractAI).squad.guardOutpost.room == room.abstractRoom.index)
		{
			return true;
		}
		if (team.Count < 7 && scav.abstractCreature.abstractAI.MigrationDestination.room == room.abstractRoom.index && scav.AI.behavior != ScavengerAI.Behavior.Travel)
		{
			return true;
		}
		return false;
	}

	public float GuardDuty(Scavenger scav)
	{
		return Custom.LerpMap(team.IndexOf(scav), 0f, 5f, 0.9f, 0.1f);
	}

	public void FeeRecieved(Player player, AbstractPhysicalObject item, int value)
	{
		if (worldOutpost == null)
		{
			return;
		}
		for (int i = 0; i < receivedItems.Count; i++)
		{
			if (receivedItems[i] == item.ID)
			{
				Custom.Log("already received this payment");
				return;
			}
		}
		receivedItems.Add(item.ID);
		for (int j = 0; j < room.socialEventRecognizer.stolenProperty.Count; j++)
		{
			if (item.ID == room.socialEventRecognizer.stolenProperty[j])
			{
				Custom.Log("Can't pay with stolen stuff!");
				return;
			}
		}
		worldOutpost.feePayed += value;
		Custom.Log("player payed fee", value.ToString(), worldOutpost.feePayed.ToString());
		if (!outPostProperty.Contains(item))
		{
			outPostProperty.Add(item);
		}
		if (worldOutpost.feePayed > 9)
		{
			float val = room.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, room.game.world.RegionNumber, (player.abstractCreature.state as PlayerState).playerNumber);
			room.game.session.creatureCommunities.InfluenceLikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, room.game.world.RegionNumber, (player.abstractCreature.state as PlayerState).playerNumber, Custom.LerpMap(val, -0.9f, 0f, 0.01f, 0.2f), 0.75f, 0f);
			for (int k = 0; k < playerTrackers.Count; k++)
			{
				if (playerTrackers[k].player != player.abstractCreature)
				{
					continue;
				}
				if (playerTrackers[k].AllowedToPass)
				{
					for (int l = 0; l < team.Count; l++)
					{
						SocialMemory.Relationship orInitiateRelationship = team[l].abstractCreature.state.socialMemory.GetOrInitiateRelationship(player.abstractCreature.ID);
						orInitiateRelationship.tempLike = Mathf.Max(orInitiateRelationship.tempLike, 0.85f);
					}
				}
				break;
			}
		}
		for (int m = 0; m < team.Count; m++)
		{
			if (team[m].animation is Scavenger.BackOffAnimation)
			{
				Custom.Log("scavenger stop back off anim");
				(team[m].animation as Scavenger.BackOffAnimation).discontinue = true;
			}
		}
	}

	public void ScavengerReportTransgression(Player player)
	{
		if (player.scavengerImmunity > 0)
		{
			return;
		}
		for (int i = 0; i < playerTrackers.Count; i++)
		{
			if (playerTrackers[i].player == player.abstractCreature)
			{
				playerTrackers[i].killOrder = true;
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1 + antlers.SpritesClaimed * 2 + 3];
		sLeaser.sprites[PoleSprite] = new FSprite("Futile_White");
		sLeaser.sprites[PoleSprite].scaleX = 0.5f;
		sLeaser.sprites[PoleSprite].scaleY = (placedObj.data as PlacedObject.ResizableObjectData).handlePos.magnitude * 2f / 16f;
		sLeaser.sprites[PoleSprite].shader = rCam.game.rainWorld.Shaders["JaggedSquare"];
		sLeaser.sprites[PoleSprite].alpha = 0f;
		sLeaser.sprites[PoleSprite].rotation = Custom.VecToDeg((placedObj.data as PlacedObject.ResizableObjectData).handlePos.normalized);
		int num = Custom.IntClamp(Mathf.RoundToInt(Mathf.Abs(antlerFlip) * 4f) + 1, 1, 4);
		for (int i = 0; i < 3; i++)
		{
			sLeaser.sprites[SkullSprite(i)] = new FSprite("skull" + num + "_" + (((!paintJob || i != 2) ? 1 : 2) + i));
			sLeaser.sprites[SkullSprite(i)].anchorY = 0.85f;
			sLeaser.sprites[SkullSprite(i)].rotation = Custom.VecToDeg((placedObj.data as PlacedObject.ResizableObjectData).handlePos.normalized) + (float)num * 2.5f * Mathf.Sign(antlerFlip);
			sLeaser.sprites[SkullSprite(i)].scaleX = 0f - Mathf.Sign(antlerFlip);
		}
		antlers.InitiateSprites(FirstAntlerSprite, sLeaser, rCam);
		antlers.InitiateSprites(FirstAntlerDetailSprite, sLeaser, rCam);
		for (int j = FirstAntlerDetailSprite; j <= LastAntlerDetailSprite; j++)
		{
			sLeaser.sprites[j].shader = rCam.game.rainWorld.Shaders["OutPostAntler"];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[PoleSprite].x = placedObj.pos.x - camPos.x;
		sLeaser.sprites[PoleSprite].y = placedObj.pos.y - camPos.y;
		Vector2 vector = placedObj.pos + (placedObj.data as PlacedObject.ResizableObjectData).handlePos;
		for (int i = 0; i < 3; i++)
		{
			sLeaser.sprites[SkullSprite(i)].x = vector.x - camPos.x;
			sLeaser.sprites[SkullSprite(i)].y = vector.y - camPos.y;
		}
		antlers.DrawSprites(FirstAntlerSprite, sLeaser, rCam, timeStacker, camPos, placedObj.pos, antlerPos, antlerFlip, boneColor, boneColor);
		antlers.DrawSprites(FirstAntlerDetailSprite, sLeaser, rCam, timeStacker, camPos, placedObj.pos, antlerPos, antlerFlip, redColor, redColor);
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		sLeaser.sprites[0].color = palette.blackColor;
		boneColor = Color.Lerp(palette.blackColor, new Color(0.9f, 0.9f, 0.8f), Mathf.Lerp(0.9f, 0.2f, rCam.room.Darkness(placedObj.pos)));
		redColor = Color.Lerp(palette.blackColor, Color.Lerp(boneColor, new Color(1f, 0f, 0f), 0.9f), Mathf.Lerp(0.5f, 0.15f, rCam.room.Darkness(placedObj.pos)));
		sLeaser.sprites[SkullSprite(0)].color = Color.Lerp(Color.Lerp(boneColor, new Color(0.6f, 0.5f, 0.1f), 0.3f), blackColor, Mathf.Lerp(0.6f, 1f, rCam.room.Darkness(placedObj.pos)));
		sLeaser.sprites[SkullSprite(1)].color = boneColor;
		sLeaser.sprites[SkullSprite(2)].color = redColor;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		FSprite[] sprites = sLeaser.sprites;
		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i].RemoveFromContainer();
		}
		for (int j = FirstAntlerSprite; j < FirstAntlerSprite + antlers.parts.Length; j++)
		{
			newContatiner.AddChild(sLeaser.sprites[j]);
		}
		for (int k = FirstAntlerDetailSprite; k < FirstAntlerDetailSprite + antlers.parts.Length; k++)
		{
			newContatiner.AddChild(sLeaser.sprites[k]);
		}
		newContatiner.AddChild(sLeaser.sprites[PoleSprite]);
		for (int l = FirstAntlerSprite + antlers.parts.Length; l <= LastAntlerSprite; l++)
		{
			newContatiner.AddChild(sLeaser.sprites[l]);
		}
		for (int m = FirstAntlerDetailSprite + antlers.parts.Length; m <= LastAntlerDetailSprite; m++)
		{
			newContatiner.AddChild(sLeaser.sprites[m]);
		}
		for (int n = 0; n < 3; n++)
		{
			newContatiner.AddChild(sLeaser.sprites[SkullSprite(n)]);
		}
	}

	public void ShortcutsReady()
	{
	}

	public void AIMapReady()
	{
		scavAccessibleTiles.Clear();
		for (int i = room.GetTilePosition(placedObj.pos).x - Mathf.RoundToInt((placedObj.data as PlacedObject.ResizableObjectData).Rad / 20f); i <= room.GetTilePosition(placedObj.pos).x + Mathf.RoundToInt((placedObj.data as PlacedObject.ResizableObjectData).Rad / 20f); i++)
		{
			for (int j = room.GetTilePosition(placedObj.pos).y - Mathf.RoundToInt((placedObj.data as PlacedObject.ResizableObjectData).Rad / 20f); j <= room.GetTilePosition(placedObj.pos).x + Mathf.RoundToInt((placedObj.data as PlacedObject.ResizableObjectData).Rad / 20f); j++)
			{
				if (room.aimap.TileAccessibleToCreature(new IntVector2(i, j), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger)))
				{
					scavAccessibleTiles.Add(new IntVector2(i, j));
				}
			}
		}
	}

	public void AddPlayerEnterWithItem(AbstractPhysicalObject item)
	{
		for (int i = 0; i < playerEnteredRoomWithItems.Count; i++)
		{
			if (playerEnteredRoomWithItems[i].ID == item.ID)
			{
				return;
			}
		}
		playerEnteredRoomWithItems.Add(item);
		Custom.Log($"item logged as player brought: {item}");
	}
}
