using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace CoralBrain;

public class CoralCircuit : UpdatableAndDeletable
{
	public class CircuitBit : UpdatableAndDeletable, IDrawable
	{
		public List<CircuitBit> neighbors;

		public List<float> neighborDists;

		public List<float> neighborDirs;

		public bool[] activeConnections;

		public IntVector2 tile;

		public Vector2 lastPos;

		public Vector2 pos;

		public Vector2 vel;

		public Vector2 stuckPos;

		public Vector2 size;

		public Vector2 rotat;

		public Vector2 lastRotat;

		public Vector2 dRotat;

		public Vector2 lastDRotat;

		public float rotatVel;

		public float dRotatVel;

		public CoralCircuit circuit;

		public bool lonely;

		public bool lastLonely;

		public bool lastInPosition;

		public int repairCounter;

		public int noRepairNeededCounter;

		private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		private bool shader;

		private bool shaderDirty;

		public CircuitBit(CoralCircuit circuit, IntVector2 tile, Room room)
		{
			this.circuit = circuit;
			this.tile = tile;
			stuckPos = room.MiddleOfTile(tile) + new Vector2(Mathf.Lerp(-9f, 9f, UnityEngine.Random.value), Mathf.Lerp(-9f, 9f, UnityEngine.Random.value));
			pos = stuckPos;
			lastPos = pos;
			float num = 0f;
			for (int i = 0; i < circuit.places.Count; i++)
			{
				num = Mathf.Max(num, Mathf.InverseLerp((circuit.places[i].data as PlacedObject.ResizableObjectData).Rad, (circuit.places[i].data as PlacedObject.ResizableObjectData).Rad - 220f, Vector2.Distance(stuckPos, circuit.places[i].pos)));
			}
			if (num > 0.5f)
			{
				size = new Vector2(Mathf.Lerp(15f, 35f, UnityEngine.Random.value), Mathf.Lerp(15f, 35f, UnityEngine.Random.value));
			}
			else
			{
				size = new Vector2(Mathf.Lerp(5f, 25f, UnityEngine.Random.value), Mathf.Lerp(5f, 25f, UnityEngine.Random.value));
			}
			size.x = Mathf.Floor(size.x);
			size.y = Mathf.Floor(size.y);
			rotat = new Vector2(0f, 1f);
			dRotat = new Vector2(0f, 1f);
		}

		public void Connect()
		{
			neighbors = new List<CircuitBit>();
			neighborDists = new List<float>();
			neighborDirs = new List<float>();
			for (int i = 0; i < 5; i++)
			{
				for (int j = 0; j < circuit.bits.GetLength(2); j++)
				{
					CircuitBit circuitBit = circuit.Bit(tile + Custom.fourDirectionsAndZero[i], j);
					if (circuitBit != null && circuitBit != this && OverLap(circuitBit))
					{
						neighbors.Add(circuitBit);
						neighborDists.Add(Vector2.Distance(stuckPos, circuitBit.stuckPos));
						neighborDirs.Add(Custom.AimFromOneVectorToAnother(stuckPos, circuitBit.stuckPos));
					}
				}
			}
			activeConnections = new bool[neighbors.Count];
			for (int k = 0; k < activeConnections.Length; k++)
			{
				activeConnections[k] = true;
			}
		}

