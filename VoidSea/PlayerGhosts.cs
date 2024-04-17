using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace VoidSea;

public class PlayerGhosts
{
	public class Ghost
	{
		public PlayerGhosts owner;

		public Player creature;

		public float swimSpeed;

		public Vector2 drift;

		public bool slatedForDeletion;

		public Ghost(PlayerGhosts owner, Player creature)
		{
			this.owner = owner;
			this.creature = creature;
			swimSpeed = Mathf.Lerp(0.9f, 1f, Custom.PushFromHalf(Random.value, 1f + 0.5f * Random.value));
			drift = Custom.RNV();
		}

		public void Update()
		{
			creature.inVoidSea = true;
			owner.voidSea.VoidSeaTreatment(creature, swimSpeed);
			for (int i = 0; i < creature.bodyChunks.Length; i++)
			{
				creature.bodyChunks[i].vel.y *= swimSpeed;
				creature.bodyChunks[i].vel += drift * 0.05f;
				creature.bodyChunks[i].vel.y -= Mathf.InverseLerp(-7000f, -2000f, owner.originalPlayer.mainBodyChunk.pos.y);
			}
			Vector2 pos = creature.mainBodyChunk.pos;
			Vector2 pos2 = owner.voidSea.room.game.cameras[0].pos;
			pos -= pos2;
			float num = 100f;
			if (pos.x < (0f - num) * 2f)
			{
				MoveCreature(pos2 + new Vector2(1400f + num, Mathf.Lerp(0f - num, 800f + num, Random.value)));
			}
			else if (pos.x > 1400f + num * 2f)
			{
				MoveCreature(pos2 + new Vector2(0f - num, Mathf.Lerp(0f - num, 800f + num, Random.value)));
			}
			if (pos.y < (0f - num) * 2f)
			{
				MoveCreature(pos2 + new Vector2(Mathf.Lerp(0f - num, 1400f + num, Random.value), 800f + num));
			}
			else if (pos.y > 800f + num * 2f)
			{
				MoveCreature(pos2 + new Vector2(Mathf.Lerp(0f - num, 1400f + num, Random.value), 0f - num));
			}
		}

		public void MoveCreature(Vector2 movePos)
		{
			if (owner.ghosts.Count > owner.IdealGhostCount)
			{
				Destroy();
				return;
			}
			swimSpeed = Mathf.Lerp(0.9f, 1f, Custom.PushFromHalf(Random.value, 1f + 0.5f * Random.value));
			owner.voidSea.Move(creature, movePos - creature.mainBodyChunk.pos, moveCamera: false);
			for (int i = 0; i < creature.bodyChunks.Length; i++)
			{
				creature.bodyChunks[i].vel = Custom.DirVec(creature.bodyChunks[i].pos, owner.originalPlayer.mainBodyChunk.pos - new Vector2(700f, 400f)) * 2f * Random.value;
			}
			drift = Custom.RNV() * Random.value;
		}

		public void Destroy()
		{
			slatedForDeletion = true;
			creature.room.abstractRoom.RemoveEntity(creature.abstractCreature);
			creature.Destroy();
		}
	}

	public VoidSeaScene voidSea;

	public Player originalPlayer;

	public List<Ghost> ghosts;

	public int IdealGhostCount => (int)Custom.LerpMap(voidSea.eggScenarioTimer, 0f, 2800f, 0f, 5f + 15f * Mathf.Pow(voidSea.eggProximity, 0.3f));

	public PlayerGhosts(Player originalPlayer, VoidSeaScene voidSea)
	{
		this.originalPlayer = originalPlayer;
		this.voidSea = voidSea;
		ghosts = new List<Ghost>();
	}

	public void Update()
	{
		for (int num = ghosts.Count - 1; num >= 0; num--)
		{
			if (ghosts[num].slatedForDeletion)
			{
				ghosts.RemoveAt(num);
			}
			else
			{
				ghosts[num].Update();
			}
		}
		if (ghosts.Count < IdealGhostCount)
		{
			AddGhost();
		}
	}

	public void AddGhost()
	{
		Vector2 pos = originalPlayer.mainBodyChunk.pos + Custom.RNV() * 2000f;
		AbstractCreature abstractCreature = new AbstractCreature(voidSea.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Slugcat), null, voidSea.room.GetWorldCoordinate(pos), new EntityID(-1, -1));
		abstractCreature.state = new PlayerState(abstractCreature, originalPlayer.playerState.playerNumber, originalPlayer.playerState.slugcatCharacter, isGhost: true);
		voidSea.room.abstractRoom.AddEntity(abstractCreature);
		abstractCreature.RealizeInRoom();
		for (int i = 0; i < abstractCreature.realizedCreature.bodyChunks.Length; i++)
		{
			abstractCreature.realizedCreature.bodyChunks[i].restrictInRoomRange = float.MaxValue;
		}
		abstractCreature.realizedCreature.CollideWithTerrain = false;
		ghosts.Add(new Ghost(this, abstractCreature.realizedCreature as Player));
	}
}
