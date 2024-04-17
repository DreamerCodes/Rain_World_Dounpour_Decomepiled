using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HUD;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class SLOracleBehaviorHasMark : SLOracleBehavior, Conversation.IOwnAConversation
{
	public class PauseReason : ExtEnum<PauseReason>
	{
		public static readonly PauseReason Leave = new PauseReason("Leave", register: true);

		public static readonly PauseReason Annoyance = new PauseReason("Annoyance", register: true);

		public static readonly PauseReason GrabNeuron = new PauseReason("GrabNeuron", register: true);

		public PauseReason(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class MiscItemType : ExtEnum<MiscItemType>
	{
		public static readonly MiscItemType NA = new MiscItemType("NA", register: true);

		public static readonly MiscItemType Rock = new MiscItemType("Rock", register: true);

		public static readonly MiscItemType Spear = new MiscItemType("Spear", register: true);

		public static readonly MiscItemType FireSpear = new MiscItemType("FireSpear", register: true);

		public static readonly MiscItemType WaterNut = new MiscItemType("WaterNut", register: true);

		public static readonly MiscItemType KarmaFlower = new MiscItemType("KarmaFlower", register: true);

		public static readonly MiscItemType DangleFruit = new MiscItemType("DangleFruit", register: true);

		public static readonly MiscItemType FlareBomb = new MiscItemType("FlareBomb", register: true);

		public static readonly MiscItemType VultureMask = new MiscItemType("VultureMask", register: true);

		public static readonly MiscItemType PuffBall = new MiscItemType("PuffBall", register: true);

		public static readonly MiscItemType JellyFish = new MiscItemType("JellyFish", register: true);

		public static readonly MiscItemType Lantern = new MiscItemType("Lantern", register: true);

		public static readonly MiscItemType Mushroom = new MiscItemType("Mushroom", register: true);

		public static readonly MiscItemType FirecrackerPlant = new MiscItemType("FirecrackerPlant", register: true);

		public static readonly MiscItemType SlimeMold = new MiscItemType("SlimeMold", register: true);

		public static readonly MiscItemType ScavBomb = new MiscItemType("ScavBomb", register: true);

		public static readonly MiscItemType BubbleGrass = new MiscItemType("BubbleGrass", register: true);

		public static readonly MiscItemType OverseerRemains = new MiscItemType("OverseerRemains", register: true);

		public MiscItemType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class MoonConversation : Conversation
	{
		public MiscItemType describeItem;

		public OracleBehavior myBehavior;

		public SLOrcacleState State
		{
			get
			{
				if (myBehavior is SLOracleBehaviorHasMark)
				{
					return (myBehavior as SLOracleBehaviorHasMark).State;
				}
				return myBehavior.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SLOracleState;
			}
		}

		public MoonConversation(ID id, OracleBehavior slOracleBehaviorHasMark, MiscItemType describeItem)
			: base(slOracleBehaviorHasMark, id, slOracleBehaviorHasMark.dialogBox)
		{
			myBehavior = slOracleBehaviorHasMark;
			currentSaveFile = slOracleBehaviorHasMark.oracle.room.game.GetStorySession.saveStateNumber;
			this.describeItem = describeItem;
			AddEvents();
		}

		public string Translate(string s)
		{
			return myBehavior.Translate(s);
		}

		protected override void AddEvents()
		{
			Custom.Log(id.ToString(), State.neuronsLeft.ToString());
			if (id == ID.MoonFirstPostMarkConversation)
			{
				switch (Mathf.Clamp(State.neuronsLeft, 0, 5))
				{
				case 1:
					events.Add(new TextEvent(this, 40, "...", 10));
					break;
				case 2:
					events.Add(new TextEvent(this, 30, Translate("Get... get away... white.... thing."), 10));
					events.Add(new TextEvent(this, 0, Translate("Please... thiss all I have left."), 10));
					break;
				case 3:
					events.Add(new TextEvent(this, 30, Translate("You!"), 10));
					events.Add(new TextEvent(this, 60, Translate("...you ate... me. Please go away. I won't speak... to you.<LINE>I... CAN'T speak to you... because... you ate...me..."), 0));
					break;
				case 4:
					LoadEventsFromFile(35);
					LoadEventsFromFile(37);
					events.Add(new TextEvent(this, 0, Translate("I'm still angry at you, but it is good to have someone to talk to after all this time.<LINE>The scavengers aren't exactly good listeners. They do bring me things though, occasionally..."), 0));
					break;
				case 5:
					events.Add(new TextEvent(this, 0, Translate("Hello <PlayerName>."), 0));
					events.Add(new TextEvent(this, 0, Translate("What are you? If I had my memories I would know..."), 0));
					if (State.playerEncounters > 0 && State.playerEncountersWithMark == 0)
					{
						events.Add(new TextEvent(this, 0, Translate("Perhaps... I saw you before?"), 0));
					}
					events.Add(new TextEvent(this, 0, Translate("You must be very brave to have made it all the way here. But I'm sorry to say your journey here is in vain."), 5));
					events.Add(new TextEvent(this, 0, Translate("As you can see, I have nothing for you. Not even my memories."), 0));
					events.Add(new TextEvent(this, 0, Translate("Or did I say that already?"), 5));
					LoadEventsFromFile(37);
					events.Add(new TextEvent(this, 0, Translate("It is good to have someone to talk to after all this time!<LINE>The scavengers aren't exactly good listeners. They do bring me things though, occasionally..."), 0));
					break;
				case 0:
					break;
				}
			}
			else if (id == ID.MoonSecondPostMarkConversation)
			{
				switch (Mathf.Clamp(State.neuronsLeft, 0, 5))
				{
				case 1:
					events.Add(new TextEvent(this, 40, "...", 10));
					break;
				case 2:
					events.Add(new TextEvent(this, 80, Translate("...leave..."), 10));
					break;
				case 3:
					events.Add(new TextEvent(this, 20, Translate("You..."), 10));
					events.Add(new TextEvent(this, 0, Translate("Please don't... take... more from me... Go."), 0));
					break;
				case 4:
					if (State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
					{
						events.Add(new TextEvent(this, 30, Translate("Oh. You."), 0));
						break;
					}
					if (State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
					{
						events.Add(new TextEvent(this, 30, Translate("Hello there! You again!"), 0));
					}
					else
					{
						events.Add(new TextEvent(this, 30, Translate("Hello there. You again!"), 0));
					}
					events.Add(new TextEvent(this, 0, Translate("I wonder what it is that you want?"), 0));
					if (State.GetOpinion != SLOrcacleState.PlayerOpinion.Dislikes && (!ModManager.MSC || myBehavior.oracle.room.game.GetStorySession.saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Saint))
					{
						events.Add(new TextEvent(this, 0, Translate("I have had scavengers come by before. Scavengers!<LINE>And they left me alive!<LINE>But... I have told you that already, haven't I?"), 0));
						events.Add(new TextEvent(this, 0, Translate("You must excuse me if I repeat myself. My memory is bad.<LINE>I used to have a pathetic five neurons... And then you ate one.<LINE>Maybe I've told you that before as well."), 0));
					}
					break;
				case 5:
					if (State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
					{
						events.Add(new TextEvent(this, 0, Translate("You again."), 10));
						break;
					}
					if (State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
					{
						events.Add(new TextEvent(this, 0, Translate("Oh, hello!"), 10));
					}
					else
					{
						events.Add(new TextEvent(this, 0, Translate("Oh, hello."), 10));
					}
					if (ModManager.MSC && myBehavior.oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
					{
						if (State.GetOpinion != SLOrcacleState.PlayerOpinion.Dislikes)
						{
							events.Add(new TextEvent(this, 0, Translate("Have you come back to see me again?"), 60));
							events.Add(new TextEvent(this, 0, Translate("Ah, you remind me of an old creature who used to visit here often."), 0));
							events.Add(new TextEvent(this, 0, Translate("The passage of time has since taken them away, of course. That was a while ago."), 0));
							events.Add(new TextEvent(this, 0, Translate("Stay as long as you'd like. But not too long."), 40));
							events.Add(new TextEvent(this, 0, Translate("This chamber is not very well insulated from the cold."), 30));
						}
						break;
					}
					events.Add(new TextEvent(this, 0, Translate("I wonder what it is that you want?"), 0));
					if (!(State.GetOpinion != SLOrcacleState.PlayerOpinion.Dislikes))
					{
						break;
					}
					events.Add(new TextEvent(this, 0, Translate("There is nothing here. Not even my memories remain."), 0));
					events.Add(new TextEvent(this, 30, Translate("Even the scavengers that come here from time to time leave with nothing. But... I have told you that already, haven't I?"), 0));
					if (State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
					{
						if (ModManager.MSC && myBehavior.CheckSlugpupsInRoom())
						{
							events.Add(new TextEvent(this, 0, Translate("I do enjoy the company though. You and your family are always welcome here."), 5));
						}
						else if (ModManager.MMF && myBehavior.CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
						{
							events.Add(new TextEvent(this, 0, Translate("I do enjoy the company of you and your friend though, <PlayerName>."), 5));
						}
						else
						{
							events.Add(new TextEvent(this, 0, Translate("I do enjoy the company though. You're welcome to stay a while, quiet little thing."), 5));
						}
					}
					break;
				case 0:
					break;
				}
			}
			else if (ModManager.MSC && id == MoreSlugcatsEnums.ConversationID.Moon_Gourmand_First_Conversation)
			{
				switch (Mathf.Clamp(State.neuronsLeft, 0, 5))
				{
				case 1:
					events.Add(new TextEvent(this, 40, "...", 10));
					break;
				case 2:
					events.Add(new TextEvent(this, 30, Translate("Get... get away... round.... thing."), 10));
					events.Add(new TextEvent(this, 0, Translate("Please... thiss all I have left."), 10));
					break;
				case 3:
					events.Add(new TextEvent(this, 30, Translate("You!"), 10));
					events.Add(new TextEvent(this, 60, Translate("...you ate... me. Please go away. I won't speak... to you.<LINE>I... CAN'T speak to you... because... you ate...me..."), 0));
					break;
				case 4:
					LoadEventsFromFile(35);
					LoadEventsFromFile(163);
					break;
				case 5:
					LoadEventsFromFile(162);
					break;
				}
			}
			else if (id == ID.MoonRecieveSwarmer)
			{
				if (!(myBehavior is SLOracleBehaviorHasMark))
				{
					return;
				}
				if (State.neuronsLeft - 1 > 2 && (myBehavior as SLOracleBehaviorHasMark).respondToNeuronFromNoSpeakMode)
				{
					events.Add(new TextEvent(this, 10, Translate("You... Strange thing. Now this?"), 10));
					events.Add(new TextEvent(this, 0, Translate("I will accept your gift..."), 10));
				}
				switch (State.neuronsLeft)
				{
				case 2:
					events.Add(new TextEvent(this, 40, "...", 10));
					events.Add(new TextEvent(this, 0, Translate("You!"), 10));
					events.Add(new TextEvent(this, 10, Translate("...you...killed..."), 10));
					events.Add(new TextEvent(this, 0, "...", 10));
					events.Add(new TextEvent(this, 0, Translate("...me"), 10));
					break;
				case 3:
					events.Add(new TextEvent(this, 10, Translate("...thank you... better..."), 10));
					events.Add(new TextEvent(this, 20, Translate("still, very... bad."), 10));
					break;
				case 4:
					events.Add(new TextEvent(this, 20, Translate("Thank you... That is a little better. Thank you, creature."), 10));
					if (!(myBehavior as SLOracleBehaviorHasMark).respondToNeuronFromNoSpeakMode)
					{
						events.Add(new TextEvent(this, 0, Translate("Maybe this is asking too much... But, would you bring me another one?"), 0));
					}
					break;
				default:
					if ((myBehavior as SLOracleBehaviorHasMark).respondToNeuronFromNoSpeakMode)
					{
						events.Add(new TextEvent(this, 0, Translate("Thank you. I do wonder what you want."), 10));
						break;
					}
					if (State.neuronGiveConversationCounter == 0)
					{
						Custom.Log("moon recieve first neuron. Has neurons:", State.neuronsLeft.ToString());
						if (State.neuronsLeft == 5)
						{
							LoadEventsFromFile(45);
						}
						else if (ModManager.MSC && (myBehavior.oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint || myBehavior.oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet))
						{
							LoadEventsFromFile(130);
						}
						else
						{
							LoadEventsFromFile(19);
						}
					}
					else if (State.neuronGiveConversationCounter == 1)
					{
						if (ModManager.MSC && (myBehavior.oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint || myBehavior.oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet))
						{
							LoadEventsFromFile(159);
						}
						else
						{
							events.Add(new TextEvent(this, 30, Translate("You get these at Five Pebbles'?<LINE>Thank you so much. I'm sure he won't mind."), 10));
							events.Add(new TextEvent(this, 10, "...", 0));
							events.Add(new TextEvent(this, 10, Translate("Or actually I'm sure he would, but he has so many of these~<LINE>it doesn't do him any difference.<LINE>For me though, it does! Thank you, little creature!"), 0));
						}
					}
					else
					{
						switch (UnityEngine.Random.Range(0, 4))
						{
						case 0:
							events.Add(new TextEvent(this, 30, Translate("Thank you, again. I feel wonderful."), 10));
							break;
						case 1:
							events.Add(new TextEvent(this, 30, Translate("Thank you so very much!"), 10));
							break;
						case 2:
							events.Add(new TextEvent(this, 30, Translate("It is strange... I'm remembering myself, but also... him."), 10));
							break;
						default:
							events.Add(new TextEvent(this, 30, Translate("Thank you... Sincerely."), 10));
							break;
						}
					}
					State.neuronGiveConversationCounter++;
					break;
				case 0:
				case 1:
					break;
				}
				(myBehavior as SLOracleBehaviorHasMark).respondToNeuronFromNoSpeakMode = false;
			}
			else if (id == ID.Moon_Pearl_Misc)
			{
				PearlIntro();
				MiscPearl(miscPearl2: false);
			}
			else if (id == ID.Moon_Pearl_Misc2)
			{
				PearlIntro();
				MiscPearl(miscPearl2: true);
			}
			else if (id == ID.Moon_Pebbles_Pearl)
			{
				PebblesPearl();
			}
			else if (id == ID.Moon_Pearl_CC)
			{
				PearlIntro();
				LoadEventsFromFile(7);
			}
			else if (!ModManager.MSC && id == ID.Moon_Pearl_SI_west)
			{
				PearlIntro();
				LoadEventsFromFile(GetARandomChatLog(whichPearl: false));
			}
			else if (!ModManager.MSC && id == ID.Moon_Pearl_SI_top)
			{
				PearlIntro();
				LoadEventsFromFile(GetARandomChatLog(whichPearl: true));
			}
			else if (id == ID.Moon_Pearl_LF_west)
			{
				PearlIntro();
				LoadEventsFromFile(10);
			}
			else if (id == ID.Moon_Pearl_LF_bottom)
			{
				PearlIntro();
				LoadEventsFromFile(11);
			}
			else if (id == ID.Moon_Pearl_HI)
			{
				PearlIntro();
				LoadEventsFromFile(12);
			}
			else if (id == ID.Moon_Pearl_SH)
			{
				PearlIntro();
				LoadEventsFromFile(13);
			}
			else if (id == ID.Moon_Pearl_DS)
			{
				PearlIntro();
				LoadEventsFromFile(14);
			}
			else if (id == ID.Moon_Pearl_SB_filtration)
			{
				PearlIntro();
				LoadEventsFromFile(15);
			}
			else if (id == ID.Moon_Pearl_GW)
			{
				PearlIntro();
				LoadEventsFromFile(16);
			}
			else if (id == ID.Moon_Pearl_SL_bridge)
			{
				PearlIntro();
				LoadEventsFromFile(17);
			}
			else if (id == ID.Moon_Pearl_SL_moon)
			{
				PearlIntro();
				LoadEventsFromFile(18);
			}
			else if (id == ID.Moon_Pearl_SU)
			{
				PearlIntro();
				LoadEventsFromFile(41);
			}
			else if (id == ID.Moon_Pearl_UW)
			{
				PearlIntro();
				LoadEventsFromFile(42);
			}
			else if (id == ID.Moon_Pearl_SB_ravine)
			{
				PearlIntro();
				LoadEventsFromFile(43);
			}
			else if (id == ID.Moon_Pearl_SL_chimney)
			{
				PearlIntro();
				LoadEventsFromFile(54);
			}
			else if (id == ID.Moon_Pearl_Red_stomach)
			{
				PearlIntro();
				LoadEventsFromFile(51);
			}
			else if (id == ID.Moon_Misc_Item)
			{
				if (ModManager.MMF && myBehavior.isRepeatedDiscussion)
				{
					events.Add(new TextEvent(this, 0, myBehavior.AlreadyDiscussedItemString(pearl: false), 10));
				}
				if (describeItem == MiscItemType.Spear)
				{
					if (ModManager.MSC && currentSaveFile == MoreSlugcatsEnums.SlugcatStatsName.Saint)
					{
						events.Add(new TextEvent(this, 10, Translate("It's a piece of sharpened rebar... What is it you want to know?<LINE>I don't wish to offend, but you seem too frail to use this effectively."), 0));
					}
					else
					{
						events.Add(new TextEvent(this, 10, Translate("It's a piece of sharpened rebar... What is it you want to know?<LINE>You seem proficient enough at using it."), 0));
					}
				}
				else if (describeItem == MiscItemType.FireSpear)
				{
					events.Add(new TextEvent(this, 10, Translate("It's a weapon made with fire powder. Did the scavengers give this to you?<LINE>Be very careful if you have to use it!"), 0));
				}
				else if (describeItem == MiscItemType.Rock)
				{
					events.Add(new TextEvent(this, 10, Translate("It's a rock. Thank you, I suppose, little creature."), 0));
				}
				else if (describeItem == MiscItemType.KarmaFlower)
				{
					LoadEventsFromFile(25);
				}
				else if (describeItem == MiscItemType.WaterNut)
				{
					events.Add(new TextEvent(this, 10, Translate("It's a delicious plant. You should have it!"), 0));
				}
				else if (describeItem == MiscItemType.DangleFruit)
				{
					LoadEventsFromFile(26);
				}
				else if (describeItem == MiscItemType.FlareBomb)
				{
					LoadEventsFromFile(27);
				}
				else if (describeItem == MiscItemType.VultureMask)
				{
					LoadEventsFromFile(28);
				}
				else if (describeItem == MiscItemType.PuffBall)
				{
					LoadEventsFromFile(29);
				}
				else if (describeItem == MiscItemType.JellyFish)
				{
					LoadEventsFromFile(30);
				}
				else if (describeItem == MiscItemType.Lantern)
				{
					LoadEventsFromFile(31);
				}
				else if (describeItem == MiscItemType.Mushroom)
				{
					LoadEventsFromFile(32);
				}
				else if (describeItem == MiscItemType.FirecrackerPlant)
				{
					LoadEventsFromFile(33);
				}
				else if (describeItem == MiscItemType.SlimeMold)
				{
					LoadEventsFromFile(34);
				}
				else if (describeItem == MiscItemType.ScavBomb)
				{
					LoadEventsFromFile(44);
				}
				else if (describeItem == MiscItemType.OverseerRemains)
				{
					if (ModManager.MSC && myBehavior.oracle.room.game.IsMoonHeartActive())
					{
						LoadEventsFromFile(169);
					}
					else
					{
						LoadEventsFromFile(52);
					}
				}
				else if (describeItem == MiscItemType.BubbleGrass)
				{
					LoadEventsFromFile(53);
				}
				else if (ModManager.MSC)
				{
					if (describeItem == MoreSlugcatsEnums.MiscItemType.SingularityGrenade)
					{
						LoadEventsFromFile(127);
					}
					else if (describeItem == MoreSlugcatsEnums.MiscItemType.EnergyCell)
					{
						State.shownEnergyCell = true;
						LoadEventsFromFile(110);
					}
					else if (describeItem == MoreSlugcatsEnums.MiscItemType.ElectricSpear)
					{
						LoadEventsFromFile(112);
					}
					else if (describeItem == MoreSlugcatsEnums.MiscItemType.InspectorEye)
					{
						LoadEventsFromFile(113);
					}
					else if (describeItem == MoreSlugcatsEnums.MiscItemType.GooieDuck)
					{
						LoadEventsFromFile(114);
					}
					else if (describeItem == MoreSlugcatsEnums.MiscItemType.NeedleEgg)
					{
						LoadEventsFromFile(116);
					}
					else if (describeItem == MoreSlugcatsEnums.MiscItemType.LillyPuck)
					{
						LoadEventsFromFile(117);
					}
					else if (describeItem == MoreSlugcatsEnums.MiscItemType.GlowWeed)
					{
						LoadEventsFromFile(118);
					}
					else if (describeItem == MoreSlugcatsEnums.MiscItemType.DandelionPeach)
					{
						LoadEventsFromFile(122);
					}
					else if (describeItem == MoreSlugcatsEnums.MiscItemType.MoonCloak)
					{
						LoadEventsFromFile(123);
					}
					else if (describeItem == MoreSlugcatsEnums.MiscItemType.EliteMask)
					{
						LoadEventsFromFile(136);
					}
					else if (describeItem == MoreSlugcatsEnums.MiscItemType.KingMask)
					{
						LoadEventsFromFile(137);
					}
					else if (describeItem == MoreSlugcatsEnums.MiscItemType.FireEgg)
					{
						LoadEventsFromFile(164);
					}
					else if (describeItem == MoreSlugcatsEnums.MiscItemType.SpearmasterSpear)
					{
						LoadEventsFromFile(166);
					}
					else if (describeItem == MoreSlugcatsEnums.MiscItemType.Seed)
					{
						LoadEventsFromFile(167);
					}
				}
			}
			else if (id == ID.Moon_Red_First_Conversation)
			{
				LoadEventsFromFile(50);
			}
			else if (id == ID.Moon_Red_Second_Conversation)
			{
				LoadEventsFromFile(55);
			}
			else if (id == ID.Moon_Yellow_First_Conversation)
			{
				LoadEventsFromFile(49);
			}
			else
			{
				if (!ModManager.MSC)
				{
					return;
				}
				if (id == ID.Moon_Pearl_SI_west)
				{
					PearlIntro();
					LoadEventsFromFile(20);
				}
				else if (id == ID.Moon_Pearl_SI_top)
				{
					PearlIntro();
					LoadEventsFromFile(21);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat3)
				{
					PearlIntro();
					LoadEventsFromFile(22);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat4)
				{
					PearlIntro();
					LoadEventsFromFile(23);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat5)
				{
					PearlIntro();
					LoadEventsFromFile(24);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SU_filt)
				{
					PearlIntro();
					LoadEventsFromFile(101);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_DM)
				{
					PearlIntro();
					LoadEventsFromFile(102);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_LC)
				{
					PearlIntro();
					LoadEventsFromFile(103);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_OE)
				{
					PearlIntro();
					LoadEventsFromFile(104);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_MS)
				{
					PearlIntro();
					LoadEventsFromFile(105);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_RM)
				{
					if (currentSaveFile != MoreSlugcatsEnums.SlugcatStatsName.Saint)
					{
						PearlIntro();
					}
					LoadEventsFromFile(106);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_Rivulet_stomach)
				{
					PearlIntro();
					LoadEventsFromFile(119);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_LC_second)
				{
					PearlIntro();
					LoadEventsFromFile(121);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_RipPebbles)
				{
					LoadEventsFromFile(124);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_RipPebbles_MeetingRiv)
				{
					LoadEventsFromFile(125);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Rivulet_First_Conversation)
				{
					LoadEventsFromFile(126);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_VS)
				{
					PearlIntro();
					LoadEventsFromFile(128);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_PearlBleaching)
				{
					LoadEventsFromFile(129);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_BroadcastMisc)
				{
					PearlIntro();
					LoadEventsFromFile(132, oneRandomLine: true, (!(myBehavior is SLOracleBehaviorHasMark) || (myBehavior as SLOracleBehaviorHasMark).holdingObject == null) ? UnityEngine.Random.Range(0, 100000) : (myBehavior as SLOracleBehaviorHasMark).holdingObject.abstractPhysicalObject.ID.RandomSeed);
					State.miscPearlCounter++;
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_RivuletEnding)
				{
					LoadEventsFromFile(154);
					if ((interfaceOwner as SLOracleBehaviorHasMark).oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad > 0)
					{
						LoadEventsFromFile(155);
					}
					else
					{
						LoadEventsFromFile(156);
					}
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_RivuletPostgame)
				{
					LoadEventsFromFile(157);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Saint_Echo_Blocked)
				{
					LoadEventsFromFile(160);
				}
				else if (id == MoreSlugcatsEnums.ConversationID.Moon_Saint_First_Conversation)
				{
					LoadEventsFromFile(161);
				}
			}
		}

		private void PearlIntro()
		{
			if (myBehavior.isRepeatedDiscussion)
			{
				events.Add(new TextEvent(this, 0, myBehavior.AlreadyDiscussedItemString(pearl: true), 10));
				return;
			}
			if (myBehavior.oracle.ID != Oracle.OracleID.SS)
			{
				switch (State.totalPearlsBrought + State.miscPearlCounter)
				{
				case 0:
					events.Add(new TextEvent(this, 0, Translate("Ah, you would like me to read this?"), 10));
					events.Add(new TextEvent(this, 0, Translate("It's a bit dusty, but I will do my best. Hold on..."), 10));
					return;
				case 1:
					events.Add(new TextEvent(this, 0, Translate("Another pearl! You want me to read this one too? Just a moment..."), 10));
					return;
				case 2:
					events.Add(new TextEvent(this, 0, Translate("And yet another one! I will read it to you."), 10));
					return;
				case 3:
					if (ModManager.MSC && myBehavior.oracle.ID == MoreSlugcatsEnums.OracleID.DM)
					{
						events.Add(new TextEvent(this, 0, Translate("Another? Let us see... to be honest, I'm as curious to see it as you are."), 10));
						return;
					}
					events.Add(new TextEvent(this, 0, Translate("Another? You're no better than the scavengers!"), 10));
					if (State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
					{
						events.Add(new TextEvent(this, 0, Translate("Let us see... to be honest, I'm as curious to see it as you are."), 10));
					}
					return;
				}
				switch (UnityEngine.Random.Range(0, 5))
				{
				case 1:
					if (ModManager.MSC && myBehavior.oracle.ID == MoreSlugcatsEnums.OracleID.DM)
					{
						events.Add(new TextEvent(this, 0, Translate("Oh? What have you found this time? Let's see what it says..."), 10));
					}
					else
					{
						events.Add(new TextEvent(this, 0, Translate("The scavengers must be jealous of you, finding all these"), 10));
					}
					break;
				case 2:
					events.Add(new TextEvent(this, 0, Translate("Here we go again, little archeologist. Let's read your pearl."), 10));
					break;
				case 3:
					events.Add(new TextEvent(this, 0, Translate("... You're getting quite good at this you know. A little archeologist beast.<LINE>Now, let's see what it says."), 10));
					break;
				default:
					events.Add(new TextEvent(this, 0, Translate("And yet another one! I will read it to you."), 10));
					break;
				case 0:
					break;
				}
				return;
			}
			switch (State.totalPearlsBrought + State.miscPearlCounter)
			{
			case 0:
				events.Add(new TextEvent(this, 0, Translate("Ah, you have found me something to read?"), 10));
				return;
			case 1:
				events.Add(new TextEvent(this, 0, Translate("Have you found something else for me to read?"), 10));
				events.Add(new TextEvent(this, 0, Translate("Let us take a look."), 10));
				return;
			case 2:
				events.Add(new TextEvent(this, 0, Translate("I am surprised you have found so many of these."), 10));
				return;
			case 3:
				events.Add(new TextEvent(this, 0, Translate("Where do you find all of these?"), 10));
				events.Add(new TextEvent(this, 0, Translate("I wonder, just how much time has passed since some of these were written."), 10));
				return;
			}
			switch (UnityEngine.Random.Range(0, 5))
			{
			case 0:
				events.Add(new TextEvent(this, 0, Translate("Let us see what you have found."), 10));
				break;
			case 1:
				events.Add(new TextEvent(this, 0, Translate("Ah. Have you found something new?"), 10));
				break;
			case 2:
				events.Add(new TextEvent(this, 0, Translate("What is this?"), 10));
				break;
			case 3:
				events.Add(new TextEvent(this, 0, Translate("Is that something new? Allow me to see."), 10));
				break;
			default:
				events.Add(new TextEvent(this, 0, Translate("Let us see if there is anything important written on this."), 10));
				break;
			}
		}

		private void MiscPearl(bool miscPearl2)
		{
			LoadEventsFromFile(38, oneRandomLine: true, (myBehavior is SLOracleBehaviorHasMark && (myBehavior as SLOracleBehaviorHasMark).holdingObject != null) ? (myBehavior as SLOracleBehaviorHasMark).holdingObject.abstractPhysicalObject.ID.RandomSeed : UnityEngine.Random.Range(0, 100000));
			State.miscPearlCounter++;
		}

		private void PebblesPearl()
		{
			switch (UnityEngine.Random.Range(0, 5))
			{
			case 0:
				events.Add(new TextEvent(this, 0, Translate("You would like me to read this?"), 10));
				events.Add(new TextEvent(this, 0, Translate("It's still warm... this was in use recently."), 10));
				break;
			case 1:
				events.Add(new TextEvent(this, 0, Translate("A pearl... This one is crystal clear - it was used just recently."), 10));
				break;
			case 2:
				events.Add(new TextEvent(this, 0, Translate("Would you like me to read this pearl?"), 10));
				events.Add(new TextEvent(this, 0, Translate("Strange... it seems to have been used not too long ago."), 10));
				break;
			case 3:
				events.Add(new TextEvent(this, 0, Translate("This pearl has been written to just now!"), 10));
				break;
			default:
				events.Add(new TextEvent(this, 0, Translate("Let's see... A pearl..."), 10));
				events.Add(new TextEvent(this, 0, Translate("And this one is fresh! It was not long ago this data was written to it!"), 10));
				break;
			}
			LoadEventsFromFile((ModManager.MSC && myBehavior.oracle.ID == MoreSlugcatsEnums.OracleID.DM) ? 168 : 40, oneRandomLine: true, (myBehavior is SLOracleBehaviorHasMark && (myBehavior as SLOracleBehaviorHasMark).holdingObject != null) ? (myBehavior as SLOracleBehaviorHasMark).holdingObject.abstractPhysicalObject.ID.RandomSeed : UnityEngine.Random.Range(0, 100000));
		}

		public int GetARandomChatLog(bool whichPearl)
		{
			List<int> list = new List<int> { 0, 1, 2, 3, 4 };
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState((myBehavior.oracle.room.game.session as StoryGameSession).saveState.seed);
			int num = list[UnityEngine.Random.Range(0, list.Count)];
			list.Remove(num);
			int num2 = list[UnityEngine.Random.Range(0, list.Count)];
			UnityEngine.Random.state = state;
			if (whichPearl)
			{
				return 20 + num;
			}
			return 20 + num2;
		}
	}

	public int sayHelloDelay = -1;

	public Conversation currentConversation;

	public int playerLeavingCounter;

	public bool resumeConversationAfterCurrentDialoge;

	public int playerAnnoyingCounter;

	public PauseReason pauseReason;

	public bool playerIsAnnoyingWhenNoConversation;

	public bool rainInterrupt;

	public bool playerHoldingNeuronNoConvo;

	public bool respondToNeuronFromNoSpeakMode;

	public int describeItemCounter;

	public List<EntityID> talkedAboutThisSession;

	public PhysicalObject moveToAndPickUpItem;

	public int moveToItemDelay;

	private bool throwAwayObjects;

	private bool holdPlayerAsleep;

	private bool deadTalk;

	public override DialogBox dialogBox
	{
		get
		{
			if (currentConversation != null)
			{
				return currentConversation.dialogBox;
			}
			if (oracle.room.game.cameras[0].hud.dialogBox == null)
			{
				oracle.room.game.cameras[0].hud.InitDialogBox();
			}
			return oracle.room.game.cameras[0].hud.dialogBox;
		}
	}

	public override Vector2 OracleGetToPos
	{
		get
		{
			if (moveToAndPickUpItem != null && moveToItemDelay > 40)
			{
				return moveToAndPickUpItem.firstChunk.pos;
			}
			return base.OracleGetToPos;
		}
	}

	public bool DamagedMode => base.State.neuronsLeft < 4;

	public new RainWorld rainWorld => oracle.room.game.rainWorld;

	protected string NameForPlayer(bool capitalized)
	{
		string text = "creature";
		bool flag = DamagedMode && UnityEngine.Random.value < 0.5f;
		if (UnityEngine.Random.value > 0.3f)
		{
			text = ((base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes) ? ((base.State.totalPearlsBrought <= 5 || DamagedMode) ? "friend" : "archaeologist") : ((!(base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)) ? "creature" : "tormentor"));
		}
		if (oracle.room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Portuguese && (text == "friend" || text == "creature"))
		{
			string text2 = base.Translate(text);
			if (capitalized && InGameTranslator.LanguageID.UsesCapitals(oracle.room.game.rainWorld.inGameTranslator.currentLanguage))
			{
				text2 = char.ToUpper(text2[0]) + text2.Substring(1);
			}
			return text2;
		}
		string text3 = base.Translate(text);
		string text4 = base.Translate("little");
		if (capitalized && InGameTranslator.LanguageID.UsesCapitals(oracle.room.game.rainWorld.inGameTranslator.currentLanguage))
		{
			text4 = char.ToUpper(text4[0]) + text4.Substring(1);
		}
		return text4 + (flag ? "... " : " ") + text3;
	}

	public SLOracleBehaviorHasMark(Oracle oracle)
		: base(oracle)
	{
		respondToNeuronFromNoSpeakMode = !base.State.SpeakingTerms;
		talkedAboutThisSession = new List<EntityID>();
	}

	public override void Update(bool eu)
	{
		if (ModManager.MSC && SingularityProtest())
		{
			if (currentConversation != null)
			{
				currentConversation.Destroy();
			}
		}
		else
		{
			protest = false;
		}
		base.Update(eu);
		if (!oracle.Consious || stillWakingUp)
		{
			oracle.room.socialEventRecognizer.ownedItemsOnGround.Clear();
			holdingObject = null;
			moveToAndPickUpItem = null;
			return;
		}
		if (ModManager.MSC)
		{
			if (rivEnding != null && currentConversation != null)
			{
				currentConversation.Update();
				oracle.room.socialEventRecognizer.ownedItemsOnGround.Clear();
				holdingObject = null;
				moveToAndPickUpItem = null;
				return;
			}
			if (rivEnding != null)
			{
				return;
			}
			if (player != null && oracle.room.game.GetStorySession.saveState.denPosition == "SL_AI" && oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding && oracle.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && !holdPlayerAsleep)
			{
				holdPlayerAsleep = true;
				oracle.room.game.GetStorySession.saveState.denPosition = "SL_S06";
				sayHelloDelay = 0;
				forceFlightMode = true;
				if (currentConversation != null)
				{
					currentConversation.Destroy();
					currentConversation = null;
				}
				currentConversation = new MoonConversation(MoreSlugcatsEnums.ConversationID.Moon_RivuletPostgame, oracle.oracleBehavior, MiscItemType.NA);
			}
			if (player != null && holdPlayerAsleep)
			{
				player.sleepCounter = 99;
				player.standing = false;
				player.flipDirection = 1;
				player.touchedNoInputCounter = 10;
				player.sleepCurlUp = 1f;
			}
			if (currentConversation == null)
			{
				forceFlightMode = false;
			}
		}
		if (player != null && hasNoticedPlayer)
		{
			if (ModManager.MMF && player.dead)
			{
				TalkToDeadPlayer();
			}
			if (movementBehavior != MovementBehavior.Meditate && movementBehavior != MovementBehavior.ShowMedia)
			{
				lookPoint = player.DangerPos;
			}
			if (sayHelloDelay < 0 && ((ModManager.MSC && oracle.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint) || oracle.room.world.rainCycle.TimeUntilRain + oracle.room.world.rainCycle.pause > 2000))
			{
				sayHelloDelay = 30;
			}
			else
			{
				if (sayHelloDelay > 0)
				{
					sayHelloDelay--;
				}
				if (sayHelloDelay == 1)
				{
					InitateConversation();
					if (!conversationAdded && oracle.room.game.session is StoryGameSession)
					{
						(oracle.room.game.session as StoryGameSession).saveState.miscWorldSaveData.SLOracleState.playerEncounters++;
						(oracle.room.game.session as StoryGameSession).saveState.miscWorldSaveData.SLOracleState.playerEncountersWithMark++;
						if (ModManager.MSC && oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && (oracle.room.game.session as StoryGameSession).saveState.miscWorldSaveData.SLOracleState.playerEncounters == 1 && oracle.room.world.overseersWorldAI != null)
						{
							oracle.room.world.overseersWorldAI.DitchDirectionGuidance();
						}
						Custom.Log("player encounter with SL AI logged");
						conversationAdded = true;
					}
				}
			}
			if (player.room != oracle.room || player.DangerPos.x < 1016f)
			{
				playerLeavingCounter++;
			}
			else
			{
				playerLeavingCounter = 0;
			}
			if (player.room == oracle.room && Custom.DistLess(player.mainBodyChunk.pos, oracle.firstChunk.pos, 100f) && !Custom.DistLess(player.mainBodyChunk.lastPos, player.mainBodyChunk.pos, 1f))
			{
				playerAnnoyingCounter++;
			}
			else
			{
				playerAnnoyingCounter--;
			}
			playerAnnoyingCounter = Custom.IntClamp(playerAnnoyingCounter, 0, 150);
			bool flag = false;
			for (int i = 0; i < player.grasps.Length; i++)
			{
				if (player.grasps[i] != null && player.grasps[i].grabbed is SLOracleSwarmer)
				{
					flag = true;
				}
			}
			if (!base.State.SpeakingTerms && currentConversation != null)
			{
				currentConversation.Destroy();
			}
			if (!rainInterrupt && player.room == oracle.room && oracle.room.world.rainCycle.TimeUntilRain < 1600 && oracle.room.world.rainCycle.pause < 1)
			{
				if (ModManager.MSC && oracle.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint)
				{
					if (currentConversation == null)
					{
						InterruptRain();
						rainInterrupt = true;
					}
				}
				else
				{
					InterruptRain();
					rainInterrupt = true;
					if (currentConversation != null)
					{
						currentConversation.Destroy();
					}
				}
			}
			if (flag)
			{
				if (currentConversation != null)
				{
					if (!currentConversation.paused || pauseReason != PauseReason.GrabNeuron)
					{
						currentConversation.paused = true;
						pauseReason = PauseReason.GrabNeuron;
						InterruptPlayerHoldNeuron();
					}
				}
				else if (!playerHoldingNeuronNoConvo)
				{
					playerHoldingNeuronNoConvo = true;
					InterruptPlayerHoldNeuron();
				}
			}
			if (currentConversation != null)
			{
				playerHoldingNeuronNoConvo = false;
				playerIsAnnoyingWhenNoConversation = false;
				if (currentConversation.slatedForDeletion)
				{
					currentConversation = null;
				}
				else
				{
					if (playerLeavingCounter > 10)
					{
						if (!currentConversation.paused)
						{
							currentConversation.paused = true;
							pauseReason = PauseReason.Leave;
							InterruptPlayerLeavingMessage();
						}
					}
					else if (playerAnnoyingCounter > 80 && !oracle.room.game.IsMoonActive())
					{
						if (!currentConversation.paused)
						{
							currentConversation.paused = true;
							pauseReason = PauseReason.Annoyance;
							InterruptPlayerAnnoyingMessage();
						}
					}
					else if (currentConversation.paused)
					{
						if (resumeConversationAfterCurrentDialoge)
						{
							if (dialogBox.messages.Count == 0)
							{
								currentConversation.paused = false;
								resumeConversationAfterCurrentDialoge = false;
								currentConversation.RestartCurrent();
							}
						}
						else if ((pauseReason == PauseReason.Leave && player.room == oracle.room && player.DangerPos.x > 1036f) || (pauseReason == PauseReason.Annoyance && playerAnnoyingCounter == 0) || (pauseReason == PauseReason.GrabNeuron && !flag))
						{
							resumeConversationAfterCurrentDialoge = true;
							ResumePausedConversation();
						}
					}
					currentConversation.Update();
				}
			}
			else if (base.State.SpeakingTerms)
			{
				if (playerHoldingNeuronNoConvo && !flag)
				{
					playerHoldingNeuronNoConvo = false;
					PlayerReleaseNeuron();
				}
				else if (playerAnnoyingCounter > 80 && !playerIsAnnoyingWhenNoConversation && !oracle.room.game.IsMoonActive())
				{
					playerIsAnnoyingWhenNoConversation = true;
					PlayerAnnoyingWhenNotTalking();
				}
				else if (playerAnnoyingCounter < 10 && playerIsAnnoyingWhenNoConversation)
				{
					playerIsAnnoyingWhenNoConversation = false;
					if (base.State.annoyances == 1)
					{
						if (base.State.neuronsLeft == 3)
						{
							dialogBox.Interrupt("...thank you.", 7);
						}
						else if (base.State.neuronsLeft > 3)
						{
							dialogBox.Interrupt(Translate("Thank you."), 7);
						}
					}
				}
			}
		}
		if ((ModManager.MSC || (!DamagedMode && base.State.SpeakingTerms)) && holdingObject == null && reelInSwarmer == null && moveToAndPickUpItem == null)
		{
			for (int j = 0; j < oracle.room.socialEventRecognizer.ownedItemsOnGround.Count; j++)
			{
				if (!Custom.DistLess(oracle.room.socialEventRecognizer.ownedItemsOnGround[j].item.firstChunk.pos, oracle.firstChunk.pos, 100f) || !WillingToInspectItem(oracle.room.socialEventRecognizer.ownedItemsOnGround[j].item))
				{
					continue;
				}
				bool flag2 = true;
				for (int k = 0; k < pickedUpItemsThisRealization.Count; k++)
				{
					if (pickedUpItemsThisRealization[k] == oracle.room.socialEventRecognizer.ownedItemsOnGround[j].item.abstractPhysicalObject.ID)
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					moveToAndPickUpItem = oracle.room.socialEventRecognizer.ownedItemsOnGround[j].item;
					if (currentConversation != null)
					{
						currentConversation.Destroy();
					}
					currentConversation = null;
					PlayerPutItemOnGround();
					break;
				}
			}
		}
		if (moveToAndPickUpItem != null)
		{
			moveToItemDelay++;
			if (!WillingToInspectItem(moveToAndPickUpItem) || moveToAndPickUpItem.grabbedBy.Count > 0)
			{
				moveToAndPickUpItem = null;
			}
			else if ((moveToItemDelay > 40 && Custom.DistLess(moveToAndPickUpItem.firstChunk.pos, oracle.firstChunk.pos, 40f)) || (moveToItemDelay < 20 && !Custom.DistLess(moveToAndPickUpItem.firstChunk.lastPos, moveToAndPickUpItem.firstChunk.pos, 5f) && Custom.DistLess(moveToAndPickUpItem.firstChunk.pos, oracle.firstChunk.pos, 20f)))
			{
				GrabObject(moveToAndPickUpItem);
				moveToAndPickUpItem = null;
			}
		}
		else
		{
			moveToItemDelay = 0;
		}
		if (player != null)
		{
			for (int l = 0; l < player.grasps.Length; l++)
			{
				if (player.grasps[l] != null && player.grasps[l].grabbed is SLOracleSwarmer)
				{
					protest = true;
					holdKnees = false;
					oracle.bodyChunks[0].vel += Custom.RNV() * oracle.health * UnityEngine.Random.value;
					oracle.bodyChunks[1].vel += Custom.RNV() * oracle.health * UnityEngine.Random.value * 2f;
					protestCounter += 1f / 22f;
					lookPoint = oracle.bodyChunks[0].pos + Custom.PerpendicularVector(oracle.bodyChunks[1].pos, oracle.bodyChunks[0].pos) * Mathf.Sin(protestCounter * (float)Math.PI * 2f) * 145f;
					if (UnityEngine.Random.value < 1f / 30f)
					{
						armsProtest = !armsProtest;
					}
					break;
				}
			}
		}
		if (!protest)
		{
			armsProtest = false;
		}
		if (holdingObject != null)
		{
			describeItemCounter++;
			if (!protest && (currentConversation == null || !currentConversation.paused) && movementBehavior != MovementBehavior.Meditate && movementBehavior != MovementBehavior.ShowMedia)
			{
				lookPoint = holdingObject.firstChunk.pos + Custom.DirVec(oracle.firstChunk.pos, holdingObject.firstChunk.pos) * 100f;
			}
			if (!(holdingObject is SSOracleSwarmer) && describeItemCounter > 40 && currentConversation == null)
			{
				if (ModManager.MMF && throwAwayObjects)
				{
					holdingObject.firstChunk.vel = new Vector2(-5f + (float)UnityEngine.Random.Range(-8, -11), 8f + (float)UnityEngine.Random.Range(1, 3));
					oracle.room.PlaySound(SoundID.Slugcat_Throw_Rock, oracle.firstChunk);
				}
				holdingObject = null;
			}
		}
		else
		{
			describeItemCounter = 0;
		}
	}

	private void PlayerPutItemOnGround()
	{
		if (ModManager.MSC && RejectDiscussItem())
		{
			return;
		}
		switch (base.State.totalItemsBrought)
		{
		case 0:
			dialogBox.Interrupt(Translate("What is that?"), 10);
			return;
		case 1:
			dialogBox.Interrupt(Translate("Another gift?"), 10);
			if (base.State.GetOpinion != SLOrcacleState.PlayerOpinion.Dislikes)
			{
				dialogBox.NewMessage(Translate("I will take a look."), 10);
			}
			return;
		case 2:
			if (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
			{
				dialogBox.Interrupt(Translate("Oh, what is that, <PlayerName>?"), 10);
			}
			else
			{
				dialogBox.Interrupt(Translate("What is that, <PlayerName>?"), 10);
			}
			return;
		case 3:
			dialogBox.Interrupt(Translate("Yet another gift?"), 10);
			return;
		}
		switch (UnityEngine.Random.Range(0, 11))
		{
		case 0:
			dialogBox.Interrupt(Translate("Something new you want me to look at, <PlayerName>?"), 10);
			break;
		case 1:
			if (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
			{
				dialogBox.Interrupt(Translate("Another gift for me?"), 10);
			}
			dialogBox.NewMessage(Translate("I will take a look."), 10);
			break;
		case 2:
			dialogBox.Interrupt(Translate("Oh, what is that, <PlayerName>?"), 10);
			break;
		case 3:
			if (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
			{
				dialogBox.Interrupt(Translate("Yet another gift? You're quite curious, <PlayerName>!"), 10);
				break;
			}
			dialogBox.Interrupt(Translate("Yet another thing?"), 10);
			dialogBox.NewMessage(Translate("Your curiosity seems boundless, <PlayerName>."), 10);
			break;
		case 4:
			dialogBox.Interrupt(Translate("Another thing you want me to look at?"), 10);
			break;
		case 5:
			if (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
			{
				dialogBox.Interrupt(Translate("Oh... I will look at it."), 10);
			}
			else
			{
				dialogBox.Interrupt(Translate("Something new you want me to look at,<LINE>I suppose, <PlayerName>?"), 10);
			}
			break;
		case 6:
			if (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
			{
				dialogBox.Interrupt(Translate("Oh... Of course I will take a look"), 10);
			}
			else
			{
				dialogBox.Interrupt(Translate("Oh... I will take a look"), 10);
			}
			break;
		case 7:
			dialogBox.Interrupt(Translate("You want me to take a look at that?"), 10);
			break;
		case 8:
			dialogBox.Interrupt(Translate("Oh... Should I look at that?"), 10);
			break;
		case 9:
			dialogBox.Interrupt(Translate("A gift for me, <PlayerName>?"), 10);
			break;
		default:
			dialogBox.Interrupt(Translate("A new gift for me, <PlayerName>?"), 10);
			break;
		}
	}

	public void PlayerInterruptByTakingItem()
	{
		if (throwAwayObjects)
		{
			if (UnityEngine.Random.value < 0.25f)
			{
				dialogBox.Interrupt(Translate("Stop it! Go away!"), 30);
			}
			else
			{
				dialogBox.Interrupt(Translate("..."), 10);
			}
			base.State.totalInterruptions++;
			return;
		}
		if (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
		{
			if (UnityEngine.Random.value < 0.5f)
			{
				dialogBox.Interrupt(Translate("Yes, take it and leave me alone."), 10);
			}
			else
			{
				dialogBox.Interrupt(Translate("And now you're taking it, apparently."), 10);
			}
		}
		else
		{
			switch (UnityEngine.Random.Range(0, 4))
			{
			case 0:
				dialogBox.Interrupt(Translate("Oh... Never mind, I suppose."), 10);
				break;
			case 1:
				if (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
				{
					dialogBox.Interrupt(Translate("Oh, you want it back?"), 10);
				}
				else
				{
					dialogBox.Interrupt(Translate("And now you're taking it back."), 10);
				}
				break;
			case 2:
				dialogBox.Interrupt(Translate("Want it back, <PlayerName>?"), 10);
				break;
			default:
				dialogBox.Interrupt(Translate("Oh..."), 10);
				dialogBox.NewMessage(Translate("Yes, you're welcome to have it back."), 10);
				break;
			}
		}
		if (currentConversation != null)
		{
			currentConversation.Destroy();
			currentConversation = null;
			base.State.totalInterruptions++;
		}
	}

	private void InitateConversation()
	{
		if (!base.State.SpeakingTerms)
		{
			dialogBox.NewMessage("...", 10);
			return;
		}
		int num = 0;
		for (int i = 0; i < player.grasps.Length; i++)
		{
			if (player.grasps[i] != null && player.grasps[i].grabbed is SSOracleSwarmer)
			{
				num++;
			}
		}
		if (ModManager.MSC && oracle.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding && base.State.playerEncountersWithMark == 0)
		{
			base.State.playerEncountersWithMark = 1;
		}
		if (ModManager.MSC && oracle.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && !oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.altEnding && oracle.room.game.GetStorySession.saveState.miscWorldSaveData.pebblesEnergyTaken && !base.State.talkedAboutPebblesDeath)
		{
			base.State.talkedAboutPebblesDeath = true;
			bool flag = base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes;
			if (base.State.playerEncountersWithMark > 0)
			{
				dialogBox.Interrupt(Translate("Hello again, <PlayerName>" + ((!flag) ? "." : "!")), 10);
				currentConversation = new MoonConversation(MoreSlugcatsEnums.ConversationID.Moon_RipPebbles, this, MiscItemType.NA);
			}
			else
			{
				currentConversation = new MoonConversation(MoreSlugcatsEnums.ConversationID.Moon_RipPebbles_MeetingRiv, this, MiscItemType.NA);
			}
		}
		else if (base.State.playerEncountersWithMark <= 0)
		{
			if (base.State.playerEncounters < 0)
			{
				base.State.playerEncounters = 0;
			}
			if (oracle.room.game.StoryCharacter == SlugcatStats.Name.Yellow)
			{
				currentConversation = new MoonConversation(Conversation.ID.Moon_Yellow_First_Conversation, this, MiscItemType.NA);
			}
			else if (oracle.room.game.StoryCharacter == SlugcatStats.Name.Red)
			{
				currentConversation = new MoonConversation(Conversation.ID.Moon_Red_First_Conversation, this, MiscItemType.NA);
			}
			else if (ModManager.MSC && oracle.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				currentConversation = new MoonConversation(MoreSlugcatsEnums.ConversationID.Moon_Rivulet_First_Conversation, this, MiscItemType.NA);
			}
			else if (ModManager.MSC && oracle.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				currentConversation = new MoonConversation(MoreSlugcatsEnums.ConversationID.Moon_Gourmand_First_Conversation, this, MiscItemType.NA);
			}
			else if (ModManager.MSC && oracle.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				if (oracle.room.game.cameras[0].ghostMode > 0f)
				{
					currentConversation = new MoonConversation(MoreSlugcatsEnums.ConversationID.Moon_Saint_Echo_Blocked, this, MiscItemType.NA);
					base.State.playerEncounters = -100;
				}
				else
				{
					currentConversation = new MoonConversation(MoreSlugcatsEnums.ConversationID.Moon_Saint_First_Conversation, this, MiscItemType.NA);
				}
			}
			else
			{
				currentConversation = new MoonConversation(Conversation.ID.MoonFirstPostMarkConversation, this, MiscItemType.NA);
			}
		}
		else if (num > 0)
		{
			PlayerHoldingSSNeuronsGreeting();
		}
		else if (base.State.playerEncountersWithMark == 1)
		{
			if (oracle.room.game.StoryCharacter == SlugcatStats.Name.Red)
			{
				currentConversation = new MoonConversation(Conversation.ID.Moon_Red_Second_Conversation, this, MiscItemType.NA);
			}
			else
			{
				currentConversation = new MoonConversation(Conversation.ID.MoonSecondPostMarkConversation, this, MiscItemType.NA);
			}
		}
		else
		{
			ThirdAndUpGreeting();
		}
	}

	private void ThirdAndUpGreeting()
	{
		switch (base.State.neuronsLeft)
		{
		case 1:
			dialogBox.Interrupt("...", 40);
			return;
		case 2:
			dialogBox.Interrupt(Translate("...leave..."), 20);
			return;
		case 3:
			dialogBox.Interrupt(Translate("...you."), 10);
			dialogBox.NewMessage(Translate("...leave me alone..."), 10);
			return;
		case 0:
			return;
		}
		if (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
		{
			switch (UnityEngine.Random.Range(0, 4))
			{
			case 0:
				dialogBox.Interrupt(Translate("Here again."), 10);
				break;
			case 1:
				dialogBox.Interrupt(Translate("You."), 10);
				dialogBox.NewMessage(Translate("I wish you would stop coming here."), 10);
				break;
			case 2:
				dialogBox.Interrupt(Translate("You again."), 10);
				dialogBox.NewMessage(Translate("Please leave me alone."), 10);
				break;
			default:
				dialogBox.Interrupt(Translate("Oh, it's you, <PlayerName>."), 10);
				break;
			}
			if (ModManager.MSC && CheckSlugpupsInRoom())
			{
				dialogBox.NewMessage(Translate("Take your offspring with you when you go."), 10);
			}
			else if (ModManager.MMF && CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
			{
				dialogBox.NewMessage(Translate("Please do not bring more wildlife into my chamber."), 10);
			}
		}
		else
		{
			bool flag = base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes;
			switch (UnityEngine.Random.Range(0, 5))
			{
			case 0:
				dialogBox.Interrupt(Translate("Hello again, <PlayerName>" + (flag ? "!" : ".")), 10);
				break;
			case 1:
				dialogBox.Interrupt(Translate("Hello, <PlayerName>" + (flag ? "!" : ".")), 10);
				dialogBox.NewMessage(Translate(flag ? "How have you been?" : "You're here again."), 10);
				break;
			case 2:
				dialogBox.Interrupt(Translate("Oh, <PlayerName>. Hello" + (flag ? "!" : ".")), 10);
				break;
			case 3:
				dialogBox.Interrupt(Translate(flag ? "It's you, <PlayerName>. Hello." : "It's you, <PlayerName>!  Hello!"), 10);
				break;
			case 4:
				dialogBox.Interrupt(Translate("Ah... <PlayerName>, you're here again" + (flag ? "!" : ".")), 10);
				break;
			default:
				dialogBox.Interrupt(Translate("Ah... <PlayerName>, you're back" + (flag ? "!" : ".")), 10);
				break;
			}
			if (ModManager.MSC && CheckSlugpupsInRoom())
			{
				if (flag)
				{
					dialogBox.NewMessage(Translate("How cute, you brought your family, <PlayerName>?"), 10);
				}
				else
				{
					dialogBox.NewMessage(Translate("Have you brought your family here?"), 10);
				}
			}
			else if (ModManager.MMF && CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
			{
				dialogBox.NewMessage(Translate("Is this your friend, <PlayerName>?"), 10);
			}
		}
		if (ModManager.MMF)
		{
			CreatureJokeDialog();
		}
	}

	private void PlayerHoldingSSNeuronsGreeting()
	{
		switch (base.State.neuronsLeft)
		{
		case 1:
			dialogBox.Interrupt("...", 40);
			return;
		case 2:
			dialogBox.Interrupt(Translate("...oh... to... save me?"), 20);
			return;
		case 3:
			dialogBox.Interrupt(Translate("You... brought that... for me?"), 20);
			return;
		case 0:
			return;
		}
		if (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
		{
			if (UnityEngine.Random.value < 0.5f)
			{
				dialogBox.Interrupt(Translate("You are bringing a neuron. Is it to taunt me?"), 30);
			}
			else
			{
				dialogBox.Interrupt(Translate("A neuron."), 30);
			}
			return;
		}
		bool flag = base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes;
		switch (UnityEngine.Random.Range(0, 3))
		{
		case 0:
			dialogBox.Interrupt(Translate("That... That is for me?"), 10);
			break;
		case 1:
			dialogBox.Interrupt(Translate("Hello" + (flag ? "!" : ".")), 10);
			dialogBox.NewMessage(Translate("That... Oh, thank you."), 10);
			break;
		default:
			dialogBox.Interrupt(Translate("Ah... <PlayerName>, a neuron from Pebbles?"), 30);
			break;
		}
	}

	public override void ConvertingSSSwarmer()
	{
		base.ConvertingSSSwarmer();
		base.State.totNeuronsGiven++;
		base.State.increaseLikeOnSave = true;
		if (reelInSwarmer == null && (currentConversation == null || currentConversation.id != Conversation.ID.MoonRecieveSwarmer) && base.State.SpeakingTerms)
		{
			currentConversation = new MoonConversation(Conversation.ID.MoonRecieveSwarmer, this, MiscItemType.NA);
		}
	}

	private void InterruptPlayerLeavingMessage()
	{
		if (base.State.totalInterruptions >= 5 && (!ModManager.MMF || base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes))
		{
			NoLongerOnSpeakingTerms();
			if (base.State.totalInterruptions == 5)
			{
				dialogBox.Interrupt(DamagedMode ? Translate("...don't... come back.") : Translate("Please don't come back."), 10);
			}
		}
		else if (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
		{
			switch (base.State.leaves)
			{
			case 0:
				currentConversation.Interrupt(DamagedMode ? Translate("...leaving now? Don't... return.") : Translate("Oh, leaving. Please don't come back."), 10);
				break;
			case 1:
				currentConversation.Interrupt(DamagedMode ? Translate("...yes... leave.") : Translate("Leaving again."), 10);
				break;
			case 2:
				currentConversation.Interrupt(DamagedMode ? Translate("This... time... don't... come back.") : Translate("You're leaving yet again. This time, stay away."), 10);
				break;
			case 3:
				currentConversation.Interrupt(DamagedMode ? Translate("Again? ... just, go.") : Translate("Yes, there you go. This is ridiculous."), 10);
				break;
			case 4:
				NoLongerOnSpeakingTerms();
				dialogBox.Interrupt(DamagedMode ? "..." : Translate("I don't know what to say. Never come back, creature!"), 10);
				break;
			}
		}
		else
		{
			switch (base.State.leaves)
			{
			case 0:
				currentConversation.Interrupt(Translate("Oh... You are leaving."), 10);
				if (!DamagedMode)
				{
					currentConversation.ForceAddMessage(Translate("Good bye, I suppose..."), 10);
				}
				break;
			case 1:
				currentConversation.Interrupt(DamagedMode ? Translate("again... leaving...") : Translate("There you go again."), 10);
				break;
			case 2:
				currentConversation.Interrupt(DamagedMode ? "..." : Translate("Yet again leaving."), 10);
				break;
			default:
				if (!DamagedMode)
				{
					switch (UnityEngine.Random.Range(0, 6))
					{
					case 0:
						currentConversation.Interrupt(Translate("Yes, there you go."), 10);
						break;
					case 1:
						currentConversation.Interrupt(Translate("Again."), 10);
						break;
					case 2:
						currentConversation.Interrupt(Translate("*sigh*"), 10);
						break;
					case 3:
						currentConversation.Interrupt(Translate("..."), 10);
						break;
					case 4:
						currentConversation.Interrupt(Translate("This again."), 10);
						break;
					default:
						currentConversation.Interrupt(Translate("<CapPlayerName>... Never mind."), 10);
						break;
					}
				}
				break;
			}
		}
		if (!ModManager.MMF)
		{
			base.State.InfluenceLike(-0.05f);
		}
		base.State.leaves++;
		base.State.totalInterruptions++;
		base.State.increaseLikeOnSave = false;
	}

	private void InterruptPlayerAnnoyingMessage()
	{
		if (!ModManager.MMF && base.State.totalInterruptions >= 5)
		{
			NoLongerOnSpeakingTerms();
			if (base.State.totalInterruptions == 5)
			{
				dialogBox.Interrupt(DamagedMode ? Translate("I will...not speak to you...") : Translate("I will not speak to you any more."), 10);
			}
		}
		else if (base.State.annoyances == 0)
		{
			currentConversation.Interrupt(DamagedMode ? Translate("...please... be still...") : Translate("Please. Be still for a moment."), 10);
		}
		else if (base.State.annoyances == 1)
		{
			currentConversation.Interrupt(DamagedMode ? Translate("...stop...") : Translate("Please stop it!"), 10);
		}
		else if (base.State.neuronsLeft > 3 && !DamagedMode)
		{
			switch (UnityEngine.Random.Range(0, 6))
			{
			case 0:
				currentConversation.Interrupt(Translate("<CapPlayerName>! Stay still and listen."), 10);
				break;
			case 1:
				currentConversation.Interrupt(Translate(ModManager.MMF ? "Calm down!" : "I won't talk to you if you continue like this."), 10);
				break;
			case 2:
				currentConversation.Interrupt(Translate("Why should I tolerate this?"), 10);
				break;
			case 3:
				currentConversation.Interrupt(Translate("STOP!"), 10);
				break;
			case 4:
				currentConversation.Interrupt(Translate("This again."), 10);
				break;
			default:
				currentConversation.Interrupt(Translate("Leave me alone!"), 10);
				break;
			}
		}
		if (!ModManager.MMF)
		{
			base.State.InfluenceLike(-0.2f);
		}
		base.State.annoyances++;
		base.State.totalInterruptions++;
		base.State.increaseLikeOnSave = false;
	}

	private void PlayerAnnoyingWhenNotTalking()
	{
		if (!ModManager.MMF && base.State.annoyances >= 5)
		{
			NoLongerOnSpeakingTerms();
			if (base.State.annoyances == 5)
			{
				if (base.State.neuronsLeft > 3)
				{
					dialogBox.Interrupt(Translate("I will not speak to you any more."), 10);
				}
				else if (base.State.neuronsLeft > 1)
				{
					dialogBox.Interrupt(Translate("I will...not speak to you..."), 10);
				}
			}
		}
		else if (base.State.annoyances == 0)
		{
			dialogBox.Interrupt(DamagedMode ? Translate("...stop...") : Translate("<CapPlayerName>... Please settle down."), 10);
		}
		else if (base.State.annoyances == 1)
		{
			dialogBox.Interrupt(DamagedMode ? Translate("no...") : Translate("Please stop it!"), 10);
		}
		else if (base.State.neuronsLeft > 3 && !DamagedMode)
		{
			switch (UnityEngine.Random.Range(0, 6))
			{
			case 0:
				dialogBox.Interrupt(Translate("Why are you doing this?"), 10);
				break;
			case 1:
				dialogBox.Interrupt(Translate("Please!"), 10);
				break;
			case 2:
				dialogBox.Interrupt(Translate("Why should I tolerate this?"), 10);
				break;
			case 3:
				dialogBox.Interrupt(Translate("STOP!"), 10);
				break;
			case 4:
				dialogBox.Interrupt(Translate("This again."), 10);
				break;
			default:
				dialogBox.Interrupt(Translate("Leave me alone!"), 10);
				break;
			}
		}
		if (!ModManager.MMF)
		{
			base.State.InfluenceLike(-0.2f);
		}
		base.State.annoyances++;
		base.State.increaseLikeOnSave = false;
	}

	private void InterruptPlayerHoldNeuron()
	{
		if (ModManager.MSC && oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && oracle.room.game.IsMoonActive())
		{
			float value = UnityEngine.Random.value;
			if (value <= 0.25f)
			{
				dialogBox.Interrupt((!DamagedMode) ? Translate("No, please, release it!") : Translate("NO! ... no. Let it go, please."), 10);
			}
			else if (value <= 0.5f)
			{
				dialogBox.Interrupt((!DamagedMode) ? Translate("Wait, that's not food!") : Translate("not... edible, please."), 10);
			}
			else if (value <= 0.75f)
			{
				dialogBox.Interrupt((!DamagedMode) ? Translate("What are you doing? Stop!") : Translate("stop, don... don't!"), 10);
			}
			else
			{
				dialogBox.Interrupt((!DamagedMode) ? Translate("Please, don't touch those!") : Translate("LET GO! p please"), 10);
			}
			if (!DamagedMode)
			{
				value = UnityEngine.Random.value;
				if (value <= 0.33f)
				{
					dialogBox.NewMessage(Translate("Despite some power to my facility being restored, those are still crucial to my survival!"), 10);
				}
				else if (value <= 0.67f)
				{
					dialogBox.NewMessage(Translate("Those are the only memories I have left. I will cease functioning without them."), 10);
				}
				else
				{
					dialogBox.NewMessage(Translate("Your intention was to help me, was it not? Then please don't play with those!"), 10);
				}
			}
			base.State.InfluenceLike(-0.1f);
		}
		else if (base.State.totalInterruptions >= 5 || base.State.hasToldPlayerNotToEatNeurons)
		{
			NoLongerOnSpeakingTerms();
			dialogBox.Interrupt(DamagedMode ? Translate("NO! I will...not speak to you...") : Translate("Release that, and leave. I will not speak to you any more."), 10);
		}
		else
		{
			dialogBox.Interrupt(DamagedMode ? Translate("NO! ... no. Let it go, please.") : Translate("No, please, release it!"), 10);
			dialogBox.NewMessage(DamagedMode ? Translate("...please...") : Translate("If you eat it or leave with it, I will die. I beg you."), 10);
			base.State.InfluenceLike(-0.2f);
		}
		base.State.hasToldPlayerNotToEatNeurons = true;
		base.State.annoyances++;
		base.State.totalInterruptions++;
		base.State.increaseLikeOnSave = false;
	}

	private void PlayerReleaseNeuron()
	{
		if (ModManager.MSC && oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && oracle.room.game.IsMoonActive())
		{
			float value = UnityEngine.Random.value;
			if (value <= 0.33f)
			{
				dialogBox.Interrupt((!DamagedMode) ? Translate("Thank you. I must ask you... Don't do that again.") : Translate("...don't... do that."), 10);
			}
			else if (value <= 0.67f)
			{
				dialogBox.Interrupt((!DamagedMode) ? Translate("Please, don't scare me like that. I don't appreciate it.") : Translate("Leave me... alone."), 10);
			}
			else
			{
				dialogBox.Interrupt((!DamagedMode) ? Translate("Those aren't toys, <PlayerName>. I cannot trust anyone with them.") : Translate("...don't... trust you."), 10);
			}
		}
		else
		{
			if (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
			{
				dialogBox.Interrupt(DamagedMode ? Translate("...don't... do that.") : Translate("Never do that again. Or just kill me quickly. Whichever way."), 5);
			}
			else if (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
			{
				dialogBox.Interrupt(DamagedMode ? Translate("Thank... you.") : Translate("Thank you. I must ask you... Don't do that again."), 5);
			}
			else
			{
				dialogBox.Interrupt(DamagedMode ? Translate("Please... don't do... that.") : Translate("I must ask you... Don't do that again."), 5);
			}
			dialogBox.NewMessage(DamagedMode ? Translate("I... won't speak to you... if you do that.") : Translate("<CapPlayerName>, if you do that, I will not speak to you any more."), 10);
		}
	}

	private void ResumePausedConversation()
	{
		if (pauseReason == PauseReason.Annoyance)
		{
			if (base.State.annoyances < 3)
			{
				currentConversation.Interrupt(DamagedMode ? Translate("Thank... you.") : Translate("Thank you."), 5);
			}
		}
		else if (pauseReason == PauseReason.Leave)
		{
			if (base.State.leaves == 1)
			{
				currentConversation.Interrupt(DamagedMode ? Translate("You... are back.") : Translate("And you are back."), 10);
			}
			else if (base.State.leaves == 2)
			{
				currentConversation.Interrupt(DamagedMode ? Translate("And...back.") : Translate("Back again."), 10);
			}
			else if (!DamagedMode)
			{
				currentConversation.Interrupt(Translate("Here again."), 10);
			}
		}
		else if (pauseReason == PauseReason.GrabNeuron)
		{
			PlayerReleaseNeuron();
		}
		if (base.State.totalInterruptions == 1)
		{
			currentConversation.ForceAddMessage(DamagedMode ? Translate("I...said...") : Translate("As I was saying..."), 10);
		}
		else if (base.State.totalInterruptions == 2)
		{
			currentConversation.ForceAddMessage(DamagedMode ? Translate("Tried to say... to you...") : Translate("As I tried to say to you..."), 10);
		}
		else if (base.State.totalInterruptions == 3)
		{
			currentConversation.ForceAddMessage(DamagedMode ? Translate("Stay! ... Still...") : Translate("Little creature, why don't you stay calm and listen?"), 10);
			currentConversation.ForceAddMessage(DamagedMode ? Translate("And...listen!") : Translate("As I tried to say to you..."), 10);
		}
		else if (base.State.totalInterruptions == 4)
		{
			if (DamagedMode)
			{
				currentConversation.ForceAddMessage(Translate("And...now you expect me to... talk again?"), 10);
				return;
			}
			currentConversation.ForceAddMessage(Translate("And now you expect me to continue speaking?"), 10);
			if (base.State.neuronsLeft < 5)
			{
				currentConversation.ForceAddMessage(Translate("First you hurt me, then you come back to annoy me.<LINE>I wish I knew what was going on in that little head of yours."), 0);
			}
			currentConversation.ForceAddMessage(Translate("Let us try again - not that it has worked well before. I was saying..."), 10);
		}
		else if (base.State.totalInterruptions == 5)
		{
			if (DamagedMode)
			{
				currentConversation.ForceAddMessage(Translate("I am... too tired."), 10);
				currentConversation.ForceAddMessage(Translate("Stop doing... this, or I... will not speak... to you again."), 10);
				return;
			}
			currentConversation.ForceAddMessage(Translate("If you behave like this, why should I talk to you?"), 10);
			currentConversation.ForceAddMessage(Translate("You come here, but you can't be respectful enough to listen to me.<LINE>Will you listen this time?"), 0);
			currentConversation.ForceAddMessage(Translate("Look at me. The only thing I have to offer is my words.<LINE>If you come here, I must assume you want me to speak? So then would you PLEASE listen?<LINE>If not, you are welcome to leave me alone."), 0);
			currentConversation.ForceAddMessage(Translate("Now if you'll let me, I will try to say this again."), 0);
		}
	}

	public override void Pain()
	{
		if (oracle.Consious && (!oracle.room.game.IsMoonActive() || (oracle.room.game.IsStorySession && (!ModManager.MSC || oracle.room.game.GetStorySession.saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Rivulet))))
		{
			NoLongerOnSpeakingTerms();
		}
		base.Pain();
	}

	private void NoLongerOnSpeakingTerms()
	{
		Custom.Log("no longer on speaking terms");
		base.State.increaseLikeOnSave = false;
		respondToNeuronFromNoSpeakMode = true;
		base.State.likesPlayer = -1f;
		if (currentConversation != null)
		{
			currentConversation.Destroy();
		}
	}

	private void InterruptRain()
	{
		switch (base.State.neuronsLeft)
		{
		case 4:
			dialogBox.Interrupt("...", 5);
			if (ModManager.MSC && oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				dialogBox.Interrupt(Translate("Cold... You better go. I will be fine."), 10);
			}
			else
			{
				dialogBox.NewMessage(Translate("Rain... You better go. I will be fine."), 10);
			}
			return;
		case 3:
			dialogBox.Interrupt("...", 5);
			if (ModManager.MSC && oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				dialogBox.Interrupt(Translate("...storm... coming... Go!"), 10);
			}
			else
			{
				dialogBox.NewMessage(Translate("...rain... coming... Go!"), 10);
			}
			return;
		case 2:
			if (ModManager.MSC && oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				dialogBox.Interrupt(Translate("...storm..."), 5);
			}
			else
			{
				dialogBox.Interrupt(Translate("...rain..."), 5);
			}
			dialogBox.NewMessage(Translate("run"), 10);
			return;
		}
		if (ModManager.MSC && oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			if (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
			{
				dialogBox.Interrupt(Translate("The storm is coming. You should leave."), 5);
				return;
			}
			dialogBox.NewMessage(Translate("It is getting cold."), 20);
			dialogBox.NewMessage(Translate("You better go, <PlayerName>! I will be fine."), 0);
			return;
		}
		if (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
		{
			dialogBox.Interrupt(Translate("The rain is coming. If you stay, you will drown. Now, leave me alone."), 5);
			if (ModManager.MSC && CheckSlugpupsInRoom())
			{
				dialogBox.NewMessage(Translate("Take your offspring with you."), 10);
			}
			else if (ModManager.MMF && CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
			{
				dialogBox.NewMessage(Translate("Take your pet with you."), 10);
			}
			return;
		}
		dialogBox.Interrupt("...", 5);
		dialogBox.NewMessage(Translate("I think the rain is approaching."), 20);
		dialogBox.NewMessage(Translate("You better go, <PlayerName>! I will be fine.<LINE>It's not pleasant, but I have been through it before."), 0);
		if (ModManager.MSC && CheckSlugpupsInRoom())
		{
			dialogBox.NewMessage(Translate("Keep your family safe!"), 10);
		}
		else if (ModManager.MMF && CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
		{
			dialogBox.NewMessage(Translate("Keep your friend safe!"), 10);
		}
	}

	public MiscItemType TypeOfMiscItem(PhysicalObject testItem)
	{
		if (testItem is WaterNut || testItem is SwollenWaterNut)
		{
			return MiscItemType.WaterNut;
		}
		if (testItem is Rock)
		{
			return MiscItemType.Rock;
		}
		if (testItem is ExplosiveSpear)
		{
			return MiscItemType.FireSpear;
		}
		if (ModManager.MSC && testItem is ElectricSpear)
		{
			return MoreSlugcatsEnums.MiscItemType.ElectricSpear;
		}
		if (ModManager.MSC && testItem is Spear && (testItem as Spear).IsNeedle)
		{
			return MoreSlugcatsEnums.MiscItemType.SpearmasterSpear;
		}
		if (testItem is Spear)
		{
			return MiscItemType.Spear;
		}
		if (testItem is KarmaFlower)
		{
			return MiscItemType.KarmaFlower;
		}
		if (testItem is DangleFruit)
		{
			return MiscItemType.DangleFruit;
		}
		if (testItem is FlareBomb)
		{
			return MiscItemType.FlareBomb;
		}
		if (testItem is VultureMask)
		{
			if (ModManager.MSC)
			{
				if ((testItem as VultureMask).AbstrMsk.scavKing)
				{
					return MoreSlugcatsEnums.MiscItemType.KingMask;
				}
				if ((testItem as VultureMask).AbstrMsk.spriteOverride != "")
				{
					return MoreSlugcatsEnums.MiscItemType.EliteMask;
				}
			}
			return MiscItemType.VultureMask;
		}
		if (testItem is PuffBall)
		{
			return MiscItemType.PuffBall;
		}
		if (testItem is JellyFish)
		{
			return MiscItemType.JellyFish;
		}
		if (testItem is Lantern)
		{
			return MiscItemType.Lantern;
		}
		if (testItem is Mushroom)
		{
			return MiscItemType.Mushroom;
		}
		if (testItem is FirecrackerPlant)
		{
			return MiscItemType.FirecrackerPlant;
		}
		if (testItem is SlimeMold)
		{
			if (ModManager.MSC && testItem.abstractPhysicalObject.type == MoreSlugcatsEnums.AbstractObjectType.Seed)
			{
				return MoreSlugcatsEnums.MiscItemType.Seed;
			}
			return MiscItemType.SlimeMold;
		}
		if (testItem is ScavengerBomb)
		{
			return MiscItemType.ScavBomb;
		}
		if (testItem is OverseerCarcass && (!ModManager.MSC || !(testItem.abstractPhysicalObject as OverseerCarcass.AbstractOverseerCarcass).InspectorMode))
		{
			return MiscItemType.OverseerRemains;
		}
		if (testItem is BubbleGrass)
		{
			return MiscItemType.BubbleGrass;
		}
		if (ModManager.MSC)
		{
			if (testItem is SingularityBomb)
			{
				return MoreSlugcatsEnums.MiscItemType.SingularityGrenade;
			}
			if (testItem is FireEgg)
			{
				return MoreSlugcatsEnums.MiscItemType.FireEgg;
			}
			if (testItem is EnergyCell)
			{
				return MoreSlugcatsEnums.MiscItemType.EnergyCell;
			}
			if (testItem is OverseerCarcass && (testItem.abstractPhysicalObject as OverseerCarcass.AbstractOverseerCarcass).InspectorMode)
			{
				return MoreSlugcatsEnums.MiscItemType.InspectorEye;
			}
			if (testItem is GooieDuck)
			{
				return MoreSlugcatsEnums.MiscItemType.GooieDuck;
			}
			if (testItem is NeedleEgg)
			{
				return MoreSlugcatsEnums.MiscItemType.NeedleEgg;
			}
			if (testItem is LillyPuck)
			{
				return MoreSlugcatsEnums.MiscItemType.LillyPuck;
			}
			if (testItem is GlowWeed)
			{
				return MoreSlugcatsEnums.MiscItemType.GlowWeed;
			}
			if (testItem is DandelionPeach)
			{
				return MoreSlugcatsEnums.MiscItemType.DandelionPeach;
			}
			if (testItem is MoonCloak)
			{
				return MoreSlugcatsEnums.MiscItemType.MoonCloak;
			}
		}
		return MiscItemType.NA;
	}

	public bool WillingToInspectItem(PhysicalObject item)
	{
		if (ModManager.MMF && talkedAboutThisSession.Contains(item.abstractPhysicalObject.ID))
		{
			return false;
		}
		if (ModManager.MSC && item is FireEgg && (item as FireEgg).activeCounter > 0)
		{
			return false;
		}
		if (item is Player)
		{
			return false;
		}
		if (!oracle.Consious || !hasNoticedPlayer || stillWakingUp || sayHelloDelay > 0)
		{
			return false;
		}
		if (protest)
		{
			return false;
		}
		if (holdingObject != null)
		{
			return false;
		}
		if (item is DataPearl)
		{
			return true;
		}
		if (item is SLOracleSwarmer)
		{
			return false;
		}
		_ = TypeOfMiscItem(item) != MiscItemType.NA;
		return true;
	}

	public override void GrabObject(PhysicalObject item)
	{
		base.GrabObject(item);
		isRepeatedDiscussion = false;
		if (throwAwayObjects || item is SSOracleSwarmer)
		{
			return;
		}
		if (base.State.HaveIAlreadyDescribedThisItem(item.abstractPhysicalObject.ID))
		{
			if (!ModManager.MMF)
			{
				AlreadyDiscussedItem(item is DataPearl);
				return;
			}
			isRepeatedDiscussion = true;
		}
		if (item is DataPearl)
		{
			Custom.Log($"{(item as DataPearl).AbstractPearl.dataPearlType}");
			if (ModManager.MSC && oracle.room.game.IsStorySession && oracle.room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				if ((item as DataPearl).AbstractPearl.dataPearlType != MoreSlugcatsEnums.DataPearlType.RM && (item as DataPearl).AbstractPearl.dataPearlType != DataPearl.AbstractDataPearl.DataPearlType.LF_west)
				{
					Custom.Log("saint derail pearl");
					if (currentConversation != null)
					{
						currentConversation.Interrupt("...", 0);
						currentConversation.Destroy();
						currentConversation = null;
					}
					if (rainWorld.progression.miscProgressionData.GetFuturePearlDeciphered(DataPearl.AbstractDataPearl.DataPearlType.CC))
					{
						isRepeatedDiscussion = true;
						string text = AlreadyDiscussedItemString(pearl: false);
						if (currentConversation != null)
						{
							currentConversation.Interrupt(text, 10);
							return;
						}
						dialogBox.Interrupt(text, 10);
					}
					currentConversation = new MoonConversation(MoreSlugcatsEnums.ConversationID.Moon_PearlBleaching, this, MiscItemType.NA);
					if (!base.State.significantPearls.Contains(DataPearl.AbstractDataPearl.DataPearlType.CC))
					{
						base.State.significantPearls.Add(DataPearl.AbstractDataPearl.DataPearlType.CC);
					}
					rainWorld.progression.miscProgressionData.SetFuturePearlDeciphered(DataPearl.AbstractDataPearl.DataPearlType.CC, forced: true);
					return;
				}
				Custom.Log("RM/LF pearl passthrough on saint.");
			}
			if ((item as DataPearl).AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Misc || (item as DataPearl).AbstractPearl.dataPearlType.Index == -1)
			{
				currentConversation = new MoonConversation(Conversation.ID.Moon_Pearl_Misc, this, MiscItemType.NA);
			}
			else if ((item as DataPearl).AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Misc2)
			{
				currentConversation = new MoonConversation(Conversation.ID.Moon_Pearl_Misc2, this, MiscItemType.NA);
			}
			else if (ModManager.MSC && (item as DataPearl).AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.BroadcastMisc)
			{
				currentConversation = new MoonConversation(MoreSlugcatsEnums.ConversationID.Moon_Pearl_BroadcastMisc, this, MiscItemType.NA);
			}
			else if ((item as DataPearl).AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.PebblesPearl)
			{
				currentConversation = new MoonConversation(Conversation.ID.Moon_Pebbles_Pearl, this, MiscItemType.NA);
			}
			else
			{
				if (base.State.significantPearls.Contains((item as DataPearl).AbstractPearl.dataPearlType))
				{
					if (!ModManager.MMF)
					{
						AlreadyDiscussedItem(pearl: true);
						return;
					}
					isRepeatedDiscussion = true;
				}
				if (currentConversation != null)
				{
					currentConversation.Interrupt("...", 0);
					currentConversation.Destroy();
					currentConversation = null;
				}
				Conversation.ID id = Conversation.DataPearlToConversation((item as DataPearl).AbstractPearl.dataPearlType);
				currentConversation = new MoonConversation(id, this, MiscItemType.NA);
				if (!isRepeatedDiscussion)
				{
					base.State.significantPearls.Add((item as DataPearl).AbstractPearl.dataPearlType);
					if (ModManager.MSC)
					{
						if (oracle.room.world.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
						{
							rainWorld.progression.miscProgressionData.SetFuturePearlDeciphered((item as DataPearl).AbstractPearl.dataPearlType);
						}
						else
						{
							rainWorld.progression.miscProgressionData.SetPearlDeciphered((item as DataPearl).AbstractPearl.dataPearlType);
						}
					}
					base.State.totalPearlsBrought++;
					Custom.Log("pearls brought up:", base.State.totalPearlsBrought.ToString());
				}
			}
		}
		else
		{
			if (ModManager.MMF && base.State.HaveIAlreadyDescribedThisItem(item.abstractPhysicalObject.ID))
			{
				isRepeatedDiscussion = true;
			}
			MiscItemType miscItemType = TypeOfMiscItem(item);
			if (miscItemType != MiscItemType.NA)
			{
				if (!ModManager.MMF && base.State.miscItemsDescribed.Contains(miscItemType))
				{
					AlreadyDiscussedItem(pearl: false);
				}
				else
				{
					Conversation.ID moon_Misc_Item = Conversation.ID.Moon_Misc_Item;
					currentConversation = new MoonConversation(moon_Misc_Item, this, miscItemType);
					if (!base.State.miscItemsDescribed.Contains(miscItemType))
					{
						base.State.miscItemsDescribed.Add(miscItemType);
					}
				}
			}
		}
		if (!isRepeatedDiscussion)
		{
			base.State.totalItemsBrought++;
			base.State.AddItemToAlreadyTalkedAbout(item.abstractPhysicalObject.ID);
		}
		talkedAboutThisSession.Add(item.abstractPhysicalObject.ID);
	}

	private void AlreadyDiscussedItem(bool pearl)
	{
		string text = (pearl ? (UnityEngine.Random.Range(0, 3) switch
		{
			0 => Translate("Oh, I have already read this one to you, <PlayerName>."), 
			1 => Translate("This one I've already read to you, <PlayerName>."), 
			_ => Translate("This one again, <PlayerName>?"), 
		}) : (UnityEngine.Random.Range(0, 3) switch
		{
			0 => Translate("I think we have already talked about this one, <PlayerName>."), 
			1 => Translate("I've told you about this one, <PlayerName>."), 
			_ => Translate("<CapPlayerName>, this one again?"), 
		}));
		if (currentConversation != null)
		{
			currentConversation.Interrupt(text, 10);
		}
		else
		{
			dialogBox.Interrupt(text, 10);
		}
	}

	public new string ReplaceParts(string s)
	{
		s = Regex.Replace(s, "<PLAYERNAME>", NameForPlayer(capitalized: false));
		s = Regex.Replace(s, "<CAPPLAYERNAME>", NameForPlayer(capitalized: true));
		s = Regex.Replace(s, "<PlayerName>", NameForPlayer(capitalized: false));
		s = Regex.Replace(s, "<CapPlayerName>", NameForPlayer(capitalized: true));
		return s;
	}

	public override string Translate(string s)
	{
		return ReplaceParts(base.Translate(s));
	}

	public new void SpecialEvent(string eventName)
	{
		if (!ModManager.MSC)
		{
			return;
		}
		Custom.Log("special event!", eventName);
		if (eventName == "cloak" && holdingObject != null && holdingObject is MoonCloak)
		{
			AbstractPhysicalObject abstractPhysicalObject = holdingObject.abstractPhysicalObject;
			holdingObject.RemoveFromRoom();
			abstractPhysicalObject.Room.RemoveEntity(abstractPhysicalObject);
			holdingObject = null;
			oracle.room.game.GetStorySession.saveState.miscWorldSaveData.moonGivenRobe = true;
			oracle.room.game.rainWorld.progression.miscProgressionData.SetCloakTimelinePosition(oracle.room.game.GetStorySession.saveStateNumber);
			IDrawable graphicsModule = oracle.graphicsModule;
			oracle.DisposeGraphicsModule();
			oracle.InitiateGraphicsModule();
			for (int i = 0; i < oracle.room.game.cameras.Length; i++)
			{
				oracle.room.game.cameras[i].ReplaceDrawable(graphicsModule, oracle.graphicsModule);
			}
		}
		if (eventName == "WanderChamber")
		{
			setMovementBehavior((movementBehavior == MovementBehavior.Idle) ? MovementBehavior.Talk : MovementBehavior.Idle);
			SetNewDestination(RandomRoomPoint());
		}
		if (eventName == "WakeupPlayer")
		{
			holdPlayerAsleep = false;
		}
		switch (eventName)
		{
		case "RivScreen1":
		case "RivScreen2":
		case "RivScreen3":
			if (rivEnding.displayImage != null)
			{
				rivEnding.displayImage.Destroy();
				rivEnding.displayImage = null;
			}
			setMovementBehavior(MovementBehavior.ShowMedia);
			SetNewDestination(RandomRoomPoint());
			consistentShowMediaPosCounter = 400;
			if (eventName == "RivScreen1")
			{
				rivEnding.displayImage = oracle.myScreen.AddImage("AIimg2_DM");
				rivEnding.displayImageNumber = 1;
			}
			if (eventName == "RivScreen2")
			{
				rivEnding.displayImage = oracle.myScreen.AddImage("AIimg2_RIVEND");
				rivEnding.displayImageNumber = 2;
			}
			if (eventName == "RivScreen3")
			{
				rivEnding.displayImage = oracle.myScreen.AddImage("AIimg2_RIVEND2");
				rivEnding.displayImageNumber = 3;
			}
			break;
		}
		if (eventName == "SitDown")
		{
			setMovementBehavior(MovementBehavior.Idle);
			forceFlightMode = false;
			timeOutOfSitZone = 50;
		}
		if (eventName == "RivEndingFade")
		{
			RainWorldGame.BeatGameMode(oracle.room.game, standardVoidSea: false);
			RoomSettings.RoomEffect effect = oracle.room.roomSettings.GetEffect(MoreSlugcatsEnums.RoomEffectType.Advertisements);
			if (effect != null)
			{
				effect.amount = 0f;
			}
			oracle.room.AddObject(new FadeOut(oracle.room, Color.black, 200f, fadeIn: false));
		}
		if (eventName == "RivEndingCredits")
		{
			oracle.room.game.overWorld.InitiateSpecialWarp_SingleRoom(null, "MS_COMMS");
		}
	}

	public void CreatureJokeDialog()
	{
		CreatureTemplate.Type type = CheckStrayCreatureInRoom();
		if (type == CreatureTemplate.Type.Vulture || type == CreatureTemplate.Type.KingVulture || type == CreatureTemplate.Type.BigEel || type == CreatureTemplate.Type.MirosBird || type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture)
		{
			dialogBox.NewMessage(Translate("Your friend is very large, how did you fit them in here?"), 10);
		}
		else if (type == CreatureTemplate.Type.Deer)
		{
			dialogBox.NewMessage(Translate("How did you bring that in here... I think it is as surprised as I am!"), 10);
		}
		else if (type == CreatureTemplate.Type.DaddyLongLegs || type == CreatureTemplate.Type.BrotherLongLegs || type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)
		{
			dialogBox.NewMessage(Translate("Oh no."), 10);
		}
		else if (type == CreatureTemplate.Type.RedCentipede)
		{
			dialogBox.NewMessage(Translate("Oh, that is not a friend..."), 10);
		}
		else if (type == CreatureTemplate.Type.TempleGuard)
		{
			dialogBox.NewMessage(Translate("What did you do!?"), 10);
		}
	}

	private bool RejectDiscussItem()
	{
		string text = string.Empty;
		bool result = false;
		if (moveToAndPickUpItem.abstractPhysicalObject.type != AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer && (base.State.GetOpinion == SLOrcacleState.PlayerOpinion.NotSpeaking || base.State.neuronsLeft < 4))
		{
			result = true;
			if (base.State.neuronsLeft <= 1)
			{
				int num = UnityEngine.Random.Range(0, 4);
				if (num == 0)
				{
					text = Translate("Go... away... bad... thing...");
				}
				if (num == 1)
				{
					text = Translate("...");
				}
				if (num == 2)
				{
					text = Translate("Too... broken...");
				}
				if (num == 3)
				{
					text = Translate("No! You... killed... me!");
				}
				if (UnityEngine.Random.value < 0.5f)
				{
					text = Translate("...");
				}
			}
			else if (base.State.neuronsLeft < 4)
			{
				int num2 = UnityEngine.Random.Range(0, 5);
				if (num2 == 0)
				{
					text = Translate("No! <CapPlayerName> ate... me!");
				}
				if (num2 == 1)
				{
					text = Translate("Go... away! You ate... me!");
				}
				if (num2 == 2)
				{
					text = Translate("No! Am... broken... because you...");
				}
				if (num2 == 3)
				{
					text = Translate("...");
				}
				if (num2 == 4)
				{
					text = Translate("No!");
				}
				if (num2 == 5)
				{
					text = Translate("Leave... me alone. Terrible... creature...");
				}
				if (UnityEngine.Random.value < 0.15f)
				{
					text = Translate("...");
				}
			}
			else
			{
				int num3 = UnityEngine.Random.Range(0, 6);
				if (num3 == 0)
				{
					text = Translate("I have nothing to say to you, <PlayerName>.");
				}
				if (num3 == 1)
				{
					text = Translate("Go away, <PlayerName>.");
				}
				if (num3 == 2)
				{
					text = Translate("...");
				}
				if (num3 == 3)
				{
					text = Translate("Leave me alone...");
				}
				if (num3 == 4)
				{
					text = Translate("Stop! Stop giving me your junk!");
				}
				if (num3 == 5)
				{
					text = Translate("Leave me alone! You terrible creature!");
				}
				if (UnityEngine.Random.value < 0.25f)
				{
					text = Translate("...");
				}
			}
			if (currentConversation != null)
			{
				currentConversation.Interrupt(text, 10);
			}
			else
			{
				dialogBox.Interrupt(text, 10);
			}
		}
		throwAwayObjects = result;
		return result;
	}

	public void PainDenial()
	{
		float value = UnityEngine.Random.value;
		if (value <= 0.33f)
		{
			dialogBox.Interrupt((!DamagedMode) ? Translate("What are you doing?! Just because you helped me doesn't mean you can treat me this way.") : Translate("...WHY?"), 10);
		}
		else if (value <= 0.67f)
		{
			dialogBox.Interrupt((!DamagedMode) ? Translate("Why would you do that to me?! I don't understand you, <PlayerName>.") : Translate("it... hurts..."), 10);
		}
		else
		{
			dialogBox.Interrupt((!DamagedMode) ? Translate("Stop, that's dangerous! You are going to break something again!") : Translate("No! Don't... hurt me!"), 10);
		}
		base.State.InfluenceLike(-0.2f);
	}

	public void DeathDenial()
	{
		playerHoldingNeuronNoConvo = false;
		if (currentConversation != null && currentConversation.paused)
		{
			currentConversation.Destroy();
		}
		if (base.State.neuronsLeft != 0 && base.State.neuronsLeft != 1)
		{
			if (base.State.neuronsLeft == 2)
			{
				dialogBox.Interrupt("...", 40);
			}
			else if (base.State.neuronsLeft == 3)
			{
				dialogBox.Interrupt(Translate("...help... me..."), 40);
			}
			else if (base.State.neuronsLeft == 4)
			{
				dialogBox.Interrupt(Translate("... stop... please..."), 40);
				dialogBox.NewMessage(Translate("...why did you... save... me..."), 40);
			}
			else if (UnityEngine.Random.Range(0, 3) == 0)
			{
				dialogBox.Interrupt(Translate("Please stop! I beg you. I don't... understand your motivation..."), 10);
			}
			else if (UnityEngine.Random.Range(0, 3) == 1)
			{
				dialogBox.Interrupt(Translate("Why?! Why would you?? Please, I have so much more I need to do, still..."), 10);
			}
			else
			{
				dialogBox.Interrupt(Translate("What are you doing? Please!! I only just got my freedom back, how cruel can you possibly be?"), 10);
			}
		}
		base.State.InfluenceLike(-0.5f);
	}

	public void TalkToDeadPlayer()
	{
		if (!deadTalk && oracle.room.ViewedByAnyCamera(oracle.firstChunk.pos, 0f))
		{
			_ = UnityEngine.Random.value;
			if (base.State.neuronsLeft > 3)
			{
				dialogBox.Interrupt(Translate("..."), 60);
				dialogBox.NewMessage(Translate("<CapPlayerName>, are you okay?"), 60);
				dialogBox.NewMessage(Translate("..."), 120);
				dialogBox.NewMessage(Translate("Oh..."), 60);
			}
			else
			{
				dialogBox.Interrupt(Translate("..."), 60);
			}
			deadTalk = true;
		}
	}
}