		public bool OverLap(CircuitBit otherBit)
		{
			if (stuckPos.x - size.x < otherBit.stuckPos.x + otherBit.size.x && stuckPos.x + size.x > otherBit.stuckPos.x - otherBit.size.x && stuckPos.y - size.y < otherBit.stuckPos.y + otherBit.size.y)
			{
				return stuckPos.y + size.y > otherBit.stuckPos.y - otherBit.size.y;
			}
			return false;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastPos = pos;
			pos += vel;
			vel *= 0.999f;
			lastRotat = rotat;
			rotat += Custom.PerpendicularVector(rotat) * rotatVel;
			rotat.Normalize();
			lastDRotat = dRotat;
			dRotat += Custom.PerpendicularVector(rotat) * dRotatVel;
			dRotat.Normalize();
			if (InPosition() != shader)
			{
				shader = InPosition();
				shaderDirty = true;
			}
			lastLonely = lonely;
			TryRepair();
			if (InPosition())
			{
				lonely = false;
				noRepairNeededCounter++;
				if (noRepairNeededCounter > 40)
				{
					repairCounter = 0;
				}
			}
			else
			{
				if (repairCounter > 100)
				{
					float num = Mathf.Pow(Mathf.InverseLerp(50f, 600f, repairCounter), 5f) * 0.2f;
					if (Custom.DistLess(pos, stuckPos, 2f))
					{
						num = Mathf.Pow(num, 0.5f);
					}
					for (int i = 0; i < neighbors.Count; i++)
					{
						if (activeConnections[i] && Custom.DistLess(neighbors[i].pos, neighbors[i].stuckPos, 2f))
						{
							num = Mathf.Pow(num, 0.5f);
						}
					}
					rotatVel *= Mathf.Lerp(1f, Custom.LerpMap(Vector2.Distance(pos, stuckPos), 40f, 140f, 0.8f, 1f), num);
					dRotatVel *= Mathf.Lerp(1f, Custom.LerpMap(Vector2.Distance(pos, stuckPos), 40f, 140f, 0.8f, 1f), num);
					rotat = Vector3.Slerp(rotat, new Vector2(0f, 1f), 0.1f * num);
					vel *= Mathf.Lerp(1f, Custom.LerpMap(Vector2.Distance(pos, stuckPos), 40f, 140f, 0.5f, 0.9f), num);
					vel += Vector2.ClampMagnitude(Custom.DirVec(pos, stuckPos) * num, Vector2.Distance(pos, stuckPos));
					if (Custom.DistLess(pos, stuckPos, Mathf.Max(1f, 10f * num)))
					{
						vel *= 0.5f;
						pos = Vector2.Lerp(pos, stuckPos, 0.2f);
						rotatVel *= 0.5f;
						dRotatVel *= 0.5f;
						rotat = Vector3.Slerp(rotat, new Vector2(0f, 1f), 0.1f);
						dRotat = Vector3.Slerp(rotat, new Vector2(0f, 1f), 0.1f);
						TryRepair();
						if (InPosition())
						{
							room.PlaySound(SoundID.Coral_Circuit_Reactivate, pos, 1f, 1f);
						}
					}
				}
				noRepairNeededCounter = 0;
				repairCounter = Math.Min(repairCounter + 1, 600);
				Vector2 vector = new Vector2(0f, 0f);
				int num2 = 0;
				if (repairCounter < 550)
				{
					for (int j = 0; j < neighbors.Count; j++)
					{
						if (!activeConnections[j])
						{
							continue;
						}
						for (int k = 0; k < neighbors[j].neighbors.Count; k++)
						{
							if (!neighbors[j].activeConnections[k] && neighbors[j].neighbors[k] == this)
							{
								room.PlaySound(SoundID.Coral_Circuit_Break, pos, 1f, 1f);
								activeConnections[j] = false;
							}
						}
						if (UnityEngine.Random.value < circuit.disconnectChance)
						{
							room.PlaySound(SoundID.Coral_Circuit_Break, pos, 1f, 1f);
							activeConnections[j] = false;
						}
						if (activeConnections[j] && Custom.DistLess(neighbors[j].pos, pos, neighborDists[j] * 1.2f))
						{
							Vector2 normalized = (neighbors[j].pos - pos).normalized;
							float num3 = Vector2.Distance(neighbors[j].pos, pos);
							float num4 = 0.45f * Mathf.InverseLerp(550f, 300f, Mathf.Max(repairCounter, neighbors[j].repairCounter));
							pos += normalized * (num3 - neighborDists[j]) * num4;
							vel += normalized * (num3 - neighborDists[j]) * num4;
							neighbors[j].pos -= normalized * (num3 - neighborDists[j]) * num4;
							neighbors[j].vel -= normalized * (num3 - neighborDists[j]) * num4;
							vector += Custom.RotateAroundOrigo(normalized, 0f - neighborDirs[j]);
							neighbors[j].TryRepair();
							num2++;
						}
						if (UnityEngine.Random.value < 0.2f && Vector2.Distance(vel, neighbors[j].vel) > 7f)
						{
							room.PlaySound(SoundID.Coral_Circuit_Break, pos, 1f, 1f);
							activeConnections[j] = false;
						}
					}
				}
				TryRepair();
				if (num2 > 0)
				{
					rotatVel = 0f;
					dRotatVel = 0f;
					dRotat = Vector3.Slerp(dRotat, new Vector2(0f, 1f), 0.5f);
					rotat = vector.normalized;
					lonely = false;
				}
				else
				{
					lonely = true;
				}
				if (repairCounter < 550 && room.aimap.getTerrainProximity(pos) < 2)
				{
					SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(pos, lastPos, vel, Mathf.Min(size.x, size.y), new IntVector2(0, 0), goThroughFloors: true);
					cd = SharedPhysics.VerticalCollision(room, cd);
					cd = SharedPhysics.HorizontalCollision(room, cd);
					pos = cd.pos;
					vel = cd.vel;
					if (cd.contactPoint.x != 0)
					{
						vel.x = Mathf.Abs(vel.x) * (float)(-cd.contactPoint.x);
					}
					if (cd.contactPoint.y != 0)
					{
						vel.y = Mathf.Abs(vel.y) * (float)(-cd.contactPoint.y);
					}
					if (cd.contactPoint.x != 0 || cd.contactPoint.y != 0)
					{
						rotatVel *= 1f + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * vel.magnitude * 0.1f;
					}
					if (room.aimap.getTerrainProximity(pos) < 2)
					{
						IntVector2 tilePosition = room.GetTilePosition(pos);
						Vector2 vector2 = new Vector2(0f, 0f);
						for (int l = 0; l < 4; l++)
						{
							if (!room.GetTile(tilePosition + Custom.fourDirections[l]).Solid && !room.aimap.getAItile(tilePosition + Custom.fourDirections[l]).narrowSpace)
							{
								float num5 = 0f;
								for (int m = 0; m < 4; m++)
								{
									num5 += (float)Mathf.Min(4, room.aimap.getTerrainProximity(tilePosition + Custom.fourDirections[l] + Custom.fourDirections[m]));
								}
								vector2 += Custom.fourDirections[l].ToVector2() * num5;
							}
						}
						vel += vector2.normalized * 0.1f;
					}
				}
			}
			if (lonely && !lastLonely)
			{
				rotatVel = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Mathf.Clamp(vel.magnitude, 1f, 5f) * 0.02f;
				dRotatVel = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Mathf.Clamp(vel.magnitude, 1f, 5f) * 0.04f;
			}
			if (!InPosition())
			{
				circuit.displacedCounter++;
			}
		}

