using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class ObstacleTracker : AIModule
{
	private class ObstacleObjects
	{
		private class ObstacleObject
		{
			public int reports;

			public int lifeTime;

			public PhysicalObject physicalObject;

			public Tracker.CreatureRepresentation creatureRepresentation;

			public ObstacleObject(PhysicalObject physicalObject)
			{
				this.physicalObject = physicalObject;
				reports = 1;
				lifeTime = 5;
			}
		}

		private List<ObstacleObject> obstacleObjects;

		private List<ObstacleObject> confirmedObstacles;

		private Tracker tracker;

		private int reportsToNoticeCreatures;

		public ObstacleObjects(Tracker tracker, int reportsToNoticeCreatures, int maxTrackedCreatures)
		{
			this.tracker = tracker;
			this.reportsToNoticeCreatures = reportsToNoticeCreatures;
			obstacleObjects = new List<ObstacleObject>();
			confirmedObstacles = new List<ObstacleObject>();
		}

		public void ReportObstacle(PhysicalObject obj)
		{
			bool flag = false;
			for (int num = obstacleObjects.Count - 1; num >= 0; num--)
			{
				if (obstacleObjects[num].physicalObject == obj)
				{
					obstacleObjects[num].reports++;
					obstacleObjects[num].lifeTime = Custom.IntClamp(obstacleObjects[num].reports * 5, 1, 50);
					if (obstacleObjects[num].reports == reportsToNoticeCreatures)
					{
						confirmedObstacles.Add(obstacleObjects[num]);
						obstacleObjects[num].creatureRepresentation = tracker.RepresentationForObject(obj, AddIfMissing: true);
					}
					flag = true;
				}
				else
				{
					obstacleObjects[num].lifeTime--;
					if (obstacleObjects[num].lifeTime < 1)
					{
						confirmedObstacles.Remove(obstacleObjects[num]);
						obstacleObjects.RemoveAt(num);
					}
				}
			}
			if (!flag)
			{
				obstacleObjects.Add(new ObstacleObject(obj));
			}
		}

		public int ObstacleWarning(WorldCoordinate coord)
		{
			for (int num = confirmedObstacles.Count - 1; num >= 0; num--)
			{
				if (confirmedObstacles[num].creatureRepresentation != null && !confirmedObstacles[num].creatureRepresentation.deleteMeNextFrame && confirmedObstacles[num].creatureRepresentation.BestGuessForPosition().room == coord.room && confirmedObstacles[num].creatureRepresentation.EstimatedChanceOfFinding > 0.05f)
				{
					if (Math.Abs(coord.x - confirmedObstacles[num].creatureRepresentation.BestGuessForPosition().x) < 2 && Math.Abs(coord.y - confirmedObstacles[num].creatureRepresentation.BestGuessForPosition().y) < 2)
					{
						return confirmedObstacles[num].reports;
					}
				}
				else
				{
					obstacleObjects.Remove(confirmedObstacles[num]);
					confirmedObstacles.RemoveAt(num);
				}
			}
			return 0;
		}

		public bool KnownObstacleObject(PhysicalObject obj)
		{
			for (int num = confirmedObstacles.Count - 1; num >= 0; num--)
			{
				if (confirmedObstacles[num].physicalObject == obj)
				{
					return true;
				}
			}
			return false;
		}

		public void EraseObstacleObject(PhysicalObject obj)
		{
			foreach (ObstacleObject obstacleObject in obstacleObjects)
			{
				if (obstacleObject.physicalObject == obj)
				{
					obstacleObjects.Remove(obstacleObject);
					break;
				}
			}
		}
	}

	private class ObstacleMap
	{
		private int[,] obstacleMap;

		private DebugSprite[,] DBSPRITES;

		private Room room;

		private int decayPerReport;

		private bool visualize;

		public ObstacleMap(Room room, int decayPerReport)
		{
			this.room = room;
			this.decayPerReport = decayPerReport;
			obstacleMap = new int[room.TileWidth, room.TileHeight];
			if (visualize)
			{
				DBSPRITES = new DebugSprite[room.TileWidth, room.TileHeight];
			}
		}

		private int TileReports(int x, int y)
		{
			if (x >= 0 && x < room.TileWidth && y >= 0 && y < room.TileHeight)
			{
				return obstacleMap[x, y];
			}
			return 0;
		}

		public int ObstacleWarning(MovementConnection connection)
		{
			return Math.Min(TileReports(connection.startCoord.x, connection.startCoord.y), TileReports(connection.destinationCoord.x, connection.destinationCoord.y));
		}

		public void ReportTile(int x, int y)
		{
			if (x < 0 || x >= room.TileWidth || y < 0 || y >= room.TileHeight)
			{
				return;
			}
			obstacleMap[x, y]++;
			if (visualize)
			{
				if (DBSPRITES[x, y] == null)
				{
					DebugSprite debugSprite = new DebugSprite(room.MiddleOfTile(x, y), new FSprite("pixel"), room);
					debugSprite.sprite.scale = 20f;
					debugSprite.sprite.color = new Color(1f, 0f, 0f);
					room.AddObject(debugSprite);
					DBSPRITES[x, y] = debugSprite;
				}
				DBSPRITES[x, y].sprite.alpha = (1f - 1f / (float)obstacleMap[x, y]) * 0.7f;
				DBSPRITES[x, y].sprite.color = new Color(1f, 1f - 1f / (float)obstacleMap[x, y] - 0.5f, 0f);
			}
		}

		public void Decay()
		{
			for (int i = 0; i < decayPerReport; i++)
			{
				DecayTile(UnityEngine.Random.Range(0, room.TileWidth), UnityEngine.Random.Range(0, room.TileHeight));
			}
		}

		private void DecayTile(int x, int y)
		{
			if (obstacleMap[x, y] > 0)
			{
				obstacleMap[x, y]--;
			}
			if (visualize && DBSPRITES[x, y] != null)
			{
				DBSPRITES[x, y].sprite.alpha = (1f - 1f / (float)obstacleMap[x, y]) * 0.7f;
				DBSPRITES[x, y].sprite.color = new Color(1f, 1f - 1f / (float)obstacleMap[x, y] - 0.5f, 0f);
				if (obstacleMap[x, y] < 1)
				{
					DBSPRITES[x, y].Destroy();
					DBSPRITES[x, y] = null;
				}
			}
		}
	}

	private ObstacleMap obstacleMap;

	private ObstacleObjects obstacleObjects;

	private bool trackTiles;

	private bool trackObjects;

	private int mapDecayPerReport;

	private int objectsReportsToNoticeCreatures;

	private int objectsMaxTrackedCreatures;

	public ObstacleTracker(ArtificialIntelligence AI, bool trackObjects, bool trackTiles, int mapDecayPerReport, int objectsReportsToNoticeCreatures, int objectsMaxTrackedCreatures)
		: base(AI)
	{
		this.trackObjects = trackObjects;
		this.trackTiles = trackTiles;
		this.mapDecayPerReport = mapDecayPerReport;
		this.objectsReportsToNoticeCreatures = objectsReportsToNoticeCreatures;
		this.objectsMaxTrackedCreatures = objectsMaxTrackedCreatures;
	}

	public override void NewRoom(Room room)
	{
		if (trackTiles)
		{
			obstacleMap = new ObstacleMap(room, mapDecayPerReport);
		}
		if (trackObjects)
		{
			obstacleObjects = new ObstacleObjects(AI.tracker, objectsReportsToNoticeCreatures, objectsMaxTrackedCreatures);
		}
	}

	public void ReportMovementFailure(MovementConnection connection, BodyChunk suspectedBlockingObject)
	{
		if (obstacleObjects != null && suspectedBlockingObject != null)
		{
			obstacleObjects.ReportObstacle(suspectedBlockingObject.owner);
		}
		else if (obstacleMap != null)
		{
			obstacleMap.ReportTile(connection.startCoord.x, connection.startCoord.y);
			obstacleMap.ReportTile(connection.destinationCoord.x, connection.destinationCoord.y);
			obstacleMap.Decay();
		}
	}

	public void EraseObstacleObject(PhysicalObject obj)
	{
		if (obstacleObjects != null)
		{
			obstacleObjects.EraseObstacleObject(obj);
		}
	}

	public bool KnownObstacleObject(PhysicalObject obj)
	{
		if (obstacleObjects != null)
		{
			return obstacleObjects.KnownObstacleObject(obj);
		}
		return false;
	}

	public int ObstacleWarning(MovementConnection connection)
	{
		int num = 0;
		if (obstacleMap != null)
		{
			num += obstacleMap.ObstacleWarning(connection);
		}
		if (obstacleObjects != null)
		{
			num += obstacleObjects.ObstacleWarning(connection.destinationCoord);
		}
		return num;
	}
}
