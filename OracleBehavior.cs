using System.Collections.Generic;
using System.Linq;
using HUD;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class OracleBehavior : Conversation.IOwnAConversation
{
	public Oracle oracle;

	public Vector2 lookPoint;

	public Player player;

	public int allStillCounter;

	public int consistentBasePosCounter;

	public int inActionCounter;

	public DebugSprite dbspr;

	public ChunkSoundEmitter voice;

	public Creature safariCreature;

	public bool isRepeatedDiscussion;

	public List<Player> PlayersInRoom => (from x in oracle?.room?.game?.NonPermaDeadPlayers?.Where((AbstractCreature x) => x.Room == oracle.room.abstractRoom)
		select x?.realizedCreature as Player into x
		orderby x.dead
		select x).ToList();

	public Player PlayerWithNeuronInStomach => PlayersInRoom?.Find((Player x) => x.objectInStomach != null && x.objectInStomach.type == AbstractPhysicalObject.AbstractObjectType.NSHSwarmer);

	public virtual Vector2 OracleGetToPos => default(Vector2);

	public virtual Vector2 BaseGetToPos => default(Vector2);

	public virtual Vector2 GetToDir => default(Vector2);

	public virtual bool EyesClosed => false;

	public RainWorld rainWorld => oracle.room.game.rainWorld;

	public virtual DialogBox dialogBox => null;

	public void StunCoopPlayers(int st)
	{
		foreach (Player item in PlayersInRoom)
		{
			item.Stun(st);
			if (item.slugOnBack != null && item.slugOnBack.HasASlug && !item.slugOnBack.slugcat.dead)
			{
				item.slugOnBack.DropSlug();
			}
		}
	}

	public OracleBehavior(Oracle oracle)
	{
		this.oracle = oracle;
		if (oracle.room.game.Players.Count == 0)
		{
			player = null;
		}
		else
		{
			player = oracle.room.game.Players[0]?.realizedCreature as Player;
			if (ModManager.CoopAvailable && (player == null || player.room != oracle.room))
			{
				player = ((PlayersInRoom.Count > 0) ? PlayersInRoom[0] : null);
			}
		}
		safariCreature = null;
	}

	public void FindPlayer()
	{
		if (!ModManager.CoopAvailable || oracle.room == null || !oracle.room.game.IsStorySession || oracle.room.game.rainWorld.safariMode)
		{
			return;
		}
		bool flag = false;
		if (oracle.ID == Oracle.OracleID.SS && oracle.room.game.StoryCharacter == SlugcatStats.Name.Red && !oracle.room.game.GetStorySession.saveState.miscWorldSaveData.pebblesSeenGreenNeuron)
		{
			Player playerWithNeuronInStomach = PlayerWithNeuronInStomach;
			if (playerWithNeuronInStomach != null)
			{
				flag = true;
				player = playerWithNeuronInStomach;
			}
		}
		else
		{
			player = oracle.room.game.Players[0]?.realizedCreature as Player;
		}
		if (player == null || player.room != oracle.room || player.inShortcut)
		{
			player = ((PlayersInRoom.Count > 0) ? PlayersInRoom[0] : null);
			if (player != null)
			{
				int num = 1;
				while (!flag && player.inShortcut && num < PlayersInRoom.Count)
				{
					player = PlayersInRoom[num];
					num++;
				}
			}
		}
		if (PlayersInRoom.Count > 0 && PlayersInRoom[0].dead && player == PlayersInRoom[0])
		{
			player = null;
		}
		if (player != null)
		{
			oracle.room.game.cameras[0].EnterCutsceneMode(player.abstractCreature, RoomCamera.CameraCutsceneType.Oracle);
		}
	}

	public virtual void Update(bool eu)
	{
		if (voice != null)
		{
			voice.alive = true;
			if (voice.slatedForDeletetion)
			{
				voice = null;
			}
		}
		if (ModManager.MSC && oracle.room != null && oracle.room.game.rainWorld.safariMode)
		{
			safariCreature = null;
			float num = float.MaxValue;
			for (int i = 0; i < oracle.room.abstractRoom.creatures.Count; i++)
			{
				if (oracle.room.abstractRoom.creatures[i].realizedCreature != null)
				{
					Creature realizedCreature = oracle.room.abstractRoom.creatures[i].realizedCreature;
					float num2 = Custom.Dist(oracle.firstChunk.pos, realizedCreature.mainBodyChunk.pos);
					if (num2 < num)
					{
						num = num2;
						safariCreature = realizedCreature;
					}
				}
			}
		}
		FindPlayer();
	}

	public virtual string Translate(string s)
	{
		return oracle.room.game.rainWorld.inGameTranslator.Translate(s);
	}

	public string AlreadyDiscussedItemString(bool pearl)
	{
		string empty = string.Empty;
		if (pearl)
		{
			if (!ModManager.MSC || oracle.ID != Oracle.OracleID.SS)
			{
				return Random.Range(0, 3) switch
				{
					1 => Translate("This one I've already read to you, <PlayerName>."), 
					0 => Translate("Oh, I have already read this one to you, <PlayerName>."), 
					_ => Translate("This one again, <PlayerName>?"), 
				};
			}
			return Random.Range(0, 3) switch
			{
				1 => Translate("We have read this one, little creature."), 
				0 => Translate("We've read this, little creature. As previously stated:"), 
				_ => Translate("I have read this. As I have stated before:"), 
			};
		}
		return Random.Range(0, 3) switch
		{
			1 => Translate("I've told you about this one, <PlayerName>."), 
			0 => Translate("I think we have already talked about this one, <PlayerName>."), 
			_ => Translate("<CapPlayerName>, this one again?"), 
		};
	}

	public bool CheckSlugpupsInRoom()
	{
		if (!ModManager.MSC)
		{
			return false;
		}
		foreach (AbstractCreature creature in oracle.room.abstractRoom.creatures)
		{
			if (creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC && creature.state.alive)
			{
				return true;
			}
		}
		return false;
	}

	public CreatureTemplate.Type CheckStrayCreatureInRoom()
	{
		foreach (AbstractCreature creature in oracle.room.abstractRoom.creatures)
		{
			if (creature.creatureTemplate.type != CreatureTemplate.Type.Slugcat && (!ModManager.MSC || creature.creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC) && creature.creatureTemplate.type != CreatureTemplate.Type.TubeWorm && creature.creatureTemplate.type != CreatureTemplate.Type.VultureGrub && creature.creatureTemplate.type != CreatureTemplate.Type.Hazer && creature.creatureTemplate.type != CreatureTemplate.Type.Fly && creature.creatureTemplate.type != CreatureTemplate.Type.Overseer && creature.state.alive)
			{
				return creature.creatureTemplate.type;
			}
		}
		return CreatureTemplate.Type.StandardGroundCreature;
	}

	public virtual void UnconciousUpdate()
	{
		FindPlayer();
	}

	public string ReplaceParts(string s)
	{
		return s;
	}

	public void SpecialEvent(string eventName)
	{
	}
}