		public void TryRepair()
		{
			if (!Custom.DistLess(pos, stuckPos, 2f) || !(rotat.y > 0.9f))
			{
				return;
			}
			rotat = new Vector2(0f, 1f);
			rotatVel = 0f;
			dRotat = new Vector2(0f, 1f);
			dRotatVel = 0f;
			pos = stuckPos;
			for (int i = 0; i < neighbors.Count; i++)
			{
				if (!activeConnections[i] && neighbors[i].InPosition() && repairCounter < 200)
				{
					activeConnections[i] = true;
				}
			}
		}

		public bool InPosition()
		{
			if (Custom.DistLess(pos, stuckPos, 1f))
			{
				return vel.magnitude == 0f;
			}
			return false;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("maze");
			sLeaser.sprites[0].scaleX = size.x / Futile.atlasManager.GetElementWithName("maze").sourcePixelSize.x;
			sLeaser.sprites[0].scaleY = size.y / Futile.atlasManager.GetElementWithName("maze").sourcePixelSize.y;
			Vector2 vector = stuckPos - size / 2f;
			sLeaser.sprites[0].color = new Color((float)((int)vector.x % 128) / 128f, (float)((int)vector.y % 128) / 128f, size.x / 128f, size.y / 128f);
			shaderDirty = true;
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (shaderDirty)
			{
				sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders[shader ? "CoralCircuit" : "DeadCoralCircuit"];
				shaderDirty = false;
			}
			sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[0].rotation = Custom.VecToDeg(Vector3.Slerp(lastRotat, rotat, timeStacker));
			sLeaser.sprites[0].scaleX = size.x / Futile.atlasManager.GetElementWithName("maze").sourcePixelSize.x * Vector3.Slerp(lastDRotat, dRotat, timeStacker).y;
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
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
			}
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Background");
			}
			for (int j = 0; j < sLeaser.sprites.Length; j++)
			{
				newContatiner.AddChild(sLeaser.sprites[j]);
			}
		}
	}

	public CoralNeuronSystem system;

	public List<IntVector2> tiles;

	public Vector2 rotation;

	public IntVector2 bottomLeft;

	public IntVector2 topRight;

	public CircuitBit[,,] bits;

	public List<PlacedObject> places;

	public int displacedCounter;

	private float disconnectChance;

	public CoralCircuit(CoralNeuronSystem system, List<PlacedObject> places)
	{
		this.system = system;
		this.places = places;
		tiles = new List<IntVector2>();
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState((int)places[0].pos.x + (int)places[0].pos.y);
		rotation = Custom.RNV();
		bottomLeft = new IntVector2(int.MaxValue, int.MaxValue);
		topRight = new IntVector2(int.MinValue, int.MinValue);
		for (int i = 0; i < places.Count; i++)
		{
			for (int j = system.room.GetTilePosition(places[i].pos).x - (int)((places[i].data as PlacedObject.ResizableObjectData).Rad / 20f) - 1; j <= system.room.GetTilePosition(places[i].pos).x + (int)((places[i].data as PlacedObject.ResizableObjectData).Rad / 20f) + 1; j++)
			{
				for (int k = system.room.GetTilePosition(places[i].pos).y - (int)((places[i].data as PlacedObject.ResizableObjectData).Rad / 20f) - 1; k <= system.room.GetTilePosition(places[i].pos).y + (int)((places[i].data as PlacedObject.ResizableObjectData).Rad / 20f) + 1; k++)
				{
					if (!system.room.GetTile(j, k).Solid && system.room.aimap.getTerrainProximity(j, k) > 2 && Custom.DistLess(system.room.MiddleOfTile(j, k), places[i].pos, (places[i].data as PlacedObject.ResizableObjectData).Rad) && !tiles.Contains(new IntVector2(j, k)))
					{
						tiles.Add(new IntVector2(j, k));
						if (j < bottomLeft.x)
						{
							bottomLeft.x = j;
						}
						if (k < bottomLeft.y)
						{
							bottomLeft.y = k;
						}
						if (j > topRight.x)
						{
							topRight.x = j;
						}
						if (k > topRight.y)
						{
							topRight.y = k;
						}
					}
				}
			}
		}
		bits = new CircuitBit[topRight.x - bottomLeft.x + 1, topRight.y - bottomLeft.y + 1, 2];
		for (int l = 0; l < tiles.Count; l++)
		{
			float num = 0f;
			for (int m = 0; m < places.Count; m++)
			{
				num = Mathf.Max(num, Mathf.InverseLerp((places[m].data as PlacedObject.ResizableObjectData).Rad, (places[m].data as PlacedObject.ResizableObjectData).Rad - 220f, Vector2.Distance(system.room.MiddleOfTile(tiles[l]), places[m].pos)));
			}
			for (int n = 0; n < ((!((double)num < 0.5)) ? 1 : 2); n++)
			{
				CircuitBit circuitBit = new CircuitBit(this, tiles[l], system.room);
				system.room.AddObject(circuitBit);
				bits[tiles[l].x - bottomLeft.x, tiles[l].y - bottomLeft.y, n] = circuitBit;
			}
		}
		for (int num2 = 0; num2 < bits.GetLength(0); num2++)
		{
			for (int num3 = 0; num3 < bits.GetLength(1); num3++)
			{
				for (int num4 = 0; num4 < bits.GetLength(2); num4++)
				{
					if (bits[num2, num3, num4] != null)
					{
						bits[num2, num3, num4].Connect();
					}
				}
			}
		}
		if (tiles.Count > 0)
		{
			for (int num5 = 0; num5 < Custom.IntClamp(tiles.Count / 15, 1, 40); num5++)
			{
				IntVector2 intVector = tiles[UnityEngine.Random.Range(0, tiles.Count)];
				CircuitBit circuitBit2 = bits[intVector.x - bottomLeft.x, intVector.y - bottomLeft.y, UnityEngine.Random.Range(0, bits.GetLength(2))];
				if (circuitBit2 != null)
				{
					system.room.AddObject(new CircuitConnector(circuitBit2, system.room));
				}
			}
			for (int num6 = 0; num6 < Custom.IntClamp(tiles.Count / 30, 1, 20); num6++)
			{
				IntVector2 intVector2 = tiles[UnityEngine.Random.Range(0, tiles.Count)];
				CircuitBit circuitBit3 = bits[intVector2.x - bottomLeft.x, intVector2.y - bottomLeft.y, UnityEngine.Random.Range(0, bits.GetLength(2))];
				if (circuitBit3 != null)
				{
					system.room.AddObject(new CircuitCrab(circuitBit3, system.room));
				}
			}
		}
		UnityEngine.Random.state = state;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		int num = 0;
		for (int i = 1; i < room.physicalObjects.Length; i++)
		{
			for (int j = 0; j < room.physicalObjects[i].Count; j++)
			{
				if (ModManager.MSC && room.physicalObjects[i][j] is Inspector)
				{
					continue;
				}
				for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
				{
					float num2 = 0f;
					for (int l = 0; l < places.Count; l++)
					{
						if (!(num2 < 1f))
						{
							break;
						}
						num2 = Mathf.Max(num2, Mathf.InverseLerp((places[l].data as PlacedObject.ResizableObjectData).Rad + 100f, (places[l].data as PlacedObject.ResizableObjectData).Rad, Vector2.Distance(room.physicalObjects[i][j].bodyChunks[k].pos, places[l].pos)));
					}
					if (!(num2 > 0f))
					{
						continue;
					}
					IntVector2 intVector = room.GetTilePosition(room.physicalObjects[i][j].bodyChunks[k].pos - new Vector2(room.physicalObjects[i][j].bodyChunks[k].rad, room.physicalObjects[i][j].bodyChunks[k].rad)) - new IntVector2(2, 2);
					IntVector2 intVector2 = room.GetTilePosition(room.physicalObjects[i][j].bodyChunks[k].pos + new Vector2(room.physicalObjects[i][j].bodyChunks[k].rad, room.physicalObjects[i][j].bodyChunks[k].rad)) + new IntVector2(2, 2);
					for (int m = intVector.x; m <= intVector2.x; m++)
					{
						for (int n = intVector.y; n <= intVector2.y; n++)
						{
							for (int num3 = 0; num3 < bits.GetLength(2); num3++)
							{
								CircuitBit circuitBit = Bit(m, n, num3);
								num++;
								float num4 = room.physicalObjects[i][j].bodyChunks[k].rad + 10f;
								if (circuitBit != null && Custom.DistLess(circuitBit.pos, circuitBit.stuckPos, 20f) && Custom.DistLess(circuitBit.pos, room.physicalObjects[i][j].bodyChunks[k].pos, num4))
								{
									Vector2 vector = Custom.DirVec(room.physicalObjects[i][j].bodyChunks[k].pos, circuitBit.pos);
									float num5 = Vector2.Distance(room.physicalObjects[i][j].bodyChunks[k].pos, circuitBit.pos);
									float num6 = Mathf.InverseLerp(50f, 10f, Vector2.Distance(circuitBit.pos, circuitBit.stuckPos)) * Mathf.InverseLerp(1f, 3f, room.physicalObjects[i][j].bodyChunks[k].vel.magnitude) * num2;
									float num7 = room.physicalObjects[i][j].bodyChunks[k].mass / (room.physicalObjects[i][j].bodyChunks[k].mass + 0.01f);
									circuitBit.pos += vector * (num4 - num5) * num6 * num7;
									circuitBit.vel += vector * (num4 - num5) * num6 * num7;
									circuitBit.vel += room.physicalObjects[i][j].bodyChunks[k].vel * num6 * num7 * 0.3f;
									room.physicalObjects[i][j].bodyChunks[k].pos -= vector * (num4 - num5) * num6 * (1f - num7);
									room.physicalObjects[i][j].bodyChunks[k].vel -= vector * (num4 - num5) * num6 * (1f - num7);
									room.physicalObjects[i][j].bodyChunks[k].vel -= room.physicalObjects[i][j].bodyChunks[k].vel * num6 * (1f - num7) * 0.3f;
									if (room.physicalObjects[i][j] is Player)
									{
										(room.physicalObjects[i][j] as Player).CollideWithCoralCircuitBit(k, circuitBit, Mathf.InverseLerp(50f, 10f, Vector2.Distance(circuitBit.pos, circuitBit.stuckPos)) * num2);
									}
								}
							}
						}
					}
				}
			}
		}
		disconnectChance = Mathf.InverseLerp(300f, 1600f, displacedCounter);
		displacedCounter = 0;
	}

	public void Explosion(Vector2 pos, float rad, float frc, Vector2 dirVec)
	{
		int num = 0;
		for (int i = 0; i < bits.GetLength(0); i++)
		{
			for (int j = 0; j < bits.GetLength(1); j++)
			{
				for (int k = 0; k < bits.GetLength(2); k++)
				{
					if (bits[i, j, k] != null && Custom.DistLess(bits[i, j, k].pos, pos, rad))
					{
						bits[i, j, k].vel += (Custom.DirVec(pos, bits[i, j, k].pos) * frc + dirVec) * Mathf.InverseLerp(rad, rad / 2f, Vector2.Distance(pos, bits[i, j, k].pos));
						num++;
					}
				}
			}
		}
		if (num > 5)
		{
			room.PlaySound(SoundID.Coral_Circuit_Jump_Explosion, pos, Mathf.InverseLerp(4f, 19f, num), 1f);
		}
	}

	public CircuitBit Bit(IntVector2 tile, int i)
	{
		return Bit(tile.x, tile.y, i);
	}

	public CircuitBit Bit(int x, int y, int i)
	{
		if (x < bottomLeft.x || x > topRight.x || y < bottomLeft.y || y > topRight.y || i < 0 || i >= bits.GetLength(2))
		{
			return null;
		}
		return bits[x - bottomLeft.x, y - bottomLeft.y, i];
	}
}
