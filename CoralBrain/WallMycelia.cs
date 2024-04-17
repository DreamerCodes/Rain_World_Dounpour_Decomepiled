using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace CoralBrain;

public class WallMycelia : UpdatableAndDeletable, IDrawable, IOwnMycelia
{
	public CoralNeuronSystem system;

	public List<PlacedObject> places;

	public int displacedCounter;

	private bool allInPlace;

	private bool lastAllInPlace;

	private float disconnectChance;

	public Mycelium[] mycelia;

	public Vector2[] rootPositions;

	public Vector2[] directions;

	public Vector2[] randomMovements;

	public Room OwnerRoom => room;

	public PlacedObject.ResizableObjectData RezData(int i)
	{
		return places[i].data as PlacedObject.ResizableObjectData;
	}

	public WallMycelia(CoralNeuronSystem system, List<PlacedObject> places)
	{
		this.system = system;
		this.places = places;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState((int)places[0].pos.x + (int)places[0].pos.y);
		List<Vector2> list = new List<Vector2>();
		List<Vector2> list2 = new List<Vector2>();
		List<float> list3 = new List<float>();
		for (int i = 0; i < places.Count; i++)
		{
			IntVector2 tilePosition = system.room.GetTilePosition(places[i].pos);
			for (int j = tilePosition.x - (int)(RezData(i).Rad / 20f); j <= tilePosition.x + (int)(RezData(i).Rad / 20f); j++)
			{
				for (int k = tilePosition.y - (int)(RezData(i).Rad / 20f); k <= tilePosition.y + (int)(RezData(i).Rad / 20f); k++)
				{
					if (j <= 0 || j >= system.room.TileWidth || k <= 0 || k >= system.room.TileHeight || system.room.GetTile(j, k).Solid)
					{
						continue;
					}
					IntVector2 intVector = new IntVector2(j, k);
					Vector2 vector = new Vector2(0f, 0f);
					for (int l = 0; l < 8; l++)
					{
						if (!system.room.GetTile(intVector + Custom.eightDirections[l]).Solid && system.room.GetTile(intVector - Custom.eightDirections[l]).Solid)
						{
							vector += Custom.eightDirections[l].ToVector2().normalized;
						}
					}
					if (!(vector.magnitude > 0f))
					{
						continue;
					}
					int num = (int)Custom.LerpMap(Vector2.Distance(system.room.MiddleOfTile(j, k), places[i].pos), RezData(i).Rad / 3f, RezData(i).Rad, Mathf.Lerp(1f, 4f, UnityEngine.Random.value), UnityEngine.Random.value);
					for (int m = 0; m < num; m++)
					{
						Vector2 vector2 = system.room.MiddleOfTile(j, k) + new Vector2(Mathf.Lerp(-9f, 9f, UnityEngine.Random.value), Mathf.Lerp(-9f, 9f, UnityEngine.Random.value));
						vector += Custom.DirVec(places[i].pos, vector2);
						vector.Normalize();
						Vector2? vector3 = SharedPhysics.ExactTerrainRayTracePos(system.room, vector2, vector2 - vector * 20f);
						if (vector3.HasValue)
						{
							list.Add(vector3.Value);
							list2.Add(vector);
							list3.Add(Custom.LerpMap(Vector2.Distance(vector3.Value, places[i].pos), 0f, RezData(i).Rad, 20f + Custom.LerpMap(RezData(i).Rad, 50f, 500f, 20f, 180f, 0.5f) * Mathf.Pow(UnityEngine.Random.value, 0.25f), 20f));
						}
					}
				}
			}
		}
		rootPositions = list.ToArray();
		directions = list2.ToArray();
		randomMovements = new Vector2[rootPositions.Length];
		mycelia = new Mycelium[rootPositions.Length];
		for (int n = 0; n < mycelia.Length; n++)
		{
			Mycelium mycelium = new Mycelium(system, this, n, list3[n], rootPositions[n]);
			mycelia[n] = mycelium;
		}
		UnityEngine.Random.state = state;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < mycelia.Length; i++)
		{
			mycelia[i].Update();
			randomMovements[i] = Vector2.ClampMagnitude(randomMovements[i] + Custom.RNV() * 0.1f, 1f);
			if (!mycelia[i].culled)
			{
				for (int j = 1; j < Math.Min(5, mycelia[i].points.GetLength(0)); j++)
				{
					float num = (float)j / (float)(Math.Min(5, mycelia[i].points.GetLength(0)) - 1);
					mycelia[i].points[j, 2] += (directions[i] + randomMovements[i]) * Mathf.Sin(num * (float)Math.PI) * 0.75f;
				}
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[mycelia.Length];
		for (int i = 0; i < mycelia.Length; i++)
		{
			mycelia[i].InitiateSprites(i, sLeaser, rCam);
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < mycelia.Length; i++)
		{
			mycelia[i].DrawSprites(i, sLeaser, rCam, timeStacker, camPos);
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
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

	public Vector2 ConnectionPos(int index, float timeStacker)
	{
		return rootPositions[index];
	}

	public Vector2 ResetDir(int index)
	{
		return directions[index];
	}
}
