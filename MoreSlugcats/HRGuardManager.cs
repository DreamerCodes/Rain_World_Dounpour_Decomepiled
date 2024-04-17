using System;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class HRGuardManager : PhysicalObject
{
	private AbstractCreature myGuard;

	private WorldCoordinate startCoord;

	private int hitsToKill;

	private bool triggered;

	private Player myPlayer;

	public VoidChain[] voidChains;

	public bool chainsActive;

	public int timeOnCamera;

	public int playerInRoomTime;

	public float hurtTime;

	public float chainsActiveTime;

	public HRGuardManager(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		startCoord = abstractPhysicalObject.pos;
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(-5000f, -5000f), 0f, 0f);
		base.bodyChunks[0].collideWithTerrain = false;
		base.bodyChunks[0].collideWithSlopes = false;
		base.bodyChunks[0].collideWithObjects = false;
		base.bodyChunks[0].restrictInRoomRange = 10000f;
		base.bodyChunks[0].defaultRestrictInRoomRange = 10000f;
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0f;
		base.gravity = 0f;
		bounce = 0f;
		surfaceFriction = 0f;
		collisionLayer = 0;
		base.waterFriction = 0f;
		base.buoyancy = 0f;
		hitsToKill = 3;
		Custom.Log("SPAWNED battle room!", abstractPhysicalObject.Room.name);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room.game.IsArenaSession)
		{
			Destroy();
			return;
		}
		if (hurtTime > 0f)
		{
			hurtTime -= 1f;
		}
		foreach (AbstractCreature creature in room.abstractRoom.creatures)
		{
			if (creature.abstractAI != null && creature.abstractAI.destination.room != room.abstractRoom.index)
			{
				creature.abstractAI.SetDestination(room.ToWorldCoordinate(room.RandomPos()));
			}
			if (creature.realizedCreature != null && creature.spawnDen.room == room.abstractRoom.index && creature.realizedCreature.enteringShortCut.HasValue && room.shortcutData(creature.realizedCreature.enteringShortCut.Value).ToNode)
			{
				creature.realizedCreature.enteringShortCut = null;
			}
		}
		if (myGuard == null && room.readyForAI)
		{
			Custom.Log($"REALIZING GUARD {abstractPhysicalObject.pos}");
			myGuard = new AbstractCreature(abstractPhysicalObject.Room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.TempleGuard), null, startCoord, abstractPhysicalObject.Room.world.game.GetNewID());
			myGuard.lavaImmune = true;
			myGuard.RealizeInRoom();
			myGuard.destroyOnAbstraction = true;
		}
		if (myGuard != null && room.game.cameras[0].followAbstractCreature != null && room.game.cameras[0].followAbstractCreature.Room != null && room.game.cameras[0].followAbstractCreature.Room == room.abstractRoom)
		{
			AbstractCreature followAbstractCreature = room.game.cameras[0].followAbstractCreature;
			if (followAbstractCreature.realizedCreature != null && followAbstractCreature.realizedCreature is Player && !followAbstractCreature.realizedCreature.inShortcut && room.game.cameras[0].followAbstractCreature.Room.index == room.abstractRoom.index)
			{
				myPlayer = followAbstractCreature.realizedCreature as Player;
			}
			else
			{
				myPlayer = null;
			}
			if (voidChains != null && myGuard.realizedCreature != null)
			{
				chainsActiveTime += 1f;
				for (int i = 0; i < voidChains.Length; i++)
				{
					voidChains[i].proximityAlpha = 0f;
					if (myPlayer != null && myPlayer.room == room)
					{
						voidChains[i].proximityAlpha = 1f - Mathf.InverseLerp(150f, 750f, Custom.Dist(myPlayer.mainBodyChunk.pos, voidChains[i].stuckPosA));
					}
					voidChains[i].stuckPosB = myGuard.realizedCreature.mainBodyChunk.pos;
					if (hurtTime > 0f)
					{
						voidChains[i].colorFlash = UnityEngine.Random.value;
						voidChains[i].proximityAlpha = 1f;
					}
					else
					{
						voidChains[i].colorFlash = Mathf.Sin((float)Math.PI * ((float)playerInRoomTime / 400f) - (float)Math.PI) * 0.25f + 0.25f + 0.5f * Mathf.Min(1f, chainsActiveTime / 80f);
					}
				}
			}
			if (chainsActive && myPlayer != null && myPlayer.enteringShortCut.HasValue && room.shortcutData(myPlayer.enteringShortCut.Value).ToNode)
			{
				myPlayer.firstChunk.vel = room.ShorcutEntranceHoleDirection(myPlayer.enteringShortCut.Value).ToVector2() * 4f;
				myPlayer.enteringShortCut = null;
			}
			if (myGuard.realizedCreature != null && room.ViewedByAnyCamera(myGuard.realizedCreature.mainBodyChunk.pos, 100f))
			{
				timeOnCamera++;
			}
			if (myPlayer != null && myPlayer.room == room)
			{
				playerInRoomTime++;
			}
			if ((timeOnCamera > 40 || playerInRoomTime > 160) && !chainsActive)
			{
				int num = 0;
				for (int j = 0; j < room.shortcuts.Length; j++)
				{
					if (room.shortcuts[j].shortCutType == ShortcutData.Type.RoomExit && room.abstractRoom.connections[room.shortcuts[j].destNode] >= 0)
					{
						num++;
					}
				}
				voidChains = new VoidChain[num];
				int num2 = 0;
				for (int k = 0; k < room.shortcuts.Length; k++)
				{
					if (room.shortcuts[k].shortCutType == ShortcutData.Type.RoomExit && room.abstractRoom.connections[room.shortcuts[k].destNode] >= 0)
					{
						Vector2 spawnPosA = room.MiddleOfTile(room.shortcuts[k].StartTile) + IntVector2.ToVector2(room.ShorcutEntranceHoleDirection(room.shortcuts[k].StartTile)) * 15f;
						VoidChain voidChain = new VoidChain(room, spawnPosA, myGuard.realizedCreature.mainBodyChunk.pos);
						room.AddObject(voidChain);
						voidChains[num2] = voidChain;
						num2++;
					}
				}
				chainsActive = true;
				if (!triggered)
				{
					room.TriggerCombatArena();
					triggered = true;
				}
				room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Chain_Lock, 0f, 1f, UnityEngine.Random.value * 0.5f + 0.8f);
			}
			if (triggered)
			{
				if (myGuard.state.dead && hitsToKill > 0)
				{
					WorldCoordinate pos = myGuard.pos;
					myGuard.realizedCreature.Destroy();
					myGuard.Destroy();
					myGuard = new AbstractCreature(abstractPhysicalObject.Room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.TempleGuard), null, pos, abstractPhysicalObject.Room.world.game.GetNewID());
					myGuard.lavaImmune = true;
					myGuard.RealizeInRoom();
					myGuard.destroyOnAbstraction = true;
					myGuard.realizedCreature.firstChunk.vel += Custom.RNV() * 4f;
					hitsToKill--;
					hurtTime = 40f;
				}
				if (myGuard.state.alive && Vector2.Distance(myGuard.realizedCreature.firstChunk.pos, room.MiddleOfTile(startCoord)) > 90f)
				{
					(myGuard.realizedCreature as TempleGuard).AI.SetDestination(startCoord);
					(myGuard.realizedCreature as TempleGuard).AI.tryHoverPos = startCoord;
					myGuard.realizedCreature.firstChunk.pos.x = Mathf.Clamp(myGuard.realizedCreature.firstChunk.pos.x, 0f, room.PixelWidth);
					myGuard.realizedCreature.firstChunk.pos.y = Mathf.Clamp(myGuard.realizedCreature.firstChunk.pos.y, 0f, room.PixelHeight);
					(myGuard.realizedCreature as TempleGuard).AI.idlePos = startCoord;
				}
				if (hitsToKill <= 0)
				{
					myGuard.realizedCreature.Die();
					(abstractPhysicalObject as AbstractConsumable).Consume();
					if (voidChains != null)
					{
						for (int l = 0; l < voidChains.Length; l++)
						{
							voidChains[l].Destroy();
							voidChains[l].RemoveFromRoom();
						}
					}
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Chain_Break, 0f, 1f, UnityEngine.Random.value * 0.5f + 0.95f);
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Chain_Break, 0f, 1f, UnityEngine.Random.value * 0.5f + 0.95f);
					Destroy();
				}
				else if (hitsToKill < 3 && UnityEngine.Random.value < Mathf.Lerp(0.01f, 0.08f, 1f - (float)hitsToKill / 2f))
				{
					hurtTime = UnityEngine.Random.value * 20f;
				}
			}
		}
		abstractPhysicalObject.pos = startCoord;
	}
}
