using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

public class ReliableIggyDirection : UpdatableAndDeletable
{
	public class ReliableIggyDirectionData : PlacedObject.ResizableObjectData
	{
		public class Symbol : ExtEnum<Symbol>
		{
			public static readonly Symbol Shelter = new Symbol("Shelter", register: true);

			public static readonly Symbol DynamicDirection = new Symbol("DynamicDirection", register: true);

			public static readonly Symbol SlugcatFace = new Symbol("SlugcatFace", register: true);

			public static readonly Symbol Food = new Symbol("Food", register: true);

			public static readonly Symbol Bat = new Symbol("Bat", register: true);

			public static readonly Symbol Danger = new Symbol("Danger", register: true);

			public Symbol(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public class Condition : ExtEnum<Condition>
		{
			public static readonly Condition AnyTime = new Condition("AnyTime", register: true);

			public static readonly Condition BeforeMoon = new Condition("BeforeMoon", register: true);

			public static readonly Condition AfterMoon = new Condition("AfterMoon", register: true);

			public static readonly Condition AfterPebblesAndMoon = new Condition("AfterPebblesAndMoon", register: true);

			public Condition(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Vector2 panelPos;

		public int exit;

		public int cyclesToShow;

		public bool pointPlayerBack;

		public Symbol symbol;

		public Condition condition;

		public List<SlugcatStats.Name> availableToPlayers;

		public ReliableIggyDirectionData(PlacedObject owner)
			: base(owner)
		{
			symbol = Symbol.Shelter;
			cyclesToShow = 5;
			exit = 0;
			condition = Condition.AnyTime;
			panelPos = new Vector2(50f, 50f);
			availableToPlayers = new List<SlugcatStats.Name>();
			availableToPlayers.Add(SlugcatStats.Name.Yellow);
			pointPlayerBack = true;
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			exit = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			if (int.TryParse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
			{
				symbol = BackwardsCompatibilityRemix.ParseReliableIggySymbol(result);
			}
			else
			{
				symbol = new Symbol(array[5]);
			}
			if (int.TryParse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				condition = BackwardsCompatibilityRemix.ParseReliableIggyCondition(result);
			}
			else
			{
				condition = new Condition(array[6]);
			}
			cyclesToShow = int.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			pointPlayerBack = array[8] == "1";
			if (Custom.IsDigitString(array[9]))
			{
				BackwardsCompatibilityRemix.ParsePlayerAvailability(array[9], availableToPlayers);
			}
			else
			{
				availableToPlayers.Clear();
				string[] array2 = array[9].Split('|');
				foreach (string text in array2)
				{
					if (text != string.Empty)
					{
						availableToPlayers.Add(new SlugcatStats.Name(text));
					}
				}
			}
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 10);
		}

		public override string ToString()
		{
			string text = string.Join("|", Array.ConvertAll(availableToPlayers.ToArray(), (SlugcatStats.Name x) => x.ToString()));
			return SaveUtils.AppendUnrecognizedStringAttrs(string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}", handlePos.x, handlePos.y, panelPos.x, panelPos.y, exit, symbol, condition, cyclesToShow, pointPlayerBack ? "1" : "0", text), "~", unrecognizedAttributes);
		}
	}

	private PlacedObject pObj;

	public bool hasBeenActivated;

	public bool firstUpdate;

	public ReliableIggyDirectionData data => pObj.data as ReliableIggyDirectionData;

	public PlayerGuideState guideState => room.game.GetStorySession.saveState.miscWorldSaveData.playerGuideState;

	public ReliableIggyDirection(PlacedObject pObj)
	{
		this.pObj = pObj;
		hasBeenActivated = false;
		firstUpdate = true;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (firstUpdate)
		{
			firstUpdate = false;
			bool flag = false;
			if (room.game.StoryCharacter == null || !data.availableToPlayers.Contains(room.game.StoryCharacter))
			{
				flag = true;
			}
			if (data.condition != ReliableIggyDirectionData.Condition.AnyTime)
			{
				if (data.condition == ReliableIggyDirectionData.Condition.BeforeMoon && room.game.GetStorySession.saveState.miscWorldSaveData.EverMetMoon)
				{
					flag = true;
				}
				else if (data.condition == ReliableIggyDirectionData.Condition.AfterMoon && !room.game.GetStorySession.saveState.miscWorldSaveData.EverMetMoon)
				{
					flag = true;
				}
				else if (data.condition == ReliableIggyDirectionData.Condition.AfterPebblesAndMoon && (!room.game.GetStorySession.saveState.miscWorldSaveData.EverMetMoon || room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad < 1))
				{
					flag = true;
				}
			}
			if (data.cyclesToShow > 0 && guideState.HowManyTimesHasForcedDirectionBeenGiven(room.abstractRoom.index) >= data.cyclesToShow)
			{
				flag = true;
			}
			if (flag)
			{
				Destroy();
				return;
			}
		}
		bool flag2 = false;
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			if (room.game.Players[i].Room.index == room.abstractRoom.index && room.game.Players[i].realizedCreature != null && (data.pointPlayerBack || room.game.Players[i].pos.abstractNode != data.exit) && room.game.Players[i].realizedCreature.room == room && Custom.DistLess(pObj.pos, room.game.Players[i].realizedCreature.mainBodyChunk.pos, data.Rad))
			{
				flag2 = true;
				break;
			}
		}
		Overseer overseer = null;
		for (int j = 0; j < room.abstractRoom.creatures.Count; j++)
		{
			if (room.abstractRoom.creatures[j].creatureTemplate.type == CreatureTemplate.Type.Overseer && room.abstractRoom.creatures[j].realizedCreature != null && room.abstractRoom.creatures[j].realizedCreature.room == room && (room.abstractRoom.creatures[j].abstractAI as OverseerAbstractAI).playerGuide)
			{
				overseer = room.abstractRoom.creatures[j].realizedCreature as Overseer;
				break;
			}
		}
		if (!flag2)
		{
			return;
		}
		if (overseer != null)
		{
			if (!hasBeenActivated)
			{
				Activate(overseer);
			}
			else
			{
				overseer.AI.communication.forcedDirectionToGive = this;
			}
		}
		else
		{
			BringGuideToPlayer();
		}
	}

	public void Activate(Overseer guide)
	{
		hasBeenActivated = true;
		if (data.cyclesToShow > 0)
		{
			guideState.IncrementTimesForcedDirectionHasBeenGiven(room.abstractRoom.index);
		}
	}

	public void BringGuideToPlayer()
	{
		if (room.world.overseersWorldAI == null || !(room.game.session is StoryGameSession))
		{
			return;
		}
		for (int i = 0; i < room.world.NumberOfRooms; i++)
		{
			for (int j = 0; j < room.world.GetAbstractRoom(room.world.firstRoomIndex + i).creatures.Count; j++)
			{
				if (room.world.GetAbstractRoom(room.world.firstRoomIndex + i).creatures[j].creatureTemplate.type == CreatureTemplate.Type.Overseer && (room.world.GetAbstractRoom(room.world.firstRoomIndex + i).creatures[j].abstractAI as OverseerAbstractAI).playerGuide)
				{
					float likesPlayer = (room.game.session as StoryGameSession).saveState.miscWorldSaveData.playerGuideState.likesPlayer;
					if (likesPlayer > -0.99f)
					{
						(room.world.GetAbstractRoom(room.world.firstRoomIndex + i).creatures[j].abstractAI as OverseerAbstractAI).goToPlayer = true;
						(room.world.GetAbstractRoom(room.world.firstRoomIndex + i).creatures[j].abstractAI as OverseerAbstractAI).playerGuideCounter = (int)Custom.LerpMap(likesPlayer, -0.99f, 1f, 400f, 1800f);
					}
					break;
				}
			}
		}
	}
}
