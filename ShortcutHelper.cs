using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class ShortcutHelper : UpdatableAndDeletable
{
	public class ObjectPusher
	{
		public PhysicalObject obj;

		public int counter;

		public int hasBeenOutCounter;

		public int waitCounter;

		public int searchTowardsSide;

		public int searchTowardsSideXTile;

		private ShortcutHelper owner;

		public bool slatedForDeletion;

		public bool givenUp;

		public List<ShortcutPusher> pushers => owner.pushers;

		public ObjectPusher(ShortcutHelper owner, PhysicalObject obj)
		{
			this.owner = owner;
			this.obj = obj;
		}

		public void Update()
		{
			if (givenUp)
			{
				return;
			}
			if (obj.room != null)
			{
				counter++;
			}
			if (obj is Creature && !owner.PopsOutOfDeadShortcuts(obj))
			{
				slatedForDeletion = true;
			}
			if (waitCounter > 0)
			{
				waitCounter--;
			}
			else
			{
				hasBeenOutCounter++;
				for (int i = 0; i < owner.pushers.Count; i++)
				{
					IntVector2 shortcutDir = pushers[i].shortcutDir;
					Vector2 vector = owner.room.MiddleOfTile(pushers[i].shortCutPos);
					bool flag = false;
					for (int j = 0; j < obj.bodyChunks.Length; j++)
					{
						if ((shortcutDir.x == 0 || !Custom.DistLess(obj.bodyChunks[j].pos, pushers[i].pushPos, obj.bodyChunks[j].rad + 10f)) && !Custom.DistLess(obj.bodyChunks[j].pos, vector, obj.bodyChunks[j].rad + 10f))
						{
							continue;
						}
						flag = true;
						searchTowardsSide = ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
						if (!owner.room.GetTile(obj.bodyChunks[j].pos + new Vector2(-20f * (float)searchTowardsSide, 20f)).Solid || !owner.room.GetTile(obj.bodyChunks[j].pos + new Vector2(-20f * (float)searchTowardsSide, 40f)).Solid)
						{
							searchTowardsSide = -searchTowardsSide;
						}
						float num = Custom.AimFromOneVectorToAnother(pushers[i].pushPos - shortcutDir.ToVector2() * 40f, obj.bodyChunks[j].pos);
						num += 30f * UnityEngine.Random.value * (float)searchTowardsSide * Custom.LerpMap(counter, 20f, 120f, 0.3f, 1f);
						float num2 = Custom.LerpMap(counter, 0f, (shortcutDir.y == 1) ? 180 : 500, 2.5f, 15f, (shortcutDir.y == 1) ? 0.8f : 3f);
						if (shortcutDir.y == 1 && obj.GoThroughFloors)
						{
							for (int k = 0; k < obj.bodyChunks.Length; k++)
							{
								obj.bodyChunks[k].goThroughFloors = false;
							}
						}
						if (shortcutDir.y < 1 || !owner.room.GetTile(obj.bodyChunks[j].pos + new Vector2(-20f, 0f)).Solid || !owner.room.GetTile(obj.bodyChunks[j].pos + new Vector2(20f, 0f)).Solid)
						{
							searchTowardsSideXTile = -1000;
							searchTowardsSide = 0;
						}
						else
						{
							searchTowardsSideXTile = owner.room.GetTilePosition(obj.bodyChunks[j].pos).x;
							if (counter > 60 && shortcutDir.y == 1 && GiveUpVerticalSpitOut(pushers[i].shortCutPos))
							{
								if (obj is Creature)
								{
									if (!(obj as Creature).enteringShortCut.HasValue && (obj as Creature).shortcutDelay < 1 && !(obj as Creature).inShortcut && obj.room == owner.room)
									{
										Custom.Log($"crit entering shortcut {Vector2.Distance(obj.bodyChunks[j].pos, vector)} {obj.bodyChunks[j].pos}");
										(obj as Creature).enteringShortCut = pushers[i].shortCutPos;
									}
									if (counter > 170)
									{
										givenUp = true;
										Custom.LogWarning("giveup");
									}
								}
								else
								{
									givenUp = true;
									num2 += 10f;
								}
							}
						}
						for (int l = 0; l < obj.bodyChunks.Length; l++)
						{
							if (shortcutDir.x != 0)
							{
								obj.bodyChunks[l].vel.x *= 0f;
							}
							else
							{
								obj.bodyChunks[l].vel.y *= 0f;
							}
							obj.bodyChunks[l].vel += Custom.DegToVec(num) * num2;
							if (!Custom.DistLess(obj.bodyChunks[l].pos, pushers[i].pushPos, obj.bodyChunks[l].rad + 10f) && Custom.DistLess(obj.bodyChunks[l].pos, vector, obj.bodyChunks[l].rad + 10f))
							{
								obj.bodyChunks[l].pos = Vector2.Lerp(obj.bodyChunks[l].pos, pushers[i].pushPos, 0.5f);
							}
							obj.bodyChunks[l].pos += 10f * shortcutDir.ToVector2();
						}
						owner.room.PlaySound(SoundID.Medium_NPC_Tick_Along_In_Shortcut, obj.bodyChunks[j].pos, 1f, 1f);
					}
					if (flag)
					{
						hasBeenOutCounter = 0;
						waitCounter = 40;
						break;
					}
				}
			}
			if (searchTowardsSide != 0 && counter > 20)
			{
				for (int m = 0; m < obj.bodyChunks.Length; m++)
				{
					if (owner.room.GetTilePosition(obj.bodyChunks[m].pos).x == searchTowardsSideXTile)
					{
						if (obj.bodyChunks[m].lastContactPoint.x == 0)
						{
							obj.bodyChunks[m].vel += new Vector2(searchTowardsSide, 0f) * Custom.LerpMap(counter, 20f, 180f, 0f, 0.1f);
						}
						if (!(obj.bodyChunks[m].pos.y < obj.bodyChunks[m].lastPos.y) || owner.room.GetTile(obj.bodyChunks[m].pos + new Vector2(20f * (float)searchTowardsSide, 0f)).Solid)
						{
							continue;
						}
						for (int n = 0; n < obj.bodyChunks.Length; n++)
						{
							obj.bodyChunks[n].pos += new Vector2(searchTowardsSide, 0f) * Custom.LerpMap(counter, 20f, 180f, 0f, 3f);
							obj.bodyChunks[n].vel += new Vector2(searchTowardsSide, 0f) * Custom.LerpMap(counter, 20f, 180f, 0f, 0.8f);
							if (!owner.room.GetTile(obj.bodyChunks[m].pos + new Vector2(20f * (float)(-searchTowardsSide), 0f)).Solid)
							{
								searchTowardsSide = 0;
							}
						}
						break;
					}
					searchTowardsSide = 0;
					break;
				}
			}
			if ((obj.room != null && obj.room != owner.room) || hasBeenOutCounter > ((obj.room == null) ? 400 : 120))
			{
				slatedForDeletion = true;
			}
		}

		public bool GiveUpVerticalSpitOut(IntVector2 tile)
		{
			for (int i = 1; i < 5; i++)
			{
				if (!owner.room.GetTile(tile + new IntVector2(-1, i)).Solid || !owner.room.GetTile(tile + new IntVector2(1, i)).Solid)
				{
					return false;
				}
			}
			return true;
		}
	}

	public class ShortcutPusher
	{
		public bool wrongHole;

		public IntVector2 shortCutPos;

		public IntVector2 shortcutDir;

		public Vector2 pushPos;

		public float swell;

		public bool swellUp;

		public bool floor;

		public List<ShortcutPusher> validNeighbors;

		public ShortcutPusher(Room room, bool wrongHole, IntVector2 shortCutPos, IntVector2 shortcutDir)
		{
			this.wrongHole = wrongHole;
			this.shortCutPos = shortCutPos;
			this.shortcutDir = shortcutDir;
			pushPos = room.MiddleOfTile(shortCutPos + shortcutDir);
			validNeighbors = new List<ShortcutPusher>();
			floor = shortcutDir.y > 0 && room.GetTile(shortCutPos + shortcutDir).Terrain == Room.Tile.TerrainType.Floor;
		}

		public void LookForNeighbors(Room room, ShortcutPusher potNeighbor)
		{
			if (potNeighbor.wrongHole != wrongHole && !(potNeighbor.shortcutDir != shortcutDir) && Math.Abs(potNeighbor.shortCutPos.x - shortCutPos.x) <= 2 && Math.Abs(potNeighbor.shortCutPos.y - shortCutPos.y) <= 2 && (shortcutDir.y == 0 || potNeighbor.shortCutPos.y == shortCutPos.y) && (shortcutDir.x == 0 || potNeighbor.shortCutPos.x == shortCutPos.x))
			{
				if (wrongHole)
				{
					DirectTowardsNeighbor(room, potNeighbor);
				}
				else
				{
					potNeighbor.DirectTowardsNeighbor(room, this);
				}
			}
		}

		public void DirectTowardsNeighbor(Room room, ShortcutPusher nb)
		{
			validNeighbors.Add(nb);
		}

		public void PushPlayerTowardsValidNeighbor(Player player, float pushFac)
		{
			swell = 2f;
			BodyChunk bodyChunk = null;
			ShortcutPusher shortcutPusher = null;
			float num = float.MaxValue;
			for (int i = 0; i < player.bodyChunks.Length; i++)
			{
				for (int j = 0; j < validNeighbors.Count; j++)
				{
					if (Custom.DistLess(player.bodyChunks[i].pos, validNeighbors[j].pushPos, num))
					{
						bodyChunk = player.bodyChunks[i];
						shortcutPusher = validNeighbors[j];
						num = Vector2.Distance(player.bodyChunks[i].pos, validNeighbors[j].pushPos);
					}
				}
			}
			if (bodyChunk == null || !(num < 40f))
			{
				return;
			}
			player.animation = Player.AnimationIndex.None;
			player.bodyMode = Player.BodyModeIndex.Default;
			for (int k = 0; k < player.bodyChunks.Length; k++)
			{
				if (shortcutDir.x != 0)
				{
					player.bodyChunks[k].vel.x += (float)shortcutPusher.shortcutDir.x * pushFac;
					player.bodyChunks[k].vel.y *= 0.5f - 0.3f * pushFac;
					player.bodyChunks[k].vel.y += player.gravity * player.room.gravity;
					player.bodyChunks[k].vel.y += Mathf.Clamp(shortcutPusher.pushPos.y - player.bodyChunks[k].pos.y, -1.2f, 1.2f);
					player.bodyChunks[k].pos.y += Mathf.Clamp(shortcutPusher.pushPos.y - player.bodyChunks[k].pos.y, -1.2f, 1.2f);
				}
				else
				{
					player.bodyChunks[k].vel.y += (float)shortcutPusher.shortcutDir.y * pushFac;
					player.bodyChunks[k].vel.x *= 0.5f - 0.3f * pushFac;
					player.bodyChunks[k].vel.x += Mathf.Clamp(shortcutPusher.pushPos.x - player.bodyChunks[k].pos.x, -1.2f, 1.2f);
					player.bodyChunks[k].pos.x += Mathf.Clamp(shortcutPusher.pushPos.x - player.bodyChunks[k].pos.x, -1.2f, 1.2f);
				}
			}
		}
	}

	public List<ShortcutPusher> pushers;

	public List<ObjectPusher> pushOutObjects;

	public int checkObj;

	public ShortcutHelper(Room room)
	{
		pushers = new List<ShortcutPusher>();
		for (int i = 0; i < room.shortcuts.Length; i++)
		{
			bool wrongHole = room.shortcuts[i].shortCutType != ShortcutData.Type.Normal && room.shortcuts[i].shortCutType != ShortcutData.Type.RoomExit;
			if (room.shortcuts[i].shortCutType == ShortcutData.Type.RoomExit && !room.world.singleRoomWorld && !room.abstractRoom.gate && room.shortcuts[i].destNode >= 0 && room.shortcuts[i].destNode < room.abstractRoom.connections.Length && room.abstractRoom.connections[room.shortcuts[i].destNode] == -1)
			{
				wrongHole = true;
			}
			pushers.Add(new ShortcutPusher(room, wrongHole, room.shortcuts[i].StartTile, room.ShorcutEntranceHoleDirection(room.shortcuts[i].StartTile)));
		}
		for (int j = 0; j < pushers.Count; j++)
		{
			for (int k = j + 1; k < pushers.Count; k++)
			{
				pushers[j].LookForNeighbors(room, pushers[k]);
			}
		}
		pushOutObjects = new List<ObjectPusher>();
	}

	public bool PopsOutOfDeadShortcuts(PhysicalObject physicalObject)
	{
		if (physicalObject.grabbedBy.Count == 0)
		{
			if (physicalObject is Creature && (!(physicalObject as Creature).Consious || physicalObject is Hazer || physicalObject is VultureGrub))
			{
				return true;
			}
			if (physicalObject is PlayerCarryableItem || physicalObject is IPlayerEdible || physicalObject is Weapon || physicalObject is NSHSwarmer)
			{
				return true;
			}
		}
		return false;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			if (room.game.Players[i].realizedCreature == null || room.game.Players[i].realizedCreature.room != room || !room.game.Players[i].realizedCreature.Consious || room.game.Players[i].realizedCreature.grabbedBy.Count != 0)
			{
				continue;
			}
			Player player = room.game.Players[i].realizedCreature as Player;
			IntVector2 intVector = new IntVector2(player.input[0].x, player.input[0].y);
			bool flag = false;
			if (ModManager.MSC && room.world.game.IsArenaSession && room.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && !room.world.game.GetArenaGameSession.exitManager.ExitsOpen())
			{
				flag = true;
			}
			for (int j = 0; j < pushers.Count; j++)
			{
				bool flag2 = false;
				if (flag)
				{
					flag2 = true;
					if (!room.shortcutData(pushers[j].shortCutPos).ToNode)
					{
						flag2 = false;
					}
				}
				if (room.lockedShortcuts.Contains(pushers[j].shortCutPos))
				{
					flag2 = true;
				}
				if (flag2 && player.enteringShortCut.HasValue && player.enteringShortCut.Value == pushers[j].shortCutPos)
				{
					player.enteringShortCut = null;
				}
				if (!(pushers[j].wrongHole || flag2) || (pushers[j].floor && !player.GoThroughFloors) || (pushers[j].shortcutDir.y > 0 && (player.animation == Player.AnimationIndex.BellySlide || player.animation == Player.AnimationIndex.DownOnFours)))
				{
					continue;
				}
				bool flag3 = (intVector.x != 0 && intVector.x == -pushers[j].shortcutDir.x) || (intVector.y != 0 && intVector.y == -pushers[j].shortcutDir.y);
				bool flag4 = player.input[0].jmp || player.jumpBoost > 0f || pushers[j].validNeighbors.Count > 0;
				if (player.enteringShortCut.HasValue && player.enteringShortCut.Value == pushers[j].shortCutPos)
				{
					player.enteringShortCut = null;
				}
				for (int k = 0; k < player.bodyChunks.Length; k++)
				{
					if (flag3 && player.input[0].jmp && !player.input[1].jmp && Custom.DistLess(pushers[j].pushPos, player.bodyChunks[k].pos, 30f + player.bodyChunks[k].rad))
					{
						player.bodyChunks[k].vel = Vector2.Lerp(player.bodyChunks[k].vel, pushers[j].shortcutDir.ToVector2() * 6f + new Vector2(0f, (pushers[j].shortcutDir.x != 0) ? 6f : 0f), 0.5f);
					}
					else if (flag4)
					{
						float num = 20f + player.bodyChunks[k].rad + Custom.LerpMap(pushers[j].swell, 0.5f, 1f, -5f, 10f, 3f) - ((intVector.y != 0 && intVector.y == -pushers[j].shortcutDir.y) ? 5f : 0f);
						pushers[j].swellUp = Custom.DistLess(pushers[j].pushPos, player.bodyChunks[k].pos, Mathf.Max(20f + player.bodyChunks[k].rad, num - 1f)) && flag3;
						if (Custom.DistLess(pushers[j].pushPos, player.bodyChunks[k].pos, num))
						{
							float num2 = Mathf.InverseLerp(num - (flag3 ? 2.5f : 5f), num - 20f, Vector2.Distance(pushers[j].pushPos, player.bodyChunks[k].pos));
							if (pushers[j].validNeighbors.Count > 0 && flag3)
							{
								pushers[j].PushPlayerTowardsValidNeighbor(player, num2);
							}
							else
							{
								player.bodyChunks[k].vel *= Mathf.Lerp(1f, 0.5f, num2);
								player.bodyChunks[k].vel.y += player.gravity * room.gravity * num2;
								player.bodyChunks[k].vel += (Vector2)Vector3.Slerp(Custom.DirVec(pushers[j].pushPos, player.bodyChunks[k].pos), pushers[j].shortcutDir.ToVector2(), 0.9f) * (flag3 ? 3f : 0.9f) * num2;
								player.bodyChunks[k].pos += (Vector2)Vector3.Slerp(Custom.DirVec(pushers[j].pushPos, player.bodyChunks[k].pos), pushers[j].shortcutDir.ToVector2(), 0.9f) * (flag3 ? 3f : 0.9f) * num2;
								if (flag3 && pushers[j].shortcutDir.x != 0)
								{
									player.bodyChunks[k].vel.y = Mathf.Lerp(player.bodyChunks[k].vel.y, Mathf.Clamp(player.bodyChunks[k].vel.y, -2f, 20f), 0.75f);
								}
							}
						}
					}
					if (player.rollDirection != 0 && pushers[j].shortcutDir.x == -player.rollDirection && Custom.DistLess(pushers[j].pushPos, player.bodyChunks[k].pos, 30f + player.bodyChunks[k].rad))
					{
						player.rollDirection = 0;
						player.animation = Player.AnimationIndex.None;
						player.rollCounter = 0;
					}
					float num3 = 10f + player.bodyChunks[k].rad;
					if (flag2)
					{
						num3 *= Mathf.InverseLerp(0f, 500f, player.timeSinceSpawned);
					}
					if (player.bodyChunks[k].pos.y > pushers[j].pushPos.y - num3 && player.bodyChunks[k].pos.y < pushers[j].pushPos.y + num3 && player.bodyChunks[k].pos.x > pushers[j].pushPos.x - num3 && player.bodyChunks[k].pos.x < pushers[j].pushPos.x + num3)
					{
						if (pushers[j].shortcutDir.x != 0)
						{
							player.bodyChunks[k].vel.x += pushers[j].pushPos.x + num3 * (float)pushers[j].shortcutDir.x - player.bodyChunks[k].pos.x;
							player.bodyChunks[k].pos.x += pushers[j].pushPos.x + num3 * (float)pushers[j].shortcutDir.x - player.bodyChunks[k].pos.x;
						}
						else
						{
							player.bodyChunks[k].vel.y += pushers[j].pushPos.y + num3 * (float)pushers[j].shortcutDir.y - player.bodyChunks[k].pos.y;
							player.bodyChunks[k].pos.y += pushers[j].pushPos.y + num3 * (float)pushers[j].shortcutDir.y - player.bodyChunks[k].pos.y;
						}
					}
				}
			}
		}
		for (int l = 0; l < pushers.Count; l++)
		{
			pushers[l].swell = Custom.LerpAndTick(pushers[l].swell, pushers[l].swellUp ? 1f : 0f, 0.02f, 1f / 30f);
			pushers[l].swellUp = false;
		}
		checkObj++;
		for (int m = 0; m < room.physicalObjects.Length; m++)
		{
			if (room.physicalObjects[m].Count <= 0)
			{
				continue;
			}
			PhysicalObject physicalObject = room.physicalObjects[m][checkObj % room.physicalObjects[m].Count];
			if (!PopsOutOfDeadShortcuts(physicalObject))
			{
				continue;
			}
			for (int n = 0; n < physicalObject.bodyChunks.Length; n++)
			{
				if (room.GetTile(physicalObject.bodyChunks[n].pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					AddPushOutObject(physicalObject);
					break;
				}
			}
		}
		for (int num4 = pushOutObjects.Count - 1; num4 >= 0; num4--)
		{
			if (pushOutObjects[num4].slatedForDeletion)
			{
				pushOutObjects.RemoveAt(num4);
			}
			else
			{
				pushOutObjects[num4].Update();
			}
		}
	}

	private void AddPushOutObject(PhysicalObject obj)
	{
		for (int i = 0; i < pushOutObjects.Count; i++)
		{
			if (pushOutObjects[i].obj == obj)
			{
				return;
			}
		}
		pushOutObjects.Add(new ObjectPusher(this, obj));
	}
}
